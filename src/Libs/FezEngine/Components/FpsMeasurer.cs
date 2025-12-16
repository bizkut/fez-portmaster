// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.FpsMeasurer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Components;

public class FpsMeasurer(Game game) : DrawableGameComponent(game)
{
  private double accumulatedTime;
  private int framesCounter;

  public override void Draw(GameTime gameTime)
  {
    this.accumulatedTime += gameTime.ElapsedGameTime.TotalSeconds;
    ++this.framesCounter;
    if (this.accumulatedTime < 1.0)
      return;
    this.EngineState.FramesPerSecond = (float) this.framesCounter / (float) this.accumulatedTime;
    --this.accumulatedTime;
    this.framesCounter = 0;
  }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }
}
