// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrileFace
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Structure;

public class TrileFace : IEquatable<TrileFace>
{
  public TrileEmplacement Id;

  public TrileFace() => this.Id = new TrileEmplacement();

  public FaceOrientation Face { get; set; }

  public override int GetHashCode() => this.Id.GetHashCode() ^ this.Face.GetHashCode();

  public override bool Equals(object obj)
  {
    return (object) (obj as TrileFace) != null && this.Equals(obj as TrileFace);
  }

  public override string ToString() => Util.ReflectToString((object) this);

  public bool Equals(TrileFace other)
  {
    return (object) other != null && other.Id == this.Id && other.Face == this.Face;
  }

  public static bool operator ==(TrileFace lhs, TrileFace rhs)
  {
    return (object) lhs != null ? lhs.Equals(rhs) : (object) rhs == null;
  }

  public static bool operator !=(TrileFace lhs, TrileFace rhs) => !(lhs == rhs);

  public static TrileFace operator +(TrileFace lhs, Vector3 rhs)
  {
    return new TrileFace()
    {
      Id = lhs.Id + rhs,
      Face = lhs.Face
    };
  }

  public static TrileFace operator -(TrileFace lhs, Vector3 rhs)
  {
    return new TrileFace()
    {
      Id = lhs.Id - rhs,
      Face = lhs.Face
    };
  }
}
