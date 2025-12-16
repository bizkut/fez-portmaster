// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.SplitCollectorEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;

#nullable disable
namespace FezEngine.Effects;

public class SplitCollectorEffect : BaseEffect
{
  private readonly SemanticMappedSingle varyingOpacity;
  private readonly SemanticMappedSingle offset;

  public SplitCollectorEffect()
    : base(nameof (SplitCollectorEffect))
  {
    this.varyingOpacity = new SemanticMappedSingle(this.effect.Parameters, nameof (VaryingOpacity));
    this.offset = new SemanticMappedSingle(this.effect.Parameters, nameof (Offset));
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.matrices.World = mesh.WorldMatrix;
    this.material.Diffuse = mesh.Material.Diffuse;
  }

  public float VaryingOpacity
  {
    set => this.varyingOpacity.Set(value);
  }

  public float Offset
  {
    set => this.offset.Set(value);
  }
}
