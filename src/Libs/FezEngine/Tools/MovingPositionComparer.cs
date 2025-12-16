// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.MovingPositionComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class MovingPositionComparer : IComparer<Vector3>
{
  private Vector3 ordering;

  public MovingPositionComparer(Vector3 ordering) => this.ordering = ordering.Sign();

  public int Compare(Vector3 lhs, Vector3 rhs)
  {
    int num = rhs.X.CompareTo(lhs.X) * (int) this.ordering.X;
    if (num == 0)
    {
      num = rhs.Y.CompareTo(lhs.Y) * (int) this.ordering.Y;
      if (num == 0)
        num = rhs.Z.CompareTo(lhs.Z) * (int) this.ordering.Z;
    }
    return num;
  }
}
