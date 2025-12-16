// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.ChunkedTrixelComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

internal class ChunkedTrixelComparer : IComparer<TrixelEmplacement>
{
  public int Compare(TrixelEmplacement x, TrixelEmplacement y)
  {
    int num = x.X.CompareTo(y.X);
    if (num == 0)
    {
      num = x.Y.CompareTo(y.Y);
      if (num == 0)
        num = x.Z.CompareTo(y.Z);
    }
    return num;
  }
}
