// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.PullUpFromStraightLedge
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class PullUpFromStraightLedge(Game game) : PlayerAction(game)
{
  private Vector3 camOrigin;
  private SoundEffect pullSound;
  private SoundEffect landSound;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.pullSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/StraightLedgeHoist");
    this.landSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeLand");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.GrabLedgeFront:
      case ActionType.GrabLedgeBack:
        if ((this.InputManager.Jump != FezButtonState.Pressed || this.InputManager.Down.IsDown()) && this.InputManager.Up != FezButtonState.Pressed && (this.InputManager.Up != FezButtonState.Down || !this.PlayerManager.Animation.Timing.Ended) || this.LevelManager.NearestTrile(this.PlayerManager.HeldInstance.Center).Deep == null)
          break;
        this.PlayerManager.Action = this.PlayerManager.Action.FacesBack() ? ActionType.PullUpBack : ActionType.PullUpFront;
        break;
      case ActionType.ShimmyFront:
      case ActionType.ShimmyBack:
        if ((this.InputManager.Jump != FezButtonState.Pressed || this.InputManager.Down.IsDown()) && this.InputManager.Up != FezButtonState.Pressed || this.LevelManager.NearestTrile(this.PlayerManager.HeldInstance.Center).Deep == null)
          break;
        this.PlayerManager.Action = this.PlayerManager.Action.FacesBack() ? ActionType.PullUpBack : ActionType.PullUpFront;
        break;
    }
  }

  protected override void Begin()
  {
    this.pullSound.EmitAt(this.PlayerManager.Position);
    this.camOrigin = this.CameraManager.Center;
    this.PlayerManager.Velocity = Vector3.Zero;
    Waiters.Wait(0.5, (Action) (() => this.landSound.EmitAt(this.PlayerManager.Position)));
    this.GomezService.OnHoist();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    Vector3 vector3 = this.PlayerManager.Size.Y / 2f * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
    if (this.PlayerManager.HeldInstance.PhysicsState != null)
    {
      this.PlayerManager.Position += this.PlayerManager.HeldInstance.PhysicsState.Velocity;
      this.camOrigin += this.PlayerManager.HeldInstance.PhysicsState.Velocity;
    }
    if (!this.CameraManager.StickyCam && !this.CameraManager.Constrained)
      this.CameraManager.Center = Vector3.Lerp(this.camOrigin, this.camOrigin + vector3, this.PlayerManager.Animation.Timing.NormalizedStep);
    this.PlayerManager.SplitUpCubeCollectorOffset = vector3 * this.PlayerManager.Animation.Timing.NormalizedStep;
    if (!this.PlayerManager.Animation.Timing.Ended)
      return true;
    this.PlayerManager.Position += vector3;
    this.PlayerManager.SplitUpCubeCollectorOffset = Vector3.Zero;
    this.PlayerManager.Position += 0.5f * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity - Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
    this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
    this.PlayerManager.HeldInstance = (TrileInstance) null;
    this.PlayerManager.Action = ActionType.Idle;
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.PullUpFront || type == ActionType.PullUpBack;
  }
}
