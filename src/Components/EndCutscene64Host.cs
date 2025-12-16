// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene64Host
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components.EndCutscene64;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class EndCutscene64Host : DrawableGameComponent
{
  public static readonly Color PurpleBlack = new Color(15, 1, 27);
  public readonly List<DrawableGameComponent> Scenes = new List<DrawableGameComponent>();
  private bool firstCycle = true;
  private bool noDestroy;
  public static EndCutscene64Host Instance;
  public SoundEmitter eNoise;

  public EndCutscene64Host(Game game)
    : base(game)
  {
    EndCutscene64Host.Instance = this;
  }

  public override void Initialize()
  {
    base.Initialize();
    DrawableGameComponent drawableGameComponent1;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent1 = (DrawableGameComponent) new ZoomOut(this.Game, this)));
    this.Scenes.Add(drawableGameComponent1);
    DrawableGameComponent drawableGameComponent2;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent2 = (DrawableGameComponent) new MulticoloredSpace(this.Game, this)));
    this.Scenes.Add(drawableGameComponent2);
    DrawableGameComponent drawableGameComponent3;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent3 = (DrawableGameComponent) new DotsAplenty(this.Game, this)));
    this.Scenes.Add(drawableGameComponent3);
    DrawableGameComponent drawableGameComponent4;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent4 = (DrawableGameComponent) new WhiteNoise(this.Game, this)));
    this.Scenes.Add(drawableGameComponent4);
    foreach (DrawableGameComponent scene in this.Scenes)
    {
      int num;
      bool flag = (num = 0) != 0;
      scene.Visible = num != 0;
      scene.Enabled = flag;
    }
    this.Scenes[0].Enabled = this.Scenes[0].Visible = true;
    this.LevelManager.LevelChanged += new Action(this.TryDestroy);
  }

  private void TryDestroy()
  {
    if (this.noDestroy)
      return;
    foreach (DrawableGameComponent scene in this.Scenes)
      ServiceHelper.RemoveComponent<DrawableGameComponent>(scene);
    this.Scenes.Clear();
    ServiceHelper.RemoveComponent<EndCutscene64Host>(this);
  }

  private void FirstCycle()
  {
    this.PlayerManager.Hidden = true;
    this.GameState.SkipRendering = true;
    this.GameState.InEndCutscene = this.GameState.InCutscene = true;
    this.GameState.SkyOpacity = 0.0f;
    this.noDestroy = true;
    this.LevelManager.Reset();
    this.noDestroy = false;
    this.SoundManager.PlayNewSong();
    this.SoundManager.PlayNewAmbience();
    this.SoundManager.KillSounds();
  }

  public void Cycle()
  {
    if (this.firstCycle)
    {
      this.FirstCycle();
      this.firstCycle = false;
    }
    ServiceHelper.RemoveComponent<DrawableGameComponent>(this.ActiveScene);
    this.Scenes.RemoveAt(0);
    if (this.Scenes.Count > 0)
    {
      this.ActiveScene.Enabled = this.ActiveScene.Visible = true;
      this.ActiveScene.Update(new GameTime());
    }
    else
      ServiceHelper.RemoveComponent<EndCutscene64Host>(this);
  }

  protected override void Dispose(bool disposing)
  {
    this.GameState.InEndCutscene = false;
    this.GameState.SkipRendering = false;
    EndCutscene64Host.Instance = (EndCutscene64Host) null;
    this.LevelManager.LevelChanged -= new Action(this.TryDestroy);
    base.Dispose(disposing);
  }

  public DrawableGameComponent ActiveScene => this.Scenes[0];

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }
}
