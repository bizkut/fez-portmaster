// Decompiled with JetBrains decompiler
// Type: FezEngine.CollisionTypeComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine;

public class CollisionTypeComparer : IEqualityComparer<CollisionType>
{
  public static readonly CollisionTypeComparer Default = new CollisionTypeComparer();

  public bool Equals(CollisionType x, CollisionType y) => x == y;

  public int GetHashCode(CollisionType obj) => (int) obj;
}
