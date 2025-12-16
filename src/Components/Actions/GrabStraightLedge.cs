// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.GrabStraightLedge
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
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

public class GrabStraightLedge(Game game) : PlayerAction(game)
{
  private Viewpoint? rotatedFrom;
  private SoundEffect sound;

  protected override void LoadContent()
  {
    this.sound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeGrab");
    this.CameraManager.ViewpointChanged += (Action) (() =>
    {
      if (!this.IsActionAllowed(this.PlayerManager.Action) || !this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.Viewpoint == this.CameraManager.LastViewpoint || this.PlayerManager.IsOnRotato || this.PlayerManager.FreshlyRespawned)
        return;
      if (this.rotatedFrom.HasValue && this.rotatedFrom.Value == this.CameraManager.Viewpoint)
      {
        this.rotatedFrom = new Viewpoint?();
      }
      else
      {
        if (this.rotatedFrom.HasValue)
          return;
        this.rotatedFrom = new Viewpoint?(this.CameraManager.LastViewpoint);
      }
    });
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Jumping:
      case ActionType.Falling:
        if (!this.InputManager.Up.IsDown())
          break;
        FaceOrientation face = this.CameraManager.Viewpoint.VisibleOrientation();
        TrileInstance deep1 = this.PlayerManager.AxisCollision[VerticalDirection.Up].Deep;
        TrileInstance deep2 = this.PlayerManager.AxisCollision[VerticalDirection.Down].Deep;
        if (deep1 != null && deep1.GetRotatedFace(face) == CollisionType.AllSides || deep2 == null || deep2.GetRotatedFace(face) != CollisionType.TopOnly || this.CollisionManager.CollideEdge(deep2.Center, Vector3.Down * (float) Math.Sign(this.CollisionManager.GravityFactor), this.PlayerManager.Size * FezMath.XZMask / 2f, Direction2D.Vertical).AnyHit())
          break;
        TrileInstance surface = this.PlayerManager.AxisCollision[VerticalDirection.Down].Surface;
        if (surface != null && surface.Trile.ActorSettings.Type.IsClimbable() || !deep2.Enabled || this.PlayerManager.Action == ActionType.Jumping && (double) ((deep2.Center - this.PlayerManager.LeaveGroundPosition) * this.CameraManager.Viewpoint.ScreenSpaceMask()).Length() < 1.25)
          break;
        this.PlayerManager.Action = ActionType.GrabLedgeBack;
        Vector3 vector3_1 = this.CameraManager.Viewpoint.DepthMask();
        Vector3 vector3_2 = this.CameraManager.Viewpoint.SideMask();
        Vector3 vector3_3 = this.CameraManager.Viewpoint.ForwardVector();
        this.PlayerManager.HeldInstance = deep2;
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity * (vector3_2 * 0.5f);
        this.PlayerManager.Position = this.PlayerManager.Position * vector3_2 + deep2.Center * (Vector3.UnitY + vector3_1) + vector3_3 * -(this.PlayerManager.HeldInstance.TransformedSize / 2f + this.PlayerManager.Size.X * vector3_1 / 4f) + this.PlayerManager.HeldInstance.Trile.Size.Y / 2f * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
        Waiters.Wait(0.1, (Action) (() =>
        {
          this.sound.EmitAt(this.PlayerManager.Position);
          this.PlayerManager.Velocity = Vector3.Zero;
        }));
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    this.GomezService.OnGrabLedge();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.rotatedFrom.HasValue && (double) this.CameraManager.ViewTransitionStep >= 0.6)
    {
      int distance = this.CameraManager.Viewpoint.GetDistance(this.rotatedFrom.Value);
      if (Math.Abs(distance) % 2 == 0)
      {
        this.PlayerManager.Background = !this.PlayerManager.Background;
        this.PlayerManager.Action = this.PlayerManager.Action.FacesBack() ? ActionType.GrabLedgeFront : ActionType.GrabLedgeBack;
      }
      else
      {
        if (this.PlayerManager.Action.FacesBack())
          this.PlayerManager.LookingDirection = Math.Sign(distance) > 0 ? HorizontalDirection.Left : HorizontalDirection.Right;
        else
          this.PlayerManager.LookingDirection = Math.Sign(distance) > 0 ? HorizontalDirection.Right : HorizontalDirection.Left;
        this.PlayerManager.Action = ActionType.GrabCornerLedge;
        this.PlayerManager.Position += this.PlayerManager.Size.Z / 4f * this.rotatedFrom.Value.ForwardVector();
        this.PlayerManager.Background = false;
      }
      this.SyncAnimation(true);
      this.rotatedFrom = new Viewpoint?();
    }
    base.Update(gameTime);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * 0.85f;
    if (this.PlayerManager.HeldInstance.PhysicsState != null && this.CameraManager.ActionRunning)
      this.PlayerManager.Position += this.PlayerManager.HeldInstance.PhysicsState.Velocity;
    return true;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.GrabLedgeFront || type == ActionType.GrabLedgeBack;
  }

  protected override bool ViewTransitionIndependent => true;
}
