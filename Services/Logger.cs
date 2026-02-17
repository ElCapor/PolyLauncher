using System;
using System.IO;
using System.Diagnostics;

namespace PolyLauncher.Services
{
    public static class Logger
    {
        private static string _logPath;
        private static object _lock = new object();

        static Logger()
        {
            try
            {
                // Try to put it next to EXE, if fails (e.g. Program Files), put in AppData
                string localPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Launcher.log");
                using (var fs = File.Open(localPath, FileMode.OpenOrCreate, FileAccess.Write)) { }
                _logPath = localPath;
            }
            catch
            {
                var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                var logDir = Path.Combine(appData, "PolyLauncher");
                Directory.CreateDirectory(logDir);
                _logPath = Path.Combine(logDir, "Launcher.log");
            }
        }

        public static void Initialize()
        {
            try
            {
                // Overwrite on each launch
                File.WriteAllText(_logPath, $"--- PolyLauncher Log Started at {DateTime.Now} ---\n");
            }
            catch
            {
                // Ignore if log file is inaccessible
            }
        }

        public static void Log(string message, string level = "INFO")
        {
            lock (_lock)
            {
                try
                {
                    string logLine = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}";
                    File.AppendAllText(_logPath, logLine + Environment.NewLine);
                    Debug.WriteLine(logLine);
                }
                catch
                {
                    // Ignore
                }
            }
        }

        public static void LogError(string message, Exception? ex = null)
        {
            string fullMessage = message;
            if (ex != null)
            {
                fullMessage += $"\nException: {ex.Message}\nStack Trace: {ex.StackTrace}";
            }
            Log(fullMessage, "ERROR");
        }
    }

    /// <summary>
    /// Custom TextWriter to redirect Console output to our Logger
    /// </summary>
    public class LogWriter : TextWriter
    {
        public override System.Text.Encoding Encoding => System.Text.Encoding.UTF8;

        public override void WriteLine(string? value)
        {
            if (value != null)
            {
                Logger.Log(value, "CONSOLE");
            }
        }

        public override void Write(string? value)
        {
            if (value != null)
            {
                Logger.Log(value, "CONSOLE");
            }
        }
    }
}
