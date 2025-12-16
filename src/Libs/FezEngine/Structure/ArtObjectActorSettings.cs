// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.ArtObjectActorSettings
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class ArtObjectActorSettings : IEquatable<ArtObjectActorSettings>
{
  public ArtObjectActorSettings()
  {
    this.InvisibleSides = new HashSet<FaceOrientation>((IEqualityComparer<FaceOrientation>) FaceOrientationComparer.Default);
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Inactive { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public ActorType ContainedTrile { get; set; }

  [Serialization(Optional = true)]
  public int? AttachedGroup { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public float SpinOffset { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public float SpinEvery { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public Viewpoint SpinView { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool OffCenter { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public Vector3 RotationCenter { get; set; }

  [Serialization(Optional = true)]
  public VibrationMotor[] VibrationPattern { get; set; }

  [Serialization(Optional = true)]
  public CodeInput[] CodePattern { get; set; }

  [Serialization(Optional = true)]
  public PathSegment Segment { get; set; }

  [Serialization(Optional = true)]
  public int? NextNode { get; set; }

  [Serialization(Optional = true)]
  public string DestinationLevel { get; set; }

  [Serialization(Optional = true)]
  public string TreasureMapName { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public float TimeswitchWindBackSpeed { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public HashSet<FaceOrientation> InvisibleSides { get; set; }

  [Serialization(Ignore = true)]
  public ArtObjectInstance NextNodeAo { get; set; }

  [Serialization(Ignore = true)]
  public ArtObjectInstance PrecedingNodeAo { get; set; }

  [Serialization(Ignore = true)]
  public bool ShouldMoveToEnd { get; set; }

  [Serialization(Ignore = true)]
  public float? ShouldMoveToHeight { get; set; }

  public bool Equals(ArtObjectActorSettings other)
  {
    if ((object) other != null && other.ContainedTrile == this.ContainedTrile && other.Inactive == this.Inactive)
    {
      int? nullable1 = other.AttachedGroup;
      int? nullable2 = this.AttachedGroup;
      if ((nullable1.GetValueOrDefault() == nullable2.GetValueOrDefault() ? (nullable1.HasValue == nullable2.HasValue ? 1 : 0) : 0) != 0 && (double) other.SpinOffset == (double) this.SpinOffset && (double) other.SpinEvery == (double) this.SpinEvery && other.SpinView == this.SpinView && other.OffCenter == this.OffCenter && other.RotationCenter == this.RotationCenter && other.VibrationPattern == this.VibrationPattern && other.CodePattern == this.CodePattern && other.Segment == this.Segment)
      {
        nullable2 = other.NextNode;
        nullable1 = this.NextNode;
        if ((nullable2.GetValueOrDefault() == nullable1.GetValueOrDefault() ? (nullable2.HasValue == nullable1.HasValue ? 1 : 0) : 0) != 0 && other.DestinationLevel == this.DestinationLevel && other.TreasureMapName == this.TreasureMapName && (double) other.TimeswitchWindBackSpeed == (double) this.TimeswitchWindBackSpeed)
          return other.InvisibleSides.Equals((object) this.InvisibleSides);
      }
    }
    return false;
  }

  public override bool Equals(object obj)
  {
    return (object) (obj as ArtObjectActorSettings) != null && this.Equals(obj as ArtObjectActorSettings);
  }

  public static bool operator ==(ArtObjectActorSettings lhs, ArtObjectActorSettings rhs)
  {
    return (object) lhs != null ? lhs.Equals(rhs) : (object) rhs == null;
  }

  public static bool operator !=(ArtObjectActorSettings lhs, ArtObjectActorSettings rhs)
  {
    return !(lhs == rhs);
  }
}
