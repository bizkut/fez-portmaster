// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Idle
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Idle(Game game) : PlayerAction(game)
{
  private TimeSpan changeAnimationIn;
  private ActionType lastSpecialIdle;
  private SoundEmitter lastSpecialIdleSound;
  private int lastFrame = -1;
  private SoundEffect sBlink;
  private SoundEffect sYawn;
  private SoundEffect sHatGrab;
  private SoundEffect sHatThrow;
  private SoundEffect sHatCatch;
  private SoundEffect sHatFinalThrow;
  private SoundEffect sHatFallOnHead;
  private SoundEffect sLayDown;
  private SoundEffect sSnore;
  private SoundEffect sWakeUp;
  private SoundEffect sIdleTurnLeft;
  private SoundEffect sIdleTurnRight;
  private SoundEffect sIdleTurnUp;
  private SoundEffect sIdleFaceFront;

  public override void Initialize()
  {
    base.Initialize();
    this.CameraManager.ViewpointChanged += (Action) (() =>
    {
      if (this.PlayerManager.Action != ActionType.Teetering)
        return;
      this.PlayerManager.Action = ActionType.Idle;
    });
    this.sBlink = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Blink");
    this.sYawn = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Yawn");
    this.sHatGrab = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/HatGrab");
    this.sHatThrow = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/HatThrow");
    this.sHatCatch = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/HatCatch");
    this.sHatFinalThrow = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/HatFinalThrow");
    this.sHatFallOnHead = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/HatFallOnHead");
    this.sLayDown = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LayDown");
    this.sSnore = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Snore");
    this.sWakeUp = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/WakeUp");
    this.sIdleTurnLeft = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/IdleTurnLeft");
    this.sIdleTurnRight = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/IdleTurnRight");
    this.sIdleTurnUp = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/IdleTurnUp");
    this.sIdleFaceFront = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/IdleFaceFront");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.Dropping:
      case ActionType.Sliding:
      case ActionType.Grabbing:
      case ActionType.Pushing:
        bool flag = (double) this.CollisionManager.GravityFactor < 0.0;
        if (!FezMath.AlmostEqual(this.PlayerManager.Velocity.XZ(), Vector2.Zero) || (double) this.InputManager.Movement.X != 0.0 || this.PlayerManager.PushedInstance != null || (flag ? ((double) this.PlayerManager.Velocity.Y >= 0.0 ? 1 : 0) : ((double) this.PlayerManager.Velocity.Y <= 0.0 ? 1 : 0)) == 0)
          break;
        this.PlayerManager.Action = ActionType.Idle;
        break;
    }
  }

  protected override void Begin()
  {
    this.lastFrame = -1;
    this.ScheduleSpecialIdle();
  }

  private void ScheduleSpecialIdle()
  {
    this.changeAnimationIn = TimeSpan.FromSeconds((double) RandomHelper.Between(7.0, 9.0));
  }

  protected override void End()
  {
    base.End();
    if (this.lastSpecialIdleSound == null || this.lastSpecialIdleSound.Dead)
      return;
    this.lastSpecialIdleSound.FadeOutAndDie(0.1f);
    this.lastSpecialIdleSound = (SoundEmitter) null;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    int num = this.PlayerManager.Animation.Timing.Frame;
    switch (this.PlayerManager.Action)
    {
      case ActionType.IdlePlay:
        if (this.lastFrame != num)
        {
          if (num == 2)
            this.lastSpecialIdleSound = this.sHatGrab.EmitAt(this.PlayerManager.Position);
          if (num == 6 || num == 13 || num == 20)
            this.lastSpecialIdleSound = this.sHatThrow.EmitAt(this.PlayerManager.Position);
          if (num == 10 || num == 17 || num == 24)
            this.lastSpecialIdleSound = this.sHatCatch.EmitAt(this.PlayerManager.Position);
          if (num == 27)
            this.lastSpecialIdleSound = this.sHatFinalThrow.EmitAt(this.PlayerManager.Position);
          if (num == 31 /*0x1F*/)
            this.lastSpecialIdleSound = this.sHatFallOnHead.EmitAt(this.PlayerManager.Position);
        }
        if (this.CheckNextIdle())
        {
          num = -1;
          break;
        }
        break;
      case ActionType.IdleSleep:
        if (this.lastFrame != num)
        {
          if (num == 1)
            this.lastSpecialIdleSound = this.sYawn.EmitAt(this.PlayerManager.Position);
          if (num == 3)
            this.lastSpecialIdleSound = this.sLayDown.EmitAt(this.PlayerManager.Position);
          if (num == 11 || num == 21 || num == 31 /*0x1F*/ || num == 41)
            this.lastSpecialIdleSound = this.sSnore.EmitAt(this.PlayerManager.Position);
          if (num == 50)
            this.lastSpecialIdleSound = this.sWakeUp.EmitAt(this.PlayerManager.Position);
          if (num == 51)
            this.sBlink.EmitAt(this.PlayerManager.Position);
        }
        if (this.CheckNextIdle())
        {
          num = -1;
          break;
        }
        break;
      case ActionType.IdleLookAround:
        if (this.lastFrame != num)
        {
          if (num == 1)
            this.lastSpecialIdleSound = this.sIdleTurnLeft.EmitAt(this.PlayerManager.Position);
          if (num == 7)
            this.lastSpecialIdleSound = this.sIdleTurnRight.EmitAt(this.PlayerManager.Position);
          if (num == 13)
            this.lastSpecialIdleSound = this.sIdleTurnUp.EmitAt(this.PlayerManager.Position);
          if (num == 19)
            this.lastSpecialIdleSound = this.sIdleFaceFront.EmitAt(this.PlayerManager.Position);
        }
        if (this.CheckNextIdle())
        {
          num = -1;
          break;
        }
        break;
      case ActionType.IdleYawn:
        if (this.lastFrame != num && num == 0)
          this.lastSpecialIdleSound = this.sYawn.EmitAt(this.PlayerManager.Position);
        if (this.CheckNextIdle())
        {
          num = -1;
          break;
        }
        break;
      default:
        if (this.PlayerManager.CanControl)
          this.changeAnimationIn -= elapsed;
        if (!this.GameState.TimePaused && !this.PlayerManager.Hidden && !this.GameState.FarawaySettings.InTransition && !this.PlayerManager.InDoorTransition && this.lastFrame != num && (num == 1 || num == 13))
          this.sBlink.EmitAt(this.PlayerManager.Position);
        if (this.changeAnimationIn.Ticks <= 0L)
        {
          switch (this.lastSpecialIdle)
          {
            case ActionType.None:
            case ActionType.IdlePlay:
              this.PlayerManager.Action = ActionType.IdleYawn;
              break;
            case ActionType.IdleSleep:
              this.PlayerManager.Action = ActionType.IdleLookAround;
              break;
            case ActionType.IdleLookAround:
              if (this.PlayerManager.HideFez)
              {
                this.PlayerManager.Action = ActionType.IdleYawn;
                break;
              }
              this.PlayerManager.Action = ActionType.IdlePlay;
              break;
            case ActionType.IdleYawn:
              this.PlayerManager.Action = ActionType.IdleSleep;
              break;
          }
          this.lastSpecialIdle = this.PlayerManager.Action;
          num = -1;
          break;
        }
        break;
    }
    this.lastFrame = num;
    return true;
  }

  private bool CheckNextIdle()
  {
    if (!this.PlayerManager.Animation.Timing.Ended)
      return false;
    this.ScheduleSpecialIdle();
    this.PlayerManager.Action = ActionType.Idle;
    return true;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.Idle || type == ActionType.IdleSleep || type == ActionType.IdlePlay || type == ActionType.IdleLookAround || type == ActionType.IdleYawn;
  }
}
