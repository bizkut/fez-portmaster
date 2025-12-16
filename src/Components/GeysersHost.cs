// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GeysersHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class GeysersHost : GameComponent
{
  private readonly List<GeysersHost.GeyserState> Geysers = new List<GeysersHost.GeyserState>();
  private SoundEffect LoopSound;

  public GeysersHost(Game game)
    : base(game)
  {
    this.UpdateOrder = -2;
  }

  public override void Initialize()
  {
    this.LoopSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Sewer/GeyserLoop");
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
  }

  private void TryInitialize()
  {
    this.Geysers.Clear();
    foreach (TrileGroup group in this.LevelManager.Groups.Values.Where<TrileGroup>((Func<TrileGroup, bool>) (x => x.ActorType == ActorType.Geyser)))
      this.Geysers.Add(new GeysersHost.GeyserState(group, this));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ViewTransitionReached)
      return;
    foreach (GeysersHost.GeyserState geyser in this.Geysers)
      geyser.Update(gameTime.ElapsedGameTime);
  }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private class GeyserState
  {
    private const float GeyserFallSpeed = 5f;
    private readonly GeysersHost Host;
    private readonly TrileGroup Group;
    private readonly BackgroundPlane TilePlane;
    private readonly BackgroundPlane TopPlane;
    private TimeSpan SinceStateChange;
    private TimeSpan SinceStartedLift;
    private bool Lifting;
    private bool ReachedApex;
    private float ReachedAtTime;
    private float InitialHeight;
    private SoundEmitter loopEmitter;
    private float heightDelta;

    public GeyserState(TrileGroup group, GeysersHost host)
    {
      ServiceHelper.InjectServices((object) this);
      this.Host = host;
      this.Group = group;
      this.SinceStateChange = TimeSpan.FromSeconds(-(double) group.GeyserOffset);
      Vector3 position = this.Group.Triles.Aggregate<TrileInstance, Vector3>(Vector3.Zero, (Func<Vector3, TrileInstance, Vector3>) ((a, b) => a + b.Center)) / (float) group.Triles.Count;
      this.TopPlane = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, "sewer/sewer_geyser_top", true)
      {
        Position = position,
        ClampTexture = true,
        Crosshatch = true,
        Doublesided = true
      };
      this.LevelManager.AddPlane(this.TopPlane);
      this.TilePlane = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, "sewer/sewer_geyser_tile", true)
      {
        Position = position,
        YTextureRepeat = true,
        Billboard = true
      };
      this.LevelManager.AddPlane(this.TilePlane);
      this.TopPlane.Timing.Step = this.TilePlane.Timing.Step;
      this.loopEmitter = this.Host.LoopSound.EmitAt(position, true, 0.0f, 0.0f);
      foreach (TrileInstance trile in group.Triles)
      {
        trile.PhysicsState.IgnoreCollision = true;
        trile.PhysicsState.IgnoreClampToWater = true;
      }
    }

    public void Update(TimeSpan elapsed)
    {
      this.SinceStateChange += elapsed;
      if (this.SinceStateChange.Ticks >= 0L)
      {
        this.Lifting = !this.Lifting;
        if (this.Lifting)
        {
          foreach (TrileInstance trile in this.Group.Triles)
          {
            trile.PhysicsState.Floating = false;
            if (!trile.PhysicsState.Puppet)
              this.InitialHeight = trile.PhysicsState.Center.Y;
          }
          this.ReachedApex = false;
          this.SinceStartedLift = TimeSpan.Zero;
          this.SinceStateChange = TimeSpan.FromSeconds(-(double) this.Group.GeyserLiftFor);
        }
        else
        {
          foreach (TrileInstance trile in this.Group.Triles)
            trile.PhysicsState.Floating = false;
          this.SinceStateChange = TimeSpan.FromSeconds(-(double) this.Group.GeyserPauseFor);
          IWaiter waiter = Waiters.Interpolate((double) this.Group.GeyserApexHeight / 2.0, (Action<float>) (s => this.heightDelta -= s * 1.25f));
          waiter.AutoPause = true;
          waiter.CustomPause = (Func<bool>) (() => !this.CameraManager.ViewTransitionReached);
        }
      }
      if (this.Lifting)
      {
        this.SinceStartedLift += elapsed;
        double linearStep = this.SinceStartedLift.TotalSeconds / Math.Sqrt((double) this.Group.GeyserApexHeight) * 1.5;
        float num = Easing.EaseInOut(linearStep, EasingType.Quadratic, EasingType.Sine);
        this.loopEmitter.VolumeFactor = (float) FezMath.Saturate(linearStep);
        if (!this.ReachedApex && linearStep >= 1.0)
        {
          this.ReachedApex = true;
          this.ReachedAtTime = (float) this.SinceStateChange.TotalSeconds;
        }
        this.heightDelta = !this.ReachedApex ? this.Group.GeyserApexHeight * num : this.Group.GeyserApexHeight + (float) (Math.Sin((this.SinceStateChange.TotalSeconds - (double) this.ReachedAtTime) * 4.0) * 1.5 / 16.0);
        foreach (TrileInstance trile in this.Group.Triles)
        {
          trile.PhysicsState.PushedUp = true;
          if (this.ReachedApex)
          {
            Vector3 center = trile.PhysicsState.Center;
            trile.PhysicsState.Center = trile.PhysicsState.Center * FezMath.XZMask + (this.InitialHeight + this.heightDelta) * Vector3.UnitY;
            trile.PhysicsState.Velocity = trile.PhysicsState.Center - center;
            trile.PhysicsState.Center = center;
          }
          else
          {
            Vector3 center = trile.PhysicsState.Center;
            trile.PhysicsState.Center = trile.PhysicsState.Center * FezMath.XZMask + (this.InitialHeight + this.heightDelta) * Vector3.UnitY;
            trile.PhysicsState.Velocity = trile.PhysicsState.Center - center;
            trile.PhysicsState.Center = center;
          }
          trile.PhysicsState.Velocity += 0.472500026f * (float) elapsed.TotalSeconds * Vector3.Up;
        }
      }
      else
      {
        foreach (TrileInstance trile in this.Group.Triles)
        {
          trile.PhysicsState.PushedUp = false;
          if (!trile.PhysicsState.Floating && (double) trile.PhysicsState.Center.Y - (double) this.LevelManager.WaterHeight > 0.0)
            trile.PhysicsState.Velocity += 0.472500026f * (float) elapsed.TotalSeconds * Vector3.Up / 2f;
        }
        this.loopEmitter.VolumeFactor = this.heightDelta / this.Group.GeyserApexHeight;
      }
      this.TopPlane.Timing.Step = this.TilePlane.Timing.Step;
      float num1 = (float) ((double) this.heightDelta + (double) this.InitialHeight - 1.0) - this.LevelManager.WaterHeight;
      this.TopPlane.Scale = new Vector3(1f, FezMath.Saturate(num1 + 1f), 1f);
      this.TopPlane.Position = FezMath.XZMask * this.TopPlane.Position + (float) ((double) this.LevelManager.WaterHeight + (double) num1 + (1.0 - (double) this.TopPlane.Scale.Y) / 2.0) * Vector3.UnitY;
      this.TopPlane.Visible = (double) this.TopPlane.Scale.Y > 0.0;
      if ((double) num1 <= 0.0)
        num1 = 0.0f;
      this.TilePlane.Scale = new Vector3(1f, num1 / this.TilePlane.Size.Y, 1f);
      this.TilePlane.Position = FezMath.XZMask * this.TilePlane.Position + (float) ((double) this.LevelManager.WaterHeight + (double) num1 / 2.0 - 0.5) * Vector3.UnitY;
      this.TilePlane.Visible = (double) this.TilePlane.Scale.Y > 0.0;
      this.loopEmitter.Position = this.TopPlane.Position;
    }

    [ServiceDependency]
    public ILevelMaterializer LevelMaterializer { private get; set; }

    [ServiceDependency]
    public IDefaultCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public IGameLevelManager LevelManager { private get; set; }
  }
}
