// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Crush
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Crush(Game game) : PlayerAction(game)
{
  private const float Duration = 1.75f;
  private float AccumlatedTime;
  private Vector3 crushPosition;

  protected override void Begin()
  {
    this.PlayerManager.Velocity = Vector3.Zero;
    this.crushPosition = this.PlayerManager.Position;
    this.AccumlatedTime = 0.0f;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    this.AccumlatedTime += (float) elapsed.TotalSeconds;
    this.PlayerManager.Position = this.crushPosition;
    this.PlayerManager.Animation.Timing.Update(elapsed, 2f);
    if ((double) this.AccumlatedTime > 1.75 * (this.PlayerManager.Action == ActionType.CrushHorizontal ? 1.2000000476837158 : 1.0))
      this.PlayerManager.Respawn();
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.CrushVertical || type == ActionType.CrushHorizontal;
  }
}
