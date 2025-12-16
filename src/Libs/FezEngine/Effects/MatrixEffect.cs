// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.MatrixEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class MatrixEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedSingle maxHeight;
  private bool groupTextureDirty;
  private Vector3 lastDiffuse;

  public MatrixEffect()
    : base(nameof (MatrixEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.maxHeight = new SemanticMappedSingle(this.effect.Parameters, nameof (MaxHeight));
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.texture.Set((Texture) mesh.Texture);
    this.groupTextureDirty = false;
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    this.matrices.World = (Matrix) group.WorldMatrix;
    if (group.Material != null)
    {
      if (this.lastDiffuse != group.Material.Diffuse)
      {
        this.material.Diffuse = group.Material.Diffuse;
        this.lastDiffuse = group.Material.Diffuse;
      }
    }
    else
      this.material.Diffuse = group.Mesh.Material.Diffuse;
    if (group.TexturingType == TexturingType.Texture2D)
    {
      this.texture.Set(group.Texture);
      this.groupTextureDirty = true;
    }
    else
    {
      if (!this.groupTextureDirty)
        return;
      this.texture.Set((Texture) group.Mesh.Texture);
    }
  }

  public float MaxHeight
  {
    set => this.maxHeight.Set(value);
  }
}
