// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Volume
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class Volume
{
  private Vector3 from;
  private Vector3 to;

  public Volume()
  {
    this.Orientations = new HashSet<FaceOrientation>((IEqualityComparer<FaceOrientation>) FaceOrientationComparer.Default);
    this.Enabled = true;
  }

  [Serialization(Ignore = true)]
  public int Id { get; set; }

  [Serialization(Ignore = true)]
  public bool Enabled { get; set; }

  [Serialization(Ignore = true)]
  public bool PlayerInside { get; set; }

  [Serialization(Ignore = true)]
  public bool? PlayerIsHigher { get; set; }

  public HashSet<FaceOrientation> Orientations { get; set; }

  [Serialization(Optional = true)]
  public VolumeActorSettings ActorSettings { get; set; }

  public Vector3 From
  {
    get => this.from;
    set
    {
      this.from = value;
      this.BoundingBox = new BoundingBox(value, this.BoundingBox.Max);
    }
  }

  public Vector3 To
  {
    get => this.to;
    set
    {
      this.to = value;
      this.BoundingBox = new BoundingBox(this.BoundingBox.Min, value);
    }
  }

  [Serialization(Ignore = true)]
  public BoundingBox BoundingBox { get; set; }
}
