// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Land
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

public class Land(Game game) : PlayerAction(game)
{
  private SoundEffect landSound;

  protected override void TestConditions()
  {
    if (this.PlayerManager.Action != ActionType.Falling || !this.PlayerManager.Grounded)
      return;
    this.PlayerManager.Action = ActionType.Landing;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.landSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Land");
  }

  protected override void Begin()
  {
    base.Begin();
    this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.RightHigh, 0.40000000596046448, TimeSpan.FromSeconds(0.15000000596046448));
    this.landSound.EmitAt(this.PlayerManager.Position);
    this.GomezService.OnLand();
    this.GameState.JetpackMode = false;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Animation.Timing.Ended)
      this.PlayerManager.Action = ActionType.Idle;
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Landing;
}
