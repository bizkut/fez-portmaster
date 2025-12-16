// Decompiled with JetBrains decompiler
// Type: EasyStorage.PCSaveDevice
// Assembly: EasyStorage, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: FEE2FDB6-A226-4BC3-AEBF-7DC7EDE4694E
// Assembly location: E:\GOG Games\Fez\EasyStorage.dll

using Common;
using SDL2;
using System;
using System.IO;

#nullable disable
namespace EasyStorage;

public class PCSaveDevice : ISaveDevice
{
  public const int MaxSize = 40960 /*0xA000*/;
  private static readonly string LocalSaveFolder = PCSaveDevice.GetLocalSaveFolder();

  private static string GetLocalSaveFolder()
  {
    string path;
    switch (SDL.SDL_GetPlatform())
    {
      case "Linux":
        string path1 = Environment.GetEnvironmentVariable("XDG_DATA_HOME");
        if (string.IsNullOrEmpty(path1))
        {
          string environmentVariable = Environment.GetEnvironmentVariable("HOME");
          if (string.IsNullOrEmpty(environmentVariable))
            return ".";
          path1 = environmentVariable + "/.local/share";
        }
        path = Path.Combine(path1, "FEZ");
        break;
      case "Mac OS X":
        string environmentVariable1 = Environment.GetEnvironmentVariable("HOME");
        if (string.IsNullOrEmpty(environmentVariable1))
          return ".";
        path = Path.Combine(environmentVariable1 + "/Library/Application Support", "FEZ");
        break;
      case "Windows":
        path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "FEZ");
        break;
      default:
        throw new NotImplementedException("Unhandled SDL2 platform!");
    }
    if (!Directory.Exists(path))
      Directory.CreateDirectory(path);
    return path;
  }

  public string RootDirectory { get; set; }

  public PCSaveDevice(string gameName) => this.RootDirectory = PCSaveDevice.LocalSaveFolder;

  public virtual bool Save(string fileName, SaveAction saveAction)
  {
    if (!Directory.Exists(this.RootDirectory))
      Directory.CreateDirectory(this.RootDirectory);
    string str = Path.Combine(this.RootDirectory, fileName);
    if (File.Exists(str))
      File.Copy(str, str + "_Backup", true);
    try
    {
      byte[] buffer = new byte[40960 /*0xA000*/];
      using (MemoryStream output = new MemoryStream(buffer))
      {
        using (BinaryWriter writer = new BinaryWriter((Stream) output))
        {
          writer.Write(DateTime.Now.ToFileTime());
          saveAction(writer);
          if (output.Length < 40960L /*0xA000*/)
          {
            long length = 40960L /*0xA000*/ - output.Length;
            writer.Write(new byte[length]);
          }
          else if (output.Length > 40960L /*0xA000*/)
            throw new InvalidOperationException("Save file greater than the imposed limit!");
        }
      }
      using (FileStream output = new FileStream(str, FileMode.Create, FileAccess.Write, FileShare.Read))
      {
        using (BinaryWriter binaryWriter = new BinaryWriter((Stream) output))
          binaryWriter.Write(buffer);
      }
      return true;
    }
    catch (Exception ex)
    {
      Logger.Log("SaveDevice", LogSeverity.Warning, "Error while saving : " + (object) ex);
    }
    return false;
  }

  public virtual bool Load(string fileName, LoadAction loadAction)
  {
    if (!Directory.Exists(this.RootDirectory))
      Directory.CreateDirectory(this.RootDirectory);
    bool flag = false;
    string path = Path.Combine(this.RootDirectory, fileName);
    if (!File.Exists(path))
      return false;
    try
    {
      using (FileStream input = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
      {
        using (BinaryReader reader = new BinaryReader((Stream) input))
        {
          reader.ReadInt64();
          loadAction(reader);
          flag = true;
        }
      }
    }
    catch (Exception ex)
    {
      if (!fileName.EndsWith("_Backup"))
      {
        if (File.Exists(path + "_Backup"))
        {
          Logger.Log("SaveDevice", LogSeverity.Warning, $"{path} | Loading error, will try with backup : {(object) ex}");
          return this.Load(fileName + "_Backup", loadAction);
        }
        Logger.Log("SaveDevice", LogSeverity.Warning, $"{path} | Loading error, no backup found : {(object) ex}");
      }
      else
        Logger.Log("SaveDevice", LogSeverity.Warning, $"{path} | Error loading backup : {(object) ex}");
    }
    return flag;
  }

  public virtual bool Delete(string fileName)
  {
    if (!Directory.Exists(this.RootDirectory))
      Directory.CreateDirectory(this.RootDirectory);
    string path = Path.Combine(this.RootDirectory, fileName);
    if (!File.Exists(path))
      return true;
    File.Delete(path);
    return !File.Exists(path);
  }

  public virtual bool FileExists(string fileName)
  {
    if (!Directory.Exists(this.RootDirectory))
      Directory.CreateDirectory(this.RootDirectory);
    return File.Exists(Path.Combine(this.RootDirectory, fileName));
  }
}
