// Decompiled with JetBrains decompiler
// Type: Common.HandlePair`2
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

using System;

#nullable disable
namespace Common;

internal struct HandlePair<T, U> : IEquatable<HandlePair<T, U>>
{
  private readonly T first;
  private readonly U second;
  private readonly int hash;

  public HandlePair(T first, U second)
  {
    this.first = first;
    this.second = second;
    this.hash = 27232 + first.GetHashCode();
  }

  public override int GetHashCode() => this.hash;

  public override bool Equals(object obj)
  {
    return obj != null && obj is HandlePair<T, U> other && this.Equals(other);
  }

  public bool Equals(HandlePair<T, U> other)
  {
    return other.first.Equals((object) this.first) && other.second.Equals((object) this.second);
  }
}
