// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.HorizontalTrailsEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;

#nullable disable
namespace FezEngine.Effects;

public class HorizontalTrailsEffect : BaseEffect
{
  private SemanticMappedSingle timing;
  private SemanticMappedVector3 right;

  public HorizontalTrailsEffect()
    : base(nameof (HorizontalTrailsEffect))
  {
    this.timing = new SemanticMappedSingle(this.effect.Parameters, nameof (Timing));
    this.right = new SemanticMappedVector3(this.effect.Parameters, "Right");
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.right.Set(this.CameraProvider.InverseView.Right);
  }

  public float Timing
  {
    get => this.timing.Get();
    set => this.timing.Set(value);
  }
}
