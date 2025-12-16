// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Slide
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Slide(Game game) : PlayerAction(game)
{
  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (FezMath.AlmostEqual(this.PlayerManager.Velocity.XZ(), Vector2.Zero) || !FezMath.AlmostEqual(this.InputManager.Movement, Vector2.Zero))
          break;
        this.PlayerManager.Action = ActionType.Sliding;
        break;
    }
  }

  protected override bool Act(TimeSpan elapsed) => true;

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Sliding;
}
