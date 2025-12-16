// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.ActionTypeComparer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using System.Collections.Generic;

#nullable disable
namespace FezGame.Structure;

public class ActionTypeComparer : IEqualityComparer<ActionType>
{
  public static readonly ActionTypeComparer Default = new ActionTypeComparer();

  public bool Equals(ActionType x, ActionType y) => x == y;

  public int GetHashCode(ActionType obj) => (int) obj;
}
