// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.MovingTrileInstanceComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class MovingTrileInstanceComparer : IComparer<TrileInstance>
{
  private Vector3 ordering;

  public MovingTrileInstanceComparer(Vector3 ordering) => this.ordering = ordering.Sign();

  public int Compare(TrileInstance lhs, TrileInstance rhs)
  {
    Vector3 position1 = rhs.Position;
    Vector3 position2 = lhs.Position;
    int num = position1.X.CompareTo(position2.X) * (int) this.ordering.X;
    if (num == 0)
    {
      num = position1.Y.CompareTo(position2.Y) * (int) this.ordering.Y;
      if (num == 0)
        num = position1.Z.CompareTo(position2.Z) * (int) this.ordering.Z;
    }
    return num;
  }
}
