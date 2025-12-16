// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.TimedButtonState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System;

#nullable disable
namespace FezEngine.Structure.Input;

public struct TimedButtonState : IEquatable<TimedButtonState>
{
  public readonly FezButtonState State;
  public readonly TimeSpan TimePressed;

  private TimedButtonState(FezButtonState state, TimeSpan timePressed)
  {
    this.State = state;
    this.TimePressed = timePressed;
  }

  internal TimedButtonState NextState(bool down, TimeSpan elapsed)
  {
    return new TimedButtonState(this.State.NextState(down), down ? this.TimePressed + elapsed : TimeSpan.Zero);
  }

  public bool Equals(TimedButtonState other)
  {
    return other.State == this.State && other.TimePressed == this.TimePressed;
  }

  public override string ToString() => Util.ReflectToString((object) this);
}
