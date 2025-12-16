// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.LesserWarp
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#nullable disable
namespace FezGame.Components.Actions;

internal class LesserWarp : PlayerAction
{
  private const float FadeSeconds = 2.5f;
  private ArtObjectInstance GateAo;
  private Vector3 OriginalPosition;
  private Mesh MaskMesh;
  private Texture2D WhiteTexture;
  private Texture2D StarTexture;
  private LesserWarp.Phases Phase;
  private float GateAngle;
  private float GateTurnSpeed;
  private Vector3 RiseAxis;
  private float RisePhi;
  private float RiseStep;
  private TimeSpan SinceRisen;
  private TimeSpan SinceStarted;
  private TrileInstance CubeShard;
  private Vector3 OriginalCenter;
  private PlaneParticleSystem particles;
  private Mesh rgbPlanes;
  private float sinceInitialized;
  private SoundEffect sRise;
  private SoundEffect sLower;
  private SoundEffect sActivate;
  private SoundEmitter eIdleSpin;
  private IWaiter fader;

  public LesserWarp(Game game)
    : base(game)
  {
    this.DrawOrder = 901;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    if (this.eIdleSpin != null && this.eIdleSpin.Cue != null && !this.eIdleSpin.Cue.IsDisposed && this.eIdleSpin.Cue.State != SoundState.Stopped)
      this.eIdleSpin.Cue.Stop();
    this.eIdleSpin = (SoundEmitter) null;
    this.rgbPlanes = (Mesh) null;
    this.particles = (PlaneParticleSystem) null;
    this.Phase = LesserWarp.Phases.None;
    this.sinceInitialized = 0.0f;
    this.GateAo = this.LevelManager.ArtObjects.Values.FirstOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.LesserGate));
    if (this.GateAo == null)
      return;
    this.InitializeRgbGate();
    this.MaskMesh = new Mesh()
    {
      DepthWrites = false,
      Texture = (Dirtyable<Texture>) (Texture) this.WhiteTexture,
      Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, -1.57079637f)
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh maskMesh = this.MaskMesh;
      maskMesh.Effect = (BaseEffect) new DefaultEffect.Textured()
      {
        Fullbright = true
      };
    }));
    this.MaskMesh.AddFace(new Vector3(2f), Vector3.Zero, FaceOrientation.Front, true, true);
    this.MaskMesh.BakeTransformWithNormal<FezVertexPositionNormalTexture>();
    this.MaskMesh.Position = this.GateAo.Position - Vector3.UnitY * 1.25f;
    this.CubeShard = this.LevelManager.Triles.Values.FirstOrDefault<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type.IsCubeShard() && (double) Vector3.Distance(x.Center, this.GateAo.Position) < 3.0));
    this.OriginalPosition = this.GateAo.Position;
    this.sRise = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/LesserWarpRise");
    this.sLower = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/LesserWarpLower");
    this.sActivate = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/WarpGateActivate");
    this.eIdleSpin = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/LesserWarpIdleSpin").EmitAt(this.GateAo.Position, true, true);
  }

  private void InitializeRgbGate()
  {
    IPlaneParticleSystems planeParticleSystems = this.PlaneParticleSystems;
    Game game = this.Game;
    PlaneParticleSystemSettings settings = new PlaneParticleSystemSettings();
    settings.SpawnVolume = new BoundingBox()
    {
      Min = this.GateAo.Position + new Vector3(-2f, -3.5f, -2f),
      Max = this.GateAo.Position + new Vector3(2f, 2f, 2f)
    };
    settings.Velocity.Base = new Vector3(0.0f, 0.6f, 0.0f);
    settings.Velocity.Variation = new Vector3(0.0f, 0.1f, 0.1f);
    settings.SpawningSpeed = 5f;
    settings.ParticleLifetime = 6f;
    settings.Acceleration = 0.375f;
    settings.SizeBirth = (VaryingVector3) new Vector3(0.25f, 0.25f, 0.25f);
    settings.ColorBirth = (VaryingColor) Color.Black;
    settings.ColorLife = (VaryingColor) Color.Black;
    settings.ColorDeath = (VaryingColor) Color.Black;
    settings.FullBright = true;
    settings.RandomizeSpawnTime = true;
    settings.Billboarding = true;
    settings.BlendingMode = BlendingMode.Additive;
    PlaneParticleSystem system = this.particles = new PlaneParticleSystem(game, 200, settings);
    planeParticleSystems.Add(system);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.particles.Settings.Texture = this.CMProvider.Global.Load<Texture2D>("Background Planes/dust_particle");
      this.particles.RefreshTexture();
    }));
    this.rgbPlanes = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true,
      SamplerState = SamplerState.LinearClamp,
      Blending = new BlendingMode?(BlendingMode.Additive)
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.rgbPlanes.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.rgbPlanes.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/rgb_gradient");
    }));
    this.particles.Enabled = this.particles.Visible = false;
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
    this.rgbPlanes.Position = this.GateAo.Position + new Vector3(0.0f, -2f, 0.0f);
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.WhiteTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/FullWhite");
    this.StarTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/black_hole/Stars");
    this.LightingPostProcess.DrawOnTopLights += new Action(this.DrawLights);
  }

  protected override void TestConditions()
  {
    if (this.PlayerManager.Action == ActionType.LesserWarp || this.GateAo == null || this.Phase == LesserWarp.Phases.FadeIn)
      return;
    Vector3 b = this.CameraManager.Viewpoint.SideMask();
    Vector3 a = (this.OriginalPosition - this.PlayerManager.Position).Abs();
    bool flag = (double) a.Dot(b) < 3.0 && (double) a.Y < 4.0 && this.CubeShard == null;
    if (this.LevelManager.Name == "ZU_CODE_LOOP" && this.GameState.SaveData.ThisLevel.ScriptingState == "NOT_COLLECTED")
      flag = false;
    if (flag && (this.Phase == LesserWarp.Phases.None || this.Phase == LesserWarp.Phases.Lower))
    {
      if (this.Phase != LesserWarp.Phases.Lower)
      {
        this.RiseAxis = this.CameraManager.Viewpoint.RightVector();
        this.RisePhi = this.CameraManager.Viewpoint.ToPhi();
        this.SinceRisen = TimeSpan.Zero;
        this.SinceStarted = TimeSpan.Zero;
      }
      this.Phase = LesserWarp.Phases.Rise;
      this.sRise.EmitAt(this.GateAo.Position);
    }
    else if (!flag && this.Phase != LesserWarp.Phases.Lower && this.Phase != LesserWarp.Phases.None)
    {
      if (this.Phase != LesserWarp.Phases.Rise)
      {
        this.RiseAxis = this.CameraManager.Viewpoint.RightVector();
        this.RisePhi = this.CameraManager.Viewpoint.ToPhi();
      }
      this.Phase = this.Phase == LesserWarp.Phases.Rise ? LesserWarp.Phases.Lower : LesserWarp.Phases.Decelerate;
      this.SinceStarted = TimeSpan.Zero;
      if (this.Phase == LesserWarp.Phases.Lower)
        this.sLower.EmitAt(this.GateAo.Position);
    }
    else if (!flag && this.Phase == LesserWarp.Phases.None && this.eIdleSpin != null && !this.eIdleSpin.Dead && this.eIdleSpin.Cue.State == SoundState.Playing)
      this.eIdleSpin.Cue.Pause();
    if (this.CubeShard != null && this.PlayerManager.LastAction == ActionType.FindingTreasure && this.CubeShard.Removed)
      this.CubeShard = (TrileInstance) null;
    if (!(this.PlayerManager.Grounded & flag) || this.InputManager.Up != FezButtonState.Pressed || !this.SpeechBubble.Hidden)
      return;
    this.PlayerManager.Action = ActionType.LesserWarp;
    if (this.Phase == LesserWarp.Phases.None)
    {
      this.RiseAxis = this.CameraManager.Viewpoint.RightVector();
      this.RisePhi = this.CameraManager.Viewpoint.ToPhi();
      this.SinceRisen = TimeSpan.Zero;
      this.SinceStarted = TimeSpan.Zero;
      this.Phase = LesserWarp.Phases.Rise;
      this.sRise.EmitAt(this.GateAo.Position);
    }
    else
    {
      if (this.Phase != LesserWarp.Phases.SpinWait)
        return;
      if (this.eIdleSpin.Cue.State == SoundState.Playing)
      {
        if (this.fader != null)
          this.fader.Cancel();
        this.eIdleSpin.FadeOutAndPause(1f);
      }
      this.DotManager.PreventPoI = true;
      this.DotManager.Burrow();
      foreach (SoundEmitter emitter in this.SoundManager.Emitters)
        emitter.FadeOutAndDie(2f);
      this.SoundManager.FadeFrequencies(true, 2f);
      this.SoundManager.FadeVolume(this.SoundManager.MusicVolumeFactor, 0.0f, 3f);
      this.sActivate.EmitAt(this.GateAo.Position);
      this.Phase = LesserWarp.Phases.Accelerate;
      this.CameraManager.Constrained = true;
      this.OriginalCenter = this.CameraManager.Center;
      this.PlayerManager.LookingDirection = HorizontalDirection.Left;
      this.PlayerManager.Velocity = Vector3.Zero;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    this.SinceStarted = TimeSpan.Zero;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    base.Update(gameTime);
    if (this.rgbPlanes == null)
      return;
    this.rgbPlanes.Rotation = this.CameraManager.Rotation;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    this.sinceInitialized += (float) elapsed.TotalSeconds;
    if (this.rgbPlanes != null)
    {
      float num1 = (float) (0.25 + (this.Phase == LesserWarp.Phases.Lower || this.Phase == LesserWarp.Phases.Rise || this.Phase == LesserWarp.Phases.FadeOut ? (double) this.RiseStep : 1.0) * 0.75);
      double totalSeconds = this.SinceStarted.TotalSeconds;
      for (int index = 0; index < 3; ++index)
      {
        float num2 = (float) (((double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 3.0) / 1.25), EasingType.Decic) * 0.89999997615814209 + (double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 1.0) / 2.0), EasingType.Cubic) * 0.10000000149011612) * (double) this.CameraManager.Radius / 2.0);
        float num3 = 0.0f;
        float amount1 = Easing.EaseIn(FezMath.Saturate((totalSeconds - 2.0) / 2.0), EasingType.Septic);
        this.rgbPlanes.Groups[index].Position = (float) Math.Sin((double) this.sinceInitialized * 1.5 + (double) index * 1.5707963705062866) * 0.375f * Vector3.UnitY + (float) (index - 1) * (1f / 1000f) * Vector3.UnitZ + MathHelper.Lerp((float) Math.Cos((double) this.sinceInitialized * 0.5 + (double) index * 1.5707963705062866 + (double) Easing.EaseIn(totalSeconds, EasingType.Quadratic)) * (float) ((double) num2 / 10.0 + 0.75), 0.0f, amount1) * Vector3.UnitX - num2 * Vector3.UnitY * 1.125f + (float) (index - 1) * Vector3.UnitY * 0.3f;
        if (this.Phase < LesserWarp.Phases.Decelerate)
        {
          float amount2 = (float) ((double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 3.0) / 1.375), EasingType.Decic) * 0.60000002384185791 + (double) Easing.EaseIn(FezMath.Saturate((totalSeconds - 1.0) / 2.0), EasingType.Cubic) * 0.40000000596046448);
          float num4 = MathHelper.Lerp(1f + num3, 1f / 16f, amount2);
          this.rgbPlanes.Groups[index].Scale = new Vector3(num4, num1 + num2, num4);
        }
      }
    }
    switch (this.Phase)
    {
      case LesserWarp.Phases.Rise:
        this.particles.Enabled = this.particles.Visible = true;
        this.SinceRisen += elapsed;
        this.RiseStep = Easing.EaseInOut((double) FezMath.Saturate((float) this.SinceRisen.TotalSeconds / 2f), EasingType.Sine);
        this.GateAo.Position = Vector3.Lerp(this.OriginalPosition, this.OriginalPosition + Vector3.UnitY, this.RiseStep);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(this.RiseAxis, this.RiseStep * 1.57079637f);
        this.MaskMesh.Position = this.GateAo.Position - Vector3.UnitY * 1.25f * (float) Math.Cos((double) this.RiseStep * 1.5707963705062866);
        this.MaskMesh.Rotation = this.GateAo.Rotation;
        if (this.PlayerManager.Action == ActionType.LesserWarp)
          this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.GateAo.Position - this.CameraManager.Viewpoint.ForwardVector() * 3f, 0.025f);
        this.GateAo.Position += Vector3.Transform(new Vector3((float) Math.Cos((double) this.GateAngle), 0.0f, (float) Math.Sin(3.1415927410125732 + (double) this.GateAngle)), Quaternion.CreateFromAxisAngle(Vector3.Up, this.RisePhi - 1.57079637f)) * 1.25f * (float) Math.Sin((double) this.RiseStep * 1.5707963705062866);
        if (this.SinceRisen.TotalSeconds > 1.5)
        {
          this.Phase = this.PlayerManager.Action == ActionType.LesserWarp ? LesserWarp.Phases.Accelerate : LesserWarp.Phases.SpinWait;
          if (this.Phase == LesserWarp.Phases.Accelerate)
          {
            this.sActivate.EmitAt(this.GateAo.Position);
            this.OriginalCenter = this.CameraManager.Center;
            this.CameraManager.Constrained = true;
            break;
          }
          this.eIdleSpin.VolumeFactor = 0.0f;
          this.fader = Waiters.Interpolate(1.0, (Action<float>) (s => this.eIdleSpin.VolumeFactor = s), (Action) (() => this.fader = (IWaiter) null));
          this.fader.AutoPause = true;
          if (!this.eIdleSpin.Dead)
          {
            this.eIdleSpin.Cue.Resume();
            break;
          }
          break;
        }
        break;
      case LesserWarp.Phases.SpinWait:
        this.SinceRisen += elapsed;
        if (this.SinceRisen.TotalSeconds >= 2.0)
          this.SinceRisen = TimeSpan.FromSeconds(2.0);
        this.RiseStep = Easing.EaseInOut((double) FezMath.Saturate((float) this.SinceRisen.TotalSeconds / 2f), EasingType.Sine);
        this.GateAo.Position = Vector3.Lerp(this.OriginalPosition, this.OriginalPosition + Vector3.UnitY, this.RiseStep);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(this.RiseAxis, this.RiseStep * 1.57079637f);
        this.MaskMesh.Position = this.GateAo.Position - Vector3.UnitY * 1.25f * (float) Math.Cos((double) this.RiseStep * 1.5707963705062866);
        this.MaskMesh.Rotation = this.GateAo.Rotation;
        this.GateTurnSpeed = MathHelper.Lerp(this.GateTurnSpeed, 0.015f, 0.075f);
        this.GateAngle = FezMath.WrapAngle(this.GateAngle + this.GateTurnSpeed);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.GateAo.Rotation;
        this.MaskMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.MaskMesh.Rotation;
        this.GateAo.Position += Vector3.Transform(new Vector3((float) Math.Cos((double) this.GateAngle), 0.0f, (float) Math.Sin(3.1415927410125732 + (double) this.GateAngle)), Quaternion.CreateFromAxisAngle(Vector3.Up, this.RisePhi - 1.57079637f)) * 1.25f * (float) Math.Sin((double) this.RiseStep * 1.5707963705062866);
        break;
      case LesserWarp.Phases.Lower:
        this.SinceRisen -= elapsed;
        this.RiseStep = Easing.EaseInOut((double) FezMath.Saturate((float) this.SinceRisen.TotalSeconds / 2f), EasingType.Sine);
        this.GateAo.Position = Vector3.Lerp(this.OriginalPosition, this.OriginalPosition + Vector3.UnitY, this.RiseStep);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(this.RiseAxis, this.RiseStep * 1.57079637f);
        this.MaskMesh.Position = this.GateAo.Position - Vector3.UnitY * 1.25f * (float) Math.Cos((double) this.RiseStep * 1.5707963705062866);
        this.MaskMesh.Rotation = this.GateAo.Rotation;
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.GateAo.Rotation;
        this.MaskMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.MaskMesh.Rotation;
        this.GateAo.Position += Vector3.Transform(new Vector3((float) Math.Cos((double) this.GateAngle), 0.0f, (float) Math.Sin(3.1415927410125732 + (double) this.GateAngle)), Quaternion.CreateFromAxisAngle(Vector3.Up, this.RisePhi - 1.57079637f)) * 1.25f * (float) Math.Sin((double) this.RiseStep * 1.5707963705062866);
        if (this.SinceRisen.Ticks <= 0L)
        {
          this.Phase = LesserWarp.Phases.None;
          break;
        }
        break;
      case LesserWarp.Phases.Accelerate:
      case LesserWarp.Phases.Warping:
        this.DotManager.PreventPoI = true;
        this.SinceRisen += elapsed;
        if (this.SinceRisen.TotalSeconds >= 2.0)
          this.SinceRisen = TimeSpan.FromSeconds(2.0);
        this.SinceStarted += elapsed;
        this.PlayerManager.Animation.Timing.Update(elapsed, Math.Max((float) this.SinceStarted.TotalSeconds / 4f, 0.0f));
        this.RiseStep = Easing.EaseInOut((double) FezMath.Saturate((float) this.SinceRisen.TotalSeconds / 2f), EasingType.Sine);
        this.GateAo.Position = Vector3.Lerp(this.OriginalPosition, this.OriginalPosition + Vector3.UnitY, this.RiseStep);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(this.RiseAxis, this.RiseStep * 1.57079637f);
        this.MaskMesh.Position = this.GateAo.Position - Vector3.UnitY * 1.25f * (float) Math.Cos((double) this.RiseStep * 1.5707963705062866);
        this.MaskMesh.Rotation = this.GateAo.Rotation;
        this.GateTurnSpeed *= 1.01f;
        this.GateTurnSpeed += (float) elapsed.TotalSeconds / 50f;
        this.GateAngle += this.GateTurnSpeed;
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.GateAo.Rotation;
        this.MaskMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.MaskMesh.Rotation;
        Vector3 vector3 = Vector3.UnitY * 15f * Easing.EaseIn(this.SinceStarted.TotalSeconds / 4.5, EasingType.Decic);
        this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.GateAo.Position - this.CameraManager.Viewpoint.ForwardVector() * 3f + vector3, 0.0375f);
        this.CameraManager.Center = this.OriginalCenter + vector3 * 0.4f;
        this.particles.Settings.Acceleration *= 1.0075f;
        this.particles.Settings.SpawningSpeed *= 1.0075f;
        VaryingVector3 velocity1 = this.particles.Settings.Velocity;
        velocity1.Base = velocity1.Base * 1.01f;
        VaryingVector3 velocity2 = this.particles.Settings.Velocity;
        velocity2.Variation = velocity2.Variation * 1.01f;
        this.GateAo.Position += Vector3.Transform(new Vector3((float) Math.Cos((double) this.GateAngle), 0.0f, (float) Math.Sin(3.1415927410125732 + (double) this.GateAngle)), Quaternion.CreateFromAxisAngle(Vector3.Up, this.RisePhi - 1.57079637f)) * 1.25f * (float) Math.Sin((double) this.RiseStep * 1.5707963705062866);
        if (this.Phase != LesserWarp.Phases.Warping && this.SinceStarted.TotalSeconds > 4.0)
        {
          this.Phase = LesserWarp.Phases.Warping;
          ScreenFade component = new ScreenFade(ServiceHelper.Game);
          component.FromColor = ColorEx.TransparentWhite;
          component.ToColor = Color.White;
          component.Duration = 0.5f;
          ServiceHelper.AddComponent((IGameComponent) component);
          component.Faded += (Action) (() =>
          {
            this.PlayerManager.Hidden = true;
            this.SinceStarted = TimeSpan.Zero;
            this.Phase = LesserWarp.Phases.Decelerate;
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
      case LesserWarp.Phases.Decelerate:
        this.DotManager.PreventPoI = true;
        this.SinceStarted += elapsed;
        this.RiseStep = 1f;
        this.GateAo.Position = Vector3.Lerp(this.OriginalPosition, this.OriginalPosition + Vector3.UnitY, this.RiseStep);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(this.RiseAxis, this.RiseStep * 1.57079637f);
        this.MaskMesh.Position = this.GateAo.Position - Vector3.UnitY * 1.25f * (float) Math.Cos((double) this.RiseStep * 1.5707963705062866);
        this.MaskMesh.Rotation = this.GateAo.Rotation;
        float b = (double) this.GateAngle < 3.1415927410125732 ? 0.0f : 6.28318548f;
        this.GateTurnSpeed = FezMath.Saturate(this.GateTurnSpeed - 0.008333334f);
        this.GateAngle += this.GateTurnSpeed;
        this.GateAngle = (double) this.GateTurnSpeed >= 0.05000000074505806 ? FezMath.WrapAngle(this.GateAngle) : MathHelper.Lerp(this.GateAngle, b, 0.05f);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.GateAo.Rotation;
        this.MaskMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.MaskMesh.Rotation;
        if (this.PlayerManager.Action == ActionType.LesserWarp)
          this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.GateAo.Position - this.CameraManager.Viewpoint.ForwardVector() * 3f, 0.025f);
        this.GateAo.Position += Vector3.Transform(new Vector3((float) Math.Cos((double) this.GateAngle), 0.0f, (float) Math.Sin(3.1415927410125732 + (double) this.GateAngle)), Quaternion.CreateFromAxisAngle(Vector3.Up, this.RisePhi - 1.57079637f)) * 1.25f * (float) Math.Sin((double) this.RiseStep * 1.5707963705062866);
        if (this.SinceStarted.TotalSeconds > 2.0 || FezMath.AlmostEqual(this.GateTurnSpeed, 0.0f) && FezMath.AlmostEqual((double) this.GateAngle, (double) b, 0.1))
        {
          this.GateAngle = b;
          this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.GateAo.Rotation;
          this.Phase = this.PlayerManager.Action == ActionType.LesserWarp ? LesserWarp.Phases.FadeOut : LesserWarp.Phases.Lower;
          this.SinceStarted = TimeSpan.Zero;
          if (!this.eIdleSpin.Dead && this.eIdleSpin.Cue.State == SoundState.Playing)
          {
            if (this.fader != null)
              this.fader.Cancel();
            this.eIdleSpin.FadeOutAndPause(1f);
          }
          if (this.PlayerManager.Action != ActionType.LesserWarp)
          {
            this.sLower.EmitAt(this.GateAo.Position);
            break;
          }
          break;
        }
        break;
      case LesserWarp.Phases.FadeOut:
      case LesserWarp.Phases.LevelChange:
        this.DotManager.PreventPoI = true;
        this.SinceStarted += elapsed;
        this.RiseStep = 1f - Easing.EaseInOut((double) FezMath.Saturate((float) this.SinceStarted.TotalSeconds / 1.75f), EasingType.Sine);
        this.GateAo.Position = Vector3.Lerp(this.OriginalPosition, this.OriginalPosition + Vector3.UnitY, this.RiseStep);
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(this.RiseAxis, this.RiseStep * 1.57079637f);
        this.MaskMesh.Position = this.GateAo.Position - Vector3.UnitY * 1.25f * (float) Math.Cos((double) this.RiseStep * 1.5707963705062866);
        this.MaskMesh.Rotation = this.GateAo.Rotation;
        this.GateAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.GateAo.Rotation;
        this.MaskMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.GateAngle) * this.MaskMesh.Rotation;
        this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.GateAo.Position - this.CameraManager.Viewpoint.ForwardVector() * 3f, 0.025f);
        this.GateAo.Position += Vector3.Transform(new Vector3((float) Math.Cos((double) this.GateAngle), 0.0f, (float) Math.Sin(3.1415927410125732 + (double) this.GateAngle)), Quaternion.CreateFromAxisAngle(Vector3.Up, this.RisePhi - 1.57079637f)) * 1.25f * (float) Math.Sin((double) this.RiseStep * 1.5707963705062866);
        if (this.Phase != LesserWarp.Phases.LevelChange && this.SinceStarted.TotalSeconds > 2.5)
        {
          this.Phase = LesserWarp.Phases.LevelChange;
          this.SinceStarted = TimeSpan.Zero;
          this.GameState.Loading = true;
          Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoLoad));
          worker.Finished += (Action) (() => this.ThreadPool.Return<bool>(worker));
          worker.Start(false);
          break;
        }
        break;
      case LesserWarp.Phases.FadeIn:
        this.SinceStarted += elapsed;
        if (this.SinceStarted.TotalSeconds > 2.5)
        {
          this.SinceStarted = TimeSpan.Zero;
          this.Phase = LesserWarp.Phases.None;
          break;
        }
        break;
    }
    return false;
  }

  private void DoLoad(bool dummy)
  {
    this.LevelManager.ChangeLevel(this.GateAo.ActorSettings.DestinationLevel);
    this.Phase = LesserWarp.Phases.FadeIn;
    this.GameState.SaveData.Ground = this.LevelManager.ArtObjects.Values.First<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.WarpGate)).Position + Vector3.Down + this.GameState.SaveData.View.VisibleOrientation().AsVector() * 2f;
    this.DotManager.PreventPoI = false;
    this.PlayerManager.Hidden = false;
    this.PlayerManager.CheckpointGround = (TrileInstance) null;
    this.PlayerManager.RespawnAtCheckpoint();
    this.CameraManager.Center = this.PlayerManager.Position + Vector3.Up * this.PlayerManager.Size.Y / 2f + Vector3.UnitY;
    this.CameraManager.SnapInterpolation();
    this.LevelMaterializer.CullInstances();
    this.GameState.ScheduleLoadEnd = true;
    this.SinceStarted = TimeSpan.Zero;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || !this.IsActionAllowed(this.PlayerManager.Action))
      return;
    if (this.Phase != LesserWarp.Phases.LevelChange && this.Phase != LesserWarp.Phases.FadeIn)
    {
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.WarpGate));
      this.MaskMesh.Draw();
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.WarpGate);
      float viewScale = this.GraphicsDevice.GetViewScale();
      float m11 = this.CameraManager.Radius / ((float) this.StarTexture.Width / 16f) / viewScale;
      float m22 = (float) ((double) this.CameraManager.Radius / (double) this.CameraManager.AspectRatio / ((double) this.StarTexture.Height / 16.0)) / viewScale;
      Matrix textureMatrix = new Matrix(m11, 0.0f, 0.0f, 0.0f, 0.0f, m22, 0.0f, 0.0f, (float) (-(double) m11 / 2.0), (float) (-(double) m22 / 2.0), 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
      this.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
      this.TargetRenderer.DrawFullscreen((Texture) this.StarTexture, textureMatrix);
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    }
    if (this.rgbPlanes != null && this.Phase <= LesserWarp.Phases.Decelerate)
    {
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
      float num = (float) ((this.Phase == LesserWarp.Phases.Lower || this.Phase == LesserWarp.Phases.Rise ? (double) this.RiseStep : 1.0) * (1.0 - (double) this.LevelManager.ActualDiffuse.R / 512.0) * 0.800000011920929);
      float amount = this.Phase == LesserWarp.Phases.Decelerate || this.Phase == LesserWarp.Phases.Lower || this.Phase == LesserWarp.Phases.FadeOut ? 1f : 0.01f;
      if (this.Phase == LesserWarp.Phases.Accelerate)
        num = 1f;
      for (int index = 0; index < 3; ++index)
      {
        this.rgbPlanes.Groups[index].Material.Diffuse = Vector3.Lerp(this.rgbPlanes.Groups[index].Material.Diffuse, new Vector3(index == 0 ? num : 0.0f, index == 1 ? num : 0.0f, index == 2 ? num : 0.0f), amount);
        this.rgbPlanes.Groups[index].Material.Opacity = MathHelper.Lerp(this.rgbPlanes.Groups[index].Material.Opacity, num, amount);
      }
      this.rgbPlanes.Draw();
    }
    if (this.Phase != LesserWarp.Phases.FadeOut && this.Phase != LesserWarp.Phases.FadeIn && this.Phase != LesserWarp.Phases.LevelChange)
      return;
    double linearStep = this.SinceStarted.TotalSeconds / 2.5;
    if (this.Phase == LesserWarp.Phases.FadeIn)
      linearStep = 1.0 - linearStep;
    float alpha = FezMath.Saturate(Easing.EaseIn(linearStep, EasingType.Cubic));
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, alpha));
  }

  private void DrawLights()
  {
    if (this.GameState.Loading || this.GameState.Paused || !this.IsActionAllowed(this.PlayerManager.Action) || this.LevelManager.WaterType == LiquidType.Sewer)
      return;
    float num = this.Phase == LesserWarp.Phases.Lower || this.Phase == LesserWarp.Phases.Rise || this.Phase == LesserWarp.Phases.FadeOut ? this.RiseStep : 1f;
    if (this.rgbPlanes != null && this.Phase <= LesserWarp.Phases.Decelerate)
    {
      this.particles.Settings.ColorLife.Base = new Color(num / 2f, num / 2f, num / 2f, 1f);
      this.particles.Settings.ColorLife.Variation = new Color(num / 2f, num / 2f, num / 2f, 1f);
      bool bufferWriteEnable = this.GraphicsDevice.GetDssCombiner().DepthBufferWriteEnable;
      StencilOperation stencilPass = this.GraphicsDevice.GetDssCombiner().StencilPass;
      this.GraphicsDevice.GetDssCombiner().DepthBufferWriteEnable = false;
      this.GraphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
      (this.rgbPlanes.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      this.rgbPlanes.Draw();
      (this.rgbPlanes.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
      this.GraphicsDevice.GetDssCombiner().DepthBufferWriteEnable = bufferWriteEnable;
      this.GraphicsDevice.GetDssCombiner().StencilPass = stencilPass;
    }
    (this.MaskMesh.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
    this.MaskMesh.Material.Opacity = num;
    this.MaskMesh.Draw();
    this.MaskMesh.Material.Opacity = 1f;
    (this.MaskMesh.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.LesserWarp || this.Phase != 0;
  }

  [ServiceDependency]
  public IDotManager DotManager { private get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }

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
    SpinWait,
    Lower,
    Accelerate,
    Warping,
    Decelerate,
    FadeOut,
    LevelChange,
    FadeIn,
  }
}
