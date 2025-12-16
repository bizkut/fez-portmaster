// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.ActiveTrackedSong
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

#nullable disable
namespace FezEngine.Components;

public class ActiveTrackedSong(Game game) : GameComponent(game)
{
  public const float DefaultFadeDuration = 2f;
  private IList<string> mutedLoops;
  public TrackedSong Song;
  private long BeatsCounted;
  private long BarsCounted;
  private double LastTotalMinutes;
  private readonly Stopwatch Watch = new Stopwatch();
  private readonly List<ActiveLoop> ActiveLoops = new List<ActiveLoop>();
  private ActiveLoop[] AllOaaTs;
  private ActiveLoop CurrentOaaT;
  private ActiveLoop NextOaaT;
  private int OoaTIndex = -1;
  private bool cancelPause;
  private bool resumeRequested;

  public event Action Beat = new Action(Util.NullAction);

  public event Action Bar = new Action(Util.NullAction);

  public bool IgnoreDayPhase { get; set; }

  public ActiveTrackedSong(Game game, TrackedSong song, IList<string> mutedLoops)
    : this(game)
  {
    this.Song = song;
    this.MutedLoops = mutedLoops;
  }

  public override void Initialize()
  {
    if (this.Song == null)
      this.Song = this.LevelManager.Song;
    if (this.MutedLoops == null)
      this.MutedLoops = this.LevelManager.MutedLoops;
    if (this.Song == null)
    {
      this.Enabled = false;
      ServiceHelper.RemoveComponent<ActiveTrackedSong>(this);
    }
    else
    {
      this.BarsCounted = this.BeatsCounted = 0L;
      this.Enabled = false;
      Waiters.Wait(0.1, (Action) (() =>
      {
        foreach (Loop loop in this.Song.Loops)
        {
          bool activeForDayPhase = this.IgnoreDayPhase;
          if (!this.IgnoreDayPhase)
            activeForDayPhase = ((((((((activeForDayPhase ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Day) ? 0 : (loop.Day ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Night) ? 0 : (loop.Night ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dawn) ? 0 : (loop.Dawn ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dusk) ? 0 : (loop.Dusk ? 1 : 0))) != 0;
          ActiveLoop activeLoop = new ActiveLoop(loop, this.MutedLoops.Contains(loop.Name), activeForDayPhase, loop.OneAtATime);
          if (loop.OneAtATime)
            activeLoop.CycleLink = new Action(this.CycleOaaTs);
          this.ActiveLoops.Add(activeLoop);
        }
        this.AllOaaTs = this.ActiveLoops.Where<ActiveLoop>((Func<ActiveLoop, bool>) (x => x.Loop.OneAtATime)).ToArray<ActiveLoop>();
        if (this.Song.Loops.Count > 0 && this.AllOaaTs.Length != 0)
          this.CycleOaaTs();
        this.Enabled = true;
        this.Watch.Start();
      }));
    }
  }

  private void CycleOaaTs()
  {
    if (this.CurrentOaaT != null && this.CurrentOaaT.Loop.CutOffTail && !this.CurrentOaaT.WaitedForDelay)
      this.CurrentOaaT.CutOff();
    this.CurrentOaaT = this.NextOaaT;
    if (this.CurrentOaaT != null)
      this.CurrentOaaT.ActiveForOoaTs = false;
    int index;
    if (this.Song.RandomOrdering)
    {
      index = RandomHelper.Random.Next(0, this.AllOaaTs.Length);
      if (this.CurrentOaaT != null && index == this.ActiveLoops.IndexOf(this.CurrentOaaT))
        index = RandomHelper.Random.Next(0, this.AllOaaTs.Length);
    }
    else
      index = this.Song.CustomOrdering == null || this.Song.CustomOrdering.Length == 0 ? (this.ActiveLoops.IndexOf(this.CurrentOaaT) + 1) % this.ActiveLoops.Count : this.Song.CustomOrdering[this.OoaTIndex = (this.OoaTIndex + 1) % this.Song.CustomOrdering.Length] - 1;
    this.NextOaaT = this.AllOaaTs[index];
    this.NextOaaT.ActiveForOoaTs = true;
    this.NextOaaT.SchedulePlay();
    if (this.CurrentOaaT != null)
      return;
    this.NextOaaT.ForcePlay();
    if (this.NextOaaT.Loop.Delay != 0)
      return;
    this.CycleOaaTs();
  }

  public override void Update(GameTime gameTime)
  {
    double totalMinutes = this.Watch.Elapsed.TotalMinutes;
    foreach (ActiveLoop activeLoop in this.ActiveLoops)
    {
      if (activeLoop.Loop.FractionalTime)
      {
        float totalBars = (float) (totalMinutes - this.LastTotalMinutes) * (float) this.Song.Tempo / (float) this.Song.TimeSignature;
        activeLoop.UpdateFractional(totalBars);
      }
      activeLoop.UpdatePrecache();
    }
    this.LastTotalMinutes = totalMinutes;
    double num1 = Math.Floor(totalMinutes * (double) this.Song.Tempo);
    if (num1 > (double) this.BeatsCounted)
    {
      this.BeatsCounted = (long) (int) num1;
      this.OnBeat();
      long num2 = this.BeatsCounted / (long) this.Song.TimeSignature;
      if (num2 > this.BarsCounted)
      {
        this.BarsCounted = num2;
        this.OnBar();
      }
    }
    if (this.IgnoreDayPhase)
      return;
    bool flag1 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Day);
    bool flag2 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Dawn);
    bool flag3 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Dusk);
    bool flag4 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Night);
    foreach (ActiveLoop activeLoop1 in this.ActiveLoops)
    {
      int num3 = activeLoop1.ActiveForDayPhase ? 1 : 0;
      activeLoop1.ActiveForDayPhase = false;
      ActiveLoop activeLoop2 = activeLoop1;
      activeLoop2.ActiveForDayPhase = ((activeLoop2.ActiveForDayPhase ? 1 : 0) | (!flag1 ? 0 : (activeLoop1.Loop.Day ? 1 : 0))) != 0;
      ActiveLoop activeLoop3 = activeLoop1;
      activeLoop3.ActiveForDayPhase = ((activeLoop3.ActiveForDayPhase ? 1 : 0) | (!flag4 ? 0 : (activeLoop1.Loop.Night ? 1 : 0))) != 0;
      ActiveLoop activeLoop4 = activeLoop1;
      activeLoop4.ActiveForDayPhase = ((activeLoop4.ActiveForDayPhase ? 1 : 0) | (!flag2 ? 0 : (activeLoop1.Loop.Dawn ? 1 : 0))) != 0;
      ActiveLoop activeLoop5 = activeLoop1;
      activeLoop5.ActiveForDayPhase = ((activeLoop5.ActiveForDayPhase ? 1 : 0) | (!flag3 ? 0 : (activeLoop1.Loop.Dusk ? 1 : 0))) != 0;
      int num4 = activeLoop1.ActiveForDayPhase ? 1 : 0;
      if (num3 != num4)
        activeLoop1.OnMuteStateChanged(16f);
    }
  }

  public void Pause()
  {
    if (!this.Enabled)
      return;
    this.resumeRequested = false;
    Waiters.Interpolate(0.25, (Action<float>) (step =>
    {
      this.cancelPause = ((this.cancelPause ? 1 : 0) | (this.resumeRequested ? 1 : (!this.Enabled ? 1 : 0))) != 0;
      if (this.cancelPause)
        return;
      this.SoundManager.MusicVolumeFactor = FezMath.Saturate(1f - Easing.EaseOut((double) step, EasingType.Sine));
    }), (Action) (() =>
    {
      if (!this.cancelPause && !this.resumeRequested)
      {
        this.Watch.Stop();
        foreach (ActiveLoop activeLoop in this.ActiveLoops)
          activeLoop.Pause();
        this.Enabled = false;
        this.SoundManager.MusicVolumeFactor = 1f;
      }
      if (this.resumeRequested)
        this.SoundManager.MusicVolumeFactor = 1f;
      this.cancelPause = this.resumeRequested = false;
    }));
  }

  public void Resume()
  {
    if (this.Enabled)
    {
      this.resumeRequested = true;
    }
    else
    {
      if (this.Watch == null)
        return;
      this.Watch.Start();
      foreach (ActiveLoop activeLoop in this.ActiveLoops)
        activeLoop.Resume();
      this.Enabled = true;
      Waiters.Interpolate(0.125, (Action<float>) (step =>
      {
        if (!this.Enabled)
          return;
        this.SoundManager.MusicVolumeFactor = FezMath.Saturate(Easing.EaseOut((double) step, EasingType.Sine));
        this.resumeRequested = false;
      }), (Action) (() => this.SoundManager.MusicVolumeFactor = 1f));
    }
  }

  private void OnBeat() => this.Beat();

  private void OnBar()
  {
    foreach (ActiveLoop activeLoop in this.ActiveLoops)
    {
      if (!activeLoop.Loop.FractionalTime)
        activeLoop.OnBar();
    }
    this.Bar();
  }

  public int CurrentBeat => (int) (this.BeatsCounted % (long) this.Song.TimeSignature);

  public int CurrentBar => (int) this.BarsCounted;

  public TimeSpan PlayPosition => this.Watch.Elapsed;

  public IList<string> MutedLoops
  {
    get => this.mutedLoops;
    set
    {
      this.mutedLoops = value;
      foreach (ActiveLoop activeLoop in this.ActiveLoops)
      {
        if (!activeLoop.Muted && this.mutedLoops.Contains(activeLoop.Loop.Name))
        {
          activeLoop.Muted = true;
          activeLoop.OnMuteStateChanged();
        }
      }
      foreach (ActiveLoop activeLoop in this.ActiveLoops)
      {
        if (activeLoop.Muted && !this.mutedLoops.Contains(activeLoop.Loop.Name))
        {
          activeLoop.Muted = false;
          activeLoop.OnMuteStateChanged();
        }
      }
    }
  }

  public void SetMutedLoops(IList<string> loops, float fadeDuration)
  {
    this.mutedLoops = loops;
    foreach (ActiveLoop activeLoop in this.ActiveLoops)
    {
      if (!activeLoop.Muted && this.mutedLoops.Contains(activeLoop.Loop.Name))
      {
        activeLoop.Muted = true;
        activeLoop.OnMuteStateChanged(fadeDuration);
      }
    }
    foreach (ActiveLoop activeLoop in this.ActiveLoops)
    {
      if (activeLoop.Muted && !this.mutedLoops.Contains(activeLoop.Loop.Name))
      {
        activeLoop.Muted = false;
        activeLoop.OnMuteStateChanged(fadeDuration);
      }
    }
  }

  public void ReInitialize(IList<string> newMutedLoops)
  {
    this.mutedLoops = newMutedLoops;
    this.Dispose(false);
    this.Initialize();
  }

  protected override void Dispose(bool disposing)
  {
    foreach (ActiveLoop activeLoop in this.ActiveLoops)
      activeLoop.Dispose();
    this.ActiveLoops.Clear();
    this.Enabled = false;
    if (!disposing)
      return;
    this.Beat = (Action) null;
    this.Bar = (Action) null;
  }

  public void FadeOutAndRemoveComponent() => this.FadeOutAndRemoveComponent(2f);

  public void FadeOutAndRemoveComponent(float fadeDuration)
  {
    if (!this.Enabled)
      return;
    foreach (ActiveLoop activeLoop in this.ActiveLoops)
    {
      activeLoop.Muted = true;
      activeLoop.OnMuteStateChanged(fadeDuration);
    }
    this.Enabled = false;
    Waiters.Wait((double) fadeDuration, (Action) (() => ServiceHelper.RemoveComponent<ActiveTrackedSong>(this)));
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }
}
