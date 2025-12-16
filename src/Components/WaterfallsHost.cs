// Decompiled with JetBrains decompiler
// Type: FezGame.Components.WaterfallsHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class WaterfallsHost(Game game) : GameComponent(game)
{
  private readonly List<WaterfallsHost.WaterfallState> Waterfalls = new List<WaterfallsHost.WaterfallState>();
  private SoundEffect SewageFallSound;

  public override void Initialize()
  {
    base.Initialize();
    this.SewageFallSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Sewer/SewageFall");
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    foreach (WaterfallsHost.WaterfallState waterfall in this.Waterfalls)
      waterfall.Dispose();
    this.Waterfalls.Clear();
    BoundingBox boundingBox1;
    foreach (BackgroundPlane plane in this.LevelManager.BackgroundPlanes.Values.ToArray<BackgroundPlane>())
    {
      if (plane.ActorType == ActorType.Waterfall || plane.ActorType == ActorType.Trickle)
      {
        Vector3 vector3_1 = Vector3.Transform(plane.Size * plane.Scale * Vector3.UnitX / 2f, plane.Rotation);
        Vector3 vector = Vector3.Transform(Vector3.UnitZ, plane.Rotation);
        Vector3 vector3_2 = FezMath.XZMask - vector.Abs();
        Vector3 vector3_3 = plane.Position + plane.Size * plane.Scale * Vector3.UnitY / 2f - new Vector3(0.0f, 1f / 32f, 0.0f) - vector * 2f / 16f;
        Game game = this.Game;
        PlaneParticleSystemSettings settings = new PlaneParticleSystemSettings();
        boundingBox1 = new BoundingBox();
        boundingBox1.Min = vector3_3 - vector3_1;
        boundingBox1.Max = vector3_3 + vector3_1;
        settings.SpawnVolume = boundingBox1;
        VaryingVector3 varyingVector3 = new VaryingVector3();
        varyingVector3.Base = Vector3.Up * 1.6f + vector / 4f;
        varyingVector3.Variation = Vector3.Up * 0.8f + vector / 4f + vector3_2 / 2f;
        settings.Velocity = varyingVector3;
        settings.Gravity = new Vector3(0.0f, -0.15f, 0.0f);
        settings.SpawningSpeed = 5f;
        settings.RandomizeSpawnTime = true;
        settings.ParticleLifetime = 2f;
        settings.FadeInDuration = 0.0f;
        settings.FadeOutDuration = 0.1f;
        settings.SizeBirth = (VaryingVector3) new Vector3(1f / 16f);
        settings.ColorLife = (VaryingColor) (this.LevelManager.WaterType == LiquidType.Sewer ? new Color(215, 232, 148) : new Color(1f, 1f, 1f, 0.75f));
        settings.Texture = this.CMProvider.Global.Load<Texture2D>("Background Planes/white_square");
        settings.BlendingMode = BlendingMode.Alphablending;
        settings.ClampToTrixels = true;
        settings.Billboarding = true;
        settings.FullBright = this.LevelManager.WaterType == LiquidType.Sewer;
        settings.UseCallback = true;
        PlaneParticleSystem planeParticleSystem = new PlaneParticleSystem(game, 25, settings);
        if (this.LevelManager.WaterType == LiquidType.Sewer)
        {
          planeParticleSystem.DrawOrder = 20;
          planeParticleSystem.Settings.StencilMask = new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level);
        }
        this.PlaneParticleSystems.Add(planeParticleSystem);
        this.Waterfalls.Add(new WaterfallsHost.WaterfallState(plane, planeParticleSystem, this));
      }
      else if (plane.ActorType == ActorType.Drips)
      {
        Vector3 vector3_4 = new Vector3(plane.Size.X, 0.0f, plane.Size.X) / 2f;
        Vector3 vector3_5 = Vector3.Transform(Vector3.UnitZ, plane.Rotation);
        Vector3 vector3_6 = FezMath.XZMask - vector3_5.Abs();
        Vector3 vector3_7 = plane.Position - new Vector3(0.0f, 0.125f, 0.0f);
        int num = plane.Crosshatch ? 1 : (plane.Billboard ? 1 : 0);
        Game game = this.Game;
        PlaneParticleSystemSettings settings1 = new PlaneParticleSystemSettings();
        boundingBox1 = new BoundingBox();
        boundingBox1.Min = vector3_7 - vector3_4;
        boundingBox1.Max = vector3_7 + vector3_4;
        settings1.SpawnVolume = boundingBox1;
        VaryingVector3 varyingVector3 = new VaryingVector3();
        varyingVector3.Base = Vector3.Zero;
        varyingVector3.Variation = Vector3.Zero;
        settings1.Velocity = varyingVector3;
        settings1.Gravity = new Vector3(0.0f, -0.15f, 0.0f);
        settings1.SpawningSpeed = 2f;
        settings1.RandomizeSpawnTime = true;
        settings1.ParticleLifetime = 2f;
        settings1.FadeInDuration = 0.0f;
        settings1.FadeOutDuration = 0.0f;
        settings1.SizeBirth = (VaryingVector3) new Vector3(1f / 16f);
        settings1.ColorLife = (VaryingColor) (this.LevelManager.WaterType == LiquidType.Sewer ? new Color(215, 232, 148) : Color.White);
        settings1.Texture = this.CMProvider.Global.Load<Texture2D>("Background Planes/white_square");
        settings1.BlendingMode = BlendingMode.Alphablending;
        settings1.ClampToTrixels = true;
        settings1.FullBright = true;
        PlaneParticleSystem system = new PlaneParticleSystem(game, 25, settings1);
        if (this.LevelManager.WaterType == LiquidType.Sewer)
        {
          system.DrawOrder = 20;
          system.Settings.StencilMask = new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level);
        }
        if (num != 0)
        {
          system.Settings.Billboarding = true;
          PlaneParticleSystemSettings settings2 = system.Settings;
          boundingBox1 = new BoundingBox();
          boundingBox1.Min = vector3_7 - vector3_4;
          boundingBox1.Max = vector3_7 + vector3_4;
          BoundingBox boundingBox2 = boundingBox1;
          settings2.SpawnVolume = boundingBox2;
        }
        else
        {
          system.Settings.Doublesided = plane.Doublesided;
          PlaneParticleSystemSettings settings3 = system.Settings;
          boundingBox1 = new BoundingBox();
          boundingBox1.Min = vector3_7 - vector3_4 * vector3_6;
          boundingBox1.Max = vector3_7 + vector3_4 * vector3_6;
          BoundingBox boundingBox3 = boundingBox1;
          settings3.SpawnVolume = boundingBox3;
          system.Settings.Orientation = new FaceOrientation?(FezMath.OrientationFromDirection(vector3_5));
        }
        this.PlaneParticleSystems.Add(system);
      }
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    foreach (WaterfallsHost.WaterfallState waterfall in this.Waterfalls)
      waterfall.Update(gameTime.ElapsedGameTime);
  }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IPlaneParticleSystems PlaneParticleSystems { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  private class WaterfallState
  {
    private readonly List<BackgroundPlane> AttachedPlanes = new List<BackgroundPlane>();
    private readonly BackgroundPlane Plane;
    private readonly BackgroundPlane Splash;
    private readonly PlaneParticleSystem ParticleSystem;
    private readonly float Top;
    private readonly Vector3 TerminalPosition;
    private readonly WaterfallsHost Host;
    private float lastDistToTop;
    private SoundEmitter BubblingEmitter;
    private float sinceAlive;

    public WaterfallState(BackgroundPlane plane, PlaneParticleSystem ps, WaterfallsHost host)
    {
      WaterfallsHost.WaterfallState waterfallState = this;
      ServiceHelper.InjectServices((object) this);
      this.Host = host;
      this.Plane = plane;
      this.ParticleSystem = ps;
      bool flag = plane.ActorType == ActorType.Trickle;
      this.Splash = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, this.LevelManager.WaterType != LiquidType.Sewer ? (this.LevelManager.WaterType != LiquidType.Purple ? this.CMProvider.CurrentLevel.Load<AnimatedTexture>("Background Planes/water/" + (flag ? "water_small_splash" : "water_large_splash")) : this.CMProvider.CurrentLevel.Load<AnimatedTexture>("Background Planes/waterPink/" + (flag ? "water_small_splash" : "water_large_splash"))) : this.CMProvider.CurrentLevel.Load<AnimatedTexture>("Background Planes/sewer/" + (flag ? "sewer_small_splash" : "sewer_large_splash")))
      {
        Doublesided = true,
        Crosshatch = true
      };
      this.LevelManager.AddPlane(this.Splash);
      this.Top = (this.Plane.Position + this.Plane.Scale * this.Plane.Size / 2f).Dot(Vector3.UnitY);
      this.TerminalPosition = this.Plane.Position - this.Plane.Scale * this.Plane.Size / 2f * Vector3.UnitY + Vector3.Transform(Vector3.UnitZ, plane.Rotation) / 16f;
      foreach (BackgroundPlane backgroundPlane in this.LevelManager.BackgroundPlanes.Values.Where<BackgroundPlane>((Func<BackgroundPlane, bool>) (x =>
      {
        int? attachedPlane = x.AttachedPlane;
        int id = plane.Id;
        return (attachedPlane.GetValueOrDefault() == id ? (attachedPlane.HasValue ? 1 : 0) : 0) != 0 && FezMath.AlmostEqual(Vector3.Transform(Vector3.UnitZ, plane.Rotation).Y, 0.0f);
      })))
        this.AttachedPlanes.Add(backgroundPlane);
      Vector3 position = this.LevelManager.WaterType == LiquidType.None ? this.Top * Vector3.UnitY + this.Plane.Position * FezMath.XZMask : this.TerminalPosition * FezMath.XZMask + this.LevelManager.WaterHeight * Vector3.UnitY;
      Waiters.Wait((double) RandomHelper.Between(0.0, 1.0), (Action) (() => closure_0.BubblingEmitter = closure_0.Host.SewageFallSound.EmitAt(position, true, RandomHelper.Centered(0.025), 0.0f)));
    }

    public void Update(TimeSpan elapsed)
    {
      float num = this.LevelManager.WaterHeight - 0.5f;
      if (this.BubblingEmitter != null)
      {
        bool flag = !this.GameState.FarawaySettings.InTransition && !this.PlayerManager.Action.IsEnteringDoor();
        this.sinceAlive = FezMath.Saturate(this.sinceAlive + (float) (elapsed.TotalSeconds / 2.0 * (flag ? 1.0 : -1.0)));
      }
      if ((double) this.TerminalPosition.Y <= (double) num)
      {
        float b = this.Top - num;
        if ((double) b <= 0.0)
        {
          if (!this.Splash.Hidden)
          {
            this.ParticleSystem.Enabled = this.ParticleSystem.Visible = false;
            this.Splash.Hidden = true;
            this.Plane.Hidden = true;
            foreach (BackgroundPlane attachedPlane in this.AttachedPlanes)
              attachedPlane.Hidden = true;
          }
          if (this.BubblingEmitter == null)
            return;
          this.BubblingEmitter.VolumeFactor = 0.0f;
        }
        else
        {
          if (this.Splash.Hidden)
          {
            this.ParticleSystem.Enabled = this.ParticleSystem.Visible = true;
            this.Splash.Hidden = false;
            this.Plane.Hidden = false;
            foreach (BackgroundPlane attachedPlane in this.AttachedPlanes)
              attachedPlane.Hidden = false;
          }
          if (this.BubblingEmitter != null)
          {
            this.BubblingEmitter.VolumeFactor = FezMath.Saturate(b / 2f) * this.sinceAlive;
            if (this.LevelManager.WaterType != LiquidType.None)
              this.BubblingEmitter.Position = FezMath.XZMask * this.TerminalPosition + num * Vector3.UnitY;
          }
          this.Splash.Position = new Vector3(this.TerminalPosition.X, num + this.Splash.Size.Y / 2f, this.TerminalPosition.Z);
          if (FezMath.AlmostEqual(this.lastDistToTop, b, 1f / 16f))
            return;
          foreach (BackgroundPlane attachedPlane in this.AttachedPlanes)
          {
            attachedPlane.Scale = new Vector3(attachedPlane.Scale.X, b / attachedPlane.Size.Y, attachedPlane.Scale.Z);
            attachedPlane.Position = new Vector3(attachedPlane.Position.X, num + b / 2f, attachedPlane.Position.Z);
          }
          this.Plane.Scale = new Vector3(this.Plane.Scale.X, b / this.Plane.Size.Y, this.Plane.Scale.Z);
          this.Plane.Position = new Vector3(this.Plane.Position.X, num + b / 2f, this.Plane.Position.Z);
          this.lastDistToTop = b;
        }
      }
      else
      {
        if (this.Splash.Hidden)
          return;
        this.Splash.Hidden = true;
      }
    }

    public void Dispose()
    {
      if (this.BubblingEmitter == null || this.BubblingEmitter.Dead)
        return;
      this.BubblingEmitter.Cue.Stop();
    }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public ILevelMaterializer LevelMaterializer { private get; set; }

    [ServiceDependency]
    public IGameLevelManager LevelManager { private get; set; }

    [ServiceDependency]
    public IContentManagerProvider CMProvider { private get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }
  }
}
