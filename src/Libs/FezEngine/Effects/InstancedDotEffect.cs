// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.InstancedDotEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class InstancedDotEffect : BaseEffect, IShaderInstantiatableEffect<Vector4>
{
  private readonly SemanticMappedSingle theta;
  private readonly SemanticMappedSingle eightShapeStep;
  private readonly SemanticMappedSingle distanceFactor;
  private readonly SemanticMappedSingle immobilityFactor;
  private readonly SemanticMappedVectorArray instanceData;

  public InstancedDotEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwInstancedDotEffect" : nameof (InstancedDotEffect))
  {
    this.theta = new SemanticMappedSingle(this.effect.Parameters, nameof (Theta));
    this.eightShapeStep = new SemanticMappedSingle(this.effect.Parameters, nameof (EightShapeStep));
    this.distanceFactor = new SemanticMappedSingle(this.effect.Parameters, nameof (DistanceFactor));
    this.immobilityFactor = new SemanticMappedSingle(this.effect.Parameters, nameof (ImmobilityFactor));
    if (!BaseEffect.UseHardwareInstancing)
      this.instanceData = new SemanticMappedVectorArray(this.effect.Parameters, "InstanceData");
    this.SimpleGroupPrepare = true;
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.material.Diffuse = mesh.Material.Diffuse;
    this.material.Opacity = mesh.Material.Opacity;
  }

  public void SetInstanceData(Vector4[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }

  public float Theta
  {
    set => this.theta.Set(value);
  }

  public float EightShapeStep
  {
    set => this.eightShapeStep.Set(value);
  }

  public float DistanceFactor
  {
    set => this.distanceFactor.Set(value);
  }

  public float ImmobilityFactor
  {
    set => this.immobilityFactor.Set(value);
  }
}
