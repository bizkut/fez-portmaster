// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TrixelParticleSystem
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class TrixelParticleSystem : DrawableGameComponent
{
  private readonly IPhysicsManager PhysicsManager;
  private readonly IGameCameraManager CameraManager;
  private readonly IGameStateManager GameState;
  private readonly ILightingPostProcess LightingPostProcess;
  private readonly ICollisionManager CollisionManager;
  private const int InstancesPerBatch = 60;
  private const int FadeOutStartSeconds = 4;
  private const int LifetimeSeconds = 6;
  private const float VelocityFactor = 0.2f;
  private const float EnergyDecay = 1.5f;
  private readonly List<TrixelParticleSystem.Particle> particles = new List<TrixelParticleSystem.Particle>();
  private readonly TrixelParticleSystem.Settings settings;
  private static readonly object StaticLock = new object();
  private static volatile TrixelParticleEffect effect;
  private Mesh mesh;
  private TimeSpan age;
  private float Opacity = 1f;
  private ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix> Geometry;

  public Vector3 Offset { get; set; }

  public TrixelParticleSystem(Game game, TrixelParticleSystem.Settings settings)
    : base(game)
  {
    this.settings = settings;
    this.DrawOrder = 10;
    this.PhysicsManager = ServiceHelper.Get<IPhysicsManager>();
    this.CameraManager = ServiceHelper.Get<IGameCameraManager>();
    this.GameState = ServiceHelper.Get<IGameStateManager>();
    this.LightingPostProcess = ServiceHelper.Get<ILightingPostProcess>();
    this.CollisionManager = ServiceHelper.Get<ICollisionManager>();
  }

  public int ActiveParticles
  {
    get
    {
      return this.particles.Count<TrixelParticleSystem.Particle>((Func<TrixelParticleSystem.Particle, bool>) (x => x.Enabled && !x.Static));
    }
  }

  public bool Dead { get; private set; }

  public override void Initialize()
  {
    base.Initialize();
    this.SetupGeometry();
    this.SetupInstances();
    this.LightingPostProcess.DrawGeometryLights += new Action<GameTime>(this.PreDraw);
  }

  private void RefreshEffects()
  {
    this.mesh.Effect = (BaseEffect) (TrixelParticleSystem.effect = new TrixelParticleEffect());
  }

  private void SetupGeometry()
  {
    lock (TrixelParticleSystem.StaticLock)
    {
      if (TrixelParticleSystem.effect == null)
        TrixelParticleSystem.effect = new TrixelParticleEffect();
    }
    BaseEffect.InstancingModeChanged += new Action(this.RefreshEffects);
    this.mesh = new Mesh()
    {
      Effect = (BaseEffect) TrixelParticleSystem.effect,
      SkipStates = true
    };
    this.mesh.AddGroup().Geometry = (IIndexedPrimitiveCollection) (this.Geometry = new ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix>(PrimitiveType.TriangleList, 60));
    Vector3 vector3 = new Vector3(0.0f);
    ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix> geometry = this.Geometry;
    VertexPositionNormalTextureInstance[] normalTextureInstanceArray = new VertexPositionNormalTextureInstance[24];
    VertexPositionNormalTextureInstance normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, -1f, -1f) * 0.5f + vector3, -Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 1f);
    normalTextureInstanceArray[0] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, 1f, -1f) * 0.5f + vector3, -Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 0.0f);
    normalTextureInstanceArray[1] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, 1f, -1f) * 0.5f + vector3, -Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 0.0f);
    normalTextureInstanceArray[2] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, -1f, -1f) * 0.5f + vector3, -Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 1f);
    normalTextureInstanceArray[3] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, -1f, -1f) * 0.5f + vector3, Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 1f);
    normalTextureInstanceArray[4] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, 1f, -1f) * 0.5f + vector3, Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 0.0f);
    normalTextureInstanceArray[5] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, 1f, 1f) * 0.5f + vector3, Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 0.0f);
    normalTextureInstanceArray[6] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, -1f, 1f) * 0.5f + vector3, Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 1f);
    normalTextureInstanceArray[7] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, -1f, 1f) * 0.5f + vector3, Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 1f);
    normalTextureInstanceArray[8] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, 1f, 1f) * 0.5f + vector3, Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 0.0f);
    normalTextureInstanceArray[9] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, 1f, 1f) * 0.5f + vector3, Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 0.0f);
    normalTextureInstanceArray[10] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, -1f, 1f) * 0.5f + vector3, Vector3.UnitZ);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 1f);
    normalTextureInstanceArray[11] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, -1f, 1f) * 0.5f + vector3, -Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 1f);
    normalTextureInstanceArray[12] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, 1f, 1f) * 0.5f + vector3, -Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 0.0f);
    normalTextureInstanceArray[13] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, 1f, -1f) * 0.5f + vector3, -Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 0.0f);
    normalTextureInstanceArray[14] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, -1f, -1f) * 0.5f + vector3, -Vector3.UnitX);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 1f);
    normalTextureInstanceArray[15] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, -1f, -1f) * 0.5f + vector3, -Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 1f);
    normalTextureInstanceArray[16 /*0x10*/] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, -1f, 1f) * 0.5f + vector3, -Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 0.0f);
    normalTextureInstanceArray[17] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, -1f, 1f) * 0.5f + vector3, -Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 0.0f);
    normalTextureInstanceArray[18] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, -1f, -1f) * 0.5f + vector3, -Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 1f);
    normalTextureInstanceArray[19] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, 1f, -1f) * 0.5f + vector3, Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 1f);
    normalTextureInstanceArray[20] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(-1f, 1f, 1f) * 0.5f + vector3, Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.125f, 0.0f);
    normalTextureInstanceArray[21] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, 1f, 1f) * 0.5f + vector3, Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 0.0f);
    normalTextureInstanceArray[22] = normalTextureInstance;
    normalTextureInstance = new VertexPositionNormalTextureInstance(new Vector3(1f, 1f, -1f) * 0.5f + vector3, Vector3.UnitY);
    normalTextureInstance.TextureCoordinate = new Vector2(0.0f, 1f);
    normalTextureInstanceArray[23] = normalTextureInstance;
    geometry.Vertices = normalTextureInstanceArray;
    this.Geometry.Indices = new int[36]
    {
      0,
      2,
      1,
      0,
      3,
      2,
      4,
      6,
      5,
      4,
      7,
      6,
      8,
      10,
      9,
      8,
      11,
      10,
      12,
      14,
      13,
      12,
      15,
      14,
      16 /*0x10*/,
      17,
      18,
      16 /*0x10*/,
      18,
      19,
      20,
      22,
      21,
      20,
      23,
      22
    };
    this.Geometry.Instances = new Matrix[this.settings.ParticleCount];
    this.Geometry.MaximizeBuffers(this.settings.ParticleCount);
    this.Geometry.InstanceCount = this.settings.ParticleCount;
    this.mesh.Texture.Set((Texture) this.settings.ExplodingInstance.Trile.TrileSet.TextureAtlas);
  }

  private void SetupInstances()
  {
    float num1 = this.settings.ExplodingInstance.Trile.ActorSettings.Type == ActorType.Vase ? 0.05f : 0.15f;
    int maxValue = 16 /*0x10*/ - this.settings.MinimumSize;
    Vector3 b1 = this.CameraManager.Viewpoint.SideMask();
    Vector3 b2 = this.CameraManager.Viewpoint.DepthMask();
    bool flag1 = (double) b1.X != 0.0;
    bool flag2 = this.CameraManager.Viewpoint == Viewpoint.Front || this.CameraManager.Viewpoint == Viewpoint.Left;
    Random random = RandomHelper.Random;
    Vector3 a1 = this.settings.EnergySource.Value;
    Vector3 vector3_1 = new Vector3(a1.Dot(b1), a1.Y, 0.0f);
    Vector3 vector3_2 = this.settings.ExplodingInstance.Center.Dot(b2) * b2;
    Vector3 vector3_3 = a1 - a1.Dot(b2) * b2;
    Vector2 vector2_1 = new Vector2((float) this.settings.ExplodingInstance.Trile.TrileSet.TextureAtlas.Width, (float) this.settings.ExplodingInstance.Trile.TrileSet.TextureAtlas.Height);
    Vector2 vector2_2 = new Vector2(128f / vector2_1.X, 16f / vector2_1.Y);
    Vector2 atlasOffset = this.settings.ExplodingInstance.Trile.AtlasOffset;
    List<SpaceDivider.DividedCell> dividedCellList = (List<SpaceDivider.DividedCell>) null;
    if (this.settings.Crumble)
      dividedCellList = SpaceDivider.Split(this.settings.ParticleCount);
    for (int index = 0; index < this.settings.ParticleCount; ++index)
    {
      TrixelParticleSystem.Particle particle = new TrixelParticleSystem.Particle(this, index)
      {
        Elasticity = num1
      };
      Vector3 vector3_4;
      Vector3 vector1;
      if (this.settings.Crumble)
      {
        SpaceDivider.DividedCell dividedCell = dividedCellList[index];
        vector3_4 = ((float) (dividedCell.Left - 8) * b1 + (float) (dividedCell.Bottom - 8) * Vector3.UnitY + (float) (dividedCell.Left - 8) * b2) / 16f;
        vector1 = ((float) dividedCell.Width * (b1 + b2) + (float) dividedCell.Height * Vector3.UnitY) / 16f;
      }
      else
      {
        vector3_4 = new Vector3((float) random.Next(0, maxValue), (float) random.Next(0, maxValue), (float) random.Next(0, maxValue));
        do
        {
          vector1 = new Vector3((float) random.Next(this.settings.MinimumSize, Math.Min(17 - (int) vector3_4.X, this.settings.MaximumSize)), (float) random.Next(this.settings.MinimumSize, Math.Min(17 - (int) vector3_4.Y, this.settings.MaximumSize)), (float) random.Next(this.settings.MinimumSize, Math.Min(17 - (int) vector3_4.Z, this.settings.MaximumSize)));
        }
        while ((double) Math.Abs(vector1.X - vector1.Y) > ((double) vector1.X + (double) vector1.Y) / 2.0 || (double) Math.Abs(vector1.Z - vector1.Y) > ((double) vector1.Z + (double) vector1.Y) / 2.0);
        vector3_4 = (vector3_4 - new Vector3(8f)) / 16f;
        vector1 /= 16f;
      }
      particle.Size = vector1;
      float num2 = flag1 ? vector1.X : vector1.Z;
      particle.TextureMatrix = new Vector4(num2 * vector2_2.X, vector1.Y * vector2_2.Y, (float) (((flag2 ? 1.0 : -1.0) * (flag1 ? (double) vector3_4.X : (double) vector3_4.Z) + (flag2 ? 0.0 : -(double) num2) + 0.5 + 1.0 / 16.0) / 8.0) * vector2_2.X + atlasOffset.X, (float) (-((double) vector3_4.Y + (double) vector1.Y) + 0.5 + 1.0 / 16.0) * vector2_2.Y + atlasOffset.Y);
      float num3 = this.settings.Darken ? RandomHelper.Between(0.3, 1.0) : 1f;
      particle.Color = new Vector3(num3, num3, num3);
      Vector3 a2 = this.settings.ExplodingInstance.Center + vector3_4 + vector1 / 2f;
      Vector3 vector3_5 = new Vector3(a2.Dot(b1), a2.Y, 0.0f);
      Vector3 vector3_6 = a2 - vector3_2 - vector3_3;
      if (vector3_6 != Vector3.Zero)
        vector3_6.Normalize();
      if (this.settings.Crumble)
        vector3_6 = Vector3.Normalize(new Vector3(RandomHelper.Centered(1.0), RandomHelper.Centered(1.0), RandomHelper.Centered(1.0)));
      float num4 = Math.Min(1f, 1.5f - Vector3.Dot(vector1, Vector3.One));
      float num5 = (float) Math.Pow(1.0 / (1.0 + (double) (vector3_5 - vector3_1).Length()), 1.5);
      particle.Center = a2;
      particle.Velocity = vector3_6 * this.settings.Energy * num4 * 0.2f * num5 + this.settings.BaseVelocity;
      if (this.settings.Incandesce)
        particle.Incandescence = 2f;
      particle.Update();
      this.particles.Add(particle);
      if (this.settings.Crumble)
        particle.Delay = FezMath.Saturate(Easing.EaseOut((double) vector3_4.Y + 0.5, EasingType.Cubic) + RandomHelper.Centered(0.10000000149011612));
    }
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.LightingPostProcess != null)
      this.LightingPostProcess.DrawGeometryLights -= new Action<GameTime>(this.PreDraw);
    if (this.Geometry != null)
    {
      this.Geometry.Dispose();
      this.Geometry = (ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix>) null;
    }
    BaseEffect.InstancingModeChanged -= new Action(this.RefreshEffects);
  }

  public void UnGround()
  {
    foreach (TrixelParticleSystem.Particle particle in this.particles)
    {
      if (particle.Enabled)
      {
        particle.Ground = new MultipleHits<TrileInstance>();
        particle.Static = false;
      }
    }
  }

  public void AddImpulse(Vector3 energySource, float energy)
  {
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    Vector3 vector3_2 = energySource * vector3_1;
    foreach (TrixelParticleSystem.Particle particle in this.particles)
    {
      if (particle.Enabled)
      {
        Vector3 vector3_3 = particle.Center * vector3_1 - vector3_2;
        float num = (float) Math.Pow(1.0 / (1.0 + (double) vector3_3.Length()), 1.5);
        if ((double) num > 0.10000000149011612)
        {
          Vector3 vector3_4 = vector3_3 == Vector3.Zero ? Vector3.Zero : Vector3.Normalize(vector3_3);
          particle.Velocity += vector3_4 * energy * 0.2f * num;
          particle.Static = false;
        }
      }
    }
  }

  public void Update(TimeSpan elapsed)
  {
    this.age += elapsed;
    if (this.age.TotalSeconds > 6.0)
    {
      this.Dead = true;
    }
    else
    {
      Vector3 vector3 = 0.472500026f * this.CollisionManager.GravityFactor * this.settings.GravityModifier * (float) elapsed.TotalSeconds * Vector3.Down;
      float num = this.CameraManager.Radius / 2f;
      float y = this.CameraManager.Center.Y;
      float totalSeconds = (float) this.age.TotalSeconds;
      bool flag1 = (double) totalSeconds > 4.0;
      if (flag1)
        this.Opacity = FezMath.Saturate(1f - (float) (((double) totalSeconds - 4.0) / 2.0));
      foreach (TrixelParticleSystem.Particle particle in this.particles)
      {
        if (this.settings.Crumble && this.age.TotalSeconds < (double) particle.Delay)
        {
          particle.Center += this.Offset;
          particle.Update();
        }
        else if (particle.Enabled)
        {
          bool flag2 = false;
          if (!particle.Static)
          {
            if ((double) y - (double) particle.Center.Y > (double) num)
            {
              particle.Hide();
              particle.Enabled = false;
              continue;
            }
            particle.Velocity += vector3;
            particle.Incandescence *= 0.95f;
            flag2 = this.PhysicsManager.Update((ISimplePhysicsEntity) particle, true, false);
            particle.Static = !flag2 && particle.Grounded && particle.StaticGrounds;
          }
          if (flag2 | flag1)
            particle.Update();
        }
      }
    }
  }

  private void PreDraw(GameTime gameTime)
  {
    if (this.GameState.Loading || !this.Visible)
      return;
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    TrixelParticleSystem.effect.Pass = LightingEffectPass.Pre;
    this.mesh.Draw();
    TrixelParticleSystem.effect.Pass = LightingEffectPass.Main;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.StereoMode)
      return;
    this.DoDraw();
  }

  public void DoDraw()
  {
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    if ((double) this.Opacity == 1.0)
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    this.mesh.Draw();
    if ((double) this.Opacity != 1.0)
      return;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  private class Particle : ISimplePhysicsEntity, IPhysicsEntity
  {
    private readonly int InstanceIndex;
    private readonly TrixelParticleSystem System;
    public bool Enabled;
    public bool Static;
    public float Incandescence;
    public Vector3 Color;
    public Vector4 TextureMatrix;

    public float Delay { get; set; }

    public bool NoVelocityClamping => false;

    public bool IgnoreCollision => false;

    public MultipleHits<TrileInstance> Ground { get; set; }

    public Vector3 Center { get; set; }

    public Vector3 Velocity { get; set; }

    public Vector3 GroundMovement { get; set; }

    public float Elasticity { get; set; }

    public bool Background { get; set; }

    public PointCollision[] CornerCollision { get; private set; }

    public MultipleHits<CollisionResult> WallCollision { get; set; }

    public Vector3 Size { get; set; }

    public bool Grounded => this.Ground.NearLow != null || this.Ground.FarHigh != null;

    public bool Sliding
    {
      get
      {
        return !FezMath.AlmostEqual(this.Velocity.X, 0.0f) || !FezMath.AlmostEqual(this.Velocity.Z, 0.0f);
      }
    }

    public Particle(TrixelParticleSystem system, int instanceIndex)
    {
      this.CornerCollision = new PointCollision[1];
      this.Enabled = true;
      this.InstanceIndex = instanceIndex;
      this.System = system;
    }

    public void Update()
    {
      this.System.Geometry.Instances[this.InstanceIndex] = new Matrix(this.Center.X, this.Center.Y, this.Center.Z, 0.0f, this.Size.X, this.Size.Y, this.Size.Z, 0.0f, this.Color.X * (1f + this.Incandescence), this.Color.Y * (1f + this.Incandescence), this.Color.Z * (1f + this.Incandescence), this.System.Opacity, this.TextureMatrix.X, this.TextureMatrix.Y, this.TextureMatrix.Z, this.TextureMatrix.W);
      this.System.Geometry.InstancesDirty = true;
    }

    public void Hide()
    {
      this.System.Geometry.Instances[this.InstanceIndex] = new Matrix();
      this.System.Geometry.InstancesDirty = true;
    }

    public bool StaticGrounds
    {
      get
      {
        return TrixelParticleSystem.Particle.IsGroundStatic(this.Ground.NearLow) && TrixelParticleSystem.Particle.IsGroundStatic(this.Ground.FarHigh);
      }
    }

    private static bool IsGroundStatic(TrileInstance ground)
    {
      if (ground == null || ground.PhysicsState == null)
        return true;
      return ground.PhysicsState.Velocity == Vector3.Zero && ground.PhysicsState.GroundMovement == Vector3.Zero;
    }
  }

  public class Settings
  {
    private TrileInstance explodingInstance;

    public Settings()
    {
      this.MinimumSize = 1;
      this.MaximumSize = 8;
      this.Energy = 1f;
      this.GravityModifier = 1f;
      this.ParticleCount = 40;
    }

    public Vector3 BaseVelocity { get; set; }

    public int MinimumSize { get; set; }

    public int MaximumSize { get; set; }

    public float Energy { get; set; }

    public float GravityModifier { get; set; }

    public Vector3? EnergySource { get; set; }

    public int ParticleCount { get; set; }

    public bool Darken { get; set; }

    public bool Incandesce { get; set; }

    public bool Crumble { get; set; }

    public TrileInstance ExplodingInstance
    {
      get => this.explodingInstance;
      set
      {
        if (!this.EnergySource.HasValue)
          this.EnergySource = new Vector3?(value.Position);
        this.explodingInstance = value;
      }
    }
  }
}
