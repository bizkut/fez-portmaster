// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.WalkRun
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class WalkRun(Game game) : PlayerAction(game)
{
  public const float SecondsBeforeRun = 0.2f;
  public const float RunAcceleration = 1.25f;
  public static readonly MovementHelper MovementHelper = new MovementHelper(4.7f, 5.875f, 0.2f);
  private int initialMovement;
  private SoundEffect turnAroundSound;

  public override void Initialize()
  {
    base.Initialize();
    WalkRun.MovementHelper.Entity = (IPhysicsEntity) this.PlayerManager;
    this.turnAroundSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TurnAround");
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
      case ActionType.Sliding:
      case ActionType.Grabbing:
      case ActionType.Pushing:
      case ActionType.Teetering:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (this.PlayerManager.Action == ActionType.Sliding && this.PlayerManager.LastAction == ActionType.Running && this.TestForTurn())
          break;
        if (this.PlayerManager.Grounded && (double) this.InputManager.Movement.X != 0.0 && this.PlayerManager.PushedInstance == null)
        {
          this.PlayerManager.Action = ActionType.Walking;
          break;
        }
        WalkRun.MovementHelper.Reset();
        break;
      case ActionType.Walking:
      case ActionType.Running:
        this.TestForTurn();
        break;
    }
  }

  private bool TestForTurn()
  {
    int num = Math.Sign(this.InputManager.Movement.X);
    if (num == 0 || num == this.PlayerManager.LookingDirection.Sign())
      return false;
    this.initialMovement = num;
    this.PlayerManager.Action = ActionType.RunTurnAround;
    this.turnAroundSound.EmitAt(this.PlayerManager.Position);
    return true;
  }

  protected override void Begin() => WalkRun.MovementHelper.Reset();

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Action == ActionType.RunTurnAround)
    {
      if (Math.Sign(this.InputManager.Movement.X) != this.initialMovement)
      {
        this.PlayerManager.LookingDirection = this.PlayerManager.LookingDirection.GetOpposite();
        this.PlayerManager.Action = ActionType.Idle;
        return false;
      }
      if (this.PlayerManager.Animation.Timing.Ended)
      {
        this.PlayerManager.LookingDirection = this.PlayerManager.LookingDirection.GetOpposite();
        this.PlayerManager.Action = ActionType.Running;
        return false;
      }
      this.PlayerManager.Animation.Timing.Update(elapsed, (float) ((1.0 + (double) Math.Abs(this.CollisionManager.GravityFactor)) / 2.0));
    }
    else if (this.PlayerManager.Action != ActionType.Landing)
    {
      float num1;
      if (WalkRun.MovementHelper.Running)
      {
        int num2 = this.PlayerManager.Action == ActionType.Walking ? 1 : 0;
        this.PlayerManager.Action = ActionType.Running;
        this.SyncAnimation(true);
        if (num2 != 0)
          this.PlayerManager.Animation.Timing.Frame = 1;
        num1 = 1.25f;
      }
      else
      {
        this.PlayerManager.Action = ActionType.Walking;
        num1 = Easing.EaseOut((double) Math.Min(1f, Math.Abs(this.InputManager.Movement.X) * 2f), EasingType.Cubic);
      }
      this.PlayerManager.Animation.Timing.Update(elapsed, (float) ((double) num1 * (1.0 + (double) Math.Abs(this.CollisionManager.GravityFactor)) / 2.0));
    }
    WalkRun.MovementHelper.Update((float) elapsed.TotalSeconds);
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.Running || type == ActionType.Landing || type == ActionType.Walking || type == ActionType.RunTurnAround;
  }
}
