// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.PlaneParticleSystem
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Components;

public class PlaneParticleSystem : DrawableGameComponent
{
  private const int InstancesPerBatch = 60;
  private readonly Pool<PlaneParticleSystem.Particle> particles;
  private readonly List<PlaneParticleSystem.Particle> activeParticles = new List<PlaneParticleSystem.Particle>();
  private Mesh mesh;
  private PlaneParticleEffect effect;
  private PlaneParticleSystemSettings settings;
  private TimeSpan age;
  private TimeSpan sinceSpawned;
  private TimeSpan untilNextSpawn;
  private BoundingFrustum cachedFrustum;
  private ShaderInstancedIndexedPrimitives<VertexPositionTextureInstance, Matrix> Geometry;
  public bool InScreen;
  private bool FadingOut;
  private float FadeOutDuration;
  private float SinceFadingOut;
  private float FadeOutAge;

  public event Action<Vector3> CollisionCallback = new Action<Vector3>(Util.NullAction<Vector3>);

  public PlaneParticleSystem()
    : base(ServiceHelper.Game)
  {
    this.particles = new Pool<PlaneParticleSystem.Particle>();
  }

  public PlaneParticleSystem(Game game, int maximumCount, PlaneParticleSystemSettings settings)
    : base(game)
  {
    this.Settings = settings;
    this.particles = new Pool<PlaneParticleSystem.Particle>(maximumCount);
    this.PrepareNextSpawn();
  }

  public PlaneParticleSystemSettings Settings
  {
    get => this.settings;
    set => this.settings = value.Clone();
  }

  private void RefreshEffects()
  {
    this.mesh.Effect = (BaseEffect) (this.effect = new PlaneParticleEffect());
  }

  public bool Dead { get; private set; }

