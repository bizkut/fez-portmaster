// Decompiled with JetBrains decompiler
// Type: FezEngine.VerticalDirectionComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine;

public class VerticalDirectionComparer : IEqualityComparer<VerticalDirection>
{
  public static readonly VerticalDirectionComparer Default = new VerticalDirectionComparer();

  public bool Equals(VerticalDirection x, VerticalDirection y) => x == y;

  public int GetHashCode(VerticalDirection obj) => (int) obj;
}
