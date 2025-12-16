// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.KeysComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public class KeysComparer : IComparer<Keys>
{
  public static readonly KeysComparer Default = new KeysComparer();

  public int Compare(Keys x, Keys y)
  {
    if (x < y)
      return -1;
    return x > y ? 1 : 0;
  }
}
