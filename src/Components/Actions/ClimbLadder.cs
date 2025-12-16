// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ClimbLadder
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

public class ClimbLadder : PlayerAction
{
  private const float ClimbingSpeed = 0.425f;
  private ClimbingApproach currentApproach;
  private bool shouldSyncAnimationHalfway;
  private Vector3? lastGrabbedLocation;
  private SoundEffect climbSound;
  private int lastFrame;

  public ClimbLadder(Game game)
    : base(game)
  {
    this.UpdateOrder = 1;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.climbSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/ClimbLadder");
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
    this.RefreshPlayerAction();
    this.RefreshPlayerDirection();
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
      case ActionType.IdleYawn:
        TrileInstance trileInstance = this.IsOnLadder(out this.currentApproach);
        if (this.currentApproach == ClimbingApproach.None)
          break;
        bool flag = false;
        if (this.InputManager.Down.IsDown() && this.PlayerManager.Grounded)
        {
          TrileInstance surface = this.LevelManager.NearestTrile(trileInstance.Center - Vector3.UnitY).Surface;
          flag = surface != null && surface.Trile.ActorSettings.Type == ActorType.Ladder;
        }
        FezButtonState fezButtonState = this.PlayerManager.Grounded ? FezButtonState.Pressed : FezButtonState.Down;
        if (!flag && this.InputManager.Up != fezButtonState && (this.PlayerManager.Grounded || this.currentApproach != ClimbingApproach.Left && this.currentApproach != ClimbingApproach.Right || Math.Sign(this.InputManager.Movement.X) != this.currentApproach.AsDirection().Sign()))
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
            this.PlayerManager.NextAction = ActionType.SideClimbingLadder;
            break;
          case ClimbingApproach.Back:
            this.PlayerManager.NextAction = ActionType.BackClimbingLadder;
            break;
          case ClimbingApproach.Front:
            this.PlayerManager.NextAction = ActionType.FrontClimbingLadder;
            break;
        }
        if (this.PlayerManager.Grounded)
        {
          ActionType actionType = this.currentApproach == ClimbingApproach.Back ? ActionType.IdleToClimb : (this.currentApproach == ClimbingApproach.Front ? ActionType.IdleToFrontClimb : (this.currentApproach == ClimbingApproach.Back ? ActionType.IdleToClimb : ActionType.IdleToSideClimb));
          if (this.CollisionManager.CollidePoint(this.GetDestination(), Vector3.Down, QueryOptions.None, 0.0f, this.CameraManager.Viewpoint).Collided)
          {
            this.WalkTo.Destination = new Func<Vector3>(this.GetDestination);
            this.WalkTo.NextAction = actionType;
            this.PlayerManager.Action = ActionType.WalkingTo;
          }
          else
          {
            this.PlayerManager.Action = actionType;
            this.PlayerManager.Position -= 0.15f * Vector3.UnitY;
          }
        }
        else
          this.PlayerManager.Action = this.currentApproach == ClimbingApproach.Back ? ActionType.JumpToClimb : ActionType.JumpToSideClimb;
        if (this.currentApproach != ClimbingApproach.Left && this.currentApproach != ClimbingApproach.Right)
          break;
        this.PlayerManager.LookingDirection = this.currentApproach.AsDirection();
        break;
    }
  }

  protected override void Begin()
  {
    this.PlayerManager.Position = this.PlayerManager.Position * Vector3.UnitY + (this.PlayerManager.HeldInstance.Position + FezMath.HalfVector) * FezMath.XZMask;
    if (this.InputManager.Down.IsDown())
      this.PlayerManager.Position -= 1f / 500f * Vector3.UnitY;
    this.GomezService.OnClimbLadder();
  }

  private TrileInstance IsOnLadder(out ClimbingApproach approach)
  {
    Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
    Vector3 vector3 = this.CameraManager.Viewpoint.RightVector();
    float num1 = float.MaxValue;
    bool flag1 = false;
    TrileInstance trileInstance = (TrileInstance) null;
    bool flag2 = true;
    if (this.currentApproach == ClimbingApproach.None)
    {
      NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.PlayerManager.Center, this.PlayerManager.Background ? QueryOptions.Background : QueryOptions.None);
      flag2 = nearestTriles.Surface != null && nearestTriles.Surface.Trile.ActorSettings.Type == ActorType.Ladder;
    }
    foreach (NearestTriles nearestTriles in this.PlayerManager.AxisCollision.Values)
    {
      if (nearestTriles.Surface != null && this.TestLadderCollision(nearestTriles.Surface, true))
      {
        TrileInstance surface = nearestTriles.Surface;
        float num2 = surface.Position.Dot(b);
        if (flag2 && (double) num2 < (double) num1)
        {
          num1 = num2;
          trileInstance = surface;
        }
      }
    }
    if (trileInstance == null)
    {
      foreach (NearestTriles nearestTriles in this.PlayerManager.AxisCollision.Values)
      {
        if (nearestTriles.Surface != null && this.TestLadderCollision(nearestTriles.Surface, false))
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

  private bool TestLadderCollision(TrileInstance instance, bool onAxis)
  {
    TrileActorSettings actorSettings = instance.Trile.ActorSettings;
    Axis axis = FezMath.AxisFromPhi(FezMath.WrapAngle(actorSettings.Face.ToPhi() + instance.Phi));
    return actorSettings.Type == ActorType.Ladder && axis == this.CameraManager.Viewpoint.VisibleAxis() == onAxis;
  }

  public override void Update(GameTime gameTime)
  {
    if ((double) this.PlayerManager.Velocity.Y < -0.0099999997764825821)
      this.lastGrabbedLocation = new Vector3?();
    if (this.shouldSyncAnimationHalfway && (double) this.CameraManager.ViewTransitionStep >= 0.5)
    {
      this.shouldSyncAnimationHalfway = false;
      this.SyncAnimation(true);
    }
    base.Update(gameTime);
  }

  private Vector3 GetDestination()
  {
    return this.PlayerManager.Position * Vector3.UnitY + (this.PlayerManager.HeldInstance.Position + FezMath.HalfVector) * FezMath.XZMask;
  }

  private bool TestLadderTopLimit(ref Vector3 upDownMovement, Vector3 forward)
  {
    if ((double) this.PlayerManager.Center.Y < (double) this.LevelManager.Size.Y - 1.0 && (double) this.PlayerManager.Center.Y > 1.0)
    {
      QueryOptions options = this.PlayerManager.Background ? QueryOptions.Background : QueryOptions.None;
      NearestTriles nearestTriles1 = this.LevelManager.NearestTrile(this.PlayerManager.Center + Vector3.Down, options);
      NearestTriles nearestTriles2 = this.LevelManager.NearestTrile(this.PlayerManager.Center + upDownMovement, options);
      NearestTriles nearestTriles3 = this.LevelManager.NearestTrile(this.PlayerManager.Center + upDownMovement + upDownMovement.Sign() * new Vector3(0.0f, 0.5f, 0.0f), options);
      bool flag = false;
      if ((nearestTriles2.Surface == null || (flag = nearestTriles3.Deep != null && (double) this.PlayerManager.Position.Dot(forward) > (double) nearestTriles3.Deep.Center.Dot(forward))) && (nearestTriles1.Deep == null || nearestTriles1.Deep.GetRotatedFace(this.PlayerManager.Background ? this.CameraManager.VisibleOrientation : this.CameraManager.VisibleOrientation.GetOpposite()) == CollisionType.None))
      {
        upDownMovement = Vector3.Zero;
        if (!flag && (this.PlayerManager.LookingDirection == HorizontalDirection.Left && this.InputManager.Left.IsDown() || this.PlayerManager.LookingDirection == HorizontalDirection.Right && this.InputManager.Right.IsDown()) && (nearestTriles2.Deep == null || nearestTriles1.Surface == null || (double) nearestTriles2.Deep.Center.Dot(forward) > (double) nearestTriles1.Surface.Center.Dot(forward)))
          return false;
      }
    }
    return true;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    TrileInstance trileInstance = this.IsOnLadder(out ClimbingApproach _);
    this.PlayerManager.HeldInstance = trileInstance;
    if (trileInstance == null || this.currentApproach == ClimbingApproach.None)
    {
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    this.lastGrabbedLocation = new Vector3?(this.PlayerManager.Position);
    this.RefreshPlayerAction();
    this.RefreshPlayerDirection();
    this.PlayerManager.Position = this.PlayerManager.Position * Vector3.UnitY + (trileInstance.Position + FezMath.HalfVector) * FezMath.XZMask;
    Vector3 upDownMovement = (float) ((double) this.InputManager.Movement.Y * 4.6999998092651367 * 0.42500001192092896) * (float) elapsed.TotalSeconds * Vector3.UnitY;
    Vector3 forward = this.CameraManager.Viewpoint.ForwardVector() * (this.PlayerManager.Background ? -1f : 1f);
    if (!this.TestLadderTopLimit(ref upDownMovement, forward))
    {
      this.PlayerManager.Action = ActionType.ClimbOverLadder;
      return false;
    }
    float num = (float) ((double) FezMath.Saturate(Math.Abs((float) ((double) this.PlayerManager.Animation.Timing.NormalizedStep * 2.0 % 1.0 - 0.5))) * 1.3999999761581421 + 0.25);
    int frame = this.PlayerManager.Animation.Timing.Frame;
    if (this.lastFrame != frame)
    {
      if (frame == 1 || frame == 4)
        this.climbSound.EmitAt(this.PlayerManager.Position);
      this.lastFrame = frame;
    }
    this.PlayerManager.Velocity = upDownMovement * num;
    if (trileInstance.PhysicsState != null)
    {
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity + trileInstance.PhysicsState.Velocity;
    }
    int timeFactor = Math.Sign(upDownMovement.Y);
    this.PlayerManager.Animation.Timing.Update(elapsed, (float) timeFactor);
    this.PlayerManager.GroundedVelocity = new Vector3?(this.PlayerManager.Velocity);
    MultipleHits<CollisionResult> multipleHits = this.CollisionManager.CollideEdge(this.PlayerManager.Center, upDownMovement, this.PlayerManager.Size / 2f, Direction2D.Vertical);
    if ((double) upDownMovement.Y < 0.0 && (multipleHits.NearLow.Collided || multipleHits.FarHigh.Collided))
    {
      TrileInstance surface = this.LevelManager.NearestTrile(multipleHits.First.Destination.Center).Surface;
      if (surface != null && surface.Trile.ActorSettings.Type == ActorType.Ladder && this.currentApproach == ClimbingApproach.Back)
      {
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Center = playerManager.Center + upDownMovement;
      }
      else
      {
        this.lastGrabbedLocation = new Vector3?();
        this.PlayerManager.HeldInstance = (TrileInstance) null;
        this.PlayerManager.Action = ActionType.Falling;
      }
    }
    return false;
  }

  private void RefreshPlayerAction()
  {
    switch (this.currentApproach)
    {
      case ClimbingApproach.Right:
      case ClimbingApproach.Left:
        this.PlayerManager.Action = ActionType.SideClimbingLadder;
        break;
      case ClimbingApproach.Back:
        this.PlayerManager.Action = ActionType.BackClimbingLadder;
        break;
      case ClimbingApproach.Front:
        this.PlayerManager.Action = ActionType.FrontClimbingLadder;
        break;
    }
  }

  private void RefreshPlayerDirection()
  {
    if (this.PlayerManager.Action != ActionType.SideClimbingLadder)
      return;
    this.PlayerManager.LookingDirection = this.currentApproach.AsDirection();
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return this.PlayerManager.Action.IsClimbingLadder();
  }
}
