using System.Drawing.Imaging;
using System.Linq;

namespace ImageCompression.Core
{
    public enum OutputFormat
    {
        Jpeg,
        Png,
        WebP,
        Bmp,
        Gif,
        Tiff
    }

    /// <summary>
    /// Describes one output format: its UI name, save dialog filter and encoder
    /// capabilities. This table is the single source of truth for all format
    /// specific behavior - adding a new format means adding one entry here.
    /// </summary>
    public sealed class OutputFormatInfo
    {
        public OutputFormat Format { get; }
        public string DisplayName { get; }
        public string FileDialogFilter { get; }
        public ImageFormat GdiFormat { get; }
        public bool SupportsQuality { get; }
        public bool SupportsTransparency { get; }

        private OutputFormatInfo(
            OutputFormat format,
            string displayName,
            string fileDialogFilter,
            ImageFormat gdiFormat,
            bool supportsQuality,
            bool supportsTransparency)
        {
            Format = format;
            DisplayName = displayName;
            FileDialogFilter = fileDialogFilter;
            GdiFormat = gdiFormat;
            SupportsQuality = supportsQuality;
            SupportsTransparency = supportsTransparency;
        }

        public static readonly OutputFormatInfo[] All =
        {
            new OutputFormatInfo(OutputFormat.Jpeg, "JPEG", "JPEG Image|*.jpg;*.jpeg", ImageFormat.Jpeg, true, false),
            new OutputFormatInfo(OutputFormat.Png, "PNG", "PNG Image|*.png", ImageFormat.Png, false, true),
            new OutputFormatInfo(OutputFormat.WebP, "WebP", "WebP Image|*.webp", null, true, true),
            new OutputFormatInfo(OutputFormat.Bmp, "BMP", "Bitmap Image|*.bmp", ImageFormat.Bmp, false, false),
            new OutputFormatInfo(OutputFormat.Gif, "GIF", "GIF Image|*.gif", ImageFormat.Gif, false, true),
            new OutputFormatInfo(OutputFormat.Tiff, "TIFF", "TIFF Image|*.tiff", ImageFormat.Tiff, false, true)
        };

        public static OutputFormatInfo FromDisplayName(string displayName)
        {
            return All.FirstOrDefault(f => f.DisplayName == displayName) ?? All[0];
        }
    }
}
