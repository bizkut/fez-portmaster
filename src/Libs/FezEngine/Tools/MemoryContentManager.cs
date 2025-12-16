// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.MemoryContentManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

#nullable disable
namespace FezEngine.Tools;

public class MemoryContentManager(IServiceProvider serviceProvider, string rootDirectory) : 
  ContentManager(serviceProvider, rootDirectory)
{
  private static Dictionary<string, byte[]> cachedAssets;
  private static readonly object ReadLock = new object();

  private string TitleUpdateRoot => this.RootDirectory;

  public void LoadEssentials()
  {
    MemoryContentManager.cachedAssets = new Dictionary<string, byte[]>(3011);
    using (FileStream input = File.OpenRead(Path.Combine(this.RootDirectory, "Essentials.pak")))
    {
      using (BinaryReader binaryReader = new BinaryReader((Stream) input))
      {
        int num1 = binaryReader.ReadInt32();
        for (int index = 0; index < num1; ++index)
        {
          string key = binaryReader.ReadString();
          int num2 = binaryReader.ReadInt32();
          if (!MemoryContentManager.cachedAssets.ContainsKey(key))
            MemoryContentManager.cachedAssets.Add(key, binaryReader.ReadBytes(num2));
          else
            binaryReader.BaseStream.Seek((long) num2, SeekOrigin.Current);
        }
      }
    }
  }

  public void Preload()
  {
    Action<string> action = (Action<string>) (name =>
    {
      using (FileStream input = File.OpenRead(Path.Combine(this.RootDirectory, name)))
      {
        using (BinaryReader binaryReader = new BinaryReader((Stream) input))
        {
          int num1 = binaryReader.ReadInt32();
          for (int index = 0; index < num1; ++index)
          {
            string key = binaryReader.ReadString();
            int num2 = binaryReader.ReadInt32();
            bool flag;
            lock (MemoryContentManager.ReadLock)
              flag = MemoryContentManager.cachedAssets.ContainsKey(key);
            if (!flag)
            {
              byte[] numArray = binaryReader.ReadBytes(num2);
              lock (MemoryContentManager.ReadLock)
                MemoryContentManager.cachedAssets.Add(key, numArray);
            }
            else
              binaryReader.BaseStream.Seek((long) num2, SeekOrigin.Current);
          }
        }
      }
    });
    action("Updates.pak");
    action("Other.pak");
  }

  protected override Stream OpenStream(string assetName)
  {
    lock (MemoryContentManager.ReadLock)
    {
      byte[] buffer;
      if (!MemoryContentManager.cachedAssets.TryGetValue(assetName.ToLower(CultureInfo.InvariantCulture).Replace('/', '\\'), out buffer))
        throw new ContentLoadException("Can't find asset named : " + assetName);
      return (Stream) new MemoryStream(buffer, 0, buffer.Length, true, true);
    }
  }

  public static IEnumerable<string> AssetNames
  {
    get => (IEnumerable<string>) MemoryContentManager.cachedAssets.Keys;
  }

  public static bool AssetExists(string name)
  {
    return MemoryContentManager.cachedAssets.ContainsKey(name.Replace('/', '\\').ToLower(CultureInfo.InvariantCulture));
  }
}
