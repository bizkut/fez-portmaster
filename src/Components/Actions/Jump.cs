// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Jump
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components.Actions;

public class Jump(Game game) : PlayerAction(game)
{
  public const float SideJumpStrength = 0.25f;
  public const float UpJumpStrength = 1.025f;
  private SoundEffect jumpSound;
  private TimeSpan sinceJumped;
  private bool scheduleJump;

  public override void Initialize()
  {
    this.jumpSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Jump");
  }

  protected override void TestConditions()
  {
    if ((FezMath.In<ActionType>(this.PlayerManager.Action, ActionType.Sliding, ActionType.GrabCornerLedge, ActionType.Running, ActionType.RunTurnAround, ActionType.Walking, ActionType.Landing, ActionType.WalkingTo, ActionType.GrabTombstone, (IEqualityComparer<ActionType>) ActionTypeComparer.Default) || this.PlayerManager.Climbing || this.PlayerManager.Swimming || this.PlayerManager.Action.IsIdle() || this.PlayerManager.Action == ActionType.Falling && this.PlayerManager.CanDoubleJump ? 1 : (this.PlayerManager.Action == ActionType.Grabbing || this.PlayerManager.Action == ActionType.Pushing ? 1 : (this.PlayerManager.Action.IsLookingAround() ? 1 : 0))) == 0 || this.InputManager.Jump != FezButtonState.Pressed && (!this.PlayerManager.Grounded && !this.PlayerManager.Action.IsOnLedge() || (double) this.PlayerManager.Velocity.Y * (double) Math.Sign(this.CollisionManager.GravityFactor) <= 0.1))
      return;
    this.PlayerManager.PushedInstance = (TrileInstance) null;
    if (this.PlayerManager.CanDoubleJump)
      this.PlayerManager.CanDoubleJump = false;
    if (this.InputManager.Down.IsDown() && (this.PlayerManager.Grounded && this.PlayerManager.Ground.First.GetRotatedFace(this.CameraManager.Viewpoint.VisibleOrientation()) == CollisionType.TopOnly || this.PlayerManager.Climbing))
      return;
    if (this.PlayerManager.Action == ActionType.GrabCornerLedge)
    {
      HorizontalDirection horizontalDirection = FezMath.DirectionFromMovement(this.InputManager.Movement.X);
      if (horizontalDirection == HorizontalDirection.None || horizontalDirection == this.PlayerManager.LookingDirection)
        return;
      Vector3 position = this.PlayerManager.Position;
      this.PlayerManager.Position += this.CameraManager.Viewpoint.RightVector() * (float) -this.PlayerManager.LookingDirection.Sign();
      this.PhysicsManager.DetermineInBackground((IPhysicsEntity) this.PlayerManager, true, false, false);
      this.PlayerManager.Position = position;
    }
    if (this.InputManager.Jump == FezButtonState.Pressed)
    {
      this.sinceJumped = TimeSpan.Zero;
      this.scheduleJump = true;
    }
    else
      this.DoJump();
    this.PlayerManager.Action = ActionType.Jumping;
  }

  private void DoJump()
  {
    bool flag = this.PlayerManager.LastAction.IsClimbingLadder() || this.PlayerManager.LastAction.IsClimbingVine() || this.PlayerManager.HeldInstance != null;
    if (flag)
    {
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity + this.CameraManager.Viewpoint.RightVector() * this.InputManager.Movement.X * 0.25f;
    }
    this.PlayerManager.HeldInstance = (TrileInstance) null;
    if (this.scheduleJump || this.InputManager.Jump == FezButtonState.Pressed)
      this.jumpSound.EmitAt(this.PlayerManager.Position);
    float gravityFactor = this.CollisionManager.GravityFactor;
    float num = (float) ((1.3250000476837158 + (double) Math.Abs(gravityFactor) * 0.675000011920929) / 2.0) * (float) Math.Sign(gravityFactor);
    IPlayerManager playerManager1 = this.PlayerManager;
    playerManager1.Velocity = playerManager1.Velocity * FezMath.XZMask;
    Vector3 vector3 = (float) (0.15000000596046448 * (double) num * 1.0249999761581421 * (flag || this.PlayerManager.Swimming ? 0.77499997615814209 : 1.0)) * Vector3.UnitY;
    IPlayerManager playerManager2 = this.PlayerManager;
    playerManager2.Velocity = playerManager2.Velocity + vector3;
    this.sinceJumped = TimeSpan.Zero;
    this.GomezService.OnJump();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    this.sinceJumped += elapsed;
    if (this.scheduleJump && this.sinceJumped.TotalMilliseconds >= 60.0)
    {
      this.DoJump();
      this.scheduleJump = false;
    }
    if (this.PlayerManager.Grounded)
      WalkRun.MovementHelper.Update((float) elapsed.TotalSeconds);
    else if (this.InputManager.Jump == FezButtonState.Down)
    {
      float num1 = this.sinceJumped.TotalSeconds < 0.25 ? 0.6f : 0.0f;
      float gravityFactor = this.CollisionManager.GravityFactor;
      float num2 = (float) ((1.2000000476837158 + (double) Math.Abs(gravityFactor) * 0.800000011920929) / 2.0) * (float) Math.Sign(gravityFactor);
      Vector3 vector3 = (float) (elapsed.TotalSeconds * (double) num1 * (double) num2 * 1.0249999761581421 / 2.0) * Vector3.UnitY;
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity + vector3;
    }
    if (((double) this.CollisionManager.GravityFactor < 0.0 ? ((double) this.PlayerManager.Velocity.Y >= 0.0 ? 1 : 0) : ((double) this.PlayerManager.Velocity.Y <= 0.0 ? 1 : 0)) != 0 && !this.PlayerManager.Grounded && !this.scheduleJump)
      this.PlayerManager.Action = ActionType.Falling;
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Jumping;
}
