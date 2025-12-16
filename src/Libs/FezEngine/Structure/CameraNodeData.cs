// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.CameraNodeData
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using System;

#nullable disable
namespace FezEngine.Structure;

public class CameraNodeData : ICloneable
{
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Perspective { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int PixelsPerTrixel { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public string SoundName { get; set; }

  public object Clone()
  {
    return (object) new CameraNodeData()
    {
      Perspective = this.Perspective,
      PixelsPerTrixel = this.PixelsPerTrixel,
      SoundName = this.SoundName
    };
  }
}
