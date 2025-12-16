// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.BurnInPostEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class BurnInPostEffect : BaseEffect
{
  private readonly SemanticMappedVector3 acceptColor;
  private readonly SemanticMappedTexture newFrameBuffer;
  private readonly SemanticMappedTexture oldFrameBuffer;

  public BurnInPostEffect()
    : base(nameof (BurnInPostEffect))
  {
    this.acceptColor = new SemanticMappedVector3(this.effect.Parameters, "AcceptColor");
    this.acceptColor.Set(new Vector3(0.933333337f, 0.0f, 0.5529412f));
    this.oldFrameBuffer = new SemanticMappedTexture(this.effect.Parameters, "OldFrameTexture");
    this.newFrameBuffer = new SemanticMappedTexture(this.effect.Parameters, "NewFrameTexture");
  }

  public Texture OldFrameBuffer
  {
    set => this.oldFrameBuffer.Set(value);
  }

  public Texture NewFrameBuffer
  {
    set => this.newFrameBuffer.Set(value);
  }
}
