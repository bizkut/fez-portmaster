// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.ShardNoteComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class ShardNoteComparer : IEqualityComparer<ShardNotes>
{
  public static readonly ShardNoteComparer Default = new ShardNoteComparer();

  public bool Equals(ShardNotes x, ShardNotes y) => x == y;

  public int GetHashCode(ShardNotes obj) => (int) obj;
}
