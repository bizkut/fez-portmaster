// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.FarawayEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class FarawayEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedSingle actualOpacity;

  public FarawayEffect()
    : base(nameof (FarawayEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.actualOpacity = new SemanticMappedSingle(this.effect.Parameters, nameof (ActualOpacity));
    this.ActualOpacity = 1f;
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.material.Diffuse = mesh.Material.Diffuse;
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    this.material.Opacity = group.Material.Opacity;
    this.texture.Set(group.Texture);
  }

  public void CleanUp() => this.texture.Set((Texture) null);

  public float ActualOpacity
  {
    set => this.actualOpacity.Set(value);
  }
}
