using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using ImageCompression.Core;

namespace ImageCompression
{
    /// <summary>
    /// Main window. This class only builds the UI and wires events - all image
    /// work is done by the services in the Core namespace.
    /// </summary>
    public partial class MainForm : Form
    {
        private Image originalImage;
        private Image processedImage;
        private string originalFilePath;
        private bool suppressSettingsEvents;

        private SizeEstimator sizeEstimator;
        private Timer autoPreviewTimer;

        private Label lblFilePath;
        private PictureBox pbOriginal;
        private PictureBox pbProcessed;
        private NumericUpDown nudWidth;
        private NumericUpDown nudHeight;
        private CheckBox chkAspectRatio;
        private TrackBar tbQuality;
        private Label lblQualityValue;
        private ComboBox cmbFormat;
        private ComboBox cmbInterpolation;
        private CheckBox chkAutoProcess;
        private Button btnProcess;
        private Button btnDownload;
        private Button btnReset;
        private Label lblOriginalInfo;
        private Label lblProcessedInfo;
        private Label lblEstimatedSize;

        public MainForm()
        {
            InitializeComponent();
            InitializeEstimator();
            WireSettingEvents();
            UpdateQualityControlsForFormat();
        }

        #region UI construction

        private void InitializeComponent()
        {
            this.Text = "Image Compressor & Resizer Pro";
            this.ClientSize = new Size(1080, 745);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.BackColor = Color.FromArgb(240, 240, 240);
            this.FormBorderStyle = FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;

            Button btnBrowse = new Button
            {
                Text = "Browse Image",
                Location = new Point(20, 20),
                Size = new Size(120, 35),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            btnBrowse.Click += BtnBrowse_Click;
            this.Controls.Add(btnBrowse);

            lblFilePath = new Label
            {
                Text = "No file selected",
                Location = new Point(150, 28),
                Size = new Size(500, 20),
                AutoSize = false,
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblFilePath);

            Panel panelOriginal = new Panel
            {
                Location = new Point(20, 70),
                Size = new Size(420, 320),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(panelOriginal);

            pbOriginal = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(400, 300),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            panelOriginal.Controls.Add(pbOriginal);

            Label lblOriginal = new Label
            {
                Text = "Original Image",
                Location = new Point(20, 395),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblOriginal);

            Panel panelProcessed = new Panel
            {
                Location = new Point(640, 70),
                Size = new Size(420, 320),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            this.Controls.Add(panelProcessed);

            pbProcessed = new PictureBox
            {
                Location = new Point(10, 10),
                Size = new Size(400, 300),
                SizeMode = PictureBoxSizeMode.Zoom
            };
            panelProcessed.Controls.Add(pbProcessed);

            Label lblProcessed = new Label
            {
                Text = "Processed Image",
                Location = new Point(640, 395),
                AutoSize = true,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            this.Controls.Add(lblProcessed);

            GroupBox gbSettings = new GroupBox
            {
                Text = "Image Settings",
                Location = new Point(20, 425),
                Size = new Size(420, 260),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            this.Controls.Add(gbSettings);

            Label lblWidth = new Label
            {
                Text = "Width (px):",
                Location = new Point(15, 30),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(lblWidth);

            nudWidth = new NumericUpDown
            {
                Location = new Point(100, 28),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10000,
                Value = 800,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(nudWidth);

            Label lblHeight = new Label
            {
                Text = "Height (px):",
                Location = new Point(15, 65),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(lblHeight);

            nudHeight = new NumericUpDown
            {
                Location = new Point(100, 63),
                Size = new Size(100, 25),
                Minimum = 1,
                Maximum = 10000,
                Value = 600,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(nudHeight);

            chkAspectRatio = new CheckBox
            {
                Text = "Lock Aspect Ratio",
                Location = new Point(220, 30),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(chkAspectRatio);

            Label lblQuality = new Label
            {
                Text = "Quality (%):",
                Location = new Point(15, 105),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(lblQuality);

            tbQuality = new TrackBar
            {
                Location = new Point(100, 100),
                Size = new Size(220, 45),
                Minimum = 1,
                Maximum = 100,
                Value = 85,
                TickFrequency = 10
            };
            gbSettings.Controls.Add(tbQuality);

            lblQualityValue = new Label
            {
                Text = "85%",
                Location = new Point(330, 105),
                AutoSize = true,
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            gbSettings.Controls.Add(lblQualityValue);

            Label lblFormat = new Label
            {
                Text = "Output Format:",
                Location = new Point(15, 150),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(lblFormat);

            cmbFormat = new ComboBox
            {
                Location = new Point(100, 148),
                Size = new Size(100, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            foreach (OutputFormatInfo format in OutputFormatInfo.All)
            {
                cmbFormat.Items.Add(format.DisplayName);
            }
            cmbFormat.SelectedIndex = 0;
            gbSettings.Controls.Add(cmbFormat);

            Label lblInterpolation = new Label
            {
                Text = "Quality Mode:",
                Location = new Point(15, 185),
                AutoSize = true,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(lblInterpolation);

            cmbInterpolation = new ComboBox
            {
                Location = new Point(100, 183),
                Size = new Size(150, 25),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9)
            };
            cmbInterpolation.Items.AddRange(new object[]
            {
                "High Quality",
                "Medium Quality",
                "Low Quality (Fast)"
            });
            cmbInterpolation.SelectedIndex = 0;
            gbSettings.Controls.Add(cmbInterpolation);

            chkAutoProcess = new CheckBox
            {
                Text = "Auto-preview on settings change",
                Location = new Point(15, 220),
                AutoSize = true,
                Checked = false,
                Font = new Font("Segoe UI", 9)
            };
            gbSettings.Controls.Add(chkAutoProcess);

            btnProcess = new Button
            {
                Text = "Process Image",
                Location = new Point(460, 455),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(0, 120, 215),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnProcess.Click += BtnProcess_Click;
            this.Controls.Add(btnProcess);

            btnDownload = new Button
            {
                Text = "Save Image",
                Location = new Point(460, 505),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(0, 150, 136),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Enabled = false,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnDownload.Click += BtnDownload_Click;
            this.Controls.Add(btnDownload);

            btnReset = new Button
            {
                Text = "Reset",
                Location = new Point(460, 555),
                Size = new Size(140, 40),
                BackColor = Color.FromArgb(220, 53, 69),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 10, FontStyle.Bold)
            };
            btnReset.Click += BtnReset_Click;
            this.Controls.Add(btnReset);

            lblEstimatedSize = new Label
            {
                Text = "Estimated output size will appear here",
                Location = new Point(620, 455),
                Size = new Size(440, 70),
                AutoSize = false,
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White,
                TextAlign = ContentAlignment.MiddleCenter
            };
            this.Controls.Add(lblEstimatedSize);

            lblOriginalInfo = new Label
            {
                Text = "",
                Location = new Point(20, 692),
                Size = new Size(420, 45),
                AutoSize = false,
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblOriginalInfo);

            lblProcessedInfo = new Label
            {
                Text = "",
                Location = new Point(640, 692),
                Size = new Size(420, 45),
                AutoSize = false,
                Font = new Font("Segoe UI", 9)
            };
            this.Controls.Add(lblProcessedInfo);
        }

        private void InitializeEstimator()
        {
            sizeEstimator = new SizeEstimator(
                this,
                CreateResizedSnapshot,
                GetSelectedFormat,
                () => tbQuality.Value,
                OnEstimateReady);

            autoPreviewTimer = new Timer { Interval = 400 };
            autoPreviewTimer.Tick += AutoPreviewTimer_Tick;
        }

        /// <summary>
        /// Event wiring happens after every control exists and has its initial
        /// value, so construction can never trigger a half-initialized handler.
        /// </summary>
        private void WireSettingEvents()
        {
            nudWidth.ValueChanged += NudWidth_ValueChanged;
            nudHeight.ValueChanged += NudHeight_ValueChanged;
            tbQuality.ValueChanged += TbQuality_ValueChanged;
            cmbFormat.SelectedIndexChanged += CmbFormat_SelectedIndexChanged;
            cmbInterpolation.SelectedIndexChanged += CmbInterpolation_SelectedIndexChanged;
        }

        #endregion

        #region Setting change handlers

        private void NudWidth_ValueChanged(object sender, EventArgs e)
        {
            if (suppressSettingsEvents)
                return;

            if (chkAspectRatio.Checked && originalImage != null && originalImage.Height > 0)
            {
                suppressSettingsEvents = true;
                try
                {
                    decimal aspectRatio = (decimal)originalImage.Width / originalImage.Height;
                    decimal calculatedHeight = Math.Round(nudWidth.Value / aspectRatio);
                    nudHeight.Value = ClampToRange(nudHeight, (int)Math.Max(1, calculatedHeight));
                }
                finally
                {
                    suppressSettingsEvents = false;
                }
            }

            OnOutputSettingsChanged();
        }

        private void NudHeight_ValueChanged(object sender, EventArgs e)
        {
            if (suppressSettingsEvents)
                return;

            if (chkAspectRatio.Checked && originalImage != null && originalImage.Height > 0)
            {
                suppressSettingsEvents = true;
                try
                {
                    decimal aspectRatio = (decimal)originalImage.Width / originalImage.Height;
                    decimal calculatedWidth = Math.Round(nudHeight.Value * aspectRatio);
                    nudWidth.Value = ClampToRange(nudWidth, (int)Math.Max(1, calculatedWidth));
                }
                finally
                {
                    suppressSettingsEvents = false;
                }
            }

            OnOutputSettingsChanged();
        }

        private void TbQuality_ValueChanged(object sender, EventArgs e)
        {
            lblQualityValue.Text = tbQuality.Value + "%";

            if (suppressSettingsEvents)
                return;

            OnOutputSettingsChanged();
        }

        private void CmbFormat_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateQualityControlsForFormat();

            if (suppressSettingsEvents)
                return;

            OnOutputSettingsChanged();
        }

        private void CmbInterpolation_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (suppressSettingsEvents)
                return;

            OnOutputSettingsChanged();
        }

        /// <summary>
        /// Single funnel for "an output setting changed": schedules one debounced
        /// size estimate and, when enabled, one debounced auto-preview - no matter
        /// how many settings changed in quick succession.
        /// </summary>
        private void OnOutputSettingsChanged()
        {
            sizeEstimator.Request();

            if (chkAutoProcess.Checked && originalImage != null)
            {
                autoPreviewTimer.Stop();
                autoPreviewTimer.Start();
            }
        }

        private void AutoPreviewTimer_Tick(object sender, EventArgs e)
        {
            autoPreviewTimer.Stop();

            if (chkAutoProcess.Checked && originalImage != null)
                ProcessImage(false);
        }

        private void UpdateQualityControlsForFormat()
        {
            OutputFormatInfo format = GetSelectedFormat();
            tbQuality.Enabled = format.SupportsQuality;
            lblQualityValue.Enabled = format.SupportsQuality;
        }

        #endregion

        #region Button handlers

        private void BtnBrowse_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog ofd = new OpenFileDialog())
            {
                ofd.Filter = "Image Files|*.jpg;*.jpeg;*.png;*.webp;*.bmp;*.gif;*.tiff";
                ofd.Title = "Select an Image";

                if (ofd.ShowDialog(this) != DialogResult.OK)
                    return;

                // Load first; the current state is only replaced after success,
                // so a failed load leaves the app fully working.
                Image loadedImage;
                try
                {
                    loadedImage = ImageLoader.LoadWithoutLockingFile(ofd.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "Error loading image: " + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                    return;
                }

                // Detach images from the picture boxes before disposing them,
                // so a repaint can never touch a disposed image.
                pbOriginal.Image = null;
                pbProcessed.Image = null;
                DisposeImage(ref originalImage);
                DisposeImage(ref processedImage);

                originalImage = loadedImage;
                originalFilePath = ofd.FileName;

                pbOriginal.Image = originalImage;
                lblFilePath.Text = Path.GetFileName(originalFilePath);
                lblProcessedInfo.Text = string.Empty;

                suppressSettingsEvents = true;
                try
                {
                    nudWidth.Value = ClampToRange(nudWidth, originalImage.Width);
                    nudHeight.Value = ClampToRange(nudHeight, originalImage.Height);
                }
                finally
                {
                    suppressSettingsEvents = false;
                }

                btnDownload.Enabled = true;
                UpdateOriginalInfo();
                OnOutputSettingsChanged();
            }
        }

        private void BtnProcess_Click(object sender, EventArgs e)
        {
            ProcessImage(true);
        }

        private void BtnDownload_Click(object sender, EventArgs e)
        {
            if (originalImage == null)
            {
                MessageBox.Show(
                    this,
                    "Please select an image first.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            OutputFormatInfo format = GetSelectedFormat();

            using (SaveFileDialog sfd = new SaveFileDialog())
            {
                sfd.Filter = format.FileDialogFilter;
                sfd.FileName = "processed_" + Path.GetFileNameWithoutExtension(originalFilePath);
                sfd.AddExtension = true;
                sfd.OverwritePrompt = true;

                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                try
                {
                    // The file is always produced from the CURRENT settings, so
                    // the saved image can never be stale.
                    using (Bitmap resized = ImageProcessor.Resize(
                        originalImage,
                        (int)nudWidth.Value,
                        (int)nudHeight.Value,
                        GetInterpolationMode(cmbInterpolation.SelectedIndex)))
                    {
                        ImageEncoder.SaveToFile(resized, sfd.FileName, format, tbQuality.Value);
                    }

                    long savedSize = new FileInfo(sfd.FileName).Length;

                    MessageBox.Show(
                        this,
                        $"Image saved successfully!\nLocation: {sfd.FileName}\nFile size: {FormatFileSize(savedSize)}",
                        "Success",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        this,
                        "Error saving image: " + ex.Message,
                        "Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
        }

        private void BtnReset_Click(object sender, EventArgs e)
        {
            suppressSettingsEvents = true;
            try
            {
                if (originalImage != null)
                {
                    nudWidth.Value = ClampToRange(nudWidth, originalImage.Width);
                    nudHeight.Value = ClampToRange(nudHeight, originalImage.Height);
                }

                tbQuality.Value = 85;
                cmbFormat.SelectedIndex = 0;
                cmbInterpolation.SelectedIndex = 0;
            }
            finally
            {
                suppressSettingsEvents = false;
            }

            UpdateQualityControlsForFormat();
            OnOutputSettingsChanged();
        }

        #endregion

        #region Image work

        private void ProcessImage(bool showWarningWhenNoImage)
        {
            if (originalImage == null)
            {
                if (showWarningWhenNoImage)
                {
                    MessageBox.Show(
                        this,
                        "Please select an image first.",
                        "Warning",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }

                return;
            }

            try
            {
                Bitmap resized = ImageProcessor.Resize(
                    originalImage,
                    (int)nudWidth.Value,
                    (int)nudHeight.Value,
                    GetInterpolationMode(cmbInterpolation.SelectedIndex));

                pbProcessed.Image = null;
                DisposeImage(ref processedImage);
                processedImage = resized;
                pbProcessed.Image = processedImage;

                lblProcessedInfo.Text = $"Dimensions: {processedImage.Width} x {processedImage.Height} px";
                lblProcessedInfo.ForeColor = SystemColors.ControlText;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    "Error processing image: " + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private Bitmap CreateResizedSnapshot()
        {
            if (originalImage == null)
                return null;

            return ImageProcessor.Resize(
                originalImage,
                (int)nudWidth.Value,
                (int)nudHeight.Value,
                GetInterpolationMode(cmbInterpolation.SelectedIndex));
        }

        private void OnEstimateReady(long estimatedSize)
        {
            if (estimatedSize < 0)
            {
                lblEstimatedSize.Text = "Could not estimate output size";
                lblEstimatedSize.ForeColor = SystemColors.ControlText;
                return;
            }

            if (string.IsNullOrWhiteSpace(originalFilePath) || !File.Exists(originalFilePath))
                return;

            long originalSize = new FileInfo(originalFilePath).Length;

            double percentChange = originalSize > 0
                ? ((double)(estimatedSize - originalSize) / originalSize) * 100
                : 0;

            Color sizeColor = estimatedSize < originalSize ? Color.Green : Color.Red;
            string sizeChangeText = estimatedSize < originalSize
                ? $"↓ {Math.Abs(percentChange):F1}% reduction"
                : $"↑ {Math.Abs(percentChange):F1}% increase";

            lblEstimatedSize.Text =
                $"Original: {FormatFileSize(originalSize)}\nEstimated: {FormatFileSize(estimatedSize)}\n{sizeChangeText}";
            lblEstimatedSize.ForeColor = sizeColor;
        }

        private void UpdateOriginalInfo()
        {
            if (originalImage == null || string.IsNullOrWhiteSpace(originalFilePath) || !File.Exists(originalFilePath))
                return;

            long fileSize = new FileInfo(originalFilePath).Length;
            lblOriginalInfo.Text =
                $"Dimensions: {originalImage.Width} x {originalImage.Height} px\nFile Size: {FormatFileSize(fileSize)}";
            lblOriginalInfo.ForeColor = SystemColors.ControlText;
        }

        #endregion

        #region Helpers

        private OutputFormatInfo GetSelectedFormat()
        {
            return OutputFormatInfo.FromDisplayName(cmbFormat.SelectedItem as string);
        }

        private static InterpolationMode GetInterpolationMode(int index)
        {
            switch (index)
            {
                case 1:
                    return InterpolationMode.Bilinear;
                case 2:
                    return InterpolationMode.Low;
                default:
                    return InterpolationMode.HighQualityBicubic;
            }
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double length = bytes;
            int order = 0;

            while (length >= 1024 && order < sizes.Length - 1)
            {
                order++;
                length /= 1024;
            }

            return $"{length:0.##} {sizes[order]}";
        }

        private static int ClampToRange(NumericUpDown control, int value)
        {
            if (value < control.Minimum)
                return (int)control.Minimum;

            if (value > control.Maximum)
                return (int)control.Maximum;

            return value;
        }

        private static void DisposeImage(ref Image image)
        {
            if (image != null)
            {
                image.Dispose();
                image = null;
            }
        }

        #endregion

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            sizeEstimator.Dispose();
            autoPreviewTimer.Dispose();

            pbOriginal.Image = null;
            pbProcessed.Image = null;

            DisposeImage(ref originalImage);
            DisposeImage(ref processedImage);

            base.OnFormClosing(e);
        }
    }
}
