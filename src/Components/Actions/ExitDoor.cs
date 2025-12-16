// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ExitDoor
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class ExitDoor(Game game) : PlayerAction(game)
{
  private SoundEffect sound;

  protected override void LoadContent()
  {
    this.sound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/ExitDoor");
  }

  protected override void Begin()
  {
    base.Begin();
    this.sound.EmitAt(this.PlayerManager.Position);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (!this.PlayerManager.Animation.Timing.Ended)
      return true;
    this.PlayerManager.Action = ActionType.Idle;
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.ExitDoor || type == ActionType.ExitDoorCarry || type == ActionType.ExitDoorCarryHeavy;
  }
}
