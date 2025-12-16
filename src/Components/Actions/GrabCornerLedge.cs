// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.GrabCornerLedge
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

public class GrabCornerLedge(Game game) : PlayerAction(game)
{
  private const float VelocityThreshold = 0.025f;
  private const float MovementThreshold = 0.1f;
  private const float DistanceThreshold = 0.35f;
  private Viewpoint? rotatedFrom;
  private Vector3 huggedCorner;
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
        HorizontalDirection lookingDirection = this.PlayerManager.LookingDirection;
        if (lookingDirection == HorizontalDirection.None)
          break;
        Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
        Vector3 b = this.CameraManager.Viewpoint.RightVector() * (float) lookingDirection.Sign();
        double num1 = (double) this.PlayerManager.Velocity.Dot(b);
        float num2 = this.InputManager.Movement.X * (float) lookingDirection.Sign();
        if (num1 <= 0.02500000037252903 && (double) num2 <= 0.10000000149011612)
          break;
        MultipleHits<CollisionResult> wallCollision = this.PlayerManager.WallCollision;
        FaceOrientation visibleOrientation = this.CameraManager.VisibleOrientation;
        TrileInstance trileInstance1 = wallCollision.NearLow.Destination ?? this.PlayerManager.CornerCollision[1 + (lookingDirection == HorizontalDirection.Left ? 2 : 0)].Instances.Deep;
        TrileInstance trileInstance2 = wallCollision.FarHigh.Destination ?? this.PlayerManager.CornerCollision[lookingDirection == HorizontalDirection.Left ? 2 : 0].Instances.Deep;
        Trile trile = trileInstance1 == null ? (Trile) null : trileInstance1.Trile;
        if (trileInstance1 == null || trileInstance1.GetRotatedFace(visibleOrientation) == CollisionType.None || trile.ActorSettings.Type == ActorType.Ladder || trileInstance1 == trileInstance2 || trileInstance2 != null && trileInstance2.GetRotatedFace(visibleOrientation) == CollisionType.AllSides || !trileInstance1.Enabled)
          break;
        TrileInstance trileInstance3 = this.LevelManager.ActualInstanceAt(trileInstance1.Center - b);
        TrileInstance deep = this.LevelManager.NearestTrile(trileInstance1.Center - b).Deep;
        if (deep != null && deep.Enabled && deep.GetRotatedFace(this.CameraManager.VisibleOrientation) != CollisionType.None || trileInstance3 != null && trileInstance3.Enabled && !trileInstance3.Trile.Immaterial)
          break;
        Vector3 vector3_2;
        if (this.PlayerManager.Action == ActionType.Jumping)
        {
          vector3_2 = (trileInstance1.Center - this.PlayerManager.LeaveGroundPosition) * vector3_1;
          if ((double) vector3_2.Length() < 1.25)
            break;
        }
        if (trileInstance1.GetRotatedFace(visibleOrientation) != CollisionType.AllSides && this.CollisionManager.CollideEdge(trileInstance1.Center + trileInstance1.TransformedSize * (Vector3.UnitY * 0.498f + b * 0.5f), Vector3.Down * (float) Math.Sign(this.CollisionManager.GravityFactor), this.PlayerManager.Size * FezMath.XZMask / 2f, Direction2D.Vertical).AnyHit())
          break;
        Vector3 vector3_3 = (-b + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * trileInstance1.TransformedSize / 2f;
        vector3_2 = this.PlayerManager.Center * vector3_1 - (trileInstance1.Center * vector3_1 + vector3_3);
        if ((double) vector3_2.Length() >= 0.34999999403953552)
          break;
        this.PlayerManager.HeldInstance = trileInstance1;
        this.PlayerManager.Action = ActionType.GrabCornerLedge;
        Waiters.Wait(0.1, (Action) (() =>
        {
          if (this.PlayerManager.HeldInstance == null)
            return;
          this.sound.EmitAt(this.PlayerManager.Position);
          this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.LeftLow, 0.10000000149011612, TimeSpan.FromSeconds(0.20000000298023224));
          this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.RightHigh, 0.40000000596046448, TimeSpan.FromSeconds(0.20000000298023224));
        }));
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    this.PlayerManager.Velocity = Vector3.Zero;
    this.PlayerManager.GroundedVelocity = new Vector3?(this.CameraManager.Viewpoint.RightVector() * 0.085f + Vector3.UnitY * 0.15f * (float) Math.Sign(this.CollisionManager.GravityFactor));
    this.InputManager.PressedToDown();
    this.GomezService.OnGrabLedge();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.rotatedFrom.HasValue && (double) this.CameraManager.ViewTransitionStep >= 0.6)
    {
      int distance = this.CameraManager.Viewpoint.GetDistance(this.rotatedFrom.Value);
      if (Math.Abs(distance) % 2 == 0)
      {
        this.PlayerManager.LookingDirection = this.PlayerManager.LookingDirection.GetOpposite();
      }
      else
      {
        if (this.PlayerManager.LookingDirection == HorizontalDirection.Right)
          this.PlayerManager.Action = Math.Sign(distance) > 0 ? ActionType.GrabLedgeBack : ActionType.GrabLedgeFront;
        else
          this.PlayerManager.Action = Math.Sign(distance) > 0 ? ActionType.GrabLedgeFront : ActionType.GrabLedgeBack;
        if (this.PlayerManager.Action == ActionType.GrabLedgeBack)
        {
          this.PlayerManager.Position -= this.PlayerManager.Size.Z / 4f * this.CameraManager.Viewpoint.ForwardVector();
          this.CorrectWallOverlap(true);
          this.PlayerManager.Background = false;
        }
        else
        {
          this.PlayerManager.Position += this.PlayerManager.Size.Z / 4f * this.CameraManager.Viewpoint.ForwardVector();
          this.PlayerManager.Background = true;
        }
      }
      this.SyncAnimation(this.IsActionAllowed(this.PlayerManager.Action));
      this.rotatedFrom = new Viewpoint?();
    }
    if (this.PlayerManager.Action.IsOnLedge())
    {
      if (this.PlayerManager.HeldInstance == null)
        this.PlayerManager.Action = ActionType.Idle;
      else if (this.PlayerManager.HeldInstance.PhysicsState != null && (double) Math.Abs(this.PlayerManager.HeldInstance.PhysicsState.Velocity.Dot(Vector3.One)) > 0.5)
      {
        this.PlayerManager.Velocity = this.PlayerManager.HeldInstance.PhysicsState.Velocity;
        this.PlayerManager.HeldInstance = (TrileInstance) null;
        this.PlayerManager.Action = ActionType.Jumping;
      }
    }
    base.Update(gameTime);
  }

  private void CorrectWallOverlap(bool overcompensate)
  {
    foreach (PointCollision pointCollision in this.PlayerManager.CornerCollision)
    {
      TrileInstance deep = pointCollision.Instances.Deep;
      if (deep != null && deep != this.PlayerManager.CarriedInstance && deep.GetRotatedFace(this.CameraManager.VisibleOrientation) == CollisionType.AllSides)
      {
        Vector3 vector = (pointCollision.Point - (deep.Center + (this.PlayerManager.Position - pointCollision.Point).Sign() * deep.TransformedSize / 2f)) * this.CameraManager.Viewpoint.SideMask();
        this.PlayerManager.Position -= vector;
        if (overcompensate)
          this.PlayerManager.Position -= vector.Sign() * (1f / 1000f) * 2f;
        if (!(this.PlayerManager.Velocity.Sign() == vector.Sign()))
          break;
        Vector3 vector3 = vector.Sign().Abs();
        this.PlayerManager.Position -= this.PlayerManager.Velocity * vector3;
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity * (Vector3.One - vector3);
        break;
      }
    }
  }

  protected override bool Act(TimeSpan elapsed)
  {
    NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.PlayerManager.HeldInstance.Center);
    CollisionType collisionType = CollisionType.None;
    bool flag = false;
    if (nearestTriles.Deep != null)
    {
      collisionType = nearestTriles.Deep.GetRotatedFace(this.CameraManager.Viewpoint.VisibleOrientation());
      flag |= collisionType == CollisionType.AllSides;
    }
    if (flag && (this.InputManager.RotateLeft == FezButtonState.Pressed || this.InputManager.RotateRight == FezButtonState.Pressed))
      this.InputManager.PressedToDown();
    if (nearestTriles.Deep == null)
      flag = true;
    if (nearestTriles.Deep != null)
      flag |= collisionType == CollisionType.TopNoStraightLedge;
    FezButtonState fezButtonState = this.PlayerManager.Animation.Timing.Ended ? FezButtonState.Down : FezButtonState.Pressed;
    if (this.CameraManager.ActionRunning && !flag && (this.PlayerManager.LookingDirection == HorizontalDirection.Right && this.InputManager.Right == fezButtonState || this.PlayerManager.LookingDirection == HorizontalDirection.Left && this.InputManager.Left == fezButtonState))
    {
      bool background = this.PlayerManager.Background;
      Vector3 position = this.PlayerManager.Position;
      this.PlayerManager.Position += this.CameraManager.Viewpoint.RightVector() * (float) -this.PlayerManager.LookingDirection.Sign() * 0.5f;
      this.PhysicsManager.DetermineInBackground((IPhysicsEntity) this.PlayerManager, true, false, false);
      int num = this.PlayerManager.Background ? 1 : 0;
      this.PlayerManager.Background = background;
      this.PlayerManager.Position = position;
      if (num == 0)
      {
        FaceOrientation face = this.CameraManager.Viewpoint.VisibleOrientation();
        TrileInstance deep1 = this.PlayerManager.AxisCollision[VerticalDirection.Up].Deep;
        TrileInstance deep2 = this.PlayerManager.AxisCollision[VerticalDirection.Down].Deep;
        if ((deep1 == null || deep1.GetRotatedFace(face) != CollisionType.AllSides) && deep2 != null && deep2.GetRotatedFace(face) == CollisionType.TopOnly && !this.CollisionManager.CollideEdge(deep2.Center, Vector3.Down * (float) Math.Sign(this.CollisionManager.GravityFactor), this.PlayerManager.Size * FezMath.XZMask / 2f, Direction2D.Vertical).AnyHit())
        {
          TrileInstance surface = this.PlayerManager.AxisCollision[VerticalDirection.Down].Surface;
          if ((surface == null || !surface.Trile.ActorSettings.Type.IsClimbable()) && deep2.Enabled)
            this.PlayerManager.Action = ActionType.FromCornerBack;
        }
      }
    }
    this.huggedCorner = (-(!this.rotatedFrom.HasValue ? this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign() : this.rotatedFrom.Value.RightVector() * (float) this.PlayerManager.LookingDirection.Sign()) + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f;
    this.PlayerManager.Position = this.PlayerManager.HeldInstance.Center + this.huggedCorner;
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.GrabCornerLedge;

  protected override bool ViewTransitionIndependent => true;
}
