// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.SoundService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Services.Scripting;

public class SoundService : ISoundService, IScriptingBase
{
  private readonly Dictionary<string, Mutable<int>> soundIndices = new Dictionary<string, Mutable<int>>();
  public static bool ImmediateEffect;

  public void ResetEvents() => this.soundIndices.Clear();

  public void Play(string soundName)
  {
    try
    {
      this.CMProvider.Global.Load<SoundEffect>("Sounds/" + soundName).Emit();
    }
    catch (Exception ex)
    {
      Logger.Log("Sounds Service", LogSeverity.Warning, ex.Message);
    }
  }

  public void PlayNext(string soundPrefix)
  {
    Mutable<int> mutable;
    if (!this.soundIndices.TryGetValue(soundPrefix, out mutable))
      this.soundIndices.Add(soundPrefix, mutable = new Mutable<int>());
    ++mutable.Value;
    try
    {
      this.CMProvider.CurrentLevel.Load<SoundEffect>($"Sounds/{soundPrefix}{(object) mutable.Value}").Emit();
    }
    catch (Exception ex)
    {
      Logger.Log("Sounds Service", LogSeverity.Warning, ex.Message);
    }
  }

  public void SetMusicVolume(float volume) => this.SoundManager.MusicVolumeFactor = volume;

  public void FadeMusicOut(float overSeconds)
  {
    this.SoundManager.FadeVolume(this.SoundManager.MusicVolumeFactor, 0.0f, overSeconds);
  }

  public void FadeMusicTo(float to, float overSeconds)
  {
    this.SoundManager.FadeVolume(this.SoundManager.MusicVolumeFactor, to, overSeconds);
  }

  public void ResetIndices(string soundPrefix) => this.soundIndices.Remove(soundPrefix);

  public void ChangeMusic(string newMusic)
  {
    if (this.SoundManager.CurrentlyPlayingSong == null || this.SoundManager.CurrentlyPlayingSong.Name != newMusic)
      this.SoundManager.PlayNewSong(newMusic, 8f);
    this.SoundManager.ScriptChangedSong = true;
  }

  public void ChangePhases(string trackName, bool dawn, bool day, bool dusk, bool night)
  {
    Loop loop = this.LevelManager.Song.Loops.FirstOrDefault<Loop>((Func<Loop, bool>) (x => x.Name == trackName));
    if (loop == null)
    {
      Logger.Log("Sound Service", $"Track not found for ChangePhases ('{trackName}')");
    }
    else
    {
      loop.Day = day;
      loop.Dawn = dawn;
      loop.Dusk = dusk;
      loop.Night = night;
    }
  }

  public void UnmuteTrack(string trackName, float fadeDuration)
  {
    if (!this.LevelManager.MutedLoops.Remove(trackName))
      return;
    this.SoundManager.UpdateSongActiveTracks(SoundService.ImmediateEffect ? 0.0f : fadeDuration);
  }

  public void MuteTrack(string trackName, float fadeDuration)
  {
    if (!this.LevelManager.MutedLoops.Contains(trackName))
    {
      this.LevelManager.MutedLoops.Add(trackName);
      this.SoundManager.UpdateSongActiveTracks(SoundService.ImmediateEffect ? 0.0f : fadeDuration);
    }
    else
      Logger.Log(nameof (SoundService), LogSeverity.Warning, $"No such track or track is already muted : '{trackName}'");
  }

  public void UnmuteAmbience(string trackName, float fadeDuration)
  {
    this.SoundManager.UnmuteAmbience(trackName, SoundService.ImmediateEffect ? 0.0f : fadeDuration);
  }

  public void MuteAmbience(string trackName, float fadeDuration)
  {
    this.SoundManager.MuteAmbience(trackName, SoundService.ImmediateEffect ? 0.0f : fadeDuration);
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }
}
