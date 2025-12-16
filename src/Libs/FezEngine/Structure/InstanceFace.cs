// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.InstanceFace
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System;

#nullable disable
namespace FezEngine.Structure;

public class InstanceFace : IEquatable<InstanceFace>
{
  public InstanceFace()
  {
  }

  public InstanceFace(TrileInstance instance, FaceOrientation face)
  {
    this.Instance = instance;
    this.Face = face;
  }

  public TrileInstance Instance { get; set; }

  public FaceOrientation Face { get; set; }

  public override int GetHashCode() => this.Instance.GetHashCode() ^ this.Face.GetHashCode();

  public override bool Equals(object obj)
  {
    return (object) (obj as TrileFace) != null && this.Equals((object) (obj as TrileFace));
  }

  public override string ToString() => Util.ReflectToString((object) this);

  public bool Equals(InstanceFace other)
  {
    return (object) other != null && other.Instance == this.Instance && other.Face == this.Face;
  }

  public static bool operator ==(InstanceFace lhs, InstanceFace rhs)
  {
    return (object) lhs != null ? lhs.Equals(rhs) : (object) rhs == null;
  }

  public static bool operator !=(InstanceFace lhs, InstanceFace rhs) => !(lhs == rhs);
}
