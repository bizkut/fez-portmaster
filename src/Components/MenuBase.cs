// Decompiled with JetBrains decompiler
// Type: FezGame.Components.MenuBase
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using EasyStorage;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.Localization;
using SDL2;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class MenuBase : DrawableGameComponent
{
  protected SpriteBatch SpriteBatch;
  protected MenuLevel CurrentMenuLevel;
  protected MenuLevel MenuRoot;
  protected MenuLevel UnlockNeedsLIVEMenu;
  protected MenuLevel HelpOptionsMenu;
  protected MenuLevel StartNewGameMenu;
  protected MenuLevel ExitToArcadeMenu;
  protected MenuLevel GameSettingsMenu;
  protected MenuLevel AudioSettingsMenu;
  protected MenuLevel VideoSettingsMenu;
  protected LeaderboardsMenuLevel LeaderboardsMenu;
  protected ControlsMenuLevel ControlsMenu;
  public CreditsMenuLevel CreditsMenu;
  protected List<MenuLevel> MenuLevels;
  protected MenuItem StereoMenuItem;
  protected MenuItem SinglethreadedMenuItem;
  protected MenuItem PauseOnLostFocusMenuItem;
  protected MenuItem ResolutionMenuItem;
  protected SaveManagementLevel SaveManagementMenu;
  protected TimeSpan sliderDownLeft;
  public MenuLevel nextMenuLevel;
  protected MenuLevel lastMenuLevel;
  protected GlyphTextRenderer tr;
  protected Mesh Selector;
  protected Mesh Frame;
  protected Mesh Mask;
  protected SoundEffect sAdvanceLevel;
  protected SoundEffect sCancel;
  protected SoundEffect sConfirm;
  protected SoundEffect sCursorUp;
  protected SoundEffect sCursorDown;
  protected SoundEffect sExitGame;
  protected SoundEffect sReturnLevel;
  protected SoundEffect sScreenNarrowen;
  protected SoundEffect sScreenWiden;
  protected SoundEffect sSliderValueDecrease;
  protected SoundEffect sSliderValueIncrease;
  protected SoundEffect sStartGame;
  protected SoundEffect sAppear;
  protected SoundEffect sDisappear;
  protected SelectorPhase selectorPhase;
  protected float sinceSelectorPhaseStarted;
  protected RenderTarget2D CurrentMenuLevelTexture;
  protected RenderTarget2D NextMenuLevelTexture;
  protected Mesh MenuLevelOverlay;
  protected int currentResolution;
  protected ScreenMode currentScreenMode;
  protected ScaleMode currentScaleMode;
  protected bool vsync;
  protected bool hwInstancing;
  protected bool hiDpi;
  protected int msaa;
  protected bool lighting;
  protected Language languageToSet = Culture.Language;
  public bool EndGameMenu;
  protected bool StartedNewGame;
  public bool CursorSelectable;
  public bool CursorClicking;
  protected float SinceMouseMoved = 3f;
  private float sinceRestartNoteShown;
  protected Rectangle? AButtonRect;
  protected Rectangle? BButtonRect;
  protected Rectangle? XButtonRect;
  public static readonly Action SliderAction = (Action) (() => { });
  private Texture2D CanClickCursor;
  private Texture2D PointerCursor;
  private Texture2D ClickedCursor;
  protected bool isDisposed;

  protected MenuBase(Game game)
    : base(game)
  {
  }

  public override void Initialize()
  {
    this.KeyboardState.IgnoreMapping = true;
    CreditsMenuLevel creditsMenuLevel = new CreditsMenuLevel();
    creditsMenuLevel.Title = "Credits";
    creditsMenuLevel.Oversized = true;
    creditsMenuLevel.IsDynamic = true;
    this.CreditsMenu = creditsMenuLevel;
    this.StartNewGameMenu = new MenuLevel()
    {
      Title = "StartNewGameTitle",
      AButtonStarts = true,
      AButtonString = "StartNewGameWithGlyph",
      AButtonAction = new Action(this.StartNewGame)
    };
    this.StartNewGameMenu.AddItem("StartNewGameTextLine", new Action(Util.NullAction));
    this.ExitToArcadeMenu = new MenuLevel()
    {
      Title = "ExitConfirmationTitle",
      AButtonString = "ExitChoiceYes",
      AButtonAction = new Action(this.ReturnToArcade)
    };
    this.ExitToArcadeMenu.AddItem("ReturnToArcadeTextLine", new Action(Util.NullAction));
    LeaderboardsMenuLevel leaderboardsMenuLevel = new LeaderboardsMenuLevel(this);
    leaderboardsMenuLevel.Title = "LeaderboardsTitle";
    leaderboardsMenuLevel.Oversized = true;
    this.LeaderboardsMenu = leaderboardsMenuLevel;
    ControlsMenuLevel controlsMenuLevel = new ControlsMenuLevel(this);
    controlsMenuLevel.Title = "Controls";
    controlsMenuLevel.Oversized = true;
    this.ControlsMenu = controlsMenuLevel;
    this.GameSettingsMenu = new MenuLevel()
    {
      Title = "GameSettings",
      BButtonString = "MenuSaveWithGlyph",
      IsDynamic = true,
      Oversized = true
    };
    this.AudioSettingsMenu = new MenuLevel()
    {
      Title = "AudioSettings",
      BButtonString = "MenuSaveWithGlyph",
      IsDynamic = true,
      Oversized = true
    };
    this.VideoSettingsMenu = new MenuLevel()
    {
      Title = "VideoSettings",
      AButtonString = (string) null,
      IsDynamic = true,
      Oversized = true
    };
    Action refreshVideoApplyString = (Action) (() => this.VideoSettingsMenu.AButtonString = SettingsManager.Resolutions[this.currentResolution].Width != SettingsManager.Settings.Width || SettingsManager.Resolutions[this.currentResolution].Height != SettingsManager.Settings.Height || this.currentScreenMode != SettingsManager.Settings.ScreenMode || this.currentScaleMode != SettingsManager.Settings.ScaleMode || this.vsync != SettingsManager.Settings.VSync || this.hwInstancing != SettingsManager.Settings.HardwareInstancing || this.msaa != SettingsManager.Settings.MultiSampleCount || this.hiDpi != SettingsManager.Settings.HighDPI || this.lighting != SettingsManager.Settings.Lighting ? "MenuApplyWithGlyph" : (string) null);
    this.ResolutionMenuItem = (MenuItem) this.VideoSettingsMenu.AddItem<string>("Resolution", new Action(this.ApplyVideo), false, (Func<string>) (() =>
    {
      if (SettingsManager.Settings.UseCurrentMode)
        return $"{(object) SettingsManager.Settings.Width}x{(object) SettingsManager.Settings.Height}";
      DisplayMode resolution = SettingsManager.Resolutions[this.currentResolution];
      int num1 = resolution.Width / 1280 /*0x0500*/;
      int num2 = resolution.Height / 720;
      return $"{(object) resolution.Width}x{(object) resolution.Height}";
    }), (Action<string, int>) ((lastValue, change) =>
    {
      this.currentResolution += change;
      if (this.currentResolution == SettingsManager.Resolutions.Count)
        this.currentResolution = 0;
      if (this.currentResolution == -1)
        this.currentResolution = SettingsManager.Resolutions.Count - 1;
      refreshVideoApplyString();
    }));
    this.ResolutionMenuItem.UpperCase = true;
    if (SettingsManager.Settings.UseCurrentMode)
    {
      this.ResolutionMenuItem.Disabled = true;
      this.ResolutionMenuItem.Selectable = false;
      ++this.VideoSettingsMenu.SelectedIndex;
    }
    int num3 = SDL.SDL_GetPlatform().Equals("Mac OS X") ? 1 : 0;
    if (num3 != 0)
      this.VideoSettingsMenu.AddItem<string>("HiDpiMode", new Action(this.ApplyVideo), false, (Func<string>) (() => !this.hiDpi ? StaticText.GetString("Off") : StaticText.GetString("On")), (Action<string, int>) ((_, __) =>
      {
        this.hiDpi = !this.hiDpi;
        refreshVideoApplyString();
      })).UpperCase = true;
    this.VideoSettingsMenu.AddItem<string>("ScreenMode", new Action(this.ApplyVideo), false, (Func<string>) (() =>
    {
      if (this.currentScreenMode == ScreenMode.Windowed)
        return StaticText.GetString("Windowed");
      return this.currentScreenMode != ScreenMode.Fullscreen ? StaticText.GetString("Borderless") : StaticText.GetString("Fullscreen");
    }), (Action<string, int>) ((_, inc) =>
    {
      this.currentScreenMode += (ScreenMode) inc;
      if (this.currentScreenMode > ScreenMode.Fullscreen)
        this.currentScreenMode = ScreenMode.Windowed;
      if (this.currentScreenMode < ScreenMode.Windowed)
        this.currentScreenMode = ScreenMode.Fullscreen;
      refreshVideoApplyString();
    })).UpperCase = true;
    this.VideoSettingsMenu.AddItem<string>("ScaleMode", new Action(this.ApplyVideo), false, (Func<string>) (() =>
    {
      if (this.currentScaleMode == ScaleMode.FullAspect)
        return StaticText.GetString("FullAspect");
      return this.currentScaleMode != ScaleMode.PixelPerfect ? StaticText.GetString("Supersampling") : StaticText.GetString("PixelPerfect");
    }), (Action<string, int>) ((_, inc) =>
    {
      this.currentScaleMode += (ScaleMode) inc;
      if (this.currentScaleMode > ScaleMode.Supersampled)
        this.currentScaleMode = ScaleMode.FullAspect;
      if (this.currentScaleMode < ScaleMode.FullAspect)
        this.currentScaleMode = ScaleMode.Supersampled;
      refreshVideoApplyString();
    })).UpperCase = true;
    this.VideoSettingsMenu.AddItem<float>("Brightness", MenuBase.SliderAction, false, (Func<float>) (() => SettingsManager.Settings.Brightness), (Action<float, int>) ((lastValue, change) =>
    {
      float num4 = (double) lastValue > 0.05000000074505806 || change >= 0 ? ((double) lastValue < 0.949999988079071 || change <= 0 ? lastValue + (float) change * 0.05f : 1f) : 0.0f;
      this.GraphicsDevice.SetGamma(SettingsManager.Settings.Brightness = num4);
    })).UpperCase = true;
    bool flag;
    this.VideoSettingsMenu.AddItem<string>("VSync", new Action(this.ApplyVideo), false, (Func<string>) (() => !this.vsync ? StaticText.GetString("Off") : StaticText.GetString("On")), (Action<string, int>) ((_, __) =>
    {
      this.vsync = !this.vsync;
      refreshVideoApplyString();
    })).UpperCase = flag = true;
    MenuItem<string> menuItem1 = this.VideoSettingsMenu.AddItem<string>("HardwareInstancing", new Action(this.ApplyVideo), false, (Func<string>) (() => !this.hwInstancing ? StaticText.GetString("Off") : StaticText.GetString("On")), (Action<string, int>) ((_, __) =>
    {
      this.hwInstancing = !this.hwInstancing;
      refreshVideoApplyString();
    }));
    menuItem1.UpperCase = true;
    if (!SettingsManager.SupportsHardwareInstancing)
    {
      menuItem1.Selectable = false;
      menuItem1.Disabled = true;
    }
    if (SettingsManager.Settings.MultiSampleOption)
    {
      MenuItem<string> menuItem2 = this.VideoSettingsMenu.AddItem<string>("MSAA", new Action(this.ApplyVideo), false, (Func<string>) (() => this.msaa != 0 ? this.msaa.ToString() + "X" : StaticText.GetString("Off")), (Action<string, int>) ((lastValue, change) =>
      {
        if (change > 0)
        {
          if (this.msaa == 0)
            this.msaa = 2;
          else
            this.msaa *= 2;
        }
        else if (this.msaa == 2)
          this.msaa = 0;
        else
          this.msaa /= 2;
        if (this.msaa > SettingsManager.MaxMultiSampleCount)
          this.msaa = SettingsManager.MaxMultiSampleCount;
        refreshVideoApplyString();
      }));
      menuItem2.UpperCase = true;
      if (SettingsManager.MaxMultiSampleCount == 0)
      {
        menuItem2.Selectable = false;
        menuItem2.Disabled = true;
      }
    }
    this.VideoSettingsMenu.AddItem<string>("Lighting", new Action(this.ApplyVideo), false, (Func<string>) (() => !this.lighting ? StaticText.GetString("Off") : StaticText.GetString("On")), (Action<string, int>) ((_, __) =>
    {
      this.lighting = !this.lighting;
      refreshVideoApplyString();
    })).UpperCase = flag = true;
    this.VideoSettingsMenu.AddItem("ResetToDefault", new Action(this.ReturnToVideoDefault));
    if (num3 != 0)
      this.VideoSettingsMenu.OnPostDraw += (Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float>) ((batch, font, tr, alpha) =>
      {
        Viewport viewport = batch.GraphicsDevice.Viewport;
        Vector2 offset = new Vector2(-384f * batch.GraphicsDevice.GetViewScale(), (float) ((double) viewport.Height / 2.0 + 256.0 * (double) batch.GraphicsDevice.GetViewScale()));
        float scale = this.Fonts.SmallFactor * batch.GraphicsDevice.GetViewScale();
        if (this.VideoSettingsMenu.SelectedIndex == 1 && this.selectorPhase == SelectorPhase.Select)
        {
          this.sinceRestartNoteShown = Math.Min(this.sinceRestartNoteShown + 0.05f, 1f);
          tr.DrawCenteredString(batch, this.Fonts.Small, StaticText.GetString("RequiresRestart"), new Color(1f, 1f, 1f, alpha * this.sinceRestartNoteShown), offset, scale);
        }
        else
          this.sinceRestartNoteShown = Math.Max(this.sinceRestartNoteShown - 0.1f, 0.0f);
      });
    this.AudioSettingsMenu.AddItem<float>("SoundVolume", MenuBase.SliderAction, false, (Func<float>) (() => SettingsManager.Settings.SoundVolume), (Action<float, int>) ((lastValue, change) =>
    {
      float num5 = (double) lastValue > 0.05000000074505806 || change >= 0 ? ((double) lastValue < 0.949999988079071 || change <= 0 ? lastValue + (float) change * 0.05f : 1f) : 0.0f;
      this.SoundManager.SoundEffectVolume = SettingsManager.Settings.SoundVolume = num5;
    })).UpperCase = true;
    this.AudioSettingsMenu.AddItem<float>("MusicVolume", MenuBase.SliderAction, false, (Func<float>) (() => SettingsManager.Settings.MusicVolume), (Action<float, int>) ((lastValue, change) =>
    {
      float num6 = (double) lastValue > 0.05000000074505806 || change >= 0 ? ((double) lastValue < 0.949999988079071 || change <= 0 ? lastValue + (float) change * 0.05f : 1f) : 0.0f;
      this.SoundManager.MusicVolume = SettingsManager.Settings.MusicVolume = num6;
    })).UpperCase = true;
    this.AudioSettingsMenu.AddItem("ResetToDefault", new Action(this.ReturnToAudioDefault));
    MenuItem<Language> menuItem3 = this.GameSettingsMenu.AddItem<Language>("Language", MenuBase.SliderAction, false, (Func<Language>) (() => this.languageToSet), (Action<Language, int>) ((lastValue, change) =>
    {
      if (change < 0 && this.languageToSet == Language.English)
        this.languageToSet = Language.Korean;
      else if (change > 0 && this.languageToSet == Language.Korean)
        this.languageToSet = Language.English;
      else
        this.languageToSet += (Language) change;
    }));
    this.GameSettingsMenu.AButtonString = (string) null;
    menuItem3.Selected = (Action) (() => Culture.Language = SettingsManager.Settings.Language = this.languageToSet);
    this.GameSettingsMenu.OnReset = (Action) (() => this.languageToSet = Culture.Language);
    menuItem3.UpperCase = true;
    menuItem3.LocalizeSliderValue = true;
    menuItem3.LocalizationTagFormat = "Language{0}";
    bool hasStereo3D = this.HasStereo3D();
    if (hasStereo3D)
      this.StereoMenuItem = (MenuItem) this.GameSettingsMenu.AddItem<string>(this.GameState.StereoMode ? "Stereo3DOn" : "Stereo3DOff", (Action) (() => { }), false, (Func<string>) (() => string.Empty), (Action<string, int>) ((_, __) => this.ToggleStereo()));
    this.PauseOnLostFocusMenuItem = (MenuItem) this.GameSettingsMenu.AddItem<string>("PauseOnLostFocus", (Action) (() => { }), false, (Func<string>) (() => !SettingsManager.Settings.PauseOnLostFocus ? StaticText.GetString("Off") : StaticText.GetString("On")), (Action<string, int>) ((_, __) => this.TogglePauseOnLostFocus()));
    this.PauseOnLostFocusMenuItem.UpperCase = true;
    this.SinglethreadedMenuItem = (MenuItem) this.GameSettingsMenu.AddItem<string>("Singlethreaded", (Action) (() => { }), false, (Func<string>) (() =>
    {
      string str = SettingsManager.Settings.Singlethreaded ? StaticText.GetString("On") : StaticText.GetString("Off");
      if (PersistentThreadPool.SingleThreaded != SettingsManager.Settings.Singlethreaded)
        str += " *";
      return str;
    }), (Action<string, int>) ((_, __) => this.ToggleSinglethreaded()));
    this.SinglethreadedMenuItem.UpperCase = true;
    this.GameSettingsMenu.AddItem("ResetToDefault", (Action) (() =>
    {
      this.ReturnToGameDefault();
      this.languageToSet = Culture.Language;
    }));
    this.GameSettingsMenu.OnPostDraw += (Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float>) ((batch, font, tr, alpha) =>
    {
      Viewport viewport = batch.GraphicsDevice.Viewport;
      Vector2 offset = new Vector2(-384f * batch.GraphicsDevice.GetViewScale(), (float) ((double) viewport.Height / 2.0 + 256.0 * (double) batch.GraphicsDevice.GetViewScale()));
      float scale = this.Fonts.SmallFactor * batch.GraphicsDevice.GetViewScale();
      int num7 = 2;
      if (hasStereo3D)
        ++num7;
      if (this.GameSettingsMenu.SelectedIndex == num7 && this.selectorPhase == SelectorPhase.Select)
      {
        this.sinceRestartNoteShown = Math.Min(this.sinceRestartNoteShown + 0.05f, 1f);
        tr.DrawCenteredString(batch, this.Fonts.Small, StaticText.GetString("RequiresRestart"), new Color(1f, 1f, 1f, alpha * this.sinceRestartNoteShown), offset, scale);
      }
      else
        this.sinceRestartNoteShown = Math.Max(this.sinceRestartNoteShown - 0.1f, 0.0f);
    });
    this.SaveManagementMenu = new SaveManagementLevel(this);
    this.HelpOptionsMenu = new MenuLevel()
    {
      Title = "HelpOptions"
    };
    this.HelpOptionsMenu.AddItem("Controls", (Action) (() => this.ChangeMenuLevel((MenuLevel) this.ControlsMenu)));
    this.HelpOptionsMenu.AddItem("GameSettings", (Action) (() => this.ChangeMenuLevel(this.GameSettingsMenu)));
    this.HelpOptionsMenu.AddItem("VideoSettings", (Action) (() =>
    {
      FezEngine.Tools.Settings s = SettingsManager.Settings;
      DisplayMode displayMode1 = SettingsManager.Resolutions.FirstOrDefault<DisplayMode>((Func<DisplayMode, bool>) (x => x.Width == s.Width && x.Height == s.Height));
      if ((object) displayMode1 == null)
        displayMode1 = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
      DisplayMode displayMode2 = displayMode1;
      this.currentResolution = SettingsManager.Resolutions.IndexOf(displayMode2);
      if (this.currentResolution == -1 || this.currentResolution >= SettingsManager.Resolutions.Count)
        this.currentResolution = 0;
      this.currentScreenMode = SettingsManager.Settings.ScreenMode;
      this.currentScaleMode = SettingsManager.Settings.ScaleMode;
      this.vsync = SettingsManager.Settings.VSync;
      this.hwInstancing = SettingsManager.Settings.HardwareInstancing;
      this.hiDpi = SettingsManager.Settings.HighDPI;
      this.msaa = SettingsManager.Settings.MultiSampleCount;
      this.lighting = SettingsManager.Settings.Lighting;
      this.ChangeMenuLevel(this.VideoSettingsMenu);
    })).UpperCase = true;
    this.HelpOptionsMenu.AddItem("AudioSettings", (Action) (() => this.ChangeMenuLevel(this.AudioSettingsMenu)));
    if (!Fez.PublicDemo)
      this.HelpOptionsMenu.AddItem("SaveManagementTitle", (Action) (() => this.ChangeMenuLevel((MenuLevel) this.SaveManagementMenu)));
    this.SaveManagementMenu.Parent = this.HelpOptionsMenu;
    this.GameSettingsMenu.Parent = this.HelpOptionsMenu;
    this.AudioSettingsMenu.Parent = this.HelpOptionsMenu;
    this.VideoSettingsMenu.Parent = this.HelpOptionsMenu;
    this.ControlsMenu.Parent = this.HelpOptionsMenu;
    this.UnlockNeedsLIVEMenu = new MenuLevel();
    this.UnlockNeedsLIVEMenu.AddItem("UnlockNeedsLIVE", MenuBase.SliderAction).Selectable = false;
    this.MenuRoot = new MenuLevel();
    MenuItem menuItem4 = this.MenuRoot.AddItem("HelpOptions", (Action) (() => this.ChangeMenuLevel(this.HelpOptionsMenu)));
    if (Fez.PublicDemo)
    {
      menuItem4.Selectable = false;
      menuItem4.Disabled = true;
    }
    this.MenuRoot.AddItem("Credits", (Action) (() => this.ChangeMenuLevel((MenuLevel) this.CreditsMenu)));
    this.CreditsMenu.Parent = this.MenuRoot;
    MenuItem menuItem5 = (MenuItem) null;
    if (this.GameState.IsTrialMode)
      menuItem5 = this.MenuRoot.AddItem("UnlockFullGame", new Action(this.UnlockFullGame));
    MenuItem menuItem6 = this.MenuRoot.AddItem("ReturnToArcade", (Action) (() => this.ChangeMenuLevel(this.ExitToArcadeMenu)));
    if (Fez.PublicDemo)
    {
      menuItem6.Disabled = true;
      if (menuItem5 != null)
        menuItem5.Disabled = true;
      menuItem6.Selectable = false;
      if (menuItem5 != null)
        menuItem5.Selectable = false;
    }
    this.MenuLevels = new List<MenuLevel>()
    {
      this.MenuRoot,
      this.UnlockNeedsLIVEMenu,
      this.StartNewGameMenu,
      this.HelpOptionsMenu,
      this.AudioSettingsMenu,
      this.VideoSettingsMenu,
      this.GameSettingsMenu,
      this.ExitToArcadeMenu,
      (MenuLevel) this.LeaderboardsMenu,
      (MenuLevel) this.ControlsMenu,
      (MenuLevel) this.CreditsMenu,
      (MenuLevel) this.SaveManagementMenu
    };
    foreach (MenuLevel menuLevel in this.MenuLevels)
    {
      if (menuLevel != this.MenuRoot && menuLevel.Parent == null)
        menuLevel.Parent = this.MenuRoot;
    }
    this.nextMenuLevel = this.EndGameMenu ? (MenuLevel) this.CreditsMenu : this.MenuRoot;
    this.GameState.DynamicUpgrade += new Action(this.DynamicUpgrade);
    this.PostInitialize();
    base.Initialize();
  }

  protected virtual void PostInitialize()
  {
  }

  protected override void LoadContent()
  {
    this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
    this.tr = new GlyphTextRenderer(this.Game);
    this.tr.IgnoreKeyboardRemapping = true;
    ContentManager contentManager = this.CMProvider.Get(CM.Menu);
    this.PointerCursor = contentManager.Load<Texture2D>("Other Textures/cursor/CURSOR_POINTER");
    this.CanClickCursor = contentManager.Load<Texture2D>("Other Textures/cursor/CURSOR_CLICKER_A");
    this.ClickedCursor = contentManager.Load<Texture2D>("Other Textures/cursor/CURSOR_CLICKER_B");
    this.sAdvanceLevel = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/AdvanceLevel");
    this.sCancel = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/Cancel");
    this.sConfirm = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/Confirm");
    this.sCursorUp = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/CursorUp");
    this.sCursorDown = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/CursorDown");
    this.sExitGame = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/Menu/ExitGame");
    this.sReturnLevel = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/ReturnLevel");
    this.sScreenNarrowen = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/ScreenNarrowen");
    this.sScreenWiden = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/ScreenWiden");
    this.sSliderValueDecrease = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/SliderValueDecrease");
    this.sSliderValueIncrease = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/SliderValueIncrease");
    this.sStartGame = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/Menu/StartGame");
    this.sAppear = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/Appear");
    this.sDisappear = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/Menu/Disappear");
    this.LeaderboardsMenu.InputManager = this.InputManager;
    this.LeaderboardsMenu.GameState = this.GameState;
    this.LeaderboardsMenu.Font = this.Fonts.Big;
    this.LeaderboardsMenu.MouseState = this.MouseState;
    this.ControlsMenu.FontManager = this.Fonts;
    this.ControlsMenu.CMProvider = this.CMProvider;
    this.CreditsMenu.FontManager = this.Fonts;
    foreach (MenuLevel menuLevel in this.MenuLevels)
    {
      menuLevel.CMProvider = this.CMProvider;
      menuLevel.Initialize();
    }
    Mesh mesh1 = new Mesh();
    DefaultEffect.VertexColored vertexColored1 = new DefaultEffect.VertexColored();
    vertexColored1.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up));
    double width1 = (double) this.GraphicsDevice.Viewport.Width;
    Viewport viewport = this.GraphicsDevice.Viewport;
    double height1 = (double) viewport.Height;
    vertexColored1.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic((float) width1, (float) height1, 0.1f, 100f));
    mesh1.Effect = (BaseEffect) vertexColored1;
    mesh1.DepthWrites = false;
    mesh1.AlwaysOnTop = true;
    mesh1.Culling = CullMode.None;
    this.Selector = mesh1;
    this.Selector.AddLines(new Color[4]
    {
      Color.White,
      Color.White,
      Color.White,
      Color.White
    }, new Vector3(-1f, -1f, 10f), new Vector3(-1f, 1f, 10f), new Vector3(1f, 1f, 10f), new Vector3(1f, -1f, 10f));
    this.Selector.AddLines(new Color[4]
    {
      Color.White,
      Color.White,
      Color.White,
      Color.White
    }, new Vector3(-1f, 1f, 10f), new Vector3(0.0f, 1f, 10f), new Vector3(-1f, -1f, 10f), new Vector3(0.0f, -1f, 10f));
    this.Selector.AddLines(new Color[4]
    {
      Color.White,
      Color.White,
      Color.White,
      Color.White
    }, new Vector3(0.0f, 1f, 10f), new Vector3(1f, 1f, 10f), new Vector3(0.0f, -1f, 10f), new Vector3(1f, -1f, 10f));
    Mesh mesh2 = new Mesh();
    DefaultEffect.VertexColored vertexColored2 = new DefaultEffect.VertexColored();
    vertexColored2.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up));
    viewport = this.GraphicsDevice.Viewport;
    double width2 = (double) viewport.Width;
    viewport = this.GraphicsDevice.Viewport;
    double height2 = (double) viewport.Height;
    vertexColored2.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic((float) width2, (float) height2, 0.1f, 100f));
    mesh2.Effect = (BaseEffect) vertexColored2;
    mesh2.DepthWrites = false;
    mesh2.AlwaysOnTop = true;
    mesh2.Culling = CullMode.None;
    mesh2.Enabled = false;
    this.Frame = mesh2;
    this.Frame.AddLines(new Color[8]
    {
      Color.White,
      Color.White,
      Color.White,
      Color.White,
      Color.White,
      Color.White,
      Color.White,
      Color.White
    }, new Vector3(-1f, -1f, 10f), new Vector3(-1f, 1f, 10f), new Vector3(1f, 1f, 10f), new Vector3(1f, -1f, 10f), new Vector3(-1f, 1f, 10f), new Vector3(1f, 1f, 10f), new Vector3(-1f, -1f, 10f), new Vector3(1f, -1f, 10f));
    Mesh mesh3 = new Mesh();
    DefaultEffect.Textured textured1 = new DefaultEffect.Textured();
    textured1.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up));
    viewport = this.GraphicsDevice.Viewport;
    double width3 = (double) viewport.Width;
    viewport = this.GraphicsDevice.Viewport;
    double height3 = (double) viewport.Height;
    textured1.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic((float) width3, (float) height3, 0.1f, 100f));
    mesh3.Effect = (BaseEffect) textured1;
    mesh3.DepthWrites = false;
    mesh3.AlwaysOnTop = true;
    mesh3.SamplerState = SamplerState.PointClamp;
    this.MenuLevelOverlay = mesh3;
    this.MenuLevelOverlay.AddFace(new Vector3(2f, 2f, 1f), new Vector3(0.0f, 0.0f, 10f), FaceOrientation.Back, true);
    Mesh mesh4 = new Mesh();
    DefaultEffect.Textured textured2 = new DefaultEffect.Textured();
    textured2.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up));
    viewport = this.GraphicsDevice.Viewport;
    double width4 = (double) viewport.Width;
    viewport = this.GraphicsDevice.Viewport;
    double height4 = (double) viewport.Height;
    textured2.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic((float) width4, (float) height4, 0.1f, 100f));
    mesh4.Effect = (BaseEffect) textured2;
    mesh4.DepthWrites = false;
    mesh4.AlwaysOnTop = true;
    this.Mask = mesh4;
    this.Mask.AddFace(new Vector3(2f, 2f, 1f), new Vector3(0.0f, 0.0f, 10f), FaceOrientation.Back, true);
    Waiters.Wait(0.0, new Action(this.Rescale));
    this.RenderToTexture();
  }

  private void ToggleStereo()
  {
    this.GameState.StereoMode = !this.GameState.StereoMode;
    this.StereoMenuItem.Text = this.GameState.StereoMode ? "Stereo3DOn" : "Stereo3DOff";
  }

  private void TogglePauseOnLostFocus()
  {
    SettingsManager.Settings.PauseOnLostFocus = !SettingsManager.Settings.PauseOnLostFocus;
  }

  private void ToggleSinglethreaded()
  {
    SettingsManager.Settings.Singlethreaded = !SettingsManager.Settings.Singlethreaded;
  }

  protected virtual void ReturnToArcade()
  {
  }

  protected virtual void ContinueGame()
  {
  }

  protected virtual void ResumeGame()
  {
  }

  private void ReturnToVideoDefault()
  {
    this.GraphicsDevice.SetGamma(SettingsManager.Settings.Brightness = SettingsManager.Settings.Brightness = 0.5f);
    DisplayMode currentDisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
    this.currentResolution = SettingsManager.Resolutions.IndexOf(currentDisplayMode);
    if (this.currentResolution == -1 || this.currentResolution >= SettingsManager.Resolutions.Count)
      this.currentResolution = 0;
    this.currentScreenMode = ScreenMode.Fullscreen;
    this.currentScaleMode = ScaleMode.FullAspect;
    this.vsync = true;
    this.hwInstancing = SettingsManager.SupportsHardwareInstancing;
    this.hiDpi = false;
    this.msaa = 0;
    this.lighting = true;
    FezEngine.Tools.Settings settings = SettingsManager.Settings;
    settings.UseCurrentMode = false;
    settings.ScreenMode = ScreenMode.Fullscreen;
    settings.Width = currentDisplayMode.Width;
    settings.Height = currentDisplayMode.Height;
    settings.ScreenMode = this.currentScreenMode;
    settings.ScaleMode = this.currentScaleMode;
    settings.VSync = this.vsync;
    settings.HardwareInstancing = this.hwInstancing;
    settings.MultiSampleCount = this.msaa;
    settings.Lighting = this.lighting;
    settings.HighDPI = this.hiDpi;
    SettingsManager.Apply();
    this.Rescale();
  }

  private void ApplyVideo()
  {
    DisplayMode resolution = SettingsManager.Resolutions[this.currentResolution];
    SettingsManager.Settings.Width = resolution.Width;
    SettingsManager.Settings.Height = resolution.Height;
    SettingsManager.Settings.ScreenMode = this.currentScreenMode;
    SettingsManager.Settings.ScaleMode = this.currentScaleMode;
    SettingsManager.Settings.VSync = this.vsync;
    SettingsManager.Settings.HardwareInstancing = this.hwInstancing;
    SettingsManager.Settings.MultiSampleCount = this.msaa;
    SettingsManager.Settings.Lighting = this.lighting;
    SettingsManager.Settings.HighDPI = this.hiDpi;
    SettingsManager.Apply();
    this.VideoSettingsMenu.AButtonString = (string) null;
    this.Rescale();
  }

  private void Rescale()
  {
    BaseEffect effect1 = this.MenuLevelOverlay.Effect;
    Viewport viewport1 = this.GraphicsDevice.Viewport;
    double width1 = (double) viewport1.Width;
    viewport1 = this.GraphicsDevice.Viewport;
    double height1 = (double) viewport1.Height;
    Matrix? nullable1 = new Matrix?(Matrix.CreateOrthographic((float) width1, (float) height1, 0.1f, 100f));
    effect1.ForcedProjectionMatrix = nullable1;
    BaseEffect effect2 = this.Mask.Effect;
    Viewport viewport2 = this.GraphicsDevice.Viewport;
    double width2 = (double) viewport2.Width;
    viewport2 = this.GraphicsDevice.Viewport;
    double height2 = (double) viewport2.Height;
    Matrix? nullable2 = new Matrix?(Matrix.CreateOrthographic((float) width2, (float) height2, 0.1f, 100f));
    effect2.ForcedProjectionMatrix = nullable2;
    BaseEffect effect3 = this.Selector.Effect;
    Viewport viewport3 = this.GraphicsDevice.Viewport;
    double width3 = (double) viewport3.Width;
    viewport3 = this.GraphicsDevice.Viewport;
    double height3 = (double) viewport3.Height;
    Matrix? nullable3 = new Matrix?(Matrix.CreateOrthographic((float) width3, (float) height3, 0.1f, 100f));
    effect3.ForcedProjectionMatrix = nullable3;
    BaseEffect effect4 = this.Frame.Effect;
    Viewport viewport4 = this.GraphicsDevice.Viewport;
    double width4 = (double) viewport4.Width;
    viewport4 = this.GraphicsDevice.Viewport;
    double height4 = (double) viewport4.Height;
    Matrix? nullable4 = new Matrix?(Matrix.CreateOrthographic((float) width4, (float) height4, 0.1f, 100f));
    effect4.ForcedProjectionMatrix = nullable4;
    this.Frame.Scale = new Vector3(this.CurrentMenuLevel == null ? 512f : (this.CurrentMenuLevel.Oversized ? 512f : 352f), 256f, 1f) * this.GraphicsDevice.GetViewScale();
  }

  private void ReturnToAudioDefault()
  {
    this.SoundManager.SoundEffectVolume = SettingsManager.Settings.SoundVolume = this.SoundManager.MusicVolume = SettingsManager.Settings.MusicVolume = 1f;
  }

  private void ReturnToGameDefault()
  {
    if (this.StereoMenuItem != null)
    {
      this.GameState.StereoMode = true;
      this.ToggleStereo();
    }
    SettingsManager.Settings.PauseOnLostFocus = false;
    this.TogglePauseOnLostFocus();
    SettingsManager.Settings.Singlethreaded = true;
    this.ToggleSinglethreaded();
    SettingsManager.Settings.Language = Culture.Language = Culture.LanguageFromCurrentCulture();
  }

  protected virtual void StartNewGame()
  {
    this.sinceSelectorPhaseStarted = 0.0f;
    this.selectorPhase = SelectorPhase.Disappear;
    this.sDisappear.Emit().Persistent = true;
  }

  private void ShowAchievements()
  {
  }

  private void UnlockFullGame()
  {
  }

  private void DynamicUpgrade()
  {
    ServiceHelper.RemoveComponent<MenuBase>(this);
    Console.WriteLine("Removed main menu component");
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    foreach (MenuLevel menuLevel in this.MenuLevels)
      menuLevel.Dispose();
    if (this.CurrentMenuLevelTexture != null)
    {
      this.CurrentMenuLevelTexture.Dispose();
      this.CurrentMenuLevelTexture = (RenderTarget2D) null;
    }
    if (this.NextMenuLevelTexture != null)
    {
      this.NextMenuLevelTexture.Dispose();
      this.NextMenuLevelTexture = (RenderTarget2D) null;
    }
    this.Selector.Dispose();
    this.Frame.Dispose();
    this.Mask.Dispose();
    this.MenuLevelOverlay.Dispose();
    this.GameState.UnPause();
    this.CMProvider.Dispose(CM.Menu);
    this.KeyboardState.IgnoreMapping = false;
    this.GameState.DynamicUpgrade -= new Action(this.DynamicUpgrade);
    this.isDisposed = true;
  }

  protected virtual bool UpdateEarlyOut() => false;

  protected virtual bool AllowDismiss() => false;

  public override void Update(GameTime gameTime)
  {
    this.UpdateSelector((float) gameTime.ElapsedGameTime.TotalSeconds);
    if (this.isDisposed || this.UpdateEarlyOut())
      return;
    MenuLevel activeLevel = this.nextMenuLevel ?? this.CurrentMenuLevel;
    if (activeLevel == null)
    {
      this.DestroyMenu();
    }
    else
    {
      if (activeLevel == this.GameSettingsMenu)
        activeLevel.AButtonString = activeLevel.SelectedItem != activeLevel.Items.Last<MenuItem>() ? (activeLevel.SelectedIndex != 0 || SettingsManager.Settings.Language == this.languageToSet ? (string) null : "MenuApplyWithGlyph") : "MenuApplyWithGlyph";
      Point position = this.MouseState.Position;
      this.SinceMouseMoved += (float) gameTime.ElapsedGameTime.TotalSeconds;
      if (this.MouseState.Movement.X != 0 || this.MouseState.Movement.Y != 0)
        this.SinceMouseMoved = 0.0f;
      MouseButtonState leftButton = this.MouseState.LeftButton;
      if (leftButton.State != MouseButtonStates.Idle)
        this.SinceMouseMoved = 0.0f;
      bool flag = false;
      foreach (MenuItem menuItem in activeLevel.Items)
      {
        if (!menuItem.Hidden && menuItem.Selectable)
        {
          if (menuItem.HoverArea.Contains(position.X, position.Y))
          {
            flag = menuItem.Selected != new Action(Util.NullAction) && menuItem.Selected != MenuBase.SliderAction;
            if (this.MouseState.Movement != Point.Zero)
            {
              int selectedIndex = activeLevel.SelectedIndex;
              activeLevel.SelectedIndex = activeLevel.Items.IndexOf(menuItem);
              if (activeLevel.SelectedIndex > selectedIndex)
                this.sCursorUp.Emit();
              else if (activeLevel.SelectedIndex < selectedIndex)
                this.sCursorDown.Emit();
            }
            leftButton = this.MouseState.LeftButton;
            if (leftButton.State == MouseButtonStates.Pressed)
              this.Select(activeLevel);
          }
          if (menuItem.IsSlider)
          {
            Rectangle hoverArea1 = menuItem.HoverArea;
            hoverArea1.X -= (int) ((double) menuItem.HoverArea.Height * 1.5);
            hoverArea1.Width = menuItem.HoverArea.Height;
            Rectangle hoverArea2 = menuItem.HoverArea;
            hoverArea2.X += menuItem.HoverArea.Width + menuItem.HoverArea.Height / 2;
            hoverArea2.Width = menuItem.HoverArea.Height;
            if (hoverArea1.Contains(position.X, position.Y))
            {
              flag = true;
              leftButton = this.MouseState.LeftButton;
              if (leftButton.State == MouseButtonStates.Pressed)
              {
                this.sSliderValueDecrease.Emit();
                this.CurrentMenuLevel.SelectedItem.Slide(-1);
              }
            }
            if (hoverArea2.Contains(position.X, position.Y))
            {
              flag = true;
              leftButton = this.MouseState.LeftButton;
              if (leftButton.State == MouseButtonStates.Pressed)
              {
                this.sSliderValueIncrease.Emit();
                this.CurrentMenuLevel.SelectedItem.Slide(1);
              }
            }
          }
        }
      }
      Point point = this.MouseState.PositionInViewport();
      if (this.AButtonRect.HasValue && this.AButtonRect.Value.Contains(point.X, point.Y))
      {
        flag = true;
        if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
          this.Select(activeLevel);
      }
      if (this.BButtonRect.HasValue && this.BButtonRect.Value.Contains(point.X, point.Y))
      {
        flag = true;
        if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
          this.UpOneLevel(activeLevel);
      }
      if (activeLevel.XButtonAction != null && this.XButtonRect.HasValue && this.XButtonRect.Value.Contains(point.X, point.Y))
      {
        flag = true;
        if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
          activeLevel.XButtonAction();
      }
      this.CursorSelectable = flag;
      this.CursorClicking = this.CursorSelectable && this.MouseState.LeftButton.State == MouseButtonStates.Down;
      if (!activeLevel.TrapInput)
      {
        if (this.InputManager.Up == FezButtonState.Pressed && activeLevel.MoveUp())
          this.sCursorUp.Emit();
        if (this.InputManager.Down == FezButtonState.Pressed && activeLevel.MoveDown())
          this.sCursorDown.Emit();
        if ((!this.EndGameMenu && this.InputManager.CancelTalk == FezButtonState.Pressed || this.EndGameMenu && this.InputManager.Start == FezButtonState.Pressed || this.InputManager.Back == FezButtonState.Pressed || activeLevel.ForceCancel) && (this.AllowDismiss() || this.CurrentMenuLevel != this.MenuRoot))
          this.UpOneLevel(activeLevel);
        if (this.InputManager.Jump == FezButtonState.Pressed || this.InputManager.Start == FezButtonState.Pressed)
          this.Select(activeLevel);
        if (!Fez.PublicDemo && activeLevel.XButtonAction != null && this.InputManager.GrabThrow == FezButtonState.Pressed)
        {
          this.sConfirm.Emit();
          activeLevel.XButtonAction();
        }
        TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
        if (this.CurrentMenuLevel != null && this.CurrentMenuLevel.SelectedItem != null && this.CurrentMenuLevel.SelectedItem.IsSlider)
        {
          if (this.InputManager.Left == FezButtonState.Down || this.InputManager.Right == FezButtonState.Down)
            this.sliderDownLeft -= elapsedGameTime;
          else
            this.sliderDownLeft = TimeSpan.FromSeconds(0.30000001192092896);
          if (this.InputManager.Left == FezButtonState.Pressed || this.InputManager.Left == FezButtonState.Down && this.sliderDownLeft.Ticks <= 0L)
          {
            if (this.sliderDownLeft.Ticks <= 0L)
              this.sliderDownLeft = TimeSpan.FromSeconds(0.10000000149011612);
            this.sSliderValueDecrease.Emit();
            this.CurrentMenuLevel.SelectedItem.Slide(-1);
          }
          if (this.InputManager.Right == FezButtonState.Pressed || this.InputManager.Right == FezButtonState.Down && this.sliderDownLeft.Ticks <= 0L)
          {
            if (this.sliderDownLeft.Ticks <= 0L)
              this.sliderDownLeft = TimeSpan.FromSeconds(0.10000000149011612);
            this.sSliderValueIncrease.Emit();
            this.CurrentMenuLevel.SelectedItem.Slide(1);
          }
        }
      }
      if (this.selectorPhase == SelectorPhase.Appear)
        return;
      activeLevel.Update(gameTime.ElapsedGameTime);
    }
  }

  private void UpOneLevel(MenuLevel activeLevel)
  {
    if (activeLevel != null && activeLevel.Items.Any<MenuItem>((Func<MenuItem, bool>) (x => x.InError)))
      return;
    this.sCancel.Emit();
    activeLevel.ForceCancel = false;
    if (this.EndGameMenu)
    {
      this.GameState.EndGame = true;
      this.GameState.Restart();
      this.Enabled = false;
      Waiters.Wait(0.40000000596046448, (Action) (() => ServiceHelper.RemoveComponent<MenuBase>(this)));
    }
    else if (activeLevel is SaveSlotSelectionLevel)
    {
      this.sinceSelectorPhaseStarted = 0.0f;
      this.selectorPhase = SelectorPhase.Disappear;
      this.GameState.ReturnToArcade();
    }
    else
    {
      if (activeLevel.Parent == this.HelpOptionsMenu)
        SettingsManager.Save();
      this.ChangeMenuLevel(activeLevel.Parent);
    }
  }

  private void Select(MenuLevel activeLevel)
  {
    if (activeLevel.AButtonAction == new Action(this.StartNewGame) || activeLevel.SelectedItem != null && (activeLevel.SelectedItem.Selected == new Action(this.ContinueGame) || activeLevel.SelectedItem.Selected == new Action(this.StartNewGame)))
      this.sStartGame.Emit().Persistent = true;
    else if (activeLevel.AButtonAction == new Action(this.ReturnToArcade) && !this.GameState.IsTrialMode)
    {
      this.SoundManager.KillSounds();
      this.sExitGame.Emit().Persistent = true;
    }
    else if ((activeLevel.AButtonAction != null || activeLevel.SelectedItem != null) && activeLevel.SelectedItem.Selected != MenuBase.SliderAction)
      this.sConfirm.Emit();
    if (activeLevel.AButtonAction != null)
      activeLevel.AButtonAction();
    else
      activeLevel.Select();
  }

  private void UpdateSelector(float elapsedSeconds)
  {
    Vector3 vector3_1 = Vector3.Zero;
    Vector3 vector3_2 = Vector3.Zero;
    float viewScale = this.GraphicsDevice.GetViewScale();
    if (this.CurrentMenuLevel != null && this.CurrentMenuLevel.SelectedItem != null)
    {
      float num1 = (this.CurrentMenuLevel.Oversized ? 512f : 256f) * viewScale;
      int num2 = this.CurrentMenuLevel.Items.Count<MenuItem>((Func<MenuItem, bool>) (x => !x.Hidden));
      float num3 = this.CurrentMenuLevel.Items.Count == 0 ? 0.0f : (this.CurrentMenuLevel.SelectedItem.Size.Y + this.Fonts.TopSpacing) * this.Fonts.BigFactor;
      int selectedIndex = this.CurrentMenuLevel.SelectedIndex;
      MenuItem menuItem = this.CurrentMenuLevel.Items[selectedIndex];
      vector3_1 = new Vector3((menuItem.Size + new Vector2(this.Fonts.SideSpacing * 2f, this.Fonts.TopSpacing)) * this.Fonts.BigFactor / 2f, 1f);
      if (num2 > 10)
      {
        bool flag = false;
        switch (Culture.Language)
        {
          case Language.English:
          case Language.Chinese:
          case Language.Japanese:
          case Language.Korean:
            for (int index = 0; index <= this.CurrentMenuLevel.SelectedIndex; ++index)
            {
              if (this.CurrentMenuLevel.Items[index].Hidden)
                --selectedIndex;
            }
            float num4 = 5f;
            if (selectedIndex == num2 - 1)
              vector3_2 = new Vector3(0.0f, (float) (((double) num4 - 9.0) * (double) num3 - (double) num3 / 2.0), 0.0f);
            else if (selectedIndex < 8)
            {
              vector3_2 = new Vector3(num1 / 2f, (float) (((double) num4 - (double) selectedIndex) * (double) num3 - (double) num3 / 2.0), 0.0f);
            }
            else
            {
              selectedIndex -= 8;
              vector3_2 = new Vector3((float) (-(double) num1 / 2.0), (float) (((double) num4 - (double) selectedIndex) * (double) num3 - (double) num3 / 2.0), 0.0f);
            }
            if (flag && selectedIndex != num2 - 1)
              vector3_1 = vector3_1 * this.Fonts.SmallFactor / this.Fonts.BigFactor;
            string text = menuItem.ToString();
            float num5 = (float) this.Game.GraphicsDevice.Viewport.Width * 0.45f;
            SpriteFont small = this.Fonts.Small;
            double maxTextSize = ((double) num5 + (double) vector3_2.X / 2.0) / ((double) this.Fonts.SmallFactor * (double) viewScale);
            string str = WordWrap.Split(text, small, (float) maxTextSize);
            int num6 = 0;
            foreach (char ch in str)
            {
              if (ch == '\n')
                ++num6;
            }
            if (num6 > 0)
            {
              vector3_1.Y *= (float) (1 + num6);
              break;
            }
            break;
          default:
            flag = true;
            goto case Language.English;
        }
      }
      else
      {
        float num7 = (float) num2 / 2f;
        for (int index = 0; index <= this.CurrentMenuLevel.SelectedIndex; ++index)
        {
          if (this.CurrentMenuLevel.Items[index].Hidden)
            --selectedIndex;
        }
        vector3_2 = new Vector3(0.0f, (float) (((double) num7 - (double) selectedIndex) * (double) num3 - (double) num3 / 2.0), 0.0f);
      }
    }
    this.sinceSelectorPhaseStarted += elapsedSeconds;
    switch (this.selectorPhase)
    {
      case SelectorPhase.Appear:
      case SelectorPhase.Disappear:
        Group group1 = this.Selector.Groups[0];
        Group group2 = this.Selector.Groups[1];
        Group group3 = this.Selector.Groups[2];
        this.Frame.Enabled = false;
        this.Selector.Material.Opacity = 1f;
        this.Selector.Enabled = true;
        this.Selector.Position = Vector3.Zero;
        this.Selector.Scale = Vector3.One;
        float num8 = Easing.EaseInOut((double) FezMath.Saturate(this.sinceSelectorPhaseStarted / 0.75f), EasingType.Sine, EasingType.Cubic);
        if (this.selectorPhase == SelectorPhase.Disappear)
          num8 = 1f - num8;
        group2.Enabled = group3.Enabled = (double) num8 > 0.5;
        float x1 = (this.nextMenuLevel.Oversized ? 512f : 352f) * viewScale;
        float num9 = FezMath.Saturate((float) (((double) num8 - 0.5) * 2.0));
        float num10 = FezMath.Saturate(num8 * 2f);
        group1.Scale = new Vector3(x1, 256f * num10 * viewScale, 1f);
        group2.Scale = new Vector3(x1 * num9, 256f * viewScale, 1f);
        group2.Position = new Vector3((float) (-(double) x1 * (1.0 - (double) num9)), 0.0f, 1f);
        group3.Scale = new Vector3(x1 * num9, 256f * viewScale, 1f);
        group3.Position = new Vector3(x1 * (1f - num9), 0.0f, 1f);
        if ((double) num8 <= 0.0 && this.selectorPhase == SelectorPhase.Disappear && !this.StartedNewGame)
          this.DestroyMenu();
        if ((double) num8 < 1.0 || this.selectorPhase != SelectorPhase.Appear)
          break;
        this.selectorPhase = SelectorPhase.Shrink;
        group1.Scale = group2.Scale = group3.Scale = Vector3.One;
        Group group4 = group2;
        Vector3 vector3_3;
        group3.Position = vector3_3 = Vector3.Zero;
        Vector3 vector3_4 = vector3_3;
        group4.Position = vector3_4;
        Mesh frame = this.Frame;
        Mesh selector = this.Selector;
        vector3_3 = new Vector3(x1, 256f * viewScale, 1f);
        Vector3 vector3_5 = vector3_3;
        selector.Scale = vector3_5;
        Vector3 vector3_6 = vector3_3;
        frame.Scale = vector3_6;
        this.Frame.Enabled = true;
        this.sinceSelectorPhaseStarted = 0.0f;
        this.CurrentMenuLevel = this.nextMenuLevel;
        this.CurrentMenuLevelTexture = this.NextMenuLevelTexture;
        break;
      case SelectorPhase.Shrink:
        float amount1 = Easing.EaseInOut((double) FezMath.Saturate(this.sinceSelectorPhaseStarted * 2.5f), EasingType.Sine, EasingType.Cubic);
        if (this.CurrentMenuLevel.SelectedItem == null || !this.CurrentMenuLevel.SelectedItem.Selectable)
        {
          this.Selector.Material.Opacity = 0.0f;
        }
        else
        {
          this.Selector.Material.Opacity = 1f;
          this.Selector.Scale = Vector3.Lerp(new Vector3((this.lastMenuLevel ?? this.CurrentMenuLevel).Oversized ? 512f : 352f, 256f, 1f) * viewScale, vector3_1, amount1);
          this.Selector.Position = Vector3.Lerp(Vector3.Zero, vector3_2, amount1);
        }
        this.Frame.Scale = Vector3.Lerp(new Vector3((this.lastMenuLevel ?? this.CurrentMenuLevel).Oversized ? 512f : 352f, 256f, 1f) * viewScale, new Vector3(this.CurrentMenuLevel.Oversized ? 512f : 352f, 256f, 1f) * viewScale, amount1);
        if ((double) amount1 < 1.0)
          break;
        this.selectorPhase = SelectorPhase.Select;
        break;
      case SelectorPhase.Grow:
        float amount2 = 1f - Easing.EaseInOut((double) FezMath.Saturate(this.sinceSelectorPhaseStarted / 0.3f), EasingType.Sine, EasingType.Quadratic);
        if (this.CurrentMenuLevel.SelectedItem == null || !this.CurrentMenuLevel.SelectedItem.Selectable)
        {
          this.Selector.Material.Opacity = 0.0f;
        }
        else
        {
          this.Selector.Material.Opacity = 1f;
          this.Selector.Scale = Vector3.Lerp(new Vector3(this.nextMenuLevel.Oversized ? 512f : 352f, 256f, 1f) * viewScale, vector3_1, amount2);
          this.Selector.Position = Vector3.Lerp(Vector3.Zero, vector3_2, amount2);
        }
        this.Frame.Scale = Vector3.Lerp(new Vector3(this.CurrentMenuLevel.Oversized ? 512f : 352f, 256f, 1f) * viewScale, new Vector3(this.nextMenuLevel.Oversized ? 512f : 352f, 256f, 1f) * viewScale, 1f - amount2);
        if ((double) amount2 > 0.0)
          break;
        this.lastMenuLevel = this.CurrentMenuLevel;
        this.CurrentMenuLevel = this.nextMenuLevel;
        this.CurrentMenuLevelTexture = this.NextMenuLevelTexture;
        if (this.CurrentMenuLevel.SelectedItem == null || !this.CurrentMenuLevel.SelectedItem.Selectable)
        {
          this.CurrentMenuLevel.Reset();
          this.selectorPhase = SelectorPhase.Select;
        }
        else
        {
          this.CurrentMenuLevel.Reset();
          this.selectorPhase = SelectorPhase.FadeIn;
        }
        this.sinceSelectorPhaseStarted = 0.0f;
        break;
      case SelectorPhase.Select:
        if (this.CurrentMenuLevel.SelectedItem == null || !this.CurrentMenuLevel.SelectedItem.Selectable)
        {
          this.Selector.Material.Opacity = 0.0f;
          break;
        }
        this.Selector.Material.Opacity = 1f;
        this.Selector.Scale = Vector3.Lerp(this.Selector.Scale, vector3_1, 0.3f);
        this.Selector.Position = Vector3.Lerp(this.Selector.Position, vector3_2, 0.3f);
        break;
      case SelectorPhase.FadeIn:
        float amount3 = Easing.EaseInOut((double) FezMath.Saturate(this.sinceSelectorPhaseStarted / 0.25f), EasingType.Sine, EasingType.Cubic);
        this.Selector.Material.Opacity = amount3;
        this.Selector.Scale = Vector3.Lerp(this.Selector.Scale, vector3_1, 0.3f);
        this.Selector.Position = Vector3.Lerp(this.Selector.Position, vector3_2, 0.3f);
        float x2 = (this.CurrentMenuLevel.Oversized ? 512f : 352f) * viewScale;
        if ((double) this.Frame.Scale.X != (double) x2)
          this.Frame.Scale = Vector3.Lerp(new Vector3((this.lastMenuLevel ?? this.CurrentMenuLevel).Oversized ? 512f : 352f, 256f, 1f) * viewScale, new Vector3(x2, 256f * viewScale, 1f), amount3);
        if ((double) amount3 < 1.0)
          break;
        this.selectorPhase = SelectorPhase.Select;
        this.sinceSelectorPhaseStarted = 0.0f;
        break;
    }
  }

  private void DestroyMenu()
  {
    ServiceHelper.RemoveComponent<MenuBase>(this);
    this.nextMenuLevel = this.CurrentMenuLevel = (MenuLevel) null;
  }

  public bool ChangeMenuLevel(MenuLevel next, bool silent = false)
  {
    if (this.CurrentMenuLevel == null)
      return false;
    bool flag1 = this.CurrentMenuLevel.SelectedItem == null || !this.CurrentMenuLevel.SelectedItem.Selectable;
    this.selectorPhase = flag1 ? SelectorPhase.FadeIn : SelectorPhase.Grow;
    bool flag2 = next == this.CurrentMenuLevel.Parent;
    if (this.CurrentMenuLevel.OnClose != null)
      this.CurrentMenuLevel.OnClose();
    if (next == null)
    {
      this.ResumeGame();
      return true;
    }
    this.nextMenuLevel = next;
    this.nextMenuLevel.Reset();
    this.RenderToTexture();
    this.sinceSelectorPhaseStarted = 0.0f;
    this.lastMenuLevel = this.CurrentMenuLevel;
    if (flag1)
    {
      this.CurrentMenuLevel = this.nextMenuLevel;
      this.CurrentMenuLevelTexture = this.NextMenuLevelTexture;
      if (this.CurrentMenuLevel == null)
        this.DestroyMenu();
    }
    else if (!silent)
    {
      if (flag2)
        this.sReturnLevel.Emit();
      else
        this.sAdvanceLevel.Emit();
      if (this.lastMenuLevel.Oversized && !this.CurrentMenuLevel.Oversized)
        this.sScreenNarrowen.Emit();
    }
    if (!this.lastMenuLevel.Oversized && this.CurrentMenuLevel.Oversized)
      this.sScreenWiden.Emit();
    return true;
  }

  private void RenderToTexture()
  {
    float viewScale = this.GraphicsDevice.GetViewScale();
    if (this.CurrentMenuLevel != null)
    {
      if (this.CurrentMenuLevelTexture != null)
      {
        this.CurrentMenuLevelTexture.Tag = (object) "DISPOSED";
        this.CurrentMenuLevelTexture.Dispose();
      }
      this.CurrentMenuLevelTexture = new RenderTarget2D(this.GraphicsDevice, FezMath.Round((double) (2 * (this.CurrentMenuLevel.Oversized ? 512 /*0x0200*/ : 352)) * (double) viewScale), (int) (512.0 * (double) viewScale), false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
      this.CurrentMenuLevelTexture.Tag = (object) ("Current | " + this.CurrentMenuLevel.Title);
      this.GraphicsDevice.SetRenderTarget(this.CurrentMenuLevelTexture);
      this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
      this.SpriteBatch.BeginPoint();
      this.DrawLevel(this.CurrentMenuLevel, true);
      this.SpriteBatch.End();
      this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
    }
    if (this.nextMenuLevel == null)
      return;
    if (this.NextMenuLevelTexture != null)
    {
      this.NextMenuLevelTexture.Tag = (object) "DISPOSED";
      this.NextMenuLevelTexture.Dispose();
    }
    this.NextMenuLevelTexture = new RenderTarget2D(this.GraphicsDevice, FezMath.Round((double) (2 * (this.nextMenuLevel.Oversized ? 512 /*0x0200*/ : 352)) * (double) viewScale), (int) (512.0 * (double) viewScale), false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PlatformContents);
    this.NextMenuLevelTexture.Tag = (object) ("Next | " + this.nextMenuLevel.Title);
    this.GraphicsDevice.SetRenderTarget(this.NextMenuLevelTexture);
    this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
    this.SpriteBatch.BeginPoint();
    this.DrawLevel(this.nextMenuLevel, true);
    this.SpriteBatch.End();
    this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
  }

  public override void Draw(GameTime gameTime)
  {
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    float viewScale = this.GraphicsDevice.GetViewScale();
    Viewport viewport = this.GraphicsDevice.Viewport;
    int num = Culture.IsCJK ? -1 : 1;
    float scale1 = (Culture.IsCJK ? this.Fonts.BigFactor + 0.25f : this.Fonts.BigFactor + 1f) * viewScale;
    Vector2 vector2 = new Vector2((float) viewport.Width / 2f, (float) ((double) viewport.Height / 2.0 - 256.0 * (double) viewScale - (double) scale1 * (25.0 + (double) this.Fonts.TopSpacing * 9.0) + (double) this.Fonts.TopSpacing * (double) scale1 * (double) num));
    this.Mask.Position = this.Selector.Position;
    this.Mask.Scale = this.Selector.Scale;
    bool isCjk = Culture.IsCJK;
    if (this.selectorPhase != SelectorPhase.Select && this.selectorPhase != SelectorPhase.Disappear)
    {
      graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.MenuWipe));
      this.Mask.Draw();
      graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    }
    if (this.selectorPhase == SelectorPhase.Grow)
    {
      this.MenuLevelOverlay.Scale = new Vector3((this.nextMenuLevel.Oversized ? 512f : 352f) * viewScale, 256f * viewScale, 1f);
      graphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.MenuWipe);
      this.MenuLevelOverlay.Texture = (Dirtyable<Texture>) (Texture) this.NextMenuLevelTexture;
      this.MenuLevelOverlay.Draw();
      graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
      if (!isCjk)
        this.SpriteBatch.BeginPoint();
      else
        this.SpriteBatch.BeginLinear();
      if (this.nextMenuLevel.Title != null)
        this.tr.DrawCenteredString(this.SpriteBatch, this.Fonts.Big, this.nextMenuLevel.Title, new Color(1f, 1f, 1f, this.sinceSelectorPhaseStarted / 0.3f), new Vector2(0.0f, vector2.Y), scale1);
      this.SpriteBatch.End();
    }
    else
      graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    if (this.selectorPhase == SelectorPhase.Shrink)
    {
      this.MenuLevelOverlay.Scale = new Vector3((this.CurrentMenuLevel.Oversized ? 512f : 352f) * viewScale, 256f * viewScale, 1f);
      this.MenuLevelOverlay.Texture = (Dirtyable<Texture>) (Texture) this.CurrentMenuLevelTexture;
      this.MenuLevelOverlay.Draw();
      if (!isCjk)
        this.SpriteBatch.BeginPoint();
      else
        this.SpriteBatch.BeginLinear();
      if (this.nextMenuLevel.Title != null)
        this.tr.DrawCenteredString(this.SpriteBatch, this.Fonts.Big, this.nextMenuLevel.Title, Color.White, new Vector2(0.0f, vector2.Y), scale1);
      this.SpriteBatch.End();
    }
    if ((this.selectorPhase == SelectorPhase.Select || this.selectorPhase == SelectorPhase.FadeIn) && this.CurrentMenuLevel != null)
    {
      if (!this.CurrentMenuLevel.IsDynamic)
      {
        this.MenuLevelOverlay.Scale = new Vector3((this.CurrentMenuLevel.Oversized ? 512f : 352f) * viewScale, 256f * viewScale, 1f);
        this.MenuLevelOverlay.Texture = (Dirtyable<Texture>) (Texture) this.CurrentMenuLevelTexture;
        this.MenuLevelOverlay.Draw();
      }
      if (!isCjk)
        this.SpriteBatch.BeginPoint();
      else
        this.SpriteBatch.BeginLinear();
      if (this.CurrentMenuLevel.IsDynamic)
        this.DrawLevel(this.CurrentMenuLevel, false);
      if (this.CurrentMenuLevel.Title != null)
        this.tr.DrawCenteredString(this.SpriteBatch, this.Fonts.Big, this.CurrentMenuLevel.Title, Color.White, new Vector2(0.0f, vector2.Y), scale1);
      this.SpriteBatch.End();
    }
    this.Selector.Draw();
    this.Frame.Draw();
    if (this.CurrentMenuLevel != null && this.selectorPhase != SelectorPhase.Disappear)
      this.DrawButtons();
    this.SpriteBatch.BeginPoint();
    float scale2 = viewScale * 2f;
    Point point = this.MouseState.PositionInViewport();
    this.SpriteBatch.Draw(this.CursorClicking ? this.ClickedCursor : (this.CursorSelectable ? this.CanClickCursor : this.PointerCursor), new Vector2((float) point.X - scale2 * 11.5f, (float) point.Y - scale2 * 8.5f), new Rectangle?(), new Color(1f, 1f, 1f, FezMath.Saturate((float) (1.0 - ((double) this.SinceMouseMoved - 2.0)))), 0.0f, Vector2.Zero, scale2, SpriteEffects.None, 0.0f);
    this.SpriteBatch.End();
  }

  protected virtual bool AlwaysShowBackButton() => false;

  private void DrawButtons()
  {
    Viewport viewport = this.GraphicsDevice.Viewport;
    float viewScale = this.GraphicsDevice.GetViewScale();
    float num = this.Frame.Scale.X;
    if (512.0 * (double) viewScale > (double) viewport.Width / 2.0)
      num = 352f * viewScale;
    Vector2 vector2_1 = new Vector2((float) ((double) viewport.Width / 2.0 + (double) num - 5.0), (float) ((double) viewport.Height / 2.0 + 512.0 * (double) viewScale / 2.0 + 5.0 + (double) this.Fonts.TopSpacing * (double) this.Fonts.BigFactor));
    MenuLevel menuLevel = this.selectorPhase == SelectorPhase.Grow ? this.nextMenuLevel : this.CurrentMenuLevel;
    bool flag1 = (this.AlwaysShowBackButton() || menuLevel != this.MenuRoot) && (!this.EndGameMenu || menuLevel != this.CreditsMenu);
    if (menuLevel is SaveSlotSelectionLevel)
      flag1 = true;
    bool flag2 = menuLevel.XButtonString != null;
    bool flag3 = menuLevel.AButtonString != null;
    if (menuLevel == this.VideoSettingsMenu && menuLevel.SelectedIndex == menuLevel.Items.Count - 1)
      flag3 = true;
    if (flag3 & flag1 & flag2)
    {
      switch (Culture.TwoLetterISOLanguageName)
      {
        case "en":
          vector2_1.X += 60f;
          break;
        case "fr":
          vector2_1.X += 230f;
          break;
        case "de":
          vector2_1.X += 210f;
          break;
        case "es":
          vector2_1.X += 230f;
          break;
        case "it":
          vector2_1.X += 125f;
          break;
        case "pt":
          vector2_1.X += 185f;
          break;
      }
    }
    if (menuLevel == this.LeaderboardsMenu)
      vector2_1.X += 45f;
    this.SpriteBatch.BeginPoint();
    SpriteFont small = this.Fonts.Small;
    float scale = this.Fonts.SmallFactor * viewScale;
    if (flag1)
    {
      string str = menuLevel.BButtonString ?? StaticText.GetString("MenuBackWithGlyph");
      if (!GamepadState.AnyConnected)
        str = str.Replace("{B}", "{BACK}");
      Vector2 vector2_2 = small.MeasureString(this.tr.FillInGlyphs(str.ToUpper(CultureInfo.InvariantCulture))) * scale;
      Vector2 position = vector2_1 - vector2_2 * Vector2.UnitX;
      this.tr.DrawShadowedText(this.SpriteBatch, small, str.ToUpper(CultureInfo.InvariantCulture), position, new Color(1f, 0.5f, 0.5f, 1f), scale);
      this.BButtonRect = new Rectangle?(new Rectangle((int) position.X, (int) position.Y, (int) vector2_2.X, (int) vector2_2.Y));
      vector2_1 = position - this.tr.Margin * Vector2.UnitX / 4f;
    }
    else
      this.BButtonRect = new Rectangle?();
    if (flag2)
    {
      Vector2 vector2_3 = small.MeasureString(this.tr.FillInGlyphs(menuLevel.XButtonString.ToUpper(CultureInfo.InvariantCulture))) * scale;
      Vector2 position = vector2_1 - vector2_3 * Vector2.UnitX;
      this.tr.DrawShadowedText(this.SpriteBatch, small, menuLevel.XButtonString.ToUpper(CultureInfo.InvariantCulture), position, new Color(0.5f, 0.5f, 1f, 1f), scale);
      this.XButtonRect = new Rectangle?(new Rectangle((int) position.X, (int) position.Y, (int) vector2_3.X, (int) vector2_3.Y));
      vector2_1 = position - this.tr.Margin * Vector2.UnitX / 4f;
    }
    else
      this.XButtonRect = new Rectangle?();
    if (flag3)
    {
      string str = menuLevel.AButtonString ?? StaticText.GetString("MenuApplyWithGlyph");
      if (!GamepadState.AnyConnected)
        str = str.Replace("{A}", "{START}");
      Vector2 vector2_4 = small.MeasureString(this.tr.FillInGlyphs(str.ToUpper(CultureInfo.InvariantCulture))) * scale;
      Vector2 position = vector2_1 - vector2_4 * Vector2.UnitX;
      this.tr.DrawShadowedText(this.SpriteBatch, small, str.ToUpper(CultureInfo.InvariantCulture), position, new Color(0.5f, 1f, 0.5f, 1f), scale);
      this.AButtonRect = new Rectangle?(new Rectangle((int) position.X, (int) position.Y, (int) vector2_4.X, (int) vector2_4.Y));
      Vector2 vector2_5 = position - this.tr.Margin * Vector2.UnitX / 4f;
    }
    else
      this.AButtonRect = new Rectangle?();
    this.SpriteBatch.End();
  }

  private void DrawLevel(MenuLevel level, bool toTexture)
  {
    float viewScale = this.GraphicsDevice.GetViewScale();
    float num1 = toTexture ? 512f * viewScale : (float) this.GraphicsDevice.Viewport.Height;
    bool flag1 = false;
    switch (Culture.Language)
    {
      case Language.English:
      case Language.Chinese:
      case Language.Japanese:
      case Language.Korean:
        lock (level)
        {
          SpriteFont font1 = !Culture.IsCJK || (double) viewScale <= 1.5 ? this.Fonts.Small : this.Fonts.Big;
          int num2 = 0;
          for (int index = 0; index < level.Items.Count; ++index)
          {
            if (!level.Items[index].Hidden)
              ++num2;
          }
          float num3 = (level.Oversized ? 512f : 256f) * viewScale;
          for (int index1 = 0; index1 < level.Items.Count; ++index1)
          {
            MenuItem menuItem = level.Items[index1];
            if (!menuItem.Hidden)
            {
              bool flag2 = false;
              string text = menuItem.ToString();
              Vector2 vector2_1 = this.tr.MeasureWithGlyphs(this.Fonts.Big, text, viewScale);
              if (string.IsNullOrEmpty(menuItem.Text))
                vector2_1 = this.Fonts.Big.MeasureString("A");
              menuItem.Size = vector2_1;
              float num4 = level.Items.Count == 0 ? 0.0f : (menuItem.Size.Y + this.Fonts.TopSpacing) * this.Fonts.BigFactor;
              float scale = this.Fonts.BigFactor * viewScale;
              if (Culture.IsCJK && (double) viewScale <= 1.5)
                scale *= 2f;
              int num5 = index1;
              Vector3 vector;
              if (num2 > 10)
              {
                for (int index2 = 0; index2 <= index1; ++index2)
                {
                  if (level.Items[index2].Hidden)
                    --num5;
                }
                flag2 = num2 > 10 && num5 != num2 - 1;
                if (flag1)
                  scale = (menuItem.IsGamerCard | flag2 ? this.Fonts.SmallFactor : this.Fonts.BigFactor) * viewScale;
                float num6 = 5f;
                if (num5 == num2 - 1)
                  vector = new Vector3(0.0f, (float) (((double) num6 - 9.0) * (double) num4 - (double) num4 / 2.0), 0.0f);
                else if (num5 < 8)
                {
                  vector = new Vector3((float) (-(double) num3 / 2.0), (float) (((double) num6 - (double) num5) * (double) num4 - (double) num4 / 2.0), 0.0f);
                }
                else
                {
                  int num7 = num5 - 8;
                  vector = new Vector3(num3 / 2f, (float) (((double) num6 - (double) num7) * (double) num4 - (double) num4 / 2.0), 0.0f);
                }
                if (flag2)
                {
                  float num8 = (float) this.Game.GraphicsDevice.Viewport.Width * 0.45f;
                  text = WordWrap.Split(text, font1, (num8 - vector.X / 2f) / scale);
                  int num9 = 0;
                  foreach (char ch in text)
                  {
                    if (ch == '\n')
                      ++num9;
                  }
                  if (num9 > 0)
                  {
                    vector2_1 = this.tr.MeasureWithGlyphs(this.Fonts.Small, text, viewScale);
                    num4 = (vector2_1.Y + this.Fonts.TopSpacing) * this.Fonts.SmallFactor;
                    menuItem.Size = new Vector2(vector2_1.X, menuItem.Size.Y);
                  }
                  else if (flag1)
                  {
                    num4 = level.Items.Count == 0 ? 0.0f : (menuItem.Size.Y + this.Fonts.TopSpacing) * this.Fonts.SmallFactor;
                    vector2_1.X *= this.Fonts.SmallFactor / this.Fonts.BigFactor;
                  }
                }
              }
              else
              {
                float num10 = (float) num2 / 2f;
                for (int index3 = 0; index3 <= index1; ++index3)
                {
                  if (level.Items[index3].Hidden)
                    --num5;
                }
                vector = new Vector3(0.0f, (float) (((double) num10 - (double) num5) * (double) num4 - (double) num4 / 2.0), 0.0f);
              }
              vector.Y *= -1f;
              vector.Y += num1 / 2f;
              vector.Y -= num4 / 2f;
              if (Culture.IsCJK)
                vector.Y += viewScale * 4f;
              SpriteFont font2 = !(menuItem.IsGamerCard | flag2) || Culture.IsCJK ? font1 : this.Fonts.Small;
              Color color = menuItem.Disabled ? new Color(0.2f, 0.2f, 0.2f, 1f) : (menuItem.InError ? new Color(1f, 0.0f, 0.0f, 1f) : new Color(1f, 1f, 1f, 1f));
              if (menuItem.IsGamerCard)
                color = new Color(0.5f, 1f, 0.5f, 1f);
              this.tr.DrawCenteredString(this.SpriteBatch, font2, text, color, vector.XY(), scale);
              Vector2 vector2_2 = this.tr.MeasureWithGlyphs(font2, text, scale);
              Point point;
              point.X = (int) ((double) this.GraphicsDevice.PresentationParameters.BackBufferWidth / 2.0 + (double) vector.X - (double) vector2_2.X / 2.0);
              point.Y = (int) ((double) vector.Y + (double) this.GraphicsDevice.PresentationParameters.BackBufferHeight / 2.0 - (double) num1 / 2.0);
              menuItem.HoverArea = new Rectangle(point.X, point.Y, (int) vector2_2.X, (int) vector2_2.Y);
              if (menuItem.IsSlider && level.SelectedItem == menuItem)
              {
                vector.Y += 7f * scale;
                if (flag2 & flag1 && (double) scale / (double) viewScale < 2.0)
                  vector.Y -= 4f * viewScale;
                float num11 = (float) ((double) viewScale * 20.0 * (double) scale / ((double) this.Fonts.BigFactor * (double) viewScale));
                if (Culture.IsCJK)
                {
                  num11 *= 0.475f * viewScale;
                  vector.Y += viewScale * 5f;
                }
                this.tr.DrawCenteredString(this.SpriteBatch, this.Fonts.Big, "{LA}", new Color(1f, 1f, 1f, 1f), new Vector2((float) ((double) vector.X - (double) vector2_1.X / 2.0 * (double) this.Fonts.BigFactor - (double) num11 * 2.0), vector.Y), (Culture.IsCJK ? 0.2f : 1f) * viewScale);
                this.tr.DrawCenteredString(this.SpriteBatch, this.Fonts.Big, "{RA}", new Color(1f, 1f, 1f, 1f), new Vector2((float) ((double) vector.X + (double) vector2_1.X / 2.0 * (double) this.Fonts.BigFactor + (double) num11 * 2.0), vector.Y), (Culture.IsCJK ? 0.2f : 1f) * viewScale);
              }
            }
          }
          Viewport viewport = this.GraphicsDevice.Viewport;
          int width = FezMath.Round((double) (2 * (level.Oversized ? 512 /*0x0200*/ : 352)) * (double) viewScale);
          int height = (int) (512.0 * (double) viewScale);
          if (viewport.Width == width && viewport.Height == height)
          {
            level.PostDraw(this.SpriteBatch, font1, this.tr, 1f);
            break;
          }
          this.SpriteBatch.End();
          this.GraphicsDevice.Viewport = new Viewport(0, 0, width, height);
          this.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, this.GraphicsDevice.SamplerStates[0], (DepthStencilState) null, RasterizerState.CullCounterClockwise, (Effect) null, Matrix.CreateTranslation(new Vector3((float) (viewport.Width - width) / 2f, (float) (viewport.Height - height) / 2f, 0.0f)));
          level.PostDraw(this.SpriteBatch, font1, this.tr, 1f);
          this.GraphicsDevice.Viewport = viewport;
          this.SpriteBatch.End();
          this.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, this.GraphicsDevice.SamplerStates[0], (DepthStencilState) null, RasterizerState.CullCounterClockwise, (Effect) null);
          break;
        }
      default:
        flag1 = true;
        goto case Language.English;
    }
  }

  private bool HasStereo3D()
  {
    for (int index = 0; index < 3; ++index)
    {
      PCSaveDevice pcSaveDevice = new PCSaveDevice("FEZ");
      string str = "SaveSlot" + (object) index;
      SaveData saveData = (SaveData) null;
      string fileName = str;
      LoadAction loadAction = (LoadAction) (stream => saveData = SaveFileOperations.Read(new CrcReader(stream)));
      if (pcSaveDevice.Load(fileName, loadAction) && saveData != null && saveData.HasStereo3D)
        return true;
    }
    return false;
  }

  [ServiceDependency]
  public IMouseStateManager MouseState { protected get; set; }

  [ServiceDependency]
  public IKeyboardStateManager KeyboardState { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }

  [ServiceDependency]
  public IInputManager InputManager { protected get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { protected get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { protected get; set; }

  [ServiceDependency]
  public IFontManager Fonts { protected get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { protected get; set; }
}
