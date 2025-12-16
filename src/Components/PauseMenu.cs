// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PauseMenu
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

internal class PauseMenu : MenuBase
{
  public static StarField Starfield;
  public bool Ready;
  private IntroZoomIn IntroZoomIn;
  public static PauseMenu Instance;
  private bool wasStrict;
  private SoundEffect sStarZoom;

  public PauseMenu(Game game)
    : base(game)
  {
    this.UpdateOrder = -10;
    this.DrawOrder = 2009;
    PauseMenu.Instance = this;
  }

  public static void PreInitialize()
  {
    ServiceHelper.AddComponent((IGameComponent) (PauseMenu.Starfield = new StarField(ServiceHelper.Game)));
  }

  protected override void PostInitialize()
  {
    if (PauseMenu.Starfield == null)
      ServiceHelper.AddComponent((IGameComponent) (PauseMenu.Starfield = new StarField(this.Game)));
    this.MenuRoot.AddItem("ResumeGame", new Action(((MenuBase) this).ResumeGame), 0);
    if (Fez.SpeedRunMode && this.GameState.SaveSlot == 4)
      this.MenuRoot.AddItem((string) null, new Action(this.ResetSpeedRun), 1).SuffixText = (Func<string>) (() => "RESET RUN");
    this.wasStrict = this.InputManager.StrictRotation;
    this.InputManager.StrictRotation = false;
  }

  protected override void ResumeGame()
  {
    ServiceHelper.AddComponent((IGameComponent) new TileTransition(ServiceHelper.Game)
    {
      ScreenCaptured = (Action) (() => ServiceHelper.RemoveComponent<PauseMenu>(this))
    });
    this.Enabled = false;
    this.sDisappear.Emit().Persistent = true;
  }

  protected override void StartNewGame()
  {
    base.StartNewGame();
    this.GameState.ClearSaveFile();
    if (this.GameState.SaveData.HasNewGamePlus)
    {
      this.GameState.SaveData.HasFPView = false;
      this.GameState.SaveData.Level = "GOMEZ_HOUSE_2D";
    }
    this.sStarZoom.Emit().Persistent = true;
    this.StartedNewGame = true;
    this.StartLoading();
    PauseMenu.Starfield.Enabled = true;
    this.GameState.InCutscene = true;
  }

  protected override void ReturnToArcade()
  {
    if (this.GameState.IsTrialMode)
    {
      this.GameService.EndTrial(true);
      Waiters.Wait(0.10000000149011612, (Action) (() => ServiceHelper.RemoveComponent<PauseMenu>(this)));
    }
    else
      this.GameState.ReturnToArcade();
    this.Enabled = false;
  }

  private void ResetSpeedRun()
  {
    this.GameState.SaveSlot = 4;
    this.GameState.LoadSaveFile((Action) (() =>
    {
      this.GameState.Save();
      this.GameState.SaveImmediately();
    }));
    SpeedRun.Dispose();
    this.StartNewGame();
    SpeedRun.Begin(this.CMProvider.Global.Load<Texture2D>("Other Textures/SpeedRun"));
  }

  private void StartLoading()
  {
    this.GameState.Loading = true;
    Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoLoad));
    worker.Finished += (Action) (() => this.ThreadPool.Return<bool>(worker));
    worker.Start(false);
  }

  private void DoLoad(bool dummy) => Logger.Try(new Action(this.DoLoad));

  private void DoLoad()
  {
    this.GameState.Loading = true;
    this.GameState.SkipLoadBackground = true;
    this.GameState.Reset();
    this.GameState.UnPause();
    this.GameState.LoadLevel();
    Logger.Log("Pause Menu", "Game restarted.");
    this.GameState.ScheduleLoadEnd = true;
    this.GameState.SkipLoadBackground = false;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sStarZoom = this.CMProvider.Global.Load<SoundEffect>("Sounds/Intro/StarZoom");
    if (this.EndGameMenu)
      return;
    this.sAppear.Emit();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    PauseMenu.Instance = (PauseMenu) null;
    if (Intro.Instance == null && EndCutscene32Host.Instance == null && EndCutscene64Host.Instance == null)
      this.GameState.InCutscene = false;
    this.InputManager.StrictRotation = this.wasStrict;
  }

  protected override bool UpdateEarlyOut()
  {
    if (this.GameState.IsTrialMode)
    {
      if (this.StartedNewGame && this.selectorPhase != SelectorPhase.Disappear)
      {
        this.sinceSelectorPhaseStarted = 0.0f;
        this.selectorPhase = SelectorPhase.Disappear;
      }
      if (this.StartedNewGame && !this.GameState.Loading)
      {
        this.DestroyMenu();
        PauseMenu.Starfield = (StarField) null;
        this.CMProvider.Dispose(CM.Intro);
        return true;
      }
    }
    else
    {
      if (this.StartedNewGame && this.IntroZoomIn == null && PauseMenu.Starfield != null && PauseMenu.Starfield.IsDisposed)
      {
        PauseMenu.Starfield = (StarField) null;
        ServiceHelper.AddComponent((IGameComponent) (this.IntroZoomIn = new IntroZoomIn(this.Game)));
      }
      if (this.StartedNewGame && this.IntroZoomIn != null && this.IntroZoomIn.IsDisposed)
      {
        this.IntroZoomIn = (IntroZoomIn) null;
        this.CMProvider.Dispose(CM.Intro);
        ServiceHelper.RemoveComponent<PauseMenu>(this);
        return true;
      }
    }
    if ((this.nextMenuLevel ?? this.CurrentMenuLevel) == null)
    {
      this.DestroyMenu();
      return true;
    }
    return this.StartedNewGame;
  }

  protected override bool AllowDismiss() => true;

  private void DestroyMenu() => this.DestroyMenu(true);

  private void DestroyMenu(bool viaSignOut)
  {
    if (viaSignOut)
    {
      ServiceHelper.RemoveComponent<PauseMenu>(this);
    }
    else
    {
      if (!this.Enabled)
        return;
      ServiceHelper.AddComponent((IGameComponent) new TileTransition(ServiceHelper.Game)
      {
        ScreenCaptured = (Action) (() => ServiceHelper.RemoveComponent<PauseMenu>(this))
      });
      this.Enabled = false;
      this.nextMenuLevel = this.CurrentMenuLevel = (MenuLevel) null;
    }
  }

  public override void Draw(GameTime gameTime)
  {
    this.Ready = true;
    if (this.IntroZoomIn == null)
      this.TargetRenderer.DrawFullscreen(Color.Black);
    if (PauseMenu.Starfield != null && !PauseMenu.Starfield.IsDisposed)
      PauseMenu.Starfield.Draw();
    base.Draw(gameTime);
  }

  protected override bool AlwaysShowBackButton() => true;

  [ServiceDependency]
  public IGameService GameService { get; private set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { get; private set; }
}
