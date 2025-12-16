// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.NpcActionExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Structure;

public static class NpcActionExtensions
{
  public static bool AllowsRandomChange(this NpcAction action)
  {
    switch (action)
    {
      case NpcAction.Idle:
      case NpcAction.Idle3:
      case NpcAction.Walk:
        return true;
      default:
        return false;
    }
  }

  public static bool Loops(this NpcAction action)
  {
    switch (action)
    {
      case NpcAction.Idle2:
      case NpcAction.Turn:
      case NpcAction.Burrow:
      case NpcAction.Hide:
      case NpcAction.ComeOut:
      case NpcAction.TakeOff:
      case NpcAction.Land:
        return false;
      default:
        return true;
    }
  }

  public static bool IsSpecialIdle(this NpcAction action)
  {
    return action == NpcAction.Idle2 || action == NpcAction.Idle3;
  }
}
