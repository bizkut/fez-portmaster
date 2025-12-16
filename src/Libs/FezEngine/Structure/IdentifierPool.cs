// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.IdentifierPool
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure;

public class IdentifierPool
{
  private int maximum;
  private readonly List<int> available = new List<int>();

  public int Take()
  {
    if (this.available.Count == 0)
      this.available.Add(this.maximum++);
    return this.available.First<int>();
  }

  public void Return(int id) => this.available.Add(id);

  public static int FirstAvailable<T>(IDictionary<int, T> values)
  {
    int val1 = -1;
    foreach (int key in (IEnumerable<int>) values.Keys)
      val1 = Math.Max(val1, key);
    return val1 + 1;
  }
}
