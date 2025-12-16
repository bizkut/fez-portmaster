// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.MouseButtonState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Structure.Input;

public struct MouseButtonState : IEquatable<MouseButtonState>
{
  private readonly MouseDragState dragState;
  private readonly MouseButtonStates state;

  internal MouseButtonState(MouseButtonStates state)
    : this(state, new MouseDragState())
  {
  }

  internal MouseButtonState(MouseButtonStates state, MouseDragState dragState)
  {
    this.dragState = dragState;
    this.state = state;
  }

  public MouseButtonStates State => this.state;

  public MouseDragState DragState => this.dragState;

  public bool Equals(MouseButtonState other)
  {
    return object.Equals((object) other.state, (object) this.state) && other.dragState.Equals(this.dragState);
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != typeof (MouseButtonState)) && this.Equals((MouseButtonState) obj);
  }

  public override int GetHashCode()
  {
    return this.state.GetHashCode() * 397 ^ this.dragState.GetHashCode();
  }

  public static bool operator ==(MouseButtonState left, MouseButtonState right)
  {
    return left.Equals(right);
  }

  public static bool operator !=(MouseButtonState left, MouseButtonState right)
  {
    return !left.Equals(right);
  }
}
