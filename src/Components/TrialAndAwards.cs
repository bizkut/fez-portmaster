// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TrialAndAwards
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Components.Scripting;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

public class TrialAndAwards(Game game) : GameComponent(game), IGameService, IScriptingBase
{
  private static readonly TimeSpan CheckFrequency = TimeSpan.FromSeconds(0.25);
  private TimeSpan sinceChecked = TrialAndAwards.CheckFrequency;
  private readonly List<MockAchievement> Achievements = new List<MockAchievement>()
  {
    new MockAchievement("Achievement_01"),
    new MockAchievement("Achievement_02"),
    new MockAchievement("Achievement_03"),
    new MockAchievement("Achievement_04"),
    new MockAchievement("Achievement_05"),
    new MockAchievement("Achievement_06"),
    new MockAchievement("Achievement_07"),
    new MockAchievement("Achievement_08"),
    new MockAchievement("Achievement_09"),
    new MockAchievement("Achievement_10"),
    new MockAchievement("Achievement_11"),
    new MockAchievement("Achievement_12")
  };

  public void ResetEvents()
  {
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += (Action) (() => this.CollisionManager.GravityFactor = 1f);
  }

  public override void Update(GameTime gameTime)
  {
    this.sinceChecked += gameTime.ElapsedGameTime;
    if (this.sinceChecked < TrialAndAwards.CheckFrequency)
      return;
    this.sinceChecked = TimeSpan.Zero;
    bool flag = PauseMenu.Instance != null && PauseMenu.Instance.EndGameMenu;
    if (this.GameState.Paused && !flag || this.GameState.InCutscene && !flag || this.GameState.Loading || this.Achievements == null)
      return;
    foreach (MockAchievement achievement in this.Achievements)
    {
      if (!achievement.IsAchieved && this.CheckAchievement(achievement.AchievementName))
      {
        if (!this.GameState.SaveData.EarnedAchievements.Contains(achievement.AchievementName))
          this.GameState.SaveData.EarnedAchievements.Add(achievement.AchievementName);
        this.GameState.AwardAchievement(achievement);
      }
    }
  }

  public bool CheckAchievement(string key)
  {
    switch (key)
    {
      case "Achievement_01":
        return this.GameState.SaveData.SecretCubes >= 32 /*0x20*/ && this.GameState.SaveData.CubeShards >= 32 /*0x20*/ && this.GameState.SaveData.Artifacts.Count >= 4;
      case "Achievement_02":
        return this.GameState.SaveData.CubeShards >= 1 && this.PlayerManager.CanControl;
      case "Achievement_03":
        if (this.GameState.SaveData.HasNewGamePlus)
          return true;
        return PauseMenu.Instance != null && PauseMenu.Instance.EndGameMenu;
      case "Achievement_04":
        return this.GameState.SaveData.SecretCubes >= 32 /*0x20*/ && this.GameState.SaveData.CubeShards >= 32 /*0x20*/;
      case "Achievement_05":
        return this.GameState.SaveData.Artifacts.Contains(ActorType.Tome);
      case "Achievement_06":
        return this.GameState.SaveData.Artifacts.Contains(ActorType.TriSkull);
      case "Achievement_07":
        return this.GameState.SaveData.Artifacts.Contains(ActorType.LetterCube);
      case "Achievement_08":
        return this.GameState.SaveData.Artifacts.Contains(ActorType.NumberCube);
      case "Achievement_09":
        return this.GameState.SaveData.SecretCubes > 0;
      case "Achievement_10":
        return this.GameState.SaveData.UnlockedWarpDestinations.Count >= 5;
      case "Achievement_11":
        return this.GameState.SaveData.AnyCodeDeciphered;
      case "Achievement_12":
        return this.GameState.SaveData.AchievementCheatCodeDone;
      default:
        return false;
    }
  }

