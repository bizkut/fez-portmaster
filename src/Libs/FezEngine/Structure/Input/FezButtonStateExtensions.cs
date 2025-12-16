// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.FezButtonStateExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Structure.Input;

public static class FezButtonStateExtensions
{
  public static bool IsDown(this FezButtonState state)
  {
    return state == FezButtonState.Pressed || state == FezButtonState.Down;
  }

  public static FezButtonState NextState(this FezButtonState state, bool pressed)
  {
    switch (state)
    {
      case FezButtonState.Up:
        return !pressed ? FezButtonState.Up : FezButtonState.Pressed;
      case FezButtonState.Pressed:
        return !pressed ? FezButtonState.Released : FezButtonState.Down;
      case FezButtonState.Released:
        return !pressed ? FezButtonState.Up : FezButtonState.Pressed;
      default:
        return !pressed ? FezButtonState.Released : FezButtonState.Down;
    }
  }
}
