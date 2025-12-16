// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.InstancedStaticPlaneEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class InstancedStaticPlaneEffect : BaseEffect, IShaderInstantiatableEffect<Matrix>
{
  private readonly SemanticMappedTexture baseTexture;
  private readonly SemanticMappedBoolean ignoreFog;
  private readonly SemanticMappedBoolean sewerHax;
  private readonly SemanticMappedMatrixArray instanceData;

  public InstancedStaticPlaneEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwInstancedStaticPlaneEffect" : nameof (InstancedStaticPlaneEffect))
  {
    this.baseTexture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.ignoreFog = new SemanticMappedBoolean(this.effect.Parameters, nameof (IgnoreFog));
    this.sewerHax = new SemanticMappedBoolean(this.effect.Parameters, "SewerHax");
    if (!BaseEffect.UseHardwareInstancing)
      this.instanceData = new SemanticMappedMatrixArray(this.effect.Parameters, "InstanceData");
    this.Pass = LightingEffectPass.Main;
  }

  public override void Prepare(Mesh mesh)
  {
    this.sewerHax.Set(this.LevelManager.WaterType == LiquidType.Sewer);
    this.matrices.WorldViewProjection = this.viewProjection;
    base.Prepare(mesh);
  }

  public override void Prepare(Group group) => this.baseTexture.Set(group.Texture);

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 0 : 1];
  }

  public bool IgnoreFog
  {
    set => this.ignoreFog.Set(value);
  }

  public void SetInstanceData(Matrix[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }
}
