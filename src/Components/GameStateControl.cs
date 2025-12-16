// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GameStateControl
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components;

public class GameStateControl : GameComponent
{
  private const float SaveWaitTimeSeconds = 4f;
  private IWaiter loadWaiter;

  public GameStateControl(Game game)
    : base(game)
  {
    this.UpdateOrder = -3;
  }

  public override void Initialize()
  {
    this.Game.Deactivated += (EventHandler<EventArgs>) ((s, ea) =>
    {
      if (!SettingsManager.Settings.PauseOnLostFocus)
        return;
      if (!this.GameState.SkipRendering)
        this.DoPause(s, ea);
      ServiceHelper.Get<ISoundManager>().GlobalVolumeFactor = 0.0f;
    });
    this.Game.Activated += (EventHandler<EventArgs>) ((_, __) =>
    {
      if (!SettingsManager.Settings.PauseOnLostFocus)
        return;
      ServiceHelper.Get<ISoundManager>().GlobalVolumeFactor = 1f;
    });
    this.InputManager.ActiveControllerDisconnected += (Action<PlayerIndex>) (_ =>
    {
      if (!SettingsManager.Settings.PauseOnLostFocus)
        return;
      this.DoPause((object) null, EventArgs.Empty);
    });
    this.GameState.DynamicUpgrade += new Action(this.DynamicUpgrade);
  }

  private void DynamicUpgrade()
  {
    this.GameState.ForcedSignOut = true;
    this.GameState.Restart();
  }

  private void DoPause(object s, EventArgs ea)
  {
    bool checkActive = s == this.Game;
    if (this.loadWaiter != null)
      return;
    this.loadWaiter = Waiters.Wait((Func<bool>) (() =>
    {
      if (this.GameState.Loading)
        return false;
      return !checkActive || Intro.Instance == null;
    }), (Action) (() =>
    {
      this.loadWaiter = (IWaiter) null;
      if (checkActive && this.Game.IsActive || MainMenu.Instance != null)
        return;
      this.GameState.Pause();
    }));
  }

  public override void Update(GameTime gameTime)
  {
    if ((double) this.GameState.SinceSaveRequest != -1.0)
    {
      this.GameState.SinceSaveRequest += (float) gameTime.ElapsedGameTime.TotalSeconds;
      if ((double) this.GameState.SinceSaveRequest > 4.0)
        this.GameState.SaveImmediately();
    }
    if (this.GameState.Loading || this.CameraManager.Viewpoint == Viewpoint.Perspective)
      return;
    if ((!this.GameState.InCutscene || this.GameState.InEndCutscene) && this.InputManager.Start == FezButtonState.Pressed)
      this.GameState.Pause();
    if (this.GameState.InCutscene || !this.PlayerManager.CanControl || this.GameState.FarawaySettings.InTransition || this.PlayerManager.HideFez || this.PlayerManager.Action == ActionType.OpeningTreasure || this.PlayerManager.Action == ActionType.OpeningDoor || this.PlayerManager.Action == ActionType.FindingTreasure || this.PlayerManager.Action == ActionType.ReadingSign || this.PlayerManager.Action == ActionType.LesserWarp || this.PlayerManager.Action == ActionType.GateWarp || this.PlayerManager.Action.IsEnteringDoor() || this.PlayerManager.Action == ActionType.ExitDoor || this.PlayerManager.Action == ActionType.TurnToBell || this.PlayerManager.Action == ActionType.TurnAwayFromBell || this.PlayerManager.Action == ActionType.HitBell || this.PlayerManager.Action == ActionType.WalkingTo || !this.CameraManager.ViewTransitionReached || this.GameState.Paused || this.LevelManager.Name == "ELDERS" || EndCutscene32Host.Instance != null || EndCutscene64Host.Instance != null)
      return;
    if (this.InputManager.OpenInventory == FezButtonState.Pressed && !this.GameState.IsTrialMode && !this.GameState.InMenuCube && !this.LevelManager.Name.StartsWith("GOMEZ_HOUSE_END") && this.PlayerManager.Action != ActionType.WalkingTo)
      this.GameState.ToggleInventory();
    if (this.GameState.InMap || this.InputManager.Back != FezButtonState.Pressed || !this.GameState.SaveData.CanOpenMap && !Fez.LevelChooser || !(this.LevelManager.Name != "PYRAMID") || this.LevelManager.Name.StartsWith("GOMEZ_HOUSE_END"))
      return;
    this.GameState.ToggleMap();
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }
}
