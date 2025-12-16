// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.DirectionalState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Structure.Input;

public struct DirectionalState
{
  public readonly Vector2 Direction;
  public readonly Vector2 Movement;
  public readonly TimedButtonState Up;
  public readonly TimedButtonState Down;
  public readonly TimedButtonState Left;
  public readonly TimedButtonState Right;

  private DirectionalState(
    Vector2 direction,
    Vector2 movement,
    TimedButtonState up,
    TimedButtonState down,
    TimedButtonState left,
    TimedButtonState right)
  {
    this.Direction = direction;
    this.Movement = movement;
    this.Up = up;
    this.Down = down;
    this.Left = left;
    this.Right = right;
  }

  internal DirectionalState NextState(
    bool up,
    bool down,
    bool left,
    bool right,
    TimeSpan elapsed)
  {
    Vector2 direction = new Vector2(left ? -1f : (right ? 1f : 0.0f), up ? 1f : (down ? -1f : 0.0f));
    Vector2 movement = direction - this.Direction;
    TimedButtonState timedButtonState = this.Up;
    TimedButtonState up1 = timedButtonState.NextState(up, elapsed);
    timedButtonState = this.Down;
    TimedButtonState down1 = timedButtonState.NextState(down, elapsed);
    timedButtonState = this.Left;
    TimedButtonState left1 = timedButtonState.NextState(left, elapsed);
    timedButtonState = this.Right;
    TimedButtonState right1 = timedButtonState.NextState(right, elapsed);
    return new DirectionalState(direction, movement, up1, down1, left1, right1);
  }

  public override string ToString() => Util.ReflectToString((object) this);
}
