// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.BellActions
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class BellActions(Game game) : PlayerAction(game)
{
  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Action == ActionType.TurnToBell && this.PlayerManager.Animation.Timing.Ended)
    {
      this.PlayerManager.Action = ActionType.HitBell;
      this.PlayerManager.Animation.Timing.Restart();
    }
    if (this.PlayerManager.Action == ActionType.HitBell && this.PlayerManager.Animation.Timing.Ended)
    {
      this.PlayerManager.Action = ActionType.TurnAwayFromBell;
      this.PlayerManager.Animation.Timing.Restart();
    }
    if (this.PlayerManager.Action == ActionType.TurnAwayFromBell && this.PlayerManager.Animation.Timing.Ended)
    {
      this.PlayerManager.Action = ActionType.Idle;
      this.PlayerManager.Animation.Timing.Restart();
    }
    this.PlayerManager.Background = false;
    return true;
  }

  protected override bool ViewTransitionIndependent => true;

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.TurnAwayFromBell || type == ActionType.HitBell || type == ActionType.TurnToBell;
  }
}
