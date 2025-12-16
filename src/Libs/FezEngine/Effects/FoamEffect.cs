// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.FoamEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;

#nullable disable
namespace FezEngine.Effects;

public class FoamEffect : BaseEffect
{
  private readonly SemanticMappedSingle timeAccumulator;
  private readonly SemanticMappedSingle shoreTotalWidth;
  private readonly SemanticMappedSingle screenCenterSide;
  private readonly SemanticMappedBoolean isEmerged;
  private readonly SemanticMappedBoolean isWobbling;

  public FoamEffect()
    : base(nameof (FoamEffect))
  {
    this.timeAccumulator = new SemanticMappedSingle(this.effect.Parameters, nameof (TimeAccumulator));
    this.shoreTotalWidth = new SemanticMappedSingle(this.effect.Parameters, nameof (ShoreTotalWidth));
    this.screenCenterSide = new SemanticMappedSingle(this.effect.Parameters, nameof (ScreenCenterSide));
    this.isEmerged = new SemanticMappedBoolean(this.effect.Parameters, "IsEmerged");
    this.isWobbling = new SemanticMappedBoolean(this.effect.Parameters, nameof (IsWobbling));
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    this.isEmerged.Set((bool) group.CustomData);
    this.material.Diffuse = group.Material.Diffuse;
    this.material.Opacity = group.Material.Opacity;
  }

  public float TimeAccumulator
  {
    set => this.timeAccumulator.Set(value);
  }

  public float ShoreTotalWidth
  {
    set => this.shoreTotalWidth.Set(value);
  }

  public float ScreenCenterSide
  {
    set => this.screenCenterSide.Set(value);
  }

  public bool IsWobbling
  {
    set => this.isWobbling.Set(value);
  }
}
