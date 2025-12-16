// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.IInputManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

public interface IInputManager
{
  event Action<PlayerIndex> ActiveControllerDisconnected;

  ControllerIndex ActiveControllers { get; }

  FezButtonState Up { get; }

  FezButtonState Down { get; }

  FezButtonState Left { get; }

  FezButtonState Right { get; }

  Vector2 Movement { get; }

  Vector2 FreeLook { get; }

  FezButtonState GrabThrow { get; }

  FezButtonState Jump { get; }

  FezButtonState CancelTalk { get; }

  FezButtonState Start { get; }

  FezButtonState Back { get; }

  FezButtonState OpenInventory { get; }

  FezButtonState RotateLeft { get; }

  FezButtonState RotateRight { get; }

  FezButtonState ClampLook { get; }

  FezButtonState FpsToggle { get; }

  FezButtonState MapZoomIn { get; }

  FezButtonState MapZoomOut { get; }

  FezButtonState ExactUp { get; }

  bool AnyButtonPressed();

  bool StrictRotation { get; set; }

  void ForceActiveController(ControllerIndex ci);

  void DetermineActiveController();

  void ClearActiveController();

  void SaveState();

  void RecoverState();

  void Reset();

  void PressedToDown();

  GamepadState ActiveGamepad { get; }
}
