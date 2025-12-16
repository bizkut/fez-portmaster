// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.IFontManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Components;

public interface IFontManager
{
  SpriteFont Big { get; }

  SpriteFont Small { get; }

  float SmallFactor { get; }

  float BigFactor { get; }

  float TopSpacing { get; }

  float SideSpacing { get; }
}
