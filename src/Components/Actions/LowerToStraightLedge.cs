// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.LowerToStraightLedge
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class LowerToStraightLedge : PlayerAction
{
  private Vector3 camOrigin;
  private SoundEffect sound;
  private SoundEffect sLowerToLedge;

  public LowerToStraightLedge(Game game)
    : base(game)
  {
    this.UpdateOrder = 3;
  }

  protected override void LoadContent()
  {
    this.sound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeGrab");
    this.sLowerToLedge = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LowerToLedge");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.LookingLeft:
      case ActionType.LookingRight:
      case ActionType.LookingUp:
      case ActionType.LookingDown:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.Sliding:
      case ActionType.Landing:
        if (this.PlayerManager.Background || (this.InputManager.Jump != FezButtonState.Pressed || !this.InputManager.Down.IsDown()) && (!this.PlayerManager.Action.IsOnLedge() || this.InputManager.Down != FezButtonState.Pressed) || !this.PlayerManager.Grounded || this.PlayerManager.Ground.First.GetRotatedFace(this.CameraManager.Viewpoint.VisibleOrientation()) != CollisionType.TopOnly || !FezMath.AlmostEqual(this.InputManager.Movement.X, 0.0f))
          break;
        TrileInstance trileInstance = this.PlayerManager.Ground.NearLow ?? this.PlayerManager.Ground.FarHigh;
        if (this.CollisionManager.CollideEdge(trileInstance.Center + trileInstance.TransformedSize * Vector3.UnitY * 0.498f, Vector3.Down * (float) Math.Sign(this.CollisionManager.GravityFactor), this.PlayerManager.Size * FezMath.XZMask / 2f, Direction2D.Vertical).AnyHit())
        {
          this.PlayerManager.Position -= Vector3.UnitY * 0.01f * (float) Math.Sign(this.CollisionManager.GravityFactor);
          IPlayerManager playerManager = this.PlayerManager;
          playerManager.Velocity = playerManager.Velocity - 0.0075000003f * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
          break;
        }
        if (this.PlayerManager.Grounded)
        {
          TrileInstance surface = this.LevelManager.NearestTrile(this.PlayerManager.Ground.First.Center, QueryOptions.None).Surface;
          if (surface != null && surface.Trile.ActorSettings.Type == ActorType.Ladder)
            break;
        }
        this.PlayerManager.HeldInstance = this.PlayerManager.Ground.NearLow;
        this.CameraManager.Viewpoint.SideMask();
        this.PlayerManager.Velocity = Vector3.Zero;
        this.PlayerManager.Action = ActionType.LowerToLedge;
        Waiters.Wait(0.3, (Action) (() =>
        {
          if (this.PlayerManager.Action != ActionType.LowerToLedge)
            return;
          this.sound.EmitAt(this.PlayerManager.Position);
          this.PlayerManager.Velocity = Vector3.Zero;
        }));
        this.camOrigin = this.CameraManager.Center;
        break;
    }
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.HeldInstance == null)
    {
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    if (this.PlayerManager.HeldInstance.PhysicsState != null)
      this.camOrigin += this.PlayerManager.HeldInstance.PhysicsState.Velocity;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.SideMask();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.DepthMask();
    Vector3 vector3_3 = this.CameraManager.Viewpoint.ForwardVector();
    this.PlayerManager.Position = this.PlayerManager.Position * vector3_1 + this.PlayerManager.HeldInstance.Center * (Vector3.UnitY + vector3_2) + vector3_3 * (float) -(0.5 + (double) this.PlayerManager.Size.X / 2.0) + this.PlayerManager.HeldInstance.Trile.Size.Y / 2f * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
    this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + this.PlayerManager.HeldInstance.Center * vector3_2 + vector3_3 * -(this.PlayerManager.HeldInstance.TransformedSize / 2f + this.PlayerManager.Size.X * vector3_2 / 4f);
    this.PhysicsManager.HugWalls((IPhysicsEntity) this.PlayerManager, false, false, true);
    Vector3 vector3_4 = this.PlayerManager.Size.Y / 2f * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
    if (!this.CameraManager.StickyCam && !this.CameraManager.Constrained)
      this.CameraManager.Center = Vector3.Lerp(this.camOrigin, this.camOrigin - vector3_4, this.PlayerManager.Animation.Timing.NormalizedStep);
    this.PlayerManager.SplitUpCubeCollectorOffset = vector3_4 * (1f - this.PlayerManager.Animation.Timing.NormalizedStep);
    if (!this.PlayerManager.Animation.Timing.Ended)
      return true;
    this.PlayerManager.SplitUpCubeCollectorOffset = Vector3.Zero;
    this.PlayerManager.Action = ActionType.GrabLedgeBack;
    return false;
  }

  protected override void Begin()
  {
    this.sLowerToLedge.EmitAt(this.PlayerManager.Position);
    base.Begin();
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.LowerToLedge;
}
