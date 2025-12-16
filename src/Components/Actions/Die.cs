// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Die
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Die(Game game) : PlayerAction(game)
{
  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      this.CameraManager.Constrained = false;
      this.PlayerManager.Respawn();
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Dying;
}
