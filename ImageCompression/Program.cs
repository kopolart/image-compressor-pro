using System;
using System.Threading;
using System.Windows.Forms;

namespace ImageCompression
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Global handlers: an unexpected error shows a message instead of
            // silently killing the application.
            Application.ThreadException += OnThreadException;
            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            Application.Run(new MainForm());
        }

        private static void OnThreadException(object sender, ThreadExceptionEventArgs e)
        {
            ShowError(e.Exception);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            ShowError(e.ExceptionObject as Exception);
        }

        private static void ShowError(Exception exception)
        {
            string message = exception == null
                ? "An unknown error occurred."
                : exception.Message;

            MessageBox.Show(
                "Unexpected error: " + message,
                "Image Compressor",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
}
