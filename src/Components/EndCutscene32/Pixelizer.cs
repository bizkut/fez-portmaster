// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene32.Pixelizer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components.EndCutscene32;

internal class Pixelizer : DrawableGameComponent
{
  private const float ZoomStepDuration = 3f;
  private readonly EndCutscene32Host Host;
  private RenderTarget2D LowResRT;
  private Mesh GoMesh;
  private Group GomezGroup;
  private Group FezGroup;
  private float TotalTime;
  private float StepTime;
  private Pixelizer.State ActiveState;
  private int ZoomStep;
  private float InitialRadius;
  private float OldSfxVol;

  public Pixelizer(Game game, EndCutscene32Host host)
    : base(game)
  {
    this.Host = host;
    this.DrawOrder = 1000;
    this.UpdateOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.GoMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh goMesh = this.GoMesh;
      goMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        Fullbright = true
      };
      this.GoMesh.Effect.ForcedViewMatrix = new Matrix?(new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, -249.95f, 1f));
      this.GoMesh.Effect.ForcedProjectionMatrix = new Matrix?(new Matrix(0.2f, 0.0f, 0.0f, 0.0f, 0.0f, 0.3555556f, 0.0f, 0.0f, 0.0f, 0.0f, -0.0020004f, 0.0f, 0.0f, 0.0f, -0.00020004f, 1f));
    }));
    this.GomezGroup = this.GoMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, Color.White, true, false, false);
    this.FezGroup = this.GoMesh.AddFace(Vector3.One / 2f, Vector3.Zero, FaceOrientation.Front, Color.Red, true, false, false);
    this.LevelManager.ActualAmbient = new Color(0.25f, 0.25f, 0.25f);
    this.LevelManager.ActualDiffuse = Color.White;
    this.OldSfxVol = this.SoundManager.SoundEffectVolume;
    this.Reset();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.LowResRT.Dispose();
    this.LowResRT = (RenderTarget2D) null;
    if (this.GoMesh != null)
      this.GoMesh.Dispose();
    this.GoMesh = (Mesh) null;
  }

  private void Reset()
  {
    this.CameraManager.PixelsPerTrixel = 2f;
    this.CameraManager.SnapInterpolation();
    this.InitialRadius = this.CameraManager.Radius;
    this.GomezGroup.Position = Vector3.Zero;
    this.FezGroup.Position = new Vector3(-0.25f, 0.75f, 0.0f);
    this.GoMesh.Scale = Vector3.One;
    this.FezGroup.Rotation = this.GomezGroup.Rotation = Quaternion.Identity;
    this.CameraManager.Center = Vector3.Zero;
    this.CameraManager.Direction = Vector3.UnitZ;
    this.CameraManager.Radius = 10f;
    this.ZoomStep = 1;
    this.TotalTime = 0.0f;
    this.StepTime = 0.0f;
    this.RescaleRT();
  }

  private void RescaleRT()
  {
    if (this.LowResRT != null)
    {
      this.TargetRenderer.UnscheduleHook(this.LowResRT);
      this.LowResRT.Dispose();
    }
    float viewHScale = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    float viewVScale = (float) this.GraphicsDevice.Viewport.Height / (720f * this.GraphicsDevice.GetViewScale());
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.LowResRT = new RenderTarget2D(this.GraphicsDevice, FezMath.Round((double) (1280 /*0x0500*/ / this.ZoomStep) * (double) viewHScale), FezMath.Round((double) (720 / this.ZoomStep) * (double) viewVScale), false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8);
      if (this.ActiveState != Pixelizer.State.Zooming)
        return;
      this.TargetRenderer.ScheduleHook(this.DrawOrder, this.LowResRT);
    }));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.StepTime += totalSeconds;
    float num = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    if (this.ActiveState == Pixelizer.State.Wait)
    {
      this.CameraManager.Center = this.PlayerManager.Center + new Vector3(0.0f, 2f, 0.0f);
      if ((double) this.StepTime <= 5.0)
        return;
      this.ChangeState();
    }
    else
    {
      if (this.ActiveState != Pixelizer.State.Zooming)
        return;
      this.TotalTime += totalSeconds;
      this.PlayerManager.FullBright = true;
      if ((double) this.StepTime > 3.0 / Math.Max(Math.Pow((double) this.ZoomStep / 10.0, 2.0), 1.0))
      {
        this.RescaleRT();
        ++this.ZoomStep;
        this.StepTime = 0.0f;
      }
      this.CameraManager.Radius = MathHelper.Lerp(this.InitialRadius, 6f * this.GraphicsDevice.GetViewScale() * num, Easing.EaseIn((double) Easing.EaseOut((double) FezMath.Saturate(this.TotalTime / 57f), EasingType.Sine), EasingType.Quadratic));
      this.CameraManager.Center = Vector3.Lerp(this.PlayerManager.Center + new Vector3(0.0f, 2f, 0.0f), this.PlayerManager.Center, Easing.EaseOut((double) FezMath.Saturate(this.TotalTime / 57f), EasingType.Sine));
      if ((double) this.TotalTime <= 57.0)
        return;
      this.ChangeState();
    }
  }

  private void ChangeState()
  {
    if (this.ActiveState == Pixelizer.State.Wait)
      this.TargetRenderer.ScheduleHook(this.DrawOrder, this.LowResRT);
    if (this.ActiveState == Pixelizer.State.Zooming)
    {
      this.TargetRenderer.UnscheduleHook(this.LowResRT);
      this.SoundManager.KillSounds();
      this.SoundManager.SoundEffectVolume = this.OldSfxVol;
      this.Host.Cycle();
    }
    else
    {
      this.StepTime = 0.0f;
      ++this.ActiveState;
      this.Update(new GameTime());
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.ActiveState == Pixelizer.State.Wait || this.GameState.Loading)
      return;
    if (this.GameState.Paused)
    {
      if (!this.TargetRenderer.IsHooked(this.LowResRT))
        return;
      this.TargetRenderer.Resolve(this.LowResRT, true);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Opaque);
      this.TargetRenderer.DrawFullscreen((Texture) this.LowResRT);
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    }
    else
    {
      GraphicsDevice graphicsDevice = this.GraphicsDevice;
      graphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.Gomez);
      Vector3 vector3 = EndCutscene32Host.PurpleBlack.ToVector3();
      graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      this.TargetRenderer.DrawFullscreen(new Color(vector3.X, vector3.Y, vector3.Z, Easing.EaseIn((double) Easing.EaseOut((double) FezMath.Saturate(this.TotalTime / 57f), EasingType.Sine), EasingType.Quartic)));
      graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
      this.SoundManager.SoundEffectVolume = (float) (1.0 - (double) Easing.EaseIn((double) FezMath.Saturate(this.TotalTime / 57f), EasingType.Quadratic) * 0.89999997615814209);
      if ((double) this.TotalTime > 54.0 && (double) this.TotalTime < 57.0)
      {
        this.PlayerManager.Hidden = true;
        this.GoMesh.Draw();
      }
      if (this.TargetRenderer.IsHooked(this.LowResRT))
      {
        this.TargetRenderer.Resolve(this.LowResRT, true);
        this.GraphicsDevice.Clear(Color.Black);
        this.GraphicsDevice.SetupViewport();
        this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
        graphicsDevice.SetBlendingMode(BlendingMode.Opaque);
        this.TargetRenderer.DrawFullscreen((Texture) this.LowResRT);
        graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      }
      if ((double) this.TotalTime <= 57.0)
        return;
      this.PlayerManager.Hidden = true;
      this.GoMesh.Draw();
    }
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  private enum State
  {
    Wait,
    Zooming,
  }
}
