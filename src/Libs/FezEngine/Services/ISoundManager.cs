// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.ISoundManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public interface ISoundManager
{
  event Action SongChanged;

  bool IsLowPass { get; }

  SoundEmitter AddEmitter(SoundEmitter emitter);

  List<SoundEmitter> Emitters { get; }

  Vector2 LimitDistance { get; }

  void FadeFrequencies(bool lowPass);

  void FadeFrequencies(bool interior, float forSeconds);

  void FadeVolume(float fromVolume, float toVolume, float overSeconds);

  float MusicVolume { get; set; }

  float SoundEffectVolume { get; set; }

  void PlayNewSong();

  void PlayNewSong(string name);

  void PlayNewSong(string name, bool interrupt);

  void PlayNewSong(float fadeDuration);

  void PlayNewSong(string name, float fadeDuration);

  void PlayNewSong(string name, float fadeDuration, bool interrupt);

  void PlayNewAmbience();

  bool ScriptChangedSong { get; set; }

  TimeSpan PlayPosition { get; }

  void Pause();

  void Resume();

  void KillSounds();

  void KillSounds(float fadeDuration);

  OggStream GetCue(string name, bool asyncPrecache = false);

  void UpdateSongActiveTracks();

  void UpdateSongActiveTracks(float fadeDuration);

  void MuteAmbience(string trackName, float fadeDuration);

  void UnmuteAmbience(string trackName, float fadeDuration);

  void MuteAmbienceTracks();

  void UnmuteAmbienceTracks();

  void UnmuteAmbienceTracks(bool apply);

  TrackedSong CurrentlyPlayingSong { get; }

  float MusicVolumeFactor { get; set; }

  float GlobalVolumeFactor { get; set; }

  void FactorizeVolume();

  void Stop();

  void UnshelfSong();

  void InitializeLibrary();

  void ReloadVolumeLevels();

  float GetVolumeLevelFor(string name);
}
