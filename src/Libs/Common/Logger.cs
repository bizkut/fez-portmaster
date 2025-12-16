// Decompiled with JetBrains decompiler
// Type: Common.Logger
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

#nullable disable
namespace Common;

public static class Logger
{
  private const string TimeFormat = "HH:mm:ss.fff";
  private const string DateTimeFormat = "yyyy-MM-dd";
  private static readonly HashSet<string> OnceLogged = new HashSet<string>();
  private static bool FirstLog = true;
  private static string LogFilePath;
  private static bool errorEncountered;

  public static void LogOnce(string component, LogSeverity severity, string message)
  {
    if (Logger.OnceLogged.Contains(component + message))
      return;
    Logger.OnceLogged.Add(component + message);
    Logger.Log(component, severity, message);
  }

  public static void LogOnce(string component, string message)
  {
    Logger.LogOnce(component, LogSeverity.Information, message);
  }

  public static void Log(string component, string message)
  {
    Logger.Log(component, LogSeverity.Information, message);
  }

  public static void Log(string component, LogSeverity severity, string message)
  {
    if (Logger.FirstLog)
    {
      try
      {
        string path = Path.Combine(Util.LocalSaveFolder, "Debug Log.txt");
        if (File.Exists(path))
          File.Delete(path);
      }
      catch
      {
      }
      int num = 1;
      for (Logger.LogFilePath = Path.Combine(Util.LocalSaveFolder, $"[{DateTime.Now.ToString("yyyy-MM-dd")}] Debug Log.txt"); File.Exists(Logger.LogFilePath); Logger.LogFilePath = Path.Combine(Util.LocalSaveFolder, $"[{DateTime.Now.ToString("yyyy-MM-dd")}] Debug Log #{num}.txt"))
        ++num;
      Logger.FirstLog = false;
      try
      {
        foreach (string file in Directory.GetFiles(Util.LocalSaveFolder, "*.txt"))
        {
          if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(file)).TotalDays > 31.0)
            File.Delete(file);
        }
      }
      catch (Exception ex)
      {
        Logger.Log(nameof (Logger), LogSeverity.Warning, "Log archival failed : " + ex.ToString());
      }
    }
    try
    {
      using (FileStream fileStream = File.Open(Logger.LogFilePath, FileMode.Append))
      {
        using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream))
          streamWriter.WriteLine("({0}) [{1}] {2} : {3}", (object) DateTime.Now.ToString("HH:mm:ss.fff"), (object) component, (object) severity.ToString().ToUpper(CultureInfo.InvariantCulture), (object) message);
      }
    }
    catch (Exception ex)
    {
    }
    if (severity != LogSeverity.Error)
      return;
    Logger.errorEncountered = true;
  }

  public static void Try(Action action)
  {
    try
    {
      action();
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
    }
  }

  public static void Try<T>(Action<T> action, T arg)
  {
    try
    {
      action(arg);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
    }
  }

  public static void Try<T, U>(Action<T, U> action, T arg1, U arg2)
  {
    try
    {
      action(arg1, arg2);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
    }
  }

  public static void Try<T, U, V>(Action<T, U, V> action, T arg1, U arg2, V arg3)
  {
    try
    {
      action(arg1, arg2, arg3);
    }
    catch (Exception ex)
    {
      Logger.LogError(ex);
    }
  }

  public static void LogError(Exception e)
  {
    Logger.Log("Unhandled Exception", LogSeverity.Error, e.ToString());
  }

  public static void Clear() => Logger.errorEncountered = false;

  public static bool ErrorEncountered
  {
    get => Logger.errorEncountered;
    private set => Logger.errorEncountered = value;
  }
}
