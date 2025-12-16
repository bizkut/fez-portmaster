// Decompiled with JetBrains decompiler
// Type: FezGame.Fez
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Components.Scripting;
using FezGame.Services;
using FezGame.Services.Scripting;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using System;

#nullable disable
namespace FezGame;

public class Fez : Game
{
  public static bool LevelChooser;
  public static bool SkipIntro;
  public static bool TrialMode;
  public static bool SkipLogos;
  public static int ForceGoldenCubes;
  public static int ForceAntiCubes;
  public static bool LongScreenshot;
  public static bool PublicDemo;
  public static bool DoubleRotations;
  public static bool NoSteamworks;
  public static bool NoMusic;
  public static bool DisableSteamworksInit;
  public static bool SpeedRunMode;
  public static string ForcedLevelName = "GOMEZ_HOUSE_2D";
  public const string ForcedTrialLevelName = "trial/BIG_TOWER";
  private readonly GraphicsDeviceManager deviceManager;
  private InputManager inputManager;
  public static string Version = "1.12";
  private float sinceLoading;

  public bool IsDisposed { get; private set; }

  public Fez()
  {
    if (SDL.SDL_GetPlatform().Equals("Windows"))
    {
      try
      {
        ThreadExecutionState.SetUp();
      }
      catch (Exception ex)
      {
        Logger.Log("ThreadExecutionState", ex.ToString());
      }
    }
    Logger.Log(nameof (Version), $"{Fez.Version}, Build Date : {LinkerTimestamp.BuildDateTime}");
    this.deviceManager = new GraphicsDeviceManager((Game) this);
    SettingsManager.DeviceManager = this.deviceManager;
    this.Content.RootDirectory = "Content";
    ServiceHelper.Game = (Game) this;
    ServiceHelper.IsFull = true;
  }

