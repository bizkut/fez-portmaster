// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Bounce
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Bounce(Game game) : PlayerAction(game)
{
  private static readonly TimeSpan BounceVibrateTime = TimeSpan.FromSeconds(0.30000001192092896);
  private const float BouncerResponse = 0.32f;
  private SoundEffect bounceHigh;
  private SoundEffect bounceLow;

  protected override void LoadContent()
  {
    this.bounceHigh = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/BounceHigh");
    this.bounceLow = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/BounceLow");
  }

  protected override void TestConditions()
  {
    if (this.PlayerManager.Action == ActionType.Bouncing && (this.GameState.InCutscene || !this.PlayerManager.CanControl))
    {
      this.PlayerManager.Action = ActionType.Landing;
    }
    else
    {
      switch (this.PlayerManager.Action)
      {
        case ActionType.Idle:
        case ActionType.Walking:
        case ActionType.Running:
        case ActionType.Jumping:
        case ActionType.Falling:
        case ActionType.Dropping:
        case ActionType.Sliding:
        case ActionType.Landing:
        case ActionType.Teetering:
        case ActionType.IdlePlay:
        case ActionType.IdleSleep:
        case ActionType.IdleLookAround:
        case ActionType.IdleYawn:
          if (!this.PlayerManager.Grounded || this.PlayerManager.Ground.First.Trile.ActorSettings.Type != ActorType.Bouncer)
            break;
          this.PlayerManager.Action = ActionType.Bouncing;
          break;
      }
    }
  }

  protected override void Begin()
  {
    base.Begin();
    this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.LeftLow, 0.5, Bounce.BounceVibrateTime, EasingType.Quadratic);
    this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.RightHigh, 0.60000002384185791, Bounce.BounceVibrateTime, EasingType.Quadratic);
    if (RandomHelper.Probability(0.5))
      this.bounceHigh.EmitAt(this.PlayerManager.Position);
    else
      this.bounceLow.EmitAt(this.PlayerManager.Position);
    IPlayerManager playerManager1 = this.PlayerManager;
    playerManager1.Velocity = playerManager1.Velocity * new Vector3(1f, 0.0f, 1f);
    IPlayerManager playerManager2 = this.PlayerManager;
    playerManager2.Velocity = playerManager2.Velocity + Vector3.UnitY * 0.32f;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Bouncing;
}
