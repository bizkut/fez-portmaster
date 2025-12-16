// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Victory
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Victory(Game game) : PlayerAction(game)
{
  private static readonly TimeSpan HappyTime = TimeSpan.FromSeconds(2.0);
  private TimeSpan sinceActive;

  protected override void Begin()
  {
    this.sinceActive = TimeSpan.Zero;
    this.PlayerManager.Velocity = new Vector3(0.0f, 0.05f, 0.0f);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Action != ActionType.VictoryForever)
    {
      this.sinceActive += elapsed;
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity * 0.95f;
      if (this.sinceActive.Ticks >= Victory.HappyTime.Ticks)
        this.PlayerManager.Action = ActionType.Idle;
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.Victory || type == ActionType.VictoryForever;
  }
}
