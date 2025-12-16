// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene32Host
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using FezGame.Components.EndCutscene32;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class EndCutscene32Host : DrawableGameComponent
{
  public static readonly Color PurpleBlack = new Color(15, 1, 27);
  public readonly List<DrawableGameComponent> Scenes = new List<DrawableGameComponent>();
  private bool firstCycle = true;
  private bool noDestroy;
  public static EndCutscene32Host Instance;

  public EndCutscene32Host(Game game)
    : base(game)
  {
    EndCutscene32Host.Instance = this;
  }

  public override void Initialize()
  {
    base.Initialize();
    DrawableGameComponent drawableGameComponent1;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent1 = (DrawableGameComponent) new Pixelizer(this.Game, this)));
    this.Scenes.Add(drawableGameComponent1);
    DrawableGameComponent drawableGameComponent2;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent2 = (DrawableGameComponent) new FezGrid(this.Game, this)));
    this.Scenes.Add(drawableGameComponent2);
    DrawableGameComponent drawableGameComponent3;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent3 = (DrawableGameComponent) new Fractal(this.Game, this)));
    this.Scenes.Add(drawableGameComponent3);
    DrawableGameComponent drawableGameComponent4;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent4 = (DrawableGameComponent) new AxisDna(this.Game, this)));
    this.Scenes.Add(drawableGameComponent4);
    DrawableGameComponent drawableGameComponent5;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent5 = (DrawableGameComponent) new TetraordialOoze(this.Game, this)));
    this.Scenes.Add(drawableGameComponent5);
    DrawableGameComponent drawableGameComponent6;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent6 = (DrawableGameComponent) new VibratingMembrane(this.Game, this)));
    this.Scenes.Add(drawableGameComponent6);
    DrawableGameComponent drawableGameComponent7;
    ServiceHelper.AddComponent((IGameComponent) (drawableGameComponent7 = (DrawableGameComponent) new DrumSolo(this.Game, this)));
    this.Scenes.Add(drawableGameComponent7);
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
    if (!(this.LevelManager.Name != "DRUM") || this.noDestroy)
      return;
    foreach (DrawableGameComponent scene in this.Scenes)
      ServiceHelper.RemoveComponent<DrawableGameComponent>(scene);
    this.Scenes.Clear();
    ServiceHelper.RemoveComponent<EndCutscene32Host>(this);
  }

  public void Cycle()
  {
    if (this.firstCycle)
    {
      this.PlayerManager.Hidden = false;
      this.GameState.SkipRendering = true;
      this.GameState.SkyOpacity = 0.0f;
      this.GameState.InEndCutscene = this.GameState.InCutscene = true;
      this.noDestroy = true;
      this.LevelManager.Reset();
      this.noDestroy = false;
      this.SoundManager.PlayNewSong("32bit", 0.0f);
      this.SoundManager.PlayNewAmbience();
      this.SoundManager.KillSounds();
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
      ServiceHelper.RemoveComponent<EndCutscene32Host>(this);
  }

  protected override void Dispose(bool disposing)
  {
    this.GameState.InEndCutscene = false;
    this.GameState.SkipRendering = false;
    EndCutscene32Host.Instance = (EndCutscene32Host) null;
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
