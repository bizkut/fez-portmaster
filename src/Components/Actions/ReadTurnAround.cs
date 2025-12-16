// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ReadTurnAround
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

internal class ReadTurnAround(Game game) : PlayerAction(game)
{
  private SoundEffect sTurnAway;
  private SoundEffect sTurnBack;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sTurnAway = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TurnAway");
    this.sTurnBack = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TurnBack");
  }

  protected override void Begin()
  {
    base.Begin();
    this.sTurnAway.EmitAt(this.PlayerManager.Position);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.CanControl && this.PlayerManager.Action == ActionType.ReadTurnAround)
    {
      this.PlayerManager.Animation.Timing.Restart();
      this.PlayerManager.Action = ActionType.EndReadTurnAround;
      this.sTurnBack.EmitAt(this.PlayerManager.Position);
    }
    if (this.PlayerManager.Action == ActionType.EndReadTurnAround && this.PlayerManager.Animation.Timing.Ended)
      this.PlayerManager.Action = ActionType.Idle;
    return true;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.ReadTurnAround || type == ActionType.EndReadTurnAround;
  }
}
