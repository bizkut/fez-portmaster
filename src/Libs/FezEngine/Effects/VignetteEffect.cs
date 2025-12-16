// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.VignetteEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;

#nullable disable
namespace FezEngine.Effects;

public class VignetteEffect : BaseEffect
{
  private readonly SemanticMappedSingle sinceStarted;

  public VignetteEffect()
    : base(nameof (VignetteEffect))
  {
    this.sinceStarted = new SemanticMappedSingle(this.effect.Parameters, nameof (SinceStarted));
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.material.Opacity = mesh.Material.Opacity;
  }

  public float SinceStarted
  {
    set => this.sinceStarted.Set(value);
    get => this.sinceStarted.Get();
  }
}
