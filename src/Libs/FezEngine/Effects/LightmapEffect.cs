// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.LightmapEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class LightmapEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedBoolean shadowPass;

  public LightmapEffect()
    : base(nameof (LightmapEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.shadowPass = new SemanticMappedBoolean(this.effect.Parameters, nameof (ShadowPass));
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.texture.Set((Texture) mesh.Texture);
  }

  public bool ShadowPass
  {
    get => this.shadowPass.Get();
    set => this.shadowPass.Set(value);
  }
}
