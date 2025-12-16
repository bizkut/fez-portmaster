// Decompiled with JetBrains decompiler
// Type: FezGame.Components.WarpDestinationsComparer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class WarpDestinationsComparer : IEqualityComparer<WarpDestinations>
{
  public static readonly WarpDestinationsComparer Default = new WarpDestinationsComparer();

  public bool Equals(WarpDestinations x, WarpDestinations y) => x == y;

  public int GetHashCode(WarpDestinations obj) => (int) obj;
}
