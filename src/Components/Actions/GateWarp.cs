// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.GateWarp
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#nullable disable
namespace FezGame.Components.Actions;

internal class GateWarp : PlayerAction
{
  private const float FadeSeconds = 2.25f;
  private ArtObjectInstance GateAo;
  private GateWarp.Phases Phase;
  private TimeSpan SinceStarted;
  private TimeSpan SinceRisen;
  private SoundEffect WarpSound;
  private PlaneParticleSystem particles;
  private Mesh rgbPlanes;
  private float sinceInitialized;
  private Vector3 originalCenter;

  public GateWarp(Game game)
    : base(game)
  {
    this.DrawOrder = 901;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LightingPostProcess.DrawOnTopLights += new Action(this.DrawLights);
    this.WarpSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Zu/WarpGateActivate");
  }

  private void InitializeRgbGate()
  {
    IPlaneParticleSystems planeParticleSystems = this.PlaneParticleSystems;
    Game game = this.Game;
    PlaneParticleSystemSettings settings = new PlaneParticleSystemSettings();
    settings.SpawnVolume = new BoundingBox()
    {
      Min = this.GateAo.Position + new Vector3(-3.25f, -6f, -3.25f),
      Max = this.GateAo.Position + new Vector3(3.25f, -2f, 3.25f)
    };
    settings.Velocity.Base = new Vector3(0.0f, 0.6f, 0.0f);
    settings.Velocity.Variation = new Vector3(0.0f, 0.1f, 0.1f);
    settings.SpawningSpeed = 7f;
    settings.ParticleLifetime = 6f;
    settings.Acceleration = 0.375f;
    settings.SizeBirth = (VaryingVector3) new Vector3(0.25f, 0.25f, 0.25f);
    settings.ColorBirth = (VaryingColor) Color.Black;
    settings.ColorLife.Base = new Color(0.5f, 0.5f, 0.5f, 1f);
    settings.ColorLife.Variation = new Color(0.5f, 0.5f, 0.5f, 1f);
    settings.ColorDeath = (VaryingColor) Color.Black;
    settings.FullBright = true;
    settings.RandomizeSpawnTime = true;
    settings.Billboarding = true;
    settings.Texture = this.CMProvider.Global.Load<Texture2D>("Background Planes/dust_particle");
    settings.BlendingMode = BlendingMode.Additive;
    PlaneParticleSystem system = this.particles = new PlaneParticleSystem(game, 400, settings);
    planeParticleSystems.Add(system);
    this.rgbPlanes = new Mesh()
    {
      Effect = (BaseEffect) new DefaultEffect.Textured(),
      DepthWrites = false,
      AlwaysOnTop = true,
      SamplerState = SamplerState.LinearClamp,
      Blending = new BlendingMode?(BlendingMode.Additive),
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/rgb_gradient")
    };
    this.rgbPlanes.AddFace(new Vector3(1f, 4.5f, 1f), new Vector3(0.0f, 3f, 0.0f), FaceOrientation.Front, true).Material = new Material()
    {
      Diffuse = Vector3.Zero,
      Opacity = 0.0f
    };
    this.rgbPlanes.AddFace(new Vector3(1f, 4.5f, 1f), new Vector3(0.0f, 3f, 0.0f), FaceOrientation.Front, true).Material = new Material()
    {
      Diffuse = Vector3.Zero,
      Opacity = 0.0f
    };
    this.rgbPlanes.AddFace(new Vector3(1f, 4.5f, 1f), new Vector3(0.0f, 3f, 0.0f), FaceOrientation.Front, true).Material = new Material()
    {
      Diffuse = Vector3.Zero,
      Opacity = 0.0f
    };
  }

