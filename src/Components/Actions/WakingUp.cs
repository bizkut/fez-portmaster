// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.WakingUp
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class WakingUp : PlayerAction
{
  private static readonly TimeSpan FadeTime = TimeSpan.FromSeconds(1.0);
  private readonly Mesh fadePlane;
  private TimeSpan sinceStarted;
  private bool respawned;
  private bool diedByLava;

  public WakingUp(Game game)
    : base(game)
  {
    this.fadePlane = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false
    };
    this.fadePlane.AddFace(Vector3.One * 2f, Vector3.Zero, FaceOrientation.Front, Color.Black, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh fadePlane = this.fadePlane;
      fadePlane.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        ForcedViewMatrix = new Matrix?(Matrix.Identity),
        ForcedProjectionMatrix = new Matrix?(Matrix.Identity)
      };
    }));
    this.Visible = false;
    this.DrawOrder = 101;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.ReInitialize);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      this.ReInitialize();
      this.PlayerManager.Action = ActionType.Idle;
    }
    return true;
  }

  private void ReInitialize()
  {
    if (this.PlayerManager.Action.IsIdle() && this.diedByLava)
      return;
    this.diedByLava = false;
    this.Visible = false;
    this.respawned = false;
    this.sinceStarted = TimeSpan.FromSeconds(-1.0);
    this.GameState.SkipFadeOut = false;
  }

  public override void Draw(GameTime gameTime)
  {
    TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
    if (this.CameraManager.ActionRunning)
      this.sinceStarted += elapsedGameTime;
    if (this.sinceStarted >= WakingUp.FadeTime && !this.respawned)
    {
      this.PlayerManager.RespawnAtCheckpoint();
      if (!this.GameState.SkipFadeOut)
        this.CameraManager.Constrained = false;
      this.respawned = true;
    }
    if (this.GameState.SkipFadeOut)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    this.fadePlane.Material.Opacity = 1f - Math.Abs((float) (1.0 - (double) this.sinceStarted.Ticks / (double) WakingUp.FadeTime.Ticks));
    this.fadePlane.Draw();
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.WakingUp;
}
