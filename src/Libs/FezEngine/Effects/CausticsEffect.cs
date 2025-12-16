// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.CausticsEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Effects;

public class CausticsEffect : BaseEffect
{
  private readonly SemanticMappedTexture animatedTexture;
  private readonly SemanticMappedMatrix nextFrameTextureMatrix;

  public CausticsEffect()
    : base(nameof (CausticsEffect))
  {
    this.animatedTexture = new SemanticMappedTexture(this.effect.Parameters, "AnimatedTexture");
    this.nextFrameTextureMatrix = new SemanticMappedMatrix(this.effect.Parameters, "NextFrameData");
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    if (mesh.CustomData == null)
      return;
    this.nextFrameTextureMatrix.Set((Matrix) mesh.CustomData);
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    if (this.IgnoreCache || !group.EffectOwner || group.InverseTransposeWorldMatrix.Dirty)
    {
      this.matrices.WorldInverseTranspose = (Matrix) group.InverseTransposeWorldMatrix;
      group.InverseTransposeWorldMatrix.Clean();
    }
    this.material.Diffuse = group.Material.Diffuse;
    this.animatedTexture.Set(group.Texture);
  }
}
