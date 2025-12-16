// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Material
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public class Material
{
  public Vector3 Diffuse;
  public float Opacity;

  public Material()
  {
    this.Diffuse = Vector3.One;
    this.Opacity = 1f;
  }

  public Material Clone()
  {
    return new Material()
    {
      Diffuse = this.Diffuse,
      Opacity = this.Opacity
    };
  }
}
