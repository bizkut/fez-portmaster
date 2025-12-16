// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.MouseButtonStatesComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure.Input;

public class MouseButtonStatesComparer : IEqualityComparer<MouseButtonStates>
{
  public static readonly MouseButtonStatesComparer Default = new MouseButtonStatesComparer();

  public bool Equals(MouseButtonStates x, MouseButtonStates y) => x == y;

  public int GetHashCode(MouseButtonStates obj) => (int) obj;
}
