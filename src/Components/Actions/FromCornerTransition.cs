// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.FromCornerTransition
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class FromCornerTransition(Game game) : PlayerAction(game)
{
  private SoundEffect transitionSound;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.transitionSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeToCorner");
  }

  protected override void Begin()
  {
    this.PlayerManager.Velocity = Vector3.Zero;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.ForwardVector();
    Vector3 vector3_3 = this.CameraManager.Viewpoint.DepthMask();
    this.PlayerManager.Position = this.PlayerManager.HeldInstance.Center + (-vector3_1 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f + vector3_2 * -(this.PlayerManager.HeldInstance.TransformedSize / 2f + this.PlayerManager.Size.X * vector3_3 / 4f);
    this.PlayerManager.ForceOverlapsDetermination();
    this.PhysicsManager.HugWalls((IPhysicsEntity) this.PlayerManager, false, false, true);
    this.transitionSound.EmitAt(this.PlayerManager.Position);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.HeldInstance.PhysicsState != null)
      this.PlayerManager.Position += this.PlayerManager.HeldInstance.PhysicsState.Velocity;
    if (!this.PlayerManager.Animation.Timing.Ended)
      return true;
    this.PlayerManager.Action = ActionType.GrabLedgeBack;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.SideMask();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.DepthMask();
    Vector3 vector3_3 = this.CameraManager.Viewpoint.ForwardVector();
    TrileInstance heldInstance = this.PlayerManager.HeldInstance;
    this.PlayerManager.Position = this.PlayerManager.Position * vector3_1 + heldInstance.Center * (Vector3.UnitY + vector3_2) + vector3_3 * (float) -(0.5 + (double) this.PlayerManager.Size.X / 4.0) + heldInstance.TransformedSize * Vector3.UnitY / 2f * (float) Math.Sign(this.CollisionManager.GravityFactor);
    this.PlayerManager.ForceOverlapsDetermination();
    this.PhysicsManager.HugWalls((IPhysicsEntity) this.PlayerManager, false, false, true);
    this.PlayerManager.HeldInstance = this.PlayerManager.AxisCollision[VerticalDirection.Down].Deep;
    return false;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.FromCornerBack;
}
