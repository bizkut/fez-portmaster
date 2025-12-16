// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.ActiveAmbienceTrack
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using System;

#nullable disable
namespace FezEngine.Components;

internal class ActiveAmbienceTrack
{
  public AmbienceTrack Track;
  private readonly OggStream cue;
  private float volume;
  private IWaiter transitionWaiter;

  public bool ActiveForDayPhase { get; set; }

  public bool ForceMuted { get; set; }

  public bool WasMuted { get; set; }

  public ActiveAmbienceTrack(AmbienceTrack track, bool activeForDayPhase)
  {
    ServiceHelper.InjectServices((object) this);
    this.volume = 0.0f;
    this.ActiveForDayPhase = activeForDayPhase;
    this.OnMuteStateChanged();
    this.Track = track;
    this.cue = this.SoundManager.GetCue(this.Track.Name);
    this.cue.Volume = this.volume;
    this.cue.Play();
  }

  public void Dispose()
  {
    if (this.cue == null || this.cue.IsDisposed)
      return;
    this.cue.Stop();
    this.cue.Dispose();
  }

  public void Pause()
  {
    if (this.cue == null || this.cue.IsDisposed)
      return;
    this.cue.Pause();
  }

  public void Resume()
  {
    if (this.cue == null || this.cue.IsDisposed)
      return;
    this.cue.Resume();
  }

  public void OnMuteStateChanged() => this.OnMuteStateChanged(2f);

  public void OnMuteStateChanged(float fadeDuration)
  {
    if (this.ActiveForDayPhase && !this.ForceMuted && (double) this.volume != 1.0)
    {
      float originalVolume = this.volume;
      IWaiter thisWaiter = (IWaiter) null;
      this.volume += 1f / 1000f;
      this.transitionWaiter = thisWaiter = Waiters.Interpolate((double) fadeDuration * (1.0 - (double) this.volume), (Action<float>) (s =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = originalVolume + s * (1f - originalVolume);
        if (this.cue == null || this.cue.IsDisposed)
          return;
        this.cue.Volume = this.volume;
      }), (Action) (() =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = 1f;
        if (this.cue == null || this.cue.IsDisposed)
          return;
        this.cue.Volume = this.volume;
      }));
    }
    else
    {
      if (this.ActiveForDayPhase && !this.ForceMuted || (double) this.volume == 0.0)
        return;
      float originalVolume = this.volume;
      IWaiter thisWaiter = (IWaiter) null;
      this.volume -= 1f / 1000f;
      this.transitionWaiter = thisWaiter = Waiters.Interpolate((double) fadeDuration * (double) this.volume, (Action<float>) (s =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = originalVolume * (1f - s);
        if (this.cue == null || this.cue.IsDisposed)
          return;
        this.cue.Volume = this.volume;
      }), (Action) (() =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = 0.0f;
        if (this.cue == null || this.cue.IsDisposed)
          return;
        this.cue.Volume = this.volume;
      }));
    }
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }
}
