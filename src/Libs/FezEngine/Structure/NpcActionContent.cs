// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.NpcActionContent
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using Microsoft.Xna.Framework.Audio;

#nullable disable
namespace FezEngine.Structure;

public class NpcActionContent
{
  [Serialization(Optional = true)]
  public string AnimationName { get; set; }

  [Serialization(Optional = true)]
  public string SoundName { get; set; }

  [Serialization(Ignore = true)]
  public AnimatedTexture Animation { get; set; }

  [Serialization(Ignore = true)]
  public SoundEffect Sound { get; set; }

  public NpcActionContent Clone()
  {
    return new NpcActionContent()
    {
      AnimationName = this.AnimationName,
      SoundName = this.SoundName
    };
  }
}
