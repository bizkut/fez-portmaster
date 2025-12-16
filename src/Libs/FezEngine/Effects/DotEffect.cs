// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.DotEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Effects;

public class DotEffect : BaseEffect
{
  private readonly SemanticMappedSingle HueOffset;
  private float hueOffset;
  private Vector3 lastDiffuse;

  public float ShiftSpeed { get; set; }

  public float AdditionalOffset { get; set; }

  public DotEffect()
    : base(nameof (DotEffect))
  {
    this.HueOffset = new SemanticMappedSingle(this.effect.Parameters, nameof (HueOffset));
    this.ShiftSpeed = 1f;
  }

  public void UpdateHueOffset(TimeSpan elapsed)
  {
    this.hueOffset += 0.05f * this.ShiftSpeed * (float) elapsed.TotalSeconds;
    float num = this.hueOffset + this.AdditionalOffset * 360f;
    while ((double) num >= 360.0)
      num -= 360f;
    while ((double) num < 0.0)
      num += 360f;
    this.HueOffset.Set(num);
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
}
