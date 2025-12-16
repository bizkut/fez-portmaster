// Decompiled with JetBrains decompiler
// Type: FezGame.Components.RainHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class RainHost : DrawableGameComponent
{
  private readonly Color RainColor = new Color(145, 182, (int) byte.MaxValue, 32 /*0x20*/);
  private PlaneParticleSystem RainPS;
  private RainHost.RainTransition Transition;
  private int flashes;
  private float flashSpeed;
  private float flashOpacity;
  private TimeSpan flashIn;
  private TimeSpan sinceFlash;
  private TimeSpan distantIn;
  private Mesh lightning;
  private float lightningSideOffset;
  private TrileInstance[] trileTops;
  private SoundEffect[] lightningSounds;
  private SoundEffect[] thunderDistant;
  private SoundEffect sPreThunder;
  private SoundEmitter ePreThunder;
  private bool doThunder;
  private Vector3 transitionCenter;
  private string LastLevelName;
  public static RainHost Instance;

  public RainHost(Game game)
    : base(game)
  {
    RainHost.Instance = this;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.DrawOrder = 500;
    this.Enabled = false;
    this.LevelManager.LevelChanging += new Action(this.TryKeepSounds);
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryKeepSounds()
  {
    if (this.Enabled && this.LevelManager.Name != this.LastLevelName)
    {
      ContentManager global = this.CMProvider.Global;
      global.Load<SoundEffect>("Sounds/Graveyard/Thunder1");
      global.Load<SoundEffect>("Sounds/Graveyard/Thunder2");
      global.Load<SoundEffect>("Sounds/Graveyard/Thunder3");
      global.Load<SoundEffect>("Sounds/Graveyard/Thunder4");
      global.Load<SoundEffect>("Sounds/Graveyard/ThunderDistant01");
      global.Load<SoundEffect>("Sounds/Graveyard/ThunderDistant02");
      global.Load<SoundEffect>("Sounds/Graveyard/ThunderDistant03");
      global.Load<SoundEffect>("Sounds/Graveyard/ThunderRumble");
      if (!this.LevelManager.Rainy)
      {
        this.lightningSounds = (SoundEffect[]) null;
        this.sPreThunder = (SoundEffect) null;
      }
    }
    this.LastLevelName = this.LevelManager.Name;
  }

  private void TryInitialize()
  {
    this.RainPS = (PlaneParticleSystem) null;
    this.ePreThunder = (SoundEmitter) null;
    if (this.lightning != null)
      this.lightning.Dispose();
    this.lightning = (Mesh) null;
    this.trileTops = (TrileInstance[]) null;
    this.Visible = this.Enabled = this.LevelManager.Rainy;
    if (!this.Enabled)
      return;
    this.doThunder = this.LevelManager.Name != "INDUSTRIAL_CITY";
    if (this.Transition == null)
    {
      Vector3 vector3 = this.RainColor.ToVector3();
      PlaneParticleSystemSettings settings = new PlaneParticleSystemSettings();
      settings.Velocity = (VaryingVector3) new Vector3(0.0f, -50f, 0.0f);
      settings.SpawningSpeed = 60f;
      settings.ParticleLifetime = 0.6f;
      settings.SpawnBatchSize = 10;
      VaryingVector3 varyingVector3 = new VaryingVector3();
      varyingVector3.Function = RainHost.RainScaling;
      settings.SizeBirth = varyingVector3;
      settings.FadeOutDuration = 0.1f;
      VaryingColor varyingColor = new VaryingColor();
      varyingColor.Base = this.RainColor;
      varyingColor.Variation = new Color(0, 0, 0, 24);
      settings.ColorLife = varyingColor;
      settings.ColorDeath = (VaryingColor) new Color(vector3.X, vector3.Y, vector3.Z, 0.0f);
      settings.ColorBirth = (VaryingColor) new Color(vector3.X, vector3.Y, vector3.Z, 0.0f);
      settings.BlendingMode = BlendingMode.Alphablending;
      settings.Billboarding = true;
      settings.NoLightDraw = true;
      this.PlaneSystems.Add(this.RainPS = new PlaneParticleSystem(this.Game, 500, settings));
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.RainPS.Settings.Texture = this.CMProvider.Global.Load<Texture2D>("Other Textures/rain/rain");
        this.RainPS.RefreshTexture();
      }));
      for (int index = 0; index < 75; ++index)
        this.PlaneSystems.RainSplash(Vector3.Zero).FadeOutAndDie(0.0f);
    }
    ContentManager contentManager = this.LevelManager.Name == null ? this.CMProvider.Global : this.CMProvider.CurrentLevel;
    this.lightning = new Mesh();
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.lightning.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/rain/lightning_a");
      this.lightning.Effect = (BaseEffect) new DefaultEffect.Textured();
    }));
    this.lightning.AddFace(Vector3.One * new Vector3(16f, 32f, 16f), Vector3.Zero, FaceOrientation.Front, true, true);
    if (this.lightningSounds == null)
      this.lightningSounds = new SoundEffect[4]
      {
        contentManager.Load<SoundEffect>("Sounds/Graveyard/Thunder1"),
        contentManager.Load<SoundEffect>("Sounds/Graveyard/Thunder2"),
        contentManager.Load<SoundEffect>("Sounds/Graveyard/Thunder3"),
        contentManager.Load<SoundEffect>("Sounds/Graveyard/Thunder4")
      };
    if (this.thunderDistant == null)
      this.thunderDistant = new SoundEffect[3]
      {
        contentManager.Load<SoundEffect>("Sounds/Graveyard/ThunderDistant01"),
        contentManager.Load<SoundEffect>("Sounds/Graveyard/ThunderDistant02"),
        contentManager.Load<SoundEffect>("Sounds/Graveyard/ThunderDistant03")
      };
    if (this.sPreThunder == null)
      this.sPreThunder = contentManager.Load<SoundEffect>("Sounds/Graveyard/ThunderRumble");
    this.trileTops = this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Enabled && !x.Trile.Immaterial && !x.Trile.ActorSettings.Type.IsClimbable() && (x.Trile.Geometry == null || !x.Trile.Geometry.Empty || x.Trile.Faces[FaceOrientation.Front] != CollisionType.None) && this.NoTop(x))).ToArray<TrileInstance>();
    this.flashIn = TimeSpan.FromSeconds((double) RandomHelper.Between(4.0, 6.0));
    this.distantIn = TimeSpan.FromSeconds((double) RandomHelper.Between(3.0, 6.0));
  }

  private static Func<Vector3, Vector3, Vector3> RainScaling
  {
    get
    {
      return (Func<Vector3, Vector3, Vector3>) ((b, v) => new Vector3(1f / 16f, RandomHelper.Between(48.0, 96.0) / 16f, 1f / 16f));
    }
  }

  private bool NoTop(TrileInstance instance)
  {
    FaceOrientation face = FaceOrientation.Top;
    TrileEmplacement traversal = instance.Emplacement.GetTraversal(ref face);
    TrileInstance trileInstance = this.LevelManager.TrileInstanceAt(ref traversal);
    return trileInstance == null || !trileInstance.Enabled || trileInstance.Trile.Immaterial;
  }

  public void ForceFlash() => this.flashIn = TimeSpan.Zero;

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    if (this.Transition != null)
    {
      if (!this.GameState.FarawaySettings.InTransition)
      {
        this.RainPS = this.Transition.RainPS;
        this.PlaneSystems.Add(this.RainPS);
        this.RainPS.MoveActiveParticles(this.transitionCenter - this.CameraManager.Center);
        this.RainPS.SetViewProjectionSticky(false);
        ServiceHelper.RemoveComponent<RainHost.RainTransition>(this.Transition);
        this.Transition = (RainHost.RainTransition) null;
      }
      else
      {
        if (this.GameState.Paused || (double) this.GameState.FarawaySettings.DestinationCrossfadeStep <= 0.0)
          return;
        this.SpawnSplashes();
        return;
      }
    }
    Vector3 center = this.CameraManager.Center;
    float num1 = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    int num2 = 60;
    Vector3 vector3 = new Vector3((float) num2 * num1, (float) num2 / this.CameraManager.AspectRatio, (float) num2 * num1) / 2f;
    this.RainPS.Settings.SpawnVolume = new BoundingBox()
    {
      Min = center - vector3 * FezMath.XZMask,
      Max = center + vector3 * new Vector3(1f, 2f, 1f)
    };
    if (this.GameState.Paused || !this.CameraManager.ActionRunning || this.GameState.InMenuCube || this.GameState.InMap || this.GameState.InFpsMode)
      return;
    this.SpawnSplashes();
  }

  private void SpawnSplashes()
  {
    int num1 = 0;
    int num2 = 0;
    while (num2 < 3 && num1 < 25)
    {
      TrileInstance trileInstance = RandomHelper.InList<TrileInstance>(this.trileTops);
      if (trileInstance.InstanceId >= 0 && this.CameraManager.Frustum.Contains(trileInstance.Center) != ContainmentType.Disjoint)
      {
        this.PlaneSystems.RainSplash(new Vector3(RandomHelper.Unit() / 2f, 0.5f, RandomHelper.Unit() / 2f) * trileInstance.TransformedSize + trileInstance.Center);
        ++num2;
      }
      else
        ++num1;
    }
    if (!RandomHelper.Probability(0.0099999997764825821))
      return;
    this.PlaneSystems.RainSplash(new Vector3(RandomHelper.Unit() / 2f, 0.625f, RandomHelper.Unit() / 2f) * this.PlayerManager.Size + this.PlayerManager.Center);
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
    if (!this.GameState.Paused && this.CameraManager.Viewpoint.IsOrthographic() && this.CameraManager.ActionRunning && !this.GameState.InMap)
    {
      this.flashIn -= elapsedGameTime;
      this.distantIn -= elapsedGameTime;
    }
    if (!this.doThunder)
      return;
    if (this.distantIn.Ticks <= 0L)
    {
      this.distantIn = TimeSpan.FromSeconds((double) RandomHelper.Between(3.0, 6.0));
      Vector3 position = this.CameraManager.Center + this.LevelManager.Size / 2f * this.CameraManager.Viewpoint.ForwardVector() + (float) ((double) RandomHelper.Centered(1.0) * (double) this.CameraManager.Radius * 0.5) * this.CameraManager.Viewpoint.RightVector() + this.CameraManager.Center * Vector3.UnitY;
      RandomHelper.InList<SoundEffect>(this.thunderDistant).EmitAt(position, RandomHelper.Centered(0.05000000074505806), RandomHelper.Between(0.75, 1.0)).NoAttenuation = true;
    }
    if (this.flashIn.Ticks <= 0L)
    {
      this.flashes = RandomHelper.Random.Next(1, 3);
      this.sinceFlash = TimeSpan.FromSeconds(-0.20000000298023224);
      this.flashSpeed = RandomHelper.Between(1.5, 3.5);
      this.flashOpacity = RandomHelper.Between(0.2, 0.4);
      this.flashIn = TimeSpan.FromSeconds((double) RandomHelper.Between(4.0, 6.0));
      this.lightning.Rotation = this.CameraManager.Rotation;
      this.lightningSideOffset = RandomHelper.Centered(1.0);
      this.lightning.Position = this.CameraManager.Center + this.LevelManager.Size / 2f * this.CameraManager.Viewpoint.ForwardVector() + (float) ((double) this.lightningSideOffset * (double) this.CameraManager.Radius * 0.5) * this.CameraManager.Viewpoint.RightVector() + RandomHelper.Centered(10.0) * Vector3.UnitY;
      if (this.ePreThunder != null)
      {
        this.ePreThunder.Cue.Stop();
        this.ePreThunder = (SoundEmitter) null;
      }
      (this.ePreThunder = this.sPreThunder.EmitAt(this.lightning.Position * FezMath.XZMask + this.CameraManager.Center * Vector3.UnitY, RandomHelper.Centered(0.02500000037252903))).NoAttenuation = true;
    }
    if (this.sinceFlash.TotalSeconds < 0.20000000298023224)
    {
      float alpha = Easing.EaseOut((double) FezMath.Saturate(1f - (float) (this.sinceFlash.TotalSeconds / 0.20000000298023224)), EasingType.Quadratic) * this.flashOpacity;
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.Sky);
      this.ImmediateRenderer.DrawFullscreen(new Color(1f, 1f, 1f, alpha));
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.Trails);
      this.ImmediateRenderer.DrawFullscreen(new Color(1f, 1f, 1f, alpha));
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.SkyLayer1);
      this.ImmediateRenderer.DrawFullscreen(new Color(0.9f, 0.9f, 0.9f, alpha));
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.SkyLayer2);
      this.ImmediateRenderer.DrawFullscreen(new Color(0.8f, 0.8f, 0.8f, alpha));
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.SkyLayer3);
      this.ImmediateRenderer.DrawFullscreen(new Color(0.7f, 0.7f, 0.7f, alpha));
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Less, FezEngine.Structure.StencilMask.SkyLayer3);
      this.ImmediateRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, alpha));
      this.sinceFlash += TimeSpan.FromTicks((long) ((double) elapsedGameTime.Ticks * (double) this.flashSpeed));
      if (this.flashes == 1 && (double) alpha / (double) this.flashOpacity < 0.5)
      {
        this.lightning.Material.Opacity = 1f;
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.SkyLayer3);
        this.lightning.Draw();
      }
      else if (this.flashes == 0 && this.sinceFlash.Ticks < 0L)
      {
        this.lightning.Material.Opacity = (float) (this.sinceFlash.TotalSeconds / -0.20000000298023224);
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.SkyLayer3);
        this.lightning.Draw();
      }
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    }
    if (this.sinceFlash.TotalSeconds <= 0.20000000298023224 || this.flashes == 0)
      return;
    --this.flashes;
    this.sinceFlash = TimeSpan.FromSeconds(-0.20000000298023224);
    this.flashSpeed = this.flashes == 0 ? 1f : RandomHelper.Between(2.0, 4.0);
    this.flashOpacity = this.flashes == 0 ? 1f : RandomHelper.Between(0.2, 0.4);
    if (this.flashes != 0)
      return;
    if (this.ePreThunder != null && !this.ePreThunder.Dead)
      this.ePreThunder.Cue.Stop();
    this.ePreThunder = (SoundEmitter) null;
    Vector3 position1 = this.lightning.Position * FezMath.XZMask + this.CameraManager.Center * Vector3.UnitY;
    SoundEmitter emitter = RandomHelper.InList<SoundEffect>(this.lightningSounds).EmitAt(position1, RandomHelper.Centered(0.02500000037252903));
    emitter.NoAttenuation = true;
    emitter.Persistent = true;
    Waiters.DoUntil((Func<bool>) (() => emitter.Dead), (Action<float>) (_ => emitter.Position = Vector3.Lerp(emitter.Position, this.CameraManager.Center, 0.025f))).AutoPause = true;
  }

  public void StartTransition()
  {
    ServiceHelper.AddComponent((IGameComponent) (this.Transition = new RainHost.RainTransition(this.Game, this.RainPS)));
    this.PlaneSystems.Remove(this.RainPS, false);
    this.RainPS.SetViewProjectionSticky(true);
    this.transitionCenter = this.CameraManager.Center;
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IPlaneParticleSystems PlaneSystems { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager ImmediateRenderer { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  public class RainTransition : DrawableGameComponent
  {
    public readonly PlaneParticleSystem RainPS;

    public RainTransition(Game game, PlaneParticleSystem rainPS)
      : base(game)
    {
      this.RainPS = rainPS;
    }

    public override void Update(GameTime gameTime)
    {
      this.RainPS.HalfUpdate = true;
      this.RainPS.Update(gameTime.ElapsedGameTime);
      this.RainPS.HalfUpdate = false;
    }

    public override void Draw(GameTime gameTime)
    {
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      this.RainPS.Draw();
    }

    [ServiceDependency]
    public IDefaultCameraManager CameraManager { private get; set; }
  }
}
