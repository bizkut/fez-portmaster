// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.LowerToCornerLedge
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class LowerToCornerLedge : PlayerAction
{
  private Vector3 camOrigin;
  private Vector3 playerOrigin;
  private SoundEffect sound;
  private SoundEffect sLowerToLedge;

  public LowerToCornerLedge(Game game)
    : base(game)
  {
    this.UpdateOrder = 3;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeGrab");
    this.sLowerToLedge = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LowerToLedge");
  }

  protected override void TestConditions()
  {
    if (this.PlayerManager.Background)
      return;
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
      case ActionType.Teetering:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (!this.PlayerManager.Grounded || this.InputManager.Down != FezButtonState.Pressed)
          break;
        TrileInstance nearLow = this.PlayerManager.Ground.NearLow;
        TrileInstance farHigh = this.PlayerManager.Ground.FarHigh;
        Trile trile = nearLow == null ? (Trile) null : nearLow.Trile;
        Vector3 vector3 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
        TrileInstance trileInstance1 = this.PlayerManager.Ground.NearLow ?? this.PlayerManager.Ground.FarHigh;
        if (this.CollisionManager.CollideEdge(trileInstance1.Center + trileInstance1.TransformedSize * (Vector3.UnitY * 0.498f + vector3 * 0.5f), Vector3.Down * (float) Math.Sign(this.CollisionManager.GravityFactor), this.PlayerManager.Size * FezMath.XZMask / 2f, Direction2D.Vertical).AnyHit() || nearLow == null || nearLow.GetRotatedFace(this.CameraManager.VisibleOrientation) == CollisionType.None || trile.ActorSettings.Type == ActorType.Ladder || nearLow == farHigh || farHigh != null && farHigh.GetRotatedFace(this.CameraManager.VisibleOrientation) != CollisionType.None)
          break;
        TrileInstance trileInstance2 = this.LevelManager.ActualInstanceAt(nearLow.Position + vector3 + new Vector3(0.5f));
        TrileInstance deep = this.LevelManager.NearestTrile(nearLow.Position + vector3 + new Vector3(0.5f)).Deep;
        if (deep != null && deep.Enabled && deep.GetRotatedFace(this.CameraManager.VisibleOrientation) != CollisionType.None || trileInstance2 != null && trileInstance2.Enabled && !trileInstance2.Trile.Immaterial && trileInstance2.Trile.ActorSettings.Type != ActorType.Vine)
          break;
        this.WalkTo.Destination = new Func<Vector3>(this.GetDestination);
        this.WalkTo.NextAction = ActionType.LowerToCornerLedge;
        this.PlayerManager.Action = ActionType.WalkingTo;
        this.PlayerManager.HeldInstance = nearLow;
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    this.PlayerManager.Velocity = Vector3.Zero;
    this.camOrigin = this.CameraManager.Center;
    this.sLowerToLedge.EmitAt(this.PlayerManager.Position);
    Waiters.Wait(0.57999998331069946, (Action) (() => this.sound.EmitAt(this.PlayerManager.Position)));
  }

  private Vector3 GetDestination()
  {
    Vector3 vector3 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    if (this.PlayerManager.Action != ActionType.LowerToCornerLedge)
      return this.PlayerManager.HeldInstance.Center + (vector3 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f + -5f / 16f * vector3;
    this.playerOrigin = this.PlayerManager.Position;
    Vector3 destination = this.PlayerManager.HeldInstance.Center + (vector3 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f;
    this.PlayerManager.SplitUpCubeCollectorOffset = this.playerOrigin - destination;
    return destination;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Action != ActionType.LowerToCornerLedge)
      return false;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    float num = (float) (4.0 * (this.LevelManager.Descending ? -1.0 : 1.0)) / this.CameraManager.PixelsPerTrixel;
    Vector3 vector3_2 = this.PlayerManager.HeldInstance.Center + ((vector3_1 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f + num * Vector3.UnitY);
    if (!this.CameraManager.StickyCam && !this.CameraManager.Constrained && !this.CameraManager.PanningConstraints.HasValue)
      this.CameraManager.Center = Vector3.Lerp(this.camOrigin, vector3_2, this.PlayerManager.Animation.Timing.NormalizedStep);
    this.PlayerManager.Position = this.PlayerManager.HeldInstance.Center + (vector3_1 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f;
    this.PlayerManager.SplitUpCubeCollectorOffset = (this.playerOrigin - this.PlayerManager.Position) * (1f - this.PlayerManager.Animation.Timing.NormalizedStep);
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      this.PlayerManager.LookingDirection = this.PlayerManager.LookingDirection.GetOpposite();
      this.PlayerManager.SplitUpCubeCollectorOffset = Vector3.Zero;
      this.PlayerManager.Action = ActionType.GrabCornerLedge;
    }
    this.PlayerManager.Animation.Timing.Update(elapsed, 1.25f);
    return false;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.LowerToCornerLedge;
}
