// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrileActorSettings
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using ContentSerialization.Attributes;
using System;

#nullable disable
namespace FezEngine.Structure;

public class TrileActorSettings : IEquatable<TrileActorSettings>
{
  public TrileActorSettings()
  {
  }

  public TrileActorSettings(TrileActorSettings copy)
  {
    this.Type = copy.Type;
    this.Face = copy.Face;
  }

  [Serialization(Optional = true)]
  public ActorType Type { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public FaceOrientation Face { get; set; }

  public override int GetHashCode()
  {
    return Util.CombineHashCodes(this.Type.GetHashCode(), this.Face.GetHashCode());
  }

  public bool Equals(TrileActorSettings other)
  {
    return (object) other != null && other.Type == this.Type && other.Face == this.Face;
  }

  public override bool Equals(object obj)
  {
    return (object) (obj as TrileActorSettings) != null && this.Equals(obj as TrileActorSettings);
  }

  public static bool operator ==(TrileActorSettings lhs, TrileActorSettings rhs)
  {
    return (object) lhs != null ? lhs.Equals(rhs) : (object) rhs == null;
  }

  public static bool operator !=(TrileActorSettings lhs, TrileActorSettings rhs) => !(lhs == rhs);
}