  public void EndTrial(bool forceRestart)
  {
    if (!this.GameState.IsTrialMode)
      return;
    ScreenFade component = new ScreenFade(ServiceHelper.Game);
    component.FromColor = ColorEx.TransparentBlack;
    component.ToColor = Color.Black;
    component.EaseOut = true;
    component.CaptureScreen = true;
    component.Duration = 1f;
    component.DrawOrder = 2050;
    component.WaitUntil = (Func<bool>) (() => !this.GameState.Loading);
    component.ScreenCaptured += (Action) (() =>
    {
      this.GameState.SkipLoadBackground = true;
      this.GameState.Loading = true;
      Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoSellScreen));
      worker.Finished += (Action) (() =>
      {
        this.ThreadPool.Return<bool>(worker);
        this.GameState.ScheduleLoadEnd = true;
        this.GameState.SkipLoadBackground = false;
      });
      worker.Start(forceRestart);
    });
    ServiceHelper.AddComponent((IGameComponent) component);
  }

  private void DoSellScreen(bool forceRestart)
  {
    if (forceRestart)
      this.GameState.Reset();
    if (this.GameState.InCutscene && Intro.Instance != null)
      ServiceHelper.RemoveComponent<Intro>(Intro.Instance);
    Intro component = new Intro(this.Game);
    component.Sell = true;
    component.FadeBackToGame = !forceRestart;
    ServiceHelper.AddComponent((IGameComponent) component);
    component.LoadVideo();
  }

  public LongRunningAction Wait(float seconds)
  {
    return new LongRunningAction((Func<float, float, bool>) ((elapsed, sinceStarted) => (double) sinceStarted >= (double) seconds));
  }

  public LongRunningAction GlitchUp()
  {
    NesGlitches component = new NesGlitches(this.Game);
    ServiceHelper.AddComponent((IGameComponent) component);
    bool disposed = false;
    component.Disposed += (EventHandler<EventArgs>) ((_, __) => disposed = true);
    return new LongRunningAction((Func<float, float, bool>) ((_, __) => disposed));
  }

  public LongRunningAction Reboot(string toLevel)
  {
    FezGame.Components.Reboot component = new FezGame.Components.Reboot(this.Game, toLevel);
    ServiceHelper.AddComponent((IGameComponent) component);
    bool disposed = false;
    component.Disposed += (EventHandler<EventArgs>) ((_, __) => disposed = true);
    return new LongRunningAction((Func<float, float, bool>) ((_, __) => disposed));
  }

  public void SetGravity(bool inverted, float factor)
  {
    if ((double) factor == 0.0)
      factor = 1f;
    factor = Math.Abs(factor);
    this.CollisionManager.GravityFactor = inverted ? -factor : factor;
  }

  public void AllowMapUsage() => this.GameState.SaveData.CanOpenMap = true;

  public void ShowCapsuleLetter()
  {
    ServiceHelper.AddComponent((IGameComponent) new GeezerLetterSender(this.Game));
  }

  public LongRunningAction ShowScroll(
    string localizedString,
    float forSeconds,
    bool onTop,
    bool onVolume)
  {
    if (this.GameState.ActiveScroll != null)
    {
      TextScroll textScroll = this.GameState.ActiveScroll;
      while (textScroll.NextScroll != null)
        textScroll = textScroll.NextScroll;
      if (textScroll.Key == localizedString && !textScroll.Closing)
        return (LongRunningAction) null;
      textScroll.Closing = true;
      textScroll.Timeout = new float?();
      TextScroll nextScroll = new TextScroll(this.Game, GameText.GetString(localizedString), onTop)
      {
        Key = localizedString
      };
      if ((double) forSeconds > 0.0)
        nextScroll.Timeout = new float?(forSeconds);
      this.GameState.ActiveScroll.NextScroll = nextScroll;
      return onVolume ? new LongRunningAction((Action) (() => this.CloseScroll(nextScroll))) : (LongRunningAction) null;
    }
    ServiceHelper.AddComponent((IGameComponent) (this.GameState.ActiveScroll = new TextScroll(this.Game, GameText.GetString(localizedString), onTop)
    {
      Key = localizedString
    }));
    if ((double) forSeconds > 0.0)
      this.GameState.ActiveScroll.Timeout = new float?(forSeconds);
    return onVolume ? new LongRunningAction((Action) (() => this.CloseScroll(localizedString))) : (LongRunningAction) null;
  }

  private void CloseScroll(TextScroll scroll)
  {
    if (this.GameState.ActiveScroll == null)
      return;
    if (this.GameState.ActiveScroll == scroll)
    {
      this.GameState.ActiveScroll.Close();
    }
    else
    {
      TextScroll textScroll = this.GameState.ActiveScroll;
      for (TextScroll nextScroll = textScroll.NextScroll; nextScroll != null; nextScroll = nextScroll.NextScroll)
      {
        if (nextScroll == scroll)
        {
          textScroll.NextScroll = nextScroll.NextScroll;
          break;
        }
        textScroll = nextScroll;
      }
    }
  }

  public void CloseScroll(string key)
  {
    if (this.GameState.ActiveScroll == null)
      return;
    if (string.IsNullOrEmpty(key))
    {
      this.GameState.ActiveScroll.Close();
      this.GameState.ActiveScroll.NextScroll = (TextScroll) null;
    }
    else if (this.GameState.ActiveScroll.Key == key)
    {
      this.GameState.ActiveScroll.Close();
    }
    else
    {
      TextScroll textScroll = this.GameState.ActiveScroll;
      for (TextScroll nextScroll = textScroll.NextScroll; nextScroll != null; nextScroll = nextScroll.NextScroll)
      {
        if (nextScroll.Key == key)
        {
          textScroll.NextScroll = nextScroll.NextScroll;
          break;
        }
        textScroll = nextScroll;
      }
    }
  }

  public void SetGlobalState(string state)
  {
    this.GameState.SaveData.ScriptingState = state;
    this.GameState.Save();
  }

  public void SetLevelState(string state)
  {
    this.GameState.SaveData.ThisLevel.ScriptingState = state;
    this.GameState.Save();
  }

  public void ResolveMapQR()
  {
    this.GameState.SaveData.MapCheatCodeDone = true;
    this.GameState.Save();
  }

  public bool IsMapQrResolved => this.GameState.SaveData.MapCheatCodeDone;

  public bool IsScrollOpen => this.GameState.ActiveScroll != null;

  public string GetGlobalState => this.GameState.SaveData.ScriptingState ?? string.Empty;

  public string GetLevelState => this.GameState.SaveData.ThisLevel.ScriptingState ?? string.Empty;

  public void ResolveSewerQR()
  {
    if (this.GameState.SaveData.World.ContainsKey("SEWER_QR") && !this.GameState.SaveData.World["SEWER_QR"].InactiveArtObjects.Contains(0))
    {
      this.GameState.SaveData.World["SEWER_QR"].InactiveArtObjects.Add(0);
      ++this.GameState.SaveData.World["SEWER_QR"].FilledConditions.SecretCount;
    }
    if (this.GameState.SaveData.World.ContainsKey("ZU_THRONE_RUINS") && !this.GameState.SaveData.World["ZU_THRONE_RUINS"].InactiveVolumes.Contains(2))
    {
      this.GameState.SaveData.World["ZU_THRONE_RUINS"].InactiveVolumes.Add(2);
      ++this.GameState.SaveData.World["ZU_THRONE_RUINS"].FilledConditions.SecretCount;
    }
    if (this.GameState.SaveData.World.ContainsKey("ZU_HOUSE_EMPTY") && !this.GameState.SaveData.World["ZU_HOUSE_EMPTY"].InactiveVolumes.Contains(2))
    {
      this.GameState.SaveData.World["ZU_HOUSE_EMPTY"].InactiveVolumes.Add(2);
      ++this.GameState.SaveData.World["ZU_HOUSE_EMPTY"].FilledConditions.SecretCount;
    }
    this.GameState.Save();
  }

  public void ResolveZuQR()
  {
    if (this.GameState.SaveData.World.ContainsKey("PARLOR") && !this.GameState.SaveData.World["PARLOR"].InactiveVolumes.Contains(4))
    {
      this.GameState.SaveData.World["PARLOR"].InactiveVolumes.Add(4);
      ++this.GameState.SaveData.World["PARLOR"].FilledConditions.SecretCount;
    }
    if (this.GameState.SaveData.World.ContainsKey("ZU_HOUSE_QR") && !this.GameState.SaveData.World["ZU_HOUSE_QR"].InactiveVolumes.Contains(0))
    {
      this.GameState.SaveData.World["ZU_HOUSE_QR"].InactiveVolumes.Add(0);
      ++this.GameState.SaveData.World["ZU_HOUSE_QR"].FilledConditions.SecretCount;
    }
    this.GameState.Save();
  }

  public bool IsSewerQrResolved
  {
    get
    {
      int num1 = !this.GameState.SaveData.World.ContainsKey("SEWER_QR") ? 0 : (this.GameState.SaveData.World["SEWER_QR"].InactiveArtObjects.Contains(0) ? 1 : 0);
      bool flag = this.GameState.SaveData.World.ContainsKey("ZU_THRONE_RUINS") && this.GameState.SaveData.World["ZU_THRONE_RUINS"].InactiveVolumes.Contains(2);
      int num2 = !this.GameState.SaveData.World.ContainsKey("ZU_HOUSE_EMPTY") ? (false ? 1 : 0) : (this.GameState.SaveData.World["ZU_HOUSE_EMPTY"].InactiveVolumes.Contains(2) ? 1 : 0);
      return (num1 | num2 | (flag ? 1 : 0)) != 0;
    }
  }

  public bool IsZuQrResolved
  {
    get
    {
      return ((!this.GameState.SaveData.World.ContainsKey("PARLOR") ? 0 : (this.GameState.SaveData.World["PARLOR"].InactiveVolumes.Contains(4) ? 1 : 0)) | (!this.GameState.SaveData.World.ContainsKey("ZU_HOUSE_QR") ? (false ? 1 : 0) : (this.GameState.SaveData.World["ZU_HOUSE_QR"].InactiveVolumes.Contains(0) ? 1 : 0))) != 0;
    }
  }

  public void Start32BitCutscene()
  {
    SpeedRun.CallTime(Util.LocalSaveFolder);
    ServiceHelper.AddComponent((IGameComponent) new EndCutscene32Host(this.Game));
  }

  public void Start64BitCutscene()
  {
    SpeedRun.CallTime(Util.LocalSaveFolder);
    ServiceHelper.AddComponent((IGameComponent) new EndCutscene64Host(this.Game));
  }

  public void Checkpoint()
  {
    Waiters.Wait((Func<bool>) (() => this.PlayerManager.Grounded && this.PlayerManager.Ground.First.Trile.ActorSettings.Type.IsSafe()), (Action) (() =>
    {
      if (this.LevelManager.Name == "LAVA" && (double) this.LevelManager.WaterHeight < 50.0)
        this.LevelManager.WaterHeight = 132f;
      this.PlayerManager.RecordRespawnInformation(true);
    }));
  }

  [ServiceDependency]
  public IThreadPool ThreadPool { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }
}
