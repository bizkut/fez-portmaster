// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.VibratingEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class VibratingEffect : BaseEffect
{
  private readonly SemanticMappedSingle intensity;
  private readonly SemanticMappedSingle timeStep;
  private readonly SemanticMappedSingle fogDensity;
  private Vector3 lastDiffuse;

  public VibratingEffect()
    : base(nameof (VibratingEffect))
  {
    this.intensity = new SemanticMappedSingle(this.effect.Parameters, nameof (Intensity));
    this.timeStep = new SemanticMappedSingle(this.effect.Parameters, nameof (TimeStep));
    this.fogDensity = new SemanticMappedSingle(this.effect.Parameters, nameof (FogDensity));
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    if (group.Material != null)
    {
      if (!(this.lastDiffuse != group.Material.Diffuse))
        return;
      this.material.Diffuse = group.Material.Diffuse;
      this.lastDiffuse = group.Material.Diffuse;
    }
    else
      this.material.Diffuse = group.Mesh.Material.Diffuse;
  }

  public float Intensity
  {
    get => this.intensity.Get();
    set => this.intensity.Set(value);
  }

  public float TimeStep
  {
    get => this.timeStep.Get();
    set => this.timeStep.Set(value);
  }

  public float FogDensity
  {
    get => this.fogDensity.Get();
    set => this.fogDensity.Set(value);
  }
}
