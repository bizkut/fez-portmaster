// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.NpcMetadata
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class NpcMetadata
{
  [Serialization(Optional = true)]
  public float WalkSpeed = 1.5f;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool AvoidsGomez;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public ActorType ActorType;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public string SoundPath;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public List<NpcAction> SoundActions = new List<NpcAction>();
}
