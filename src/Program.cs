// Decompiled with JetBrains decompiler
// Type: FezGame.Program
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Tools;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using SDL2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading;

#nullable disable
namespace FezGame;

internal static class Program
{
  private static Fez fez;

  [STAThread]
  private static void Main(string[] args)
  {
    Logger.Clear();
    FNALoggerEXT.LogInfo = (Action<string>) (msg => Logger.Log("FNA", LogSeverity.Information, msg));
    FNALoggerEXT.LogWarn = (Action<string>) (msg => Logger.Log("FNA", LogSeverity.Warning, msg));
    FNALoggerEXT.LogError = (Action<string>) (msg => Logger.Log("FNA", LogSeverity.Error, msg));
    SettingsManager.InitializeSettings();
    PersistentThreadPool.SingleThreaded = SettingsManager.Settings.Singlethreaded;
    Queue<string> stringQueue = new Queue<string>();
    foreach (string str in args)
      stringQueue.Enqueue(str);
    while (stringQueue.Count > 0)
    {
      switch (stringQueue.Dequeue().ToLower(CultureInfo.InvariantCulture))
      {
        case "--attempt-highdpi":
          SettingsManager.Settings.HighDPI = true;
          continue;
        case "--clear-save-file":
        case "-c":
          string str1 = Path.Combine(Util.LocalSaveFolder, "SaveSlot");
          if (File.Exists(str1 + "0"))
          {
            File.Delete(str1 + "0");
            continue;
          }
          continue;
        case "--gotta-gomez-fast":
          Fez.SpeedRunMode = true;
          PersistentThreadPool.SingleThreaded = true;
          continue;
        case "--msaa-option":
          SettingsManager.Settings.MultiSampleOption = true;
          continue;
        case "--no-gamepad":
        case "-ng":
          SettingsManager.Settings.DisableController = true;
          continue;
        case "--no-music":
        case "-nm":
          Fez.NoMusic = true;
          continue;
        case "--public-demo":
        case "-pd":
          Fez.PublicDemo = true;
          string str2 = Path.Combine(Util.LocalSaveFolder, "SaveSlot");
          for (int index = -1; index < 3; ++index)
          {
            if (File.Exists(str2 + (object) index))
              File.Delete(str2 + (object) index);
          }
          continue;
        case "--region":
        case "-r":
          SettingsManager.Settings.Language = (Language) Enum.Parse(typeof (Language), stringQueue.Dequeue());
          continue;
        case "--singlethreaded":
        case "-st":
          SettingsManager.Settings.Singlethreaded = true;
          PersistentThreadPool.SingleThreaded = true;
          continue;
        case "--trace":
          TraceFlags.TraceContentLoad = true;
          continue;
        default:
          continue;
      }
    }
    if (SettingsManager.Settings.HighDPI)
      Environment.SetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI", "1");
    Logger.Try(new Action(Program.MainInternal));
    if (Program.fez != null)
      Program.fez.Dispose();
    Program.fez = (Fez) null;
    Logger.Log("FEZ", "Exiting.");
    if (!SDL.SDL_GetPlatform().Equals("Windows"))
      return;
    try
    {
      ThreadExecutionState.TearDown();
    }
    catch (Exception ex)
    {
      Logger.Log("ThreadExecutionState", ex.ToString());
    }
  }

  private static void MainInternal()
  {
    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
    Program.fez = new Fez();
    if (Program.fez.IsDisposed)
      return;
    Program.fez.Run();
  }
}
