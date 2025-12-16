// Decompiled with JetBrains decompiler
// Type: FezGame.Components.MainMenu
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components;

internal class MainMenu : MenuBase
{
  public static MainMenu Instance;
  private SaveSlotSelectionLevel SaveSlotMenuLevel;
  private MenuLevel RealMenuRoot;

  public bool StartedGame { get; private set; }

  public bool ContinuedGame { get; private set; }

  public bool SellingTime { get; private set; }

  public bool HasBought { get; private set; }

  public bool ReturnedToArcade { get; private set; }

  public MainMenu(Game game)
    : base(game)
  {
    this.UpdateOrder = -10;
    this.DrawOrder = 2010;
    MainMenu.Instance = this;
  }

  protected override void PostInitialize()
  {
    if (!Fez.PublicDemo && this.GameState.SaveSlot == -1)
    {
      this.SaveSlotMenuLevel = new SaveSlotSelectionLevel();
      this.MenuLevels.Add((MenuLevel) this.SaveSlotMenuLevel);
      this.SaveSlotMenuLevel.Parent = this.MenuRoot;
      this.nextMenuLevel = (MenuLevel) this.SaveSlotMenuLevel;
      this.SaveSlotMenuLevel.RecoverMainMenu = new Func<bool>(this.RecoverMenuRoot);
      this.RealMenuRoot = this.MenuRoot;
      this.MenuRoot = (MenuLevel) this.SaveSlotMenuLevel;
      this.SaveSlotMenuLevel.RunStart = new Action(((MenuBase) this).StartNewGame);
    }
    else
      this.AddTopElements();
  }

  private bool RecoverMenuRoot()
  {
    if (this.CurrentMenuLevel == null)
      return false;
    this.MenuRoot = this.RealMenuRoot;
    this.AddTopElements();
    this.ChangeMenuLevel(this.MenuRoot);
    return true;
  }

  private void AddTopElements()
  {
    MenuItem menuItem = this.MenuRoot.AddItem("ContinueGame", new Action(((MenuBase) this).ContinueGame), 0);
    menuItem.Disabled = this.GameState.SaveData.IsNew || this.GameState.SaveData.Level == null || this.GameState.SaveData.CanNewGamePlus;
    menuItem.Selectable = !menuItem.Disabled;
    if (this.GameState.IsTrialMode || this.GameState.SaveData.IsNew && !this.GameState.SaveData.CanNewGamePlus)
      this.MenuRoot.AddItem("StartNewGame", new Action(((MenuBase) this).StartNewGame), menuItem.Disabled, 1);
    else
      this.MenuRoot.AddItem("StartNewGame", (Action) (() => this.ChangeMenuLevel(this.StartNewGameMenu)), menuItem.Disabled, 1);
    if (this.GameState.SaveData.CanNewGamePlus)
      this.MenuRoot.AddItem("StartNewGamePlus", new Action(this.NewGamePlus), 2);
    this.MenuRoot.SelectedIndex = this.GameState.SaveData.CanNewGamePlus ? 2 : (this.MenuRoot.Items[0].Selectable ? 0 : 1);
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sAppear.Emit();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    MainMenu.Instance = (MainMenu) null;
  }

  protected override void StartNewGame()
  {
    base.StartNewGame();
    this.GameState.ClearSaveFile();
    this.GameState.SaveData.IsNew = false;
    if (this.GameState.SaveData.HasNewGamePlus)
      this.GameState.SaveData.Level = "GOMEZ_HOUSE_2D";
    this.StartedGame = true;
  }

  protected override void ContinueGame()
  {
    this.sinceSelectorPhaseStarted = 0.0f;
    this.selectorPhase = SelectorPhase.Disappear;
    this.sDisappear.Emit();
    this.ContinuedGame = true;
  }

  protected override void ReturnToArcade()
  {
    if (this.GameState.IsTrialMode)
    {
      this.SellingTime = true;
    }
    else
    {
      this.sinceSelectorPhaseStarted = 0.0f;
      this.selectorPhase = SelectorPhase.Disappear;
      this.GameState.ReturnToArcade();
      this.ReturnedToArcade = true;
    }
  }

  private void NewGamePlus()
  {
    this.GameState.SaveData.Level = "GOMEZ_HOUSE_2D";
    this.GameState.SaveData.IsNewGamePlus = true;
    this.sinceSelectorPhaseStarted = 0.0f;
    this.selectorPhase = SelectorPhase.Disappear;
    this.sDisappear.Emit().Persistent = true;
    this.StartedGame = true;
  }

  protected override bool UpdateEarlyOut()
  {
    return this.ContinuedGame || this.StartedGame || this.HasBought || this.SellingTime || this.ReturnedToArcade;
  }

  protected override bool AllowDismiss() => this.CurrentMenuLevel == this.SaveSlotMenuLevel;

  public override void Draw(GameTime gameTime) => base.Draw(gameTime);
}
