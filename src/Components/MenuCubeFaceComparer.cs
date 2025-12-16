// Decompiled with JetBrains decompiler
// Type: FezGame.Components.MenuCubeFaceComparer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class MenuCubeFaceComparer : IEqualityComparer<MenuCubeFace>
{
  public static readonly MenuCubeFaceComparer Default = new MenuCubeFaceComparer();

  public bool Equals(MenuCubeFace x, MenuCubeFace y) => x == y;

  public int GetHashCode(MenuCubeFace obj) => (int) obj;
}
