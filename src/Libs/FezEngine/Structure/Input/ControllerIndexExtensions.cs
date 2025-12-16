// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.ControllerIndexExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure.Input;

public static class ControllerIndexExtensions
{
  private static readonly PlayerIndex[] None = new PlayerIndex[0];
  private static readonly PlayerIndex[] One = new PlayerIndex[1];
  private static readonly PlayerIndex[] Two = new PlayerIndex[1]
  {
    PlayerIndex.Two
  };
  private static readonly PlayerIndex[] Three = new PlayerIndex[1]
  {
    PlayerIndex.Three
  };
  private static readonly PlayerIndex[] Four = new PlayerIndex[1]
  {
    PlayerIndex.Four
  };
  private static readonly PlayerIndex[] Any = new PlayerIndex[4]
  {
    PlayerIndex.One,
    PlayerIndex.Two,
    PlayerIndex.Three,
    PlayerIndex.Four
  };

  public static PlayerIndex GetPlayer(this ControllerIndex index)
  {
    switch (index)
    {
      case ControllerIndex.One:
        return PlayerIndex.One;
      case ControllerIndex.Two:
        return PlayerIndex.Two;
      case ControllerIndex.Three:
        return PlayerIndex.Three;
      case ControllerIndex.Four:
        return PlayerIndex.Four;
      default:
        return PlayerIndex.One;
    }
  }

  public static PlayerIndex[] GetPlayers(this ControllerIndex index)
  {
    switch (index)
    {
      case ControllerIndex.None:
        return ControllerIndexExtensions.None;
      case ControllerIndex.One:
        return ControllerIndexExtensions.One;
      case ControllerIndex.Two:
        return ControllerIndexExtensions.Two;
      case ControllerIndex.Three:
        return ControllerIndexExtensions.Three;
      case ControllerIndex.Four:
        return ControllerIndexExtensions.Four;
      case ControllerIndex.Any:
        return ControllerIndexExtensions.Any;
      default:
        return ControllerIndexExtensions.None;
    }
  }

  public static ControllerIndex ToControllerIndex(this PlayerIndex index)
  {
    switch (index)
    {
      case PlayerIndex.One:
        return ControllerIndex.One;
      case PlayerIndex.Two:
        return ControllerIndex.Two;
      case PlayerIndex.Three:
        return ControllerIndex.Three;
      case PlayerIndex.Four:
        return ControllerIndex.Four;
      default:
        return ControllerIndex.None;
    }
  }
}
