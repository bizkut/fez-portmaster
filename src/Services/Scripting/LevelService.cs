// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.LevelService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Components.Scripting;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Services.Scripting;

public class LevelService : ILevelService, IScriptingBase
{
  private SoundEffect sewageLevelSound;
  private SoundEffect sSolvedSecret;
  private SoundEmitter sewageLevelEmitter;
  private readonly Stack<object> waterStopStack = new Stack<object>();

  public event Action Start = new Action(Util.NullAction);

  public void OnStart()
  {
    if (this.sSolvedSecret == null)
      this.sSolvedSecret = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/SecretSolved");
    this.Start();
  }

  public bool FirstVisit => this.GameState.SaveData.ThisLevel.FirstVisit;

  public LongRunningAction ExploChangeLevel(string levelName) => (LongRunningAction) null;

  public LongRunningAction SetWaterHeight(float height)
  {
    float sign = (float) Math.Sign(height - this.LevelManager.WaterHeight);
    TrileInstance[] buoyantTriles = this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type.IsBuoyant())).ToArray<TrileInstance>();
    return new LongRunningAction((Func<float, float, bool>) ((elapsed, __) =>
    {
      if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || this.CameraManager.Viewpoint == Viewpoint.Perspective)
        return false;
      if ((double) Math.Sign(height - this.LevelManager.WaterHeight) != (double) sign)
        return true;
      this.LevelManager.WaterSpeed = 1.2f * sign;
      this.LevelManager.WaterHeight += this.LevelManager.WaterSpeed * elapsed;
      if (this.LevelManager.WaterType != LiquidType.Lava)
      {
        foreach (TrileInstance trileInstance in buoyantTriles)
        {
          if ((double) trileInstance.Center.Y < (double) this.LevelManager.WaterHeight)
            trileInstance.PhysicsState.Velocity = new Vector3(0.0f, 0.01f, 0.0f);
        }
      }
      return false;
    }), (Action) (() =>
    {
      this.LevelManager.WaterSpeed = 0.0f;
      if (this.LevelManager.WaterType == LiquidType.Water)
        this.GameState.SaveData.GlobalWaterLevelModifier = new float?(this.LevelManager.WaterHeight - this.LevelManager.OriginalWaterHeight);
      else
        this.GameState.SaveData.ThisLevel.LastStableLiquidHeight = new float?(this.LevelManager.WaterHeight);
    }));
  }

  public LongRunningAction RaiseWater(float unitsPerSecond, float toHeight)
  {
    float sign = (float) Math.Sign(toHeight - this.LevelManager.WaterHeight);
    TrileInstance[] buoyantTriles = this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type.IsBuoyant())).ToArray<TrileInstance>();
    if ((double) this.LevelManager.WaterSpeed != 0.0)
      this.waterStopStack.Push(new object());
    bool flag = false;
    if (this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.Name == "WATER_WHEEL")
    {
      if (this.sewageLevelEmitter != null && !this.sewageLevelEmitter.Dead)
      {
        this.sewageLevelEmitter.Cue.Stop();
        flag = true;
      }
      if (this.sewageLevelSound == null)
        this.sewageLevelSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Sewer/SewageLevelChange");
      this.sewageLevelEmitter = this.sewageLevelSound.Emit(true, !flag);
      if (!flag)
      {
        this.sewageLevelEmitter.VolumeFactor = 0.0f;
        this.sewageLevelEmitter.Cue.Resume();
      }
    }
    return new LongRunningAction((Func<float, float, bool>) ((elapsed, __) =>
    {
      if (this.GameState.Paused || this.GameState.InMap || this.GameState.ForceTimePaused || !this.CameraManager.ActionRunning || this.CameraManager.Viewpoint == Viewpoint.Perspective)
        return false;
      if ((double) Math.Sign(toHeight - this.LevelManager.WaterHeight) != (double) sign || this.waterStopStack.Count > 0)
        return true;
      if (this.sewageLevelEmitter != null && !this.sewageLevelEmitter.Dead)
        this.sewageLevelEmitter.VolumeFactor = FezMath.Saturate(this.sewageLevelEmitter.VolumeFactor + elapsed * 3f);
      this.LevelManager.WaterSpeed = unitsPerSecond * sign;
      this.LevelManager.WaterHeight += this.LevelManager.WaterSpeed * elapsed;
      foreach (TrileInstance trileInstance in buoyantTriles)
      {
        if (!trileInstance.PhysicsState.Floating && trileInstance.PhysicsState.Static && (double) trileInstance.Center.Y < (double) this.LevelManager.WaterHeight - 0.5)
        {
          trileInstance.PhysicsState.ForceNonStatic = true;
          trileInstance.PhysicsState.Ground = new MultipleHits<TrileInstance>();
        }
      }
      return false;
    }), (Action) (() =>
    {
      this.LevelManager.WaterSpeed = 0.0f;
      if (this.LevelManager.WaterType == LiquidType.Water)
        this.GameState.SaveData.GlobalWaterLevelModifier = new float?(this.LevelManager.WaterHeight - this.LevelManager.OriginalWaterHeight);
      else
        this.GameState.SaveData.ThisLevel.LastStableLiquidHeight = new float?(this.LevelManager.WaterHeight);
      if (this.waterStopStack.Count == 0)
      {
        if (this.sewageLevelEmitter != null && !this.sewageLevelEmitter.Dead)
          this.sewageLevelEmitter.FadeOutAndDie(0.75f);
        this.sewageLevelEmitter = (SoundEmitter) null;
      }
      if (this.waterStopStack.Count <= 0)
        return;
      this.waterStopStack.Pop();
    }));
  }

  public void StopWater() => this.waterStopStack.Push(new object());

  public LongRunningAction AllowPipeChangeLevel(string levelName)
  {
    this.PlayerManager.NextLevel = levelName;
    this.PlayerManager.PipeVolume = this.ScriptingManager.EvaluatedScript.InitiatingTrigger.Object.Identifier;
    return new LongRunningAction((Action) (() =>
    {
      this.PlayerManager.NextLevel = (string) null;
      this.PlayerManager.PipeVolume = new int?();
    }));
  }

  public LongRunningAction ChangeLevel(string levelName, bool asDoor, bool spin, bool trialEnding)
  {
    if (asDoor)
    {
      this.PlayerManager.SpinThroughDoor = spin;
      this.PlayerManager.NextLevel = levelName;
      this.PlayerManager.DoorVolume = this.ScriptingManager.EvaluatedScript.InitiatingTrigger.Object.Identifier;
      this.PlayerManager.DoorEndsTrial = trialEnding && this.GameState.IsTrialMode;
      IGameLevelManager levelManager = this.LevelManager;
      int num1 = levelManager.WentThroughSecretPassage ? 1 : 0;
      int? doorVolume = this.PlayerManager.DoorVolume;
      int num2;
      if (doorVolume.HasValue)
      {
        IDictionary<int, Volume> volumes1 = this.LevelManager.Volumes;
        doorVolume = this.PlayerManager.DoorVolume;
        int key1 = doorVolume.Value;
        if (volumes1[key1].ActorSettings != null)
        {
          IDictionary<int, Volume> volumes2 = this.LevelManager.Volumes;
          doorVolume = this.PlayerManager.DoorVolume;
          int key2 = doorVolume.Value;
          num2 = volumes2[key2].ActorSettings.IsSecretPassage ? 1 : 0;
          goto label_5;
        }
      }
      num2 = 0;
label_5:
      levelManager.WentThroughSecretPassage = (num1 | num2) != 0;
      return new LongRunningAction((Action) (() =>
      {
        this.PlayerManager.NextLevel = (string) null;
        this.PlayerManager.DoorVolume = new int?();
        this.PlayerManager.DoorEndsTrial = false;
        if (this.PlayerManager.Action.IsEnteringDoor())
          return;
        this.LevelManager.WentThroughSecretPassage = false;
      }));
    }
    ServiceHelper.AddComponent((IGameComponent) new LevelTransition(ServiceHelper.Game, levelName));
    return new LongRunningAction();
  }

  public LongRunningAction ChangeLevelToVolume(
    string levelName,
    int toVolume,
    bool asDoor,
    bool spin,
    bool trialEnding)
  {
    if (this.PlayerManager.Action.IsEnteringDoor())
      return (LongRunningAction) null;
    this.LevelManager.DestinationVolumeId = new int?(toVolume);
    LongRunningAction lra = this.ChangeLevel(levelName, asDoor, spin, trialEnding);
    return new LongRunningAction((Action) (() =>
    {
      if (lra.OnDispose != null)
        lra.OnDispose();
      if (this.PlayerManager.Action.IsEnteringDoor())
        return;
      this.LevelManager.DestinationVolumeId = new int?();
    }));
  }

  public LongRunningAction ReturnToLastLevel(bool asDoor, bool spin)
  {
    return this.ChangeLevel(this.LevelManager.LastLevelName, asDoor, spin, false);
  }

  public LongRunningAction ChangeToFarAwayLevel(string levelName, int toVolume, bool trialEnding)
  {
    this.LevelManager.DestinationIsFarAway = true;
    this.LevelManager.DestinationVolumeId = new int?(toVolume);
    LongRunningAction lra = this.ChangeLevel(levelName, true, false, trialEnding);
    return new LongRunningAction((Action) (() =>
    {
      if (lra.OnDispose != null)
        lra.OnDispose();
      this.LevelManager.DestinationIsFarAway = false;
      if (this.PlayerManager.Action.IsEnteringDoor())
        return;
      this.LevelManager.DestinationVolumeId = new int?();
    }));
  }

  public void ResolvePuzzle()
  {
    ++this.GameState.SaveData.ThisLevel.FilledConditions.SecretCount;
    List<Volume> currentVolumes = this.PlayerManager.CurrentVolumes;
    Volume volume;
    if ((volume = currentVolumes.FirstOrDefault<Volume>((Func<Volume, bool>) (x => x.ActorSettings != null && x.ActorSettings.IsPointOfInterest))) != null && volume.Enabled)
    {
      volume.Enabled = false;
      this.GameState.SaveData.ThisLevel.InactiveVolumes.Add(volume.Id);
    }
    this.GameState.Save();
    this.sSolvedSecret.Play();
    this.SoundManager.MusicVolumeFactor = 0.125f;
    Waiters.Wait(2.75, (Action) (() => this.SoundManager.FadeVolume(0.125f, 1f, 3f))).AutoPause = true;
  }

  public void ResolvePuzzleSilent()
  {
    ++this.GameState.SaveData.ThisLevel.FilledConditions.SecretCount;
    this.GameState.Save();
  }

  public void ResolvePuzzleSoundOnly()
  {
    this.sSolvedSecret.Play();
    this.SoundManager.MusicVolumeFactor = 0.125f;
    Waiters.Wait(2.75, (Action) (() => this.SoundManager.FadeVolume(0.125f, 1f, 3f))).AutoPause = true;
  }

  public void ResetEvents() => this.Start = new Action(Util.NullAction);

  [ServiceDependency]
  public IGameService GameService { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  internal IScriptingManager ScriptingManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
