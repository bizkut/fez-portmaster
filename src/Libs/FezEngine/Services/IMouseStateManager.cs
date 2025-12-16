// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IMouseStateManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public interface IMouseStateManager
{
  MouseButtonState LeftButton { get; }

  MouseButtonState MiddleButton { get; }

  MouseButtonState RightButton { get; }

  int WheelTurns { get; }

  FezButtonState WheelTurnedUp { get; }

  FezButtonState WheelTurnedDown { get; }

  Point Position { get; }

  Point Movement { get; }

  void Update(GameTime time);

  IntPtr RenderPanelHandle { set; }

  IntPtr ParentFormHandle { set; }
}
