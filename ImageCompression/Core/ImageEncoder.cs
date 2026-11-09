using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp.Formats.Webp;
using SharpImage = SixLabors.ImageSharp.Image;

namespace ImageCompression.Core
{
    /// <summary>
    /// Encodes images to the supported output formats. WebP is delegated to
    /// ImageSharp (pinned to the free, Apache licensed 2.1.x line), everything
    /// else uses the built-in GDI+ encoders. All WebP specific code lives here
    /// and in ImageLoader, so swapping the WebP library later only touches
    /// these two files.
    /// </summary>
    public static class ImageEncoder
    {
        public static void SaveToFile(Image image, string filePath, OutputFormatInfo format, int quality)
        {
            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                EncodeToStream(image, fileStream, format, quality);
            }
        }

        public static long GetEncodedSize(Image image, OutputFormatInfo format, int quality)
        {
            using (MemoryStream memoryStream = new MemoryStream())
            {
                EncodeToStream(image, memoryStream, format, quality);
                return memoryStream.Length;
            }
        }

        public static void EncodeToStream(Image image, Stream output, OutputFormatInfo format, int quality)
        {
            if (format.Format == OutputFormat.WebP)
            {
                EncodeWebP(image, output, quality);
                return;
            }

            Bitmap flattened = format.SupportsTransparency ? null : ImageProcessor.FlattenToWhite(image);

            try
            {
                Image source = flattened ?? image;

                if (format.Format == OutputFormat.Jpeg)
                {
                    ImageCodecInfo codec = GetGdiEncoder("image/jpeg");
                    if (codec != null)
                    {
                        using (EncoderParameters parameters = new EncoderParameters(1))
                        {
                            parameters.Param[0] = new EncoderParameter(
                                Encoder.Quality,
                                (long)ClampQuality(quality));

                            source.Save(output, codec, parameters);
                        }

                        return;
                    }
                }

                source.Save(output, format.GdiFormat);
            }
            finally
            {
                if (flattened != null)
                    flattened.Dispose();
            }
        }

        /// <summary>
        /// Encodes as WebP using ImageSharp, because GDI+ has no WebP encoder.
        /// A lossless PNG round-trip hands the pixels (including transparency)
        /// from GDI+ to ImageSharp. Quality 100 produces a lossless WebP file.
        /// </summary>
        private static void EncodeWebP(Image image, Stream output, int quality)
        {
            using (MemoryStream intermediate = new MemoryStream())
            {
                image.Save(intermediate, ImageFormat.Png);
                intermediate.Position = 0;

                using (SharpImage webpImage = SharpImage.Load(intermediate))
                {
                    WebpEncoder encoder = new WebpEncoder
                    {
                        Quality = ClampQuality(quality),
                        FileFormat = quality >= 100
                            ? WebpFileFormatType.Lossless
                            : WebpFileFormatType.Lossy
                    };

                    webpImage.Save(output, encoder);
                }
            }
        }

        private static int ClampQuality(int quality)
        {
            return Math.Max(1, Math.Min(100, quality));
        }

        private static ImageCodecInfo GetGdiEncoder(string mimeType)
        {
            return ImageCodecInfo
                .GetImageEncoders()
                .FirstOrDefault(e => string.Equals(e.MimeType, mimeType, StringComparison.OrdinalIgnoreCase));
        }
    }
}
