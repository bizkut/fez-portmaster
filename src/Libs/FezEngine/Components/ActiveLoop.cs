// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.ActiveLoop
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Components;

internal class ActiveLoop
{
  public readonly Loop Loop;
  private float fractionalBar;
  private int barsToCount;
  private int loopsToPlay;
  private float barsBeforePlay;
  private bool nextCuePrecached;
  private bool playing;
  private readonly List<OggStream> strayCues = new List<OggStream>();
  private OggStream currentCue;
  private OggStream nextCue;
  private float volume;
  private IWaiter transitionWaiter;
  public bool WaitedForDelay;

  public bool ActiveForDayPhase { get; set; }

  public bool Muted { get; set; }

  public bool ActiveForOoaTs { get; set; }

  public Action CycleLink { get; set; }

  public ActiveLoop(Loop loop, bool muted, bool activeForDayPhase, bool dontStart)
  {
    ServiceHelper.InjectServices((object) this);
    this.Muted = muted;
    this.volume = muted || !activeForDayPhase ? 0.0f : 1f;
    this.ActiveForDayPhase = activeForDayPhase;
    this.OnMuteStateChanged();
    this.Loop = loop;
    this.nextCuePrecached = true;
    this.nextCue = this.SoundManager.GetCue(loop.Name);
    this.barsBeforePlay = (float) this.Loop.Delay;
    if (dontStart || this.Loop.Delay != 0)
      return;
    this.FirstPlay();
  }

  public void UpdateFractional(float totalBars)
  {
    if (this.Loop.OneAtATime && !this.ActiveForOoaTs)
      return;
    this.fractionalBar += totalBars;
    if ((double) this.fractionalBar >= 1.0)
    {
      this.OnBar();
      --this.fractionalBar;
    }
    if (this.playing || (double) this.barsBeforePlay > (double) this.fractionalBar)
      return;
    this.barsBeforePlay -= this.fractionalBar;
    this.fractionalBar = 0.0f;
    if (this.Loop.OneAtATime)
      this.CycleLink();
    this.WaitedForDelay = false;
    this.FirstPlay();
  }

  public void UpdatePrecache()
  {
    if (this.nextCuePrecached || this.strayCues.Count != 0)
      return;
    this.nextCue = this.SoundManager.GetCue(this.Loop.Name, true);
    this.nextCuePrecached = true;
  }

  public void OnBar()
  {
    if (this.Loop.OneAtATime && !this.ActiveForOoaTs)
      return;
    if (this.playing)
    {
      --this.barsToCount;
      if (this.barsToCount == 0)
      {
        --this.loopsToPlay;
        if (this.loopsToPlay == 0)
        {
          this.playing = false;
          this.barsBeforePlay = !this.Loop.FractionalTime ? (float) RandomHelper.Random.Next(this.Loop.TriggerFrom, this.Loop.TriggerTo + 1) : RandomHelper.Between((double) this.Loop.TriggerFrom, (double) this.Loop.TriggerTo);
          if ((double) this.barsBeforePlay == 0.0)
            this.FirstPlay();
        }
        else
          this.Play();
      }
    }
    else
    {
      --this.barsBeforePlay;
      if ((double) this.barsBeforePlay <= 0.0)
      {
        if (this.Loop.OneAtATime)
          this.CycleLink();
        this.WaitedForDelay = false;
        this.FirstPlay();
      }
    }
    for (int index = this.strayCues.Count - 1; index >= 0; --index)
    {
      if (this.strayCues[index].IsStopped)
      {
        this.strayCues[index].Dispose();
        this.strayCues.RemoveAt(index);
      }
    }
  }

  public void SchedulePlay()
  {
    this.playing = false;
    if (this.Loop.FractionalTime)
      this.barsBeforePlay = RandomHelper.Between((double) this.Loop.TriggerFrom, (double) this.Loop.TriggerTo);
    else
      this.barsBeforePlay = (float) RandomHelper.Random.Next(this.Loop.TriggerFrom, this.Loop.TriggerTo + 1);
  }

