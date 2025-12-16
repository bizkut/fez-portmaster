// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PlayerActions
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components.Actions;
using FezGame.Components.Scripting;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class PlayerActions : GameComponent
{
  private HorizontalDirection oldLookDir;
  private int lastFrame;
  private readonly Dictionary<SurfaceType, SoundEffect[]> SurfaceHits = new Dictionary<SurfaceType, SoundEffect[]>((IEqualityComparer<SurfaceType>) SurfaceTypeComparer.Default);
  private SoundEffect LeftStep;
  private SoundEffect RightStep;
  private bool isLeft;
  public const float PlayerSpeed = 4.7f;
  private readonly List<PlayerAction> LightActions = new List<PlayerAction>();

  public PlayerActions(Game game)
    : base(game)
  {
    this.UpdateOrder = 1;
    WalkTo walkTo = new WalkTo(game);
    ServiceHelper.InjectServices((object) walkTo);
    ServiceHelper.AddComponent((IGameComponent) walkTo, true);
    if (walkTo.UpdateOrder == 0)
    {
      this.LightActions.Add((PlayerAction) walkTo);
      walkTo.Enabled = false;
    }
    this.AddAction((PlayerAction) new Fall(game));
    this.AddAction((PlayerAction) new Idle(game));
    this.AddAction((PlayerAction) new DropDown(game));
    this.AddAction((PlayerAction) new LowerToStraightLedge(game));
    this.AddAction((PlayerAction) new Land(game));
    this.AddAction((PlayerAction) new Slide(game));
    this.AddAction((PlayerAction) new Lift(game));
    this.AddAction((PlayerAction) new Jump(game));
    this.AddAction((PlayerAction) new WalkRun(game));
    this.AddAction((PlayerAction) new Bounce(game));
    this.AddAction((PlayerAction) new ClimbLadder(game));
    this.AddAction((PlayerAction) new ReadSign(game));
    this.AddAction((PlayerAction) new FreeFall(game));
    this.AddAction((PlayerAction) new Die(game));
    this.AddAction((PlayerAction) new Victory(game));
    this.AddAction((PlayerAction) new GrabCornerLedge(game));
    this.AddAction((PlayerAction) new PullUpFromCornerLedge(game));
    this.AddAction((PlayerAction) new LowerToCornerLedge(game));
    this.AddAction((PlayerAction) new Carry(game));
    this.AddAction((PlayerAction) new Throw(game));
    this.AddAction((PlayerAction) new DropTrile(game));
    this.AddAction((PlayerAction) new Suffer(game));
    this.AddAction((PlayerAction) new EnterDoor(game));
    this.AddAction((PlayerAction) new Grab(game));
    this.AddAction((PlayerAction) new Push(game));
    this.AddAction((PlayerAction) new SuckedIn(game));
    this.AddAction((PlayerAction) new ClimbVine(game));
    this.AddAction((PlayerAction) new WakingUp(game));
    this.AddAction((PlayerAction) new Jetpack(game));
    this.AddAction((PlayerAction) new OpenTreasure(game));
    this.AddAction((PlayerAction) new OpenDoor(game));
    this.AddAction((PlayerAction) new Swim(game));
    this.AddAction((PlayerAction) new Sink(game));
    this.AddAction((PlayerAction) new LookAround(game));
    this.AddAction((PlayerAction) new Teeter(game));
    this.AddAction((PlayerAction) new EnterTunnel(game));
    this.AddAction((PlayerAction) new PushPivot(game));
    this.AddAction((PlayerAction) new PullUpFromStraightLedge(game));
    this.AddAction((PlayerAction) new GrabStraightLedge(game));
    this.AddAction((PlayerAction) new ShimmyOnLedge(game));
    this.AddAction((PlayerAction) new ToCornerTransition(game));
    this.AddAction((PlayerAction) new FromCornerTransition(game));
    this.AddAction((PlayerAction) new ToClimbTransition(game));
    this.AddAction((PlayerAction) new ClimbOverLadder(game));
    this.AddAction((PlayerAction) new PivotTombstone(game));
    this.AddAction((PlayerAction) new EnterPipe(game));
    this.AddAction((PlayerAction) new ExitDoor(game));
    this.AddAction((PlayerAction) new LesserWarp(game));
    this.AddAction((PlayerAction) new GateWarp(game));
    this.AddAction((PlayerAction) new SleepWake(game));
    this.AddAction((PlayerAction) new ReadTurnAround(game));
    this.AddAction((PlayerAction) new BellActions(game));
    this.AddAction((PlayerAction) new Crush(game));
    this.AddAction((PlayerAction) new PlayingDrums(game));
    this.AddAction((PlayerAction) new Floating(game));
    this.AddAction((PlayerAction) new Standing(game));
    ServiceHelper.AddComponent((IGameComponent) new PlayerActions.ActionsManager(game, this));
  }

  private void AddAction(PlayerAction action)
  {
    ServiceHelper.AddComponent((IGameComponent) action);
    if (action.UpdateOrder != 0 || action.IsUpdateOverridden || !(action.GetType().Name != "Jump"))
      return;
    this.LightActions.Add(action);
    action.Enabled = false;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.CameraManager.ViewpointChanged += (Action) (() => this.LevelManager.ScreenInvalidated += (Action) (() =>
    {
      this.PhysicsManager.DetermineOverlaps((IComplexPhysicsEntity) this.PlayerManager);
      if (this.CameraManager.Viewpoint.IsOrthographic() && this.CameraManager.LastViewpoint != this.CameraManager.Viewpoint && !this.PlayerManager.HandlesZClamping)
      {
        this.CorrectWallOverlap(true);
        if (this.PhysicsManager.DetermineInBackground((IPhysicsEntity) this.PlayerManager, !this.PlayerManager.IsOnRotato, true, !this.PlayerManager.Climbing && !this.LevelManager.LowPass) && !this.CameraManager.Constrained)
        {
          double num = 4.0 * (this.LevelManager.Descending ? -1.0 : 1.0) / (double) this.CameraManager.PixelsPerTrixel;
          Vector3 vector3 = this.CameraManager.LastViewpoint.ScreenSpaceMask();
          this.CameraManager.Center = vector3 * this.CameraManager.Center + this.PlayerManager.Position * (Vector3.One - vector3);
        }
      }
      this.PhysicsManager.DetermineOverlaps((IComplexPhysicsEntity) this.PlayerManager);
    }));
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      this.LevelManager.ScreenInvalidated += (Action) (() => this.PhysicsManager.HugWalls((IPhysicsEntity) this.PlayerManager, false, false, !this.PlayerManager.Climbing));
      if (!string.IsNullOrEmpty(this.LevelManager.Name))
        return;
      foreach (PlayerAction lightAction in this.LightActions)
        lightAction.Reset();
    });
    foreach (SurfaceType key in Util.GetValues<SurfaceType>())
      this.SurfaceHits.Add(key, this.CMProvider.GetAllIn("Sounds/Gomez\\Footsteps\\" + (object) key).Select<string, SoundEffect>((Func<string, SoundEffect>) (f => this.CMProvider.Global.Load<SoundEffect>(f))).ToArray<SoundEffect>());
    this.LeftStep = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez\\Footsteps\\Left");
    this.RightStep = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez\\Footsteps\\Right");
    this.ScriptingManager.CutsceneSkipped += (Action) (() =>
    {
      while (!this.PlayerManager.CanControl)
        this.PlayerManager.CanControl = true;
    });
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.PlayerManager.Hidden || this.GameState.InCutscene)
      return;
    this.PlayerManager.FreshlyRespawned = false;
    Vector3 position = this.PlayerManager.Position;
    if (!this.PlayerManager.CanControl)
    {
      this.InputManager.SaveState();
      this.InputManager.Reset();
    }
    if (this.CameraManager.Viewpoint != Viewpoint.Perspective && this.CameraManager.ActionRunning && !this.GameState.InMenuCube && !this.GameState.Paused && this.CameraManager.RequestedViewpoint == Viewpoint.None && !this.GameState.InMap && !this.LevelManager.IsInvalidatingScreen)
    {
      if (this.PlayerManager.Action.AllowsLookingDirectionChange() && !FezMath.AlmostEqual(this.InputManager.Movement.X, 0.0f))
      {
        this.oldLookDir = this.PlayerManager.LookingDirection;
        this.PlayerManager.LookingDirection = FezMath.DirectionFromMovement(this.InputManager.Movement.X);
      }
      Vector3 velocity = this.PlayerManager.Velocity;
      this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
      if (this.PlayerManager.Grounded && this.PlayerManager.Ground.NearLow == null)
      {
        TrileInstance farHigh = this.PlayerManager.Ground.FarHigh;
        Vector3 b = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
        float num = (farHigh.Center - farHigh.TransformedSize / 2f * b - (this.PlayerManager.Center + this.PlayerManager.Size / 2f * b)).Dot(b);
        if ((double) num > -0.25)
        {
          this.PlayerManager.Position -= Vector3.UnitY * 0.01f * (float) Math.Sign(this.CollisionManager.GravityFactor);
          if (farHigh.GetRotatedFace(this.CameraManager.Viewpoint.VisibleOrientation()) == CollisionType.AllSides)
          {
            this.PlayerManager.Position += num * b;
            this.PlayerManager.Velocity = velocity * Vector3.UnitY;
          }
          else
            this.PlayerManager.Velocity = velocity;
          this.PlayerManager.GroundedVelocity = new Vector3?(this.PlayerManager.Velocity);
          this.PlayerManager.Ground = new MultipleHits<TrileInstance>();
        }
      }
      this.PlayerManager.RecordRespawnInformation();
      if (!this.PlayerManager.Action.HandlesZClamping() && (this.oldLookDir != this.PlayerManager.LookingDirection || this.PlayerManager.LastAction == ActionType.RunTurnAround) && this.PlayerManager.Action != ActionType.Dropping && this.PlayerManager.Action != ActionType.GrabCornerLedge && this.PlayerManager.Action != ActionType.SuckedIn && this.PlayerManager.Action != ActionType.CrushVertical && this.PlayerManager.Action != ActionType.CrushHorizontal)
        this.CorrectWallOverlap(false);
    }
    if (this.PlayerManager.Grounded)
      this.PlayerManager.IgnoreFreefall = false;
    if (this.PlayerManager.Animation != null && this.lastFrame != this.PlayerManager.Animation.Timing.Frame)
    {
      if (this.PlayerManager.Grounded)
      {
        SurfaceType surfaceType = this.PlayerManager.Ground.First.Trile.SurfaceType;
        if (this.PlayerManager.Action == ActionType.Landing && this.PlayerManager.Animation.Timing.Frame == 0)
          this.PlaySurfaceHit(surfaceType, false);
        else if ((this.PlayerManager.Action == ActionType.PullUpBack || this.PlayerManager.Action == ActionType.PullUpFront || this.PlayerManager.Action == ActionType.PullUpCornerLedge) && this.PlayerManager.Animation.Timing.Frame == 5)
          this.PlaySurfaceHit(surfaceType, false);
        else if (this.PlayerManager.Action.GetAnimationPath() == "Walk")
        {
          if (this.PlayerManager.Animation.Timing.Frame == 1 || this.PlayerManager.Animation.Timing.Frame == 4)
          {
            if (this.PlayerManager.Action != ActionType.Sliding)
            {
              (this.isLeft ? this.LeftStep : this.RightStep).EmitAt(this.PlayerManager.Position, RandomHelper.Between(-0.10000000149011612, 0.10000000149011612), RandomHelper.Between(0.89999997615814209, 1.0));
              this.isLeft = !this.isLeft;
            }
            this.PlaySurfaceHit(surfaceType, false);
          }
        }
        else if (this.PlayerManager.Action == ActionType.Running)
        {
          if (this.PlayerManager.Animation.Timing.Frame == 0 || this.PlayerManager.Animation.Timing.Frame == 3)
            this.PlaySurfaceHit(surfaceType, true);
        }
        else if (this.PlayerManager.CarriedInstance != null)
        {
          if (this.PlayerManager.Action.GetAnimationPath() == "CarryHeavyWalk")
          {
            if (this.PlayerManager.Animation.Timing.Frame == 0 || this.PlayerManager.Animation.Timing.Frame == 4)
              this.PlaySurfaceHit(surfaceType, true);
          }
          else if (this.PlayerManager.Action.GetAnimationPath() == "CarryWalk" && (this.PlayerManager.Animation.Timing.Frame == 3 || this.PlayerManager.Animation.Timing.Frame == 7))
            this.PlaySurfaceHit(surfaceType, true);
        }
        else
          this.isLeft = false;
      }
      else
        this.isLeft = false;
      this.lastFrame = this.PlayerManager.Animation.Timing.Frame;
    }
    if (this.PlayerManager.CanControl)
      return;
    this.InputManager.RecoverState();
  }

  private void PlaySurfaceHit(SurfaceType surfaceType, bool withStep)
  {
    if (withStep)
    {
      (this.isLeft ? this.LeftStep : this.RightStep).EmitAt(this.PlayerManager.Position, RandomHelper.Between(-0.10000000149011612, 0.10000000149011612), RandomHelper.Between(0.89999997615814209, 1.0));
      this.isLeft = !this.isLeft;
    }
    RandomHelper.InList<SoundEffect>(this.SurfaceHits[surfaceType]).EmitAt(this.PlayerManager.Position, RandomHelper.Between(-0.10000000149011612, 0.10000000149011612), RandomHelper.Between(0.89999997615814209, 1.0));
  }

  private void CorrectWallOverlap(bool overcompensate)
  {
    Vector3 vector1 = Vector3.Zero;
    float num = 0.0f;
    foreach (PointCollision pointCollision in this.PlayerManager.CornerCollision)
    {
      TrileInstance deep = pointCollision.Instances.Deep;
      if (deep != null && deep != this.PlayerManager.CarriedInstance && deep.GetRotatedFace(this.CameraManager.VisibleOrientation) == CollisionType.AllSides)
      {
        Vector3 b = this.CameraManager.Viewpoint.SideMask();
        Vector3 vector3 = (deep.Center - this.PlayerManager.Center).Sign();
        Vector3 vector2 = (deep.Center - vector3 * deep.TransformedSize / 2f - pointCollision.Point) * b;
        if ((double) vector2.Abs().Dot(b) > (double) num)
        {
          num = vector2.Abs().Dot(b);
          vector1 = vector2;
        }
      }
    }
    if ((double) num > 0.0)
    {
      Vector3 vector3_1 = vector1 + vector1.Sign() * (1f / 1000f) * 2f;
      this.PlayerManager.Position += vector3_1;
      if (this.PlayerManager.CarriedInstance != null && !this.CameraManager.ViewTransitionReached)
        this.PlayerManager.CarriedInstance.Position += vector3_1;
      if (this.PlayerManager.Velocity.Sign() == -vector1.Sign())
      {
        Vector3 vector3_2 = vector1.Sign().Abs();
        this.PlayerManager.Position -= this.PlayerManager.Velocity * vector3_2;
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity * (Vector3.One - vector3_2);
      }
    }
    Volume volume;
    if (!(this.LevelManager.Name == "BOILEROOM") || !this.LevelManager.Volumes.TryGetValue(3, out volume))
      return;
    BoundingBox boundingBox = volume.BoundingBox;
    boundingBox.Max -= FezMath.XZMask * 1.5f;
    boundingBox.Min += FezMath.XZMask * 1.5f;
    if (boundingBox.Contains(this.PlayerManager.Position) != ContainmentType.Disjoint)
      return;
    Vector3 position = this.PlayerManager.Position;
    position.X = MathHelper.Clamp(position.X, boundingBox.Min.X, boundingBox.Max.X);
    position.Z = MathHelper.Clamp(position.Z, boundingBox.Min.Z, boundingBox.Max.Z);
    this.PlayerManager.Position = position;
  }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IPhysicsManager PhysicsManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  internal IScriptingManager ScriptingManager { get; set; }

  private class ActionsManager : GameComponent
  {
    private readonly PlayerActions Host;

    public ActionsManager(Game game, PlayerActions host)
      : base(game)
    {
      this.Host = host;
    }

    public override void Update(GameTime gameTime)
    {
      if (this.GameState.Paused || this.GameState.Loading || this.GameState.InCutscene || this.GameState.InMap || this.GameState.InFpsMode || this.GameState.InMenuCube)
        return;
      if (!this.PlayerManager.CanControl)
      {
        this.InputManager.SaveState();
        this.InputManager.Reset();
      }
      bool actionNotRunning = !this.CameraManager.ActionRunning;
      foreach (PlayerAction lightAction in this.Host.LightActions)
        lightAction.LightUpdate(gameTime, actionNotRunning);
      if (this.PlayerManager.CanControl)
        return;
      this.InputManager.RecoverState();
    }

    [ServiceDependency]
    public IGameCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public IInputManager InputManager { private get; set; }
  }
}
