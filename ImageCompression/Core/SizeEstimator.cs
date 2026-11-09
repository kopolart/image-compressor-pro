using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ImageCompression.Core
{
    /// <summary>
    /// Estimates the encoded output size without freezing the UI.
    /// Requests are debounced (rapid slider changes trigger one estimate, not
    /// dozens), the resize snapshot is taken on the UI thread (GDI+ images are
    /// not thread safe) and the slow encode runs on a background thread.
    /// Stale results are discarded.
    /// </summary>
    public sealed class SizeEstimator : IDisposable
    {
        private const int DebounceMilliseconds = 350;

        private readonly Control owner;
        private readonly Func<Bitmap> createSnapshot;
        private readonly Func<OutputFormatInfo> getFormat;
        private readonly Func<int> getQuality;
        private readonly Action<long> onEstimated;
        private readonly System.Windows.Forms.Timer debounceTimer;
        private int version;

        /// <param name="owner">Control used to marshal results back to the UI thread.</param>
        /// <param name="createSnapshot">Runs on the UI thread; returns the resized bitmap to measure, or null when there is nothing to estimate. The estimator takes ownership and disposes it.</param>
        /// <param name="getFormat">Reads the currently selected output format (UI thread).</param>
        /// <param name="getQuality">Reads the current quality setting (UI thread).</param>
        /// <param name="onEstimated">Receives the estimated byte count on the UI thread, or -1 when estimation failed.</param>
        public SizeEstimator(
            Control owner,
            Func<Bitmap> createSnapshot,
            Func<OutputFormatInfo> getFormat,
            Func<int> getQuality,
            Action<long> onEstimated)
        {
            this.owner = owner;
            this.createSnapshot = createSnapshot;
            this.getFormat = getFormat;
            this.getQuality = getQuality;
            this.onEstimated = onEstimated;

            debounceTimer = new System.Windows.Forms.Timer { Interval = DebounceMilliseconds };
            debounceTimer.Tick += DebounceTimer_Tick;
        }

        /// <summary>Schedules a new estimate, resetting the debounce window.</summary>
        public void Request()
        {
            debounceTimer.Stop();
            debounceTimer.Start();
        }

        public void Dispose()
        {
            Interlocked.Increment(ref version);
            debounceTimer.Stop();
            debounceTimer.Dispose();
        }

        private void DebounceTimer_Tick(object sender, EventArgs e)
        {
            debounceTimer.Stop();

            Bitmap snapshot;
            try
            {
                snapshot = createSnapshot();
            }
            catch (Exception)
            {
                onEstimated(-1);
                return;
            }

            if (snapshot == null)
                return;

            OutputFormatInfo format = getFormat();
            int quality = getQuality();
            int requestVersion = Interlocked.Increment(ref version);

            Task.Run(() => EncodeAndReport(snapshot, format, quality, requestVersion));
        }

        private void EncodeAndReport(Bitmap snapshot, OutputFormatInfo format, int quality, int requestVersion)
        {
            long size;
            try
            {
                size = ImageEncoder.GetEncodedSize(snapshot, format, quality);
            }
            catch (Exception)
            {
                size = -1;
            }
            finally
            {
                snapshot.Dispose();
            }

            try
            {
                if (requestVersion != Volatile.Read(ref version) || !owner.IsHandleCreated || owner.IsDisposed)
                    return;

                owner.BeginInvoke((Action)delegate
                {
                    if (requestVersion == Volatile.Read(ref version))
                        onEstimated(size);
                });
            }
            catch (Exception)
            {
                // The form is closing - nothing to report.
            }
        }
    }
}