  public void ForcePlay()
  {
    this.barsBeforePlay = (float) this.Loop.Delay;
    if (this.Loop.Delay == 0)
      this.FirstPlay();
    else
      this.WaitedForDelay = true;
  }

  private void FirstPlay()
  {
    this.playing = true;
    this.loopsToPlay = RandomHelper.Random.Next(this.Loop.LoopTimesFrom, this.Loop.LoopTimesTo + 1);
    this.Play();
  }

  private void Play()
  {
    if (!this.nextCuePrecached)
      this.nextCue = this.SoundManager.GetCue(this.Loop.Name);
    this.nextCue.Volume = this.volume;
    this.nextCue.Play();
    if (this.currentCue != null)
      this.strayCues.Add(this.currentCue);
    this.currentCue = this.nextCue;
    this.barsToCount = this.Loop.Duration;
    this.nextCuePrecached = false;
  }

  public void Dispose()
  {
    if (this.currentCue != null)
    {
      this.currentCue.Stop();
      this.currentCue.Dispose();
      this.currentCue = (OggStream) null;
    }
    this.nextCue.Stop();
    this.nextCue.Dispose();
    this.nextCue = (OggStream) null;
    foreach (OggStream strayCue in this.strayCues)
    {
      strayCue.Stop();
      strayCue.Dispose();
    }
    this.strayCues.Clear();
    this.CycleLink = (Action) null;
  }

  public void Pause()
  {
    if (this.currentCue == null || this.currentCue.IsDisposed)
      return;
    this.currentCue.Pause();
  }

  public void Resume()
  {
    if (this.currentCue == null || this.currentCue.IsDisposed)
      return;
    this.currentCue.Resume();
  }

  public void CutOff()
  {
    if (this.currentCue != null)
    {
      this.currentCue.Stop();
      this.currentCue.Dispose();
    }
    this.currentCue = (OggStream) null;
  }

  public void OnMuteStateChanged() => this.OnMuteStateChanged(2f);

  public void OnMuteStateChanged(float fadeDuration)
  {
    if (this.ActiveForDayPhase && !this.Muted && (double) this.volume != 1.0)
    {
      float originalVolume = this.volume;
      IWaiter thisWaiter = (IWaiter) null;
      this.volume -= 1f / 1000f;
      this.transitionWaiter = thisWaiter = Waiters.Interpolate((double) fadeDuration * (1.0 - (double) this.volume), (Action<float>) (s =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = Easing.EaseOut((double) originalVolume + (double) s * (1.0 - (double) originalVolume), EasingType.Sine);
        if (this.currentCue == null || this.currentCue.IsDisposed)
          return;
        this.currentCue.Volume = this.volume;
      }), (Action) (() =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = 1f;
        if (this.currentCue == null || this.currentCue.IsDisposed)
          return;
        this.currentCue.Volume = this.volume;
      }));
    }
    else
    {
      if (this.ActiveForDayPhase && !this.Muted || (double) this.volume == 0.0)
        return;
      float originalVolume = this.volume;
      IWaiter thisWaiter = (IWaiter) null;
      this.volume += 1f / 1000f;
      this.transitionWaiter = thisWaiter = Waiters.Interpolate((double) fadeDuration * (double) this.volume, (Action<float>) (s =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = Easing.EaseOut((double) originalVolume * (1.0 - (double) s), EasingType.Sine);
        if (this.currentCue == null || this.currentCue.IsDisposed)
          return;
        this.currentCue.Volume = this.volume;
      }), (Action) (() =>
      {
        if (this.transitionWaiter != thisWaiter)
          return;
        this.volume = 0.0f;
        if (this.currentCue == null || this.currentCue.IsDisposed)
          return;
        this.currentCue.Volume = this.volume;
      }));
    }
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }
}
