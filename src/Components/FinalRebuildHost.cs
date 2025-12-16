// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FinalRebuildHost
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
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class FinalRebuildHost : DrawableGameComponent
{
  private const float ZoomDuration = 10f;
  private const float FlickerDuration = 1.25f;
  private const float SpinFillDuration = 10f;
  private const float Start1Duration = 5f;
  private const float Start2Duration = 4f;
  private const float Start3Duration = 6f;
  private const float SmoothStartDuration = 10f;
  private readonly Vector3[] CubeOffsets;
  private RenderTargetHandle RtHandle;
  private InvertEffect InvertEffect;
  private Mesh SolidCubes;
  private Mesh WhiteCube;
  private Quaternion OriginalCubeRotation;
  private ArtObjectInstance HexahedronAo;
  private NesGlitches Glitches;
  private Mesh RaysMesh;
  private Mesh FlareMesh;
  private SoundEffect sHexAppear;
  private SoundEffect sCubeAppear;
  private SoundEffect sMotorSpin1;
  private SoundEffect sMotorSpin2;
  private SoundEffect sMotorSpinAOK;
  private SoundEffect sMotorSpinCrash;
  private SoundEffect sRayWhiteout;
  private SoundEffect sAku;
  private SoundEffect sAmbientDrone;
  private SoundEffect sZoomIn;
  private SoundEmitter eAku;
  private SoundEmitter eMotor;
  private SoundEmitter eAmbient;
  private FinalRebuildHost.Phases ActivePhase;
  private float PhaseTime;
  private bool FirstUpdate;
  private float lastStep;

  public FinalRebuildHost(Game game)
    : base(game)
  {
    this.CubeOffsets = new Vector3[64 /*0x40*/];
    for (int index1 = 0; index1 < 4; ++index1)
    {
      for (int index2 = 0; index2 < 4; ++index2)
      {
        for (int index3 = 0; index3 < 4; ++index3)
          this.CubeOffsets[index1 * 16 /*0x10*/ + index2 * 4 + index3] = new Vector3((float) index2 - 1.5f, (float) index1 - 1.5f, (float) index3 - 1.5f);
      }
    }
    this.DrawOrder = 750;
    this.Visible = this.Enabled = false;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Destroy();
    this.Visible = this.Enabled = this.LevelManager.Name == "HEX_REBUILD";
    if (!this.Enabled)
      return;
    DefaultCameraManager.NoInterpolation = true;
    this.GameState.HideHUD = true;
    this.CameraManager.ChangeViewpoint(Viewpoint.Right, 0.0f);
    this.PlayerManager.Background = false;
    this.PlayerManager.IgnoreFreefall = true;
    ArtObject artObject = this.CMProvider.CurrentLevel.Load<ArtObject>("Art Objects/NEW_HEXAO");
    int key = IdentifierPool.FirstAvailable<ArtObjectInstance>(this.LevelManager.ArtObjects);
    this.HexahedronAo = new ArtObjectInstance(artObject)
    {
      Id = key
    };
    this.LevelManager.ArtObjects.Add(key, this.HexahedronAo);
    this.HexahedronAo.Initialize();
    this.HexahedronAo.Hidden = true;
    this.WhiteCube = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      DepthWrites = false
    };
    this.WhiteCube.Rotation = this.CameraManager.Rotation * Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.One, Vector3.Zero, Vector3.Up));
    this.WhiteCube.AddColoredBox(new Vector3(4f), Vector3.Zero, Color.White, true);
    this.SolidCubes = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Opaque)
    };
    this.OriginalCubeRotation = this.SolidCubes.Rotation = this.WhiteCube.Rotation;
    ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry1 = this.LevelManager.ActorTriles(ActorType.CubeShard).FirstOrDefault<Trile>().Geometry;
    ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry2 = this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>().Geometry;
    this.sHexAppear = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/HexAppear");
    this.sCubeAppear = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/CubeAppear");
    this.sMotorSpin1 = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/MotorStart1");
    this.sMotorSpin2 = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/MotorStart2");
    this.sMotorSpinAOK = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/MotorStartAOK");
    this.sMotorSpinCrash = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/MotorStartCrash");
    this.sRayWhiteout = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/RayWhiteout");
    this.sAku = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/Aku");
    this.sZoomIn = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/ZoomIn");
    this.sAmbientDrone = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/AmbientDrone");
    for (int index = 0; index < Math.Min(this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes, 64 /*0x40*/); ++index)
    {
      Vector3 cubeOffset = this.CubeOffsets[index];
      ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> indexedPrimitives = index < this.GameState.SaveData.CubeShards ? geometry1 : geometry2;
      Group group = this.SolidCubes.AddGroup();
      group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) indexedPrimitives.Vertices).ToArray<VertexPositionNormalTextureInstance>(), indexedPrimitives.Indices, indexedPrimitives.PrimitiveType);
      group.Position = cubeOffset;
      group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (float) RandomHelper.Random.Next(0, 4) * 1.57079637f);
      group.Enabled = false;
      group.Material = new Material();
    }
    this.RaysMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      DepthWrites = false
    };
    this.FlareMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.FlareMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.WhiteCube.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      Mesh solidCubes = this.SolidCubes;
      solidCubes.Effect = (BaseEffect) new DefaultEffect.LitTextured()
      {
        Specular = true,
        Emissive = 0.5f,
        AlphaIsEmissive = true
      };
      this.SolidCubes.Texture = this.LevelMaterializer.TrilesMesh.Texture;
      this.InvertEffect = new InvertEffect();
      this.RaysMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.FlareMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.FlareMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/flare_alpha");
      ServiceHelper.AddComponent((IGameComponent) (this.Glitches = new NesGlitches(this.Game)));
      this.RtHandle = this.TargetRenderer.TakeTarget();
      this.TargetRenderer.ScheduleHook(this.DrawOrder, this.RtHandle.Target);
    }));
  }

  private void Destroy()
  {
    if (this.Glitches != null)
      ServiceHelper.RemoveComponent<NesGlitches>(this.Glitches);
    this.Glitches = (NesGlitches) null;
    if (this.RtHandle != null)
    {
      this.TargetRenderer.UnscheduleHook(this.RtHandle.Target);
      this.TargetRenderer.ReturnTarget(this.RtHandle);
    }
    this.RtHandle = (RenderTargetHandle) null;
    if (this.SolidCubes != null)
      this.SolidCubes.Dispose();
    this.SolidCubes = (Mesh) null;
    if (this.WhiteCube != null)
    {
      DefaultCameraManager.NoInterpolation = false;
      this.PlayerManager.IgnoreFreefall = false;
      this.WhiteCube.Dispose();
      this.WhiteCube = (Mesh) null;
    }
    if (this.RaysMesh != null)
      this.RaysMesh.Dispose();
    if (this.FlareMesh != null)
      this.FlareMesh.Dispose();
    this.RaysMesh = this.FlareMesh = (Mesh) null;
    if (this.InvertEffect != null)
      this.InvertEffect.Dispose();
    this.InvertEffect = (InvertEffect) null;
    this.HexahedronAo = (ArtObjectInstance) null;
    this.FirstUpdate = true;
    this.sAmbientDrone = this.sAku = this.sZoomIn = this.sHexAppear = this.sCubeAppear = this.sMotorSpin1 = this.sMotorSpin2 = this.sMotorSpinAOK = this.sMotorSpinCrash = this.sRayWhiteout = (SoundEffect) null;
    this.eAku = this.eAmbient = this.eMotor = (SoundEmitter) null;
    this.ActivePhase = FinalRebuildHost.Phases.ZoomInNega;
    this.PhaseTime = 0.0f;
    this.GameState.SkipRendering = false;
    this.GameState.HideHUD = false;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    if (this.FirstUpdate)
    {
      gameTime = new GameTime();
      this.FirstUpdate = false;
    }
    this.PhaseTime += (float) gameTime.ElapsedGameTime.TotalSeconds;
    switch (this.ActivePhase)
    {
      case FinalRebuildHost.Phases.ZoomInNega:
        this.GameState.SkipRendering = true;
        if (gameTime.ElapsedGameTime.Ticks == 0L)
        {
          this.Glitches.ActiveGlitches = 0;
          this.Glitches.FreezeProbability = 0.0f;
          this.CameraManager.PixelsPerTrixel = 0.5f;
          this.CameraManager.SnapInterpolation();
          this.PlayerManager.Position = Vector3.Zero;
          this.PlayerManager.LookingDirection = HorizontalDirection.Right;
          this.SetHexVisible(false);
          this.CollisionManager.GravityFactor = 1f;
          this.sZoomIn.Emit();
          this.eAmbient = this.sAmbientDrone.Emit(true, 0.0f, 0.0f);
        }
        float amount = Easing.EaseIn((double) FezMath.Saturate(this.PhaseTime / 10f), EasingType.Sine);
        if ((double) this.PhaseTime > 0.25)
          this.CameraManager.Radius *= MathHelper.Lerp(0.99f, 1f, amount);
        this.PlayerManager.Action = (double) this.PhaseTime > 7.0 ? ActionType.StandWinking : ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        float num1 = Easing.EaseIn((double) FezMath.Saturate(this.PhaseTime / 11f), EasingType.Sine);
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 0.125f, 0.0f) + new Vector3(0.0f, (float) (Math.Sin((double) this.PhaseTime) * 0.25 * (1.0 - (double) num1)), 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        this.eAmbient.VolumeFactor = num1;
        if ((double) this.PhaseTime <= 11.0)
          break;
        Waiters.Wait(0.75, (Action) (() => this.sHexAppear.Emit())).AutoPause = true;
        this.ChangePhase();
        break;
      case FinalRebuildHost.Phases.FlickerIn:
        this.GameState.SkipRendering = true;
        if (gameTime.ElapsedGameTime.Ticks == 0L)
        {
          this.WhiteCube.Material.Diffuse = Vector3.Zero;
          this.WhiteCube.Rotation = this.OriginalCubeRotation;
          this.CameraManager.PixelsPerTrixel = 3f;
          this.CameraManager.SnapInterpolation();
          this.PlayerManager.Position = Vector3.Zero;
          if (this.eAmbient != null)
            this.eAmbient.VolumeFactor = 0.625f;
          this.PhaseTime = -1f;
        }
        this.PlayerManager.Action = ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 4.5f, 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        this.WhiteCube.Position = this.PlayerManager.Position + new Vector3(0.0f, 6f, 0.0f);
        if ((double) this.PhaseTime <= 2.25)
          break;
        this.ChangePhase();
        break;
      case FinalRebuildHost.Phases.SpinFill:
        this.GameState.SkipRendering = true;
        if (gameTime.ElapsedGameTime.Ticks == 0L)
        {
          this.CameraManager.PixelsPerTrixel = 3f;
          this.CameraManager.SnapInterpolation();
          this.PlayerManager.Position = Vector3.Zero;
          this.WhiteCube.Material.Diffuse = Vector3.One;
          for (int index = 0; index < this.SolidCubes.Groups.Count; ++index)
            this.SolidCubes.Groups[index].CustomData = (object) null;
        }
        this.PlayerManager.Action = ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 4.5f, 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        float num2 = Easing.EaseInOut((double) FezMath.Saturate(this.PhaseTime / 11f), EasingType.Sine);
        this.SolidCubes.Position = this.WhiteCube.Position = this.PlayerManager.Position + new Vector3(0.0f, 6f, 0.0f);
        this.SolidCubes.Rotation = this.WhiteCube.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((double) num2 * 6.2831854820251465 * 3.0)) * this.OriginalCubeRotation;
        float num3 = Easing.EaseInOut((double) FezMath.Saturate(this.PhaseTime / 10f), EasingType.Quadratic);
        float pitch = MathHelper.Clamp((float) (((double) num3 - (double) this.lastStep) * 200.0 - 0.20000000298023224), -1f, 1f);
        float num4 = 1f / (float) this.SolidCubes.Groups.Count;
        for (int index = 0; index < this.SolidCubes.Groups.Count; ++index)
        {
          float num5 = (float) index / (float) this.SolidCubes.Groups.Count;
          float p = Easing.EaseIn((double) FezMath.Saturate((num3 - num5) / num4), EasingType.Sine);
          if ((double) p == 1.0)
          {
            this.SolidCubes.Groups[index].Material.Diffuse = Vector3.One;
            this.SolidCubes.Groups[index].Enabled = true;
          }
          else if ((double) p == 0.0)
          {
            this.SolidCubes.Groups[index].Enabled = false;
          }
          else
          {
            if ((double) p > 0.125 && this.SolidCubes.Groups[index].CustomData == null)
            {
              this.sCubeAppear.Emit(pitch);
              this.SolidCubes.Groups[index].CustomData = (object) true;
            }
            this.SolidCubes.Groups[index].Material.Diffuse = new Vector3((float) RandomHelper.Probability((double) p).AsNumeric(), (float) RandomHelper.Probability((double) p).AsNumeric(), (float) RandomHelper.Probability((double) p).AsNumeric());
            this.SolidCubes.Groups[index].Enabled = RandomHelper.Probability((double) p);
          }
        }
        this.lastStep = num3;
        if ((double) this.PhaseTime <= 12.0)
          break;
        this.eMotor = this.sMotorSpin1.Emit();
        this.eAku = this.sAku.Emit(true, 0.0f, 0.0f);
        this.lastStep = 0.0f;
        this.ChangePhase();
        break;
      case FinalRebuildHost.Phases.MotorStart1:
        this.GameState.SkipRendering = true;
        if (gameTime.ElapsedGameTime.Ticks == 0L)
        {
          this.HexahedronAo.Position = this.SolidCubes.Position;
          this.lastStep = 0.0f;
          for (int index = 0; index < this.SolidCubes.Groups.Count; ++index)
          {
            this.SolidCubes.Groups[index].Enabled = true;
            this.SolidCubes.Groups[index].Material.Diffuse = Vector3.One;
          }
          this.SolidCubes.Rotation = Quaternion.Identity;
          this.SolidCubes.Position = Vector3.Zero;
          this.SolidCubes.CollapseToBufferWithNormal<VertexPositionNormalTextureInstance>();
          this.SolidCubes.Position = this.PlayerManager.Position + new Vector3(0.0f, 6f, 0.0f);
        }
        this.PlayerManager.Action = ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 4.5f, 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        float num6 = Easing.EaseIn((double) Easing.EaseOut((double) FezMath.Saturate(this.PhaseTime / 5f), EasingType.Sine), EasingType.Sextic);
        this.SetHexVisible((double) num6 - (double) this.lastStep > 0.00825);
        this.lastStep = num6;
        this.SolidCubes.Rotation = this.WhiteCube.Rotation = this.HexahedronAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((double) num6 * 6.2831854820251465 * 4.0)) * this.OriginalCubeRotation;
        if ((double) this.PhaseTime <= 5.0)
          break;
        this.eMotor = this.sMotorSpin2.Emit();
        this.ChangePhase();
        break;
      case FinalRebuildHost.Phases.MotorStart2:
        this.GameState.SkipRendering = true;
        if (gameTime.ElapsedGameTime.Ticks == 0L)
        {
          this.HexahedronAo.Position = this.SolidCubes.Position;
          this.lastStep = 0.0f;
        }
        this.PlayerManager.Action = ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 4.5f, 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        float num7 = Easing.EaseIn((double) Easing.EaseOut((double) FezMath.Saturate(this.PhaseTime / 4f), EasingType.Sine), EasingType.Sextic);
        float num8 = num7 - this.lastStep;
        this.SetHexVisible((double) num8 > 0.01);
        this.lastStep = num7;
        if (this.GameState.SaveData.SecretCubes + this.GameState.SaveData.CubeShards < 64 /*0x40*/)
        {
          this.Glitches.DisappearProbability = 0.05f;
          float num9 = Easing.EaseIn((double) num8 / 0.0099999997764825821, EasingType.Quartic);
          this.Glitches.ActiveGlitches = FezMath.Round((double) num9 * 7.0 + (double) (int) RandomHelper.Between(0.0, (double) num9 * 10.0));
          this.Glitches.FreezeProbability = (float) ((double) num8 / 0.0099999997764825821 * (1.0 / 400.0));
        }
        this.SolidCubes.Rotation = this.WhiteCube.Rotation = this.HexahedronAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((double) num7 * 6.2831854820251465 * 5.0)) * this.OriginalCubeRotation;
        if ((double) this.PhaseTime <= 4.0 + (this.GameState.SaveData.SecretCubes + this.GameState.SaveData.CubeShards >= 64 /*0x40*/ ? 0.5 : 0.0))
          break;
        if (this.GameState.SaveData.SecretCubes + this.GameState.SaveData.CubeShards < 64 /*0x40*/)
        {
          this.eMotor = this.sMotorSpinCrash.Emit();
          this.ChangePhase();
          break;
        }
        this.eMotor = this.sMotorSpinAOK.Emit();
        this.ChangePhaseTo(FinalRebuildHost.Phases.SmoothStart);
        break;
      case FinalRebuildHost.Phases.MotorStart3:
        this.GameState.SkipRendering = true;
        if (gameTime.ElapsedGameTime.Ticks == 0L)
          this.HexahedronAo.Position = this.SolidCubes.Position;
        this.PlayerManager.Action = ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 4.5f, 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        float num10 = Easing.EaseIn((double) this.PhaseTime / 6.0, EasingType.Sextic);
        float num11 = Math.Min(num10 - this.lastStep, 0.05f);
        this.SetHexVisible((double) num11 > 1.0 / 80.0);
        this.lastStep = num10;
        this.Glitches.DisappearProbability = 0.0375f;
        float num12 = Easing.EaseIn((double) num11 / 0.037500001490116119, EasingType.Quartic);
        this.Glitches.ActiveGlitches = FezMath.Round((double) FezMath.Saturate(num12) * 500.0 + (double) (int) RandomHelper.Between(0.0, (double) FezMath.Saturate(num12) * 250.0));
        this.Glitches.FreezeProbability = Easing.EaseIn((double) num11 / 0.05000000074505806, EasingType.Quadratic) * 0.15f;
        this.SolidCubes.Rotation = this.WhiteCube.Rotation = this.HexahedronAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((double) num10 * 6.2831854820251465 * 20.0)) * this.OriginalCubeRotation;
        if ((double) this.PhaseTime <= 8.0)
          break;
        this.ChangePhase();
        break;
      case FinalRebuildHost.Phases.Crash:
        this.Glitches.FreezeProbability = 1f;
        if ((double) this.PhaseTime <= 2.0)
          break;
        if (this.eAku != null)
          this.eAku.FadeOutAndDie(0.0f);
        if (this.eAmbient != null)
          this.eAmbient.FadeOutAndDie(0.0f, false);
        this.Glitches.ActiveGlitches = 0;
        this.Glitches.FreezeProbability = 0.0f;
        this.Glitches.DisappearProbability = 0.0f;
        this.GlitchReboot();
        break;
      case FinalRebuildHost.Phases.SmoothStart:
        this.GameState.SkipRendering = true;
        if (gameTime.ElapsedGameTime.Ticks == 0L)
        {
          this.HexahedronAo.Position = this.SolidCubes.Position;
          this.lastStep = 0.0f;
          for (int index = 0; index < this.SolidCubes.Groups.Count; ++index)
            this.SolidCubes.Groups[index].Enabled = true;
        }
        this.PlayerManager.Action = ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 4.5f, 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        float num13 = Easing.EaseInOut((double) FezMath.Saturate(this.PhaseTime / 10f), EasingType.Quadratic);
        this.SetHexVisible((double) num13 > 0.42500001192092896);
        this.lastStep = num13;
        this.SolidCubes.Rotation = this.WhiteCube.Rotation = this.HexahedronAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((double) num13 * 6.2831854820251465 * 18.0)) * this.OriginalCubeRotation;
        if ((double) this.PhaseTime <= 10.0)
          break;
        this.eAku.FadeOutAndDie(2f);
        this.sRayWhiteout.Emit();
        this.ChangePhase();
        break;
      case FinalRebuildHost.Phases.ShineReboot:
        this.GameState.SkipRendering = true;
        TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
        if (elapsedGameTime.Ticks == 0L)
        {
          this.HexahedronAo.Position = this.SolidCubes.Position;
          this.SetHexVisible(true);
          this.RaysMesh.ClearGroups();
          this.HexahedronAo.Rotation = this.OriginalCubeRotation;
          if (this.eAmbient != null)
            this.eAmbient.FadeOutAndDie(0.0f, false);
        }
        this.PlayerManager.Action = ActionType.Standing;
        this.PlayerManager.Velocity = Vector3.Zero;
        this.CameraManager.Center = this.PlayerManager.Position + new Vector3(0.0f, 4.5f, 0.0f);
        this.CameraManager.SnapInterpolation();
        this.GameState.SkipRendering = false;
        elapsedGameTime = gameTime.ElapsedGameTime;
        this.UpdateRays((float) elapsedGameTime.TotalSeconds);
        if ((double) this.PhaseTime <= 4.0)
          break;
        this.SmoothReboot();
        break;
    }
  }

  private void GlitchReboot()
  {
    ServiceHelper.AddComponent((IGameComponent) new Reboot(this.Game, "GOMEZ_HOUSE_END_32"));
    Waiters.Wait(0.10000000149011612, (Action) (() =>
    {
      this.Destroy();
      this.Enabled = this.Visible = false;
    }));
    this.Enabled = false;
  }

  private void SmoothReboot()
  {
    ServiceHelper.AddComponent((IGameComponent) new Intro(this.Game)
    {
      Fake = true,
      FakeLevel = "GOMEZ_HOUSE_END_64",
      Glitch = false
    });
    Waiters.Wait(0.10000000149011612, (Action) (() =>
    {
      this.Destroy();
      this.Enabled = this.Visible = false;
    }));
    this.Enabled = false;
  }

  private void SetHexVisible(bool visible)
  {
    if (this.eAku != null)
      this.eAku.VolumeFactor = visible ? 1f : 0.0f;
    if (this.eMotor != null)
      this.eMotor.VolumeFactor = visible ? 0.0f : 1f;
    if (this.eAmbient != null)
      this.eAmbient.VolumeFactor = visible ? 0.0f : 0.625f;
    this.HexahedronAo.Hidden = !visible;
    this.HexahedronAo.Visible = visible;
    this.HexahedronAo.ArtObject.Group.Enabled = visible;
  }

  private void ChangePhase()
  {
    this.PhaseTime = 0.0f;
    ++this.ActivePhase;
    this.Update(new GameTime());
  }

  private void ChangePhaseTo(FinalRebuildHost.Phases phase)
  {
    this.PhaseTime = 0.0f;
    this.ActivePhase = phase;
    this.Update(new GameTime());
  }

  private void UpdateRays(float elapsedSeconds)
  {
    int num = (double) this.PhaseTime > 1.5 ? 1 : 0;
    this.MakeRay();
    if (num != 0)
      this.MakeRay();
    for (int index = this.RaysMesh.Groups.Count - 1; index >= 0; --index)
    {
      Group group = this.RaysMesh.Groups[index];
      DotHost.RayState customData = group.CustomData as DotHost.RayState;
      customData.Age += elapsedSeconds * 0.15f;
      group.Material.Diffuse = Vector3.One * FezMath.Saturate(customData.Age * 8f);
      group.Scale *= new Vector3(1.5f, 1f, 1f);
      if ((double) customData.Age > 1.0)
        this.RaysMesh.RemoveGroupAt(index);
    }
    this.RaysMesh.AlwaysOnTop = false;
    this.FlareMesh.Position = this.RaysMesh.Position = this.HexahedronAo.Position;
    this.FlareMesh.Rotation = this.RaysMesh.Rotation = this.CameraManager.Rotation;
    this.FlareMesh.Material.Opacity = Easing.EaseIn((double) FezMath.Saturate(this.PhaseTime / 2.5f), EasingType.Cubic);
    this.FlareMesh.Scale = Vector3.One + this.RaysMesh.Scale * Easing.EaseIn(((double) this.PhaseTime - 0.25) / 1.75, EasingType.Decic) * 4f;
  }

  private void MakeRay()
  {
    if (this.RaysMesh.Groups.Count >= 150 || !RandomHelper.Probability(0.1 + (double) Easing.EaseIn((double) FezMath.Saturate(this.PhaseTime / 1.75f), EasingType.Sine) * 0.9))
      return;
    float num = RandomHelper.Probability(0.75) ? 0.1f : 0.4f;
    Group group = this.RaysMesh.AddGroup();
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

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
    {
      if (this.RtHandle == null || !this.TargetRenderer.IsHooked(this.RtHandle.Target))
        return;
      this.TargetRenderer.Resolve(this.RtHandle.Target, true);
      this.TargetRenderer.DrawFullscreen(Color.Black);
    }
    else
    {
      switch (this.ActivePhase)
      {
        case FinalRebuildHost.Phases.ZoomInNega:
          this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, 1f - Easing.EaseOut((double) FezMath.Saturate(this.PhaseTime / 10f), EasingType.Quadratic)));
          this.TargetRenderer.Resolve(this.RtHandle.Target, true);
          this.TargetRenderer.DrawFullscreen((BaseEffect) this.InvertEffect, (Texture) this.RtHandle.Target);
          break;
        case FinalRebuildHost.Phases.FlickerIn:
          float p = Easing.EaseIn((double) FezMath.Saturate(this.PhaseTime / 1.25f), EasingType.Quadratic);
          if (RandomHelper.Probability(0.5))
            this.WhiteCube.Material.Diffuse = new Vector3((float) RandomHelper.Probability((double) p).AsNumeric(), (float) RandomHelper.Probability((double) p).AsNumeric(), (float) RandomHelper.Probability((double) p).AsNumeric());
          this.WhiteCube.Enabled = true;
          this.WhiteCube.Draw();
          this.TargetRenderer.Resolve(this.RtHandle.Target, true);
          this.TargetRenderer.DrawFullscreen((BaseEffect) this.InvertEffect, (Texture) this.RtHandle.Target);
          break;
        case FinalRebuildHost.Phases.SpinFill:
          this.WhiteCube.Draw();
          this.SolidCubes.Draw();
          this.TargetRenderer.Resolve(this.RtHandle.Target, true);
          this.TargetRenderer.DrawFullscreen((BaseEffect) this.InvertEffect, (Texture) this.RtHandle.Target);
          break;
        case FinalRebuildHost.Phases.MotorStart1:
        case FinalRebuildHost.Phases.MotorStart2:
        case FinalRebuildHost.Phases.MotorStart3:
        case FinalRebuildHost.Phases.SmoothStart:
          if (!this.HexahedronAo.Visible)
          {
            this.WhiteCube.Draw();
            this.SolidCubes.Draw();
            if (this.TargetRenderer.IsHooked(this.RtHandle.Target))
            {
              this.TargetRenderer.Resolve(this.RtHandle.Target, true);
              this.TargetRenderer.DrawFullscreen((BaseEffect) this.InvertEffect, (Texture) this.RtHandle.Target);
              break;
            }
            this.TargetRenderer.ScheduleHook(this.DrawOrder, this.RtHandle.Target);
            break;
          }
          if (!this.TargetRenderer.IsHooked(this.RtHandle.Target))
            break;
          this.TargetRenderer.Resolve(this.RtHandle.Target, false);
          this.TargetRenderer.DrawFullscreen((Texture) this.RtHandle.Target);
          break;
        case FinalRebuildHost.Phases.ShineReboot:
          this.RaysMesh.Draw();
          this.FlareMesh.Draw();
          break;
      }
    }
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { get; set; }

  private enum Phases
  {
    ZoomInNega,
    FlickerIn,
    SpinFill,
    MotorStart1,
    MotorStart2,
    MotorStart3,
    Crash,
    SmoothStart,
    ShineReboot,
  }
}
