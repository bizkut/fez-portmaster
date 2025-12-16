// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.InstancedAnimatedPlaneEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class InstancedAnimatedPlaneEffect : BaseEffect, IShaderInstantiatableEffect<Matrix>
{
  private readonly SemanticMappedTexture animatedTexture;
  private readonly SemanticMappedVector2 frameScale;
  private readonly SemanticMappedBoolean ignoreFog;
  private readonly SemanticMappedBoolean sewerHax;
  private readonly SemanticMappedBoolean ignoreShading;
  private readonly SemanticMappedMatrixArray instanceData;

  public InstancedAnimatedPlaneEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwInstancedAnimatedPlaneEffect" : nameof (InstancedAnimatedPlaneEffect))
  {
    this.animatedTexture = new SemanticMappedTexture(this.effect.Parameters, "AnimatedTexture");
    this.ignoreFog = new SemanticMappedBoolean(this.effect.Parameters, nameof (IgnoreFog));
    this.sewerHax = new SemanticMappedBoolean(this.effect.Parameters, "SewerHax");
    this.ignoreShading = new SemanticMappedBoolean(this.effect.Parameters, nameof (IgnoreShading));
    if (!BaseEffect.UseHardwareInstancing)
      this.instanceData = new SemanticMappedMatrixArray(this.effect.Parameters, "InstanceData");
    this.frameScale = new SemanticMappedVector2(this.effect.Parameters, "FrameScale");
    this.Pass = LightingEffectPass.Main;
  }

  public override void Prepare(Mesh mesh)
  {
    this.sewerHax.Set(this.LevelManager.WaterType == LiquidType.Sewer);
    this.matrices.WorldViewProjection = this.viewProjection;
    base.Prepare(mesh);
  }

  public override void Prepare(Group group)
  {
    this.animatedTexture.Set(group.Texture);
    this.frameScale.Set((Vector2) group.CustomData);
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

  public void SetInstanceData(Matrix[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }
}
