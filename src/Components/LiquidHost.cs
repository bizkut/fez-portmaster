// Decompiled with JetBrains decompiler
// Type: FezGame.Components.LiquidHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

public class LiquidHost : DrawableGameComponent
{
  private const int MinShoreSegmentWidth = 10;
  private const int MaxShoreSegmentWidth = 22;
  private const int ShoreThickness = 4;
  private const int ShoreActualTotalWidth = 48 /*0x30*/;
  public static readonly Dictionary<LiquidType, LiquidColorScheme> ColorSchemes = new Dictionary<LiquidType, LiquidColorScheme>((IEqualityComparer<LiquidType>) LiquidTypeComparer.Default)
  {
    {
      LiquidType.Water,
      new LiquidColorScheme()
      {
        LiquidBody = new Color(61, 117, 254),
        SolidOverlay = new Color(40, 76, 162),
        SubmergedFoam = new Color(91, 159, 254),
        EmergedFoam = new Color(175, 205, (int) byte.MaxValue)
      }
    },
    {
      LiquidType.Blood,
      new LiquidColorScheme()
      {
        LiquidBody = new Color(174, 26, 0),
        SolidOverlay = new Color(84, 0, 21),
        SubmergedFoam = new Color(230, 81, 55),
        EmergedFoam = new Color((int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue)
      }
    },
    {
      LiquidType.Sewer,
      new LiquidColorScheme()
      {
        LiquidBody = new Color(82, (int) sbyte.MaxValue, 57),
        SolidOverlay = new Color(32 /*0x20*/, 70, 49),
        SubmergedFoam = new Color(174, 196, 64 /*0x40*/),
        EmergedFoam = new Color(174, 196, 64 /*0x40*/)
      }
    },
    {
      LiquidType.Lava,
      new LiquidColorScheme()
      {
        LiquidBody = new Color(209, 0, 0),
        SolidOverlay = new Color(150, 0, 0),
        SubmergedFoam = new Color((int) byte.MaxValue, 0, 0),
        EmergedFoam = new Color((int) byte.MaxValue, 0, 0)
      }
    },
    {
      LiquidType.Purple,
      new LiquidColorScheme()
      {
        LiquidBody = new Color(194, 1, 171),
        SolidOverlay = new Color(76, 9, 103),
        SubmergedFoam = new Color(247, 52, 223),
        EmergedFoam = new Color(254, 254, 254)
      }
    },
    {
      LiquidType.Green,
      new LiquidColorScheme()
      {
        LiquidBody = new Color(47, (int) byte.MaxValue, 139),
        SolidOverlay = new Color(0, 167, 134),
        SubmergedFoam = new Color(0, 218, 175),
        EmergedFoam = new Color(184, 249, 207)
      }
    }
  };
  private Mesh LiquidMesh;
  private Mesh FoamMesh;
  private Mesh RaysMesh;
  private Mesh CausticsMesh;
  private AnimatedTexture CausticsAnimation;
  private AnimationTiming BackgroundCausticsTiming;
  private float CausticsHeight;
  private PlaneParticleSystem BubbleSystem;
  private PlaneParticleSystem EmbersSystem;
  private SoundEmitter eSewageBubbling;
  private SoundEffect sSmallBubble;
  private SoundEffect sMidBubble;
  private SoundEffect sLargeBubble;
  private AnimatedTexture LargeBubbleAnim;
  private AnimatedTexture MediumBubbleAnim;
  private AnimatedTexture SmallBubbleAnim;
  private AnimatedTexture SmokeAnim;
  private TimeSpan TimeUntilBubble;
  private LiquidType? LastWaterType;
  private LiquidHost.WaterTransitionRenderer TransitionRenderer;
  private FoamEffect FoamEffect;
  private bool WaterVisible;
  private float WaterLevel;
  private Vector3 RightVector;
  private Quaternion CameraRotation;
  private Vector3 ScreenCenter;
  private float CameraRadius;
  private Vector3 CameraPosition;
  private Vector3 CameraInterpolatedCenter;
  private Vector3 ForwardVector;
  private float OriginalPixPerTrix;
  public static LiquidHost Instance;
  private float lastVariation;
  private TimeSpan accumulator;
  private float lastVisibleWaterHeight = -1f;
  public bool ForcedUpdate;
  private float OriginalDistance;

  public LiquidHost(Game game)
    : base(game)
  {
    this.DrawOrder = 50;
    LiquidHost.Instance = this;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LightingPostProcess.DrawOnTopLights += new Action(this.DrawLights);
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.Visible = this.Enabled = false;
    this.lastVisibleWaterHeight = -1f;
  }

  private void TryInitialize()
  {
    this.GameState.WaterBodyColor = LiquidHost.ColorSchemes[LiquidType.Water].LiquidBody.ToVector3();
    this.GameState.WaterFoamColor = LiquidHost.ColorSchemes[LiquidType.Water].EmergedFoam.ToVector3();
    int waterType = (int) this.LevelManager.WaterType;
    LiquidType? lastWaterType = this.LastWaterType;
    int valueOrDefault = (int) lastWaterType.GetValueOrDefault();
    if ((waterType == valueOrDefault ? (lastWaterType.HasValue ? 1 : 0) : 0) != 0)
    {
      this.CreateParticleSystems();
      this.ReestablishLiquidHeight();
      this.ReloadSounds();
      this.ForcedUpdate = true;
      this.Update(new GameTime());
      this.ForcedUpdate = false;
    }
    else
    {
      this.LastWaterType = new LiquidType?(this.LevelManager.WaterType);
      this.Visible = this.Enabled = this.LevelManager.WaterType != 0;
      this.lastVisibleWaterHeight = -1f;
      this.ReestablishLiquidHeight();
      this.ReloadSounds();
      this.CreateFoam();
      this.CreateParticleSystems();
      if (this.Enabled)
      {
        LiquidColorScheme colorScheme = LiquidHost.ColorSchemes[this.LevelManager.WaterType];
        this.GameState.WaterBodyColor = colorScheme.LiquidBody.ToVector3();
        this.GameState.WaterFoamColor = colorScheme.EmergedFoam.ToVector3();
        this.LiquidMesh.Groups[0].Material.Diffuse = colorScheme.LiquidBody.ToVector3();
        this.LiquidMesh.Groups[1].Material.Diffuse = colorScheme.SolidOverlay.ToVector3();
        this.FoamMesh.Groups[0].Material.Diffuse = colorScheme.SubmergedFoam.ToVector3();
        if (this.FoamMesh.Groups.Count > 1)
          this.FoamMesh.Groups[1].Material.Diffuse = colorScheme.EmergedFoam.ToVector3();
      }
      this.ForcedUpdate = true;
      this.Update(new GameTime());
      this.ForcedUpdate = false;
    }
  }

  private void ReloadSounds()
  {
    if (this.LevelManager.WaterType == LiquidType.Sewer)
    {
      Vector3 position = this.CameraManager.InterpolatedCenter * FezMath.XZMask + this.LevelManager.WaterHeight * Vector3.UnitY;
      this.eSewageBubbling = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Sewer/SewageBubbling").EmitAt(position, true);
    }
    else if (this.eSewageBubbling != null && !this.eSewageBubbling.Dead)
      this.eSewageBubbling.Cue.Stop();
    if (this.LevelManager.WaterType == LiquidType.Lava)
    {
      this.sSmallBubble = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Lava/SmallBubble");
      this.sMidBubble = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Lava/MediumBubble");
      this.sLargeBubble = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Lava/LargeBubble");
    }
    else
      this.sSmallBubble = this.sMidBubble = this.sLargeBubble = (SoundEffect) null;
  }

  private void ReestablishLiquidHeight()
  {
    if (this.LevelManager.Name != null)
    {
      if (this.LevelManager.WaterType == LiquidType.Water)
      {
        this.LevelManager.OriginalWaterHeight = this.LevelManager.WaterHeight;
        if (this.GameState.SaveData.GlobalWaterLevelModifier.HasValue)
          this.LevelManager.WaterHeight += this.GameState.SaveData.GlobalWaterLevelModifier.Value;
      }
      else if (!this.GameState.SaveData.ThisLevel.LastStableLiquidHeight.HasValue)
        this.GameState.SaveData.ThisLevel.LastStableLiquidHeight = new float?(this.LevelManager.WaterHeight);
      else
        this.LevelManager.WaterHeight = this.GameState.SaveData.ThisLevel.LastStableLiquidHeight.Value;
    }
    if ((double) this.PlayerManager.Position.Y <= (double) this.LevelManager.WaterHeight)
    {
      float num1 = this.PlayerManager.Position.Y - 1f - this.LevelManager.WaterHeight;
      this.LevelManager.WaterHeight += num1;
      if (this.LevelManager.WaterType == LiquidType.Water && this.GameState.SaveData.GlobalWaterLevelModifier.HasValue)
      {
        SaveData saveData = this.GameState.SaveData;
        float? waterLevelModifier = saveData.GlobalWaterLevelModifier;
        float num2 = num1;
        saveData.GlobalWaterLevelModifier = waterLevelModifier.HasValue ? new float?(waterLevelModifier.GetValueOrDefault() + num2) : new float?();
      }
    }
    if (!(this.LevelManager.Name == "SEWER_START"))
      return;
    if ((double) this.LevelManager.WaterHeight > 21.75)
      this.LevelManager.WaterHeight = 24.5f;
    else
      this.LevelManager.WaterHeight = 19f;
  }

  public void StartTransition()
  {
    ServiceHelper.AddComponent((IGameComponent) (this.TransitionRenderer = new LiquidHost.WaterTransitionRenderer(this.Game, this)));
    this.WaterLevel = this.LevelManager.WaterHeight;
    this.RightVector = this.CameraManager.Viewpoint.RightVector();
  }

  public void LockView() => this.TransitionRenderer.LockView();

  public bool InTransition => this.TransitionRenderer != null;

  public void EndTransition()
  {
    if (this.TransitionRenderer == null)
      return;
    ServiceHelper.RemoveComponent<LiquidHost.WaterTransitionRenderer>(this.TransitionRenderer);
    this.TransitionRenderer = (LiquidHost.WaterTransitionRenderer) null;
    this.ForcedUpdate = true;
    this.Update(new GameTime());
    this.ForcedUpdate = false;
  }

  private void CreateFoam()
  {
    if (this.FoamMesh == null)
    {
      this.FoamMesh = new Mesh()
      {
        AlwaysOnTop = true,
        DepthWrites = false,
        Blending = new BlendingMode?(BlendingMode.Alphablending),
        Culling = CullMode.CullClockwiseFace
      };
      DrawActionScheduler.Schedule((Action) (() => this.FoamMesh.Effect = (BaseEffect) (this.FoamEffect = new FoamEffect())));
    }
    this.FoamMesh.ClearGroups();
    this.RaysMesh.ClearGroups();
    this.FoamMesh.Rotation = Quaternion.Identity;
    this.FoamMesh.Position = Vector3.Zero;
    if (this.LevelManager.WaterType == LiquidType.None)
      return;
    switch (this.LevelManager.WaterType)
    {
      case LiquidType.Water:
      case LiquidType.Blood:
      case LiquidType.Purple:
      case LiquidType.Green:
        if (this.FoamEffect == null)
          DrawActionScheduler.Schedule((Action) (() => this.FoamEffect.IsWobbling = true));
        else
          this.FoamEffect.IsWobbling = true;
        float x1 = (float) RandomHelper.Random.Next(10, 22) / 16f;
        for (float x2 = -24f; (double) x2 < 24.0; x2 += x1)
        {
          Group group = this.FoamMesh.AddFace(new Vector3(1f, 0.125f, 1f), Vector3.Zero, FaceOrientation.Back, Color.White, true);
          group.Position = new Vector3(0.5f, -1f / 16f, 0.0f);
          group.BakeTransform<VertexPositionNormalColor>();
          IndexedUserPrimitives<VertexPositionNormalColor> geometry = group.Geometry as IndexedUserPrimitives<VertexPositionNormalColor>;
          for (int index = 0; index < geometry.Vertices.Length; ++index)
            geometry.Vertices[index].Normal = new Vector3(x2, 0.0f, 0.0f);
          group.Scale = new Vector3(x1, 1f, 1f);
        }
        Group buffer1 = this.FoamMesh.CollapseToBuffer<VertexPositionNormalColor>();
        buffer1.Material = new Material();
        buffer1.Position = new Vector3(0.0f, -1f, 0.0f);
        buffer1.CustomData = (object) true;
        for (float x3 = -24f; (double) x3 < 24.0; x3 += x1)
        {
          Group group = this.FoamMesh.AddFace(new Vector3(1f, 0.125f, 1f), Vector3.Zero, FaceOrientation.Back, Color.White, true);
          group.Position = new Vector3(0.5f, 1f / 16f, 0.0f);
          group.BakeTransform<VertexPositionNormalColor>();
          IndexedUserPrimitives<VertexPositionNormalColor> geometry = group.Geometry as IndexedUserPrimitives<VertexPositionNormalColor>;
          for (int index = 0; index < geometry.Vertices.Length; ++index)
            geometry.Vertices[index].Normal = new Vector3(x3, 0.0f, 0.0f);
          group.Scale = new Vector3(x1, 1f, 1f);
        }
        Group buffer2 = this.FoamMesh.CollapseToBuffer<VertexPositionNormalColor>(1, this.FoamMesh.Groups.Count - 1);
        buffer2.Material = new Material();
        buffer2.Position = new Vector3(0.0f, -1f, 0.0f);
        buffer2.CustomData = (object) false;
        break;
      case LiquidType.Lava:
      case LiquidType.Sewer:
        if (this.FoamEffect == null)
          DrawActionScheduler.Schedule((Action) (() => this.FoamEffect.IsWobbling = false));
        else
          this.FoamEffect.IsWobbling = false;
        Group group1 = this.FoamMesh.AddFace(new Vector3(1f, 0.125f, 1f), Vector3.Zero, FaceOrientation.Back, Color.White, false);
        group1.Position = new Vector3(0.5f, -0.125f, 0.0f);
        group1.BakeTransform<VertexPositionNormalColor>();
        group1.Scale = new Vector3(100f, 0.5f, 1f);
        group1.Position = new Vector3(-100f, -1f, 0.0f);
        group1.Material = new Material();
        group1.CustomData = (object) true;
        break;
    }
  }

  private void CreateParticleSystems()
  {
    if (this.LevelManager.WaterType == LiquidType.None)
      return;
    LiquidColorScheme colorScheme = LiquidHost.ColorSchemes[this.LevelManager.WaterType];
    switch (this.LevelManager.WaterType)
    {
      case LiquidType.Lava:
      case LiquidType.Sewer:
        Color color = new Color(colorScheme.SubmergedFoam.ToVector3() * 0.5f);
        PlaneParticleSystemSettings particleSystemSettings1 = new PlaneParticleSystemSettings();
        particleSystemSettings1.Velocity = (VaryingVector3) new Vector3(0.0f, 0.15f, 0.0f);
        particleSystemSettings1.Gravity = new Vector3(0.0f, 0.0f, 0.0f);
        particleSystemSettings1.SpawningSpeed = 50f;
        particleSystemSettings1.ParticleLifetime = 2.2f;
        particleSystemSettings1.SpawnBatchSize = 1;
        VaryingVector3 varyingVector3_1 = new VaryingVector3();
        varyingVector3_1.Base = new Vector3(1f / 16f);
        varyingVector3_1.Variation = new Vector3(1f / 16f);
        varyingVector3_1.Function = VaryingVector3.Uniform;
        particleSystemSettings1.SizeBirth = varyingVector3_1;
        VaryingVector3 varyingVector3_2 = new VaryingVector3();
        varyingVector3_2.Base = new Vector3(0.125f);
        varyingVector3_2.Variation = new Vector3(0.125f);
        varyingVector3_2.Function = VaryingVector3.Uniform;
        particleSystemSettings1.SizeDeath = varyingVector3_2;
        particleSystemSettings1.FadeInDuration = 0.1f;
        particleSystemSettings1.FadeOutDuration = 0.1f;
        VaryingColor varyingColor = new VaryingColor();
        varyingColor.Base = color;
        varyingColor.Variation = color;
        varyingColor.Function = VaryingColor.Uniform;
        particleSystemSettings1.ColorLife = varyingColor;
        particleSystemSettings1.Texture = this.CMProvider.Global.Load<Texture2D>("Background Planes/white_square");
        particleSystemSettings1.BlendingMode = BlendingMode.Alphablending;
        particleSystemSettings1.Billboarding = true;
        PlaneParticleSystemSettings settings1 = particleSystemSettings1;
        IPlaneParticleSystems planeParticleSystems1 = this.PlaneParticleSystems;
        PlaneParticleSystem planeParticleSystem1 = new PlaneParticleSystem(this.Game, 100, settings1);
        planeParticleSystem1.DrawOrder = this.DrawOrder + 1;
        PlaneParticleSystem planeParticleSystem2 = planeParticleSystem1;
        this.BubbleSystem = planeParticleSystem1;
        PlaneParticleSystem system1 = planeParticleSystem2;
        planeParticleSystems1.Add(system1);
        if (this.LevelManager.WaterType == LiquidType.Sewer)
          break;
        PlaneParticleSystemSettings particleSystemSettings2 = new PlaneParticleSystemSettings();
        VaryingVector3 varyingVector3_3 = new VaryingVector3();
        varyingVector3_3.Variation = new Vector3(1f);
        particleSystemSettings2.Velocity = varyingVector3_3;
        particleSystemSettings2.Gravity = new Vector3(0.0f, 0.01f, 0.0f);
        particleSystemSettings2.SpawningSpeed = 40f;
        particleSystemSettings2.ParticleLifetime = 2f;
        particleSystemSettings2.SpawnBatchSize = 1;
        particleSystemSettings2.RandomizeSpawnTime = true;
        VaryingVector3 varyingVector3_4 = new VaryingVector3();
        varyingVector3_4.Function = LiquidHost.EmberScaling;
        particleSystemSettings2.SizeBirth = varyingVector3_4;
        particleSystemSettings2.FadeInDuration = 0.15f;
        particleSystemSettings2.FadeOutDuration = 0.4f;
        particleSystemSettings2.ColorBirth = (VaryingColor) new Color((int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue, 0);
        particleSystemSettings2.ColorLife.Base = new Color((int) byte.MaxValue, 16 /*0x10*/, 16 /*0x10*/);
        particleSystemSettings2.ColorLife.Variation = new Color(0, 32 /*0x20*/, 32 /*0x20*/);
        particleSystemSettings2.ColorLife.Function = VaryingColor.Uniform;
        particleSystemSettings2.ColorDeath = (VaryingColor) new Color(0, 0, 0, 32 /*0x20*/);
        particleSystemSettings2.Texture = this.CMProvider.Global.Load<Texture2D>("Background Planes/white_square");
        particleSystemSettings2.BlendingMode = BlendingMode.Alphablending;
        particleSystemSettings2.Billboarding = true;
        PlaneParticleSystemSettings settings2 = particleSystemSettings2;
        IPlaneParticleSystems planeParticleSystems2 = this.PlaneParticleSystems;
        PlaneParticleSystem planeParticleSystem3 = new PlaneParticleSystem(this.Game, 50, settings2);
        planeParticleSystem3.DrawOrder = this.DrawOrder + 1;
        PlaneParticleSystem planeParticleSystem4 = planeParticleSystem3;
        this.EmbersSystem = planeParticleSystem3;
        PlaneParticleSystem system2 = planeParticleSystem4;
        planeParticleSystems2.Add(system2);
        break;
    }
  }

  protected override void LoadContent()
  {
    this.LiquidMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false,
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      Culling = CullMode.None
    };
    Group g = this.LiquidMesh.AddColoredBox(Vector3.One, Vector3.Zero, Color.White, true);
    g.Position = new Vector3(0.0f, -0.5f, 0.0f);
    g.BakeTransform<FezVertexPositionColor>();
    g.Position = new Vector3(0.0f, -1f, 0.0f);
    g.Scale = new Vector3(150f);
    g.Material = new Material();
    g = this.LiquidMesh.AddColoredBox(Vector3.One, Vector3.Zero, Color.White, true);
    g.Position = new Vector3(0.0f, -0.5f, 0.0f);
    g.BakeTransform<FezVertexPositionColor>();
    g.Position = new Vector3(0.0f, -1f, 0.0f);
    g.Scale = new Vector3(150f);
    g.Material = new Material();
    this.RaysMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false,
      Blending = new BlendingMode?(BlendingMode.Additive),
      Culling = CullMode.CullClockwiseFace
    };
    this.CausticsAnimation = this.CMProvider.Global.Load<AnimatedTexture>("Other Textures/FINAL_caustics");
    this.CausticsAnimation.Timing.Loop = true;
    this.BackgroundCausticsTiming = this.CausticsAnimation.Timing.Clone();
    this.BackgroundCausticsTiming.RandomizeStep();
    this.CausticsMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false,
      SamplerState = SamplerState.PointWrap
    };
    g = this.CausticsMesh.AddTexturedCylinder(Vector3.One, Vector3.Zero, 3, 4, false, false);
    g.Material = new Material();
    this.SmallBubbleAnim = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/lava/lava_a");
    this.MediumBubbleAnim = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/lava/lava_b");
    this.LargeBubbleAnim = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/lava/lava_c");
    this.SmokeAnim = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/lava/lava_smoke");
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.LiquidMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.RaysMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.CausticsMesh.Effect = (BaseEffect) new CausticsEffect();
      g.Texture = (Texture) this.CausticsAnimation.Texture;
    }));
  }

  public static Func<Vector3, Vector3, Vector3> EmberScaling
  {
    get
    {
      return (Func<Vector3, Vector3, Vector3>) ((b, v) =>
      {
        if (RandomHelper.Probability(0.333))
          return new Vector3(0.125f, 1f / 16f, 1f);
        return RandomHelper.Probability(0.5) ? new Vector3(1f / 16f, 0.125f, 1f) : new Vector3(1f / 16f, 1f / 16f, 1f);
      });
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading && !this.ForcedUpdate && this.TransitionRenderer == null || this.GameState.TimePaused && !this.ForcedUpdate)
      return;
    Matrix inverseView;
    if (this.TransitionRenderer == null)
    {
      this.WaterLevel = this.LevelManager.WaterHeight;
      inverseView = this.CameraManager.InverseView;
      this.RightVector = inverseView.Right;
      this.CameraRotation = this.CameraManager.Rotation;
      this.ScreenCenter = this.CameraManager.Center;
      this.CameraRadius = this.CameraManager.Radius;
      this.CameraPosition = this.CameraManager.Position;
      this.CameraInterpolatedCenter = this.CameraManager.InterpolatedCenter;
      inverseView = this.CameraManager.InverseView;
      this.ForwardVector = inverseView.Forward;
      this.OriginalPixPerTrix = this.CameraManager.PixelsPerTrixel;
    }
    if (this.GameState.FarawaySettings.InTransition && (double) this.GameState.FarawaySettings.OriginFadeOutStep == 1.0 && this.TransitionRenderer != null && !this.TransitionRenderer.ViewLocked)
    {
      this.LockView();
      this.CameraRadius = this.CameraManager.Radius;
      double waterLevel = (double) this.WaterLevel;
      inverseView = this.CameraManager.InverseView;
      double y = (double) inverseView.Translation.Y;
      this.OriginalDistance = (float) (waterLevel - y - 0.625);
    }
    float waterLevel1 = this.WaterLevel;
    if (this.TransitionRenderer != null && (double) this.GameState.FarawaySettings.OriginFadeOutStep == 1.0)
      waterLevel1 = (float) ((double) this.WaterLevel - (double) this.OriginalDistance + (double) this.OriginalDistance * ((double) this.CameraRadius / (double) MathHelper.Lerp(this.CameraRadius, this.GameState.FarawaySettings.DestinationRadius / 4f, this.GameState.FarawaySettings.TransitionStep)));
    this.FoamMesh.Rotation = this.CameraRotation;
    if (this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.WaterType == LiquidType.Sewer)
      this.BubbleSystem.Enabled = this.CameraManager.Viewpoint != Viewpoint.Perspective;
    if (this.LevelManager.WaterType == LiquidType.Lava)
      this.EmbersSystem.Enabled = this.CameraManager.Viewpoint != Viewpoint.Perspective;
    foreach (Group group in this.RaysMesh.Groups)
      group.Rotation = this.CameraRotation;
    float num1 = this.CameraPosition.Y - this.CameraRadius / 2f / this.CameraManager.AspectRatio;
    if (this.WaterVisible || (double) this.lastVisibleWaterHeight < (double) waterLevel1)
      this.lastVisibleWaterHeight = waterLevel1;
    if (this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava)
    {
      this.BubbleSystem.Visible = (double) waterLevel1 > (double) num1;
      this.BubbleSystem.Enabled &= (double) waterLevel1 > (double) num1;
    }
    if (this.LevelManager.WaterType == LiquidType.Lava)
      this.EmbersSystem.Settings.SpawnVolume = new BoundingBox()
      {
        Min = this.ScreenCenter - new Vector3(this.CameraRadius / 2f),
        Max = this.ScreenCenter + new Vector3(this.CameraRadius / 2f / this.CameraManager.AspectRatio)
      };
    if (this.LevelManager.WaterType == LiquidType.Sewer)
      this.eSewageBubbling.Position = this.CameraInterpolatedCenter * FezMath.XZMask + waterLevel1 * Vector3.UnitY;
    this.WaterVisible = (double) this.lastVisibleWaterHeight > (double) num1 || this.TransitionRenderer != null;
    if (!this.WaterVisible && !this.ForcedUpdate)
    {
      if ((this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.WaterType == LiquidType.Sewer) && (double) this.lastVisibleWaterHeight != (double) waterLevel1)
        this.BubbleSystem.Clear();
      if (this.LevelManager.WaterType != LiquidType.Lava || this.GameState.Loading)
        return;
      this.SpawnBubbles(gameTime.ElapsedGameTime, waterLevel1, true);
    }
    else
    {
      this.accumulator += gameTime.ElapsedGameTime;
      if (this.accumulator.TotalSeconds > 6.2831854820251465)
        this.accumulator -= TimeSpan.FromSeconds(6.2831854820251465);
      float num2 = (float) (Math.Sin(this.accumulator.TotalSeconds / 2.0) * 2.0 / 16.0);
      float waterLevel2 = waterLevel1 - this.lastVariation + num2;
      this.lastVariation = num2;
      this.RaysMesh.Position = (waterLevel2 - 0.5f) * Vector3.UnitY;
      this.LiquidMesh.Position = this.ScreenCenter * FezMath.XZMask + (waterLevel2 + 0.5f) * Vector3.UnitY;
      if (this.LevelManager.Sky != null && this.LevelManager.Sky.Name != "Cave")
        this.CausticsMesh.Position = FezMath.XZMask * this.CausticsMesh.Position + (waterLevel2 - 0.5f) * Vector3.UnitY;
      if (this.LevelManager.WaterType.IsWater())
      {
        if ((this.LevelManager.Sky == null ? 0 : (this.LevelManager.Sky.Name == "Cave" ? 1 : 0)) != 0)
          this.BackgroundCausticsTiming.Update(gameTime.ElapsedGameTime, 0.375f);
        this.CausticsAnimation.Timing.Update(gameTime.ElapsedGameTime, 0.875f);
      }
      if (this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.WaterType == LiquidType.Sewer)
      {
        this.BubbleSystem.Settings.Velocity = (VaryingVector3) new Vector3(0.0f, (float) (0.15000000596046448 + (double) this.LevelManager.WaterSpeed * 0.75), 0.0f);
        this.BubbleSystem.Settings.SpawnVolume = new BoundingBox()
        {
          Min = (this.ScreenCenter - new Vector3(this.CameraRadius / 1.5f)) * FezMath.XZMask + (waterLevel2 - 1.8f) * Vector3.UnitY,
          Max = (this.ScreenCenter + new Vector3(this.CameraRadius / 1.5f)) * FezMath.XZMask + (waterLevel2 - 0.8f) * Vector3.UnitY
        };
      }
      switch (this.LevelManager.WaterType)
      {
        case LiquidType.Water:
        case LiquidType.Blood:
        case LiquidType.Purple:
        case LiquidType.Green:
          this.FoamMesh.Position = (waterLevel2 + 0.5f) * Vector3.UnitY + this.CameraInterpolatedCenter * this.ForwardVector;
          if (this.FoamEffect != null)
          {
            this.FoamEffect.TimeAccumulator = (float) this.accumulator.TotalSeconds;
            this.FoamEffect.ScreenCenterSide = this.CameraInterpolatedCenter.Dot(this.RightVector);
            this.FoamEffect.ShoreTotalWidth = 48f;
          }
          if (this.TransitionRenderer == null && RandomHelper.Probability(0.03))
          {
            Vector3 vector3_1 = this.ScreenCenter - this.CameraRadius / 2f * FezMath.XZMask;
            Vector3 vector3_2 = this.ScreenCenter + this.CameraRadius / 2f * FezMath.XZMask;
            Vector3 vector3_3 = new Vector3(RandomHelper.Between((double) vector3_1.X, (double) vector3_2.X), 0.0f, RandomHelper.Between((double) vector3_1.Z, (double) vector3_2.Z));
            float num3 = RandomHelper.Between(0.1, 1.25);
            float num4 = 3f + RandomHelper.Centered(1.0);
            Group group = this.RaysMesh.AddColoredQuad(new Vector3(0.0f, 0.0f, 0.0f), new Vector3(-num4 - num3, -num4, 0.0f), new Vector3(-num4, -num4, 0.0f), new Vector3(-num3, 0.0f, 0.0f), Color.White, Color.Black, Color.Black, Color.White);
            group.CustomData = (object) new LiquidHost.RayCustomData()
            {
              RandomSpeed = RandomHelper.Between(0.5, 1.5)
            };
            group.Material = new Material();
            group.Position = vector3_3;
          }
          for (int index = 0; index < this.RaysMesh.Groups.Count; ++index)
          {
            Group group = this.RaysMesh.Groups[index];
            LiquidHost.RayCustomData customData = (LiquidHost.RayCustomData) group.CustomData;
            if (customData != null)
            {
              customData.AccumulatedTime += gameTime.ElapsedGameTime;
              group.Material.Diffuse = new Vector3(Easing.EaseOut(Math.Sin(customData.AccumulatedTime.TotalSeconds / 5.0 * 3.1415927410125732), EasingType.Quadratic) * 0.2f);
              group.Position += (float) gameTime.ElapsedGameTime.TotalSeconds * this.RightVector * 0.4f * customData.RandomSpeed;
              if (customData.AccumulatedTime.TotalSeconds > 5.0)
              {
                this.RaysMesh.RemoveGroupAt(index);
                --index;
              }
            }
          }
          break;
        case LiquidType.Lava:
          this.FoamMesh.Position = this.LiquidMesh.Position;
          if (!this.ForcedUpdate)
            this.SpawnBubbles(gameTime.ElapsedGameTime, waterLevel2, false);
          if (this.FoamEffect != null)
          {
            this.FoamEffect.ScreenCenterSide = this.CameraInterpolatedCenter.Dot(this.RightVector);
            this.FoamEffect.ShoreTotalWidth = 48f;
            break;
          }
          break;
        case LiquidType.Sewer:
          this.FoamMesh.Position = this.LiquidMesh.Position;
          if (this.FoamEffect != null)
          {
            this.FoamEffect.ScreenCenterSide = this.CameraInterpolatedCenter.Dot(this.RightVector);
            this.FoamEffect.ShoreTotalWidth = 48f;
            break;
          }
          break;
      }
      if (this.LevelManager.WaterType != LiquidType.Lava || (double) this.LevelManager.WaterHeight < 135.5)
        return;
      foreach (TrileInstance key in (IEnumerable<TrileInstance>) this.LevelManager.PickupGroups.Keys)
        key.PhysicsState.IgnoreCollision = true;
    }
  }

  private void SpawnBubbles(TimeSpan elapsed, float waterLevel, bool invisible)
  {
    this.TimeUntilBubble -= elapsed;
    if (this.TimeUntilBubble.TotalSeconds > 0.0)
      return;
    AnimatedTexture animation = RandomHelper.Probability(0.7) ? this.SmallBubbleAnim : (RandomHelper.Probability(0.7) ? this.MediumBubbleAnim : this.LargeBubbleAnim);
    Vector3 position = new Vector3(RandomHelper.Between((double) this.ScreenCenter.X - (double) this.CameraRadius / 2.0, (double) this.ScreenCenter.X + (double) this.CameraRadius / 2.0), (float) ((double) waterLevel + (double) animation.FrameHeight / 32.0 - 0.5 - 1.0 / 16.0), RandomHelper.Between((double) this.ScreenCenter.Z - (double) this.CameraRadius / 2.0, (double) this.ScreenCenter.Z + (double) this.CameraRadius / 2.0));
    if (!invisible)
      this.LevelManager.AddPlane(new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, animation)
      {
        Position = position,
        Rotation = this.CameraRotation * (RandomHelper.Probability(0.5) ? Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f) : Quaternion.Identity),
        Doublesided = true,
        Loop = false,
        Timing = {
          Step = 0.0f
        }
      });
    if (animation == this.SmallBubbleAnim)
      this.sSmallBubble.EmitAt(position).FadeDistance = 20f;
    else if (animation == this.MediumBubbleAnim)
      this.sMidBubble.EmitAt(position).FadeDistance = 20f;
    else
      this.sLargeBubble.EmitAt(position).FadeDistance = 20f;
    if (!invisible && animation != this.SmallBubbleAnim)
      Waiters.Wait(0.1, (Action) (() => this.LevelManager.AddPlane(new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, this.SmokeAnim)
      {
        Position = new Vector3(position.X, (float) ((double) waterLevel + (double) this.SmokeAnim.FrameHeight / 32.0 - 0.5 + 0.125), position.Z) + this.ForwardVector,
        Rotation = this.CameraRotation * (RandomHelper.Probability(0.5) ? Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f) : Quaternion.Identity),
        Doublesided = true,
        Opacity = 0.4f,
        Loop = false,
        Timing = {
          Step = 0.0f
        }
      })));
    this.TimeUntilBubble = TimeSpan.FromSeconds((double) RandomHelper.Between(0.1, 0.4));
  }

  public void DrawLights()
  {
    if (!this.Visible || this.LevelManager.WaterType == LiquidType.None || this.GameState.Loading)
      return;
    LiquidColorScheme colorScheme = LiquidHost.ColorSchemes[this.LevelManager.WaterType];
    Vector3 vector3_1 = new Vector3(0.5f);
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    this.LiquidMesh.AlwaysOnTop = this.CameraManager.Viewpoint != Viewpoint.Perspective && !this.CameraManager.ProjectionTransition;
    this.LiquidMesh.Groups[0].Material.Diffuse = vector3_1;
    this.LiquidMesh.Groups[0].Enabled = true;
    this.LiquidMesh.Groups[1].Enabled = false;
    this.LiquidMesh.Draw();
    this.LiquidMesh.Groups[0].Material.Diffuse = colorScheme.LiquidBody.ToVector3();
    if (this.LevelManager.WaterType.IsWater() && !this.GameState.InFpsMode)
    {
      if (this.LevelManager.WaterType != LiquidType.Water)
      {
        Vector3 vector3_2 = LiquidHost.ColorSchemes[this.LevelManager.WaterType].LiquidBody.ToVector3();
        this.CausticsMesh.FirstGroup.Material.Diffuse = vector3_2 / Math.Max(Math.Max(vector3_2.X, vector3_2.Y), vector3_2.Z);
      }
      else
        this.CausticsMesh.FirstGroup.Material.Diffuse = Vector3.One;
      if (this.LevelManager.Sky != null && this.LevelManager.Sky.Name == "Cave")
      {
        float num1 = this.CameraRadius * 3f;
        this.CausticsMesh.Position = new Vector3((float) ((double) this.LevelManager.Size.X / 2.0 - (double) num1 / 2.0), this.WaterLevel - 0.5f, (float) ((double) this.LevelManager.Size.Z / 2.0 - (double) num1 / 2.0));
        this.CausticsHeight = 12f;
        this.CausticsMesh.Scale = new Vector3(num1, this.CausticsHeight, num1);
        this.CausticsMesh.SamplerState = SamplerState.LinearWrap;
        this.CausticsMesh.Culling = CullMode.CullClockwiseFace;
        float num2 = num1 / (this.CausticsHeight / 2f);
        int width = this.CausticsAnimation.Texture.Width;
        int height = this.CausticsAnimation.Texture.Height;
        int frame1 = this.BackgroundCausticsTiming.Frame;
        Rectangle offset1 = this.CausticsAnimation.Offsets[frame1];
        this.CausticsMesh.TextureMatrix = (Dirtyable<Matrix>) new Matrix(num2 * (float) offset1.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset1.Height / (float) height, 0.0f, 0.0f, (float) ((double) -offset1.Width / (double) width / 4.0) * num2, (float) offset1.Y / (float) height, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
        Rectangle offset2 = this.CausticsAnimation.Offsets[(frame1 + 1) % this.BackgroundCausticsTiming.FrameTimings.Length];
        this.CausticsMesh.CustomData = (object) new Matrix(num2 * (float) offset2.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset2.Height / (float) height, 0.0f, 0.0f, (float) ((double) -offset2.Width / (double) width / 4.0) * num2, (float) offset2.Y / (float) height, this.BackgroundCausticsTiming.NextFrameContribution, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
        this.CausticsMesh.Blending = new BlendingMode?(BlendingMode.Maximum);
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Greater, FezEngine.Structure.StencilMask.Level);
        this.CausticsMesh.Draw();
        Vector3 vector3_3 = this.LevelManager.Size * 1.5f;
        float num3 = Math.Max(vector3_3.X, vector3_3.Z);
        this.CausticsHeight = 3f;
        this.CausticsMesh.Position = new Vector3((float) ((double) vector3_3.X / 1.5 / 2.0 + (double) vector3_3.X * -0.5), this.WaterLevel - 0.5f, (float) ((double) vector3_3.Z / 1.5 / 2.0 + (double) vector3_3.Z * -0.5));
        this.CausticsMesh.Culling = CullMode.CullCounterClockwiseFace;
        this.CausticsMesh.Scale = new Vector3(num3, this.CausticsHeight, num3);
        this.CausticsMesh.Blending = new BlendingMode?();
        this.CausticsMesh.SamplerState = SamplerState.LinearWrap;
        float num4 = num3 / (this.CausticsHeight / 2f);
        int frame2 = this.CausticsAnimation.Timing.Frame;
        Rectangle offset3 = this.CausticsAnimation.Offsets[frame2];
        this.CausticsMesh.TextureMatrix = (Dirtyable<Matrix>) new Matrix(num4 * (float) offset3.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset3.Height / (float) height, 0.0f, 0.0f, (float) (-(double) num4 / 2.0), (float) offset3.Y / (float) height, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
        Rectangle offset4 = this.CausticsAnimation.Offsets[(frame2 + 1) % this.CausticsAnimation.Timing.FrameTimings.Length];
        this.CausticsMesh.CustomData = (object) new Matrix(num4 * (float) offset4.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset4.Height / (float) height, 0.0f, 0.0f, (float) (-(double) num4 / 2.0), (float) offset4.Y / (float) height, this.CausticsAnimation.Timing.NextFrameContribution, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
        this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      }
      else
      {
        this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
        Vector3 vector3_4 = this.LevelManager.Size * 1.5f;
        float num5 = Math.Max(vector3_4.X, vector3_4.Z);
        this.CausticsMesh.SamplerState = SamplerState.LinearWrap;
        this.CausticsHeight = 3f;
        this.CausticsMesh.Scale = new Vector3(num5, this.CausticsHeight, num5);
        float num6 = num5 / (this.CausticsHeight / 2f);
        int width = this.CausticsAnimation.Texture.Width;
        int height = this.CausticsAnimation.Texture.Height;
        int frame = this.CausticsAnimation.Timing.Frame;
        Rectangle offset5 = this.CausticsAnimation.Offsets[frame];
        this.CausticsMesh.TextureMatrix = (Dirtyable<Matrix>) new Matrix(num6 * (float) offset5.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset5.Height / (float) height, 0.0f, 0.0f, (float) (-(double) num6 / 2.0), (float) offset5.Y / (float) height, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
        Rectangle offset6 = this.CausticsAnimation.Offsets[(frame + 1) % this.CausticsAnimation.Timing.FrameTimings.Length];
        this.CausticsMesh.CustomData = (object) new Matrix(num6 * (float) offset6.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset6.Height / (float) height, 0.0f, 0.0f, (float) (-(double) num6 / 2.0), (float) offset6.Y / (float) height, this.CausticsAnimation.Timing.NextFrameContribution, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
      }
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.LessEqual, FezEngine.Structure.StencilMask.Level);
      this.CausticsMesh.Draw();
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    }
    if (this.FoamMesh.Groups.Count <= 1)
      return;
    this.FoamMesh.Groups[0].Enabled = false;
    this.FoamMesh.Groups[1].Material.Diffuse = vector3_1;
    this.FoamMesh.Draw();
    this.FoamMesh.Groups[1].Material.Diffuse = colorScheme.EmergedFoam.ToVector3();
    this.FoamMesh.Groups[0].Enabled = true;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.TransitionRenderer != null || this.GameState.StereoMode)
      return;
    this.DoDraw();
  }

  public void DoDraw(bool skipUnderwater = false)
  {
    bool flag = this.CameraManager.Viewpoint == Viewpoint.Perspective || this.CameraManager.ProjectionTransition;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    LiquidColorScheme colorScheme = LiquidHost.ColorSchemes[this.LevelManager.WaterType];
    Vector3 vector3 = this.LevelManager.WaterType == LiquidType.Sewer || this.GameState.StereoMode ? Vector3.One : this.LevelManager.ActualDiffuse.ToVector3();
    graphicsDevice.GetDssCombiner().StencilEnable = true;
    if (!this.CameraManager.ViewTransitionReached && this.GameState.InFpsMode)
    {
      if (this.CameraManager.Viewpoint.IsOrthographic())
        flag &= (double) this.CameraManager.ViewTransitionStep < 0.87000000476837158;
      else
        flag &= (double) this.CameraManager.ViewTransitionStep > 0.10000000149011612;
    }
    this.LiquidMesh.AlwaysOnTop = !flag;
    graphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
    this.LiquidMesh.Groups[0].Enabled = true;
    this.LiquidMesh.Groups[1].Enabled = false;
    this.LiquidMesh.Groups[0].Material.Diffuse *= vector3;
    this.LiquidMesh.Draw();
    this.LiquidMesh.Groups[0].Material.Diffuse = colorScheme.LiquidBody.ToVector3();
    if (!skipUnderwater)
    {
      graphicsDevice.PrepareStencilRead(CompareFunction.LessEqual, FezEngine.Structure.StencilMask.Level);
      this.LiquidMesh.Groups[0].Enabled = false;
      this.LiquidMesh.Groups[1].Enabled = true;
      if (this.GameState.FarawaySettings.InTransition)
      {
        if ((double) this.GameState.FarawaySettings.TransitionStep < 0.5)
          this.LiquidMesh.Groups[1].Material.Opacity = 1f - this.GameState.FarawaySettings.OriginFadeOutStep;
        else if ((double) this.GameState.FarawaySettings.DestinationCrossfadeStep > 0.0)
          this.LiquidMesh.Groups[1].Material.Opacity = this.GameState.FarawaySettings.DestinationCrossfadeStep;
      }
      else
        this.LiquidMesh.Groups[1].Material.Opacity = 1f;
      this.LiquidMesh.Groups[1].Material.Diffuse *= vector3;
      this.LiquidMesh.Draw();
      this.LiquidMesh.Groups[1].Material.Diffuse = colorScheme.SolidOverlay.ToVector3();
    }
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Water));
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
    this.LiquidMesh.Groups[0].Enabled = true;
    this.LiquidMesh.Draw();
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    float num = 1f;
    if (!this.GameState.FarawaySettings.InTransition)
    {
      if (this.CameraManager.Viewpoint == Viewpoint.Perspective)
        num = 0.0f;
      else if (this.CameraManager.ProjectionTransition && this.CameraManager.Viewpoint == Viewpoint.Perspective)
        num = 1f - this.CameraManager.ViewTransitionStep;
      else if (this.CameraManager.ProjectionTransition && this.CameraManager.Viewpoint.IsOrthographic())
        num = this.CameraManager.ViewTransitionStep;
    }
    this.FoamMesh.Groups[0].Material.Opacity = num;
    if (this.FoamMesh.Groups.Count > 1)
      this.FoamMesh.Groups[1].Material.Opacity = num;
    this.FoamMesh.Groups[0].Material.Diffuse *= vector3;
    if (this.FoamMesh.Groups.Count > 1)
      this.FoamMesh.Groups[1].Material.Diffuse *= vector3;
    if (48.0 < (double) this.CameraManager.Radius)
    {
      this.FoamMesh.Position -= new Vector3(48f, 0.0f, 48f);
      this.FoamMesh.Draw();
      this.FoamMesh.Position += new Vector3(96f, 0.0f, 96f);
      this.FoamMesh.Draw();
      this.FoamMesh.Position -= new Vector3(48f, 0.0f, 48f);
    }
    this.FoamMesh.Draw();
    this.FoamMesh.Groups[0].Material.Diffuse = colorScheme.SubmergedFoam.ToVector3();
    if (this.FoamMesh.Groups.Count > 1)
      this.FoamMesh.Groups[1].Material.Diffuse = colorScheme.EmergedFoam.ToVector3();
    this.RaysMesh.AlwaysOnTop = !flag;
    this.RaysMesh.Draw();
    graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IPlaneParticleSystems PlaneParticleSystems { get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public IDotManager DotManager { get; set; }

  private class RayCustomData
  {
    public TimeSpan AccumulatedTime;
    public float RandomSpeed;
  }

  private class WaterTransitionRenderer : DrawableGameComponent
  {
    private readonly LiquidHost Host;
    public bool ViewLocked;

    public WaterTransitionRenderer(Game game, LiquidHost host)
      : base(game)
    {
      this.DrawOrder = 1001;
      this.Host = host;
    }

    public void LockView()
    {
      (this.Host.LiquidMesh.Effect as DefaultEffect).ForcedViewMatrix = new Matrix?(this.Host.CameraManager.View);
      this.Host.FoamEffect.ForcedViewMatrix = new Matrix?(this.Host.CameraManager.View);
      (this.Host.RaysMesh.Effect as DefaultEffect).ForcedViewMatrix = new Matrix?(this.Host.CameraManager.View);
      (this.Host.CausticsMesh.Effect as CausticsEffect).ForcedViewMatrix = new Matrix?(this.Host.CameraManager.View);
      this.ViewLocked = true;
    }

    public override void Update(GameTime gameTime)
    {
      if (this.EngineState.Loading || this.EngineState.Paused)
        return;
      float num = (float) this.GraphicsDevice.Viewport.Width / (this.CameraManager.PixelsPerTrixel * 16f);
      float viewScale = this.GraphicsDevice.GetViewScale();
      if ((double) this.EngineState.FarawaySettings.OriginFadeOutStep == 1.0)
      {
        float linearStep = (float) (((double) this.EngineState.FarawaySettings.TransitionStep - 0.12749999761581421) / 0.87250000238418579);
        num = MathHelper.Lerp(num, this.EngineState.FarawaySettings.DestinationRadius, Easing.EaseInOut((double) linearStep, EasingType.Sine));
      }
      Matrix orthographic = Matrix.CreateOrthographic(num / viewScale, num / this.CameraManager.AspectRatio / viewScale, this.CameraManager.NearPlane, this.CameraManager.FarPlane);
      (this.Host.LiquidMesh.Effect as DefaultEffect).ForcedProjectionMatrix = new Matrix?(orthographic);
      this.Host.FoamEffect.ForcedProjectionMatrix = new Matrix?(orthographic);
      (this.Host.RaysMesh.Effect as DefaultEffect).ForcedProjectionMatrix = new Matrix?(orthographic);
      (this.Host.CausticsMesh.Effect as CausticsEffect).ForcedProjectionMatrix = new Matrix?(orthographic);
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      (this.Host.LiquidMesh.Effect as DefaultEffect).ForcedViewMatrix = new Matrix?();
      this.Host.FoamEffect.ForcedViewMatrix = new Matrix?();
      (this.Host.RaysMesh.Effect as DefaultEffect).ForcedViewMatrix = new Matrix?();
      (this.Host.CausticsMesh.Effect as CausticsEffect).ForcedViewMatrix = new Matrix?();
      (this.Host.LiquidMesh.Effect as DefaultEffect).ForcedProjectionMatrix = new Matrix?();
      this.Host.FoamEffect.ForcedProjectionMatrix = new Matrix?();
      (this.Host.RaysMesh.Effect as DefaultEffect).ForcedProjectionMatrix = new Matrix?();
      (this.Host.CausticsMesh.Effect as CausticsEffect).ForcedProjectionMatrix = new Matrix?();
      this.Host.ForcedUpdate = true;
      this.Host.Update(new GameTime());
      this.Host.ForcedUpdate = false;
    }

    public override void Draw(GameTime gameTime)
    {
      this.Host.DoDraw(this.EngineState.StereoMode);
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    }

    [ServiceDependency]
    public IEngineStateManager EngineState { get; set; }

    [ServiceDependency]
    public IGameCameraManager CameraManager { get; set; }
  }
}
