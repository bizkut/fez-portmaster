// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.PlayerIndexComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class PlayerIndexComparer : IEqualityComparer<PlayerIndex>
{
  public static readonly PlayerIndexComparer Default = new PlayerIndexComparer();

  public bool Equals(PlayerIndex x, PlayerIndex y) => x == y;

  public int GetHashCode(PlayerIndex obj) => (int) obj;
}
