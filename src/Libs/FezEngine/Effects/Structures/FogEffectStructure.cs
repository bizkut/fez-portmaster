// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.FogEffectStructure
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects.Structures;

internal class FogEffectStructure
{
  private readonly SemanticMappedInt32 fogType;
  private readonly SemanticMappedVector3 fogColor;
  private readonly SemanticMappedSingle fogDensity;

  public FogEffectStructure(EffectParameterCollection parameters)
  {
    this.fogType = new SemanticMappedInt32(parameters, "Fog_Type");
    this.fogColor = new SemanticMappedVector3(parameters, "Fog_Color");
    this.fogDensity = new SemanticMappedSingle(parameters, "Fog_Density");
  }

  public FogType FogType
  {
    set => this.fogType.Set((int) value);
  }

  public Color FogColor
  {
    set => this.fogColor.Set(value.ToVector3());
  }

  public float FogDensity
  {
    set => this.fogDensity.Set(value);
  }
}
