// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.RebootPOSTEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class RebootPOSTEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedMatrix pseudoWorldMatrix;

  public RebootPOSTEffect()
    : base(nameof (RebootPOSTEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.pseudoWorldMatrix = new SemanticMappedMatrix(this.effect.Parameters, "PseudoWorldMatrix");
    this.PseudoWorld = Matrix.Identity;
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.material.Diffuse = mesh.Material.Diffuse;
    this.material.Opacity = mesh.Material.Opacity;
    this.texture.Set((Texture) mesh.Texture);
  }

  public Matrix PseudoWorld
  {
    set => this.pseudoWorldMatrix.Set(value);
  }
}
