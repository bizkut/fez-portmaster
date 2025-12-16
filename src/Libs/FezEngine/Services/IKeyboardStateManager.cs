// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IKeyboardStateManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#nullable disable
namespace FezEngine.Services;

public interface IKeyboardStateManager
{
  bool IgnoreMapping { get; set; }

  FezButtonState GetKeyState(Keys key);

  void RegisterKey(Keys key);

  void UpdateMapping();

  FezButtonState Jump { get; }

  FezButtonState GrabThrow { get; }

  FezButtonState CancelTalk { get; }

  FezButtonState Up { get; }

  FezButtonState Down { get; }

  FezButtonState Left { get; }

  FezButtonState Right { get; }

  FezButtonState LookUp { get; }

  FezButtonState LookDown { get; }

  FezButtonState LookRight { get; }

  FezButtonState LookLeft { get; }

  FezButtonState OpenMap { get; }

  FezButtonState MapZoomIn { get; }

  FezButtonState MapZoomOut { get; }

  FezButtonState Pause { get; }

  FezButtonState OpenInventory { get; }

  FezButtonState RotateLeft { get; }

  FezButtonState RotateRight { get; }

  FezButtonState FpViewToggle { get; }

  FezButtonState ClampLook { get; }

  void Update(KeyboardState state, GameTime time);
}
