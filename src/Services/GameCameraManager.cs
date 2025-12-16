// Decompiled with JetBrains decompiler
// Type: FezGame.Services.GameCameraManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Services;

public class GameCameraManager(Game game) : 
  DefaultCameraManager(game),
  IGameCameraManager,
  IDefaultCameraManager,
  ICameraProvider
{
  private static readonly float FirstPersonFov = MathHelper.ToRadians(75f);
  private int concurrentChanges;
  private float originalCarriedPhi;
  private bool shouldRotateInstance;

  public override float InterpolationSpeed
  {
    get
    {
      return (float) ((double) base.InterpolationSpeed * (1.5 + (double) Math.Abs(this.CollisionManager.GravityFactor) * 0.5) / 2.0);
    }
    set => base.InterpolationSpeed = value;
  }

  public void CancelViewTransition()
  {
    this.directionTransition = (Vector3SplineInterpolation) null;
    this.viewpoint = this.lastViewpoint;
    this.current = this.predefinedViews[this.viewpoint];
    this.current.Direction = -this.viewpoint.ForwardVector();
    this.ViewTransitionCancelled = true;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += (Action) (() => this.lastViewpoint = this.viewpoint);
  }

  public override bool ChangeViewpoint(Viewpoint newView, float speedFactor)
  {
    if ((double) speedFactor != 0.0 && (newView == this.viewpoint || this.concurrentChanges >= 1) || this.PlayerManager.Action == ActionType.GrabTombstone && !this.PlayerManager.Animation.Timing.Ended)
      return false;
    if (!this.ViewTransitionReached && (double) speedFactor != 0.0)
      ++this.concurrentChanges;
    this.shouldRotateInstance = newView.IsOrthographic() && this.viewpoint.IsOrthographic();
    if (newView == Viewpoint.Perspective)
      this.predefinedViews[newView].Direction = this.current.Direction;
    base.ChangeViewpoint(newView, speedFactor);
    if (this.PlayerManager.CarriedInstance != null && this.concurrentChanges == 0)
      this.originalCarriedPhi = this.PlayerManager.CarriedInstance.Phi;
    this.CameraService.OnRotate();
    return true;
  }

  public void RecordNewCarriedInstancePhi()
  {
    this.PlayerManager.CarriedInstance.Phi = FezMath.SnapPhi(this.PlayerManager.CarriedInstance.Phi);
    TrileMaterializer trileMaterializer = this.LevelMaterializer.GetTrileMaterializer(this.PlayerManager.CarriedInstance.VisualTrile);
    trileMaterializer.UpdateInstance(this.PlayerManager.CarriedInstance);
    trileMaterializer.CommitBatch();
    this.originalCarriedPhi = this.PlayerManager.CarriedInstance.Phi;
    this.shouldRotateInstance = false;
  }

  protected override void PostUpdate()
  {
    if (this.GameState.Loading)
      return;
    if (this.concurrentChanges == 1 && (double) this.ViewTransitionStep > 0.8)
      this.concurrentChanges = 0;
    if (this.ViewTransitionReached)
      this.concurrentChanges = 0;
    if (this.PlayerManager.CarriedInstance == null || !this.shouldRotateInstance)
      return;
    this.PlayerManager.CarriedInstance.Phi = FezMath.WrapAngle(this.originalCarriedPhi + (FezMath.WrapAngle((float) Math.Atan2((double) this.LastViewpoint.ForwardVector().Z, (double) this.LastViewpoint.ForwardVector().X)) - FezMath.WrapAngle((float) (3.1415927410125732 - Math.Atan2((double) this.View.Forward.Z, (double) this.View.Forward.X)))));
    TrileMaterializer trileMaterializer = this.LevelMaterializer.GetTrileMaterializer(this.PlayerManager.CarriedInstance.VisualTrile);
    trileMaterializer.UpdateInstance(this.PlayerManager.CarriedInstance);
    trileMaterializer.CommitBatch();
  }

  protected override void DollyZoom()
  {
    float viewScale = this.GraphicsDeviceService.GraphicsDevice.GetViewScale();
    if (!this.GameState.InFpsMode)
    {
      base.DollyZoom();
    }
    else
    {
      bool flag = this.viewpoint.IsOrthographic();
      float num1 = (double) this.directionTransition.TotalStep == 0.0 ? 1f / 1000f : this.directionTransition.TotalStep;
      if ((double) num1 == 1.0 && !this.directionTransition.Reached)
        num1 -= 1f / 1000f;
      float firstPersonFov = GameCameraManager.FirstPersonFov;
      float num2 = MathHelper.Lerp(flag ? firstPersonFov : 0.0f, flag ? 0.0f : firstPersonFov, num1);
      float num3 = this.radiusBeforeTransition;
      if (this.DollyZoomOut)
        num3 = this.radiusBeforeTransition + (float) ((1.0 - (double) Easing.EaseIn((double) num1, EasingType.Quadratic)) * 15.0);
      float num4 = (float) ((double) num3 / (double) this.AspectRatio / (2.0 * Math.Tan((double) num2 / 2.0))) / viewScale;
      if (this.directionTransition.Reached)
      {
        this.ProjectionTransition = false;
        if (!flag)
        {
          this.predefinedViews[this.lastViewpoint].Direction = -this.lastViewpoint.ForwardVector();
          this.current.Radius = 0.1f;
        }
        else
        {
          this.current.Radius = this.radiusBeforeTransition;
          this.NearPlane = 0.25f;
          this.FarPlane = 500f;
          this.GameState.InFpsMode = false;
        }
        this.FogManager.Density = this.LevelManager.Sky == null ? 0.0f : this.LevelManager.Sky.FogDensity;
        this.DollyZoomOut = false;
        this.RebuildProjection();
        this.SnapInterpolation();
      }
      else
      {
        this.FogManager.Density = (this.LevelManager.Sky == null ? 0.0f : this.LevelManager.Sky.FogDensity) * Easing.EaseIn(flag ? 1.0 - (double) num1 : (double) num1, EasingType.Quadratic);
        float num5 = (float) ((double) num4 * (flag ? (double) num1 : 1.0 - (double) num1) + 0.10000000149011612);
        this.NearPlane = Math.Max(0.25f, 0.25f + num5 - num3);
        this.FarPlane = Math.Max(num5 + this.NearPlane, 499.75f);
        this.FieldOfView = num2;
        this.projection = Matrix.CreatePerspectiveFieldOfView(this.FieldOfView, this.AspectRatio, this.NearPlane, this.FarPlane);
        this.OnProjectionChanged();
        this.current.Radius = num5;
        this.view = Matrix.CreateLookAt(this.current.Radius * this.current.Direction + this.current.Center, this.current.Center, Vector3.UnitY);
        this.OnViewChanged();
      }
    }
  }

  public override bool ActionRunning
  {
    get
    {
      return Fez.LongScreenshot || base.ActionRunning || this.PlayerManager.Action == ActionType.PivotTombstone || this.PlayerManager.Action == ActionType.GrabTombstone;
    }
  }

  public Viewpoint RequestedViewpoint { get; set; }

  public Vector3 OriginalDirection { get; set; }

  public override float Radius
  {
    get => !this.GameState.MenuCubeIsZoomed ? base.Radius : 18f;
    set => base.Radius = value;
  }

  [ServiceDependency]
  public IGraphicsDeviceService GraphicsDeviceService { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ICameraService CameraService { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }
}
