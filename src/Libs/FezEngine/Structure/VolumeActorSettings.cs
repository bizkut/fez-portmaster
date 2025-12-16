// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.VolumeActorSettings
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class VolumeActorSettings
{
  [Serialization(Optional = true)]
  public Vector2 FarawayPlaneOffset;
  [Serialization(Ignore = true)]
  public float WaterOffset;
  [Serialization(Ignore = true)]
  public string DestinationSong;
  [Serialization(Ignore = true)]
  public float DestinationPixelsPerTrixel;
  [Serialization(Ignore = true)]
  public float DestinationRadius;
  [Serialization(Ignore = true)]
  public Vector2 DestinationOffset;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool IsPointOfInterest;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool WaterLocked;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool IsSecretPassage;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public CodeInput[] CodePattern;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public List<DotDialogueLine> DotDialogue = new List<DotDialogueLine>();
  [Serialization(Ignore = true)]
  public int NextLine = -1;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool IsBlackHole;
  [Serialization(Ignore = true)]
  public bool Sucking;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool NeedsTrigger;
  [Serialization(Ignore = true)]
  public bool PreventHey;
}
