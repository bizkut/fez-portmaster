// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GomezHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

public class GomezHost : DrawableGameComponent
{
  private const float InvincibilityBlinkSpeed = 5f;
  private GomezEffect effect;
  public Vector3 InterpolatedPosition;
  public static GomezHost Instance;
  private TimeSpan sinceBackgroundChanged;
  private bool lastBackground;
  private bool lastHideFez;
  private AnimatedTexture lastAnimation;
  private readonly FezGame.Services.PlayerManager dummyPlayer = new FezGame.Services.PlayerManager();

  public Mesh PlayerMesh { get; private set; }

  public GomezHost(Game game)
    : base(game)
  {
    this.PlayerMesh = new Mesh()
    {
      SamplerState = SamplerState.PointClamp
    };
    this.UpdateOrder = 11;
    this.DrawOrder = 9;
    ServiceHelper.InjectServices((object) this.dummyPlayer);
    GomezHost.Instance = this;
  }

  public override void Initialize()
  {
    this.PlayerMesh.AddFace(new Vector3(1f), new Vector3(0.0f, 0.25f, 0.0f), FaceOrientation.Front, true, true);
    this.PlayerManager.MeshHost = this;
    this.LevelManager.LevelChanged += (Action) (() => this.effect.ColorSwapMode = this.LevelManager.WaterType == LiquidType.Sewer ? ColorSwapMode.Gameboy : (this.LevelManager.WaterType == LiquidType.Lava ? ColorSwapMode.VirtualBoy : (this.LevelManager.BlinkingAlpha ? ColorSwapMode.Cmyk : ColorSwapMode.None)));
    this.LightingPostProcess.DrawGeometryLights += new Action<GameTime>(this.PreDraw);
    TimeInterpolation.RegisterCallback(new Action<GameTime>(this.InterpolatePosition), 25);
    base.Initialize();
  }

  protected override void LoadContent()
  {
    DrawActionScheduler.Schedule((Action) (() => this.PlayerMesh.Effect = (BaseEffect) (this.effect = new GomezEffect())));
  }

