// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.AnimatedPlaneEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class AnimatedPlaneEffect : BaseEffect
{
  private readonly SemanticMappedTexture animatedTexture;
  private readonly SemanticMappedBoolean ignoreFog;
  private readonly SemanticMappedBoolean fullbright;
  private readonly SemanticMappedBoolean alphaIsEmissive;
  private readonly SemanticMappedBoolean ignoreShading;
  private readonly SemanticMappedBoolean sewerHax;

  public AnimatedPlaneEffect()
    : base(nameof (AnimatedPlaneEffect))
  {
    this.animatedTexture = new SemanticMappedTexture(this.effect.Parameters, "AnimatedTexture");
    this.ignoreFog = new SemanticMappedBoolean(this.effect.Parameters, nameof (IgnoreFog));
    this.fullbright = new SemanticMappedBoolean(this.effect.Parameters, "Fullbright");
    this.alphaIsEmissive = new SemanticMappedBoolean(this.effect.Parameters, "AlphaIsEmissive");
    this.ignoreShading = new SemanticMappedBoolean(this.effect.Parameters, nameof (IgnoreShading));
    this.sewerHax = new SemanticMappedBoolean(this.effect.Parameters, "SewerHax");
    this.Pass = LightingEffectPass.Main;
  }

  public override void Prepare(Mesh mesh)
  {
    this.sewerHax.Set(this.LevelManager.WaterType == LiquidType.Sewer);
    base.Prepare(mesh);
    if (!this.ForcedViewMatrix.HasValue)
      return;
    Matrix? nullable = this.ForcedProjectionMatrix;
    if (!nullable.HasValue)
      return;
    MatricesEffectStructure matrices = this.matrices;
    nullable = this.ForcedViewMatrix;
    Matrix matrix1 = nullable.Value;
    nullable = this.ForcedProjectionMatrix;
    Matrix matrix2 = nullable.Value;
    Matrix matrix3 = matrix1 * matrix2;
    matrices.ViewProjection = matrix3;
  }

  public override void Prepare(Group group)
  {
    if (this.IgnoreCache || !group.EffectOwner || group.InverseTransposeWorldMatrix.Dirty)
    {
      this.matrices.WorldInverseTranspose = (Matrix) group.InverseTransposeWorldMatrix;
      group.InverseTransposeWorldMatrix.Clean();
    }
    this.matrices.World = (Matrix) group.WorldMatrix;
    if (group.TextureMatrix.Value.HasValue)
    {
      this.matrices.TextureMatrix = group.TextureMatrix.Value.Value;
      this.textureMatrixDirty = true;
    }
    else if (this.textureMatrixDirty)
    {
      this.matrices.TextureMatrix = Matrix.Identity;
      this.textureMatrixDirty = false;
    }
    this.animatedTexture.Set(group.Texture);
    this.material.Diffuse = group.Material.Diffuse;
    this.material.Opacity = group.Material.Opacity;
    if (group.CustomData is PlaneCustomData customData)
    {
      this.fullbright.Set(customData.Fullbright);
      this.alphaIsEmissive.Set(customData.AlphaIsEmissive);
    }
    else
    {
      this.fullbright.Set(false);
      this.alphaIsEmissive.Set(false);
    }
  }

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 0 : 1];
  }

  public bool IgnoreFog
  {
    set => this.ignoreFog.Set(value);
  }

  public bool IgnoreShading
  {
    set => this.ignoreShading.Set(value);
  }
}