  public void Revive()
  {
    this.Dead = false;
    this.Enabled = this.Visible = true;
    this.FadingOut = false;
    this.age = this.sinceSpawned = this.untilNextSpawn = TimeSpan.Zero;
    this.PrepareNextSpawn();
    if (this.effect == null)
    {
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.effect.Additive = this.settings.BlendingMode == BlendingMode.Additive;
        this.effect.Fullbright = this.settings.FullBright;
      }));
    }
    else
    {
      this.effect.Additive = this.settings.BlendingMode == BlendingMode.Additive;
      this.effect.Fullbright = this.settings.FullBright;
    }
  }

  public void SetViewProjectionSticky(bool enabled)
  {
    this.effect.ForcedViewProjection = enabled ? new Matrix?(this.CameraManager.View * this.CameraManager.Projection) : new Matrix?();
    this.cachedFrustum = enabled ? this.CameraManager.Frustum : (BoundingFrustum) null;
  }

  public void MoveActiveParticles(Vector3 offset)
  {
    foreach (PlaneParticleSystem.Particle activeParticle in this.activeParticles)
    {
      activeParticle.Position += offset;
      activeParticle.CommitToMatrix();
    }
  }

  public void FadeOutAndDie(float forSeconds)
  {
    if (this.FadingOut)
      return;
    this.FadingOut = true;
    this.FadeOutDuration = forSeconds;
    this.SinceFadingOut = 0.0f;
  }

  public int ActiveParticles => this.activeParticles.Count;

  public int DrawnParticles => this.Geometry.InstanceCount;

  public int MaximumCount
  {
    get => this.particles.Size;
    set
    {
      if (this.particles.Size == value)
        return;
      this.Clear();
      this.particles.Size = value;
      while (this.particles.Available > this.particles.Size)
        this.particles.Take();
      if (this.mesh == null)
        return;
      this.SetupGeometry();
    }
  }

  private void SetupGeometry()
  {
    this.mesh.ClearGroups();
    this.mesh.AddGroup().Geometry = (IIndexedPrimitiveCollection) (this.Geometry = new ShaderInstancedIndexedPrimitives<VertexPositionTextureInstance, Matrix>(PrimitiveType.TriangleList, 60));
    this.Geometry.Vertices = new VertexPositionTextureInstance[4]
    {
      new VertexPositionTextureInstance(new Vector3(-0.5f, -0.5f, 0.0f), new Vector2(0.0f, 1f)),
      new VertexPositionTextureInstance(new Vector3(-0.5f, 0.5f, 0.0f), new Vector2(0.0f, 0.0f)),
      new VertexPositionTextureInstance(new Vector3(0.5f, 0.5f, 0.0f), new Vector2(1f, 0.0f)),
      new VertexPositionTextureInstance(new Vector3(0.5f, -0.5f, 0.0f), new Vector2(1f, 1f))
    };
    this.Geometry.Indices = new int[6]{ 0, 1, 2, 0, 2, 3 };
    this.Geometry.Instances = new Matrix[this.MaximumCount];
    this.Geometry.MaximizeBuffers(this.MaximumCount);
    int size = this.particles.Size;
    List<PlaneParticleSystem.Particle> particleList = new List<PlaneParticleSystem.Particle>();
    while (this.particles.Available > 0)
    {
      PlaneParticleSystem.Particle particle = this.particles.Take();
      particle.InstanceIndex = --size;
      particleList.Add(particle);
    }
    foreach (PlaneParticleSystem.Particle particle in particleList)
      this.particles.Return(particle);
    if (this.settings.Texture == null)
      return;
    this.mesh.Texture.Set((Texture) this.settings.Texture);
  }

  public void RefreshTexture() => this.mesh.Texture.Set((Texture) this.settings.Texture);

  public void Clear()
  {
    foreach (PlaneParticleSystem.Particle activeParticle in this.activeParticles)
    {
      activeParticle.Hide();
      this.particles.Return(activeParticle);
    }
    this.activeParticles.Clear();
  }

  public bool Initialized { get; private set; }

  public override void Initialize()
  {
    base.Initialize();
    this.mesh = new Mesh() { SkipStates = true };
    BaseEffect.InstancingModeChanged += new Action(this.RefreshEffects);
    DrawActionScheduler.Schedule((Action) (() => this.mesh.Effect = (BaseEffect) (this.effect = new PlaneParticleEffect())));
    this.SetupGeometry();
    this.Initialized = true;
    this.LightingPostProcess.DrawGeometryLights += new Action<GameTime>(this.DrawLights);
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.LightingPostProcess != null)
      this.LightingPostProcess.DrawGeometryLights -= new Action<GameTime>(this.DrawLights);
    if (this.Geometry != null)
    {
      this.Geometry.Dispose();
      this.Geometry = (ShaderInstancedIndexedPrimitives<VertexPositionTextureInstance, Matrix>) null;
    }
    if (this.mesh != null)
    {
      this.mesh.Dispose();
      this.mesh = (Mesh) null;
    }
    this.effect = (PlaneParticleEffect) null;
    this.Initialized = false;
    BaseEffect.InstancingModeChanged -= new Action(this.RefreshEffects);
  }

  public bool HalfUpdate { get; set; }

  private float BillboardingPhi { get; set; }

  public void Update(TimeSpan elapsed)
  {
    if (!this.Enabled)
      return;
    BoundingFrustum boundingFrustum = this.CameraManager.Frustum;
    if (this.effect != null)
    {
      if (!this.effect.ForcedViewProjection.HasValue)
      {
        Vector3 position = this.CameraManager.Position;
        Vector3 center = this.CameraManager.Center;
        this.BillboardingPhi = (float) Math.Atan2((double) position.X - (double) center.X, (double) position.Z - (double) center.Z);
      }
      else
        boundingFrustum = this.cachedFrustum;
    }
    if ((double) this.settings.SystemLifetime != 0.0)
    {
      this.age += elapsed;
      if (this.age.TotalSeconds > (double) this.settings.SystemLifetime)
      {
        this.Enabled = false;
        this.Visible = false;
        this.Dead = true;
        foreach (PlaneParticleSystem.Particle activeParticle in this.activeParticles)
          this.particles.Return(activeParticle);
        this.activeParticles.Clear();
        return;
      }
    }
    if (this.FadingOut)
    {
      this.SinceFadingOut += (float) elapsed.TotalSeconds;
      this.FadeOutAge = FezMath.Saturate(this.SinceFadingOut / this.FadeOutDuration);
      if ((double) this.FadeOutAge >= 1.0)
      {
        this.Enabled = false;
        this.Visible = false;
        this.Dead = true;
        foreach (PlaneParticleSystem.Particle activeParticle in this.activeParticles)
          this.particles.Return(activeParticle);
        this.activeParticles.Clear();
        return;
      }
    }
    if (!this.InScreen)
      return;
    this.sinceSpawned -= elapsed;
    bool flag = this.untilNextSpawn.Ticks > 0L;
    this.untilNextSpawn -= elapsed;
    while (this.sinceSpawned.Ticks <= 0L || flag && this.untilNextSpawn.Ticks <= 0L)
    {
      if (flag && this.untilNextSpawn.Ticks <= 0L)
      {
        for (int index = 0; index < this.settings.SpawnBatchSize; ++index)
        {
          if (this.particles.Available > 0)
          {
            PlaneParticleSystem.Particle particle = this.particles.Take();
            particle.Initialize(this);
            this.activeParticles.Add(particle);
          }
        }
        flag = false;
      }
      if (this.sinceSpawned.Ticks <= 0L)
        this.PrepareNextSpawn();
    }
    int val1 = -1;
    float totalSeconds = (float) elapsed.TotalSeconds;
    int num = !this.HalfUpdate ? this.activeParticles.Count : this.activeParticles.Count / 2;
    for (int index = 0; index < num; ++index)
    {
      PlaneParticleSystem.Particle activeParticle = this.activeParticles[index];
      activeParticle.Update(totalSeconds);
      if (this.Settings.UseCallback && !activeParticle.DeathProvoked)
      {
        Vector3 position = activeParticle.Position;
        if ((double) this.Geometry.Instances[activeParticle.InstanceIndex].M12 < (double) this.LevelManager.WaterHeight)
        {
          this.CollisionCallback(position);
          activeParticle.ProvokeDeath();
          continue;
        }
        TrileEmplacement id = new TrileEmplacement((int) position.X, (int) position.Y, (int) position.Z);
        if (this.LevelManager.IsInRange(ref id))
        {
          TrileInstance trileInstance = this.LevelManager.TrileInstanceAt(ref id);
          if (trileInstance != null && trileInstance.Enabled && !trileInstance.Trile.Immaterial && boundingFrustum.Contains(trileInstance.Center) != ContainmentType.Disjoint)
          {
            Vector3 transformedSize = trileInstance.TransformedSize;
            Vector3 center = trileInstance.Center;
            if (new BoundingBox(center - transformedSize / 2f, center + transformedSize / 2f).Contains(position) != ContainmentType.Disjoint)
            {
              this.CollisionCallback(position * FezMath.XZMask + trileInstance.Center * Vector3.UnitY + trileInstance.Trile.Size.Y / 2f * Vector3.UnitY);
              activeParticle.ProvokeDeath();
            }
          }
        }
      }
      if (activeParticle.Dead)
      {
        activeParticle.Hide();
        this.activeParticles.RemoveAt(index);
        --index;
        --num;
        this.particles.Return(activeParticle);
      }
      else
        val1 = Math.Max(val1, activeParticle.InstanceIndex);
    }
    this.Geometry.InstanceCount = val1;
  }

  private void PrepareNextSpawn()
  {
    float max = 1f / this.settings.SpawningSpeed;
    TimeSpan timeSpan = TimeSpan.FromSeconds(this.settings.RandomizeSpawnTime ? (double) RandomHelper.Between(0.0, (double) max) : (double) max);
    if (this.untilNextSpawn < this.sinceSpawned)
      this.untilNextSpawn = this.sinceSpawned;
    this.untilNextSpawn += timeSpan;
    this.sinceSpawned += TimeSpan.FromSeconds((double) max);
  }

  private void DrawLights(GameTime gameTime)
  {
    if (!this.Visible || this.Settings.NoLightDraw || !this.InScreen)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.GetRasterCombiner().CullMode = CullMode.CullCounterClockwiseFace;
    graphicsDevice.GetDssCombiner().DepthBufferFunction = CompareFunction.LessEqual;
    bool bufferWriteEnable = graphicsDevice.GetDssCombiner().DepthBufferWriteEnable;
    StencilOperation stencilPass = graphicsDevice.GetDssCombiner().StencilPass;
    if (this.settings.BlendingMode == BlendingMode.Additive)
    {
      graphicsDevice.GetDssCombiner().DepthBufferWriteEnable = false;
      graphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
      graphicsDevice.GetBlendCombiner().BlendingMode = BlendingMode.Additive;
    }
    else
      graphicsDevice.GetBlendCombiner().BlendingMode = BlendingMode.Alphablending;
    this.effect.Pass = LightingEffectPass.Pre;
    this.mesh.Draw();
    this.effect.Pass = LightingEffectPass.Main;
    if (this.settings.BlendingMode != BlendingMode.Additive)
      return;
    graphicsDevice.GetDssCombiner().DepthBufferWriteEnable = bufferWriteEnable;
    graphicsDevice.GetDssCombiner().StencilPass = stencilPass;
    graphicsDevice.GetBlendCombiner().BlendingMode = BlendingMode.Alphablending;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.DrawOrder == 0 || this.EngineState.Loading || this.EngineState.StereoMode || this.EngineState.InMap)
      return;
    this.InScreen = this.CameraManager.Frustum.Contains(this.Settings.SpawnVolume) != 0;
    if (!this.InScreen)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    FezEngine.Structure.StencilMask? stencilMask = this.Settings.StencilMask;
    if (stencilMask.HasValue)
    {
      graphicsDevice.PrepareStencilWrite(this.Settings.StencilMask);
    }
    else
    {
      graphicsDevice.GetDssCombiner().StencilFunction = CompareFunction.Always;
      graphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
    }
    graphicsDevice.GetDssCombiner().DepthBufferWriteEnable = false;
    graphicsDevice.GetRasterCombiner().CullMode = CullMode.CullCounterClockwiseFace;
    graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    this.Draw();
    stencilMask = this.Settings.StencilMask;
    if (!stencilMask.HasValue)
      return;
    graphicsDevice.GetDssCombiner().StencilFunction = CompareFunction.Always;
    graphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
  }

  public void Draw()
  {
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    if (this.settings.Doublesided)
      graphicsDevice.GetRasterCombiner().CullMode = CullMode.None;
    if (this.Settings.BlendingMode != BlendingMode.Alphablending)
      graphicsDevice.SetBlendingMode(this.Settings.BlendingMode);
    this.mesh.Draw();
    if (this.Settings.BlendingMode != BlendingMode.Alphablending)
      graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    if (!this.Settings.Doublesided)
      return;
    graphicsDevice.GetRasterCombiner().CullMode = CullMode.CullCounterClockwiseFace;
  }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { get; set; }

  private class Particle
  {
    private readonly IDefaultCameraManager Camera;
    public Vector3 Position;
    private PlaneParticleSystem System;
    private PlaneParticleSystemSettings Settings;
    private Vector4 SpawnColor;
    private Vector4 LifeColor;
    private Vector4 DieColor;
    private float AgeSeconds;
    private Vector3 Velocity;
    private Vector3 GravityEffect;
    private Vector3 SizeBirth;
    private Vector3 SizeDeath;
    private Vector3 Scale;
    private float Phi;

    public int InstanceIndex { get; set; }

    public Vector3 TotalVelocity => this.Velocity + this.GravityEffect;

    public bool DeathProvoked { get; private set; }

    public Particle() => this.Camera = ServiceHelper.Get<IDefaultCameraManager>();

    public void Initialize(PlaneParticleSystem system)
    {
      this.System = system;
      this.Settings = system.Settings;
      this.LifeColor = this.Settings.ColorLife.Evaluate().ToVector4();
      this.SpawnColor = this.Settings.ColorBirth == null ? new Vector4(this.LifeColor.XYZ(), 0.0f) : this.Settings.ColorBirth.Evaluate().ToVector4();
      this.DieColor = this.Settings.ColorDeath == null ? new Vector4(this.LifeColor.XYZ(), 0.0f) : this.Settings.ColorDeath.Evaluate().ToVector4();
      this.Scale = this.SizeBirth = this.Settings.SizeBirth.Evaluate();
      this.SizeDeath = this.Settings.SizeDeath.Base == new Vector3(-1f) ? this.SizeBirth : this.Settings.SizeDeath.Evaluate();
      this.Position = new Vector3(RandomHelper.Between((double) this.Settings.SpawnVolume.Min.X, (double) this.Settings.SpawnVolume.Max.X), RandomHelper.Between((double) this.Settings.SpawnVolume.Min.Y, (double) this.Settings.SpawnVolume.Max.Y), RandomHelper.Between((double) this.Settings.SpawnVolume.Min.Z, (double) this.Settings.SpawnVolume.Max.Z));
      Vector3? energySource = this.Settings.EnergySource;
      if (energySource.HasValue)
      {
        float num = this.Settings.Velocity.Evaluate().Length();
        Vector3 position = this.Position;
        energySource = this.Settings.EnergySource;
        Vector3 vector3 = energySource.Value;
        this.Velocity = Vector3.Normalize(position - vector3) * num;
      }
      else
        this.Velocity = this.Settings.Velocity.Evaluate();
      this.GravityEffect = Vector3.Zero;
      this.AgeSeconds = 0.0f;
      this.DeathProvoked = false;
      this.Phi = this.Settings.Orientation.HasValue ? this.Settings.Orientation.Value.ToPhi() : this.Camera.Viewpoint.ToPhi();
      this.System.Geometry.Instances[this.InstanceIndex] = new Matrix(this.Position.X, this.Position.Y, this.Position.Z, this.Phi, this.SizeBirth.X, this.SizeBirth.Y, this.SizeBirth.Z, 0.0f, this.SpawnColor.X, this.SpawnColor.Y, this.SpawnColor.Z, this.SpawnColor.W, 0.0f, 0.0f, 0.0f, 0.0f);
      this.System.Geometry.InstancesDirty = true;
    }

    public void Update(float elapsedSeconds)
    {
      this.AgeSeconds += elapsedSeconds;
      float amount = this.AgeSeconds / this.Settings.ParticleLifetime;
      if ((double) elapsedSeconds != 0.0)
        this.GravityEffect += this.Settings.Gravity;
      if ((double) this.Settings.Acceleration != 0.0)
      {
        this.Velocity.X = FezMath.DoubleIter(this.Velocity.X, elapsedSeconds, 1f / this.Settings.Acceleration);
        this.Velocity.Y = FezMath.DoubleIter(this.Velocity.Y, elapsedSeconds, 1f / this.Settings.Acceleration);
        this.Velocity.Z = FezMath.DoubleIter(this.Velocity.Z, elapsedSeconds, 1f / this.Settings.Acceleration);
      }
      this.Position += (this.Velocity + this.GravityEffect) * elapsedSeconds;
      Vector4 vector4 = (double) amount >= (double) this.Settings.FadeInDuration ? ((double) amount <= 1.0 - (double) this.Settings.FadeOutDuration ? this.LifeColor : Vector4.Lerp(this.LifeColor, this.DieColor, (amount - (1f - this.Settings.FadeOutDuration)) / this.Settings.FadeOutDuration)) : Vector4.Lerp(this.SpawnColor, this.LifeColor, amount / this.Settings.FadeInDuration);
      if (this.System.FadingOut)
        vector4 = Vector4.Lerp(vector4, this.DieColor, this.System.FadeOutAge);
      if (this.Settings.Billboarding)
        this.Phi = this.System.BillboardingPhi;
      if (this.SizeBirth != this.SizeDeath)
        this.Scale = Vector3.Lerp(this.SizeBirth, this.SizeDeath, amount);
      Vector3 vector3 = this.Position;
      if (this.Settings.ClampToTrixels)
        vector3 = (vector3 * 16f).Round() / 16f + this.Scale / 2f;
      this.System.Geometry.Instances[this.InstanceIndex] = new Matrix(vector3.X, vector3.Y, vector3.Z, this.Phi, this.Scale.X, this.Scale.Y, this.Scale.Z, 0.0f, vector4.X, vector4.Y, vector4.Z, vector4.W, 0.0f, 0.0f, 0.0f, 0.0f);
      this.System.Geometry.InstancesDirty = true;
    }

    public void CommitToMatrix()
    {
      Vector3 vector3 = this.Position;
      if (this.Settings.ClampToTrixels)
        vector3 = (vector3 * 16f).Round() / 16f + this.Scale / 2f;
      this.System.Geometry.Instances[this.InstanceIndex] = new Matrix(vector3.X, vector3.Y, vector3.Z, this.Phi, this.SizeBirth.X, this.SizeBirth.Y, this.SizeBirth.Z, 0.0f, this.SpawnColor.X, this.SpawnColor.Y, this.SpawnColor.Z, this.SpawnColor.W, 0.0f, 0.0f, 0.0f, 0.0f);
      this.System.Geometry.InstancesDirty = true;
    }

    public void Hide()
    {
      this.System.Geometry.Instances[this.InstanceIndex] = new Matrix();
      this.System.Geometry.InstancesDirty = true;
    }

    public bool Dead => (double) this.AgeSeconds > (double) this.Settings.ParticleLifetime;

    public void ProvokeDeath()
    {
      this.AgeSeconds = this.Settings.ParticleLifetime + float.Epsilon;
      this.DeathProvoked = true;
    }
  }
}
