// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ClimbVine
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class ClimbVine : PlayerAction
{
  private const float ClimbingSpeed = 0.475f;
  private const float SideClimbingFactor = 0.75f;
  private ClimbingApproach currentApproach;
  private bool shouldSyncAnimationHalfway;
  private Vector3? lastGrabbedLocation;
  private SoundEffect climbSound;
  private int lastFrame;

  public ClimbVine(Game game)
    : base(game)
  {
    this.UpdateOrder = 1;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.climbSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/ClimbVine");
  }

  public override void Initialize()
  {
    this.CameraManager.ViewpointChanged += new Action(this.ChangeApproach);
    base.Initialize();
  }

  private void ChangeApproach()
  {
    this.lastGrabbedLocation = new Vector3?();
    if (!this.IsActionAllowed(this.PlayerManager.Action) || !this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.Viewpoint == this.CameraManager.LastViewpoint || this.PlayerManager.IsOnRotato)
      return;
    int num = (int) (this.currentApproach + this.CameraManager.Viewpoint.GetDistance(this.CameraManager.LastViewpoint));
    if (num > 4)
      num -= 4;
    if (num < 1)
      num += 4;
    this.currentApproach = (ClimbingApproach) num;
    this.RefreshPlayerAction(true);
    this.shouldSyncAnimationHalfway = true;
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.LookingLeft:
      case ActionType.LookingRight:
      case ActionType.LookingUp:
      case ActionType.LookingDown:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.Jumping:
      case ActionType.Lifting:
      case ActionType.Falling:
      case ActionType.Bouncing:
      case ActionType.Flying:
      case ActionType.Dropping:
      case ActionType.Sliding:
      case ActionType.Landing:
      case ActionType.Teetering:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.GrabCornerLedge:
      case ActionType.GrabLedgeBack:
      case ActionType.IdleYawn:
        TrileInstance trileInstance = this.IsOnVine(out this.currentApproach);
        if (this.currentApproach == ClimbingApproach.None || (!this.InputManager.Up.IsDown() || this.PlayerManager.Action.IsOnLedge()) && (this.PlayerManager.Grounded || this.currentApproach != ClimbingApproach.Left && this.currentApproach != ClimbingApproach.Right || Math.Sign(this.InputManager.Movement.X) != this.currentApproach.AsDirection().Sign()) && (!this.PlayerManager.Action.IsOnLedge() || !this.InputManager.Down.IsDown()))
          break;
        if (this.lastGrabbedLocation.HasValue)
        {
          Vector3 a = this.PlayerManager.Position - this.lastGrabbedLocation.Value;
          Vector3 b = this.CameraManager.Viewpoint.SideMask();
          if ((double) Math.Abs(a.Dot(b)) <= 1.0 && (double) a.Dot(Vector3.UnitY) < 1.625 && (double) a.Dot(Vector3.UnitY) > -0.5)
            break;
        }
        this.PlayerManager.HeldInstance = trileInstance;
        switch (this.currentApproach)
        {
          case ClimbingApproach.Right:
          case ClimbingApproach.Left:
            this.PlayerManager.NextAction = ActionType.SideClimbingVine;
            break;
          case ClimbingApproach.Back:
            this.PlayerManager.NextAction = ActionType.BackClimbingVine;
            break;
          case ClimbingApproach.Front:
            this.PlayerManager.NextAction = ActionType.FrontClimbingVine;
            break;
        }
        if (this.PlayerManager.Action.IsOnLedge())
        {
          this.PlayerManager.Action = this.PlayerManager.NextAction;
          this.PlayerManager.NextAction = ActionType.None;
          break;
        }
        this.PlayerManager.Action = this.currentApproach == ClimbingApproach.Back ? ActionType.JumpToClimb : ActionType.JumpToSideClimb;
        this.PlayerManager.Velocity = Vector3.Zero;
        if (this.currentApproach != ClimbingApproach.Left && this.currentApproach != ClimbingApproach.Right)
          break;
        this.PlayerManager.LookingDirection = this.currentApproach.AsDirection();
        break;
    }
  }

  private TrileInstance IsOnVine(out ClimbingApproach approach)
  {
    Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
    Vector3 vector3 = this.CameraManager.Viewpoint.RightVector();
    float num1 = float.MaxValue;
    bool flag1 = false;
    TrileInstance trileInstance = (TrileInstance) null;
    bool flag2 = true;
    if (this.currentApproach == ClimbingApproach.None)
    {
      NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.PlayerManager.Position - 1f / 500f * Vector3.UnitY);
      flag2 = nearestTriles.Surface != null && nearestTriles.Surface.Trile.ActorSettings.Type == ActorType.Vine;
    }
    foreach (PointCollision pointCollision in this.PlayerManager.CornerCollision)
    {
      if (pointCollision.Instances.Surface != null && this.TestVineCollision(pointCollision.Instances.Surface, true))
      {
        TrileInstance surface = pointCollision.Instances.Surface;
        float num2 = surface.Position.Dot(b);
        if (flag2 && (double) num2 < (double) num1 && this.TestVineCollision(pointCollision.Instances.Surface, true))
        {
          num1 = num2;
          trileInstance = surface;
        }
      }
    }
    foreach (NearestTriles nearestTriles in this.PlayerManager.AxisCollision.Values)
    {
      if (nearestTriles.Surface != null && this.TestVineCollision(nearestTriles.Surface, false))
      {
        TrileInstance surface = nearestTriles.Surface;
        float num3 = surface.Position.Dot(b);
        if (flag2 && (double) num3 < (double) num1)
        {
          flag1 = true;
          num1 = num3;
          trileInstance = surface;
        }
      }
    }
    if (trileInstance != null)
    {
      float num4 = FezMath.OrientationFromPhi(FezMath.WrapAngle(trileInstance.Trile.ActorSettings.Face.ToPhi() + trileInstance.Phi)).AsVector().Dot(flag1 ? vector3 : b);
      approach = !flag1 ? ((double) num4 > 0.0 ? ClimbingApproach.Front : ClimbingApproach.Back) : ((double) num4 > 0.0 ? ClimbingApproach.Left : ClimbingApproach.Right);
    }
    else
      approach = ClimbingApproach.None;
    return trileInstance;
  }

  private bool TestVineCollision(TrileInstance instance, bool onAxis)
  {
    TrileActorSettings actorSettings = instance.Trile.ActorSettings;
    Axis axis = FezMath.AxisFromPhi(FezMath.WrapAngle(actorSettings.Face.ToPhi() + instance.Phi));
    return actorSettings.Type == ActorType.Vine && axis == this.CameraManager.Viewpoint.VisibleAxis() == onAxis;
  }

  protected override void Begin()
  {
    if (this.currentApproach == ClimbingApproach.None)
    {
      ClimbingApproach approach;
      this.PlayerManager.HeldInstance = this.IsOnVine(out approach);
      this.currentApproach = approach;
    }
    this.GomezService.OnClimbVine();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.InMap || this.GameState.Paused || !this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.InMenuCube)
      return;
    if ((double) this.PlayerManager.Velocity.Y < -0.0099999997764825821)
      this.lastGrabbedLocation = new Vector3?();
    if (this.shouldSyncAnimationHalfway && (double) this.CameraManager.ViewTransitionStep >= 0.5)
    {
      if (this.PlayerManager.Action == ActionType.BackClimbingVine || this.PlayerManager.Action == ActionType.BackClimbingVineSideways)
        this.PlayerManager.Background = false;
      this.shouldSyncAnimationHalfway = false;
      this.SyncAnimation(true);
      this.RefreshPlayerDirection(true);
    }
    base.Update(gameTime);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    ClimbingApproach approach;
    TrileInstance trileInstance1 = this.IsOnVine(out approach);
    this.PlayerManager.HeldInstance = trileInstance1;
    if (trileInstance1 == null || this.currentApproach == ClimbingApproach.None)
    {
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    this.lastGrabbedLocation = new Vector3?(this.PlayerManager.Position);
    if ((this.currentApproach == ClimbingApproach.Back || this.currentApproach == ClimbingApproach.Front) && (approach == ClimbingApproach.Right || approach == ClimbingApproach.Left))
      this.currentApproach = approach;
    if (this.PlayerManager.Action == ActionType.SideClimbingVine && (double) Math.Abs(this.InputManager.Movement.X) > 0.5)
    {
      Vector3 vector3 = (float) Math.Sign(this.InputManager.Movement.X) * this.CameraManager.Viewpoint.RightVector();
      NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.PlayerManager.Position + vector3);
      if (nearestTriles.Surface != null && nearestTriles.Surface.Trile.ActorSettings.Type == ActorType.Vine)
      {
        this.PlayerManager.Position += vector3 * 0.1f;
        this.PlayerManager.ForceOverlapsDetermination();
        trileInstance1 = this.IsOnVine(out this.currentApproach);
        this.PlayerManager.HeldInstance = trileInstance1;
      }
    }
    if (trileInstance1 == null || this.currentApproach == ClimbingApproach.None)
    {
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    this.RefreshPlayerAction(false);
    this.RefreshPlayerDirection(false);
    Vector3 vector3_1 = trileInstance1.Position + FezMath.HalfVector;
    Vector3 vector3_2 = Vector3.Zero;
    switch (this.currentApproach)
    {
      case ClimbingApproach.Right:
      case ClimbingApproach.Left:
        TrileInstance trileInstance2 = this.LevelManager.ActualInstanceAt(this.PlayerManager.Position);
        vector3_2 = trileInstance2 == null || trileInstance2.Trile.ActorSettings.Type != ActorType.Vine ? FezMath.XZMask : this.CameraManager.Viewpoint.SideMask();
        break;
      case ClimbingApproach.Back:
      case ClimbingApproach.Front:
        vector3_2 = this.CameraManager.Viewpoint.DepthMask();
        break;
    }
    this.PlayerManager.Position = this.PlayerManager.Position * (Vector3.One - vector3_2) + vector3_1 * vector3_2;
    Vector2 vector2 = this.InputManager.Movement * 4.7f * 0.475f * (float) elapsed.TotalSeconds;
    Vector3 vector = Vector3.Zero;
    if (this.PlayerManager.Action != ActionType.SideClimbingVine)
      vector = Vector3.Transform(Vector3.UnitX * vector2.X * 0.75f, this.CameraManager.Rotation);
    Vector3 impulse = vector2.Y * Vector3.UnitY;
    QueryOptions options = this.PlayerManager.Background ? QueryOptions.Background : QueryOptions.None;
    FaceOrientation face = this.PlayerManager.Background ? this.CameraManager.VisibleOrientation : this.CameraManager.VisibleOrientation.GetOpposite();
    NearestTriles nearestTriles1 = this.LevelManager.NearestTrile(this.PlayerManager.Center + Vector3.Down * 1.5f + this.PlayerManager.Size / 2f * vector.Sign(), options);
    NearestTriles nearestTriles2 = this.LevelManager.NearestTrile(this.PlayerManager.Center + vector * 2f, options);
    if ((nearestTriles2.Surface == null || nearestTriles2.Surface.Trile.ActorSettings.Type != ActorType.Vine) && (nearestTriles1.Deep == null || nearestTriles1.Deep.GetRotatedFace(face) == CollisionType.None))
      vector = Vector3.Zero;
    NearestTriles nearestTriles3 = this.LevelManager.NearestTrile(this.PlayerManager.Center + this.PlayerManager.Size / 2f * Vector3.Down, options);
    NearestTriles nearestTriles4 = this.LevelManager.NearestTrile(this.PlayerManager.Center + impulse * 2f, options);
    if ((nearestTriles4.Surface == null || nearestTriles4.Surface.Trile.ActorSettings.Type != ActorType.Vine) && (nearestTriles3.Deep == null || nearestTriles3.Deep.GetRotatedFace(face) == CollisionType.None))
    {
      impulse = Vector3.Zero;
      if (this.InputManager.Up.IsDown())
      {
        Vector3 vector3_3 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
        TrileInstance deep1 = this.LevelManager.NearestTrile(trileInstance1.Center + vector3_3 * 0.5f + vector3_3 * trileInstance1.TransformedSize / 2f).Deep;
        if (deep1 != null && !deep1.Trile.Immaterial && deep1.Enabled && deep1.GetRotatedFace(face) != CollisionType.None)
        {
          TrileInstance trileInstance3 = this.LevelManager.ActualInstanceAt(deep1.Position - vector3_3 + new Vector3(0.5f));
          TrileInstance deep2 = this.LevelManager.NearestTrile(deep1.Position - vector3_3 + new Vector3(0.5f)).Deep;
          if ((deep2 == null || !deep2.Enabled || deep2.GetRotatedFace(this.CameraManager.VisibleOrientation) == CollisionType.None) && (trileInstance3 == null || !trileInstance3.Enabled || trileInstance3.Trile.Immaterial))
          {
            this.PlayerManager.HeldInstance = deep1;
            this.PlayerManager.Action = ActionType.GrabCornerLedge;
            Vector3 vector3_4 = (-vector3_3 + Vector3.UnitY) * deep1.TransformedSize / 2f;
            this.PlayerManager.Position = deep1.Center + vector3_4;
            this.PlayerManager.ForceOverlapsDetermination();
            return false;
          }
        }
      }
    }
    float num1 = (float) ((double) FezMath.Saturate(Math.Abs((float) ((double) this.PlayerManager.Animation.Timing.NormalizedStep * 2.0 % 1.0 - 0.5))) * 1.3999999761581421 + 0.25);
    float num2 = FezMath.Saturate(Math.Abs((float) (((double) this.PlayerManager.Animation.Timing.NormalizedStep + 0.30000001192092896) % 1.0))) + 0.2f;
    int frame = this.PlayerManager.Animation.Timing.Frame;
    if (this.lastFrame != frame)
    {
      bool flag = (double) Math.Abs(this.InputManager.Movement.Y) < 0.5;
      if (flag && frame == 0 || !flag && (frame == 1 || frame == 4))
        this.climbSound.EmitAt(this.PlayerManager.Position, RandomHelper.Between(-0.10000000149011612, 0.10000000149011612), RandomHelper.Between(0.89999997615814209, 1.0));
      this.lastFrame = frame;
    }
    this.PlayerManager.Velocity = vector * num2 + impulse * num1;
    if (trileInstance1.PhysicsState != null)
    {
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity + trileInstance1.PhysicsState.Velocity;
    }
    float timeFactor = impulse == Vector3.Zero ? 0.0f : Math.Abs(this.InputManager.Movement.Y);
    if (this.PlayerManager.Action != ActionType.SideClimbingVine)
      timeFactor = vector == Vector3.Zero ? timeFactor : FezMath.Saturate(timeFactor + Math.Abs(this.InputManager.Movement.X));
    this.PlayerManager.Animation.Timing.Update(elapsed, timeFactor);
    this.PlayerManager.GroundedVelocity = new Vector3?(this.PlayerManager.Velocity);
    MultipleHits<CollisionResult> multipleHits = this.CollisionManager.CollideEdge(this.PlayerManager.Center, impulse, this.PlayerManager.Size / 2f, Direction2D.Vertical);
    if ((double) impulse.Y < 0.0 && (multipleHits.NearLow.Collided || multipleHits.FarHigh.Collided) && multipleHits.First.Destination.GetRotatedFace(this.CameraManager.VisibleOrientation) != CollisionType.None)
    {
      this.lastGrabbedLocation = new Vector3?();
      this.PlayerManager.HeldInstance = (TrileInstance) null;
      this.PlayerManager.Action = ActionType.Falling;
    }
    return false;
  }

  private void RefreshPlayerAction(bool force)
  {
    if (!force && this.InputManager.Movement == Vector2.Zero)
      return;
    switch (this.currentApproach)
    {
      case ClimbingApproach.Right:
      case ClimbingApproach.Left:
        this.PlayerManager.Action = ActionType.SideClimbingVine;
        break;
      case ClimbingApproach.Back:
        this.PlayerManager.Action = (double) this.InputManager.Movement.Y != 0.0 ? ActionType.BackClimbingVine : ActionType.BackClimbingVineSideways;
        break;
      case ClimbingApproach.Front:
        this.PlayerManager.Action = (double) this.InputManager.Movement.Y != 0.0 ? ActionType.FrontClimbingVine : ActionType.FrontClimbingVineSideways;
        break;
    }
  }

  private void RefreshPlayerDirection(bool force)
  {
    if (!force && this.InputManager.Movement == Vector2.Zero)
      return;
    if (this.PlayerManager.Action == ActionType.SideClimbingVine)
      this.PlayerManager.LookingDirection = this.currentApproach.AsDirection();
    else
      this.PlayerManager.LookingDirection = FezMath.DirectionFromMovement(this.InputManager.Movement.X);
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return this.PlayerManager.Action.IsClimbingVine();
  }
}