  protected override void Begin()
  {
    base.Begin();
    this.SinceStarted = TimeSpan.Zero;
    this.PlayerManager.LookingDirection = HorizontalDirection.Left;
    this.SinceRisen = TimeSpan.Zero;
    this.Phase = GateWarp.Phases.Rise;
    foreach (SoundEmitter emitter in this.SoundManager.Emitters)
      emitter.FadeOutAndDie(2f);
    this.SoundManager.FadeFrequencies(true, 2f);
    this.SoundManager.FadeVolume(this.SoundManager.MusicVolumeFactor, 0.0f, 3f);
    this.WarpSound.EmitAt(this.PlayerManager.Position);
    this.rgbPlanes = (Mesh) null;
    this.particles = (PlaneParticleSystem) null;
    this.sinceInitialized = 0.0f;
    this.originalCenter = this.CameraManager.Center;
    this.CameraManager.Constrained = true;
    this.GateAo = this.LevelManager.ArtObjects.Values.FirstOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.WarpGate));
    if (this.GateAo == null || this.GameState.SaveData.UnlockedWarpDestinations.Count <= 1)
      return;
    this.InitializeRgbGate();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    Vector3 position = this.GateAo.Position;
    if (this.GateAo.ArtObject != null && (double) this.GateAo.ArtObject.Size.Y == 8.0)
      position -= new Vector3(0.0f, 1f, 0.0f);
    this.sinceInitialized += (float) elapsed.TotalSeconds;
    if (this.rgbPlanes != null)
    {
      this.rgbPlanes.Rotation = this.CameraManager.Rotation;
      this.rgbPlanes.Position = position + new Vector3(0.0f, -2f, 0.0f);
      double totalSeconds = this.SinceStarted.TotalSeconds;
      for (int index = 0; index < 3; ++index)
      {
        float num1 = (float) (((double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 3.0) / 1.25), EasingType.Decic) * 0.89999997615814209 + (double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 1.0) / 2.0), EasingType.Cubic) * 0.10000000149011612) * (double) this.CameraManager.Radius / 2.0);
        float num2 = 0.0f;
        float amount1 = Easing.EaseIn(FezMath.Saturate((totalSeconds - 2.0) / 2.0), EasingType.Septic);
        this.rgbPlanes.Groups[index].Position = (float) Math.Sin((double) this.sinceInitialized * 1.5 + (double) index * 1.5707963705062866) * 0.375f * Vector3.UnitY + (float) (index - 1) * (1f / 1000f) * Vector3.UnitZ + MathHelper.Lerp((float) Math.Cos((double) this.sinceInitialized * 0.5 + (double) index * 1.5707963705062866 + (double) Easing.EaseIn(totalSeconds, EasingType.Quadratic)) * (float) ((double) num1 / 10.0 + 0.75), 0.0f, amount1) * Vector3.UnitX - num1 * Vector3.UnitY * 1.125f + (float) (index - 1) * Vector3.UnitY * 0.3f;
        if (this.Phase < GateWarp.Phases.Decelerate)
        {
          float amount2 = (float) ((double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 3.0) / 1.375), EasingType.Decic) * 0.60000002384185791 + (double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 1.0) / 2.0), EasingType.Cubic) * 0.40000000596046448);
          float num3 = MathHelper.Lerp(1f + num2, 1f / 16f, amount2);
          this.rgbPlanes.Groups[index].Scale = new Vector3(num3, 1f + num1, num3);
        }
      }
    }
    switch (this.Phase)
    {
      case GateWarp.Phases.Rise:
        this.Phase = GateWarp.Phases.Accelerate;
        break;
      case GateWarp.Phases.Accelerate:
      case GateWarp.Phases.Warping:
        this.SinceRisen += elapsed;
        this.SinceStarted += elapsed;
        this.PlayerManager.Animation.Timing.Update(elapsed, Math.Max((float) this.SinceRisen.TotalSeconds / 2f, 0.0f));
        Vector3 vector3 = Vector3.UnitY * 15f * Easing.EaseIn(this.SinceStarted.TotalSeconds / 4.5, EasingType.Decic);
        this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.GateAo.Position - this.CameraManager.Viewpoint.ForwardVector() * 3f + vector3, 0.0375f);
        this.CameraManager.Center = this.originalCenter + vector3 * 0.4f;
        this.particles.Settings.Acceleration *= 1.0075f;
        this.particles.Settings.SpawningSpeed *= 1.0075f;
        VaryingVector3 velocity1 = this.particles.Settings.Velocity;
        velocity1.Base = velocity1.Base * 1.01f;
        VaryingVector3 velocity2 = this.particles.Settings.Velocity;
        velocity2.Variation = velocity2.Variation * 1.0115f;
        if (this.Phase != GateWarp.Phases.Warping && this.SinceStarted.TotalSeconds > 4.0)
        {
          this.Phase = GateWarp.Phases.Warping;
          ScreenFade component = new ScreenFade(ServiceHelper.Game);
          component.FromColor = ColorEx.TransparentWhite;
          component.ToColor = Color.White;
          component.Duration = 0.5f;
          ServiceHelper.AddComponent((IGameComponent) component);
          component.Faded += (Action) (() =>
          {
            this.PlayerManager.Hidden = true;
            this.SinceStarted = TimeSpan.Zero;
            this.Phase = GateWarp.Phases.Decelerate;
            this.particles.FadeOutAndDie(1f);
            this.rgbPlanes = (Mesh) null;
            ServiceHelper.AddComponent((IGameComponent) new ScreenFade(ServiceHelper.Game)
            {
              FromColor = Color.White,
              ToColor = ColorEx.TransparentWhite,
              Duration = 1f
            });
          });
          break;
        }
        break;
      case GateWarp.Phases.Decelerate:
        this.SinceStarted += elapsed;
        this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, position - this.CameraManager.Viewpoint.ForwardVector() * 3f, 0.025f);
        if (this.SinceStarted.TotalSeconds > 1.0)
        {
          this.Phase = GateWarp.Phases.FadeOut;
          this.SinceStarted = TimeSpan.Zero;
          break;
        }
        break;
      case GateWarp.Phases.FadeOut:
      case GateWarp.Phases.LevelChange:
        this.SinceStarted += elapsed;
        this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, position - this.CameraManager.Viewpoint.ForwardVector() * 3f, 0.025f);
        if (this.Phase != GateWarp.Phases.LevelChange && this.SinceStarted.TotalSeconds > 2.25)
        {
          this.Phase = GateWarp.Phases.LevelChange;
          this.SinceStarted = TimeSpan.Zero;
          this.GameState.Loading = true;
          Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoLoad));
          worker.Finished += (Action) (() => this.ThreadPool.Return<bool>(worker));
          worker.Start(false);
          break;
        }
        break;
      case GateWarp.Phases.FadeIn:
        this.SinceStarted += elapsed;
        if (this.SinceStarted.TotalSeconds > 2.25)
        {
          this.SinceStarted = TimeSpan.Zero;
          this.Phase = GateWarp.Phases.None;
          break;
        }
        break;
    }
    return false;
  }

  private void DoLoad(bool dummy)
  {
    this.LevelManager.ChangeLevel(this.PlayerManager.WarpPanel.Destination);
    this.Phase = GateWarp.Phases.FadeIn;
    this.GameState.SaveData.View = this.PlayerManager.OriginWarpViewpoint;
    this.GameState.SaveData.Ground = this.LevelManager.ArtObjects.Values.First<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.WarpGate)).Position - Vector3.UnitY + this.GameState.SaveData.View.VisibleOrientation().AsVector() * 2f;
    this.PlayerManager.CheckpointGround = (TrileInstance) null;
    this.PlayerManager.RespawnAtCheckpoint();
    this.CameraManager.Center = this.PlayerManager.Position + Vector3.Up * this.PlayerManager.Size.Y / 2f + Vector3.UnitY;
    this.CameraManager.SnapInterpolation();
    this.LevelMaterializer.CullInstances();
    this.PlayerManager.Hidden = false;
    this.GameState.ScheduleLoadEnd = true;
    this.SinceStarted = TimeSpan.Zero;
    this.PlayerManager.WarpPanel = (WarpPanel) null;
    this.particles = (PlaneParticleSystem) null;
  }

  public override void Draw(GameTime gameTime)
  {
    base.Draw(gameTime);
    if (!this.IsActionAllowed(this.PlayerManager.Action))
      return;
    if (this.rgbPlanes != null && this.Phase <= GateWarp.Phases.Decelerate)
    {
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
      int num = 1;
      float amount = 0.01f;
      for (int index = 0; index < 3; ++index)
      {
        this.rgbPlanes.Groups[index].Material.Diffuse = Vector3.Lerp(this.rgbPlanes.Groups[index].Material.Diffuse, new Vector3(index == 0 ? (float) num : 0.0f, index == 1 ? (float) num : 0.0f, index == 2 ? (float) num : 0.0f), amount);
        this.rgbPlanes.Groups[index].Material.Opacity = MathHelper.Lerp(this.rgbPlanes.Groups[index].Material.Opacity, (float) num, amount);
      }
      this.rgbPlanes.Draw();
    }
    if (this.Phase != GateWarp.Phases.FadeOut && this.Phase != GateWarp.Phases.FadeIn && this.Phase != GateWarp.Phases.LevelChange)
      return;
    double linearStep = this.SinceStarted.TotalSeconds / 2.25;
    if (this.Phase == GateWarp.Phases.FadeIn)
      linearStep = 1.0 - linearStep;
    float alpha = FezMath.Saturate(Easing.EaseIn(linearStep, EasingType.Cubic));
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, alpha));
  }

  private void DrawLights()
  {
    if (!this.IsActionAllowed(this.PlayerManager.Action) || this.LevelManager.WaterType == LiquidType.Sewer || this.rgbPlanes == null || this.Phase > GateWarp.Phases.Decelerate)
      return;
    (this.rgbPlanes.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
    this.rgbPlanes.Draw();
    (this.rgbPlanes.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.GateWarp || this.Phase != 0;
  }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { private get; set; }

  [ServiceDependency]
  public IPlaneParticleSystems PlaneParticleSystems { private get; set; }

  private enum Phases
  {
    None,
    Rise,
    Accelerate,
    Warping,
    Decelerate,
    FadeOut,
    LevelChange,
    FadeIn,
  }
}
