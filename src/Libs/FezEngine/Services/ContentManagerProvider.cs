// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.ContentManagerProvider
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable disable
namespace FezEngine.Services;

public class ContentManagerProvider : GameComponent, IContentManagerProvider
{
  private readonly ContentManager global;
  private readonly Dictionary<string, SharedContentManager> levelScope;
  private readonly Dictionary<CM, SharedContentManager> temporary;

  public ContentManagerProvider(Game game)
    : base(game)
  {
    this.global = (ContentManager) new SharedContentManager(nameof (Global));
    this.levelScope = new Dictionary<string, SharedContentManager>();
    this.temporary = new Dictionary<CM, SharedContentManager>();
  }

  public override void Initialize()
  {
    this.LevelManager.LevelChanged += new Action(this.CleanAndPrecache);
  }

  private void CleanAndPrecache()
  {
    foreach (string key in this.levelScope.Keys.ToArray<string>())
    {
      if (key != this.LevelManager.Name)
      {
        this.levelScope[key].Dispose();
        this.levelScope.Remove(key);
      }
    }
  }

  public ContentManager Global => this.global;

  public ContentManager GetForLevel(string levelName)
  {
    SharedContentManager forLevel;
    if (!this.levelScope.TryGetValue(levelName, out forLevel))
    {
      this.levelScope.Add(levelName, forLevel = new SharedContentManager(levelName));
      forLevel.RootDirectory = this.global.RootDirectory;
    }
    return (ContentManager) forLevel;
  }

  public ContentManager Get(CM name)
  {
    SharedContentManager sharedContentManager;
    if (!this.temporary.TryGetValue(name, out sharedContentManager))
    {
      this.temporary.Add(name, sharedContentManager = new SharedContentManager(name.ToString()));
      sharedContentManager.RootDirectory = this.global.RootDirectory;
    }
    return (ContentManager) sharedContentManager;
  }

  public void Dispose(CM name)
  {
    SharedContentManager sharedContentManager;
    if (!this.temporary.TryGetValue(name, out sharedContentManager))
      return;
    sharedContentManager.Dispose();
    this.temporary.Remove(name);
  }

  public ContentManager CurrentLevel => this.GetForLevel(this.LevelManager.Name ?? "");

  public IEnumerable<string> GetAllIn(string directory)
  {
    directory = directory.Replace('/', '\\').ToLower(CultureInfo.InvariantCulture);
    return MemoryContentManager.AssetNames.Where<string>((Func<string, bool>) (x => x.StartsWith(directory)));
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }
}
