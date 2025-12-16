// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Intro
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Services.Scripting;
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
using System;
using System.Globalization;
using System.Threading;

#nullable disable
namespace FezGame.Components;

public class Intro : DrawableGameComponent
{
  private Texture2D TrixelEngineText;
  private Texture2D TrapdoorLogo;
  private GlyphTextRenderer tr;
  private readonly Mesh TrixelPlanes = new Mesh()
  {
    AlwaysOnTop = true,
    Blending = new BlendingMode?(BlendingMode.Multiply),
    DepthWrites = false
  };
  private Mesh SaveIndicatorMesh;
  private Mesh TrialMesh;
  private FezLogo FezLogo;
  private PolytronLogo PolytronLogo;
  private IntroZoomIn IntroZoomIn;
  private IntroPanDown IntroPanDown;
  private MainMenu MainMenu;
  private SoundEffect sTitleBassHit;
  private SoundEffect sTrixelIn;
  private SoundEffect sTrixelOut;
  private SoundEffect sExitGame;
  private SoundEffect sConfirm;
  private SoundEffect sDrone;
  private SoundEffect sStarZoom;
  private SoundEmitter eDrone;
  private Intro.Phase phase;
  private Intro.Screen screen;
  private SpriteBatch spriteBatch;
  private TimeSpan phaseTime;
  private bool scheduledBackToGame;
  private bool ZoomToHouse;
  private static bool PreloadStarted;
  private static bool PreloadComplete;
  private static bool FirstBootComplete;
  private float opacity;
  private float promptOpacity;
  private static StarField Starfield;
  private static bool HasShownSaveIndicator;
  private static bool firstDrawDone;
  private float dotdotdot;
  private bool didPanDown;

  public static Intro Instance { get; private set; }

  public bool Glitch { get; set; }

  public bool Fake { get; set; }

  public bool Sell { get; set; }

  public bool FadeBackToGame { get; set; }

  public bool Restarted { get; set; }

  public bool NewSaveSlot { get; set; }

  public string FakeLevel { get; set; }

  public bool FullLogos { get; set; }

  public Intro(Game game)
    : base(game)
  {
    this.DrawOrder = 2005;
    this.UpdateOrder = -1;
    Intro.Instance = this;
  }

