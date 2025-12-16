// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Jetpack
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Jetpack(Game game) : PlayerAction(game)
{
  private const float JetpackSpeed = 0.075f;

  protected override void TestConditions()
  {
    if (!this.GameState.JetpackMode && !this.GameState.DebugMode || this.InputManager.Jump != FezButtonState.Down || this.PlayerManager.Action == ActionType.FindingTreasure || this.PlayerManager.Action == ActionType.Dying || this.PlayerManager.Action == ActionType.OpeningTreasure || this.PlayerManager.Action == ActionType.Suffering || !(this.LevelManager.Name != "VILLAGEVILLE_2D") || !(this.LevelManager.Name != "ELDERS") || !(this.LevelManager.Name != "VILLAGEVILLE_3D_END_64") || this.PlayerManager.Action == ActionType.LesserWarp || this.PlayerManager.Action == ActionType.GateWarp)
      return;
    this.PlayerManager.CarriedInstance = (TrileInstance) null;
    this.PlayerManager.Action = ActionType.Flying;
  }

  protected override void Begin()
  {
    base.Begin();
    this.PlayerManager.CarriedInstance = this.PlayerManager.PushedInstance = (TrileInstance) null;
    this.CameraManager.Constrained = false;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.InputManager.Jump == FezButtonState.Down)
    {
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity + (float) (0.15000000596046448 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 1.0249999761581421) * Vector3.UnitY * 0.075f;
      this.PlayerManager.LeaveGroundPosition = new Vector3(this.PlayerManager.LeaveGroundPosition.X, this.PlayerManager.Position.Y, this.PlayerManager.LeaveGroundPosition.Z);
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Flying;
}
