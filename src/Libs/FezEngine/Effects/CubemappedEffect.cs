// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.CubemappedEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class CubemappedEffect : BaseEffect
{
  private readonly SemanticMappedTexture cubemap;
  private readonly SemanticMappedBoolean forceShading;

  public CubemappedEffect()
    : base(nameof (CubemappedEffect))
  {
    this.cubemap = new SemanticMappedTexture(this.effect.Parameters, "CubemapTexture");
    this.forceShading = new SemanticMappedBoolean(this.effect.Parameters, nameof (ForceShading));
    this.Pass = LightingEffectPass.Main;
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    if (this.IgnoreCache || !group.EffectOwner || group.InverseTransposeWorldMatrix.Dirty)
    {
      this.matrices.WorldInverseTranspose = (Matrix) group.InverseTransposeWorldMatrix;
      group.InverseTransposeWorldMatrix.Clean();
    }
    this.cubemap.Set(group.Texture);
    this.material.Diffuse = group.Material != null ? group.Material.Diffuse : group.Mesh.Material.Diffuse;
  }

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 0 : 1];
  }

  public bool ForceShading
  {
    set => this.forceShading.Set(value);
  }
}
