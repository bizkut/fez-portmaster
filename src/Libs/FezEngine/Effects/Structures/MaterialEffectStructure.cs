// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.MaterialEffectStructure
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects.Structures;

internal class MaterialEffectStructure
{
  private readonly SemanticMappedVector3 diffuse;
  private readonly SemanticMappedSingle opacity;

  public MaterialEffectStructure(EffectParameterCollection parameters)
  {
    this.diffuse = new SemanticMappedVector3(parameters, "Material_Diffuse");
    this.opacity = new SemanticMappedSingle(parameters, "Material_Opacity");
  }

  public Vector3 Diffuse
  {
    set => this.diffuse.Set(value);
  }

  public float Opacity
  {
    set => this.opacity.Set(value);
  }
}
