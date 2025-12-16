// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Grab
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Grab(Game game) : PlayerAction(game)
{
  private int pushingDirectionSign;
  private SoundEffect grabSound;
  private int lastFrame;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.grabSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/GrabPickup");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.Sliding:
      case ActionType.Teetering:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (!this.PlayerManager.Grounded || this.PlayerManager.Background || (double) this.InputManager.Movement.X == 0.0 || this.PlayerManager.LookingDirection == HorizontalDirection.None)
          break;
        TrileInstance destination = this.PlayerManager.WallCollision.NearLow.Destination;
        if (destination == null || !destination.Trile.ActorSettings.Type.IsPickable() || destination.GetRotatedFace(this.CameraManager.VisibleOrientation) != CollisionType.AllSides || destination.Hidden || destination.PhysicsState == null || !destination.PhysicsState.Grounded)
          break;
        NearestTriles nearestTriles = this.LevelManager.NearestTrile(destination.Position);
        if (nearestTriles.Surface != null && nearestTriles.Surface.Trile.ForceHugging || (double) Math.Abs(destination.Center.Y - this.PlayerManager.Position.Y) > 0.5 || destination.Trile.ActorSettings.Type == ActorType.Couch && FezMath.OrientationFromPhi(destination.Trile.ActorSettings.Face.ToPhi() + destination.Phi) != this.CameraManager.Viewpoint.GetRotatedView(this.PlayerManager.LookingDirection == HorizontalDirection.Right ? -1 : 1).VisibleOrientation())
          break;
        this.PlayerManager.Action = ActionType.Grabbing;
        this.PlayerManager.PushedInstance = destination;
        break;
    }
  }

  protected override void Begin()
  {
    this.pushingDirectionSign = this.PlayerManager.LookingDirection.Sign();
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.PushedInstance == null || this.PlayerManager.PushedInstance.Hidden || this.PlayerManager.PushedInstance.PhysicsState == null)
    {
      this.PlayerManager.Action = ActionType.Idle;
      this.PlayerManager.PushedInstance = (TrileInstance) null;
      return false;
    }
    int frame = this.PlayerManager.Animation.Timing.Frame;
    if (this.lastFrame != frame && this.PlayerManager.LastAction != ActionType.Pushing && this.PlayerManager.Action == ActionType.Grabbing)
    {
      if (frame == 3)
        this.grabSound.EmitAt(this.PlayerManager.Position);
      this.lastFrame = frame;
    }
    Vector3 vector3_1 = this.CameraManager.Viewpoint.SideMask();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.DepthMask();
    Vector3 vector3_3 = this.CameraManager.Viewpoint.RightVector() * (float) this.pushingDirectionSign;
    this.PlayerManager.Center = Vector3.Up * this.PlayerManager.Center + (vector3_2 + vector3_1) * this.PlayerManager.PushedInstance.Center + -vector3_3 * (this.PlayerManager.PushedInstance.TransformedSize / 2f + this.PlayerManager.Size / 2f);
    if ((this.PlayerManager.Action == ActionType.Pushing || this.PlayerManager.Action == ActionType.Grabbing) && (this.pushingDirectionSign == -Math.Sign(this.InputManager.Movement.X) || !this.PlayerManager.Grounded))
    {
      this.PlayerManager.PushedInstance = (TrileInstance) null;
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    if (this.PlayerManager.Action != ActionType.Grabbing || !this.PlayerManager.Animation.Timing.Ended || (double) this.InputManager.Movement.X == 0.0)
      return this.PlayerManager.Action == ActionType.Grabbing;
    this.PlayerManager.Action = ActionType.Pushing;
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.Grabbing || type == ActionType.Pushing;
  }
}
