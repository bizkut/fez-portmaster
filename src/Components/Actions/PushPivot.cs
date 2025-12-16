// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.PushPivot
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

internal class PushPivot(Game game) : PlayerAction(game)
{
  private SoundEffect sTurnAway;
  private SoundEffect sTurnBack;
  private SoundEffect sFallOnFace;
  private SoundEmitter eTurnAway;
  private SoundEmitter eTurnBack;
  private TimeSpan sinceStarted;
  private bool reverse;
  private int lastFrame;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sTurnAway = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TurnAway");
    this.sTurnBack = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TurnBack");
    this.sFallOnFace = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Land");
  }

  protected override void Begin()
  {
    this.sinceStarted = TimeSpan.Zero;
    this.eTurnAway = this.sTurnAway.EmitAt(this.PlayerManager.Position);
    this.reverse = false;
    this.lastFrame = -1;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    int frame = this.PlayerManager.Animation.Timing.Frame;
    this.sinceStarted += elapsed;
    if (this.sinceStarted.TotalSeconds < 0.25 && !this.InputManager.GrabThrow.IsDown())
    {
      this.eTurnAway.FadeOutAndDie(0.1f);
      this.reverse = true;
    }
    if (this.reverse)
    {
      this.PlayerManager.Animation.Timing.Update(elapsed, -1f);
      if ((double) this.PlayerManager.Animation.Timing.Step <= 0.0)
        this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    if (this.PlayerManager.Animation.Timing.Frame == 32 /*0x20*/ && (this.eTurnBack == null || this.eTurnBack.Dead))
      this.eTurnBack = this.sTurnBack.EmitAt(this.PlayerManager.Position);
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    if (frame != this.lastFrame && frame == 18)
      this.sFallOnFace.EmitAt(this.PlayerManager.Position);
    this.lastFrame = frame;
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.PushingPivot;
}
