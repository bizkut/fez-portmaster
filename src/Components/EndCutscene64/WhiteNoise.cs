// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene64.WhiteNoise
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components.EndCutscene64;

internal class WhiteNoise : DrawableGameComponent
{
  private Texture2D NoiseTexture;
  private readonly EndCutscene64Host Host;
  private float StepTime;
  private WhiteNoise.State ActiveState;
  private VignetteEffect VignetteEffect;
  private ScanlineEffect ScanlineEffect;
  private SoundEffect sTvOff;
  private SoundEmitter eNoise;
  private Matrix NoiseOffset;

  public WhiteNoise(Game game, EndCutscene64Host host)
    : base(game)
  {
    this.Host = host;
    this.DrawOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.VignetteEffect = new VignetteEffect();
      this.ScanlineEffect = new ScanlineEffect();
      this.NoiseTexture = this.CMProvider.Get(CM.EndCutscene).Load<Texture2D>("Other Textures/noise");
    }));
    this.sTvOff = this.CMProvider.Get(CM.EndCutscene).Load<SoundEffect>("Sounds/Ending/Cutscene64/TVOff");
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    Waiters.Wait(2.0, (Action) (() => this.CMProvider.Dispose(CM.EndCutscene)));
  }

  private void Reset()
  {
    this.eNoise = this.Host.eNoise;
    this.StepTime = 0.0f;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    if ((double) totalSeconds == 0.0)
      this.Reset();
    this.StepTime += totalSeconds;
    switch (this.ActiveState)
    {
      case WhiteNoise.State.Wait:
        this.VignetteEffect.SinceStarted = 0.0f;
        if ((double) this.StepTime <= 3.0)
          break;
        this.sTvOff.Emit();
        if (this.eNoise != null)
          this.eNoise.FadeOutAndDie(0.15f);
        this.ChangeState();
        break;
      case WhiteNoise.State.TVOff:
        this.VignetteEffect.SinceStarted = this.StepTime;
        if ((double) this.StepTime <= 6.5)
          break;
        this.ChangeState();
        this.GameState.SkyOpacity = 1f;
        break;
      case WhiteNoise.State.ToCredits:
        this.eNoise = (SoundEmitter) null;
        this.Host.Cycle();
        Waiters.Interpolate(1.0, (Action<float>) (s => PauseMenu.Starfield.Opacity = s));
        PauseMenu.Starfield.Opacity = 0.0f;
        this.GameState.Pause(true);
        break;
    }
  }

  private void ChangeState()
  {
    this.StepTime = 0.0f;
    ++this.ActiveState;
    this.Update(new GameTime());
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    switch (this.ActiveState)
    {
      case WhiteNoise.State.Wait:
      case WhiteNoise.State.TVOff:
        this.GraphicsDevice.Clear(Color.Black);
        int width = this.GraphicsDevice.Viewport.Width;
        int height = this.GraphicsDevice.Viewport.Height;
        this.NoiseOffset = new Matrix()
        {
          M11 = (float) width / 1024f,
          M22 = (float) height / 512f,
          M33 = 1f,
          M44 = 1f,
          M31 = RandomHelper.Unit(),
          M32 = RandomHelper.Unit()
        };
        this.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        this.TargetRenderer.DrawFullscreen((BaseEffect) this.ScanlineEffect, (Texture) this.NoiseTexture, new Matrix?(this.NoiseOffset), Color.White);
        if ((double) this.VignetteEffect.SinceStarted > 0.0)
          this.TargetRenderer.DrawFullscreen(new Color(1f, 1f, 1f, Easing.EaseOut((double) FezMath.Saturate(this.VignetteEffect.SinceStarted * 2f), EasingType.Quadratic)));
        this.GraphicsDevice.SetBlendingMode(BlendingMode.Multiply);
        this.TargetRenderer.DrawFullscreen((BaseEffect) this.VignetteEffect, new Color(1f, 1f, 1f, this.ActiveState == WhiteNoise.State.Wait ? 0.425f : 1f));
        this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
        if ((double) this.VignetteEffect.SinceStarted <= 1.0)
          break;
        this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, FezMath.Saturate(this.VignetteEffect.SinceStarted - 1f)));
        break;
      case WhiteNoise.State.ToCredits:
        this.GraphicsDevice.Clear(Color.Black);
        break;
    }
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  private enum State
  {
    Wait,
    TVOff,
    ToCredits,
  }
}
