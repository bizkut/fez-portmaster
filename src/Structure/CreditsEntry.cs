// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.CreditsEntry
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezGame.Structure;

internal class CreditsEntry
{
  public bool IsTitle;
  public bool IsSubtitle;
  public Texture2D Image;
  public string Text;
  public Color Color = Color.White;
  public Vector2 Size;
}
