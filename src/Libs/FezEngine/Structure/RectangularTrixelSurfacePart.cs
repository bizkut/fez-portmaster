// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.RectangularTrixelSurfacePart
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using ContentSerialization.Attributes;
using System;

#nullable disable
namespace FezEngine.Structure;

public class RectangularTrixelSurfacePart : IEquatable<RectangularTrixelSurfacePart>
{
  public TrixelEmplacement Start { get; set; }

  [Serialization(Name = "tSize")]
  public int TangentSize { get; set; }

  [Serialization(Name = "bSize")]
  public int BitangentSize { get; set; }

  [Serialization(Ignore = true)]
  public FaceOrientation Orientation { get; set; }

  public override int GetHashCode()
  {
    int hashCode1 = this.Start.GetHashCode();
    int num = this.TangentSize;
    int hashCode2 = num.GetHashCode();
    num = this.BitangentSize;
    int hashCode3 = num.GetHashCode();
    int hashCode4 = this.Orientation.GetHashCode();
    return Util.CombineHashCodes(hashCode1, hashCode2, hashCode3, hashCode4);
  }

  public override bool Equals(object obj)
  {
    return obj != null && this.Equals(obj as RectangularTrixelSurfacePart);
  }

  public bool Equals(RectangularTrixelSurfacePart other)
  {
    return other != null && other.Orientation == this.Orientation && other.Start.Equals(this.Start) && other.TangentSize == this.TangentSize && other.BitangentSize == this.BitangentSize;
  }
}
