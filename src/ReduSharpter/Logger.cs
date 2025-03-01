using System;
using System.IO;
using System.Threading;

public static class Logger
{
  private static readonly string logFilePath = "../../../output/log.txt";
  private static readonly object lockObj = new object();

  static Logger()
  {
    if (!File.Exists(logFilePath))
    {
      File.Create(logFilePath).Close();
    }
  }

  public static void Info(string message)
  {
    Log("INFO", message);
  }

  public static void Warning(string message)
  {
    Log("WARNING", message);
  }

  public static void Error(string message)
  {
    Log("ERROR", message);
  }

  private static void Log(string level, string message)
  {
    string logMessage =
      $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}";

    lock (lockObj)
    {
      Console.WriteLine(logMessage);
      File.AppendAllText(logFilePath, logMessage + Environment.NewLine);
    }
  }
}
