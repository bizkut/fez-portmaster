// Decompiled with JetBrains decompiler
// Type: FezGame.Components.StaticPreloader
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Services;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

public class StaticPreloader(Game game) : GameComponent(game)
{
  public override void Initialize()
  {
    base.Initialize();
    SharedContentManager.Preload();
    Logger.Log(nameof (StaticPreloader), "SharedContentManager preloaded.");
    this.SoundManager.InitializeLibrary();
    Logger.Log(nameof (StaticPreloader), "Music library initialized.");
    this.SoundManager.ReloadVolumeLevels();
    Logger.Log(nameof (StaticPreloader), "Volume levels loaded.");
    this.PlayerManager.FillAnimations();
    Logger.Log(nameof (StaticPreloader), "Animations filled.");
    TextScroll.PreInitialize();
    Logger.Log(nameof (StaticPreloader), "Text scroll pre-initialized.");
    WorldMap.PreInitialize();
    Logger.Log(nameof (StaticPreloader), "World map pre-initialized.");
    PauseMenu.PreInitialize();
    Logger.Log(nameof (StaticPreloader), "Pause menu pre-initialized.");
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }
}
