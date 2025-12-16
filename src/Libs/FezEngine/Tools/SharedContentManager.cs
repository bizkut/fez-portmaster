// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.SharedContentManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects.Structures;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

#nullable disable
namespace FezEngine.Tools;

public class SharedContentManager : ContentManager
{
  private static SharedContentManager.CommonContentManager Common;
  private readonly string Name;
  private List<string> loadedAssets;

  public SharedContentManager(string name)
    : base((IServiceProvider) ServiceHelper.Game.Services, ServiceHelper.Game.Content.RootDirectory)
  {
    if (SharedContentManager.Common == null)
    {
      SharedContentManager.Common = new SharedContentManager.CommonContentManager((IServiceProvider) ServiceHelper.Game.Services, ServiceHelper.Game.Content.RootDirectory);
      SharedContentManager.Common.LoadEssentials();
    }
    this.Name = name;
    this.loadedAssets = new List<string>();
  }

  public static string GetCleanPath(string path)
  {
    path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
    int startIndex1;
    for (int startIndex2 = 1; startIndex2 < path.Length; startIndex2 = Math.Max(startIndex1 - 1, 1))
    {
      int num = path.IndexOf("\\..\\", startIndex2);
      if (num < 0)
        return path;
      startIndex1 = path.LastIndexOf(Path.DirectorySeparatorChar, num - 1) + 1;
      path = path.Remove(startIndex1, num - startIndex1 + "\\..\\".Length);
    }
    return path;
  }

  public override T Load<T>(string assetName)
  {
    assetName = SharedContentManager.GetCleanPath(assetName);
    this.loadedAssets.Add(assetName);
    return SharedContentManager.Common.Load<T>(this.Name, assetName);
  }

  public override void Unload()
  {
    if (this.loadedAssets == null)
      throw new ObjectDisposedException(typeof (SharedContentManager).Name);
    SharedContentManager.Common.Unload(this);
    this.loadedAssets = (List<string>) null;
    base.Unload();
  }

  public static void Preload() => SharedContentManager.Common.Preload();

  private class CommonContentManager(IServiceProvider serviceProvider, string rootDirectory) : 
    MemoryContentManager(serviceProvider, rootDirectory)
  {
    private readonly Dictionary<string, SharedContentManager.CommonContentManager.ReferencedAsset> references = new Dictionary<string, SharedContentManager.CommonContentManager.ReferencedAsset>();

    public T Load<T>(string name, string assetName)
    {
      lock (this)
      {
        assetName = SharedContentManager.GetCleanPath(assetName);
        SharedContentManager.CommonContentManager.ReferencedAsset referencedAsset;
        if (!this.references.TryGetValue(assetName, out referencedAsset))
        {
          if (TraceFlags.TraceContentLoad)
            Logger.Log("Content", $"[{name}] Loading {typeof (T).Name} {assetName}");
          referencedAsset = new SharedContentManager.CommonContentManager.ReferencedAsset()
          {
            Asset = (object) this.ReadAsset<T>(assetName)
          };
          this.references.Add(assetName, referencedAsset);
        }
        ++referencedAsset.References;
        if (referencedAsset.Asset is SoundEffect)
          (referencedAsset.Asset as SoundEffect).Name = assetName.Substring("Sounds/".Length);
        return (T) referencedAsset.Asset;
      }
    }

    private T ReadAsset<T>(string assetName)
    {
      return this.ReadAsset<T>(assetName, new Action<IDisposable>(Util.NullAction<IDisposable>));
    }

    public void Unload(SharedContentManager container)
    {
      lock (this)
      {
        foreach (string loadedAsset in container.loadedAssets)
        {
          if (loadedAsset == null)
          {
            Logger.Log("Content", LogSeverity.Warning, "Null-named asset in content manager : " + container.Name);
          }
          else
          {
            SharedContentManager.CommonContentManager.ReferencedAsset referencedAsset;
            if (!this.references.TryGetValue(loadedAsset, out referencedAsset))
            {
              Logger.Log("Content", LogSeverity.Warning, "Couldn't find asset in references : " + loadedAsset);
            }
            else
            {
              --referencedAsset.References;
              if (referencedAsset.References == 0)
              {
                if (referencedAsset.Asset is Texture)
                  (referencedAsset.Asset as Texture).Unhook();
                if (referencedAsset.Asset is IDisposable)
                  (referencedAsset.Asset as IDisposable).Dispose();
                this.references.Remove(loadedAsset);
                referencedAsset.Asset = (object) null;
              }
            }
          }
        }
      }
    }

    private class ReferencedAsset
    {
      public object Asset;
      public int References;
    }
  }
}
