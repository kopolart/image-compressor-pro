using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using SixLabors.ImageSharp.Formats.Png;
using SharpImage = SixLabors.ImageSharp.Image;

namespace ImageCompression.Core
{
    /// <summary>
    /// Loads images from disk without locking the file, applies EXIF
    /// orientation (so phone photos appear upright) and falls back to
    /// ImageSharp for formats GDI+ cannot decode (e.g. WebP).
    /// </summary>
    public static class ImageLoader
    {
        private const int ExifOrientationTag = 0x0112;

        public static Image LoadWithoutLockingFile(string filePath)
        {
            byte[] imageBytes = File.ReadAllBytes(filePath);
            return TryLoadWithGdi(imageBytes) ?? LoadWithImageSharp(imageBytes);
        }

        private static Bitmap TryLoadWithGdi(byte[] imageBytes)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream(imageBytes))
                using (Image decoded = Image.FromStream(memoryStream))
                {
                    Bitmap copy = new Bitmap(decoded);
                    ApplyExifOrientation(decoded, copy);
                    return copy;
                }
            }
            catch (ArgumentException)
            {
                // GDI+ does not recognize the format - the caller falls back to ImageSharp.
                return null;
            }
        }

        private static Bitmap LoadWithImageSharp(byte[] imageBytes)
        {
            using (SharpImage decoded = SharpImage.Load(imageBytes))
            using (MemoryStream pngStream = new MemoryStream())
            {
                // Lossless PNG round-trip hands the pixels (including transparency)
                // from ImageSharp back to GDI+.
                decoded.Save(pngStream, new PngEncoder());
                pngStream.Position = 0;

                using (Image gdiImage = Image.FromStream(pngStream))
                {
                    return new Bitmap(gdiImage);
                }
            }
        }

        private static void ApplyExifOrientation(Image source, Bitmap target)
        {
            try
            {
                if (Array.IndexOf(source.PropertyIdList, ExifOrientationTag) < 0)
                    return;

                PropertyItem orientationProperty = source.GetPropertyItem(ExifOrientationTag);
                if (orientationProperty == null || orientationProperty.Value == null || orientationProperty.Value.Length == 0)
                    return;

                switch (orientationProperty.Value[0])
                {
                    case 2: target.RotateFlip(RotateFlipType.RotateNoneFlipX); break;
                    case 3: target.RotateFlip(RotateFlipType.Rotate180FlipNone); break;
                    case 4: target.RotateFlip(RotateFlipType.RotateNoneFlipY); break;
                    case 5: target.RotateFlip(RotateFlipType.Rotate90FlipX); break;
                    case 6: target.RotateFlip(RotateFlipType.Rotate90FlipNone); break;
                    case 7: target.RotateFlip(RotateFlipType.Rotate270FlipX); break;
                    case 8: target.RotateFlip(RotateFlipType.Rotate270FlipNone); break;
                }
            }
            catch (Exception)
            {
                // Orientation metadata is optional - never fail a load because of it.
            }
        }
    }
}
