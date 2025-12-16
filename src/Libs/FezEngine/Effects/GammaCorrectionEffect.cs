// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.GammaCorrectionEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class GammaCorrectionEffect : BaseEffect
{
  private readonly SemanticMappedSingle brightness;
  private readonly SemanticMappedTexture mainBufferTexture;

  public GammaCorrectionEffect()
    : base("GammaCorrection")
  {
    this.mainBufferTexture = new SemanticMappedTexture(this.effect.Parameters, nameof (MainBufferTexture));
    this.brightness = new SemanticMappedSingle(this.effect.Parameters, nameof (Brightness));
  }

  public float Brightness
  {
    set => this.brightness.Set(value);
    get => this.brightness.Get();
  }

  public Texture MainBufferTexture
  {
    set => this.mainBufferTexture.Set(value);
    get => this.mainBufferTexture.Get();
  }
}
