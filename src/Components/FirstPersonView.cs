// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FirstPersonView
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

internal class FirstPersonView : DrawableGameComponent
{
  private float GomezOpacityOrigin;
  private float GomezOpacityDest;
  private Vector3 CenterOrigin;
  private Vector3 CenterDest;
  private FishEyeEffect fishEyeEffect;
  private RenderTargetHandle fishEyeRT;

  public FirstPersonView(Game game)
    : base(game)
  {
    this.UpdateOrder = -10;
    this.DrawOrder = 199;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    DrawActionScheduler.Schedule((Action) (() => this.fishEyeEffect = new FishEyeEffect()));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading || this.GameState.InMap || this.GameState.InMenuCube || Fez.PublicDemo || this.GameState.FarawaySettings.InTransition || !this.GameState.SaveData.HasFPView)
      return;
    if (!this.PlayerManager.Grounded)
    {
      this.UnDotize();
    }
    else
    {
      if ((this.PlayerManager.Action.IsIdle() || this.PlayerManager.Action == ActionType.Sliding) && !this.GameState.InCutscene && !this.CameraManager.ProjectionTransition && this.CameraManager.ViewTransitionReached && !this.LevelManager.Flat && this.PlayerManager.CanControl && EndCutscene32Host.Instance == null && EndCutscene64Host.Instance == null)
      {
        if (this.InputManager.FpsToggle == FezButtonState.Pressed)
        {
          if (this.GameState.InFpsMode)
          {
            this.CameraManager.Direction = -this.CameraManager.LastViewpoint.ForwardVector();
            this.CameraManager.ChangeViewpoint(this.CameraManager.LastViewpoint);
            this.CenterDest = this.CenterOrigin;
            this.CenterOrigin = this.PlayerManager.Center + Vector3.UnitY * 0.3f;
            this.GomezOpacityOrigin = 0.0f;
            this.GomezOpacityDest = 1f;
          }
          else
          {
            this.GameState.InFpsMode = true;
            this.fishEyeRT = this.TRM.TakeTarget();
            this.TRM.ScheduleHook(this.DrawOrder, this.fishEyeRT.Target);
            this.UnDotize();
            this.CameraManager.ChangeViewpoint(Viewpoint.Perspective);
            this.CenterOrigin = this.CameraManager.Center;
            this.CenterDest = this.PlayerManager.Center + Vector3.UnitY * 0.3f;
            this.GomezOpacityOrigin = 1f;
            this.GomezOpacityDest = 0.0f;
          }
        }
        if (this.CameraManager.Viewpoint == Viewpoint.Perspective && (this.InputManager.CancelTalk == FezButtonState.Pressed || this.InputManager.Back == FezButtonState.Pressed || this.InputManager.Start == FezButtonState.Pressed || this.InputManager.Jump == FezButtonState.Pressed))
        {
          this.InputManager.PressedToDown();
          this.CameraManager.Direction = -this.CameraManager.LastViewpoint.ForwardVector();
          this.CameraManager.ChangeViewpoint(this.CameraManager.LastViewpoint);
          this.CenterDest = this.CenterOrigin;
          this.CenterOrigin = this.PlayerManager.Center + Vector3.UnitY * 0.3f;
          this.GomezOpacityOrigin = 0.0f;
          this.GomezOpacityDest = 1f;
        }
      }
      if (this.GameState.InFpsMode && this.CameraManager.ProjectionTransition)
      {
        this.CameraManager.Center = Vector3.Lerp(this.CenterOrigin, this.CenterDest, this.CameraManager.ViewTransitionStep);
        this.PlayerManager.GomezOpacity = MathHelper.Lerp(this.GomezOpacityOrigin, this.GomezOpacityDest, this.CameraManager.ViewTransitionStep);
        this.fishEyeEffect.Intensity = this.CameraManager.ViewTransitionStep;
      }
      if (!this.GameState.InFpsMode || this.CameraManager.Viewpoint != Viewpoint.Perspective || this.CameraManager.ProjectionTransition)
        return;
      this.CameraManager.Center = this.PlayerManager.Center + Vector3.UnitY * 0.3f;
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.fishEyeRT == null || !this.TRM.IsHooked(this.fishEyeRT.Target))
      return;
    this.TRM.Resolve(this.fishEyeRT.Target, this.CameraManager.Viewpoint == Viewpoint.Perspective);
    this.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);
    this.GraphicsDevice.SetupViewport();
    this.TRM.DrawFullscreen((BaseEffect) this.fishEyeEffect, (Texture) this.fishEyeRT.Target);
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective)
      return;
    this.TRM.ReturnTarget(this.fishEyeRT);
    this.fishEyeRT = (RenderTargetHandle) null;
  }

  private void UnDotize()
  {
    if (this.DotManager.Owner != this)
      return;
    this.DotManager.Burrow();
  }

  [ServiceDependency]
  public ITargetRenderingManager TRM { private get; set; }

  [ServiceDependency]
  public IDotManager DotManager { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }
}
