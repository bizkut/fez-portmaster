// Decompiled with JetBrains decompiler
// Type: FezEngine.HorizontalDirectionComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine;

public class HorizontalDirectionComparer : IEqualityComparer<HorizontalDirection>
{
  public static readonly HorizontalDirectionComparer Default = new HorizontalDirectionComparer();

  public bool Equals(HorizontalDirection x, HorizontalDirection y) => x == y;

  public int GetHashCode(HorizontalDirection obj) => (int) obj;
}
