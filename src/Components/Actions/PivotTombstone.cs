// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.PivotTombstone
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class PivotTombstone(Game game) : PlayerAction(game)
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
    if (this.PlayerManager.Action == ActionType.GrabTombstone && this.PlayerManager.Animation.Timing.Ended && !this.InputManager.GrabThrow.IsDown())
    {
      this.PlayerManager.Action = ActionType.LetGoOfTombstone;
      this.PlayerManager.Animation.Timing.Restart();
      this.sTurnBack.EmitAt(this.PlayerManager.Position);
    }
    if (this.PlayerManager.Action == ActionType.LetGoOfTombstone && this.PlayerManager.Animation.Timing.Ended)
      this.PlayerManager.Action = ActionType.Idle;
    this.PlayerManager.Background = false;
    return true;
  }

  protected override bool ViewTransitionIndependent => true;

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.LetGoOfTombstone || type == ActionType.PivotTombstone || type == ActionType.GrabTombstone;
  }
}
