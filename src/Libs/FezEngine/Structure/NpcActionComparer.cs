// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.NpcActionComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

internal class NpcActionComparer : IEqualityComparer<NpcAction>
{
  public static readonly NpcActionComparer Default = new NpcActionComparer();

  public bool Equals(NpcAction x, NpcAction y) => x == y;

  public int GetHashCode(NpcAction obj) => (int) obj;
}
