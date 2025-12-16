// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.MovementPath
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class MovementPath
{
  public MovementPath() => this.Segments = new List<PathSegment>();

  [Serialization(Ignore = true)]
  public int Id { get; set; }

  [Serialization(Optional = true)]
  public bool IsSpline { get; set; }

  [Serialization(Optional = true)]
  public float OffsetSeconds { get; set; }

  [Serialization(CollectionItemName = "segment")]
  public List<PathSegment> Segments { get; set; }

  public PathEndBehavior EndBehavior { get; set; }

  public bool NeedsTrigger { get; set; }

  [Serialization(Optional = true)]
  public string SoundName { get; set; }

  [Serialization(Ignore = true)]
  public bool RunOnce { get; set; }

  [Serialization(Ignore = true)]
  public bool RunSingleSegment { get; set; }

  [Serialization(Ignore = true)]
  public bool Backwards { get; set; }

  [Serialization(Ignore = true)]
  public bool InTransition { get; set; }

  [Serialization(Ignore = true)]
  public bool OutTransition { get; set; }

  [Serialization(Optional = true)]
  public bool SaveTrigger { get; set; }

  public MovementPath Clone()
  {
    List<PathSegment> pathSegmentList = new List<PathSegment>(this.Segments.Count);
    foreach (PathSegment segment in this.Segments)
      pathSegmentList.Add(segment.Clone());
    return new MovementPath()
    {
      IsSpline = this.IsSpline,
      OffsetSeconds = this.OffsetSeconds,
      Segments = pathSegmentList,
      NeedsTrigger = this.NeedsTrigger,
      SoundName = this.SoundName,
      RunOnce = this.RunOnce,
      RunSingleSegment = this.RunSingleSegment,
      Backwards = this.Backwards,
      InTransition = this.InTransition,
      OutTransition = this.OutTransition,
      SaveTrigger = this.SaveTrigger
    };
  }
}
