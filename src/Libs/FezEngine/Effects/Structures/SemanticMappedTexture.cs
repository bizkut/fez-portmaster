// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.SemanticMappedTexture
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Effects.Structures;

public class SemanticMappedTexture(EffectParameterCollection parent, string semanticName) : 
  SemanticMappedParameter<Texture>(parent, semanticName.Replace("Texture", "Sampler"))
{
  protected override void DoSet(Texture value)
  {
    if (value == null)
      this.parameter.SetValue(value);
    else if (value.IsDisposed)
    {
      this.parameter.SetValue((Texture) null);
    }
    else
    {
      HashSet<SemanticMappedTexture> semanticMappedTextureSet;
      if (value.Tag == null)
      {
        semanticMappedTextureSet = new HashSet<SemanticMappedTexture>();
        value.Tag = (object) semanticMappedTextureSet;
      }
      else
        semanticMappedTextureSet = value.Tag as HashSet<SemanticMappedTexture>;
      semanticMappedTextureSet?.Add(this);
      this.parameter.SetValue(value);
    }
  }
}
