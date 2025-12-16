// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.MouseDragState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Structure.Input;

public struct MouseDragState : IEquatable<MouseDragState>
{
  private readonly Point start;
  private readonly Point movement;
  private readonly bool preDrag;

  internal MouseDragState(Point start, Point current)
    : this(start, current, false)
  {
  }

  internal MouseDragState(Point start, Point current, bool preDrag)
  {
    this.start = start;
    this.preDrag = preDrag;
    this.movement = new Point(current.X - start.X, current.Y - start.Y);
  }

  public Point Start => this.start;

  public Point Movement => this.movement;

  internal bool PreDrag => this.preDrag;

  public bool Equals(MouseDragState other)
  {
    return other.start.Equals(this.start) && other.movement.Equals(this.movement) && other.preDrag.Equals(this.preDrag);
  }

  public override bool Equals(object obj)
  {
    return obj != null && !(obj.GetType() != typeof (MouseDragState)) && this.Equals((MouseDragState) obj);
  }

  public override int GetHashCode()
  {
    Point point = this.start;
    int num = point.GetHashCode() * 397;
    point = this.movement;
    int hashCode = point.GetHashCode();
    return (num ^ hashCode) * 397 ^ this.preDrag.GetHashCode();
  }

  public static bool operator ==(MouseDragState left, MouseDragState right) => left.Equals(right);

  public static bool operator !=(MouseDragState left, MouseDragState right) => !left.Equals(right);
}
