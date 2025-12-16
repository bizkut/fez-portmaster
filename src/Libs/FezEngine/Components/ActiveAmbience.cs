// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.ActiveAmbience
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Components;

internal class ActiveAmbience : GameComponent
{
  private List<AmbienceTrack> Tracks;
  private readonly List<ActiveAmbienceTrack> ActiveTracks = new List<ActiveAmbienceTrack>();
  private bool cancelPause;
  private bool resumeRequested;

  public ActiveAmbience(Game game, IEnumerable<AmbienceTrack> tracks)
    : base(game)
  {
    this.Tracks = new List<AmbienceTrack>(tracks);
  }

  public override void Initialize()
  {
    this.Enabled = false;
    foreach (AmbienceTrack track in this.Tracks)
    {
      bool activeForDayPhase = ((((((!this.TimeManager.IsDayPhaseForMusic(DayPhase.Day) ? (false ? 1 : 0) : (track.Day ? 1 : 0)) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Night) ? 0 : (track.Night ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dawn) ? 0 : (track.Dawn ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dusk) ? 0 : (track.Dusk ? 1 : 0))) != 0;
      this.ActiveTracks.Add(new ActiveAmbienceTrack(track, activeForDayPhase));
    }
    this.Tracks.Clear();
    this.Tracks = (List<AmbienceTrack>) null;
    this.Enabled = true;
  }

  public override void Update(GameTime gameTime)
  {
    bool flag1 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Day);
    bool flag2 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Dawn);
    bool flag3 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Dusk);
    bool flag4 = this.TimeManager.IsDayPhaseForMusic(DayPhase.Night);
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
    {
      int num1 = activeTrack.ActiveForDayPhase ? 1 : 0;
      activeTrack.ActiveForDayPhase = false;
      ActiveAmbienceTrack activeAmbienceTrack1 = activeTrack;
      activeAmbienceTrack1.ActiveForDayPhase = ((activeAmbienceTrack1.ActiveForDayPhase ? 1 : 0) | (!flag1 ? 0 : (activeTrack.Track.Day ? 1 : 0))) != 0;
      ActiveAmbienceTrack activeAmbienceTrack2 = activeTrack;
      activeAmbienceTrack2.ActiveForDayPhase = ((activeAmbienceTrack2.ActiveForDayPhase ? 1 : 0) | (!flag4 ? 0 : (activeTrack.Track.Night ? 1 : 0))) != 0;
      ActiveAmbienceTrack activeAmbienceTrack3 = activeTrack;
      activeAmbienceTrack3.ActiveForDayPhase = ((activeAmbienceTrack3.ActiveForDayPhase ? 1 : 0) | (!flag2 ? 0 : (activeTrack.Track.Dawn ? 1 : 0))) != 0;
      ActiveAmbienceTrack activeAmbienceTrack4 = activeTrack;
      activeAmbienceTrack4.ActiveForDayPhase = ((activeAmbienceTrack4.ActiveForDayPhase ? 1 : 0) | (!flag3 ? 0 : (activeTrack.Track.Dusk ? 1 : 0))) != 0;
      int num2 = activeTrack.ActiveForDayPhase ? 1 : 0;
      if (num1 != num2 && !activeTrack.ForceMuted)
        activeTrack.OnMuteStateChanged(16f);
    }
  }

  public void Pause()
  {
    if (!this.Enabled)
      return;
    Waiters.Interpolate(0.25, (Action<float>) (step => this.cancelPause = ((this.cancelPause ? 1 : 0) | (this.resumeRequested ? 1 : (!this.Enabled ? 1 : 0))) != 0), (Action) (() =>
    {
      if (!this.cancelPause && !this.resumeRequested)
      {
        foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
          activeTrack.Pause();
        this.Enabled = false;
      }
      this.cancelPause = this.resumeRequested = false;
    }));
  }

  public void Resume()
  {
    if (this.Enabled)
      return;
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
      activeTrack.Resume();
    this.Enabled = true;
    this.resumeRequested = true;
    Waiters.Interpolate(0.125, (Action<float>) (step =>
    {
      if (!this.Enabled)
        return;
      this.resumeRequested = false;
    }));
  }

  public void ChangeTracks(IEnumerable<AmbienceTrack> tracks)
  {
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
    {
      foreach (AmbienceTrack track in tracks)
      {
        if (track.Name == activeTrack.Track.Name)
        {
          activeTrack.Track = track;
          int num1 = !activeTrack.ActiveForDayPhase ? 0 : (activeTrack.ForceMuted ? 0 : (!activeTrack.WasMuted ? 1 : 0));
          activeTrack.WasMuted = false;
          bool flag = ((((((!this.TimeManager.IsDayPhaseForMusic(DayPhase.Day) ? (false ? 1 : 0) : (track.Day ? 1 : 0)) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Night) ? 0 : (track.Night ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dawn) ? 0 : (track.Dawn ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dusk) ? 0 : (track.Dusk ? 1 : 0))) != 0;
          activeTrack.ActiveForDayPhase = flag;
          int num2 = activeTrack.ActiveForDayPhase ? 1 : 0;
          if (num1 != num2)
          {
            if (!activeTrack.ForceMuted)
            {
              activeTrack.OnMuteStateChanged(2f);
              break;
            }
            break;
          }
          break;
        }
      }
    }
    foreach (ActiveAmbienceTrack activeAmbienceTrack in this.ActiveTracks.Where<ActiveAmbienceTrack>((Func<ActiveAmbienceTrack, bool>) (x => !tracks.Any<AmbienceTrack>((Func<AmbienceTrack, bool>) (y => y.Name == x.Track.Name)))))
    {
      activeAmbienceTrack.ForceMuted = true;
      activeAmbienceTrack.OnMuteStateChanged(2f);
      ActiveAmbienceTrack t1 = activeAmbienceTrack;
      Waiters.Wait(2.0, (Action) (() =>
      {
        t1.Dispose();
        this.ActiveTracks.Remove(t1);
      }));
    }
    foreach (AmbienceTrack track in tracks.Where<AmbienceTrack>((Func<AmbienceTrack, bool>) (x => !this.ActiveTracks.Any<ActiveAmbienceTrack>((Func<ActiveAmbienceTrack, bool>) (y => y.Track.Name == x.Name)))))
    {
      bool activeForDayPhase = ((((((!this.TimeManager.IsDayPhaseForMusic(DayPhase.Day) ? (false ? 1 : 0) : (track.Day ? 1 : 0)) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Night) ? 0 : (track.Night ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dawn) ? 0 : (track.Dawn ? 1 : 0))) != 0 ? 1 : 0) | (!this.TimeManager.IsDayPhaseForMusic(DayPhase.Dusk) ? 0 : (track.Dusk ? 1 : 0))) != 0;
      this.ActiveTracks.Add(new ActiveAmbienceTrack(track, activeForDayPhase));
    }
  }

  public void MuteTrack(string name, float fadeDuration)
  {
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
    {
      if (activeTrack.Track.Name == name)
      {
        activeTrack.ForceMuted = true;
        activeTrack.OnMuteStateChanged(fadeDuration);
        break;
      }
    }
  }

  public void UnmuteTrack(string name, float fadeDuration)
  {
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
    {
      if (activeTrack.Track.Name == name)
      {
        activeTrack.ForceMuted = false;
        activeTrack.OnMuteStateChanged(fadeDuration);
        break;
      }
    }
  }

  public void UnmuteTracks(bool apply)
  {
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
    {
      if (activeTrack.ForceMuted)
        activeTrack.WasMuted = true;
      activeTrack.ForceMuted = false;
      if (apply)
        activeTrack.OnMuteStateChanged();
    }
  }

  public void MuteTracks()
  {
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
    {
      activeTrack.ForceMuted = true;
      activeTrack.OnMuteStateChanged();
    }
  }

  protected override void Dispose(bool disposing)
  {
    foreach (ActiveAmbienceTrack activeTrack in this.ActiveTracks)
      activeTrack.Dispose();
    this.ActiveTracks.Clear();
    this.Enabled = false;
  }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }
}
