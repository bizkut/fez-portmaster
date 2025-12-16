// Decompiled with JetBrains decompiler
// Type: FezGame.Components.IntroZoomIn
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

internal class IntroZoomIn : DrawableGameComponent
{
  private TimeSpan SinceStarted;
  private float Scale;
  private float Opacity;
  private SoundEffect sZoomToHouse;
  private ZoomInEffect zoomInEffect;
  private RenderTargetHandle zoomRtHandle;

  public bool IsDisposed { get; private set; }

  public IntroZoomIn(Game game)
    : base(game)
  {
    this.DrawOrder = 2005;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.GameState.InCutscene = this.GameState.ForceTimePaused = false;
    this.PlayerManager.Action = ActionType.SleepWake;
    this.PlayerManager.LookingDirection = HorizontalDirection.Left;
    this.PlayerManager.Position += this.CameraManager.Viewpoint.RightVector() * -3f / 16f;
    this.sZoomToHouse = this.CMProvider.Get(CM.Intro).Load<SoundEffect>("Sounds/Intro/ZoomToHouse");
    this.sZoomToHouse.Emit();
    this.zoomInEffect = new ZoomInEffect();
    this.zoomRtHandle = this.TargetRenderingManager.TakeTarget();
    this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.zoomRtHandle.Target);
  }

  protected override void Dispose(bool disposing)
  {
    this.IsDisposed = true;
    if (!string.IsNullOrEmpty(this.LevelManager.Name))
    {
      while (!this.PlayerManager.CanControl)
        this.PlayerManager.CanControl = true;
      this.SoundManager.PlayNewSong();
      this.SoundManager.PlayNewAmbience();
      this.SoundManager.MusicVolumeFactor = 1f;
    }
    this.TargetRenderingManager.ReturnTarget(this.zoomRtHandle);
    this.TargetRenderingManager.UnscheduleHook(this.zoomRtHandle.Target);
    this.zoomRtHandle = (RenderTargetHandle) null;
    this.zoomInEffect.Dispose();
    this.zoomInEffect = (ZoomInEffect) null;
    base.Dispose(disposing);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused)
      return;
    this.SinceStarted += gameTime.ElapsedGameTime;
    float num = (float) this.SinceStarted.TotalSeconds / 6f;
    this.Scale = MathHelper.Lerp(200f, 1f, Easing.EaseOut((double) FezMath.Saturate(num), EasingType.Cubic));
    this.Opacity = FezMath.Saturate(num / 0.5f);
    if ((double) num < 1.0)
      return;
    if (this.LevelManager.Name == "GOMEZ_HOUSE_2D" && this.GameState.SaveData.IsNewGamePlus)
      this.GameState.OnHudElementChanged();
    ServiceHelper.RemoveComponent<IntroZoomIn>(this);
  }

  public override void Draw(GameTime gameTime)
  {
    this.TargetRenderingManager.Resolve(this.zoomRtHandle.Target, true);
    this.TargetRenderingManager.DrawFullscreen(Color.Black);
    Matrix matrix1 = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, -0.5f, -0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
    Matrix matrix2 = new Matrix(this.Scale, 0.0f, 0.0f, 0.0f, 0.0f, this.Scale, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
    Matrix matrix3 = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.5f, 0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
    this.GraphicsDevice.SamplerStates[0] = SamplerStates.PointMipWrap;
    this.GraphicsDevice.SetupViewport();
    this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.zoomInEffect, (Texture) this.zoomRtHandle.Target, new Matrix?(matrix1 * matrix2 * matrix3), new Color(1f, 1f, 1f, this.Opacity));
  }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
