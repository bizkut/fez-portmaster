// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.LiquidTypeComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class LiquidTypeComparer : IEqualityComparer<LiquidType>
{
  public static readonly LiquidTypeComparer Default = new LiquidTypeComparer();

  public bool Equals(LiquidType x, LiquidType y) => x == y;

  public int GetHashCode(LiquidType obj) => (int) obj;
}
