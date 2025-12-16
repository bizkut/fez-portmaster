// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.CombineEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class CombineEffect : BaseEffect
{
  private readonly SemanticMappedTexture rightTexture;
  private readonly SemanticMappedTexture leftTexture;
  private readonly SemanticMappedSingle redGamma;
  private readonly SemanticMappedMatrix rightFilter;
  private readonly SemanticMappedMatrix leftFilter;

  public CombineEffect()
    : base(nameof (CombineEffect))
  {
    this.rightTexture = new SemanticMappedTexture(this.effect.Parameters, nameof (RightTexture));
    this.leftTexture = new SemanticMappedTexture(this.effect.Parameters, nameof (LeftTexture));
    this.redGamma = new SemanticMappedSingle(this.effect.Parameters, nameof (RedGamma));
    this.rightFilter = new SemanticMappedMatrix(this.effect.Parameters, nameof (RightFilter));
    this.leftFilter = new SemanticMappedMatrix(this.effect.Parameters, nameof (LeftFilter));
    this.LeftFilter = new Matrix(0.2125f, 0.7154f, 0.0721f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
    this.RightFilter = new Matrix(0.0f, 0.0f, 0.0f, 0.0f, 0.2125f, 0.7154f, 0.0721f, 0.0f, 0.2125f, 0.7154f, 0.0721f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
  }

  public Texture2D LeftTexture
  {
    set => this.leftTexture.Set((Texture) value);
  }

  public Texture2D RightTexture
  {
    set => this.rightTexture.Set((Texture) value);
  }

  public float RedGamma
  {
    set => this.redGamma.Set(value);
  }

  public Matrix RightFilter
  {
    set => this.rightFilter.Set(value);
  }

  public Matrix LeftFilter
  {
    set => this.leftFilter.Set(value);
  }
}
