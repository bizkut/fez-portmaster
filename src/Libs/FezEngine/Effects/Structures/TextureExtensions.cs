// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.TextureExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Effects.Structures;

public static class TextureExtensions
{
  public static void Unhook(this Texture texture)
  {
    if (texture.Tag == null)
      return;
    foreach (SemanticMappedParameter<Texture> semanticMappedParameter in texture.Tag as HashSet<SemanticMappedTexture>)
      semanticMappedParameter.Set((Texture) null);
    texture.Tag = (object) null;
  }
}
