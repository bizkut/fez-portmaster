// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrixelFace
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System;

#nullable disable
namespace FezEngine.Structure;

public class TrixelFace : IEquatable<TrixelFace>
{
  public TrixelEmplacement Id;

  public TrixelFace()
    : this(new TrixelEmplacement(), FaceOrientation.Left)
  {
  }

  public TrixelFace(int x, int y, int z, FaceOrientation face)
    : this(new TrixelEmplacement(x, y, z), face)
  {
  }

  public TrixelFace(TrixelEmplacement id, FaceOrientation face)
  {
    this.Id = id;
    this.Face = face;
  }

  public FaceOrientation Face { get; set; }

  public override int GetHashCode() => this.Id.GetHashCode() + this.Face.GetHashCode();

  public override bool Equals(object obj) => obj is TrixelFace && this.Equals(obj as TrixelFace);

  public override string ToString() => Util.ReflectToString((object) this);

  public bool Equals(TrixelFace other) => other.Id.Equals(this.Id) && other.Face == this.Face;
}