  protected override void Initialize()
  {
    SettingsManager.InitializeResolutions();
    SettingsManager.InitializeCapabilities();
    this.IsFixedTimeStep = false;
    this.deviceManager.SynchronizeWithVerticalRetrace = true;
    SettingsManager.Apply();
    ServiceHelper.AddService((object) new KeyboardStateManager());
    ServiceHelper.AddService((object) new MouseStateManager());
    ServiceHelper.AddService((object) new PlayerManager());
    ServiceHelper.AddService((object) new CollisionManager());
    ServiceHelper.AddService((object) new DebuggingBag());
    ServiceHelper.AddService((object) new PhysicsManager());
    ServiceHelper.AddService((object) new TimeManager());
    ServiceHelper.AddService((object) new GameStateManager());
    ServiceHelper.AddComponent((IGameComponent) new GamepadsManager((Game) this, !SettingsManager.Settings.DisableController), true);
    ServiceHelper.AddComponent((IGameComponent) (this.inputManager = new InputManager((Game) this, true, true, !SettingsManager.Settings.DisableController)), true);
    ServiceHelper.AddComponent((IGameComponent) new ContentManagerProvider((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new GameCameraManager((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new GameLevelMaterializer((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new GameLevelManager((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new FogManager((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new FezEngine.Services.TargetRenderingManager((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new SoundManager((Game) this, Fez.NoMusic), true);
    ServiceHelper.AddComponent((IGameComponent) new PersistentThreadPool((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new TrixelParticleSystems((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new TrialAndAwards((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new SpeechBubble((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new PlaneParticleSystems((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new FontManager((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new ScriptingHost((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new DotHost((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new RotatingGroupsHost((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new BigWaterfallHost((Game) this), true);
    ServiceHelper.AddComponent((IGameComponent) new BlackHolesHost((Game) this), true);
    ServiceHelper.AddService((object) new CameraService());
    ServiceHelper.AddService((object) new GomezService());
    ServiceHelper.AddService((object) new LevelService());
    ServiceHelper.AddService((object) new SoundService());
    ServiceHelper.AddService((object) new TimeService());
    ServiceHelper.AddService((object) new VolumeService());
    ServiceHelper.AddService((object) new ArtObjectService());
    ServiceHelper.AddService((object) new GroupService());
    ServiceHelper.AddService((object) new PlaneService());
    ServiceHelper.AddService((object) new NpcService());
    ServiceHelper.AddService((object) new ScriptService());
    ServiceHelper.AddService((object) new SwitchService());
    ServiceHelper.AddService((object) new BitDoorService());
    ServiceHelper.AddService((object) new SuckBlockService());
    ServiceHelper.AddService((object) new PathService());
    ServiceHelper.AddService((object) new SpinBlockService());
    ServiceHelper.AddService((object) new WarpGateService());
    ServiceHelper.AddService((object) new TombstoneService());
    ServiceHelper.AddService((object) new PivotService());
    ServiceHelper.AddService((object) new ValveService());
    ServiceHelper.AddService((object) new CodePatternService());
    ServiceHelper.AddService((object) new LaserEmitterService());
    ServiceHelper.AddService((object) new LaserReceiverService());
    ServiceHelper.AddService((object) new TimeswitchService());
    ServiceHelper.AddService((object) new DotService());
    ServiceHelper.AddService((object) new OwlService());
    ServiceHelper.InitializeServices();
    ServiceHelper.InjectServices((object) this);
    this.GameState.SaveData = new SaveData();
    this.Window.Title = "FEZ";
    if (Fez.SkipIntro)
    {
      Fez.LoadComponents(this);
      base.Initialize();
      this.GameState.SaveSlot = 0;
      this.GameState.SignInAndChooseStorage(new Action(Util.NullAction));
      this.GameState.LoadSaveFile((Action) (() => this.GameState.LoadLevelAsync(new Action(Util.NullAction))));
      this.GameState.SaveData.CanOpenMap = true;
      this.GameState.SaveData.IsNew = false;
    }
    else
    {
      ServiceHelper.AddComponent((IGameComponent) new Intro((Game) this));
      base.Initialize();
    }
  }

  internal static void LoadComponents(Fez game)
  {
    if (ServiceHelper.FirstLoadDone)
      return;
    ServiceHelper.AddComponent((IGameComponent) new StaticPreloader((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GammaCorrection((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new LoadingScreen((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new TimeHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GameLevelHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new PlayerCameraControl((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GameLightingPostProcess((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new PlayerActions((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new HeadsUpDisplay((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GomezHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new VolumesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GameStateControl((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SkyHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new PickupsHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new BombsHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GameSequencer((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new BurnInPostProcess((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new WatchersHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new MovingGroupsHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new AnimatedPlanesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GameNpcHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new PushSwitchesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new BitDoorsHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SuckBlocksHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new LevelLooper((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new CameraPathsHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new WarpGateHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new AttachedPlanesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SpinBlocksHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new PivotsHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SpinningTreasuresHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new LiquidHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new TombstonesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SplitUpCubeHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new ValvesBoltsTimeswitchesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new RumblerHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new RainHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new TempleOfLoveHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new WaterfallsHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GeysersHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SewerLightHacks((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new FarawayPlaceHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new CodeMachineHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new CrumblersHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new MailboxesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new PointsOfInterestHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new OrreryHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new OwlStatueHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new CryptHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new BellHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new TelescopeHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new TetrisPuzzleHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SaveIndicator((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new UnfoldPuzzleHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new QrCodesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GameWideCodes((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new FirstPersonView((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new Quantumizer((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new NameOfGodPuzzleHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new ClockTowerHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new PyramidHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new FinalRebuildHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new SecretPassagesHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new OwlHeadHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new StargateHost((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new FlickeringNeon((Game) game));
    ServiceHelper.AddComponent((IGameComponent) new GodRays((Game) game));
    if (Fez.PublicDemo)
      ServiceHelper.AddComponent((IGameComponent) new IdleRestarter((Game) game));
    ServiceHelper.FirstLoadDone = true;
  }

  protected override void Update(GameTime gameTime)
  {
    TimeInterpolation.NeedsInterpolation = true;
    TimeSpan totalGameTime = gameTime.TotalGameTime;
    TimeSpan timeSpan = totalGameTime - TimeInterpolation.LastUpdate;
    if (timeSpan < TimeInterpolation.UpdateTimestep)
      return;
    double totalMilliseconds;
    for (totalMilliseconds = timeSpan.TotalMilliseconds; totalMilliseconds > 17.0; totalMilliseconds -= 17.0)
      base.Update(new GameTime(totalGameTime, TimeInterpolation.UpdateTimestep));
    TimeInterpolation.LastUpdate = totalGameTime.Subtract(TimeSpan.FromTicks((long) (totalMilliseconds * 10000.0)));
  }

  protected override void Draw(GameTime gameTime)
  {
    TimeInterpolation.ProcessCallbacks(gameTime);
    DrawActionScheduler.Process();
    if (this.GameState.ScheduleLoadEnd && (!this.GameState.DotLoading || (double) this.sinceLoading > 5.0))
    {
      this.GameState.ScheduleLoadEnd = this.GameState.Loading = this.GameState.DotLoading = false;
      ServiceHelper.FirstLoadDone = true;
      this.sinceLoading = 0.0f;
    }
    if (this.GameState.DotLoading)
      this.sinceLoading += (float) gameTime.ElapsedGameTime.TotalSeconds;
    if (this.inputManager.StrictRotation && !this.GameState.InMap)
      this.inputManager.StrictRotation = false;
    if (!this.GameState.Loading && !this.GameState.SkipRendering)
      this.TargetRenderingManager.OnPreDraw(gameTime);
    this.TargetRenderingManager.OnRtPrepare();
    this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1f, 0);
    this.GraphicsDevice.SetupViewport();
    this.GraphicsDevice.PrepareDraw();
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    base.Draw(gameTime);
    SpeedRun.Draw((float) Math.Floor((double) this.GraphicsDevice.GetViewScale()));
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    OggStream.AbortPrecacher();
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }
}
