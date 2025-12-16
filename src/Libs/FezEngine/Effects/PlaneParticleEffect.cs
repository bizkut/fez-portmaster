// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.PlaneParticleEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class PlaneParticleEffect : BaseEffect, IShaderInstantiatableEffect<Matrix>
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedBoolean additive;
  private readonly SemanticMappedBoolean fullbright;
  private readonly SemanticMappedMatrixArray instanceData;

  public Matrix? ForcedViewProjection { get; set; }

  public PlaneParticleEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwPlaneParticleEffect" : nameof (PlaneParticleEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.additive = new SemanticMappedBoolean(this.effect.Parameters, nameof (Additive));
    this.fullbright = new SemanticMappedBoolean(this.effect.Parameters, nameof (Fullbright));
    if (!BaseEffect.UseHardwareInstancing)
      this.instanceData = new SemanticMappedMatrixArray(this.effect.Parameters, "InstanceData");
    this.Pass = LightingEffectPass.Main;
    this.SimpleMeshPrepare = this.SimpleGroupPrepare = true;
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.texture.Set((Texture) mesh.Texture);
    if (!this.ForcedViewProjection.HasValue)
      return;
    this.matrices.WorldViewProjection = this.ForcedViewProjection.Value;
  }

  public void SetInstanceData(Matrix[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 0 : 1];
  }

  public bool Additive
  {
    set => this.additive.Set(value);
  }

  public bool Fullbright
  {
    set => this.fullbright.Set(value);
  }
}
