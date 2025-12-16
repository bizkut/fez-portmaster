// Decompiled with JetBrains decompiler
// Type: FezEngine.AxisComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine;

public class AxisComparer : IEqualityComparer<Axis>
{
  public static readonly AxisComparer Default = new AxisComparer();

  public bool Equals(Axis x, Axis y) => x == y;

  public int GetHashCode(Axis obj) => (int) obj;
}
