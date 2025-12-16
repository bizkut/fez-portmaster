// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IGamepadsManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Services;

public interface IGamepadsManager
{
  GamepadState this[PlayerIndex index] { get; }
}
