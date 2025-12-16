// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.SoundManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using ContentSerialization;
using FezEngine.Components;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

#nullable disable
namespace FezEngine.Services;

public class SoundManager : GameComponent, ISoundManager
{
  private const float VolumeFadeSeconds = 2f;
  private const float LowFrequency = 0.025f;
  private const float MasterMusicVolume = 1f;
  public static bool NoMusic;
  private Dictionary<string, byte[]> MusicCache;
  private ActiveTrackedSong ActiveSong;
  private ActiveTrackedSong ShelvedSong;
  private ActiveAmbience ActiveAmbience;
  private VolumeLevels VolumeLevels;
  private float FrequencyStep;
  public static bool NoMoreSounds;
  private bool initialized;
  private const int SoundPerUpdate = 5;
  private readonly List<SoundEmitter> toUpdate = new List<SoundEmitter>(5);
  private int firstIndex;
  private float musicVolume;
  private float musicVolumeFactor;
  private float soundEffectVolume;
  private float globalVolumeFactor;
  private static readonly MethodInfo applyFilter = typeof (SoundEffectInstance).GetMethod("INTERNAL_applyLowPassFilter", BindingFlags.Instance | BindingFlags.NonPublic);
  private object[] gainContainer = new object[1]
  {
    (object) 1f
  };
  private IWaiter freqTransitionWaiter;
  private IWaiter volTransitionWaiter;

  public bool IsLowPass { get; private set; }

  public List<SoundEmitter> Emitters { get; private set; }

  public Vector2 LimitDistance { get; private set; }

  public bool ScriptChangedSong { get; set; }

  public event Action SongChanged;

  public SoundManager(Game game, bool noMusic = false)
    : base(game)
  {
    this.Emitters = new List<SoundEmitter>();
    this.UpdateOrder = 100;
    SoundManager.NoMusic = noMusic;
  }

  public override void Initialize()
  {
    this.musicVolumeFactor = 1f;
    this.MusicVolume = SettingsManager.Settings.MusicVolume;
    this.SoundEffectVolume = SettingsManager.Settings.SoundVolume;
    this.GlobalVolumeFactor = 1f;
    this.CameraManager.ProjectionChanged += new Action(this.UpdateLimitDistance);
    this.UpdateLimitDistance();
    this.LevelManager.LevelChanging += new Action(this.KillSounds);
    this.EngineState.PauseStateChanged += (Action) (() =>
    {
      if (this.EngineState.Paused)
        this.Pause();
      else
        this.Resume();
    });
  }

  public void InitializeLibrary()
  {
    if (this.initialized)
      return;
    this.initialized = true;
    using (FileStream input = File.OpenRead(Path.Combine("Content", "Music.pak")))
    {
      using (BinaryReader binaryReader = new BinaryReader((Stream) input))
      {
        int capacity = binaryReader.ReadInt32();
        this.MusicCache = new Dictionary<string, byte[]>(capacity);
        for (int index = 0; index < capacity; ++index)
        {
          string key = binaryReader.ReadString();
          int count = binaryReader.ReadInt32();
          if (this.MusicCache.ContainsKey(key))
            Logger.Log(nameof (SoundManager), $"Skipped {key} track because it was already loaded");
          else
            this.MusicCache.Add(key, binaryReader.ReadBytes(count));
        }
      }
    }
  }

