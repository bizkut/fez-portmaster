// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.FishEyeEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class FishEyeEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedVector2 intensity;

  public FishEyeEffect()
    : base("ScreenSpaceFisheye")
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.intensity = new SemanticMappedVector2(this.effect.Parameters, nameof (Intensity));
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.material.Diffuse = mesh.Material.Diffuse;
    this.material.Opacity = mesh.Material.Opacity;
    this.intensity.Set(new Vector2(this.Intensity / this.GraphicsDeviceService.GraphicsDevice.Viewport.AspectRatio, this.Intensity) * 0.05f);
    this.texture.Set((Texture) mesh.Texture);
  }

  public float Intensity { get; set; }
}
