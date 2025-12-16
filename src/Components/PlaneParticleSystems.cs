// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PlaneParticleSystems
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

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
using System.Threading;

#nullable disable
namespace FezGame.Components;

public class PlaneParticleSystems : DrawableGameComponent, IPlaneParticleSystems
{
  private readonly Pool<PlaneParticleSystem> PooledParticleSystems = new Pool<PlaneParticleSystem>();
  private const int LimitBeforeMultithread = 8;
  private Worker<MtUpdateContext<List<PlaneParticleSystem>>> otherThread;
  private readonly List<PlaneParticleSystem> OtherDeadParticleSystems = new List<PlaneParticleSystem>();
  private readonly List<PlaneParticleSystem> ActiveParticleSystems = new List<PlaneParticleSystem>();
  private readonly List<PlaneParticleSystem> DeadParticleSystems = new List<PlaneParticleSystem>();
  private Texture2D WhiteSquare;
  private SoundEffect liquidSplash;

  public PlaneParticleSystems(Game game)
    : base(game)
  {
    this.DrawOrder = 20;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.WhiteSquare = this.CMProvider.Global.Load<Texture2D>("Background Planes/white_square");
    this.liquidSplash = this.CMProvider.Global.Load<SoundEffect>("Sounds/Nature/WaterSplash");
    this.PooledParticleSystems.Size = 5;
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      foreach (PlaneParticleSystem activeParticleSystem in this.ActiveParticleSystems)
      {
        activeParticleSystem.Clear();
        this.PooledParticleSystems.Return(activeParticleSystem);
      }
      this.ActiveParticleSystems.Clear();
      if (this.LevelManager.Rainy)
        return;
      this.PooledParticleSystems.Size = 5;
      this.OtherDeadParticleSystems.Capacity = 50;
      this.DeadParticleSystems.Capacity = 50;
      this.ActiveParticleSystems.Capacity = 100;
      while (this.PooledParticleSystems.Available > 5)
        ServiceHelper.RemoveComponent<PlaneParticleSystem>(this.PooledParticleSystems.Take());
    });
    this.otherThread = this.ThreadPool.Take<MtUpdateContext<List<PlaneParticleSystem>>>(new Action<MtUpdateContext<List<PlaneParticleSystem>>>(this.UpdateParticleSystems));
    this.otherThread.Priority = ThreadPriority.Normal;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.InMap || this.GameState.InMenuCube)
      return;
    TimeSpan timeSpan = gameTime.ElapsedGameTime;
    if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning)
      timeSpan = TimeSpan.Zero;
    int count = this.ActiveParticleSystems.Count;
    if (count >= 8)
    {
      this.otherThread.Start(new MtUpdateContext<List<PlaneParticleSystem>>()
      {
        Elapsed = timeSpan,
        StartIndex = 0,
        EndIndex = count / 2,
        Result = this.OtherDeadParticleSystems
      });
      this.UpdateParticleSystems(new MtUpdateContext<List<PlaneParticleSystem>>()
      {
        Elapsed = timeSpan,
        StartIndex = count / 2,
        EndIndex = count,
        Result = this.DeadParticleSystems
      });
      this.otherThread.Join();
    }
    else
      this.UpdateParticleSystems(new MtUpdateContext<List<PlaneParticleSystem>>()
      {
        Elapsed = timeSpan,
        StartIndex = 0,
        EndIndex = count,
        Result = this.DeadParticleSystems
      });
    if (this.OtherDeadParticleSystems.Count > 0)
    {
      this.DeadParticleSystems.AddRange((IEnumerable<PlaneParticleSystem>) this.OtherDeadParticleSystems);
      this.OtherDeadParticleSystems.Clear();
    }
    if (this.DeadParticleSystems.Count <= 0)
      return;
    foreach (PlaneParticleSystem deadParticleSystem in this.DeadParticleSystems)
    {
      this.ActiveParticleSystems.Remove(deadParticleSystem);
      deadParticleSystem.Clear();
      this.PooledParticleSystems.Return(deadParticleSystem);
    }
    this.DeadParticleSystems.Clear();
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.StereoMode || this.GameState.InMap)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
    graphicsDevice.GetDssCombiner().StencilFunction = CompareFunction.Always;
    graphicsDevice.GetDssCombiner().DepthBufferEnable = true;
    graphicsDevice.GetDssCombiner().DepthBufferFunction = CompareFunction.LessEqual;
    graphicsDevice.GetDssCombiner().DepthBufferWriteEnable = false;
    graphicsDevice.GetRasterCombiner().CullMode = CullMode.CullCounterClockwiseFace;
    this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    bool flag = this.LevelManager.Name == "ELDERS";
    this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    for (int index = 0; index < this.ActiveParticleSystems.Count; ++index)
    {
      PlaneParticleSystem activeParticleSystem = this.ActiveParticleSystems[index];
      activeParticleSystem.InScreen = flag || this.CameraManager.Frustum.Contains(activeParticleSystem.Settings.SpawnVolume) != 0;
      if (activeParticleSystem.InScreen && activeParticleSystem.DrawOrder == 0)
        activeParticleSystem.Draw();
    }
  }

  public void ForceDraw()
  {
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
    graphicsDevice.GetDssCombiner().StencilFunction = CompareFunction.Always;
    graphicsDevice.GetDssCombiner().DepthBufferWriteEnable = false;
    graphicsDevice.GetDssCombiner().DepthBufferFunction = CompareFunction.LessEqual;
    graphicsDevice.GetRasterCombiner().CullMode = CullMode.CullCounterClockwiseFace;
    graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    for (int index = 0; index < this.ActiveParticleSystems.Count; ++index)
    {
      if (this.ActiveParticleSystems[index].InScreen)
        this.ActiveParticleSystems[index].Draw();
    }
  }

  private void UpdateParticleSystems(MtUpdateContext<List<PlaneParticleSystem>> context)
  {
    for (int startIndex = context.StartIndex; startIndex < context.EndIndex; ++startIndex)
    {
      if (startIndex < this.ActiveParticleSystems.Count)
      {
        this.ActiveParticleSystems[startIndex].Update(context.Elapsed);
        if (this.ActiveParticleSystems[startIndex].Dead)
          context.Result.Add(this.ActiveParticleSystems[startIndex]);
      }
    }
  }

  public PlaneParticleSystem RainSplash(Vector3 center)
  {
    Vector3 vector3 = center;
    PlaneParticleSystem system = this.PooledParticleSystems.Take();
    system.MaximumCount = 3;
    if (system.Settings == null)
      system.Settings = new PlaneParticleSystemSettings();
    system.Settings.NoLightDraw = true;
    system.Settings.SpawnVolume = new BoundingBox()
    {
      Min = vector3 - FezMath.XZMask * 0.15f,
      Max = vector3 + FezMath.XZMask * 0.15f
    };
    system.Settings.Velocity.Function = (Func<Vector3, Vector3, Vector3>) null;
    system.Settings.Velocity.Base = new Vector3(0.0f, 3.5f, 0.0f);
    system.Settings.Velocity.Variation = new Vector3(2f, 1.5f, 2f);
    system.Settings.Gravity = new Vector3(0.0f, -0.4f, 0.0f);
    system.Settings.SpawningSpeed = 60f;
    system.Settings.ParticleLifetime = 0.275f;
    system.Settings.SystemLifetime = 0.275f;
    system.Settings.FadeInDuration = 0.0f;
    system.Settings.FadeOutDuration = 0.5f;
    system.Settings.SpawnBatchSize = 3;
    system.Settings.SizeBirth.Function = (Func<Vector3, Vector3, Vector3>) null;
    system.Settings.SizeBirth.Variation = Vector3.Zero;
    system.Settings.SizeBirth.Base = new Vector3(1f / 16f);
    system.Settings.ColorLife.Base = new Color(145, 182, (int) byte.MaxValue, 96 /*0x60*/);
    system.Settings.ColorLife.Variation = new Color(0, 0, 0, 32 /*0x20*/);
    system.Settings.ColorLife.Function = (Func<Color, Color, Color>) null;
    system.Settings.Texture = this.WhiteSquare;
    system.Settings.BlendingMode = BlendingMode.Alphablending;
    system.Settings.Billboarding = true;
    this.Add(system);
    return system;
  }

  public void Splash(IPhysicsEntity entity, bool outwards) => this.Splash(entity, outwards, 0.0f);

  public void Splash(IPhysicsEntity entity, bool outwards, float velocityBonus)
  {
    if (this.LevelManager.WaterType == LiquidType.None)
      return;
    Vector3 vector3 = entity.Center * FezMath.XZMask + this.LevelManager.WaterHeight * Vector3.UnitY - Vector3.UnitY * 0.5f;
    float num1 = Math.Min(Math.Abs(entity.Velocity.Y) / 0.25f, 1.75f) + velocityBonus;
    this.liquidSplash.EmitAt(entity.Center, RandomHelper.Centered(0.014999999664723873), FezMath.Saturate(num1));
    if (outwards)
    {
      vector3 += entity.Velocity * FezMath.XZMask * 10f;
      num1 = 0.5f;
    }
    bool flag = this.LevelManager.WaterType == LiquidType.Lava;
    LiquidColorScheme colorScheme = LiquidHost.ColorSchemes[this.LevelManager.WaterType];
    Color color1 = flag ? colorScheme.SolidOverlay : Color.White;
    Color color2 = new Color((colorScheme.SubmergedFoam.ToVector3() + color1.ToVector3()) / 2f);
    Color color3 = new Color(color1.ToVector3() - color2.ToVector3());
    int num2 = 1;
    if (flag)
    {
      num1 /= 4f;
      num2 *= 2;
    }
    PlaneParticleSystemSettings particleSystemSettings1 = new PlaneParticleSystemSettings();
    particleSystemSettings1.SpawnVolume = new BoundingBox()
    {
      Min = vector3 - FezMath.XZMask * 0.15f,
      Max = vector3 + FezMath.XZMask * 0.15f
    };
    particleSystemSettings1.Velocity.Base = new Vector3(0.0f, 7f * num1, 0.0f);
    particleSystemSettings1.Velocity.Variation = new Vector3(2f, 2f * num1, 2f);
    particleSystemSettings1.Gravity = new Vector3(0.0f, flag ? -0.15f : -0.35f, 0.0f);
    particleSystemSettings1.SpawningSpeed = 60f;
    particleSystemSettings1.ParticleLifetime = 0.85f;
    particleSystemSettings1.SystemLifetime = 0.85f;
    particleSystemSettings1.FadeInDuration = 0.0f;
    particleSystemSettings1.FadeOutDuration = 0.5f;
    particleSystemSettings1.SpawnBatchSize = 50;
    VaryingVector3 varyingVector3 = new VaryingVector3();
    varyingVector3.Base = new Vector3(0.125f * (float) num2);
    varyingVector3.Variation = new Vector3(1f / 16f * (float) num2);
    varyingVector3.Function = VaryingVector3.Uniform;
    particleSystemSettings1.SizeBirth = varyingVector3;
    particleSystemSettings1.SizeDeath = (VaryingVector3) new Vector3(1f / 32f * (float) (num2 * num2));
    particleSystemSettings1.ColorLife.Base = color2;
    particleSystemSettings1.ColorLife.Variation = color3;
    particleSystemSettings1.ColorLife.Function = VaryingColor.Uniform;
    particleSystemSettings1.Texture = this.WhiteSquare;
    particleSystemSettings1.BlendingMode = BlendingMode.Alphablending;
    particleSystemSettings1.Billboarding = true;
    particleSystemSettings1.FullBright = this.LevelManager.WaterType == LiquidType.Sewer;
    PlaneParticleSystemSettings particleSystemSettings2 = particleSystemSettings1;
    if (!outwards)
      particleSystemSettings2.EnergySource = new Vector3?(entity.Center - entity.Velocity * new Vector3(1f, -0.5f, 1f) * 2.5f);
    PlaneParticleSystem system = this.PooledParticleSystems.Take();
    system.MaximumCount = 50;
    system.Settings = particleSystemSettings2;
    this.Add(system);
  }

  public void Add(PlaneParticleSystem system)
  {
    if (!system.Initialized)
      ServiceHelper.AddComponent((IGameComponent) system);
    if (!system.Initialized)
      system.Initialize();
    system.Revive();
    this.ActiveParticleSystems.Add(system);
  }

  public void Remove(PlaneParticleSystem system, bool returnToPool)
  {
    this.ActiveParticleSystems.Remove(system);
    if (!returnToPool)
      return;
    system.Clear();
    this.PooledParticleSystems.Return(system);
  }

  protected override void Dispose(bool disposing)
  {
    this.ThreadPool.Return<MtUpdateContext<List<PlaneParticleSystem>>>(this.otherThread);
    base.Dispose(disposing);
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { private get; set; }
}
