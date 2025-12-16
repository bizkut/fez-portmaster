// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.GomezEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class GomezEffect : BaseEffect
{
  private readonly SemanticMappedTexture animatedTexture;
  private readonly SemanticMappedBoolean silhouette;
  private readonly SemanticMappedSingle background;
  private readonly SemanticMappedBoolean colorSwap;
  private readonly SemanticMappedBoolean noMoreFez;
  private readonly SemanticMappedVector3 redSwap;
  private readonly SemanticMappedVector3 blackSwap;
  private readonly SemanticMappedVector3 whiteSwap;
  private readonly SemanticMappedVector3 yellowSwap;
  private readonly SemanticMappedVector3 graySwap;
  private ColorSwapMode colorSwapMode;

  public GomezEffect()
    : base(nameof (GomezEffect))
  {
    this.animatedTexture = new SemanticMappedTexture(this.effect.Parameters, "AnimatedTexture");
    this.silhouette = new SemanticMappedBoolean(this.effect.Parameters, nameof (Silhouette));
    this.background = new SemanticMappedSingle(this.effect.Parameters, nameof (Background));
    this.colorSwap = new SemanticMappedBoolean(this.effect.Parameters, "ColorSwap");
    this.redSwap = new SemanticMappedVector3(this.effect.Parameters, "RedSwap");
    this.blackSwap = new SemanticMappedVector3(this.effect.Parameters, "BlackSwap");
    this.whiteSwap = new SemanticMappedVector3(this.effect.Parameters, "WhiteSwap");
    this.yellowSwap = new SemanticMappedVector3(this.effect.Parameters, "YellowSwap");
    this.graySwap = new SemanticMappedVector3(this.effect.Parameters, "GraySwap");
    this.noMoreFez = new SemanticMappedBoolean(this.effect.Parameters, nameof (NoMoreFez));
    this.Pass = LightingEffectPass.Main;
  }

  public override BaseEffect Clone()
  {
    return (BaseEffect) new GomezEffect()
    {
      Animation = this.animatedTexture.Get(),
      Silhouette = this.silhouette.Get(),
      Background = this.background.Get(),
      ColorSwapMode = this.colorSwapMode
    };
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    if (this.IgnoreCache || !group.EffectOwner || group.WorldMatrix.Dirty)
    {
      this.matrices.World = (Matrix) group.WorldMatrix;
      group.WorldMatrix.Clean();
    }
    if (group.TextureMatrix.Value.HasValue)
    {
      this.matrices.TextureMatrix = group.TextureMatrix.Value.Value;
      this.textureMatrixDirty = true;
    }
    else
    {
      if (!this.textureMatrixDirty)
        return;
      this.matrices.TextureMatrix = Matrix.Identity;
      this.textureMatrixDirty = false;
    }
  }

  public Texture Animation
  {
    set => this.animatedTexture.Set(value);
  }

  public bool Silhouette
  {
    set => this.silhouette.Set(value);
  }

  public float Background
  {
    set => this.background.Set(value);
  }

  public ColorSwapMode ColorSwapMode
  {
    get => this.colorSwapMode;
    set
    {
      this.colorSwapMode = value;
      switch (this.colorSwapMode)
      {
        case ColorSwapMode.None:
          this.colorSwap.Set(false);
          break;
        case ColorSwapMode.VirtualBoy:
          this.colorSwap.Set(true);
          this.redSwap.Set(new Vector3(0.619607866f, 0.0f, 0.0196078438f));
          this.blackSwap.Set(new Vector3(0.0f, 0.0f, 0.0f));
          this.whiteSwap.Set(new Vector3(0.996078432f, 0.003921569f, 0.0f));
          this.yellowSwap.Set(new Vector3(0.8156863f, 0.003921569f, 0.0f));
          this.graySwap.Set(new Vector3(0.396078438f, 0.003921569f, 0.0f));
          break;
        case ColorSwapMode.Gameboy:
          this.colorSwap.Set(true);
          this.redSwap.Set(new Vector3(0.321568638f, 0.498039216f, 0.223529413f));
          this.blackSwap.Set(new Vector3(0.1254902f, 0.274509817f, 0.192156866f));
          this.whiteSwap.Set(new Vector3(0.843137264f, 0.9098039f, 0.5803922f));
          this.yellowSwap.Set(new Vector3(0.68235296f, 0.768627465f, 0.2509804f));
          this.graySwap.Set(new Vector3(0.321568638f, 0.498039216f, 0.223529413f));
          break;
        case ColorSwapMode.Cmyk:
          this.colorSwap.Set(true);
          this.redSwap.Set(new Vector3(0.933333337f, 0.0f, 0.5529412f));
          this.blackSwap.Set(new Vector3(0.0f, 0.0f, 0.0f));
          this.whiteSwap.Set(new Vector3(1f, 1f, 1f));
          this.yellowSwap.Set(new Vector3(1f, 1f, 0.0f));
          this.graySwap.Set(new Vector3(1f, 1f, 1f));
          break;
      }
    }
  }

  public bool NoMoreFez
  {
    set => this.noMoreFez.Set(value);
  }

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 0 : 1];
  }
}
