// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.FastBlurEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class FastBlurEffect : BaseEffect
{
  private readonly SemanticMappedVector2 texelSize;
  private readonly SemanticMappedVector2 direction;
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedSingle blurWidth;

  public FastBlurEffect()
    : base(nameof (FastBlurEffect))
  {
    this.texelSize = new SemanticMappedVector2(this.effect.Parameters, "TexelSize");
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.blurWidth = new SemanticMappedSingle(this.effect.Parameters, nameof (BlurWidth));
    this.direction = new SemanticMappedVector2(this.effect.Parameters, "Direction");
    this.effect.Parameters["Weights"].SetValue(new float[5]
    {
      0.08812122f,
      0.167555347f,
      0.136911243f,
      0.09517907f,
      0.0562937222f
    });
    this.effect.Parameters["Offsets"].SetValue(new float[5]
    {
      0.0f,
      -0.0152997831f,
      -0.0356500447f,
      -0.0558822826f,
      -0.075930886f
    });
    this.BlurWidth = 1f;
  }

  public BlurPass Pass
  {
    set
    {
      if (value == BlurPass.Horizontal)
        this.direction.Set(Vector2.UnitX);
      if (value != BlurPass.Vertical)
        return;
      this.direction.Set(Vector2.UnitY);
    }
  }

  public float BlurWidth
  {
    set => this.blurWidth.Set(value);
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.texture.Set((Texture) mesh.Texture);
    this.texelSize.Set(new Vector2(1f / (float) mesh.TextureMap.Width, 1f / (float) mesh.TextureMap.Height));
  }
}
