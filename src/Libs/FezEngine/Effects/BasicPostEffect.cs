// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.BasicPostEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class BasicPostEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;

  public BasicPostEffect()
    : base(nameof (BasicPostEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.material.Diffuse = mesh.Material.Diffuse;
    this.material.Opacity = mesh.Material.Opacity;
    this.texture.Set((Texture) mesh.Texture);
  }
}
