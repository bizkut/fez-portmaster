// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.PlayingDrums
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class PlayingDrums(Game game) : PlayerAction(game)
{
  protected override bool Act(TimeSpan elapsed)
  {
    return this.PlayerManager.Action == ActionType.DrumsIdle;
  }

  protected override bool IsActionAllowed(ActionType type) => type.IsPlayingDrums();
}
