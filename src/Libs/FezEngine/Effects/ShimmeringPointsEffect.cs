// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.ShimmeringPointsEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class ShimmeringPointsEffect : BaseEffect
{
  private readonly SemanticMappedVector3 randomSeed;
  private readonly SemanticMappedSingle saturation;

  public ShimmeringPointsEffect()
    : base(nameof (ShimmeringPointsEffect))
  {
    this.randomSeed = new SemanticMappedVector3(this.effect.Parameters, "RandomSeed");
    this.saturation = new SemanticMappedSingle(this.effect.Parameters, nameof (Saturation));
    this.saturation.Set(1f);
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.randomSeed.Set(new Vector3(RandomHelper.Unit(), RandomHelper.Unit(), RandomHelper.Unit()));
    this.material.Diffuse = mesh.Material.Diffuse;
    this.material.Opacity = mesh.Material.Opacity;
  }

  public float Saturation
  {
    set => this.saturation.Set(value);
  }
}
