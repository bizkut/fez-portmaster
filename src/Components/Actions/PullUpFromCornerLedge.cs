// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.PullUpFromCornerLedge
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

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

public class PullUpFromCornerLedge(Game game) : PlayerAction(game)
{
  private Vector3 camOrigin;
  private Vector3 gomezDelta;
  private SoundEffect pullSound;
  private SoundEffect landSound;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.pullSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/CornerLedgeHoist");
    this.landSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeLand");
  }

  protected override void TestConditions()
  {
    if (this.PlayerManager.Action != ActionType.GrabCornerLedge || (this.InputManager.Jump != FezButtonState.Pressed || (double) this.InputManager.Movement.X == (double) -this.PlayerManager.LookingDirection.Sign() || this.InputManager.Down.IsDown()) && this.InputManager.Up != FezButtonState.Pressed && (this.InputManager.Up != FezButtonState.Down || !this.PlayerManager.Animation.Timing.Ended || this.PlayerManager.LastAction == ActionType.SideClimbingVine) || this.LevelManager.NearestTrile(this.PlayerManager.HeldInstance.Center).Deep == null)
      return;
    this.PlayerManager.Action = ActionType.PullUpCornerLedge;
  }

  protected override void Begin()
  {
    base.Begin();
    this.pullSound.EmitAt(this.PlayerManager.Position);
    Vector3 vector3 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    this.camOrigin = this.CameraManager.Center;
    this.gomezDelta = this.PlayerManager.Size * (vector3 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) / 2f;
    Waiters.Wait(0.57999998331069946, (Action) (() => this.landSound.EmitAt(this.PlayerManager.Position)));
    this.GomezService.OnHoist();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    float num = (float) (4.0 * (this.LevelManager.Descending ? -1.0 : 1.0)) / this.CameraManager.PixelsPerTrixel;
    Vector3 vector3_2 = this.PlayerManager.HeldInstance.Center + ((-vector3_1 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f + this.gomezDelta + num * Vector3.UnitY);
    if (!this.CameraManager.StickyCam && !this.CameraManager.Constrained && !this.CameraManager.PanningConstraints.HasValue)
      this.CameraManager.Center = Vector3.Lerp(this.camOrigin, vector3_2, this.PlayerManager.Animation.Timing.NormalizedStep);
    this.PlayerManager.SplitUpCubeCollectorOffset = this.gomezDelta * this.PlayerManager.Animation.Timing.NormalizedStep;
    this.PlayerManager.Position = this.PlayerManager.HeldInstance.Center + (-vector3_1 + Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor)) * this.PlayerManager.HeldInstance.TransformedSize / 2f;
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      this.PlayerManager.Position += this.gomezDelta;
      this.PlayerManager.HeldInstance = (TrileInstance) null;
      this.PlayerManager.SplitUpCubeCollectorOffset = Vector3.Zero;
      this.PhysicsManager.DetermineInBackground((IPhysicsEntity) this.PlayerManager, !this.PlayerManager.IsOnRotato, true, !this.PlayerManager.Climbing);
      this.PlayerManager.Position += 0.5f * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity - Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
      this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
      this.PlayerManager.Action = ActionType.Idle;
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.PullUpCornerLedge;
}
