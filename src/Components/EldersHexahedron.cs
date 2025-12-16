// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EldersHexahedron
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Components.Actions;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class EldersHexahedron : DrawableGameComponent
{
  private static readonly string[] HexStrings = new string[8]
  {
    "HEX_A",
    "HEX_B",
    "HEX_C",
    "HEX_D",
    "HEX_E",
    "HEX_F",
    "HEX_G",
    "HEX_H"
  };
  private readonly ArtObjectInstance AoInstance;
  private Vector3 Origin;
  private Vector3 CameraOrigin;
  private Vector3 GomezOrigin;
  private Quaternion AoRotationOrigin;
  private EldersHexahedron.StarfieldRenderer SfRenderer;
  private StarField Starfield;
  private PlaneParticleSystem Particles;
  private Mesh BeamMesh;
  private Mesh BeamMask;
  private Mesh SolidCubes;
  private Mesh SmallCubes;
  private Mesh MatrixMesh;
  private Mesh RaysMesh;
  private Mesh FlareMesh;
  private ArtObjectInstance TinyChapeau;
  private BackgroundPlane[] StatuePlanes;
  private NesGlitches Glitches;
  private readonly Texture2D[] MatrixWords = new Texture2D[8];
  private Mesh DealGlassesPlane;
  private Mesh TrialRaysMesh;
  private Mesh TrialFlareMesh;
  private float TrialTimeAccumulator;
  private SoundEffect sCollectFez;
  private SoundEffect sHexaTalk;
  private SoundEffect sAmbientHex;
  private SoundEffect sBeamGrow;
  private SoundEffect sExplode;
  private SoundEffect sGomezBeamUp;
  private SoundEffect sTinyBeam;
  private SoundEffect sMatrixRampUp;
  private SoundEffect sHexSlowDown;
  private SoundEffect sRayExplosion;
  private SoundEffect sHexRise;
  private SoundEffect sGomezBeamAppear;
  private SoundEffect sNightTransition;
  private SoundEffect sHexAlign;
  private SoundEffect sStarTrails;
  private SoundEffect sWhiteOut;
  private SoundEffect sHexDisappear;
  private SoundEffect sSparklyParticles;
  private SoundEffect sTrialWhiteOut;
  private SoundEffect[] sHexDrones;
  private SoundEmitter eAmbientHex;
  private SoundEmitter eSparklyParticles;
  private SoundEmitter eHexaTalk;
  private SoundEmitter eHexDrone;
  private int currentDroneIndex;
  private float SpinSpeed;
  private float DestinationSpinSpeed;
  private float SincePhaseStarted;
  private float LastPhaseRadians;
  private float OriginalSpin;
  private float CameraSpinSpeed;
  private float CameraSpins;
  private float DestinationSpins;
  private float ExplodeSpeed;
  private EldersHexahedron.Phase CurrentPhase;
  private Quaternion RotationFrom;
  private float WhiteOutFactor;
  private bool playedRise1;

  public EldersHexahedron(Game game, ArtObjectInstance aoInstance)
    : base(game)
  {
    this.UpdateOrder = 20;
    this.DrawOrder = 101;
    this.AoInstance = aoInstance;
  }

  public override void Initialize()
  {
    base.Initialize();
    StarField starField = new StarField(this.Game);
    starField.Opacity = 0.0f;
    starField.HasHorizontalTrails = true;
    starField.FollowCamera = true;
    StarField component = starField;
    this.Starfield = starField;
    ServiceHelper.AddComponent((IGameComponent) component);
    ServiceHelper.AddComponent((IGameComponent) (this.SfRenderer = new EldersHexahedron.StarfieldRenderer(this.Game, this)));
    this.StatuePlanes = this.LevelManager.BackgroundPlanes.Values.Where<BackgroundPlane>((Func<BackgroundPlane, bool>) (x => x.Id >= 0)).ToArray<BackgroundPlane>();
    this.DealGlassesPlane = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = false,
      SamplerState = SamplerState.PointClamp
    };
    this.DealGlassesPlane.AddFace(new Vector3(1f, 0.25f, 1f), Vector3.Zero, FaceOrientation.Right, true, true);
    ArtObject artObject = this.CMProvider.CurrentLevel.Load<ArtObject>("Art Objects/TINY_CHAPEAUAO");
    int key = IdentifierPool.FirstAvailable<ArtObjectInstance>(this.LevelManager.ArtObjects);
    this.TinyChapeau = new ArtObjectInstance(artObject)
    {
      Id = key
    };
    this.LevelManager.ArtObjects.Add(key, this.TinyChapeau);
    this.TinyChapeau.Initialize();
    this.TinyChapeau.Hidden = true;
    this.TinyChapeau.ArtObject.Group.Position = new Vector3(-0.125f, 0.375f, -0.125f);
    this.TinyChapeau.ArtObject.Group.BakeTransformInstanced<VertexPositionNormalTextureInstance, Matrix>();
    this.BeamMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = false,
      Material = {
        Diffuse = new Vector3(221f, 178f, (float) byte.MaxValue) / (float) byte.MaxValue
      }
    };
    Group vg = this.BeamMesh.AddFace(new Vector3(1f, 1f, 1f), Vector3.Zero, FaceOrientation.Right, true, true);
    Group hg = this.BeamMesh.AddFace(new Vector3(2f, 1f, 2f), Vector3.Zero, FaceOrientation.Right, true, true);
    hg.Material = new Material()
    {
      Opacity = 0.4f,
      Diffuse = new Vector3(221f, 178f, (float) byte.MaxValue) / (float) byte.MaxValue
    };
    hg.Enabled = false;
    this.BeamMask = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = false
    };
    this.BeamMask.AddFace(new Vector3(1f, 1f, 1f), Vector3.Zero, FaceOrientation.Right, true, true);
    this.MatrixMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = false,
      Blending = new BlendingMode?(BlendingMode.Multiply2X)
    };
    this.RaysMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = false,
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    this.FlareMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = false,
      SamplerState = SamplerState.LinearClamp,
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    this.FlareMesh.AddFace(new Vector3(1f, 1f, 1f), Vector3.Zero, FaceOrientation.Right, true, true);
    this.TrialRaysMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.TrialFlareMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.TrialFlareMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Right, true);
    this.LoadSounds();
    this.AoInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f) * Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
    this.Origin = this.AoInstance.Position;
    this.AoRotationOrigin = Quaternion.CreateFromAxisAngle(Vector3.Forward, 0.7853982f) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
    this.AoInstance.Material = new Material();
    this.PlayerManager.Position = new Vector3(18.5f, 879f / 32f, 34.5f);
    this.PlayerManager.Position = this.PlayerManager.Position * Vector3.UnitY + FezMath.XZMask * this.AoInstance.Position;
    this.GomezOrigin = this.PlayerManager.Position;
    this.CameraManager.Center = new Vector3(18.5f, 879f / 32f, 34.5f);
    this.CameraManager.Center = this.CameraManager.Center * Vector3.UnitY + FezMath.XZMask * this.AoInstance.Position + new Vector3(0.0f, 4f, 0.0f);
    this.CameraManager.SnapInterpolation();
    this.CameraOrigin = this.CameraManager.Center;
    while (!this.PlayerManager.CanControl)
      this.PlayerManager.CanControl = true;
    this.PlayerManager.CanControl = false;
    this.CameraManager.Constrained = true;
    this.PlayerManager.HideFez = true;
    this.GenerateCubes();
    this.SpinSpeed = 225f;
    this.sHexSlowDown.Emit();
    this.eAmbientHex = this.sAmbientHex.Emit(true, 0.0f, 0.5f);
    Vector3 vector3 = -this.CameraManager.Viewpoint.ForwardVector() * 4f;
    IPlaneParticleSystems planeParticleSystems = this.PlaneParticleSystems;
    Game game = this.Game;
    PlaneParticleSystemSettings settings = new PlaneParticleSystemSettings();
    settings.SpawnVolume = new BoundingBox()
    {
      Min = this.PlayerManager.Position + new Vector3(-1f, 5f, -1f) + vector3,
      Max = this.PlayerManager.Position + new Vector3(1f, 20f, 1f) + vector3
    };
    settings.Velocity.Base = new Vector3(0.0f, -0.5f, 0.0f);
    settings.Velocity.Variation = new Vector3(0.0f, -0.25f, 0.1f);
    settings.SpawningSpeed = 12f;
    settings.ParticleLifetime = 15f;
    settings.Acceleration = -0.1f;
    settings.SizeBirth = (VaryingVector3) new Vector3(0.125f, 0.125f, 0.125f);
    settings.ColorBirth = (VaryingColor) Color.Black;
    settings.ColorLife.Base = new Color(0.6f, 0.6f, 0.6f, 1f);
    settings.ColorLife.Variation = new Color(0.1f, 0.1f, 0.1f, 0.0f);
    settings.ColorDeath = (VaryingColor) Color.Black;
    settings.FullBright = true;
    settings.RandomizeSpawnTime = true;
    settings.FadeInDuration = 0.25f;
    settings.FadeOutDuration = 0.5f;
    settings.BlendingMode = BlendingMode.Additive;
    PlaneParticleSystem system = this.Particles = new PlaneParticleSystem(game, 200, settings);
    planeParticleSystems.Add(system);
    this.Particles.Enabled = false;
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.DealGlassesPlane.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.DealGlassesPlane.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures" + (this.GameState.SaveData.Finished64 ? "/deal_with_3d" : "/deal_with_it"));
      this.BeamMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      vg.Texture = (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/VerticalGradient");
      hg.Texture = (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/HorizontalGradient");
      this.BeamMask.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.MatrixMesh.Effect = (BaseEffect) new MatrixEffect();
      this.RaysMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.FlareMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.FlareMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/flare_alpha");
      this.TrialRaysMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.TrialFlareMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.TrialFlareMesh.Texture = this.FlareMesh.Texture;
      for (int index = 1; index < 9; ++index)
        this.MatrixWords[index - 1] = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/zuish_matrix/" + (object) index);
      this.Particles.Settings.Texture = this.CMProvider.Global.Load<Texture2D>("Background Planes/dust_particle");
      this.Particles.RefreshTexture();
    }));
    this.LevelManager.LevelChanged += new Action(this.Kill);
  }

  private void LoadSounds()
  {
    this.sCollectFez = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Collects/CollectFez");
    this.sHexaTalk = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Npc/HexahedronTalk");
    this.sAmbientHex = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/AmbientHex");
    this.sBeamGrow = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/BeamGrow");
    this.sExplode = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/Explode");
    this.sGomezBeamUp = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/GomezBeamUp");
    this.sHexSlowDown = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexSlowDown");
    this.sMatrixRampUp = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/MatrixRampUp");
    this.sRayExplosion = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/RayExplosion");
    this.sTinyBeam = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/TinyBeam");
    this.sHexRise = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexRise");
    this.sGomezBeamAppear = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/GomezBeamAppear");
    this.sNightTransition = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/NightTransition");
    this.sHexAlign = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexAlign");
    this.sStarTrails = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/StarTrails");
    this.sWhiteOut = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/WhiteOut");
    this.sHexDisappear = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexDisappear");
    this.sSparklyParticles = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/SparklyParticles");
    this.sTrialWhiteOut = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ending/Pyramid/WhiteOut");
    this.sHexDrones = new SoundEffect[5]
    {
      this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexDrones/HexDrone1"),
      this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexDrones/HexDrone2"),
      this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexDrones/HexDrone3"),
      this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexDrones/HexDrone4"),
      this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Intro/Elders/HexDrones/HexDrone5")
    };
  }

  private void GenerateCubes()
  {
    Vector3[] vector3Array = new Vector3[64 /*0x40*/];
    for (int index1 = 0; index1 < 4; ++index1)
    {
      for (int index2 = 0; index2 < 4; ++index2)
      {
        for (int index3 = 0; index3 < 4; ++index3)
          vector3Array[index1 * 16 /*0x10*/ + index2 * 4 + index3] = new Vector3((float) index1 - 1.5f, (float) index2 - 1.5f, (float) index3 - 1.5f);
      }
    }
    this.SolidCubes = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Opaque)
    };
    this.SmallCubes = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Opaque)
    };
    this.SmallCubes.Rotation = this.SolidCubes.Rotation = this.AoRotationOrigin;
    Trile trile1 = this.LevelManager.ActorTriles(ActorType.CubeShard).FirstOrDefault<Trile>();
    Trile trile2 = this.LevelManager.ActorTriles(ActorType.GoldenCube).FirstOrDefault<Trile>();
    foreach (Vector3 vector3 in vector3Array)
    {
      Group group1 = this.SolidCubes.AddGroup();
      group1.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) trile1.Geometry.Vertices).ToArray<VertexPositionNormalTextureInstance>(), trile1.Geometry.Indices, trile1.Geometry.PrimitiveType);
      group1.Position = vector3;
      group1.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (float) RandomHelper.Random.Next(0, 4) * 1.57079637f);
      group1.BakeTransformWithNormal<VertexPositionNormalTextureInstance>();
      EldersHexahedron.ShardProjectionData shardProjectionData = new EldersHexahedron.ShardProjectionData();
      shardProjectionData.Direction = vector3 * RandomHelper.Between(0.5, 5.0);
      shardProjectionData.Spin = Quaternion.CreateFromAxisAngle(RandomHelper.NormalizedVector(), RandomHelper.Between(0.0, Math.PI / 1000.0));
      group1.CustomData = (object) shardProjectionData;
      Group group2 = this.SmallCubes.AddGroup();
      group2.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) trile2.Geometry.Vertices).ToArray<VertexPositionNormalTextureInstance>(), trile2.Geometry.Indices, trile2.Geometry.PrimitiveType);
      group2.Position = vector3 * RandomHelper.Between(0.5, 1.0);
      group2.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (float) RandomHelper.Random.Next(0, 4) * 1.57079637f);
      group2.BakeTransformWithNormal<VertexPositionNormalTextureInstance>();
      shardProjectionData = new EldersHexahedron.ShardProjectionData();
      shardProjectionData.Direction = vector3 * RandomHelper.Between(0.5, 5.0);
      shardProjectionData.Spin = Quaternion.CreateFromAxisAngle(RandomHelper.NormalizedVector(), RandomHelper.Between(0.0, Math.PI / 1000.0));
      group2.CustomData = (object) shardProjectionData;
    }
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh solidCubes = this.SolidCubes;
      solidCubes.Effect = (BaseEffect) new DefaultEffect.LitTextured()
      {
        Specular = true,
        Emissive = 0.5f,
        AlphaIsEmissive = true
      };
      this.SmallCubes.Effect = (BaseEffect) new DefaultEffect.LitTextured()
      {
        Specular = true
      };
      this.SolidCubes.Texture = this.SmallCubes.Texture = (Dirtyable<Texture>) (Texture) this.LevelManager.TrileSet.TextureAtlas;
    }));
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    ServiceHelper.RemoveComponent<StarField>(this.Starfield);
    ServiceHelper.RemoveComponent<EldersHexahedron.StarfieldRenderer>(this.SfRenderer);
    ServiceHelper.RemoveComponent<NesGlitches>(this.Glitches);
    this.SolidCubes.Dispose();
    this.SmallCubes.Dispose();
    this.MatrixMesh.Dispose();
    this.BeamMask.Dispose();
    this.BeamMesh.Dispose();
    this.TrialRaysMesh.Dispose();
    this.TrialFlareMesh.Dispose();
    this.FlareMesh.Dispose();
    this.RaysMesh.Dispose();
    this.DealGlassesPlane.Dispose();
    this.GameState.SkyOpacity = 1f;
    this.Visible = this.Enabled = false;
    this.LevelManager.LevelChanged -= new Action(this.Kill);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || this.CameraManager.Viewpoint == Viewpoint.Perspective || this.CurrentPhase == EldersHexahedron.Phase.WaitSpin && this.GameState.InCutscene)
      return;
    this.AoInstance.Visible = true;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.SincePhaseStarted += totalSeconds;
    switch (this.CurrentPhase)
    {
      case EldersHexahedron.Phase.ZoomOut:
        double num1;
        float amount1 = Easing.EaseInOut(num1 = (double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 1.5) / 5.0)), EasingType.Quadratic);
        if (num1 > 0.0 && !this.playedRise1)
        {
          this.sHexRise.Emit();
          this.playedRise1 = true;
        }
        this.CameraManager.PixelsPerTrixel = MathHelper.Lerp(3f, 2f, amount1);
        this.CameraManager.Center = Vector3.Lerp(this.CameraOrigin, this.CameraOrigin + new Vector3(0.0f, 4f, 0.0f), amount1);
        this.CameraManager.SnapInterpolation();
        this.AoInstance.Position = Vector3.Lerp(this.Origin + new Vector3(0.0f, 3.5f, 0.0f), this.Origin + new Vector3(0.0f, 8f, 0.0f), amount1);
        if ((double) this.SpinSpeed > 0.5)
          this.SpinSpeed *= 0.98f;
        this.AoInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.SpinSpeed * 0.01f) * this.AoInstance.Rotation;
        if (num1 != 1.0)
          break;
        this.PlayerManager.CanControl = true;
        this.CurrentPhase = EldersHexahedron.Phase.Talk1;
        this.SincePhaseStarted = 0.0f;
        this.eHexaTalk = this.sHexaTalk.Emit(true);
        this.Talk1();
        break;
      case EldersHexahedron.Phase.Talk1:
        this.LastPhaseRadians = FezMath.WrapAngle(this.SincePhaseStarted / 2f);
        this.AoInstance.Position = this.Origin + new Vector3(0.0f, (float) (8.0 + Math.Sin((double) this.LastPhaseRadians) * 0.25), 0.0f);
        this.AoInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.SpinSpeed * 0.01f) * this.AoInstance.Rotation;
        break;
      case EldersHexahedron.Phase.Beam:
        this.Starfield.Opacity = Easing.EaseInOut((double) FezMath.Saturate(this.SincePhaseStarted / 5f), EasingType.Sine);
        float amount2 = Easing.EaseInOut((double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 1.0) / 3.5)), EasingType.Sine);
        if ((double) amount2 > 0.0 && this.sHexAlign != null)
        {
          this.sHexAlign.Emit();
          this.sHexAlign = (SoundEffect) null;
        }
        if ((double) this.SincePhaseStarted < 1.0)
        {
          this.AoInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.SpinSpeed * 0.01f) * this.AoInstance.Rotation;
          this.RotationFrom = this.AoInstance.Rotation;
        }
        else
        {
          this.RotationFrom = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.SpinSpeed * 0.01f) * this.RotationFrom;
          this.AoInstance.Rotation = Quaternion.Slerp(this.RotationFrom, this.AoRotationOrigin, amount2);
        }
        this.AoInstance.Position = this.Origin + new Vector3(0.0f, (float) (8.0 + Math.Sin((double) this.SincePhaseStarted / 2.0 + (double) this.LastPhaseRadians) * 0.25), 0.0f);
        float amount3 = Easing.EaseOut((double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 5.0) / 2.0)), EasingType.Quadratic);
        this.BeamMesh.Material.Opacity = amount3 * 0.425f;
        if ((double) amount3 > 0.0 && RandomHelper.Probability(Math.Pow(1.0 - (double) amount3, 5.0)))
          this.BeamMesh.Material.Opacity = RandomHelper.Between(0.0, 0.5);
        this.BeamMesh.Position = new Vector3(this.Origin.X, (float) (((double) this.AoInstance.Position.Y + 26.0) / 2.0), this.Origin.Z) + this.CameraManager.InverseView.Forward * 5f;
        this.BeamMesh.Scale = new Vector3(89f / 16f, this.AoInstance.Position.Y - 26f, 89f / 16f);
        if ((double) amount3 > 0.0 && this.sGomezBeamAppear != null)
        {
          this.sGomezBeamAppear.Emit();
          this.sGomezBeamAppear = (SoundEffect) null;
        }
        float num2 = this.PlayerManager.Position.Y + 3f / 16f;
        Mesh beamMask1 = this.BeamMask;
        Vector3 vector3_1 = new Vector3(this.Origin.X, (float) ((26.0 + (double) num2) / 2.0), this.Origin.Z);
        Matrix inverseView1 = this.CameraManager.InverseView;
        Vector3 vector3_2 = inverseView1.Forward * 5f;
        Vector3 vector3_3 = vector3_1 + vector3_2;
        inverseView1 = this.CameraManager.InverseView;
        Vector3 vector3_4 = inverseView1.Right / 32f;
        Vector3 vector3_5 = vector3_3 + vector3_4;
        beamMask1.Position = vector3_5;
        this.BeamMask.Rotation = this.BeamMesh.Rotation;
        this.BeamMask.Scale = new Vector3(this.PlayerManager.Size.X + 7f / 32f, num2 - 26f, this.PlayerManager.Size.Z + 7f / 32f);
        if ((double) this.SincePhaseStarted > 5.5)
        {
          amount3 = Easing.EaseOut((double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 5.5) / 3.0)), EasingType.Sine);
          if (this.sGomezBeamUp != null)
          {
            this.sGomezBeamUp.Emit();
            this.sGomezBeamUp = (SoundEffect) null;
          }
          this.PlayerManager.Action = ActionType.Jumping;
          this.PlayerManager.Velocity = Vector3.Zero;
          this.PlayerManager.Position = Vector3.Lerp(this.GomezOrigin, this.GomezOrigin + new Vector3(0.0f, 3f, 0.0f), amount3);
        }
        if ((double) amount3 != 1.0)
          break;
        this.Starfield.Enabled = true;
        this.CurrentPhase = EldersHexahedron.Phase.MatrixSpin;
        this.LastPhaseRadians = FezMath.WrapAngle(this.SincePhaseStarted / 2f + this.LastPhaseRadians);
        this.SincePhaseStarted = 0.0f;
        Vector3 position = this.CameraManager.Position;
        Vector3 center = this.CameraManager.Center;
        this.OriginalSpin = (float) Math.Atan2((double) position.X - (double) center.X, (double) position.Z - (double) center.Z);
        this.CameraSpinSpeed = 5E-05f;
        this.sMatrixRampUp.Emit();
        break;
      case EldersHexahedron.Phase.MatrixSpin:
        this.PlayerManager.Action = ActionType.Jumping;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.PlayerManager.Position = this.GomezOrigin + new Vector3(0.0f, 3f, 0.0f);
        this.CameraManager.ForceInterpolation = true;
        this.AoInstance.Position = this.Origin + new Vector3(0.0f, (float) (8.0 + Math.Sin((double) this.SincePhaseStarted / 2.0 + (double) this.LastPhaseRadians) * 0.25), 0.0f);
        if ((double) this.SincePhaseStarted > 6.5 && this.sStarTrails != null)
        {
          this.sStarTrails.Emit();
          this.sStarTrails = (SoundEffect) null;
        }
        if ((double) this.SincePhaseStarted > 8.0 && this.sWhiteOut != null)
        {
          this.sWhiteOut.Emit();
          this.sWhiteOut = (SoundEffect) null;
        }
        float num3 = (float) (0.05000000074505806 + 2.0 * (double) Easing.EaseIn((double) this.SincePhaseStarted / 10.0, EasingType.Quintic));
        if ((double) this.SincePhaseStarted > 10.0)
          num3 = (float) (0.05000000074505806 + (1.0 - ((double) this.SincePhaseStarted - 10.0) / 3.0));
        if (RandomHelper.Probability((double) num3))
        {
          for (int index = 0; (double) index <= Math.Floor((double) num3); ++index)
          {
            Texture2D matrixWord = this.MatrixWords[RandomHelper.Random.Next(0, 8)];
            Vector3 size = new Vector3((float) matrixWord.Width, (float) matrixWord.Height, (float) matrixWord.Width) / 16f;
            Group group = this.MatrixMesh.AddFace(size, new Vector3(0.0f, (float) (-(double) size.Y / 2.0), 0.0f), FaceOrientation.Right, true, true);
            group.Texture = (Texture) matrixWord;
            float num4 = RandomHelper.Between((double) this.BeamMesh.Scale.X * -0.5 + (double) size.X / 2.0, (double) this.BeamMesh.Scale.X * 0.5 - (double) size.X / 2.0);
            group.Position += new Vector3(num4, this.BeamMesh.Scale.Y - 1f, num4);
            group.CustomData = (object) RandomHelper.Between((double) Math.Max(1f, this.SincePhaseStarted / 3f), (double) this.SincePhaseStarted * 3.0);
          }
        }
        (this.MatrixMesh.Effect as MatrixEffect).MaxHeight = this.MatrixMesh.Position.Y + this.BeamMesh.Scale.Y / 2f;
        for (int index = this.MatrixMesh.Groups.Count - 1; index >= 0; --index)
        {
          Group group = this.MatrixMesh.Groups[index];
          group.Position -= new Vector3(0.0f, totalSeconds * (float) group.CustomData, 0.0f);
          group.CustomData = (object) (float) ((double) (float) group.CustomData * (1.0 + (double) totalSeconds / 2.0));
          if ((double) group.Position.Y < -(double) this.BeamMesh.Scale.Y)
            this.MatrixMesh.RemoveGroupAt(index);
        }
        this.WhiteOutFactor = (double) this.SincePhaseStarted <= 8.0 || (double) this.SincePhaseStarted >= 10.0 ? ((double) this.SincePhaseStarted <= 10.0 || (double) this.SincePhaseStarted >= 13.0 ? 0.0f : (float) Math.Pow(1.0 - ((double) this.SincePhaseStarted - 10.0) / 3.0, 2.0)) : (float) Math.Pow(((double) this.SincePhaseStarted - 8.0) / 2.0, 6.0);
        if ((double) this.SincePhaseStarted < 10.0)
        {
          if ((double) this.CameraSpinSpeed < 0.07)
            this.CameraSpinSpeed *= 1.0175f;
          this.CameraSpins += this.CameraSpinSpeed;
        }
        else
        {
          this.Starfield.ReverseTiming = true;
          float amount4 = Easing.EaseOut((double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 10.0) / 3.0)), EasingType.Quadratic);
          this.CameraSpins = MathHelper.Lerp(this.DestinationSpins, (float) FezMath.Round((double) this.DestinationSpins + (double) this.CameraSpinSpeed * 60.0), amount4);
          this.MatrixMesh.Material.Diffuse = new Vector3(1f - amount4);
          if ((double) this.SincePhaseStarted > 14.0)
          {
            this.CameraManager.ForceInterpolation = false;
            this.PlayerManager.CanControl = true;
            this.LastPhaseRadians = FezMath.WrapAngle(this.SincePhaseStarted / 2f + this.LastPhaseRadians);
            this.SincePhaseStarted = 0.0f;
            this.CurrentPhase = EldersHexahedron.Phase.Talk2;
            this.eHexaTalk = this.sHexaTalk.Emit(true);
            this.Talk2();
          }
        }
        this.CameraManager.Direction = Vector3.Normalize(new Vector3((float) Math.Sin((double) this.OriginalSpin + (double) this.CameraSpins * 6.2831854820251465), 0.0f, (float) Math.Cos((double) this.OriginalSpin + (double) this.CameraSpins * 6.2831854820251465)));
        this.CameraManager.SnapInterpolation();
        Mesh beamMesh = this.BeamMesh;
        Vector3 vector3_6 = new Vector3(this.Origin.X, (float) (((double) this.AoInstance.Position.Y + 26.0) / 2.0), this.Origin.Z);
        Matrix inverseView2 = this.CameraManager.InverseView;
        Vector3 vector3_7 = inverseView2.Forward * 5f;
        Vector3 vector3_8 = vector3_6 + vector3_7;
        beamMesh.Position = vector3_8;
        this.BeamMesh.Rotation = this.CameraManager.Rotation * Quaternion.CreateFromAxisAngle(Vector3.Up, -1.57079637f);
        float num5 = this.PlayerManager.Position.Y + 3f / 16f;
        Mesh beamMask2 = this.BeamMask;
        Vector3 vector3_9 = new Vector3(this.Origin.X, (float) ((26.0 + (double) num5) / 2.0), this.Origin.Z);
        inverseView2 = this.CameraManager.InverseView;
        Vector3 vector3_10 = inverseView2.Forward * 5f;
        Vector3 vector3_11 = vector3_9 + vector3_10;
        inverseView2 = this.CameraManager.InverseView;
        Vector3 vector3_12 = inverseView2.Right / 32f;
        Vector3 vector3_13 = vector3_11 + vector3_12;
        beamMask2.Position = vector3_13;
        this.BeamMask.Rotation = this.BeamMesh.Rotation;
        this.MatrixMesh.Rotation = this.BeamMask.Rotation;
        this.MatrixMesh.Position = this.BeamMask.Position;
        this.AoInstance.Rotation = this.BeamMesh.Rotation * this.AoRotationOrigin;
        break;
      case EldersHexahedron.Phase.Talk2:
        this.AoInstance.Position = this.Origin + new Vector3(0.0f, (float) (8.0 + Math.Sin((double) this.SincePhaseStarted / 2.0 + (double) this.LastPhaseRadians) * 0.25), 0.0f);
        if (this.InputManager.CancelTalk == FezButtonState.Pressed)
          this.SpeechBubble.Hide();
        this.PlayerManager.Action = ActionType.Jumping;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.PlayerManager.Position = this.GomezOrigin + new Vector3(0.0f, 3f, 0.0f);
        break;
      case EldersHexahedron.Phase.Disappear:
        float num6 = Easing.EaseInOut((double) FezMath.Saturate(this.SincePhaseStarted / 2f), EasingType.Sine);
        this.AoInstance.Material.Opacity = 1f - num6;
        this.BeamMesh.Material.Opacity = this.AoInstance.Material.Opacity * 0.425f;
        this.AoInstance.MarkDirty();
        this.PlayerManager.Action = ActionType.Falling;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.PlayerManager.Position = Vector3.Lerp(this.GomezOrigin, this.GomezOrigin + new Vector3(0.0f, 3f, 0.0f), 1f - num6);
        float num7 = this.PlayerManager.Position.Y + 3f / 16f;
        this.BeamMask.Position = new Vector3(this.Origin.X, (float) ((26.0 + (double) num7) / 2.0), this.Origin.Z) + this.CameraManager.InverseView.Forward * 5f + this.CameraManager.InverseView.Right / 32f;
        this.BeamMask.Rotation = this.BeamMesh.Rotation;
        this.BeamMask.Scale = new Vector3(this.PlayerManager.Size.X + 7f / 32f, num7 - 26f, this.PlayerManager.Size.Z + 7f / 32f);
        if ((double) num6 >= 0.5)
        {
          if (!this.Particles.Enabled)
            this.eSparklyParticles = this.sSparklyParticles.Emit(true);
          this.Particles.Enabled = true;
          this.eSparklyParticles.VolumeFactor = num6 - 0.5f;
        }
        if ((double) num6 != 1.0)
          break;
        this.SincePhaseStarted = 0.0f;
        this.AoInstance.Hidden = true;
        this.AoInstance.Visible = false;
        this.AoInstance.MarkDirty();
        this.CurrentPhase = EldersHexahedron.Phase.FezBeamGrow;
        break;
      case EldersHexahedron.Phase.FezBeamGrow:
        if ((double) this.SincePhaseStarted > 2.0)
          this.PlayerManager.Action = ActionType.LookingUp;
        this.BeamMesh.Position = new Vector3(this.Origin.X, this.AoInstance.Position.Y, this.Origin.Z) + this.CameraManager.InverseView.Forward * 5f;
        this.BeamMesh.Groups[1].Enabled = true;
        if ((double) this.SincePhaseStarted < 2.0)
        {
          if (this.sTinyBeam != null)
          {
            this.sTinyBeam.Emit();
            this.sTinyBeam = (SoundEffect) null;
          }
          float num8 = Easing.EaseIn((double) FezMath.Saturate(this.SincePhaseStarted), EasingType.Quintic);
          this.BeamMesh.Material.Opacity = num8 * 0.6f;
          this.BeamMesh.Scale = new Vector3(1f / 16f * num8, this.CameraManager.Radius * 1.5f, 1f / 16f * num8);
        }
        else if ((double) this.SincePhaseStarted > 3.0 && (double) this.SincePhaseStarted < 3.5)
        {
          if (this.sBeamGrow != null)
          {
            this.ScheduleFades();
            this.sBeamGrow.Emit();
            this.sBeamGrow = (SoundEffect) null;
          }
          float num9 = (float) Math.Pow((double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 3.0) / 0.5)), 5.0);
          this.BeamMesh.Scale = new Vector3((float) (1.0 / 16.0 + (double) this.CameraManager.Radius * (double) num9), this.CameraManager.Radius, (float) (1.0 / 16.0 + (double) this.CameraManager.Radius * (double) num9));
          this.BeamMesh.Material.Opacity = (float) (0.60000002384185791 + (double) num9 * 0.40000000596046448);
        }
        else if ((double) this.SincePhaseStarted > 3.5)
        {
          this.BeamMesh.Scale = new Vector3(2f, this.CameraManager.Radius, 2f);
          this.BeamMesh.Material.Opacity = 0.5f;
        }
        if ((double) this.SincePhaseStarted <= 6.0)
          break;
        this.SincePhaseStarted = 0.0f;
        this.CurrentPhase = EldersHexahedron.Phase.FezComeDown;
        break;
      case EldersHexahedron.Phase.FezComeDown:
        if (!this.GameState.SaveData.IsNewGamePlus)
        {
          this.PlayerManager.Action = ActionType.LookingUp;
          this.TinyChapeau.Hidden = false;
          this.TinyChapeau.Visible = true;
          this.TinyChapeau.ArtObject.Group.Enabled = true;
        }
        Vector3 vector3_14 = this.PlayerManager.Position + new Vector3(0.0f, 20f, 0.0f);
        Vector3 vector3_15 = this.PlayerManager.Position + new Vector3(0.0f, (float) ((double) this.PlayerManager.Size.Y / 2.0 + 3.0 / 16.0), 0.0f) + (float) this.PlayerManager.LookingDirection.Sign() * this.CameraManager.Viewpoint.RightVector() * -4f / 16f;
        if (this.GameState.SaveData.IsNewGamePlus)
          vector3_15 += new Vector3(0.0f, -21f / 64f, -7f / 32f);
        float amount5 = MathHelper.Lerp(Easing.EaseOut((double) FezMath.Saturate(this.SincePhaseStarted / 20f), EasingType.Sine), Easing.EaseOut((double) FezMath.Saturate(this.SincePhaseStarted / 20f), EasingType.Quadratic), 0.5f);
        if (this.GameState.SaveData.IsNewGamePlus)
        {
          this.DealGlassesPlane.Position = Vector3.Lerp(vector3_14, vector3_15, amount5) - this.CameraManager.Viewpoint.ForwardVector() * 4f;
        }
        else
        {
          this.TinyChapeau.Position = Vector3.Lerp(vector3_14, vector3_15, amount5) - this.CameraManager.Viewpoint.ForwardVector() * 4f;
          this.TinyChapeau.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((1.0 - (double) amount5) * 20.0));
        }
        if ((double) this.SincePhaseStarted > 4.0)
        {
          this.CameraManager.PixelsPerTrixel = 15f;
          this.CameraManager.Center = (this.GameState.SaveData.IsNewGamePlus ? this.DealGlassesPlane.Position : this.TinyChapeau.Position) + new Vector3(0.0f, -0.25f, 0.0f);
          this.CameraManager.SnapInterpolation();
          this.eSparklyParticles.VolumeFactor = 1f;
          if (this.GameState.SaveData.IsNewGamePlus)
            this.PlayerManager.Action = ActionType.Standing;
        }
        else if (this.GameState.SaveData.IsNewGamePlus)
          this.PlayerManager.Action = ActionType.LookingUp;
        if ((double) this.SincePhaseStarted < 19.75)
          break;
        this.TinyChapeau.Hidden = true;
        this.TinyChapeau.Visible = false;
        this.PlayerManager.HideFez = false;
        this.Particles.FadeOutAndDie(0.1f);
        this.eSparklyParticles.FadeOutAndDie(1.25f);
        this.sRayExplosion.Emit();
        if (this.GameState.SaveData.Finished64)
          this.GameState.StereoMode = true;
        this.SincePhaseStarted = 0.0f;
        this.CurrentPhase = EldersHexahedron.Phase.Beamsplode;
        this.RaysMesh.Position = this.TinyChapeau.Position - new Vector3(0.0f, 1f / 16f, 0.0f) + this.CameraManager.Viewpoint.ForwardVector() * 10f;
        ServiceHelper.AddComponent((IGameComponent) new ScreenFade(this.Game)
        {
          FromColor = Color.White,
          ToColor = ColorEx.TransparentWhite,
          Duration = 0.125f,
          EaseOut = true
        });
        break;
      case EldersHexahedron.Phase.Beamsplode:
        bool fullForce = false;
        if ((double) this.SincePhaseStarted > 2.5)
        {
          this.CameraManager.PixelsPerTrixel = 10f;
          this.CameraManager.Center = this.CameraOrigin - new Vector3(0.0f, 3f, 0.0f);
          this.CameraManager.SnapInterpolation();
        }
        else if ((double) this.SincePhaseStarted > 0.25)
        {
          fullForce = true;
          this.CameraManager.PixelsPerTrixel = 1f;
          this.CameraManager.Center = this.CameraOrigin + new Vector3(0.0f, 15f, 0.0f);
          this.CameraManager.SnapInterpolation();
          this.BeamMesh.Material.Opacity = this.BeamMesh.Groups[1].Material.Opacity = 0.0f;
        }
        this.RaysMesh.Position = this.PlayerManager.Position + new Vector3(0.0f, (float) ((double) this.PlayerManager.Size.Y / 2.0 + 3.0 / 16.0), 0.0f) + (float) this.PlayerManager.LookingDirection.Sign() * this.CameraManager.Viewpoint.RightVector() * -4f / 16f - new Vector3(0.0f, 1f / 16f, 0.0f) + this.CameraManager.Viewpoint.ForwardVector() * 10f;
        if ((double) this.SincePhaseStarted < 2.5 && RandomHelper.Probability(fullForce ? 0.5 : 0.25))
          this.AddSplodeBeam(fullForce);
        for (int index = this.RaysMesh.Groups.Count - 1; index >= 0; --index)
        {
          Group group = this.RaysMesh.Groups[index];
          EldersHexahedron.RayData customData = (EldersHexahedron.RayData) group.CustomData;
          group.Rotation *= Quaternion.CreateFromAxisAngle(this.CameraManager.Viewpoint.ForwardVector(), customData.Speed * 0.01f * (float) customData.Sign);
          Vector3 vector3_16 = Vector3.Transform((group.Geometry as IndexedUserPrimitives<FezVertexPositionColor>).Vertices[1].Position, group.Rotation);
          bool flag = group.Geometry.VertexCount > 2;
          Vector3 vector3_17 = Vector3.Zero;
          if (flag)
            vector3_17 = Vector3.Transform((group.Geometry as IndexedUserPrimitives<FezVertexPositionColor>).Vertices[2].Position, group.Rotation);
          customData.SinceAlive += totalSeconds;
          customData.Speed *= 0.975f;
          group.CustomData = (object) customData;
          double num10 = Math.Atan2((double) vector3_16.Y, (double) vector3_16.Z);
          double num11 = Math.Atan2((double) vector3_17.Y, (double) vector3_17.Z);
          if (num10 < 0.10000000149011612 || num10 > 3.041592652099677 || flag && (num11 < 0.10000000149011612 || num11 > 3.041592652099677))
            group.Material.Opacity *= 0.8f;
          else if ((double) customData.SinceAlive > 1.0)
            group.Material.Opacity *= 0.9f;
          else
            group.Material.Opacity = 1f;
          if ((double) this.SincePhaseStarted > 2.5)
          {
            group.Material.Opacity *= 0.6f;
            group.Scale *= 0.6f;
          }
          if (num10 < 0.0 || (double) group.Material.Opacity < 0.0040000001899898052)
            this.RaysMesh.RemoveGroupAt(index);
        }
        if ((double) this.SincePhaseStarted <= 3.0)
          break;
        this.RaysMesh.Groups.Clear();
        this.SincePhaseStarted = 0.0f;
        this.CurrentPhase = EldersHexahedron.Phase.Yay;
        ServiceHelper.AddComponent((IGameComponent) (this.Glitches = new NesGlitches(this.Game)));
        break;
      case EldersHexahedron.Phase.Yay:
        this.DealGlassesPlane.Material.Opacity = FezMath.Saturate(1f - this.SincePhaseStarted);
        if ((double) this.SincePhaseStarted <= 1.0)
          break;
        this.PlayerManager.Action = ActionType.Victory;
        this.sCollectFez.Emit();
        this.PlayerManager.CanControl = true;
        this.PlayerManager.CanRotate = true;
        if (this.GameState.SaveData.Finished32)
          this.GameState.SaveData.HasFPView = true;
        if (this.GameState.SaveData.Finished64)
          this.GameState.SaveData.HasFPView = this.GameState.SaveData.HasStereo3D = true;
        this.CurrentPhase = EldersHexahedron.Phase.WaitSpin;
        this.SincePhaseStarted = 0.0f;
        this.eHexDrone = this.sHexDrones[0].Emit(true, 0.0f, 0.0f);
        this.AoInstance.Visible = false;
        this.CameraManager.ViewpointChanged += new Action(this.AddSpin);
        this.ScheduleText();
        break;
      case EldersHexahedron.Phase.WaitSpin:
        this.WaitSpin(totalSeconds);
        break;
      case EldersHexahedron.Phase.HexaExplode:
        this.HexaExplode(totalSeconds);
        break;
      case EldersHexahedron.Phase.ThatsIt:
        this.Glitches.ActiveGlitches = 0;
        this.Glitches.FreezeProbability = 0.0f;
        this.Kill();
        break;
    }
  }

  private void WaitSpin(float elapsedTime)
  {
    if ((double) this.SincePhaseStarted < 3.0 && (double) this.SincePhaseStarted > 1.0)
    {
      this.CameraManager.PixelsPerTrixel = MathHelper.Lerp(10f, 2f, Easing.EaseInOut((double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 1.0) / 2.0)), EasingType.Sine));
      this.CameraManager.Center = Vector3.Lerp(this.CameraOrigin - new Vector3(0.0f, 3f, 0.0f), this.CameraOrigin + new Vector3(0.0f, 4f, 0.0f), Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.SincePhaseStarted - 1.0) / 2.0)), EasingType.Sine));
      this.AoInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f) * Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
      this.CameraManager.SnapInterpolation();
      this.AoInstance.Visible = false;
      this.PlayerManager.CanRotate = true;
    }
    if ((double) this.SincePhaseStarted < 2.0)
      return;
    float num = Easing.EaseInOut((double) FezMath.Saturate(this.SincePhaseStarted - 3f), EasingType.Sine);
    this.AoInstance.Material.Opacity = num;
    if (this.eHexDrone != null)
      this.eHexDrone.VolumeFactor = num;
    this.AoInstance.Hidden = false;
    this.AoInstance.Visible = true;
    this.AoInstance.MarkDirty();
    if ((double) Math.Abs(this.SpinSpeed) < (double) Math.Abs(this.DestinationSpinSpeed))
    {
      this.SpinSpeed *= 1f + elapsedTime;
      if ((double) Math.Abs(this.SpinSpeed) > (double) Math.Abs(this.DestinationSpinSpeed))
        this.SpinSpeed = this.DestinationSpinSpeed;
    }
    if ((double) Math.Abs(this.SpinSpeed) > 32.0)
    {
      this.DestinationSpinSpeed *= 1f + elapsedTime;
      if (!this.GameState.IsTrialMode)
      {
        this.UpdateRays(elapsedTime * 2f);
        if ((double) Math.Abs(this.SpinSpeed) > 50.0)
        {
          this.UpdateRays(elapsedTime);
          this.UpdateRays(elapsedTime);
        }
      }
    }
    float max = Easing.EaseIn((double) this.SpinSpeed / 100.0, EasingType.Quadratic);
    IGameCameraManager cameraManager = this.CameraManager;
    cameraManager.InterpolatedCenter = cameraManager.InterpolatedCenter + new Vector3(RandomHelper.Between(-(double) max, (double) max), RandomHelper.Between(-(double) max, (double) max), RandomHelper.Between(-(double) max, (double) max));
    if (!this.GameState.IsTrialMode && (double) Math.Abs(this.SpinSpeed) > 30.0)
    {
      this.Glitches.DisappearProbability = FezMath.Saturate((float) (1.0 - ((double) Math.Abs(this.SpinSpeed) - 30.0) / 98.0)) * 0.1f;
      this.Glitches.ActiveGlitches = FezMath.Round((double) FezMath.Saturate((float) (((double) Math.Abs(this.SpinSpeed) - 30.0) / 98.0)) * 75.0);
      this.Glitches.FreezeProbability = Easing.EaseIn((double) FezMath.Saturate((float) (((double) Math.Abs(this.SpinSpeed) - 30.0) / 98.0)), EasingType.Cubic) * 0.01f;
    }
    if (this.CameraManager.Viewpoint != Viewpoint.Perspective && !this.CameraManager.ProjectionTransition)
    {
      this.AoInstance.Position = this.CameraManager.Center + new Vector3(0.0f, (float) Math.Sin((double) this.SincePhaseStarted / 2.0 + (double) this.LastPhaseRadians) * 0.25f, 0.0f);
      this.AoInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.SpinSpeed * 0.01f) * this.AoInstance.Rotation;
    }
    if ((double) Math.Abs(this.SpinSpeed) < 105.0)
      return;
    if (this.PlayerManager.CanControl)
      ServiceHelper.AddComponent((IGameComponent) new ScreenFade(this.Game)
      {
        FromColor = Color.White,
        ToColor = ColorEx.TransparentWhite,
        Duration = 0.75f
      });
    this.PlayerManager.CanControl = false;
    this.CameraManager.ViewpointChanged -= new Action(this.AddSpin);
    this.ExplodeSpeed = 1f / 16f;
    this.CameraSpinSpeed = 1E-05f;
    this.CameraSpins = 0.0f;
    Vector3 position = this.CameraManager.Position;
    Vector3 center = this.CameraManager.Center;
    this.OriginalSpin = (float) Math.Atan2((double) position.X - (double) center.X, (double) position.Z - (double) center.Z);
    this.ScheduleExplode();
  }

  private void HexaExplode(float elapsedTime)
  {
    this.AoInstance.Hidden = true;
    this.AoInstance.Visible = false;
    this.SmallCubes.Position = this.SolidCubes.Position = this.AoInstance.Position;
    if ((double) this.SincePhaseStarted > 0.25)
    {
      double num = (double) this.SincePhaseStarted / 13.0;
      if (this.sExplode != null)
      {
        this.sExplode.Emit();
        this.sExplode = (SoundEffect) null;
      }
      this.ExplodeSpeed *= 0.95f;
      foreach (Group group in this.SolidCubes.Groups)
      {
        group.Position += ((EldersHexahedron.ShardProjectionData) group.CustomData).Direction * (float) (0.004999999888241291 + (double) this.ExplodeSpeed + (double) this.CameraSpinSpeed / 3.0);
        group.Rotation *= ((EldersHexahedron.ShardProjectionData) group.CustomData).Spin;
      }
      foreach (Group group in this.SmallCubes.Groups)
      {
        group.Position += ((EldersHexahedron.ShardProjectionData) group.CustomData).Direction * (float) (0.004999999888241291 + (double) this.ExplodeSpeed + (double) this.CameraSpinSpeed / 3.0);
        group.Rotation *= ((EldersHexahedron.ShardProjectionData) group.CustomData).Spin;
      }
    }
    this.CameraSpinSpeed *= 1.01f;
    if ((double) this.SincePhaseStarted < 10.0)
      this.CameraSpinSpeed = MathHelper.Min(this.CameraSpinSpeed, 0.004f);
    this.CameraSpins += this.CameraSpinSpeed;
    this.CameraSpins += this.ExplodeSpeed / 5f;
    this.CameraManager.Direction = Vector3.Normalize(new Vector3((float) Math.Sin((double) this.OriginalSpin + (double) this.CameraSpins * 6.2831854820251465), 0.0f, (float) Math.Cos((double) this.OriginalSpin + (double) this.CameraSpins * 6.2831854820251465)));
    this.CameraManager.SnapInterpolation();
    if (!this.GameState.IsTrialMode)
    {
      this.Glitches.ActiveGlitches = FezMath.Round((double) Easing.EaseIn((double) FezMath.Saturate(this.SincePhaseStarted / 13f), EasingType.Decic) * 400.0 + 2.0);
      this.Glitches.FreezeProbability = (double) this.SincePhaseStarted < 8.0 ? 0.0f : ((double) this.SincePhaseStarted < 10.0 ? 1f / 1000f : ((double) this.SincePhaseStarted < 11.0 ? 0.1f : 0.01f));
      if ((double) this.SincePhaseStarted > 13.0)
        this.Glitches.FreezeProbability = 1f;
    }
    if (this.GameState.IsTrialMode)
    {
      this.UpdateRays(elapsedTime * this.CameraSpinSpeed);
      this.UpdateRays(elapsedTime * this.CameraSpinSpeed);
    }
    else
    {
      for (int index = this.TrialRaysMesh.Groups.Count - 1; index >= 0; --index)
      {
        Group group = this.TrialRaysMesh.Groups[index];
        group.Material.Diffuse = new Vector3(FezMath.Saturate(1f - this.SincePhaseStarted));
        group.Scale *= new Vector3(1.5f, 1f, 1f);
        if (FezMath.AlmostEqual(group.Material.Diffuse.X, 0.0f))
          this.TrialRaysMesh.RemoveGroupAt(index);
      }
    }
    if (!this.GameState.IsTrialMode && (double) this.SincePhaseStarted > 15.0)
    {
      this.CurrentPhase = EldersHexahedron.Phase.ThatsIt;
      ServiceHelper.AddComponent((IGameComponent) new Reboot(this.Game, "GOMEZ_HOUSE"));
    }
    else
    {
      if (!this.GameState.IsTrialMode || (double) this.TrialTimeAccumulator <= 7.5)
        return;
      this.CurrentPhase = EldersHexahedron.Phase.ThatsIt;
      this.GameState.SkipLoadScreen = true;
      ServiceHelper.AddComponent((IGameComponent) new ScreenFade(this.Game)
      {
        FromColor = Color.White,
        ToColor = ColorEx.TransparentWhite,
        Duration = 2f
      });
      this.LevelManager.ChangeLevel("ARCH");
      Waiters.Wait((Func<bool>) (() => !this.GameState.Loading), (Action) (() =>
      {
        this.GameState.SkipLoadScreen = false;
        this.Visible = false;
        while (!this.PlayerManager.CanControl)
          this.PlayerManager.CanControl = true;
        this.SoundManager.MusicVolumeFactor = 1f;
      }));
    }
  }

  private void UpdateRays(float elapsedSeconds)
  {
    if (this.GameState.IsTrialMode)
    {
      if (this.TrialRaysMesh.Groups.Count < 50 && RandomHelper.Probability(0.25))
      {
        float x = 6f + RandomHelper.Centered(4.0);
        float num = RandomHelper.Between(0.5, (double) x / 2.5);
        Group group = this.TrialRaysMesh.AddGroup();
        group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionTexture>(new FezVertexPositionTexture[6]
        {
          new FezVertexPositionTexture(new Vector3(0.0f, (float) ((double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(0.0f, 0.0f)),
          new FezVertexPositionTexture(new Vector3(x, num / 2f, 0.0f), new Vector2(1f, 0.0f)),
          new FezVertexPositionTexture(new Vector3(x, (float) ((double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(1f, 0.45f)),
          new FezVertexPositionTexture(new Vector3(x, (float) (-(double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(1f, 0.55f)),
          new FezVertexPositionTexture(new Vector3(x, (float) (-(double) num / 2.0), 0.0f), new Vector2(1f, 1f)),
          new FezVertexPositionTexture(new Vector3(0.0f, (float) (-(double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(0.0f, 1f))
        }, new int[12]{ 0, 1, 2, 0, 2, 5, 5, 2, 3, 5, 3, 4 }, PrimitiveType.TriangleList);
        group.CustomData = (object) new DotHost.RayState();
        group.Material = new Material()
        {
          Diffuse = new Vector3(0.0f)
        };
        group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Forward, RandomHelper.Between(0.0, 6.2831854820251465));
      }
      for (int index = this.TrialRaysMesh.Groups.Count - 1; index >= 0; --index)
      {
        Group group = this.TrialRaysMesh.Groups[index];
        DotHost.RayState customData = group.CustomData as DotHost.RayState;
        customData.Age += elapsedSeconds * 0.15f;
        float num = Easing.EaseOut((double) Easing.EaseOut(Math.Sin((double) customData.Age * 6.2831854820251465 - 1.5707963705062866) * 0.5 + 0.5, EasingType.Quintic), EasingType.Quintic);
        group.Material.Diffuse = Vector3.Lerp(Vector3.One, customData.Tint.ToVector3(), 0.05f) * 0.15f * num;
        float speed = customData.Speed;
        group.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Forward, (float) ((double) elapsedSeconds * (double) speed * (0.10000000149011612 + (double) Easing.EaseIn((double) this.TrialTimeAccumulator / 3.0, EasingType.Quadratic) * 0.20000000298023224)));
        group.Scale = new Vector3((float) ((double) num * 0.75 + 0.25), (float) ((double) num * 0.5 + 0.5), 1f);
        if ((double) customData.Age > 1.0)
          this.TrialRaysMesh.RemoveGroupAt(index);
      }
      this.TrialFlareMesh.Position = this.TrialRaysMesh.Position = this.AoInstance.Position;
      this.TrialFlareMesh.Rotation = this.TrialRaysMesh.Rotation = this.CameraManager.Rotation;
      this.TrialRaysMesh.Scale = new Vector3(Easing.EaseIn((double) this.TrialTimeAccumulator / 2.0, EasingType.Quadratic) + 1f);
      this.TrialFlareMesh.Material.Opacity = (float) (0.125 + (double) Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.TrialTimeAccumulator - 2.0) / 3.0)), EasingType.Cubic) * 0.875);
      this.TrialFlareMesh.Scale = Vector3.One + this.TrialRaysMesh.Scale * Easing.EaseIn((double) Math.Max(this.TrialTimeAccumulator - 2.5f, 0.0f) / 1.5, EasingType.Cubic) * 4f;
    }
    else
    {
      this.MakeRay();
      for (int index = this.TrialRaysMesh.Groups.Count - 1; index >= 0; --index)
      {
        Group group = this.TrialRaysMesh.Groups[index];
        DotHost.RayState customData = group.CustomData as DotHost.RayState;
        customData.Age += elapsedSeconds * 0.15f;
        group.Material.Diffuse = Vector3.One * FezMath.Saturate(customData.Age * 8f);
        group.Scale *= new Vector3(2f, 1f, 1f);
      }
      this.TrialRaysMesh.AlwaysOnTop = false;
      this.TrialRaysMesh.Position = this.AoInstance.Position;
      this.TrialRaysMesh.Rotation = this.CameraManager.Rotation;
      this.TrialRaysMesh.Scale = new Vector3(Easing.EaseIn((double) this.TrialTimeAccumulator / 2.0, EasingType.Quadratic) + 1f);
    }
  }

  private void MakeRay()
  {
    if (this.TrialRaysMesh.Groups.Count >= 150 || !RandomHelper.Probability(0.25))
      return;
    float num = RandomHelper.Probability(0.75) ? 0.1f : 0.5f;
    Group group = this.TrialRaysMesh.AddGroup();
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionColor>(new FezVertexPositionColor[6]
    {
      new FezVertexPositionColor(new Vector3(0.0f, (float) ((double) num / 2.0 * 0.5), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, num / 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, (float) ((double) num / 2.0 * 0.5), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, (float) (-(double) num / 2.0 * 0.5), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, (float) (-(double) num / 2.0), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(0.0f, (float) (-(double) num / 2.0 * 0.5), 0.0f), Color.White)
    }, new int[12]{ 0, 1, 2, 0, 2, 5, 5, 2, 3, 5, 3, 4 }, PrimitiveType.TriangleList);
    group.CustomData = (object) new DotHost.RayState();
    group.Material = new Material()
    {
      Diffuse = new Vector3(0.0f)
    };
    group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, RandomHelper.Between(0.0, -3.1415927410125732)) * Quaternion.CreateFromAxisAngle(Vector3.Forward, RandomHelper.Between(0.0, 6.2831854820251465));
  }

  private void ScheduleFades()
  {
    ServiceHelper.AddComponent((IGameComponent) new ScreenFade(this.Game)
    {
      FromColor = ColorEx.TransparentWhite,
      ToColor = Color.White,
      Duration = 0.5f,
      EasingType = EasingType.Quintic,
      Faded = (Action) (() => ServiceHelper.AddComponent((IGameComponent) new ScreenFade(this.Game)
      {
        FromColor = Color.White,
        ToColor = ColorEx.TransparentWhite,
        Duration = 4f,
        EaseOut = true
      }))
    });
  }

  private void AddSpin()
  {
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective || this.CameraManager.ProjectionTransition)
      return;
    this.DestinationSpinSpeed = Math.Abs(this.DestinationSpinSpeed);
    this.SpinSpeed = Math.Abs(this.SpinSpeed);
    if ((double) this.SpinSpeed < 3.0)
    {
      this.SpinSpeed += 3f;
      this.DestinationSpinSpeed += 4f;
    }
    else
      this.DestinationSpinSpeed *= 3f;
    ++this.currentDroneIndex;
    if (this.currentDroneIndex <= 4)
    {
      this.eHexDrone.FadeOutAndDie(0.5f);
      this.eHexDrone = this.sHexDrones[this.currentDroneIndex].Emit(true, 0.0f, 0.0f);
      Waiters.Interpolate(0.5, (Action<float>) (s =>
      {
        if (this.eHexDrone == null || this.eHexDrone.Dead)
          return;
        this.eHexDrone.VolumeFactor = s;
      }));
    }
    else
      this.currentDroneIndex = 4;
    this.DestinationSpinSpeed *= (float) this.CameraManager.LastViewpoint.GetDistance(this.CameraManager.Viewpoint);
    this.SpinSpeed *= (float) this.CameraManager.LastViewpoint.GetDistance(this.CameraManager.Viewpoint);
  }

  private void AddSplodeBeam(bool fullForce)
  {
    this.AddSplodeBeamInternal(fullForce);
    if (!fullForce)
      return;
    this.AddSplodeBeamInternal(true);
  }

  private void AddSplodeBeamInternal(bool fullForce)
  {
    float num1 = RandomHelper.Between(0.0, 3.1415927410125732);
    float num2 = RandomHelper.Between(0.0, 3.0 / 32.0);
    Group group = this.RaysMesh.AddColoredTriangle(Vector3.Zero, new Vector3(0.0f, (float) Math.Sin((double) num1 - (double) num2) * 55f, (float) Math.Cos((double) num1 - (double) num2) * 55f), new Vector3(0.0f, (float) Math.Sin((double) num1 + (double) num2) * 55f, (float) Math.Cos((double) num1 + (double) num2) * 55f), new Color(241, 23, 101), new Color(37, 22, 53), new Color(37, 22, 53));
    group.Material = new Material() { Opacity = 1f };
    group.CustomData = (object) new EldersHexahedron.RayData()
    {
      Sign = RandomHelper.Sign(),
      Speed = (RandomHelper.Between(0.5, 1.5) * (fullForce ? 6f : 3f))
    };
  }

  private void ScheduleText()
  {
    IWaiter waiter = Waiters.Wait(3.0, (Action) (() =>
    {
      this.GameService.ShowScroll(this.GameState.SaveData.IsNewGamePlus ? (this.GameState.SaveData.HasStereo3D ? "STEREO_INSTRUCTIONS" : "FPVIEW_INSTRUCTIONS") : "ROTATE_INSTRUCTIONS", 0.0f, true, false);
      this.PlayerManager.Action = ActionType.Idle;
    }));
    waiter.CustomPause = (Func<bool>) (() => this.GameState.InCutscene);
    waiter.AutoPause = true;
  }

  private void ScheduleExplode()
  {
    Waiters.Wait((Func<bool>) (() => this.PlayerManager.Grounded), (Action) (() =>
    {
      this.WalkTo.Destination = (Func<Vector3>) (() => this.PlayerManager.Position * Vector3.UnitY + this.Origin * FezMath.XZMask);
      this.WalkTo.NextAction = ActionType.LookingUp;
      this.PlayerManager.Action = ActionType.WalkingTo;
      this.CurrentPhase = EldersHexahedron.Phase.HexaExplode;
      this.SincePhaseStarted = 0.0f;
      if (this.GameState.IsTrialMode)
        Waiters.Wait(1.0, (Action) (() => this.sTrialWhiteOut.Emit()));
      Waiters.Interpolate(0.5, (Action<float>) (s => this.eHexDrone.Pitch = FezMath.Saturate(s)), (Action) (() => this.eHexDrone.FadeOutAndDie(0.1f)));
    })).AutoPause = true;
  }

  private void Talk1()
  {
    Waiters.Interpolate(0.5, (Action<float>) (s => this.eAmbientHex.VolumeFactor = FezMath.Saturate((float) (0.25 * (1.0 - (double) s) + 0.25)) * 0.85f));
    this.Say(0, 4, (Action) (() =>
    {
      Waiters.Interpolate(0.5, (Action<float>) (s => this.eAmbientHex.VolumeFactor = FezMath.Saturate((float) (0.25 * (double) s + 0.25)) * 0.85f));
      this.sNightTransition.Emit();
      this.SincePhaseStarted = 0.0f;
      this.CurrentPhase = EldersHexahedron.Phase.Beam;
      this.eHexaTalk.FadeOutAndDie(0.1f);
      this.eHexaTalk = (SoundEmitter) null;
      this.PlayerManager.CanControl = false;
    }));
  }

  private void Talk2()
  {
    Waiters.Interpolate(0.5, (Action<float>) (s => this.eAmbientHex.VolumeFactor = FezMath.Saturate((float) (0.25 * (1.0 - (double) s) + 0.25)) * 0.85f));
    this.Say(5, 7, (Action) (() =>
    {
      Waiters.Interpolate(0.5, (Action<float>) (s => this.eAmbientHex.VolumeFactor = FezMath.Saturate((float) (0.25 * (1.0 - (double) s))) * 0.85f));
      this.sHexDisappear.Emit();
      this.SincePhaseStarted = 0.0f;
      this.CurrentPhase = EldersHexahedron.Phase.Disappear;
      this.eHexaTalk.FadeOutAndDie(0.1f);
      this.eHexaTalk = (SoundEmitter) null;
      this.PlayerManager.CanControl = false;
    }));
  }

  private void Say(int current, int stopAt, Action onEnded)
  {
    if (this.eHexaTalk == null || this.eHexaTalk.Dead)
      return;
    IWaiter w = Waiters.Wait(0.25, new Action(this.eHexaTalk.Cue.Resume));
    w.AutoPause = true;
    string stringRaw = GameText.GetStringRaw(EldersHexahedron.HexStrings[current]);
    IWaiter w2 = Waiters.Wait(0.25 + 0.10000000149011612 * (double) stringRaw.Length, new Action(this.eHexaTalk.Cue.Pause));
    w2.AutoPause = true;
    this.ArtObjectService.Say(this.AoInstance.Id, stringRaw, true).Ended += (Action) (() =>
    {
      if (w.Alive)
        w.Cancel();
      if (w2.Alive)
      {
        if (this.eHexaTalk != null && !this.eHexaTalk.Dead && this.eHexaTalk.Cue.State != SoundState.Paused)
          this.eHexaTalk.Cue.Pause();
        w2.Cancel();
      }
      if (current == stopAt)
        onEnded();
      else
        this.Say(current + 1, stopAt, onEnded);
    });
  }

  private void Kill() => ServiceHelper.RemoveComponent<EldersHexahedron>(this);

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    if (this.CurrentPhase > EldersHexahedron.Phase.Talk1)
    {
      this.GameState.SkyOpacity = 1f - this.Starfield.Opacity;
      foreach (BackgroundPlane statuePlane in this.StatuePlanes)
        statuePlane.Opacity = Easing.EaseIn(1.0 - (double) this.Starfield.Opacity, EasingType.Quadratic);
      this.SfRenderer.Visible = true;
    }
    switch (this.CurrentPhase)
    {
      case EldersHexahedron.Phase.Beam:
        this.DrawBeamWithMask();
        break;
      case EldersHexahedron.Phase.MatrixSpin:
        this.DrawBeamWithMask();
        this.GraphicsDevice.SetBlendingMode(BlendingMode.Additive);
        this.TargetRenderer.DrawFullscreen(new Color(this.WhiteOutFactor * 0.6f, this.WhiteOutFactor * 0.6f, this.WhiteOutFactor * 0.6f));
        this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
        break;
      case EldersHexahedron.Phase.Talk2:
        this.DrawBeamWithMask();
        break;
      case EldersHexahedron.Phase.Disappear:
        this.DrawBeamWithMask();
        break;
      case EldersHexahedron.Phase.FezBeamGrow:
      case EldersHexahedron.Phase.FezComeDown:
        this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        this.BeamMesh.Draw();
        if (this.GameState.SaveData.IsNewGamePlus)
          this.DealGlassesPlane.Draw();
        this.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        break;
      case EldersHexahedron.Phase.Beamsplode:
        if (this.GameState.SaveData.IsNewGamePlus)
          this.DealGlassesPlane.Draw();
        this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        this.BeamMesh.Draw();
        this.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        if ((double) this.SincePhaseStarted <= 0.25)
          break;
        this.RaysMesh.Draw();
        break;
      case EldersHexahedron.Phase.Yay:
        if (!this.GameState.SaveData.IsNewGamePlus)
          break;
        this.DealGlassesPlane.Draw();
        break;
      case EldersHexahedron.Phase.WaitSpin:
        this.TrialRaysMesh.Draw();
        break;
      case EldersHexahedron.Phase.HexaExplode:
        this.SolidCubes.Draw();
        this.SmallCubes.Draw();
        if (this.GameState.IsTrialMode)
        {
          this.TargetRenderer.DrawFullscreen(new Color(1f, 1f, 1f, FezMath.Saturate(Easing.EaseIn(((double) this.TrialTimeAccumulator - 6.0) / 1.0, EasingType.Quintic))));
          this.TrialFlareMesh.Draw();
        }
        this.TrialRaysMesh.Draw();
        break;
      case EldersHexahedron.Phase.ThatsIt:
        if (!this.GameState.IsTrialMode)
          break;
        this.TargetRenderer.DrawFullscreen(Color.White);
        break;
    }
  }

  private void DrawBeamWithMask()
  {
    this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.LightShaft));
    this.BeamMask.Draw();
    this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.LightShaft);
    this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
    this.BeamMesh.Draw();
    this.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
    this.MatrixMesh.Draw();
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
  }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IPlaneParticleSystems PlaneParticleSystems { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IArtObjectService ArtObjectService { get; set; }

  [ServiceDependency]
  public IGameService GameService { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IInputManager InputManager { get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { get; set; }

  [ServiceDependency(Optional = true)]
  public IWalkToService WalkTo { protected get; set; }

  private enum Phase
  {
    ZoomOut,
    Talk1,
    Beam,
    MatrixSpin,
    Talk2,
    Disappear,
    FezBeamGrow,
    FezComeDown,
    Beamsplode,
    Yay,
    WaitSpin,
    HexaExplode,
    ThatsIt,
  }

  private struct RayData
  {
    public int Sign;
    public float Speed;
    public float SinceAlive;
  }

  private struct ShardProjectionData
  {
    public Vector3 Direction;
    public Quaternion Spin;
  }

  private class StarfieldRenderer : DrawableGameComponent
  {
    private readonly EldersHexahedron Host;

    public StarfieldRenderer(Game game, EldersHexahedron host)
      : base(game)
    {
      this.Host = host;
      this.DrawOrder = 0;
      this.Visible = false;
    }

    public override void Draw(GameTime gameTime)
    {
      this.Host.TargetRenderer.DrawFullscreen(new Color(0.141176477f, 0.0882353f, 0.20588237f, this.Host.Starfield.Opacity));
      this.Host.Starfield.Draw();
    }
  }
}
