// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PlayerCameraControl
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
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

public class PlayerCameraControl : GameComponent
{
  private const float TrilesBeforeScreenMove = 1.5f;
  public const float VerticalOffset = 4f;
  private Vector3 StickyCenter;
  private float MinimumStickDistance;
  private SoundEffect swooshLeft;
  private SoundEffect swooshRight;
  private SoundEffect slowSwooshLeft;
  private SoundEffect slowSwooshRight;
  private Vector2 lastFactors;

  public PlayerCameraControl(Game game)
    : base(game)
  {
    this.UpdateOrder = 10;
  }

  public override void Initialize()
  {
    this.swooshLeft = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateLeft");
    this.swooshRight = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateRight");
    this.slowSwooshLeft = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateLeftHalfSpeed");
    this.slowSwooshRight = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateRightHalfSpeed");
    this.CameraManager.Radius = this.CameraManager.DefaultViewableWidth;
    this.LevelManager.LevelChanged += (Action) (() => this.CameraManager.StickyCam = false);
    this.CameraManager.ViewpointChanged += (Action) (() =>
    {
      this.MinimumStickDistance = 2f;
      if (this.CameraManager.Viewpoint != Viewpoint.Perspective && !this.GameState.InMap)
        return;
      this.CameraManager.OriginalDirection = this.CameraManager.Direction;
    });
    TimeInterpolation.RegisterCallback((Action<GameTime>) (_ => this.PerFrameFollowGomez()), 50);
  }

