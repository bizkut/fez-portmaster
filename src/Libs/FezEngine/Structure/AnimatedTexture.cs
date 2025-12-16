// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.AnimatedTexture
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Effects.Structures;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Structure;

public class AnimatedTexture : IDisposable
{
  public Texture2D Texture { get; set; }

  public Rectangle[] Offsets { get; set; }

  public AnimationTiming Timing { get; set; }

  public int FrameWidth { get; set; }

  public int FrameHeight { get; set; }

  public Vector2 PotOffset { get; set; }

  [Serialization(Ignore = true)]
  public bool NoHat { get; set; }

  public void Dispose()
  {
    if (this.Texture != null)
    {
      this.Texture.Unhook();
      this.Texture.Dispose();
    }
    this.Texture = (Texture2D) null;
    this.Timing = (AnimationTiming) null;
  }
}
