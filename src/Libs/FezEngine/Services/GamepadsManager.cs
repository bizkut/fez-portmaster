// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.GamepadsManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public class GamepadsManager : GameComponent, IGamepadsManager
{
  private readonly Dictionary<PlayerIndex, GamepadState> gamepadStates = new Dictionary<PlayerIndex, GamepadState>((IEqualityComparer<PlayerIndex>) PlayerIndexComparer.Default);

  public GamepadsManager(Game game, bool enabled = true)
    : base(game)
  {
    this.Enabled = enabled;
    if (!this.Enabled)
      return;
    this.gamepadStates.Add(PlayerIndex.One, new GamepadState(PlayerIndex.One));
    this.gamepadStates.Add(PlayerIndex.Two, new GamepadState(PlayerIndex.Two));
    this.gamepadStates.Add(PlayerIndex.Three, new GamepadState(PlayerIndex.Three));
    this.gamepadStates.Add(PlayerIndex.Four, new GamepadState(PlayerIndex.Four));
  }

  public override void Update(GameTime gameTime)
  {
    TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
    GamepadState.AnyConnected = false;
    this.gamepadStates[PlayerIndex.One].Update(elapsedGameTime);
    this.gamepadStates[PlayerIndex.Two].Update(elapsedGameTime);
    this.gamepadStates[PlayerIndex.Three].Update(elapsedGameTime);
    this.gamepadStates[PlayerIndex.Four].Update(elapsedGameTime);
  }

  public GamepadState this[PlayerIndex index] => this.gamepadStates[index];
}
