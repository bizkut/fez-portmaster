// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.TimeHost
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

public class TimeHost(Game game) : GameComponent(game)
{
  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.TimePaused || this.EngineState.Loading)
      return;
    this.TimeManager.CurrentTime += TimeSpan.FromMilliseconds(gameTime.ElapsedGameTime.TotalMilliseconds * (double) this.TimeManager.TimeFactor);
    this.TimeManager.OnTick();
  }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }
}
