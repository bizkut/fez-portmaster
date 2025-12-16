// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.FakePointSpritesEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class FakePointSpritesEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedSingle viewScale;
  private bool groupTextureDirty;

  public FakePointSpritesEffect()
    : base(nameof (FakePointSpritesEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.viewScale = new SemanticMappedSingle(this.effect.Parameters, "ViewScale");
  }

  public override BaseEffect Clone() => (BaseEffect) new FakePointSpritesEffect();

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.texture.Set((Texture) mesh.Texture);
    this.viewScale.Set(this.GraphicsDeviceService.GraphicsDevice.GetViewScale());
    this.groupTextureDirty = false;
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
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
}
