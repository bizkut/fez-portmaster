// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.CamShake
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

public class CamShake : GameComponent
{
  private TimeSpan SinceStarted;
  private float RemainingDistance;

  public static CamShake CurrentCamShake { get; private set; }

  public CamShake(Game game)
    : base(game)
  {
    CamShake.CurrentCamShake = this;
  }

  public float Distance { private get; set; }

  public TimeSpan Duration { private get; set; }

  public bool IsDisposed { get; private set; }

  public void Reset() => this.SinceStarted = TimeSpan.Zero;

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    CamShake.CurrentCamShake = (CamShake) null;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.RightHigh, 1.0, this.Duration, EasingType.Linear);
    this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.LeftLow, 1.0, this.Duration, EasingType.Linear);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Loading || this.EngineState.Paused || this.EngineState.InMap)
      return;
    this.SinceStarted += gameTime.ElapsedGameTime;
    if (this.SinceStarted > this.Duration)
    {
      ServiceHelper.RemoveComponent<CamShake>(this);
      this.IsDisposed = true;
    }
    this.RemainingDistance = MathHelper.Lerp(this.Distance, 0.0f, (float) Math.Sqrt(this.SinceStarted.TotalSeconds / this.Duration.TotalSeconds));
    this.CameraManager.InterpolatedCenter += new Vector3(RandomHelper.Centered((double) this.RemainingDistance * 2.0), RandomHelper.Centered((double) this.RemainingDistance * 2.0), RandomHelper.Centered((double) this.RemainingDistance * 2.0));
  }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }
}
