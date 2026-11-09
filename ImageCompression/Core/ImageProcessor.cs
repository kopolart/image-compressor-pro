using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace ImageCompression.Core
{
    /// <summary>
    /// Pixel level operations: resizing and transparency flattening.
    /// </summary>
    public static class ImageProcessor
    {
        public static Bitmap Resize(Image image, int width, int height, InterpolationMode interpolationMode)
        {
            Bitmap resized = new Bitmap(width, height);

            using (Graphics graphics = Graphics.FromImage(resized))
            {
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.CompositingQuality = CompositingQuality.HighQuality;
                graphics.InterpolationMode = interpolationMode;
                graphics.SmoothingMode = SmoothingMode.HighQuality;
                graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                using (ImageAttributes wrapMode = new ImageAttributes())
                {
                    wrapMode.SetWrapMode(WrapMode.TileFlipXY);
                    graphics.DrawImage(
                        image,
                        new Rectangle(0, 0, width, height),
                        0,
                        0,
                        image.Width,
                        image.Height,
                        GraphicsUnit.Pixel,
                        wrapMode);
                }
            }

            return resized;
        }

        /// <summary>
        /// Draws the image onto a white background. Used before encoding to
        /// formats without transparency support (JPEG, BMP), where transparent
        /// pixels would otherwise turn black.
        /// </summary>
        public static Bitmap FlattenToWhite(Image image)
        {
            Bitmap flattened = new Bitmap(image.Width, image.Height);

            using (Graphics graphics = Graphics.FromImage(flattened))
            {
                graphics.Clear(Color.White);
                graphics.DrawImage(image, new Rectangle(0, 0, image.Width, image.Height));
            }

            return flattened;
        }
    }
}
