// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.DisplayModeEqualityComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

internal class DisplayModeEqualityComparer : IEqualityComparer<DisplayMode>
{
  public static readonly DisplayModeEqualityComparer Default = new DisplayModeEqualityComparer();

  public bool Equals(DisplayMode x, DisplayMode y) => x.Width == y.Width && x.Height == y.Height;

  public int GetHashCode(DisplayMode obj)
  {
    int num = obj.Width;
    int hashCode1 = num.GetHashCode();
    num = obj.Height;
    int hashCode2 = num.GetHashCode();
    return Util.CombineHashCodes(hashCode1, hashCode2);
  }
}