  private void UpdateLimitDistance()
  {
    this.LimitDistance = new Vector2(1f, 1f / this.CameraManager.AspectRatio) * this.CameraManager.Radius / 2f;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Loading)
      return;
    this.UpdateEmitters();
  }

  private void UpdateEmitters()
  {
    lock (this.Emitters)
    {
      for (int index = this.Emitters.Count - 1; index >= 0; --index)
      {
        if (this.Emitters[index].Dead)
        {
          this.Emitters[index].Dispose();
          this.Emitters.RemoveAt(index);
        }
      }
      this.FactorizeVolume();
    }
  }

  public void FactorizeVolume()
  {
    if (this.Emitters.Count == 0)
      return;
    float num1 = 0.0f;
    int num2 = 0;
    this.firstIndex %= this.Emitters.Count;
    this.toUpdate.Clear();
    int firstIndex = this.firstIndex;
    int num3 = 0;
    while (num3 < this.Emitters.Count)
    {
      if (firstIndex == this.Emitters.Count)
        firstIndex -= this.Emitters.Count;
      SoundEmitter emitter = this.Emitters[firstIndex];
      num1 += emitter.NonFactorizedVolume;
      if ((double) emitter.NonFactorizedVolume != 0.0)
        ++num2;
      if (num3 < 5 || emitter.New)
      {
        this.toUpdate.Add(emitter);
        emitter.New = false;
      }
      ++num3;
      ++firstIndex;
    }
    this.firstIndex += 5;
    float num4 = (float) (-1.0 / (num2 == 0 ? 1.0 : (double) num2) + 2.0);
    foreach (SoundEmitter soundEmitter in this.toUpdate)
    {
      if (soundEmitter.FactorizeVolume)
        soundEmitter.VolumeFactor = FezMath.Saturate(num4 / ((double) num1 == 0.0 ? 1f : num1));
      soundEmitter.Update();
    }
  }

  public float MusicVolume
  {
    get => this.musicVolume;
    set
    {
      this.musicVolume = FezMath.Saturate(value);
      OggStream.SyncAllVolume();
    }
  }

  public float MusicVolumeFactor
  {
    get => this.musicVolumeFactor;
    set
    {
      this.musicVolumeFactor = FezMath.Saturate(value);
      this.MusicVolume = this.musicVolume;
      OggStream.SyncAllVolume();
    }
  }

  public float SoundEffectVolume
  {
    get => this.soundEffectVolume;
    set
    {
      this.soundEffectVolume = FezMath.Saturate(value);
      SoundEffect.MasterVolume = FezMath.Saturate(Easing.EaseIn((double) this.soundEffectVolume * (double) this.globalVolumeFactor, EasingType.Quadratic));
      OggStream.SyncAllVolume();
    }
  }

  public float GlobalVolumeFactor
  {
    get => this.globalVolumeFactor;
    set
    {
      this.globalVolumeFactor = value;
      this.SoundEffectVolume = this.soundEffectVolume;
      this.MusicVolume = this.musicVolume;
    }
  }

  public float LowPassHFGain
  {
    get => (float) this.gainContainer[0];
    set
    {
      this.gainContainer[0] = (object) MathHelper.Clamp(value, 0.0f, 1f);
      lock (this.Emitters)
      {
        foreach (SoundEmitter emitter in this.Emitters)
        {
          if (emitter.Cue != null && emitter.LowPass)
            SoundManager.applyFilter.Invoke((object) emitter.Cue, this.gainContainer);
        }
      }
      OggStream.SyncAllFilter(SoundManager.applyFilter, this.gainContainer);
    }
  }

  public void RegisterLowPass(SoundEffectInstance sfi)
  {
    SoundManager.applyFilter.Invoke((object) sfi, this.gainContainer);
  }

  public void FadeFrequencies(bool lowPass) => this.FadeFrequencies(lowPass, 2f);

  public void FadeFrequencies(bool toLowPass, float fadeDuration)
  {
    if (!this.IsLowPass & toLowPass)
    {
      float originalStep = this.FrequencyStep;
      IWaiter thisWaiter = (IWaiter) null;
      this.freqTransitionWaiter = thisWaiter = Waiters.Interpolate((double) fadeDuration * (1.0 - (double) originalStep), (Action<float>) (s =>
      {
        if (this.freqTransitionWaiter != thisWaiter)
          return;
        this.FrequencyStep = originalStep + s * (1f - originalStep);
        this.LowPassHFGain = MathHelper.Lerp(1f, 0.025f, Easing.EaseOut((double) this.FrequencyStep, EasingType.Cubic));
      }), (Action) (() =>
      {
        if (this.freqTransitionWaiter != thisWaiter)
          return;
        this.FrequencyStep = 1f;
        this.LowPassHFGain = 0.025f;
      }));
    }
    else if (this.IsLowPass && !toLowPass)
    {
      float originalStep = this.FrequencyStep;
      IWaiter thisWaiter = (IWaiter) null;
      this.freqTransitionWaiter = thisWaiter = Waiters.Interpolate((double) fadeDuration * (double) originalStep, (Action<float>) (s =>
      {
        if (this.freqTransitionWaiter != thisWaiter)
          return;
        this.FrequencyStep = originalStep * (1f - s);
        this.LowPassHFGain = MathHelper.Lerp(1f, 0.025f, Easing.EaseIn((double) this.FrequencyStep, EasingType.Cubic));
      }), (Action) (() =>
      {
        if (this.freqTransitionWaiter != thisWaiter)
          return;
        this.FrequencyStep = 0.0f;
        this.LowPassHFGain = 1f;
      }));
    }
    this.IsLowPass = toLowPass;
  }

  public void FadeVolume(float fromVolume, float toVolume, float overSeconds)
  {
    IWaiter thisWaiter = (IWaiter) null;
    this.volTransitionWaiter = thisWaiter = Waiters.Interpolate((double) overSeconds, (Action<float>) (step =>
    {
      if (this.volTransitionWaiter != thisWaiter || this.EngineState.DotLoading)
        return;
      this.MusicVolumeFactor = MathHelper.Lerp(fromVolume, toVolume, step);
    }), (Action) (() =>
    {
      if (this.volTransitionWaiter != thisWaiter)
        return;
      if (!this.EngineState.DotLoading)
        this.MusicVolumeFactor = toVolume;
      this.volTransitionWaiter = (IWaiter) null;
    }));
  }

  public void Pause()
  {
    lock (this.Emitters)
    {
      foreach (SoundEmitter emitter in this.Emitters)
      {
        if (!emitter.Dead && emitter.Cue.State == SoundState.Playing)
        {
          emitter.Cue.Pause();
          emitter.WasPlaying = true;
        }
      }
    }
    if (this.ActiveSong != null)
      this.ActiveSong.Pause();
    if (this.ActiveAmbience == null)
      return;
    this.ActiveAmbience.Pause();
  }

  public void Resume()
  {
    lock (this.Emitters)
    {
      foreach (SoundEmitter emitter in this.Emitters)
      {
        if (!emitter.Dead && emitter.WasPlaying && emitter.Cue.State == SoundState.Paused)
          emitter.Cue.Resume();
      }
    }
    if (this.ActiveSong != null)
      this.ActiveSong.Resume();
    if (this.ActiveAmbience == null)
      return;
    this.ActiveAmbience.Resume();
  }

  public void KillSounds()
  {
    lock (this.Emitters)
    {
      foreach (SoundEmitter soundEmitter in this.Emitters.ToArray())
      {
        if (!soundEmitter.Persistent)
        {
          if (!soundEmitter.Dead)
            soundEmitter.Dispose();
          this.Emitters.Remove(soundEmitter);
        }
      }
    }
  }

  public void KillSounds(float fadeDuration)
  {
    lock (this.Emitters)
    {
      foreach (SoundEmitter soundEmitter in this.Emitters.ToArray())
      {
        if (!soundEmitter.Persistent)
        {
          soundEmitter.FadeOutAndDie(fadeDuration, false);
          this.Emitters.Remove(soundEmitter);
        }
      }
    }
  }

  public OggStream GetCue(string name, bool asyncPrecache = false)
  {
    OggStream cue = (OggStream) null;
    try
    {
      string str = name.Replace(" ^ ", "\\");
      bool flag = name.Contains("Ambience");
      byte[] buffer = this.MusicCache[str.ToLower(CultureInfo.InvariantCulture)];
      cue = new OggStream(new MemoryStream(buffer, 0, buffer.Length, false, true))
      {
        Category = flag ? "Ambience" : "Music",
        IsLooped = flag
      };
      cue.RealName = name;
      if (name.Contains("Gomez"))
        cue.LowPass = false;
    }
    catch (Exception ex)
    {
      Logger.Log(nameof (SoundManager), LogSeverity.Error, $"Failed for '{name}' ('{name.Replace(" ^ ", "\\").ToLower(CultureInfo.InvariantCulture)}' : {(object) ex}");
      Logger.Log(nameof (SoundManager), "Music Cache contained : " + Util.DeepToString<string>((IEnumerable<string>) this.MusicCache.Keys));
    }
    return cue;
  }

  public void UpdateSongActiveTracks()
  {
    if (this.ActiveSong == null)
      return;
    this.ActiveSong.SetMutedLoops(this.LevelManager.MutedLoops, 6f);
  }

  public void UpdateSongActiveTracks(float fadeDuration)
  {
    if (this.ActiveSong == null)
      return;
    this.ActiveSong.SetMutedLoops(this.LevelManager.MutedLoops, fadeDuration);
  }

  public void PlayNewSong()
  {
    if (SoundManager.NoMusic)
      return;
    TrackedSong currentlyPlayingSong1 = this.CurrentlyPlayingSong;
    if (this.ActiveSong != null)
      this.ActiveSong.FadeOutAndRemoveComponent();
    if (this.LevelManager.Song == null)
      this.ActiveSong = (ActiveTrackedSong) null;
    else
      ServiceHelper.AddComponent((IGameComponent) (this.ActiveSong = new ActiveTrackedSong(this.Game)));
    TrackedSong currentlyPlayingSong2 = this.CurrentlyPlayingSong;
    if (currentlyPlayingSong1 == currentlyPlayingSong2)
      return;
    this.SongChanged();
  }

  public void PlayNewSong(float fadeDuration)
  {
    if (SoundManager.NoMusic)
      return;
    TrackedSong currentlyPlayingSong1 = this.CurrentlyPlayingSong;
    if (this.ActiveSong != null)
      this.ActiveSong.FadeOutAndRemoveComponent(fadeDuration);
    if (this.LevelManager.Song == null)
      this.ActiveSong = (ActiveTrackedSong) null;
    else
      ServiceHelper.AddComponent((IGameComponent) (this.ActiveSong = new ActiveTrackedSong(this.Game)));
    TrackedSong currentlyPlayingSong2 = this.CurrentlyPlayingSong;
    if (currentlyPlayingSong1 == currentlyPlayingSong2)
      return;
    this.SongChanged();
  }

  public void PlayNewSong(string name)
  {
    if (SoundManager.NoMusic)
      return;
    this.PlayNewSong(name, true);
  }

  public void PlayNewSong(string name, bool interrupt)
  {
    if (SoundManager.NoMusic)
      return;
    TrackedSong currentlyPlayingSong1 = this.CurrentlyPlayingSong;
    if (!interrupt)
      this.ShelvedSong = this.ActiveSong;
    else if (this.ActiveSong != null)
      this.ActiveSong.FadeOutAndRemoveComponent();
    if (string.IsNullOrEmpty(name))
    {
      this.ActiveSong = (ActiveTrackedSong) null;
    }
    else
    {
      TrackedSong song = this.CMProvider.CurrentLevel.Load<TrackedSong>("Music/" + name);
      song.Initialize();
      ServiceHelper.AddComponent((IGameComponent) (this.ActiveSong = new ActiveTrackedSong(this.Game, song, this.LevelManager.MutedLoops)));
    }
    TrackedSong currentlyPlayingSong2 = this.CurrentlyPlayingSong;
    if (currentlyPlayingSong1 == currentlyPlayingSong2)
      return;
    this.SongChanged();
  }

  public void PlayNewSong(string name, float fadeDuration)
  {
    if (SoundManager.NoMusic)
      return;
    this.PlayNewSong(name, fadeDuration, true);
  }

  public void PlayNewSong(string name, float fadeDuration, bool interrupt)
  {
    if (SoundManager.NoMusic)
      return;
    TrackedSong currentlyPlayingSong1 = this.CurrentlyPlayingSong;
    if (!interrupt)
      this.ShelvedSong = this.ActiveSong;
    else if (this.ActiveSong != null)
      this.ActiveSong.FadeOutAndRemoveComponent(fadeDuration);
    if (string.IsNullOrEmpty(name))
    {
      this.ActiveSong = (ActiveTrackedSong) null;
    }
    else
    {
      TrackedSong song = this.CMProvider.CurrentLevel.Load<TrackedSong>("Music/" + name);
      song.Initialize();
      ServiceHelper.AddComponent((IGameComponent) (this.ActiveSong = new ActiveTrackedSong(this.Game, song, this.LevelManager.MutedLoops)));
    }
    TrackedSong currentlyPlayingSong2 = this.CurrentlyPlayingSong;
    if (currentlyPlayingSong1 == currentlyPlayingSong2)
      return;
    this.SongChanged();
  }

  public void PlayNewAmbience()
  {
    if (this.ActiveAmbience == null)
      ServiceHelper.AddComponent((IGameComponent) (this.ActiveAmbience = new ActiveAmbience(this.Game, (IEnumerable<AmbienceTrack>) this.LevelManager.AmbienceTracks)));
    else
      this.ActiveAmbience.ChangeTracks((IEnumerable<AmbienceTrack>) this.LevelManager.AmbienceTracks);
  }

  public void UnmuteAmbienceTracks()
  {
    if (this.ActiveAmbience == null)
      return;
    this.ActiveAmbience.UnmuteTracks(false);
  }

  public void UnmuteAmbienceTracks(bool apply)
  {
    if (this.ActiveAmbience == null)
      return;
    this.ActiveAmbience.UnmuteTracks(apply);
  }

  public void MuteAmbienceTracks()
  {
    if (this.ActiveAmbience == null)
      return;
    this.ActiveAmbience.MuteTracks();
  }

  public void MuteAmbience(string trackName, float fadeDuration)
  {
    if (this.ActiveAmbience != null)
      this.ActiveAmbience.MuteTrack(trackName, fadeDuration);
    else
      Waiters.Wait((Func<bool>) (() => this.ActiveAmbience != null), (Action) (() => this.ActiveAmbience.MuteTrack(trackName, fadeDuration)));
  }

  public void UnmuteAmbience(string trackName, float fadeDuration)
  {
    if (this.ActiveAmbience != null)
      this.ActiveAmbience.UnmuteTrack(trackName, fadeDuration);
    else
      Waiters.Wait((Func<bool>) (() => this.ActiveAmbience != null), (Action) (() => this.ActiveAmbience.UnmuteTrack(trackName, fadeDuration)));
  }

  public TimeSpan PlayPosition
  {
    get => this.ActiveSong != null ? this.ActiveSong.PlayPosition : TimeSpan.Zero;
  }

  public SoundEmitter AddEmitter(SoundEmitter emitter)
  {
    if (!SoundManager.NoMoreSounds)
    {
      lock (this.Emitters)
        this.Emitters.Add(emitter);
    }
    return emitter;
  }

  public TrackedSong CurrentlyPlayingSong
  {
    get => this.ActiveSong != null ? this.ActiveSong.Song : (TrackedSong) null;
  }

  public void UnshelfSong()
  {
    if (this.ShelvedSong == null)
      return;
    this.ActiveSong = this.ShelvedSong;
    this.ShelvedSong = (ActiveTrackedSong) null;
  }

  public void Stop()
  {
    if (this.ActiveSong != null)
      ServiceHelper.RemoveComponent<ActiveTrackedSong>(this.ActiveSong);
    if (this.ActiveAmbience != null)
      ServiceHelper.RemoveComponent<ActiveAmbience>(this.ActiveAmbience);
    this.ActiveSong = (ActiveTrackedSong) null;
    this.ActiveAmbience = (ActiveAmbience) null;
    this.MusicVolumeFactor = 1f;
  }

  public void ReloadVolumeLevels()
  {
    string cleanPath = SharedContentManager.GetCleanPath(Path.Combine(Path.Combine(this.CMProvider.Global.RootDirectory, "Sounds"), "SoundLevels.sdl"));
    FileStream fileStream;
    try
    {
      fileStream = new FileStream(cleanPath, FileMode.Open, FileAccess.Read);
    }
    catch (Exception ex)
    {
      switch (ex)
      {
        case FileNotFoundException _:
        case DirectoryNotFoundException _:
          Logger.Log("Sound Levels", LogSeverity.Warning, "Could not find levels file, ignoring...");
          fileStream = (FileStream) null;
          break;
        default:
          Logger.Log("Sound Levels", LogSeverity.Warning, ex.Message);
          return;
      }
    }
    if (fileStream == null)
      this.VolumeLevels = new VolumeLevels();
    else
      this.VolumeLevels = SdlSerializer.Deserialize<VolumeLevels>(new StreamReader((Stream) fileStream));
  }

  public float GetVolumeLevelFor(string name)
  {
    if (this.VolumeLevels == null)
      return 1f;
    lock (this.VolumeLevels)
    {
      if (name.Contains("Gomez") && this.LevelManager.Name == "PYRAMID")
        return 0.0f;
      VolumeLevel volumeLevel;
      return this.VolumeLevels.Sounds.TryGetValue(name.Replace("/", "\\"), out volumeLevel) ? volumeLevel.Level : 1f;
    }
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }
}
