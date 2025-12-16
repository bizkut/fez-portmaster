// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.SoundEmitter
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezEngine.Structure;

public class SoundEmitter
{
  private readonly float VolumeLevel;
  private bool factorizeVolume;
  private bool pausedForViewTransition;
  public bool New = true;
  private Vector3? position;
  private float pitch;
  private IWaiter deathWaiter;
  private IWaiter fadePauseWaiter;

  public SoundEffectInstance Cue { get; private set; }

  public bool FactorizeVolume
  {
    get => this.factorizeVolume;
    set => this.factorizeVolume = value;
  }

  public bool PauseViewTransitions { get; set; }

  public float VolumeMaster { get; set; }

  public float VolumeFactor { get; set; }

  public float FadeDistance { get; set; }

  public float NonFactorizedVolume { get; set; }

  public bool Persistent { get; set; }

  public bool NoAttenuation { get; set; }

  public Vector3 AxisMask { get; set; }

  public bool OverrideMap { get; set; }

  public bool WasPlaying { get; set; }

  public bool LowPass { get; private set; }

  public Vector3 Position
  {
    get => !this.position.HasValue ? Vector3.Zero : this.position.Value;
    set => this.position = new Vector3?(value);
  }

  public float Pitch
  {
    get => this.pitch;
    set => this.Cue.Pitch = this.pitch = value;
  }

  public float Pan { get; set; }

  internal SoundEmitter(
    SoundEffect sound,
    bool looped,
    float pitch,
    float volumeFactor,
    bool paused,
    Vector3? position)
  {
    this.SM = ServiceHelper.Get<ISoundManager>();
    this.EngineState = ServiceHelper.Get<IEngineStateManager>();
    this.CameraManager = ServiceHelper.Get<IDefaultCameraManager>();
    ILevelManager levelManager = ServiceHelper.Get<ILevelManager>();
    this.VolumeLevel = this.SM.GetVolumeLevelFor(sound.Name);
    this.VolumeMaster = this.EngineState.DotLoading ? 0.0f : 1f;
    this.FadeDistance = 10f;
    this.AxisMask = Vector3.One;
    this.VolumeFactor = volumeFactor;
    this.position = position;
    if (SoundManager.NoMoreSounds)
      return;
    try
    {
      this.Cue = sound.CreateInstance();
      this.Pitch = pitch;
      this.Cue.IsLooped = looped;
      if (!paused)
      {
        this.Update();
        this.Cue.Play();
      }
      else
      {
        this.Cue.Volume = 0.0f;
        this.Cue.Play();
        this.Cue.Pause();
      }
      this.LowPass = !levelManager.LowPass && !sound.Name.Contains("Ui") && !sound.Name.Contains("Warp") && !sound.Name.Contains("Zoom") && !sound.Name.Contains("Trixel");
      if (!this.LowPass)
        return;
      (this.SM as SoundManager).RegisterLowPass(this.Cue);
    }
    catch (InstancePlayLimitException ex)
    {
      Logger.Log(nameof (SoundEmitter), LogSeverity.Warning, "Couldn't create sound instance (too many instances)");
    }
  }

  public void Update()
  {
    if (this.Cue == null)
      return;
    if (this.PauseViewTransitions)
    {
      if (this.Cue.State == SoundState.Paused && this.CameraManager.ViewTransitionReached && this.pausedForViewTransition)
      {
        this.pausedForViewTransition = false;
        this.Cue.Resume();
      }
      else if (this.Cue.State == SoundState.Playing && !this.CameraManager.ViewTransitionReached)
      {
        this.pausedForViewTransition = true;
        this.Cue.Pause();
      }
    }
    if (this.Cue.State == SoundState.Paused || this.EngineState.InMap && !this.OverrideMap)
      return;
    if (this.position.HasValue)
    {
      Vector3 right = this.CameraManager.InverseView.Right;
      Vector3 interpolatedCenter = this.CameraManager.InterpolatedCenter;
      Vector2 vector2 = new Vector2()
      {
        X = (this.Position - interpolatedCenter).Dot(right),
        Y = interpolatedCenter.Y - this.Position.Y
      };
      float num1 = 1f;
      if (!this.NoAttenuation)
      {
        float num2 = vector2.Length();
        num1 = (double) num2 > 10.0 ? (float) (0.60000002384185791 / (((double) num2 - 10.0) / 5.0 + 1.0)) : (float) (1.0 - (double) Easing.EaseIn((double) num2 / 10.0, EasingType.Quadratic) * 0.40000000596046448);
      }
      this.NonFactorizedVolume = num1;
      this.Cue.Volume = FezMath.Saturate(this.NonFactorizedVolume * this.VolumeFactor * this.VolumeLevel * this.VolumeMaster);
      this.Cue.Pan = MathHelper.Clamp((float) ((double) vector2.X / (double) this.SM.LimitDistance.X * 1.5), -1f, 1f);
    }
    else
    {
      this.Cue.Volume = FezMath.Saturate(this.VolumeFactor * this.VolumeLevel * this.VolumeMaster);
      this.Cue.Pan = this.Pan;
    }
  }

  public bool Dead
  {
    get => this.Cue == null || this.Cue.IsDisposed || this.Cue.State == SoundState.Stopped;
  }

  public void FadeOutAndDie(float forSeconds, bool autoPause)
  {
    if ((double) forSeconds == 0.0)
    {
      if (this.Cue == null || this.Cue.IsDisposed || this.Cue.State == SoundState.Stopped)
        return;
      this.Cue.Stop();
    }
    else
    {
      if (this.Dead || this.deathWaiter != null)
        return;
      float volumeFactor = this.VolumeFactor * this.VolumeLevel * this.VolumeMaster;
      this.deathWaiter = Waiters.Interpolate((double) forSeconds, (Action<float>) (s => this.VolumeFactor = volumeFactor * (1f - s)), (Action) (() =>
      {
        if (this.Cue != null && !this.Cue.IsDisposed && this.Cue.State != SoundState.Stopped)
          this.Cue.Stop();
        this.deathWaiter = (IWaiter) null;
      }));
      this.deathWaiter.AutoPause = autoPause;
    }
  }

  public void FadeOutAndDie(float forSeconds) => this.FadeOutAndDie(forSeconds, true);

  public void FadeOutAndPause(float forSeconds)
  {
    if ((double) forSeconds == 0.0)
    {
      if (this.Cue == null || this.Cue.IsDisposed || this.Cue.State == SoundState.Paused)
        return;
      this.Cue.Pause();
    }
    else
    {
      if (this.Dead || this.fadePauseWaiter != null)
        return;
      float volumeFactor = this.VolumeFactor * this.VolumeLevel * this.VolumeMaster;
      this.fadePauseWaiter = Waiters.Interpolate((double) forSeconds, (Action<float>) (s => this.VolumeFactor = volumeFactor * (1f - s)), (Action) (() =>
      {
        if (this.Cue != null && !this.Cue.IsDisposed && this.Cue.State != SoundState.Paused)
          this.Cue.Pause();
        this.fadePauseWaiter = (IWaiter) null;
      }));
      this.fadePauseWaiter.AutoPause = true;
    }
  }

  public void Dispose()
  {
    if (this.Cue != null)
      this.Cue.Dispose();
    this.Cue = (SoundEffectInstance) null;
  }

  [ServiceDependency]
  public ISoundManager SM { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }
}