  private Vector3 GetPositionOffset(IPlayerManager playerManager)
  {
    if (this.lastAnimation == null)
      return Vector3.Zero;
    Vector3 vector3_1 = (float) ((1.0 - ((double) playerManager.Size.Y + (playerManager.CarriedInstance != null || playerManager.Action == ActionType.ThrowingHeavy ? -2.0 : 0.0))) / 2.0) * Vector3.UnitY;
    Vector2 vector2 = playerManager.Action.GetOffset() / 16f;
    vector2.Y -= this.lastAnimation.PotOffset.Y / 64f;
    Viewpoint view = this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning ? this.CameraManager.Viewpoint : this.CameraManager.LastViewpoint;
    Vector3 vector3_2 = vector2.X * view.RightVector() * (float) playerManager.LookingDirection.Sign() + vector2.Y * Vector3.UnitY;
    return vector3_1 + vector3_2;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.InMap || this.PlayerManager.Animation == null)
      return;
    if (this.lastAnimation != this.PlayerManager.Animation)
    {
      this.effect.Animation = (Texture) this.PlayerManager.Animation.Texture;
      this.lastAnimation = this.PlayerManager.Animation;
    }
    int width = this.lastAnimation.Texture.Width;
    int height = this.lastAnimation.Texture.Height;
    Rectangle offset = this.lastAnimation.Offsets[this.lastAnimation.Timing.Frame];
    this.PlayerMesh.FirstGroup.TextureMatrix.Set(new Matrix?(new Matrix((float) offset.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset.Height / (float) height, 0.0f, 0.0f, (float) offset.X / (float) width, (float) offset.Y / (float) height, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)));
    if (this.lastBackground != this.PlayerManager.Background && !this.PlayerManager.Action.NoBackgroundDarkening())
    {
      this.sinceBackgroundChanged = TimeSpan.Zero;
      this.lastBackground = this.PlayerManager.Background;
      if (!this.LevelManager.LowPass && EndCutscene32Host.Instance == null && EndCutscene64Host.Instance == null)
        this.SoundManager.FadeFrequencies(this.PlayerManager.Background);
    }
    if (this.sinceBackgroundChanged.TotalSeconds < 1.0)
      this.sinceBackgroundChanged += gameTime.ElapsedGameTime;
    this.effect.Background = this.PlayerManager.Action.NoBackgroundDarkening() ? 0.0f : FezMath.Saturate(this.PlayerManager.Background ? (float) (this.sinceBackgroundChanged.TotalSeconds * 2.0) : (float) (1.0 - this.sinceBackgroundChanged.TotalSeconds * 2.0));
    this.PlayerMesh.Scale = new Vector3((float) this.PlayerManager.Animation.FrameWidth / 16f, (float) this.PlayerManager.Animation.FrameHeight / 16f * (float) Math.Sign(this.CollisionManager.GravityFactor), 1f);
    this.PlayerMesh.Position = this.PlayerManager.Position + this.GetPositionOffset(this.PlayerManager);
    this.InterpolatedPosition = this.PlayerManager.Position;
    bool flag = this.PlayerManager.HideFez && !this.GameState.SaveData.IsNewGamePlus && !this.PlayerManager.Animation.NoHat && !this.PlayerManager.Action.IsCarry();
    if (this.lastHideFez == flag)
      return;
    this.lastHideFez = flag;
    this.effect.NoMoreFez = this.lastHideFez;
  }

  public void InterpolatePosition(GameTime gameTime)
  {
    if (this.GameState.Loading || this.PlayerManager.Hidden || this.GameState.InMap || FezMath.AlmostEqual(this.PlayerManager.GomezOpacity, 0.0f))
    {
      this.InterpolatedPosition = this.PlayerManager.Position;
    }
    else
    {
      if (!TimeInterpolation.NeedsInterpolation || DefaultCameraManager.NoInterpolation || !this.CameraManager.ViewTransitionReached || !this.CameraManager.Viewpoint.IsOrthographic())
        return;
      this.PlayerManager.CopyTo((IPlayerManager) this.dummyPlayer);
      this.dummyPlayer.Velocity += (float) (3.1500000953674316 * (double) this.CollisionManager.GravityFactor * 0.15000000596046448 * gameTime.ElapsedGameTime.TotalSeconds) * -Vector3.UnitY;
      this.PhysicsManager.Update((IComplexPhysicsEntity) this.dummyPlayer);
      TimeSpan timeSpan = gameTime.TotalGameTime - TimeInterpolation.LastUpdate;
      double totalSeconds1 = timeSpan.TotalSeconds;
      timeSpan = TimeInterpolation.UpdateTimestep;
      double totalSeconds2 = timeSpan.TotalSeconds;
      double amount = totalSeconds1 / totalSeconds2;
      Vector3 position1 = this.PlayerManager.Position;
      Vector3 positionOffset1 = this.GetPositionOffset(this.PlayerManager);
      Vector3 vector3_1 = position1 + positionOffset1;
      Vector3 position2 = this.dummyPlayer.Position;
      Vector3 positionOffset2 = this.GetPositionOffset((IPlayerManager) this.dummyPlayer);
      Vector3 vector3_2 = position2 + positionOffset2;
      this.PlayerMesh.Position = Vector3.Lerp(vector3_1, vector3_2, (float) amount);
      this.InterpolatedPosition = Vector3.Lerp(position1, position2, (float) amount);
    }
  }

  private void PreDraw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.PlayerManager.Hidden || this.GameState.InFpsMode)
      return;
    this.effect.Pass = LightingEffectPass.Pre;
    if (!this.PlayerManager.FullBright)
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    else
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    this.PlayerMesh.Draw();
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    this.effect.Pass = LightingEffectPass.Main;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.StereoMode && !this.GameState.FarawaySettings.InTransition)
      return;
    this.DoDraw_Internal(gameTime);
  }

  public void DoDraw_Internal(GameTime gameTime)
  {
    if (this.GameState.Loading || this.PlayerManager.Hidden || this.GameState.InMap || FezMath.AlmostEqual(this.PlayerManager.GomezOpacity, 0.0f))
      return;
    if (this.GameState.StereoMode || this.LevelManager.Quantum)
    {
      this.PlayerMesh.Rotation = this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.LastViewpoint == Viewpoint.None ? this.CameraManager.Rotation : Quaternion.CreateFromAxisAngle(Vector3.UnitY, this.CameraManager.LastViewpoint.ToPhi());
      if (this.PlayerManager.LookingDirection == HorizontalDirection.Left)
        this.PlayerMesh.Rotation *= FezMath.QuaternionFromPhi(3.14159274f);
    }
    this.PlayerMesh.Material.Opacity = this.PlayerManager.Action == ActionType.Suffering || this.PlayerManager.Action == ActionType.Sinking ? (float) FezMath.Saturate((Math.Sin((double) this.PlayerManager.BlinkSpeed * 6.2831854820251465 * 5.0) + 0.5 - (double) this.PlayerManager.BlinkSpeed * 1.25) * 2.0) : this.PlayerManager.GomezOpacity;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    if (!this.PlayerManager.Action.SkipSilhouette())
    {
      graphicsDevice.PrepareStencilRead(CompareFunction.Greater, FezEngine.Structure.StencilMask.NoSilhouette);
      this.PlayerMesh.DepthWrites = false;
      this.PlayerMesh.AlwaysOnTop = true;
      this.effect.Silhouette = true;
      this.PlayerMesh.Draw();
    }
    if (!this.PlayerManager.Background)
    {
      graphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.Hole);
      this.PlayerMesh.AlwaysOnTop = true;
      this.PlayerMesh.DepthWrites = false;
      this.effect.Silhouette = false;
      this.PlayerMesh.Draw();
    }
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Gomez));
    this.PlayerMesh.AlwaysOnTop = this.PlayerManager.Action.NeedsAlwaysOnTop();
    this.PlayerMesh.DepthWrites = !this.GameState.InFpsMode;
    this.effect.Silhouette = false;
    this.PlayerMesh.Draw();
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public IPhysicsManager PhysicsManager { private get; set; }
}
