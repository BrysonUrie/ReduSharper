using System;
using System.IO;

namespace RosylinHDD
{
    public static class Logger
    {
        public static void LogError(Exception ex, string outputDirectoryPath)
        {
            try
            {
                string logPath = Path.Combine(outputDirectoryPath, "error.log");
                File.AppendAllText(logPath, $"{DateTime.Now}: {ex.Message}{Environment.NewLine}{ex.StackTrace}{Environment.NewLine}");
            }
            catch
            {
                Console.WriteLine("Failed to log the error to the file.");
            }
        }
    }
}
