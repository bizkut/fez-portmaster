// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.InstanceActorSettings
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using ContentSerialization.Attributes;
using System;

#nullable disable
namespace FezEngine.Structure;

public class InstanceActorSettings : IEquatable<InstanceActorSettings>
{
  public const int Steps = 16 /*0x10*/;

  public InstanceActorSettings()
  {
  }

  public InstanceActorSettings(InstanceActorSettings copy)
  {
    this.ContainedTrile = copy.ContainedTrile;
    this.SignText = copy.SignText;
    if (copy.Sequence != null)
    {
      this.Sequence = new bool[16 /*0x10*/];
      Array.Copy((Array) copy.Sequence, (Array) this.Sequence, 16 /*0x10*/);
    }
    this.SequenceSampleName = copy.SequenceSampleName;
    this.SequenceAlternateSampleName = copy.SequenceAlternateSampleName;
    this.HostVolume = copy.HostVolume;
  }

  [Serialization(Ignore = true)]
  public bool Inactive { get; set; }

  [Serialization(Optional = true)]
  public int? ContainedTrile { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public string SignText { get; set; }

  [Serialization(Optional = true)]
  public bool[] Sequence { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public string SequenceSampleName { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public string SequenceAlternateSampleName { get; set; }

  [Serialization(Optional = true)]
  public int? HostVolume { get; set; }

  public override int GetHashCode()
  {
    return Util.CombineHashCodes((object) this.ContainedTrile, (object) this.SignText, (object) this.Sequence, (object) this.SequenceSampleName, (object) this.SequenceAlternateSampleName, (object) this.HostVolume);
  }

  public bool Equals(InstanceActorSettings other)
  {
    if ((object) other != null)
    {
      int? containedTrile = other.ContainedTrile;
      int? nullable = this.ContainedTrile;
      if ((containedTrile.GetValueOrDefault() == nullable.GetValueOrDefault() ? (containedTrile.HasValue == nullable.HasValue ? 1 : 0) : 0) != 0 && other.SignText == this.SignText && object.Equals((object) other.Sequence, (object) this.Sequence) && other.SequenceSampleName == this.SequenceSampleName && other.SequenceAlternateSampleName == this.SequenceAlternateSampleName)
      {
        nullable = other.HostVolume;
        int? hostVolume = this.HostVolume;
        return nullable.GetValueOrDefault() == hostVolume.GetValueOrDefault() && nullable.HasValue == hostVolume.HasValue;
      }
    }
    return false;
  }

  public override bool Equals(object obj)
  {
    return (object) (obj as InstanceActorSettings) != null && this.Equals(obj as InstanceActorSettings);
  }

  public static bool operator ==(InstanceActorSettings lhs, InstanceActorSettings rhs)
  {
    return (object) lhs != null ? lhs.Equals(rhs) : (object) rhs == null;
  }

  public static bool operator !=(InstanceActorSettings lhs, InstanceActorSettings rhs)
  {
    return !(lhs == rhs);
  }
}
