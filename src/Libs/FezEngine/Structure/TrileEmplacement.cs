// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrileEmplacement
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
public struct TrileEmplacement : IEquatable<TrileEmplacement>, IComparable<TrileEmplacement>
{
  public int X;
  public int Y;
  public int Z;

  public TrileEmplacement(Vector3 position)
  {
    this.X = FezMath.Round((double) position.X);
    this.Y = FezMath.Round((double) position.Y);
    this.Z = FezMath.Round((double) position.Z);
  }

  public TrileEmplacement(int x, int y, int z)
  {
    this.X = x;
    this.Y = y;
    this.Z = z;
  }

  public TrileEmplacement GetOffset(Vector3 vector)
  {
    return new TrileEmplacement(this.X + (int) vector.X, this.Y + (int) vector.Y, this.Z + (int) vector.Z);
  }

  public TrileEmplacement GetOffset(int offsetX, int offsetY, int offsetZ)
  {
    return new TrileEmplacement(this.X + offsetX, this.Y + offsetY, this.Z + offsetZ);
  }

  [Serialization(Ignore = true)]
  public Vector3 AsVector
  {
    get => new Vector3((float) this.X, (float) this.Y, (float) this.Z);
    set
    {
      this.X = FezMath.Round((double) value.X);
      this.Y = FezMath.Round((double) value.Y);
      this.Z = FezMath.Round((double) value.Z);
    }
  }

  public override bool Equals(object obj) => obj is TrileEmplacement other && this.Equals(other);

  public override int GetHashCode() => this.X ^ this.Y << 10 ^ this.Z << 20;

  public override string ToString() => $"({this.X}, {this.Y}, {this.Z})";

  public void TraverseInto(FaceOrientation face)
  {
    switch (face)
    {
      case FaceOrientation.Left:
        --this.X;
        break;
      case FaceOrientation.Down:
        --this.Y;
        break;
      case FaceOrientation.Back:
        --this.Z;
        break;
      case FaceOrientation.Top:
        ++this.Y;
        break;
      case FaceOrientation.Front:
        ++this.Z;
        break;
      default:
        ++this.X;
        break;
    }
  }

  public TrileEmplacement GetTraversal(ref FaceOrientation face)
  {
    switch (face)
    {
      case FaceOrientation.Left:
        return new TrileEmplacement(this.X - 1, this.Y, this.Z);
      case FaceOrientation.Down:
        return new TrileEmplacement(this.X, this.Y - 1, this.Z);
      case FaceOrientation.Back:
        return new TrileEmplacement(this.X, this.Y, this.Z - 1);
      case FaceOrientation.Top:
        return new TrileEmplacement(this.X, this.Y + 1, this.Z);
      case FaceOrientation.Front:
        return new TrileEmplacement(this.X, this.Y, this.Z + 1);
      default:
        return new TrileEmplacement(this.X + 1, this.Y, this.Z);
    }
  }

  public bool Equals(TrileEmplacement other)
  {
    return other.X == this.X && other.Y == this.Y && other.Z == this.Z;
  }

  public int CompareTo(TrileEmplacement other)
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

  public static bool operator ==(TrileEmplacement lhs, TrileEmplacement rhs)
  {
    return (ValueType) lhs != null ? lhs.Equals(rhs) : (ValueType) rhs == null;
  }

  public static bool operator !=(TrileEmplacement lhs, TrileEmplacement rhs) => !(lhs == rhs);

  public static TrileEmplacement operator +(TrileEmplacement lhs, TrileEmplacement rhs)
  {
    return new TrileEmplacement(lhs.AsVector + rhs.AsVector);
  }

  public static TrileEmplacement operator -(TrileEmplacement lhs, TrileEmplacement rhs)
  {
    return new TrileEmplacement(lhs.AsVector - rhs.AsVector);
  }

  public static TrileEmplacement operator +(TrileEmplacement lhs, Vector3 rhs)
  {
    return new TrileEmplacement(lhs.AsVector + rhs);
  }

  public static TrileEmplacement operator -(TrileEmplacement lhs, Vector3 rhs)
  {
    return new TrileEmplacement(lhs.AsVector - rhs);
  }

  public static TrileEmplacement operator /(TrileEmplacement lhs, float rhs)
  {
    return new TrileEmplacement(lhs.AsVector / rhs);
  }

  public static TrileEmplacement operator *(TrileEmplacement lhs, Vector3 rhs)
  {
    return new TrileEmplacement(lhs.AsVector * rhs);
  }
}