  private void TrackBeforeRotation()
  {
    if (this.GameState.Paused || this.CameraManager.Constrained && !this.CameraManager.PanningConstraints.HasValue && !this.GameState.InMap || this.PlayerManager.Hidden || !this.CameraManager.ActionRunning && !this.CameraManager.ForceTransition || this.CameraManager.Viewpoint == Viewpoint.Perspective)
      return;
    this.FollowGomez();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.InCutscene)
      return;
    if (!this.PlayerManager.CanControl && !this.GameState.InMenuCube && !this.GameState.InMap)
    {
      this.InputManager.SaveState();
      this.InputManager.Reset();
    }
    MouseButtonState leftButton;
    if ((!this.PlayerManager.Action.PreventsRotation() || this.GameState.InMap || this.GameState.InMenuCube || this.GameState.InFpsMode) && this.PlayerManager.CanRotate && (!this.LevelManager.Flat || this.PlayerManager.Action == ActionType.GrabTombstone || this.GameState.InMap || this.GameState.InFpsMode || this.GameState.InMenuCube) && !this.GameState.Paused && !this.GameState.InCutscene)
    {
      bool flag = this.PlayerManager.Action == ActionType.GrabTombstone;
      if (!this.GameState.InFpsMode && !this.GameState.DisallowRotation)
      {
        if (this.InputManager.RotateLeft == FezButtonState.Pressed || flag && this.InputManager.RotateLeft == FezButtonState.Down)
        {
          this.TrackBeforeRotation();
          this.RotateViewLeft();
        }
        else if (this.InputManager.RotateRight == FezButtonState.Pressed || flag && this.InputManager.RotateRight == FezButtonState.Down)
        {
          this.TrackBeforeRotation();
          this.RotateViewRight();
        }
      }
      if (this.CameraManager.Viewpoint == Viewpoint.Perspective || this.CameraManager.RequestedViewpoint == Viewpoint.Perspective || this.GameState.InMap)
      {
        if (this.CameraManager.Viewpoint == Viewpoint.Perspective && !this.GameState.InMenuCube && !this.GameState.InMap)
          this.StickyCenter = this.CameraManager.Center = Vector3.Lerp(this.CameraManager.Center, this.PlayerManager.Position + (float) (4.0 * (this.LevelManager.Descending ? -1.0 : 1.0)) / this.CameraManager.PixelsPerTrixel * Vector3.UnitY, 0.075f);
        if (this.GameState.InFpsMode)
        {
          if (this.InputManager.FreeLook != Vector2.Zero)
          {
            int num1 = SettingsManager.Settings.InvertLookX ? 1 : -1;
            int num2 = SettingsManager.Settings.InvertLookY ? 1 : -1;
            if (this.MouseState.LeftButton.State != MouseButtonStates.Dragging)
            {
              num1 *= -1;
              num2 *= -1;
            }
            Vector3 direction = this.CameraManager.Direction;
            Matrix inverseView = this.CameraManager.InverseView;
            Quaternion fromAxisAngle1 = Quaternion.CreateFromAxisAngle(inverseView.Right, this.InputManager.FreeLook.Y * 0.4f * (float) num2);
            Vector3 vector3_1 = Vector3.Transform(direction, fromAxisAngle1);
            if ((double) vector3_1.Y > 0.7 || (double) vector3_1.Y < -0.7)
            {
              float num3 = 0.7f / new Vector2(vector3_1.X, vector3_1.Z).Length();
              vector3_1 = new Vector3(vector3_1.X * num3, 0.7f * (float) Math.Sign(vector3_1.Y), vector3_1.Z * num3);
            }
            Vector3 vector3_2 = vector3_1;
            inverseView = this.CameraManager.InverseView;
            Quaternion fromAxisAngle2 = Quaternion.CreateFromAxisAngle(inverseView.Up, (float) (-(double) this.InputManager.FreeLook.X * 0.5) * (float) num1);
            Vector3 to = Vector3.Transform(vector3_2, fromAxisAngle2);
            if (!this.CameraManager.ActionRunning)
              this.CameraManager.AlterTransition(FezMath.Slerp(this.CameraManager.Direction, to, 0.1f));
            else
              this.CameraManager.Direction = FezMath.Slerp(this.CameraManager.Direction, to, 0.1f);
            if ((double) this.CameraManager.Direction.Y < -0.625)
              this.CameraManager.Direction = new Vector3(this.CameraManager.Direction.X, -0.625f, this.CameraManager.Direction.Z);
          }
        }
        else if (this.InputManager.FreeLook != Vector2.Zero || this.GameState.InMap)
        {
          Vector3 vector3 = Vector3.Transform(this.CameraManager.OriginalDirection, Matrix.CreateFromAxisAngle(Vector3.Up, 1.57079637f));
          Vector3 to1 = Vector3.Transform(this.CameraManager.OriginalDirection, Matrix.CreateFromAxisAngle(vector3, -1.57079637f));
          Vector2 vector2 = this.InputManager.FreeLook / (this.GameState.MenuCubeIsZoomed ? 1.75f : 6.875f);
          float step = 0.1f;
          if (this.GameState.InMap && this.MouseState.LeftButton.State == MouseButtonStates.Dragging)
          {
            MouseDragState dragState = this.MouseState.LeftButton.DragState;
            double x = (double) -dragState.Movement.X;
            leftButton = this.MouseState.LeftButton;
            dragState = leftButton.DragState;
            double y = (double) dragState.Movement.Y;
            vector2 = Vector2.Clamp(new Vector2((float) x, (float) y) / (300f * this.GraphicsDeviceService.GraphicsDevice.GetViewScale()), -Vector2.One, Vector2.One) / (55f / 16f);
            step = 0.2f;
            this.lastFactors = vector2;
          }
          if (this.GameState.InMap)
          {
            leftButton = this.MouseState.LeftButton;
            if (leftButton.State == MouseButtonStates.DragEnded)
            {
              if ((double) this.lastFactors.X > 0.17499999701976776)
                this.RotateViewRight();
              else if ((double) this.lastFactors.X < -0.17499999701976776)
                this.RotateViewLeft();
            }
          }
          if (this.GameState.InMap)
          {
            vector2 *= new Vector2(3.425f, 1.725f);
            vector2.Y += 0.25f;
            vector2.X += 0.5f;
          }
          Vector3 to2 = FezMath.Slerp(FezMath.Slerp(this.CameraManager.OriginalDirection, vector3, vector2.X), to1, vector2.Y);
          if (!this.CameraManager.ActionRunning)
            this.CameraManager.AlterTransition(FezMath.Slerp(this.CameraManager.Direction, to2, step));
          else
            this.CameraManager.Direction = FezMath.Slerp(this.CameraManager.Direction, to2, step);
        }
        else if (!this.CameraManager.ActionRunning)
          this.CameraManager.AlterTransition(FezMath.Slerp(this.CameraManager.Direction, this.CameraManager.OriginalDirection, 0.1f));
        else
          this.CameraManager.Direction = FezMath.Slerp(this.CameraManager.Direction, this.CameraManager.OriginalDirection, 0.1f);
      }
    }
    if (this.CameraManager.RequestedViewpoint != Viewpoint.None)
    {
      if (this.CameraManager.RequestedViewpoint != this.CameraManager.Viewpoint)
        this.RotateTo(this.CameraManager.RequestedViewpoint);
      this.CameraManager.RequestedViewpoint = Viewpoint.None;
    }
    if (!this.GameState.Paused && (!this.CameraManager.Constrained || this.CameraManager.PanningConstraints.HasValue || this.GameState.InMap) && !this.PlayerManager.Hidden)
    {
      if ((this.CameraManager.ActionRunning || this.CameraManager.ForceTransition) && this.CameraManager.Viewpoint != Viewpoint.Perspective)
      {
        if (this.InputManager.FreeLook != Vector2.Zero)
        {
          if (!this.CameraManager.StickyCam)
            this.StickyCenter = this.CameraManager.Center;
          this.MinimumStickDistance = float.MaxValue;
          Vector2 vector2_1 = this.InputManager.FreeLook;
          leftButton = this.MouseState.LeftButton;
          if (leftButton.State == MouseButtonStates.Dragging)
            vector2_1 = -vector2_1;
          this.CameraManager.StickyCam = true;
          Vector2 vector2_2 = new Vector2(this.CameraManager.Radius, this.CameraManager.Radius / this.CameraManager.AspectRatio) * 0.4f / this.GraphicsDeviceService.GraphicsDevice.GetViewScale();
          this.StickyCenter = Vector3.Lerp(this.StickyCenter, this.PlayerManager.Position + (vector2_1.X * this.CameraManager.Viewpoint.RightVector() * vector2_2.X + vector2_1.Y * Vector3.UnitY * vector2_2.Y), 0.05f);
        }
        if (this.InputManager.ClampLook == FezButtonState.Pressed)
          this.CameraManager.StickyCam = false;
      }
    }
    else
      this.CameraManager.StickyCam = false;
    if (this.PlayerManager.CanControl || this.GameState.InMenuCube || this.GameState.InMap)
      return;
    this.InputManager.RecoverState();
  }

  private void PerFrameFollowGomez()
  {
    if (this.GameState.Loading || this.GameState.InCutscene || EndCutscene32Host.Instance != null || this.GameState.Paused || this.CameraManager.Constrained && !this.CameraManager.PanningConstraints.HasValue && !this.GameState.InMap || this.PlayerManager.Hidden || !this.CameraManager.ActionRunning && !this.CameraManager.ForceTransition || this.CameraManager.Viewpoint == Viewpoint.Perspective)
      return;
    this.FollowGomez();
  }

  private void FollowGomez()
  {
    float num1 = this.CameraManager.PixelsPerTrixel;
    if (this.GameState.FarawaySettings.InTransition && FezMath.AlmostEqual(this.GameState.FarawaySettings.DestinationCrossfadeStep, 1f))
      num1 = MathHelper.Lerp(this.CameraManager.PixelsPerTrixel, this.GameState.FarawaySettings.DestinationPixelsPerTrixel, (float) (((double) this.GameState.FarawaySettings.TransitionStep - 0.875) / 0.125));
    float num2 = (float) (4.0 * (this.LevelManager.Descending ? -1.0 : 1.0)) / num1;
    Vector3 interpolatedPosition = GomezHost.Instance.InterpolatedPosition;
    Vector3 vector3_1 = new Vector3(this.CameraManager.Center.X, interpolatedPosition.Y + num2, this.CameraManager.Center.Z);
    Vector3 center = this.CameraManager.Center;
    if (this.CameraManager.StickyCam)
    {
      Vector3 vector3_2 = interpolatedPosition + Vector3.UnitY * num2;
      Vector3 vector3_3 = this.StickyCenter * this.CameraManager.Viewpoint.ScreenSpaceMask() - vector3_2 * this.CameraManager.Viewpoint.ScreenSpaceMask();
      float val1 = vector3_3.Length() + 1f;
      if (this.InputManager.FreeLook == Vector2.Zero)
      {
        this.MinimumStickDistance = Math.Min(val1, this.MinimumStickDistance);
        float viewScale = this.GraphicsDeviceService.GraphicsDevice.GetViewScale();
        if ((double) Math.Abs(vector3_3.X + vector3_3.Z) > (double) this.CameraManager.Radius * 0.40000000596046448 / (double) viewScale || (double) Math.Abs(vector3_3.Y) > (double) this.CameraManager.Radius * 0.40000000596046448 / (double) this.CameraManager.AspectRatio / (double) viewScale)
          this.MinimumStickDistance = 2.5f;
        if ((double) this.MinimumStickDistance < 4.0)
          this.StickyCenter = Vector3.Lerp(this.StickyCenter, vector3_2, (float) Math.Pow(1.0 / (double) this.MinimumStickDistance, 4.0));
      }
      vector3_1 = this.StickyCenter;
      if ((double) val1 <= 1.1)
        this.CameraManager.StickyCam = false;
    }
    else
    {
      if ((double) MathHelper.Clamp(interpolatedPosition.X, center.X - 1.5f, center.X + 1.5f) != (double) interpolatedPosition.X)
      {
        float num3 = interpolatedPosition.X - center.X;
        vector3_1.X += num3 - 1.5f * (float) Math.Sign(num3);
      }
      if ((double) MathHelper.Clamp(interpolatedPosition.Z, center.Z - 1.5f, center.Z + 1.5f) != (double) interpolatedPosition.Z)
      {
        float num4 = interpolatedPosition.Z - this.CameraManager.Center.Z;
        vector3_1.Z += num4 - 1.5f * (float) Math.Sign(num4);
      }
    }
    Vector2? panningConstraints = this.CameraManager.PanningConstraints;
    if (panningConstraints.HasValue && WorldMap.Instance == null)
    {
      Vector3 vector3_4 = this.CameraManager.Viewpoint.DepthMask();
      Vector3 vector3_5 = this.CameraManager.Viewpoint.SideMask();
      Vector3 vector3_6;
      ref Vector3 local = ref vector3_6;
      double x1 = (double) this.CameraManager.ConstrainedCenter.X;
      double x2 = (double) vector3_1.X;
      panningConstraints = this.CameraManager.PanningConstraints;
      double x3 = (double) panningConstraints.Value.X;
      double x4 = (double) MathHelper.Lerp((float) x1, (float) x2, (float) x3);
      double y1 = (double) this.CameraManager.ConstrainedCenter.Y;
      double y2 = (double) vector3_1.Y;
      panningConstraints = this.CameraManager.PanningConstraints;
      double y3 = (double) panningConstraints.Value.Y;
      double y4 = (double) MathHelper.Lerp((float) y1, (float) y2, (float) y3);
      double z1 = (double) this.CameraManager.ConstrainedCenter.Z;
      double z2 = (double) vector3_1.Z;
      panningConstraints = this.CameraManager.PanningConstraints;
      double x5 = (double) panningConstraints.Value.X;
      double z3 = (double) MathHelper.Lerp((float) z1, (float) z2, (float) x5);
      local = new Vector3((float) x4, (float) y4, (float) z3);
      this.CameraManager.Center = this.CameraManager.Center * vector3_4 + vector3_5 * vector3_6 + Vector3.UnitY * vector3_6;
    }
    else
    {
      if (this.GameState.InMenuCube || WorldMap.Instance != null || Intro.Instance != null || FezMath.In<ActionType>(this.PlayerManager.Action, ActionType.PullUpCornerLedge, ActionType.LowerToCornerLedge, ActionType.PullUpFront, ActionType.LowerToLedge, ActionType.PullUpBack, ActionType.Victory, (IEqualityComparer<ActionType>) ActionTypeComparer.Default))
        return;
      this.CameraManager.Center = vector3_1;
    }
  }

  private void RotateViewLeft()
  {
    bool flag = this.PlayerManager.Action == ActionType.GrabTombstone;
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective || this.GameState.InMap)
    {
      this.CameraManager.OriginalDirection = Vector3.Transform(this.CameraManager.OriginalDirection, Quaternion.CreateFromAxisAngle(Vector3.Up, -1.57079637f));
      if (!this.GameState.InMenuCube && !this.GameState.InMap)
        this.EmitLeft();
    }
    else if (this.CameraManager.ChangeViewpoint(this.CameraManager.Viewpoint.GetRotatedView(-1), (flag ? 2f : 1f) * Math.Abs(1f / this.CollisionManager.GravityFactor)) && !flag)
      this.EmitLeft();
    if (this.LevelManager.NodeType != LevelNodeType.Lesser || !(this.PlayerManager.AirTime != TimeSpan.Zero))
      return;
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * this.CameraManager.Viewpoint.ScreenSpaceMask();
  }

  private void RotateViewRight()
  {
    bool flag = this.PlayerManager.Action == ActionType.GrabTombstone;
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective || this.GameState.InMap)
    {
      this.CameraManager.OriginalDirection = Vector3.Transform(this.CameraManager.OriginalDirection, Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f));
      if (!this.GameState.InMenuCube && !this.GameState.InMap)
        this.EmitRight();
    }
    else if (this.CameraManager.ChangeViewpoint(this.CameraManager.Viewpoint.GetRotatedView(1), (flag ? 2f : 1f) * Math.Abs(1f / this.CollisionManager.GravityFactor)) && !flag)
      this.EmitRight();
    if (this.LevelManager.NodeType != LevelNodeType.Lesser || !(this.PlayerManager.AirTime != TimeSpan.Zero))
      return;
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * this.CameraManager.Viewpoint.ScreenSpaceMask();
  }

  private void EmitLeft()
  {
    if (Fez.LongScreenshot)
      return;
    if ((double) this.CollisionManager.GravityFactor == 1.0)
      this.swooshLeft.Emit();
    else
      this.slowSwooshLeft.Emit();
  }

  private void EmitRight()
  {
    if (Fez.LongScreenshot)
      return;
    if ((double) this.CollisionManager.GravityFactor == 1.0)
      this.swooshRight.Emit();
    else
      this.slowSwooshRight.Emit();
  }

  private void RotateTo(Viewpoint view)
  {
    if (Math.Abs(this.CameraManager.Viewpoint.GetDistance(view)) > 1)
      this.EmitRight();
    this.CameraManager.ChangeViewpoint(view);
  }

  [ServiceDependency]
  public IGraphicsDeviceService GraphicsDeviceService { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IMouseStateManager MouseState { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
