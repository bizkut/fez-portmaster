// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.TimedAnalogButtonState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System;

#nullable disable
namespace FezEngine.Structure.Input;

public struct TimedAnalogButtonState
{
  private const double TriggerThreshold = 0.5;
  public readonly float Value;
  public readonly FezButtonState State;
  public readonly TimeSpan TimePressed;

  private TimedAnalogButtonState(float value, FezButtonState state, TimeSpan timePressed)
  {
    this.Value = value;
    this.State = state;
    this.TimePressed = timePressed;
  }

  internal TimedAnalogButtonState NextState(float value, TimeSpan elapsed)
  {
    bool pressed = (double) value > 0.5;
    return new TimedAnalogButtonState(value, this.State.NextState(pressed), pressed ? this.TimePressed + elapsed : TimeSpan.Zero);
  }

  public override string ToString() => Util.ReflectToString((object) this);
}
