// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.DefaultCameraManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public class DefaultCameraManager : CameraManager, IDefaultCameraManager, ICameraProvider
{
  public const float TrixelsPerTrile = 16f;
  protected const float TransitionTiltFactor = 0.0f;
  public static float TransitionSpeed = 0.45f;
  public static bool NoInterpolation;
  protected static readonly float DefaultFov = MathHelper.ToRadians(45f);
  protected const float DefaultNearPlane = 0.25f;
  protected const float DefaultFarPlane = 500f;
  protected readonly Dictionary<Viewpoint, DefaultCameraManager.PredefinedView> predefinedViews;
  protected DefaultCameraManager.PredefinedView current = new DefaultCameraManager.PredefinedView();
  protected float defaultViewableWidth = 26.666666f;
  protected readonly DefaultCameraManager.PredefinedView interpolated = new DefaultCameraManager.PredefinedView();
  protected Vector3SplineInterpolation directionTransition;
  protected Viewpoint viewpoint;
  protected Viewpoint lastViewpoint;
  protected Viewpoint olderViewpoint;
  protected bool viewNewlyReached;
  protected bool projNewlyReached;
  protected bool projReached;
  protected float radiusBeforeTransition;
  private GameTime dummyGt = new GameTime();
  private bool transitionNewlyReached;
  private Vector3 viewOffset;
  private float pixelsPerTrixel;
  private bool constrained;

  public virtual float InterpolationSpeed { get; set; }

  public bool ViewTransitionCancelled { get; protected set; }

  public bool ForceTransition { get; set; }

  public Matrix InverseView { get; private set; }

  public event Action ViewpointChanged = new Action(Util.NullAction);

  public event Action PreViewpointChanged = new Action(Util.NullAction);

  protected DefaultCameraManager(Game game)
    : base(game)
  {
    this.predefinedViews = new Dictionary<Viewpoint, DefaultCameraManager.PredefinedView>((IEqualityComparer<Viewpoint>) ViewpointComparer.Default);
    this.FieldOfView = DefaultCameraManager.DefaultFov;
    this.NearPlane = 0.25f;
    this.FarPlane = 500f;
    this.Frustum = new BoundingFrustum(Matrix.Identity);
  }

  public override void Initialize()
  {
    this.InterpolationSpeed = 10f;
    this.ResetViewpoints();
    this.viewpoint = Viewpoint.Right;
    this.current = this.predefinedViews[Viewpoint.Right];
    this.SnapInterpolation();
    this.GraphicsService.DeviceReset += (EventHandler<EventArgs>) ((_param1, _param2) =>
    {
      this.PixelsPerTrixel = this.pixelsPerTrixel;
      this.RebuildProjection();
    });
    TimeInterpolation.RegisterCallback(new Action<GameTime>(this.InterpolationCallback), 100);
  }

  private void RebuildFrustum() => this.Frustum.Matrix = this.view * this.projection;

  public bool ChangeViewpoint(Viewpoint newView) => this.ChangeViewpoint(newView, 1f);

  public virtual bool ChangeViewpoint(Viewpoint newViewpoint, float speedFactor)
  {
    bool flag = newViewpoint.IsOrthographic() != this.viewpoint.IsOrthographic();
    if (flag && this.ProjectionTransition)
      return false;
    this.ProjectionTransition = flag && (double) speedFactor > 0.0;
    this.radiusBeforeTransition = this.viewpoint.IsOrthographic() ? this.current.Radius : this.predefinedViews[this.lastViewpoint].Radius;
    if ((double) speedFactor > 0.0)
    {
      float num = (float) ((double) (Math.Abs(newViewpoint.GetDistance(this.Viewpoint)) - 1) / 2.0 + 1.0);
      if (newViewpoint == Viewpoint.Perspective || this.Viewpoint == Viewpoint.Perspective)
        num = 1f;
      Vector3 direction1 = this.current.Direction;
      Vector3 direction2 = this.predefinedViews[newViewpoint].Direction;
      this.directionTransition = new Vector3SplineInterpolation(TimeSpan.FromSeconds((double) DefaultCameraManager.TransitionSpeed * (double) num * (double) speedFactor), new Vector3[3]
      {
        direction1,
        DefaultCameraManager.GetIntemediateVector(direction1, direction2),
        direction2
      });
      this.directionTransition.Start();
    }
    if (this.viewpoint.IsOrthographic())
    {
      this.current.Direction = -this.viewpoint.ForwardVector();
      this.current.Radius = this.DefaultViewableWidth;
    }
    this.olderViewpoint = this.lastViewpoint;
    this.lastViewpoint = this.viewpoint;
    this.viewpoint = newViewpoint;
    Vector3 center = this.Center;
    this.current = this.predefinedViews[newViewpoint];
    this.current.Center = center;
    if (this.lastViewpoint != Viewpoint.None)
    {
      this.PreViewpointChanged();
      if (!this.ViewTransitionCancelled)
        this.ViewpointChanged();
    }
    if ((double) speedFactor == 0.0 && !this.ViewTransitionCancelled)
      this.RebuildView();
    bool transitionCancelled = this.ViewTransitionCancelled;
    this.ViewTransitionCancelled = false;
    if ((double) speedFactor > 0.0 && !transitionCancelled)
    {
      this.directionTransition.Update(this.dummyGt);
      this.current.Direction = this.directionTransition.Current;
    }
    return !transitionCancelled;
  }

  public void AlterTransition(Vector3 newDestinationDirection)
  {
    this.directionTransition.Points[1] = DefaultCameraManager.GetIntemediateVector(this.directionTransition.Points[0], newDestinationDirection);
    this.directionTransition.Points[2] = newDestinationDirection;
  }

  public void AlterTransition(Viewpoint newTo)
  {
    Viewpoint rotatedView = FezMath.OrientationFromDirection(this.directionTransition.Points[0]).AsViewpoint().GetRotatedView(FezMath.OrientationFromDirection(this.directionTransition.Points[2]).AsViewpoint().GetDistance(newTo));
    Vector3 direction1 = this.predefinedViews[rotatedView].Direction;
    Vector3 direction2 = this.predefinedViews[newTo].Direction;
    this.directionTransition.Points[0] = direction1;
    this.directionTransition.Points[1] = DefaultCameraManager.GetIntemediateVector(direction1, direction2);
    this.directionTransition.Points[2] = direction2;
    this.current = this.predefinedViews[newTo];
    this.lastViewpoint = rotatedView;
    this.viewpoint = newTo;
  }

  private static Vector3 GetIntemediateVector(Vector3 from, Vector3 to)
  {
    return (!FezMath.AlmostEqual(FezMath.AngleBetween(from, to), 3.14159274f) ? FezMath.Slerp(from, to, 0.5f) : Vector3.Cross(Vector3.Normalize(to - from), Vector3.UnitY)) + Vector3.UnitY * 0.0f;
  }

  public void SnapInterpolation()
  {
    this.interpolated.Center = this.current.Center;
    this.interpolated.Direction = this.current.Direction;
    this.interpolated.Radius = this.current.Radius;
    this.InterpolationReached = true;
    this.RebuildView();
    (this.LevelManager as FezEngine.Services.LevelManager).PrepareFullCull();
    this.RebuildProjection();
  }

  public Vector3 Center
  {
    get => this.current.Center;
    set => this.current.Center = value;
  }

  public Vector3 InterpolatedCenter
  {
    get => this.interpolated.Center;
    set => this.interpolated.Center = value;
  }

  public virtual float Radius
  {
    get => !this.ProjectionTransition ? this.interpolated.Radius : this.current.Radius;
    set
    {
      int num = !FezMath.AlmostEqual(this.current.Radius, value) ? 1 : 0;
      this.current.Radius = value;
      if (num == 0 || !this.viewpoint.IsOrthographic())
        return;
      this.RebuildProjection();
    }
  }

  public Vector3 Direction
  {
    get => this.current.Direction;
    set => this.current.Direction = value;
  }

  public float DefaultViewableWidth
  {
    get => this.defaultViewableWidth;
    set
    {
      this.defaultViewableWidth = value;
      foreach (DefaultCameraManager.PredefinedView predefinedView in this.predefinedViews.Values)
        predefinedView.Radius = this.defaultViewableWidth;
      this.RebuildProjection();
    }
  }

  public virtual void ResetViewpoints()
  {
    this.predefinedViews.Clear();
    this.predefinedViews.Add(Viewpoint.Perspective, new DefaultCameraManager.PredefinedView(Vector3.Zero, this.defaultViewableWidth, Vector3.One));
    this.predefinedViews.Add(Viewpoint.Front, new DefaultCameraManager.PredefinedView(Vector3.Zero, this.defaultViewableWidth, -Viewpoint.Front.ForwardVector()));
    this.predefinedViews.Add(Viewpoint.Right, new DefaultCameraManager.PredefinedView(Vector3.Zero, this.defaultViewableWidth, -Viewpoint.Right.ForwardVector()));
    this.predefinedViews.Add(Viewpoint.Back, new DefaultCameraManager.PredefinedView(Vector3.Zero, this.defaultViewableWidth, -Viewpoint.Back.ForwardVector()));
    this.predefinedViews.Add(Viewpoint.Left, new DefaultCameraManager.PredefinedView(Vector3.Zero, this.defaultViewableWidth, -Viewpoint.Left.ForwardVector()));
    if (this.viewpoint == Viewpoint.None)
      return;
    this.current = this.predefinedViews[this.viewpoint];
  }

  public Viewpoint Viewpoint => this.viewpoint;

  public FaceOrientation VisibleOrientation => this.Viewpoint.VisibleOrientation();

  public Viewpoint LastViewpoint
  {
    get => this.lastViewpoint != Viewpoint.Perspective ? this.lastViewpoint : this.olderViewpoint;
  }

  public void RebuildProjection()
  {
    this.AspectRatio = this.GraphicsService.GraphicsDevice.Viewport.AspectRatio;
    float width = this.interpolated.Radius / this.GraphicsService.GraphicsDevice.GetViewScale();
    if (this.viewpoint.IsOrthographic())
      this.projection = Matrix.CreateOrthographic(width, width / this.AspectRatio, this.NearPlane, this.FarPlane);
    else
      this.projection = Matrix.CreatePerspectiveFieldOfView(this.FieldOfView, this.AspectRatio, this.NearPlane, this.FarPlane);
    this.OnProjectionChanged();
  }

  public void RebuildView()
  {
    this.view = Matrix.CreateLookAt((this.viewpoint.IsOrthographic() ? (float) (((double) this.FarPlane - (double) this.NearPlane) / 2.0) : this.current.Radius) * this.current.Direction + this.current.Center, this.current.Center, Vector3.UnitY);
    this.OnViewChanged();
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.ViewTransitionReached && !this.ForceTransition)
    {
      this.transitionNewlyReached = true;
      this.directionTransition.Update(gameTime);
      this.current.Direction = this.directionTransition.Current;
    }
    else if (this.directionTransition != null && this.transitionNewlyReached)
    {
      this.transitionNewlyReached = false;
      this.current.Direction = this.directionTransition.Current;
    }
    if (this.ProjectionTransition)
      this.DollyZoom();
    else
      this.Interpolate(this.dummyGt);
  }

  public void InterpolationCallback(GameTime gameTime)
  {
    if (this.ProjectionTransition)
      return;
    this.Interpolate(gameTime);
  }

  public bool ProjectionTransitionNewlyReached
  {
    get => this.transitionNewlyReached && this.lastViewpoint == Viewpoint.Perspective;
  }

  public bool DollyZoomOut { protected get; set; }

  protected virtual void DollyZoom()
  {
    bool flag = this.viewpoint.IsOrthographic();
    float num1 = (double) this.directionTransition.TotalStep == 0.0 ? 1f / 1000f : this.directionTransition.TotalStep;
    float num2 = MathHelper.Lerp(flag ? DefaultCameraManager.DefaultFov : 0.0f, flag ? 0.0f : DefaultCameraManager.DefaultFov, num1);
    float num3 = this.radiusBeforeTransition;
    if (this.DollyZoomOut)
      num3 = this.radiusBeforeTransition + (float) ((1.0 - (double) Easing.EaseIn((double) num1, EasingType.Quadratic)) * 15.0);
    float num4 = (float) ((double) num3 / (double) this.AspectRatio / (2.0 * Math.Tan((double) num2 / 2.0)));
    if (this.directionTransition.Reached)
    {
      this.ProjectionTransition = false;
      if (!flag)
      {
        this.predefinedViews[this.lastViewpoint].Direction = -this.lastViewpoint.ForwardVector();
        this.current.Radius = num4;
      }
      else
      {
        this.current.Radius = this.radiusBeforeTransition;
        this.NearPlane = 0.25f;
        this.FarPlane = 500f;
      }
      this.FogManager.Density = this.LevelManager.Sky == null ? 0.0f : this.LevelManager.Sky.FogDensity;
      this.DollyZoomOut = false;
      this.RebuildProjection();
      this.SnapInterpolation();
    }
    else
    {
      this.FogManager.Density = (this.LevelManager.Sky == null ? 0.0f : this.LevelManager.Sky.FogDensity) * Easing.EaseIn(flag ? 1.0 - (double) num1 : (double) num1, EasingType.Quadratic);
      this.NearPlane = Math.Max(0.25f, 0.25f + num4 - num3);
      this.FarPlane = Math.Max(num4 + this.NearPlane, 499.75f);
      this.FieldOfView = num2;
      this.projection = Matrix.CreatePerspectiveFieldOfView(this.FieldOfView, this.AspectRatio, this.NearPlane, this.FarPlane);
      this.OnProjectionChanged();
      this.current.Radius = num4;
      this.view = Matrix.CreateLookAt(this.current.Radius * this.current.Direction + this.current.Center, this.current.Center, Vector3.UnitY);
      this.OnViewChanged();
    }
  }

  private void Interpolate(GameTime gameTime)
  {
    float num = FezMath.Saturate(FezMath.GetReachFactor(0.017f * this.InterpolationSpeed, (float) gameTime.ElapsedGameTime.TotalSeconds));
    if (DefaultCameraManager.NoInterpolation)
      num = 0.0f;
    if (this.current.Direction == Vector3.Zero)
      return;
    this.current.Direction = Vector3.Normalize(this.current.Direction);
    this.interpolated.Center = Vector3.Lerp(this.interpolated.Center, this.current.Center, num);
    this.interpolated.Radius = MathHelper.Lerp(this.interpolated.Radius, this.current.Radius, num);
    this.interpolated.Direction = FezMath.Slerp(this.interpolated.Direction, this.current.Direction, num);
    bool flag1 = (double) DefaultCameraManager.TransitionSpeed < 2.0 && FezMath.AlmostEqual(this.interpolated.Direction, this.current.Direction) && FezMath.AlmostEqual(this.interpolated.Center, this.current.Center);
    bool flag2 = FezMath.AlmostEqual(this.interpolated.Radius, this.current.Radius);
    if (this.ForceInterpolation)
    {
      flag1 = false;
      flag2 = false;
    }
    this.viewNewlyReached = flag1 && !this.InterpolationReached;
    if (this.viewNewlyReached)
    {
      this.interpolated.Direction = this.current.Direction;
      this.interpolated.Center = this.current.Center;
    }
    this.projNewlyReached = flag2 && !this.projReached;
    if (this.projNewlyReached)
      this.interpolated.Radius = this.current.Radius;
    this.InterpolationReached = flag1 & flag2;
    this.projReached = flag2;
    if (!flag1 || this.viewNewlyReached || (double) num == 1.0)
      this.RebuildInterpolatedView();
    if (!flag2 || this.projNewlyReached || (double) num == 1.0)
      this.RebuildInterpolatedProj();
    this.PostUpdate();
  }

  protected virtual void PostUpdate()
  {
  }

  private void RebuildInterpolatedView()
  {
    this.view = Matrix.CreateLookAt((this.viewpoint.IsOrthographic() ? 249.875f : this.interpolated.Radius) * this.interpolated.Direction + this.interpolated.Center, this.interpolated.Center, Vector3.UnitY);
    this.OnViewChanged();
  }

  private void RebuildInterpolatedProj()
  {
    if (!this.viewpoint.IsOrthographic() || (double) this.interpolated.Radius == (double) this.current.Radius && !this.projNewlyReached || this.ProjectionTransition)
      return;
    float width = this.interpolated.Radius / this.GraphicsService.GraphicsDevice.GetViewScale();
    this.projection = Matrix.CreateOrthographic(width, width / this.AspectRatio, 0.25f, 500f);
    this.OnProjectionChanged();
  }

  public bool ViewTransitionReached
  {
    get
    {
      return (this.directionTransition == null || this.directionTransition.Reached) && !this.ForceTransition;
    }
  }

  public virtual bool ActionRunning => this.ViewTransitionReached;

  public float ViewTransitionStep
  {
    get
    {
      return this.directionTransition != null && !this.ViewTransitionReached ? this.directionTransition.TotalStep : 0.0f;
    }
  }

  protected override void OnViewChanged()
  {
    this.InverseView = Matrix.Invert(this.view);
    if (!float.IsNaN(this.InverseView.M11) && !float.IsNaN(this.InverseView.M12) && !float.IsNaN(this.InverseView.M13) && !float.IsNaN(this.InverseView.M14) && !float.IsNaN(this.InverseView.M21) && !float.IsNaN(this.InverseView.M22) && !float.IsNaN(this.InverseView.M23) && !float.IsNaN(this.InverseView.M24) && !float.IsNaN(this.InverseView.M31) && !float.IsNaN(this.InverseView.M32) && !float.IsNaN(this.InverseView.M33) && !float.IsNaN(this.InverseView.M34))
    {
      Quaternion rotation;
      Vector3 translation;
      this.InverseView.Decompose(out Vector3 _, out rotation, out translation);
      this.Position = translation;
      this.Rotation = rotation;
    }
    this.RebuildFrustum();
    base.OnViewChanged();
  }

  protected override void OnProjectionChanged()
  {
    this.RebuildFrustum();
    base.OnProjectionChanged();
  }

  public Quaternion Rotation { get; protected set; }

  public bool InterpolationReached { get; protected set; }

  public float FieldOfView { get; protected set; }

  public BoundingFrustum Frustum { get; protected set; }

  public Vector3 Position { get; protected set; }

  public float NearPlane { get; protected set; }

  public float FarPlane { get; protected set; }

  public float AspectRatio { get; set; }

  public bool ProjectionTransition { get; set; }

  public bool ForceInterpolation { get; set; }

  public Vector3 ViewOffset
  {
    get => this.viewOffset;
    set
    {
      Vector3 viewOffset = this.viewOffset;
      if (this.ProjectionTransition)
      {
        this.current.Center -= this.viewOffset;
        this.viewOffset = value;
        this.current.Center += this.viewOffset;
        if (this.viewOffset == Vector3.Zero && viewOffset == Vector3.Zero)
          return;
        this.DollyZoom();
      }
      else
      {
        this.interpolated.Center -= this.viewOffset;
        this.viewOffset = value;
        this.interpolated.Center += this.viewOffset;
        if (this.viewOffset == Vector3.Zero && viewOffset == Vector3.Zero)
          return;
        this.RebuildInterpolatedView();
      }
    }
  }

  public float PixelsPerTrixel
  {
    get => this.pixelsPerTrixel;
    set
    {
      this.pixelsPerTrixel = value;
      this.DefaultViewableWidth = (float) this.Game.GraphicsDevice.Viewport.Width / (this.PixelsPerTrixel * 16f);
    }
  }

  public bool StickyCam { get; set; }

  public Vector2? PanningConstraints { get; set; }

  public Vector3 ConstrainedCenter { get; set; }

  public bool Constrained
  {
    get => this.constrained;
    set => this.constrained = value;
  }

  [ServiceDependency]
  public IGraphicsDeviceService GraphicsService { protected get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { protected get; set; }

  [ServiceDependency]
  public IFogManager FogManager { protected get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }

  protected class PredefinedView
  {
    public Vector3 Center;
    public float Radius;
    public Vector3 Direction;

    public PredefinedView()
    {
    }

    public PredefinedView(Vector3 center, float radius, Vector3 direction)
    {
      this.Center = center;
      this.Radius = radius;
      this.Direction = direction;
    }
  }
}
