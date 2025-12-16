// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.InstancedArtObjectEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class InstancedArtObjectEffect : BaseEffect, IShaderInstantiatableEffect<Matrix>
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedMatrixArray instanceData;

  public InstancedArtObjectEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwInstancedArtObjectEffect" : nameof (InstancedArtObjectEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "CubemapTexture");
    if (!BaseEffect.UseHardwareInstancing)
      this.instanceData = new SemanticMappedMatrixArray(this.effect.Parameters, "InstanceData");
    this.Pass = LightingEffectPass.Main;
    this.SimpleMeshPrepare = this.SimpleGroupPrepare = true;
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    this.texture.Set(group.Texture);
  }

  public void SetInstanceData(Matrix[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 0 : 1];
  }
}
