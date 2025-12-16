// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.SkyBackEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class SkyBackEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;

  public SkyBackEffect()
    : base(nameof (SkyBackEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    if (!mesh.Texture.Dirty)
      return;
    this.texture.Set((Texture) mesh.Texture);
    mesh.Texture.Clean();
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    this.matrices.World = (Matrix) group.WorldMatrix;
  }
}
