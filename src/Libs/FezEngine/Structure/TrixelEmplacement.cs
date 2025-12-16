// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrixelEmplacement
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Structure;

[TypeSerialization(FlattenToList = true)]
public struct TrixelEmplacement : IEquatable<TrixelEmplacement>, IComparable<TrixelEmplacement>
{
  public int X;
  public int Y;
  public int Z;

  public TrixelEmplacement(TrixelEmplacement other)
  {
    this.X = other.X;
    this.Y = other.Y;
    this.Z = other.Z;
  }

  public TrixelEmplacement(Vector3 position)
  {
    this.X = FezMath.Round((double) position.X);
    this.Y = FezMath.Round((double) position.Y);
    this.Z = FezMath.Round((double) position.Z);
  }

  public TrixelEmplacement(int x, int y, int z)
  {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public void Offset(int offsetX, int offsetY, int offsetZ)
  {
    this.X += offsetX;
    this.Y += offsetY;
    this.Z += offsetZ;
  }

  [Serialization(Ignore = true)]
  public Vector3 Position
  {
    get => new Vector3((float) this.X, (float) this.Y, (float) this.Z);
    set
    {
      this.X = FezMath.Round((double) value.X);
      this.Y = FezMath.Round((double) value.Y);
      this.Z = FezMath.Round((double) value.Z);
    }
  }

  public override bool Equals(object obj) => obj is TrixelEmplacement other && this.Equals(other);

  public override int GetHashCode() => this.X ^ this.Y << 10 ^ this.Z << 20;

  public override string ToString() => $"{{X:{this.X}, Y:{this.Y}, Z:{this.Z}}}";

  public TrixelEmplacement GetTraversal(FaceOrientation face)
  {
    return new TrixelEmplacement(this.Position + face.AsVector());
  }

  public void TraverseInto(FaceOrientation face) => this.Position += face.AsVector();

  public bool IsNeighbor(TrixelEmplacement other)
  {
    return Math.Abs(this.X - other.X) == 1 || Math.Abs(this.Y - other.Y) == 1 || Math.Abs(this.Z - other.Z) == 1;
  }

  public bool Equals(TrixelEmplacement other)
  {
    return (ValueType) other != null && other.X == this.X && other.Y == this.Y && other.Z == this.Z;
  }

  public int CompareTo(TrixelEmplacement other)
  {
    int num = this.X.CompareTo(other.X);
    if (num == 0)
    {
      num = this.Y.CompareTo(other.Y);
      if (num == 0)
        num = this.Z.CompareTo(other.Z);
    }
    return num;
  }

  public static bool operator ==(TrixelEmplacement lhs, TrixelEmplacement rhs) => lhs.Equals(rhs);

  public static bool operator !=(TrixelEmplacement lhs, TrixelEmplacement rhs) => !(lhs == rhs);

  public static TrixelEmplacement operator +(TrixelEmplacement lhs, TrixelEmplacement rhs)
  {
    return new TrixelEmplacement(lhs.Position + rhs.Position);
  }

  public static TrixelEmplacement operator -(TrixelEmplacement lhs, TrixelEmplacement rhs)
  {
    return new TrixelEmplacement(lhs.Position - rhs.Position);
  }

  public static TrixelEmplacement operator +(TrixelEmplacement lhs, Vector3 rhs)
  {
    return new TrixelEmplacement(lhs.Position + rhs);
  }

  public static TrixelEmplacement operator -(TrixelEmplacement lhs, Vector3 rhs)
  {
    return new TrixelEmplacement(lhs.Position - rhs);
  }

  public static TrixelEmplacement operator /(TrixelEmplacement lhs, float rhs)
  {
    return new TrixelEmplacement(lhs.Position / rhs);
  }

  public static bool operator <(TrixelEmplacement lhs, TrixelEmplacement rhs)
  {
    return lhs.CompareTo(rhs) < 0;
  }

  public static bool operator >(TrixelEmplacement lhs, TrixelEmplacement rhs)
  {
    return lhs.CompareTo(rhs) > 0;
  }

  public static bool operator <(TrixelEmplacement lhs, Vector3 rhs)
  {
    return lhs.CompareTo(new TrixelEmplacement(rhs)) < 0;
  }

  public static bool operator >(TrixelEmplacement lhs, Vector3 rhs)
  {
    return lhs.CompareTo(new TrixelEmplacement(rhs)) > 0;
  }
}
