// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.SkyLayer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;

#nullable disable
namespace FezEngine.Structure;

public class SkyLayer
{
  [Serialization(Optional = true)]
  public string Name { get; set; }

  [Serialization(Optional = true)]
  public bool InFront { get; set; }

  [Serialization(Optional = true)]
  public float Opacity { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public float FogTint { get; set; }

  public SkyLayer() => this.Opacity = 1f;

  public SkyLayer ShallowCopy()
  {
    return new SkyLayer()
    {
      Name = this.Name,
      InFront = this.InFront,
      Opacity = this.Opacity,
      FogTint = this.FogTint
    };
  }

  public void UpdateFromCopy(SkyLayer copy)
  {
    this.Name = copy.Name;
    this.InFront = copy.InFront;
    this.Opacity = copy.Opacity;
    this.FogTint = copy.FogTint;
  }
}