  protected override void LoadContent()
  {
    ContentManager IntroCM = this.CMProvider.Get(CM.Intro);
    StaticText.GetString("Loading");
    bool is1440 = (double) this.GraphicsDevice.GetViewScale() >= 1.5;
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.TrixelEngineText = IntroCM.Load<Texture2D>("Other Textures/splash/trixels" + (is1440 ? "_1440" : ""));
      this.TrapdoorLogo = IntroCM.Load<Texture2D>("Other Textures/splash/trapdoor");
      this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
    }));
    this.tr = new GlyphTextRenderer(this.Game);
    this.tr.IgnoreKeyboardRemapping = true;
    this.KeyboardState.IgnoreMapping = true;
    this.TrixelPlanes.Position = (Vector3.Right + Vector3.Up) * -0.125f - Vector3.Up * 0.25f;
    this.TrixelPlanes.Scale = new Vector3(0.75f);
    this.TrixelPlanes.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Back, Color.Magenta, true, false);
    this.TrixelPlanes.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Top, Color.Yellow, true, false);
    this.TrixelPlanes.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Left, Color.Cyan, true, false);
    this.GraphicsDevice.SetupViewport();
    float aspectRatio = this.GraphicsDevice.Viewport.AspectRatio;
    this.TrialMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false
    };
    this.TrialMesh.AddColoredBox(Vector3.One, Vector3.Zero, new Color(209, 0, 55), true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh trixelPlanes = this.TrixelPlanes;
      trixelPlanes.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(2f * aspectRatio, 2f, 0.1f, 100f)),
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.UnitY - Vector3.UnitZ - Vector3.UnitX, Vector3.Zero, Vector3.Up))
      };
      Mesh trialMesh = this.TrialMesh;
      trialMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(5f * aspectRatio, 5f, 0.1f, 100f)),
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.UnitY - Vector3.UnitZ - Vector3.UnitX, Vector3.Zero, Vector3.Up))
      };
    }));
    ServiceHelper.AddComponent((IGameComponent) (this.PolytronLogo = new PolytronLogo(this.Game)));
    this.screen = Intro.Screen.WhiteScreen;
    Intro.FirstBootComplete = true;
    if (this.Restarted && !this.FullLogos)
      this.screen = Intro.Screen.Fez;
    if (this.GameState.ForcedSignOut)
    {
      this.InputManager.ClearActiveController();
      this.screen = Intro.Screen.SignInChooseDevice;
      ServiceHelper.AddComponent((IGameComponent) (Intro.Starfield = new StarField(this.Game)));
    }
    if (this.GameState.LoggedOutPlayerTag != null)
    {
      this.InputManager.ClearActiveController();
      this.screen = Intro.Screen.SignOutPrompt;
      ServiceHelper.AddComponent((IGameComponent) (Intro.Starfield = new StarField(this.Game)));
    }
    if (Fez.SkipLogos)
      this.screen = Intro.Screen.Fez;
    if (this.Fake)
    {
      this.screen = Intro.Screen.Polytron;
      this.PolytronLogo.Enabled = true;
    }
    if (this.Sell)
    {
      this.SoundManager.MuteAmbienceTracks();
      this.SoundManager.PlayNewSong("GOMEZ", 0.1f);
      this.screen = Intro.Screen.SellScreen;
    }
    this.GameState.ForceTimePaused = true;
    this.GameState.InCutscene = true;
    this.phaseTime = TimeSpan.FromSeconds(-0.60000002384185791);
    this.sTitleBassHit = IntroCM.Load<SoundEffect>("Sounds/Intro/LogoZoom");
    this.sTrixelIn = IntroCM.Load<SoundEffect>("Sounds/Intro/TrixelLogoIn");
    this.sTrixelOut = IntroCM.Load<SoundEffect>("Sounds/Intro/TrixelLogoOut");
    this.sExitGame = IntroCM.Load<SoundEffect>("Sounds/Ui/Menu/ExitGame");
    this.sConfirm = IntroCM.Load<SoundEffect>("Sounds/Ui/Menu/Confirm");
    this.sDrone = IntroCM.Load<SoundEffect>("Sounds/Intro/FezLogoDrone");
    this.sStarZoom = IntroCM.Load<SoundEffect>("Sounds/Intro/StarZoom");
    FezLogo fezLogo = new FezLogo(this.Game);
    fezLogo.Glitched = this.Glitch;
    FezLogo component = fezLogo;
    this.FezLogo = fezLogo;
    ServiceHelper.AddComponent((IGameComponent) component);
    if (this.Sell)
    {
      this.FezLogo.Inverted = true;
      this.FezLogo.TransitionStarted = true;
      this.FezLogo.LogoTextureXFade = 1f;
      this.FezLogo.Opacity = 1f;
      Intro.Starfield = this.FezLogo.Starfield;
      this.Enabled = this.Visible = false;
    }
    if (Intro.HasShownSaveIndicator)
      return;
    float viewAspect = this.GraphicsDevice.Viewport.AspectRatio;
    this.SaveIndicatorMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      AlwaysOnTop = true,
      DepthWrites = false
    };
    this.SaveIndicatorMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, Color.Red, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh saveIndicatorMesh = this.SaveIndicatorMesh;
      saveIndicatorMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(14f * viewAspect, 14f, 0.1f, 100f)),
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up))
      };
    }));
  }

  private void Kill() => ServiceHelper.RemoveComponent<Intro>(this);

  public void LoadVideo() => this.Enabled = this.Visible = true;

  protected override void Dispose(bool disposing)
  {
    this.KeyboardState.IgnoreMapping = false;
    if (this.CMProvider != null)
    {
      this.CMProvider.Dispose(CM.Intro);
      this.GameState.DynamicUpgrade -= new Action(this.Kill);
      Intro.Instance = (Intro) null;
      if (this.FezLogo != null)
        ServiceHelper.RemoveComponent<FezLogo>(this.FezLogo);
      if (this.PolytronLogo != null)
      {
        ServiceHelper.RemoveComponent<PolytronLogo>(this.PolytronLogo);
        this.PolytronLogo = (PolytronLogo) null;
      }
      if (this.IntroPanDown != null && !this.IntroPanDown.IsDisposed)
        ServiceHelper.RemoveComponent<IntroPanDown>(this.IntroPanDown);
      if (this.IntroZoomIn != null && !this.IntroZoomIn.IsDisposed)
        ServiceHelper.RemoveComponent<IntroZoomIn>(this.IntroZoomIn);
    }
    if (Intro.Starfield != null && Intro.Starfield.IsDisposed)
      Intro.Starfield = (StarField) null;
    this.TrixelPlanes.Dispose();
    this.TrialMesh.Dispose();
    if (this.SaveIndicatorMesh != null)
      this.SaveIndicatorMesh.Dispose();
    base.Dispose(disposing);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused)
      return;
    this.phaseTime += gameTime.ElapsedGameTime;
    if (this.screen < Intro.Screen.Fez && this.screen != Intro.Screen.ESRB_PEGI && (this.screen != Intro.Screen.SaveIndicator || !SettingsManager.FirstOpen) && this.InputManager.AnyButtonPressed() && ServiceHelper.FirstLoadDone)
    {
      if (this.phase == Intro.Phase.FadeIn)
        this.ChangePhase();
      this.ChangePhase();
    }
    this.UpdateLogo();
  }

  private void DoPreLoad(bool dummy)
  {
    Logger.Try<Fez>(new Action<Fez>(Fez.LoadComponents), this.Game as Fez);
    Intro.PreloadComplete = true;
    Logger.Log(nameof (Intro), "Preloading complete.");
  }

  private void UpdateLogo()
  {
    double totalSeconds = this.phaseTime.TotalSeconds;
    switch (this.screen)
    {
      case Intro.Screen.ESRB_PEGI:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            this.opacity = 1f;
            this.ChangePhase();
            return;
          case Intro.Phase.Wait:
            if (totalSeconds < 4.0)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 0.25)
              this.ChangePhase();
            this.opacity = 1f - Easing.EaseIn(FezMath.Saturate(totalSeconds * 4.0), EasingType.Sine);
            return;
          default:
            return;
        }
      case Intro.Screen.XBLA:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (totalSeconds >= 0.125)
              this.ChangePhase();
            this.opacity = Easing.EaseIn(FezMath.Saturate(totalSeconds * 8.0), EasingType.Sine);
            return;
          case Intro.Phase.Wait:
            if (totalSeconds < 2.0)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 0.25)
              this.ChangePhase();
            this.opacity = 1f - Easing.EaseIn(FezMath.Saturate(totalSeconds * 4.0), EasingType.Sine);
            return;
          default:
            return;
        }
      case Intro.Screen.MGS:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (totalSeconds >= 0.125)
              this.ChangePhase();
            this.opacity = Easing.EaseIn(FezMath.Saturate(totalSeconds * 8.0), EasingType.Sine);
            return;
          case Intro.Phase.Wait:
            if (totalSeconds < 2.0)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 0.25)
              this.ChangePhase();
            this.opacity = Math.Min(this.opacity, 1f - Easing.EaseIn(FezMath.Saturate(totalSeconds * 4.0), EasingType.Sine));
            return;
          default:
            return;
        }
      case Intro.Screen.WhiteScreen:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (totalSeconds >= 0.125)
              this.ChangePhase();
            this.opacity = Easing.EaseIn(FezMath.Saturate(totalSeconds * 8.0), EasingType.Sine);
            return;
          case Intro.Phase.Wait:
            if (!Intro.PreloadStarted)
            {
              this.GameState.SkipLoadScreen = true;
              this.GameState.Loading = true;
              Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoPreLoad));
              worker.Priority = ThreadPriority.AboveNormal;
              worker.Finished += (Action) (() =>
              {
                this.ThreadPool.Return<bool>(worker);
                this.GameState.ScheduleLoadEnd = true;
                this.GameState.SkipLoadScreen = false;
              });
              worker.Start(false);
              Intro.PreloadStarted = true;
            }
            if (!Intro.PreloadComplete)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 0.25)
              this.ChangePhase();
            this.opacity = Math.Min(this.opacity, 1f - Easing.EaseIn(FezMath.Saturate(totalSeconds * 4.0), EasingType.Sine));
            return;
          default:
            return;
        }
      case Intro.Screen.Polytron:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (totalSeconds >= 1.5)
              this.ChangePhase();
            this.opacity = 0.0f;
            this.PolytronLogo.Opacity = 1f;
            return;
          case Intro.Phase.Wait:
            if (totalSeconds < 1.5)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 0.25)
              this.ChangePhase();
            this.PolytronLogo.Opacity = 1f - Easing.EaseIn(FezMath.Saturate(totalSeconds * 4.0), EasingType.Sine);
            return;
          default:
            return;
        }
      case Intro.Screen.Trapdoor:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (totalSeconds >= 0.125)
              this.ChangePhase();
            this.opacity = Easing.EaseIn(FezMath.Saturate(totalSeconds * 8.0), EasingType.Sine);
            return;
          case Intro.Phase.Wait:
            if (totalSeconds < 1.5)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 0.25)
              this.ChangePhase();
            this.opacity = Math.Min(this.opacity, 1f - Easing.EaseIn(FezMath.Saturate(totalSeconds * 4.0), EasingType.Sine));
            return;
          default:
            return;
        }
      case Intro.Screen.TrixelEngine:
        float num = 0.0f;
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (this.sTrixelIn != null)
            {
              this.sTrixelIn.Emit();
              this.sTrixelIn = (SoundEffect) null;
            }
            if (totalSeconds >= 1.0)
              this.ChangePhase();
            num = (float) ((1.0 - (double) Easing.EaseOut(FezMath.Saturate(totalSeconds), EasingType.Quadratic)) * 6.0);
            this.opacity = Easing.EaseIn(FezMath.Saturate(totalSeconds * 2.0 - 0.5), EasingType.Quadratic);
            break;
          case Intro.Phase.Wait:
            if (totalSeconds >= 1.0)
            {
              this.ChangePhase();
              break;
            }
            break;
          case Intro.Phase.FadeOut:
            if (this.sTrixelOut != null)
            {
              this.sTrixelOut.Emit();
              this.sTrixelOut = (SoundEffect) null;
            }
            if (totalSeconds >= 0.5)
              this.ChangePhase();
            this.opacity = Math.Min(this.opacity, 1f - Easing.EaseIn(FezMath.Saturate(totalSeconds * 2.0), EasingType.Quadratic));
            num = Easing.EaseIn(FezMath.Saturate(totalSeconds * 2.0), EasingType.Quadratic) * -4f;
            break;
        }
        this.TrixelPlanes.Groups[0].Position = Vector3.Right * (0.5f + num);
        this.TrixelPlanes.Groups[1].Position = Vector3.Up * (0.5f + num);
        this.TrixelPlanes.Groups[2].Position = Vector3.Backward * num;
        break;
      case Intro.Screen.SaveIndicator:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            this.eDrone.VolumeFactor = FezMath.Saturate((float) totalSeconds / 0.125f);
            this.opacity = Easing.EaseIn(FezMath.Saturate(totalSeconds / 0.125), EasingType.Sine);
            if (totalSeconds < 0.125)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.Wait:
            if (totalSeconds < 2.625)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            this.opacity = Math.Min(this.opacity, 1f - Easing.EaseOut(FezMath.Saturate(totalSeconds / 0.25), EasingType.Sine));
            this.eDrone.VolumeFactor = (float) (1.0 - (double) FezMath.Saturate((float) totalSeconds / 0.25f) * 0.5);
            if (totalSeconds < 0.25)
              return;
            this.ChangePhase();
            return;
          default:
            return;
        }
      case Intro.Screen.Fez:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (totalSeconds >= 0.75)
              this.ChangePhase();
            this.opacity = Easing.EaseInOut(FezMath.Saturate(totalSeconds / 0.75), EasingType.Sine);
            this.promptOpacity = Easing.EaseIn(FezMath.Saturate((totalSeconds - 0.25) / 0.5), EasingType.Sine);
            return;
          case Intro.Phase.Wait:
            this.opacity = this.promptOpacity = 1f;
            if (this.InputManager.Jump != FezButtonState.Pressed && this.InputManager.Start != FezButtonState.Pressed)
              return;
            if (!this.Fake)
              this.InputManager.DetermineActiveController();
            this.SoundManager.PlayNewSong((string) null, 0.0f);
            this.FezLogo.TransitionStarted = true;
            this.sTitleBassHit.Emit().Persistent = true;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            this.promptOpacity = this.opacity = 1f - Easing.EaseOut(FezMath.Saturate(totalSeconds / 1.0), EasingType.Sine);
            if (totalSeconds < 1.0)
              return;
            this.ChangePhase();
            return;
          default:
            return;
        }
      case Intro.Screen.SellScreen:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            this.GameState.SkipRendering = true;
            this.opacity = Easing.EaseInOut(FezMath.Saturate(totalSeconds - 14.0), EasingType.Sine);
            if (totalSeconds < 15.0)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.Wait:
            this.FezLogo.LogoTextureXFade = 1f - Easing.EaseIn((double) FezMath.Saturate((float) ((totalSeconds - 76.0) / 4.0)), EasingType.Sine);
            if (this.InputManager.Jump == FezButtonState.Pressed || this.InputManager.Start == FezButtonState.Pressed)
            {
              if (Fez.PublicDemo)
              {
                this.GameState.Restart();
                return;
              }
              this.sExitGame.Emit();
              this.GameState.ReturnToArcade();
              this.Enabled = false;
              return;
            }
            if (!Fez.PublicDemo && (this.InputManager.CancelTalk == FezButtonState.Pressed || this.InputManager.Back == FezButtonState.Pressed))
            {
              this.GameState.SkipRendering = false;
              this.FezLogo.Inverted = false;
              if (!this.FadeBackToGame)
              {
                this.sTitleBassHit.Emit();
              }
              else
              {
                this.SoundManager.PlayNewSong((string) null, 0.5f);
                this.SoundManager.UnshelfSong();
                this.SoundManager.Resume();
                this.SoundManager.UnmuteAmbienceTracks(true);
                this.scheduledBackToGame = true;
                this.FezLogo.DoubleTime = true;
                this.PlayerManager.Hidden = true;
                this.GameState.InCutscene = false;
                this.GameState.ForceTimePaused = false;
              }
              this.ChangePhase();
              return;
            }
            if (Fez.PublicDemo || this.InputManager.GrabThrow != FezButtonState.Pressed)
              return;
            this.sConfirm.Emit();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 2.0)
            {
              if (this.scheduledBackToGame)
              {
                this.PlayerManager.Hidden = false;
                this.PlayerManager.Ground = new MultipleHits<TrileInstance>();
                this.PlayerManager.Velocity = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448 * 0.01666666753590107) * -Vector3.UnitY;
                this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
                this.PlayerManager.Velocity = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448 * 0.01666666753590107) * -Vector3.UnitY;
                this.PlayerManager.Action = ActionType.ExitDoor;
                ServiceHelper.RemoveComponent<Intro>(this);
              }
              else
                this.ChangePhase();
            }
            this.opacity = 1f - Easing.EaseInOut(FezMath.Saturate(totalSeconds / 2.0), EasingType.Sine);
            return;
          default:
            return;
        }
      case Intro.Screen.Zoom:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            this.ChangePhase();
            return;
          case Intro.Phase.Wait:
            if (!this.FezLogo.IsFullscreen)
              return;
            Intro.Starfield = this.FezLogo.Starfield;
            ServiceHelper.RemoveComponent<FezLogo>(this.FezLogo);
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            this.ChangePhase();
            return;
          default:
            return;
        }
      case Intro.Screen.SignOutPrompt:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (totalSeconds >= 0.5)
              this.ChangePhase();
            this.opacity = FezMath.Saturate((float) totalSeconds * 2f);
            return;
          case Intro.Phase.Wait:
            if (this.InputManager.Jump != FezButtonState.Pressed && this.InputManager.Start != FezButtonState.Pressed)
              return;
            this.InputManager.DetermineActiveController();
            this.FezLogo.TransitionStarted = true;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (totalSeconds >= 0.5)
              this.ChangePhase();
            this.opacity = 1f - FezMath.Saturate((float) totalSeconds * 2f);
            return;
          default:
            return;
        }
      case Intro.Screen.SignInChooseDevice:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (this.Fake)
              this.ChangePhase();
            else
              this.GameState.SignInAndChooseStorage(new Action(this.ChangePhase));
            this.ChangePhase();
            return;
          case Intro.Phase.Wait:
            return;
          case Intro.Phase.FadeOut:
            this.ChangePhase();
            return;
          default:
            return;
        }
      case Intro.Screen.MainMenu:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            this.GameState.ForcedSignOut = false;
            if (!this.Fake)
            {
              if (!this.GameState.SaveData.HasNewGamePlus || this.GameState.SaveData.Level != null)
              {
                this.GameState.LoadSaveFile((Action) (() => Waiters.Wait(0.0, (Action) (() =>
                {
                  this.Enabled = true;
                  ServiceHelper.AddComponent((IGameComponent) (this.MainMenu = new MainMenu(this.Game)));
                  this.ChangePhase();
                }))));
                this.Enabled = false;
                return;
              }
              ServiceHelper.AddComponent((IGameComponent) (this.MainMenu = new MainMenu(this.Game)));
              this.ChangePhase();
              return;
            }
            this.ChangePhase();
            return;
          case Intro.Phase.Wait:
            if (this.Fake || this.MainMenu.StartedGame)
            {
              this.ZoomToHouse = true;
              Intro.Starfield.Enabled = true;
              this.sStarZoom.Emit().Persistent = true;
              this.StartLoading();
              this.ChangePhase();
              return;
            }
            if (this.MainMenu.HasBought)
            {
              this.GameState.ClearSaveFile();
              this.screen = Intro.Screen.SignInChooseDevice;
              this.phase = Intro.Phase.FadeIn;
              return;
            }
            if (this.MainMenu.SellingTime)
            {
              this.GameService.EndTrial(true);
              ServiceHelper.RemoveComponent<MainMenu>(this.MainMenu);
              this.MainMenu = (MainMenu) null;
              this.Enabled = false;
              return;
            }
            if (!this.MainMenu.ContinuedGame)
              return;
            this.StartLoading();
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            this.ChangePhase();
            return;
          default:
            return;
        }
      case Intro.Screen.Warp:
        switch (this.phase)
        {
          case Intro.Phase.FadeIn:
            if (this.GameState.Loading)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.Wait:
            if (this.GameState.IsTrialMode)
            {
              ServiceHelper.AddComponent((IGameComponent) new TileTransition(ServiceHelper.Game)
              {
                ScreenCaptured = (Action) (() =>
                {
                  ServiceHelper.RemoveComponent<Intro>(this);
                  this.GameState.InCutscene = false;
                  this.GameState.ForceTimePaused = false;
                })
              });
              this.ChangePhase();
            }
            if (this.ZoomToHouse && Intro.Starfield != null && Intro.Starfield.IsDisposed && this.IntroZoomIn == null)
              ServiceHelper.AddComponent((IGameComponent) (this.IntroZoomIn = new IntroZoomIn(this.Game)));
            if (this.ZoomToHouse && this.IntroZoomIn != null && this.IntroZoomIn.IsDisposed)
            {
              this.IntroZoomIn = (IntroZoomIn) null;
              this.GameState.SaveData.CanNewGamePlus = false;
              Logger.Log(nameof (Intro), "Intro is done and game is go!");
              ServiceHelper.RemoveComponent<Intro>(this);
            }
            if (this.GameState.IsTrialMode || this.ZoomToHouse || this.IntroPanDown == null || this.IntroPanDown.DoPanDown)
              return;
            this.ChangePhase();
            return;
          case Intro.Phase.FadeOut:
            if (this.GameState.IsTrialMode || this.IntroPanDown != null && !this.IntroPanDown.IsDisposed)
              return;
            this.IntroPanDown = (IntroPanDown) null;
            Logger.Log(nameof (Intro), "Intro is done and game is go!");
            ServiceHelper.RemoveComponent<Intro>(this);
            return;
          default:
            return;
        }
    }
  }

  private void ChangePhase()
  {
    switch (this.phase)
    {
      case Intro.Phase.FadeIn:
        this.phase = Intro.Phase.Wait;
        break;
      case Intro.Phase.Wait:
        this.phase = Intro.Phase.FadeOut;
        break;
      case Intro.Phase.FadeOut:
        this.phase = Intro.Phase.FadeIn;
        this.ChangeScreen();
        break;
    }
    this.phaseTime = TimeSpan.Zero;
  }

  private void ChangeScreen()
  {
    switch (this.screen)
    {
      case Intro.Screen.ESRB_PEGI:
        this.screen = Intro.Screen.XBLA;
        break;
      case Intro.Screen.XBLA:
        this.screen = Intro.Screen.MGS;
        break;
      case Intro.Screen.MGS:
        this.screen = Intro.Screen.WhiteScreen;
        break;
      case Intro.Screen.WhiteScreen:
        this.screen = Intro.Screen.Polytron;
        this.PolytronLogo.Enabled = true;
        break;
      case Intro.Screen.Polytron:
        this.PolytronLogo.Enabled = false;
        this.PolytronLogo.End();
        this.screen = Intro.Screen.Trapdoor;
        break;
      case Intro.Screen.Trapdoor:
        this.screen = Intro.Screen.TrixelEngine;
        break;
      case Intro.Screen.TrixelEngine:
        if (Intro.HasShownSaveIndicator || this.Fake)
        {
          this.screen = Intro.Screen.Fez;
          this.eDrone = this.sDrone.Emit(true, 0.0f, 0.5f);
          break;
        }
        this.screen = Intro.Screen.SaveIndicator;
        this.eDrone = this.sDrone.Emit(true, 0.0f, 0.0f);
        Intro.HasShownSaveIndicator = true;
        break;
      case Intro.Screen.SaveIndicator:
        this.screen = Intro.Screen.Fez;
        this.SoundManager.PlayNewSong("FEZ", 1f);
        if (this.eDrone == null)
          break;
        this.eDrone.FadeOutAndDie(3f, false);
        break;
      case Intro.Screen.Fez:
        this.screen = Intro.Screen.Zoom;
        if (!this.Glitch)
          break;
        this.eDrone.FadeOutAndDie(3f, false);
        break;
      case Intro.Screen.SellScreen:
        this.screen = Intro.Screen.Zoom;
        break;
      case Intro.Screen.Zoom:
        this.screen = Intro.Screen.SignInChooseDevice;
        break;
      case Intro.Screen.SignOutPrompt:
        this.GameState.LoggedOutPlayerTag = (string) null;
        this.screen = Intro.Screen.SignInChooseDevice;
        break;
      case Intro.Screen.SignInChooseDevice:
        this.screen = Intro.Screen.MainMenu;
        break;
      case Intro.Screen.MainMenu:
        this.screen = Intro.Screen.Warp;
        break;
    }
  }

  private void StartLoading()
  {
    this.GameState.SkipLoadBackground = true;
    this.GameState.Loading = true;
    Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoLoad));
    worker.Priority = ThreadPriority.Normal;
    worker.Finished += (Action) (() => this.ThreadPool.Return<bool>(worker));
    worker.Start(false);
  }

  private void DoLoad(bool dummy) => Logger.Try(new Action(this.DoLoad));

  private void DoLoad()
  {
    if (!this.Fake)
      this.GameState.LoadLevel();
    else
      this.LevelManager.ChangeLevel(this.FakeLevel);
    Logger.Log(nameof (Intro), "Level load complete.");
    if (!this.ZoomToHouse)
      ServiceHelper.AddComponent((IGameComponent) (this.IntroPanDown = new IntroPanDown(this.Game)));
    this.GameState.ScheduleLoadEnd = true;
    this.GameState.SkipLoadBackground = false;
  }

  public override void Draw(GameTime gameTime)
  {
    if (Fez.SkipLogos)
      return;
    float viewScale = this.GraphicsDevice.GetViewScale();
    if (!Intro.firstDrawDone)
    {
      Logger.Log(nameof (Intro), "First draw done!");
      Intro.firstDrawDone = true;
    }
    Vector2 vector2_1 = new Vector2((float) this.GraphicsDevice.Viewport.Width, (float) this.GraphicsDevice.Viewport.Height);
    Vector2 vector2_2 = (vector2_1 / 2f).Round();
    if (this.screen < Intro.Screen.SignOutPrompt)
    {
      if (this.screen == Intro.Screen.SellScreen && this.phase == Intro.Phase.FadeOut && this.scheduledBackToGame)
        this.TargetRenderer.DrawFullscreen(new Color(1f, 1f, 1f, this.opacity));
      else if (this.screen == Intro.Screen.WhiteScreen)
        this.GraphicsDevice.Clear(Color.White);
      else
        this.TargetRenderer.DrawFullscreen(Color.White);
    }
    switch (this.screen)
    {
      case Intro.Screen.WhiteScreen:
        this.spriteBatch.BeginPoint();
        this.dotdotdot += (float) (gameTime.ElapsedGameTime.TotalSeconds * 2.0);
        string str1 = StaticText.GetString("Loading");
        string str2 = str1.Substring(0, str1.Length - 3);
        for (int index = 0; index < (int) this.dotdotdot % 4; ++index)
          str2 += ".";
        this.tr.DrawString(this.spriteBatch, this.Fonts.Small, str2.ToUpper(CultureInfo.InvariantCulture), new Vector2(50f, (float) this.GraphicsDevice.Viewport.Height - (float) (65.0 * ((1.0 + (double) viewScale) / 2.0))), new Color(0.0f, 0.0f, 0.0f, this.opacity), this.Fonts.SmallFactor * viewScale);
        string text1 = "v" + Fez.Version;
        Vector2 vector2_3 = this.Fonts.Small.MeasureString(text1) * this.Fonts.SmallFactor * viewScale;
        GlyphTextRenderer tr = this.tr;
        SpriteBatch spriteBatch = this.spriteBatch;
        SpriteFont small = this.Fonts.Small;
        string text2 = text1;
        Viewport viewport = this.GraphicsDevice.Viewport;
        double x = (double) (viewport.Width - 50) - (double) vector2_3.X;
        viewport = this.GraphicsDevice.Viewport;
        double y = (double) viewport.Height - 65.0 * ((1.0 + (double) viewScale) / 2.0);
        Vector2 position = new Vector2((float) x, (float) y);
        Color color = new Color(0.0f, 0.0f, 0.0f, this.opacity);
        double scale1 = (double) this.Fonts.SmallFactor * (double) viewScale;
        tr.DrawString(spriteBatch, small, text2, position, color, (float) scale1);
        this.spriteBatch.End();
        break;
      case Intro.Screen.Polytron:
        this.PolytronLogo.Draw(gameTime);
        break;
      case Intro.Screen.Trapdoor:
        this.spriteBatch.BeginPoint();
        this.spriteBatch.Draw(this.TrapdoorLogo, vector2_2 - (new Vector2((float) this.TrapdoorLogo.Width, (float) this.TrapdoorLogo.Height) / 2f).Round(), new Color(1f, 1f, 1f, this.opacity));
        this.spriteBatch.End();
        break;
      case Intro.Screen.TrixelEngine:
        if ((double) this.opacity != 0.0)
        {
          this.TrixelPlanes.Position = this.TrixelPlanes.Position * new Vector3(1f, 0.0f, 1f) + -0.375f * Vector3.UnitY;
          this.TrixelPlanes.Scale = new Vector3(0.75f);
          this.TrixelPlanes.Draw();
        }
        this.spriteBatch.BeginPoint();
        this.spriteBatch.Draw(this.TrixelEngineText, (new Vector2(vector2_2.X, vector2_2.Y / 1.8f) - new Vector2((float) this.TrixelEngineText.Width, (float) this.TrixelEngineText.Height) / 2f).Round(), new Color(1f, 1f, 1f, this.opacity));
        this.spriteBatch.End();
        break;
      case Intro.Screen.SaveIndicator:
        this.SaveIndicatorMesh.Material.Opacity = this.opacity;
        this.SaveIndicatorMesh.FirstGroup.Position = new Vector3(0.0f, 2.5f, 0.0f);
        this.SaveIndicatorMesh.FirstGroup.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float) (-gameTime.ElapsedGameTime.TotalSeconds * 3.0)) * this.SaveIndicatorMesh.FirstGroup.Rotation;
        this.SaveIndicatorMesh.Draw();
        this.spriteBatch.BeginPoint();
        this.tr.DrawCenteredString(this.spriteBatch, this.Fonts.Big, StaticText.GetString("SaveIndicator"), new Color(0.0f, 0.0f, 0.0f, this.opacity), new Vector2(0.0f, vector2_1.Y * 0.425f), this.Fonts.BigFactor * viewScale);
        this.spriteBatch.End();
        break;
      case Intro.Screen.Fez:
      case Intro.Screen.Zoom:
        if (!this.FezLogo.IsDisposed)
        {
          if (!this.FezLogo.TransitionStarted)
            this.FezLogo.Opacity = this.opacity;
          this.FezLogo.Draw(gameTime);
        }
        else
        {
          this.TargetRenderer.DrawFullscreen(Color.Black);
          Intro.Starfield.Draw();
        }
        if (Culture.IsCJK)
          this.spriteBatch.BeginLinear();
        else
          this.spriteBatch.BeginPoint();
        string text3 = StaticText.GetString("SplashStart");
        if (!GamepadState.AnyConnected)
          text3 = text3.Replace("{A}", "{START}");
        this.tr.DrawCenteredString(this.spriteBatch, this.Fonts.Big, text3, new Color(0.0f, 0.0f, 0.0f, this.promptOpacity), new Vector2(150f * viewScale, (float) ((double) vector2_1.Y * 4.0 / 5.0)), this.Fonts.BigFactor * viewScale);
        this.spriteBatch.End();
        break;
      case Intro.Screen.SellScreen:
        if (this.phase == Intro.Phase.FadeOut)
        {
          this.FezLogo.LogoTextureXFade = Math.Min(this.opacity, this.FezLogo.LogoTextureXFade);
          if (this.scheduledBackToGame)
            this.FezLogo.Opacity = this.opacity;
        }
        this.FezLogo.Draw(gameTime);
        if (Culture.IsCJK)
          this.spriteBatch.BeginLinear();
        else
          this.spriteBatch.BeginPoint();
        float scale2 = this.Fonts.BigFactor + 0.25f;
        if (!Culture.IsCJK)
          scale2 += 0.75f;
        this.tr.DrawString(this.spriteBatch, this.Fonts.Big, StaticText.GetString("SellTitle"), new Vector2(334f, 50f), new Color(0.0f, 0.0f, 0.0f, this.opacity), scale2);
        if (Fez.PublicDemo)
          this.tr.DrawStringLFLeftAlign(this.spriteBatch, this.Fonts.Small, StaticText.GetString("SellButtonsPublicDemo"), new Color(0.0f, 0.0f, 0.0f, this.opacity), new Vector2(953f, 683f), this.Fonts.SmallFactor);
        else
          this.tr.DrawStringLFLeftAlign(this.spriteBatch, this.Fonts.Small, StaticText.GetString(this.FadeBackToGame ? "SellButtonsEndTrial" : "SellButtons"), new Color(0.0f, 0.0f, 0.0f, this.opacity), new Vector2(953f, 683f), this.Fonts.SmallFactor);
        this.spriteBatch.End();
        break;
      case Intro.Screen.SignOutPrompt:
        this.TargetRenderer.DrawFullscreen(Color.Black);
        Intro.Starfield.Draw();
        this.spriteBatch.BeginPoint();
        this.tr.DrawCenteredString(this.spriteBatch, this.Fonts.Big, StaticText.GetString("SignOutNotice"), new Color(1f, 1f, 1f, this.opacity), new Vector2(0.0f, vector2_1.Y / 3f), this.Fonts.BigFactor);
        this.spriteBatch.End();
        break;
      case Intro.Screen.SignInChooseDevice:
      case Intro.Screen.MainMenu:
      case Intro.Screen.Warp:
        if (this.IntroZoomIn == null && (this.IntroPanDown == null || !this.IntroPanDown.DoPanDown))
        {
          this.TargetRenderer.DrawFullscreen(Color.Black);
          if (Intro.Starfield != null && !Intro.Starfield.IsDisposed)
            Intro.Starfield.Draw();
        }
        if (this.IntroPanDown == null || this.IntroPanDown.DoPanDown || this.didPanDown)
          break;
        this.DoPanDown();
        break;
    }
  }

  private void DoPanDown()
  {
    ServiceHelper.AddComponent((IGameComponent) new TileTransition(ServiceHelper.Game)
    {
      ScreenCaptured = (Action) (() => this.IntroPanDown.DoPanDown = true),
      WaitFor = (Func<bool>) (() => (double) this.IntroPanDown.SinceStarted > 0.0)
    });
    this.didPanDown = true;
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IPhysicsManager PhysicsManager { private get; set; }

  [ServiceDependency]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { private get; set; }

  [ServiceDependency]
  public IFontManager Fonts { private get; set; }

  [ServiceDependency]
  public IGameService GameService { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private enum Screen
  {
    ESRB_PEGI,
    XBLA,
    MGS,
    WhiteScreen,
    Polytron,
    Trapdoor,
    TrixelEngine,
    SaveIndicator,
    Fez,
    SellScreen,
    Zoom,
    SignOutPrompt,
    SignInChooseDevice,
    MainMenu,
    Warp,
  }

  private enum Phase
  {
    FadeIn,
    Wait,
    FadeOut,
  }
}
