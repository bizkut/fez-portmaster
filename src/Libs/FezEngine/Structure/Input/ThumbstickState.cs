// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.ThumbstickState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Structure.Input;

public struct ThumbstickState
{
  private const double PressThreshold = 0.5;
  public readonly Vector2 Position;
  public readonly Vector2 Movement;
  public readonly TimedButtonState Clicked;
  public readonly TimedButtonState Up;
  public readonly TimedButtonState Down;
  public readonly TimedButtonState Left;
  public readonly TimedButtonState Right;

  private ThumbstickState(
    Vector2 position,
    Vector2 movement,
    TimedButtonState clicked,
    TimedButtonState up,
    TimedButtonState down,
    TimedButtonState left,
    TimedButtonState right)
  {
    this.Position = position;
    this.Movement = movement;
    this.Clicked = clicked;
    this.Up = up;
    this.Down = down;
    this.Left = left;
    this.Right = right;
  }

  internal ThumbstickState NextState(Vector2 position, bool clicked, TimeSpan elapsed)
  {
    Vector2 position1 = position;
    Vector2 movement = position - this.Position;
    TimedButtonState timedButtonState = this.Clicked;
    TimedButtonState clicked1 = timedButtonState.NextState(clicked, elapsed);
    timedButtonState = this.Up;
    TimedButtonState up = timedButtonState.NextState((double) FezMath.Saturate(position.Y) > 0.5, elapsed);
    timedButtonState = this.Down;
    TimedButtonState down = timedButtonState.NextState((double) FezMath.Saturate(-position.Y) > 0.5, elapsed);
    timedButtonState = this.Left;
    TimedButtonState left = timedButtonState.NextState((double) FezMath.Saturate(-position.X) > 0.5, elapsed);
    timedButtonState = this.Right;
    TimedButtonState right = timedButtonState.NextState((double) FezMath.Saturate(position.X) > 0.5, elapsed);
    return new ThumbstickState(position1, movement, clicked1, up, down, left, right);
  }

  public override string ToString() => Util.ReflectToString((object) this);

  public static Vector2 CircleToSquare(Vector2 point)
  {
    double num = Math.Atan2((double) point.Y, (double) point.X) + 3.1415927410125732;
    if (num <= 0.78539818525314331 || num > 5.4977874755859375)
      return point * (float) (1.0 / Math.Cos(num));
    if (num > 0.78539818525314331 && num <= 2.3561944961547852)
      return point * (float) (1.0 / Math.Sin(num));
    if (num > 2.3561944961547852 && num <= 3.9269909858703613)
      return point * (float) (-1.0 / Math.Cos(num));
    if (num > 3.9269909858703613 && num <= 5.4977874755859375)
      return point * (float) (-1.0 / Math.Sin(num));
    throw new InvalidOperationException("Invalid angle...?");
  }
}
