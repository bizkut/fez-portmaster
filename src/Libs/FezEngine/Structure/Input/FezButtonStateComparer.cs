// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.FezButtonStateComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure.Input;

public class FezButtonStateComparer : IEqualityComparer<FezButtonState>
{
  public static readonly FezButtonStateComparer Default = new FezButtonStateComparer();

  public bool Equals(FezButtonState x, FezButtonState y) => x == y;

  public int GetHashCode(FezButtonState obj) => (int) obj;
}
