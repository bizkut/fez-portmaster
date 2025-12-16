// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.WalkTo
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class WalkTo(Game game) : PlayerAction(game), IWalkToService
{
  private readonly MovementHelper movementHelper = new MovementHelper(4.7f, 5.875f, 0.2f);
  private HorizontalDirection originalLookingDirection;
  private bool stoppedByWall;

  protected override void Begin()
  {
    this.originalLookingDirection = this.PlayerManager.LookingDirection;
    this.movementHelper.Entity = (IPhysicsEntity) this.PlayerManager;
    this.stoppedByWall = false;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    float timeFactor = this.movementHelper.Running ? 1.25f : 1f;
    this.PlayerManager.Animation.Timing.Update(elapsed, timeFactor);
    this.PlayerManager.LookingDirection = this.originalLookingDirection;
    float a = (this.Destination() - this.PlayerManager.Position).Dot(this.CameraManager.Viewpoint.RightVector());
    int num1 = (double) a < 0.0 ? -1 : 1;
    this.PlayerManager.LookingDirection = (double) a < 0.0 ? HorizontalDirection.Left : HorizontalDirection.Right;
    this.stoppedByWall = this.PlayerManager.WallCollision.AnyCollided();
    if (FezMath.AlmostEqual((double) a, 0.0, 0.01) || this.stoppedByWall)
    {
      this.ChangeAction();
    }
    else
    {
      this.movementHelper.Update((float) elapsed.TotalSeconds, (float) num1 * 0.75f);
      float num2 = this.PlayerManager.Velocity.Dot(this.CameraManager.Viewpoint.SideMask());
      this.PlayerManager.Velocity = this.PlayerManager.Velocity * (Vector3.UnitY + this.CameraManager.Viewpoint.DepthMask()) + this.CameraManager.Viewpoint.RightVector() * Math.Min(Math.Abs(num2), Math.Abs(a)) * (float) num1;
    }
    return false;
  }

  private void ChangeAction()
  {
    this.PlayerManager.LookingDirection = this.originalLookingDirection;
    this.PlayerManager.Action = this.NextAction;
    if (!this.stoppedByWall)
    {
      this.PlayerManager.Position = this.Destination();
      this.PhysicsManager.HugWalls((IPhysicsEntity) this.PlayerManager, false, false, true);
    }
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
    this.Destination = (Func<Vector3>) null;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.WalkingTo;

  public Func<Vector3> Destination { get; set; }

  public ActionType NextAction { get; set; }
}
