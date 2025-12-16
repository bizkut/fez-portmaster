// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.LookAround
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class LookAround(Game game) : PlayerAction(game)
{
  private ActionType nextAction;
  private SoundEffect rightSound;
  private SoundEffect leftSound;
  private SoundEffect upSound;
  private SoundEffect downSound;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.rightSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LookRight");
    this.leftSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LookLeft");
    this.upSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LookUp");
    this.downSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LookDown");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.LookingLeft:
      case ActionType.LookingRight:
      case ActionType.LookingUp:
      case ActionType.LookingDown:
      case ActionType.Teetering:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (this.PlayerManager.CanControl)
        {
          Vector2 vector2 = this.InputManager.FreeLook;
          if (this.MouseState.LeftButton.State == MouseButtonStates.Dragging)
            vector2 = -vector2;
          if ((double) vector2.Y < -0.4)
            this.nextAction = ActionType.LookingDown;
          else if ((double) vector2.Y > 0.4)
            this.nextAction = ActionType.LookingUp;
          else if ((double) vector2.X < -0.4)
            this.nextAction = ActionType.LookingLeft;
          else if ((double) vector2.X > 0.4)
            this.nextAction = ActionType.LookingRight;
          else if (FezMath.AlmostEqual(this.InputManager.FreeLook, Vector2.Zero))
            this.nextAction = ActionType.Idle;
        }
        else
          this.nextAction = this.PlayerManager.Action.IsLookingAround() ? this.PlayerManager.Action : ActionType.Idle;
        if (this.PlayerManager.LookingDirection == HorizontalDirection.Left && (this.nextAction == ActionType.LookingLeft || this.nextAction == ActionType.LookingRight))
          this.nextAction = this.nextAction == ActionType.LookingRight ? ActionType.LookingLeft : ActionType.LookingRight;
        if (this.PlayerManager.Action.IsIdle() && this.nextAction != ActionType.None && this.nextAction != ActionType.Idle)
        {
          this.PlaySound();
          this.PlayerManager.Action = this.nextAction;
          this.nextAction = ActionType.None;
        }
        if (this.nextAction != this.PlayerManager.Action)
          break;
        this.nextAction = ActionType.None;
        break;
      default:
        this.nextAction = ActionType.None;
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    if (!this.PlayerManager.CanControl)
      return;
    this.GomezService.OnLookAround();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if ((double) this.PlayerManager.Animation.Timing.NormalizedStep <= 0.55)
      this.PlayerManager.Animation.Timing.Update(elapsed);
    else if (this.nextAction != ActionType.None)
      this.PlayerManager.Animation.Timing.Update(elapsed, 1.25f);
    if (this.PlayerManager.Animation.Timing.Ended && this.nextAction != ActionType.None)
    {
      this.PlaySound();
      this.PlayerManager.Action = this.nextAction;
      this.nextAction = ActionType.None;
    }
    return false;
  }

  private void PlaySound()
  {
    switch (this.nextAction)
    {
      case ActionType.LookingLeft:
        this.leftSound.EmitAt(this.PlayerManager.Position);
        break;
      case ActionType.LookingRight:
        this.rightSound.EmitAt(this.PlayerManager.Position);
        break;
      case ActionType.LookingUp:
        this.upSound.EmitAt(this.PlayerManager.Position);
        break;
      case ActionType.LookingDown:
        this.downSound.EmitAt(this.PlayerManager.Position);
        break;
    }
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.LookingDown || type == ActionType.LookingLeft || type == ActionType.LookingRight || type == ActionType.LookingUp;
  }

  [ServiceDependency]
  public IMouseStateManager MouseState { private get; set; }
}
