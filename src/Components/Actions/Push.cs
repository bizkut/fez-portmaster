// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Push
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Push(Game game) : PlayerAction(game)
{
  private readonly MovementHelper movementHelper = new MovementHelper(1.87999988f, 0.0f, float.MaxValue);
  private TrileGroup pickupGroup;
  private SoundEffect sCratePush;
  private SoundEffect sGomezPush;
  private SoundEmitter eCratePush;
  private SoundEmitter eGomezPush;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sCratePush = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/PushPickup");
    this.sGomezPush = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/GomezPush");
  }

  protected override void Begin()
  {
    base.Begin();
    if (this.eCratePush == null || this.eCratePush.Dead)
      this.eCratePush = this.sCratePush.EmitAt(this.PlayerManager.PushedInstance.Center, true);
    else
      this.eCratePush.Cue.Resume();
    if (this.eGomezPush == null || this.eGomezPush.Dead)
      this.eGomezPush = this.sGomezPush.EmitAt(this.PlayerManager.Position, true);
    else
      this.eGomezPush.Cue.Resume();
    if (this.LevelManager.PickupGroups.TryGetValue(this.PlayerManager.PushedInstance, out this.pickupGroup))
      return;
    this.pickupGroup = (TrileGroup) null;
  }

  protected override void TestConditions()
  {
    base.TestConditions();
    if (this.PlayerManager.Action != ActionType.Pushing && this.eCratePush != null && !this.eCratePush.Dead && this.eCratePush.Cue.State != SoundState.Paused)
      this.eCratePush.Cue.Pause();
    if (this.PlayerManager.Action == ActionType.Pushing || this.eGomezPush == null || this.eGomezPush.Dead || this.eGomezPush.Cue.State == SoundState.Paused)
      return;
    this.eGomezPush.Cue.Pause();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.PushedInstance == null || this.PlayerManager.PushedInstance.Hidden || this.PlayerManager.PushedInstance.PhysicsState == null)
    {
      this.PlayerManager.Action = ActionType.Idle;
      this.PlayerManager.PushedInstance = (TrileInstance) null;
      return false;
    }
    Vector3 b = this.CameraManager.Viewpoint.SideMask();
    Vector3 vector3 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    TrileInstance pushedInstance = this.PlayerManager.PushedInstance;
    InstancePhysicsState physicsState = pushedInstance.PhysicsState;
    this.eCratePush.Position = pushedInstance.Center;
    this.eGomezPush.Position = this.PlayerManager.Center;
    if (!physicsState.Grounded)
    {
      this.PlayerManager.PushedInstance = (TrileInstance) null;
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    int stackSize = this.FindStackSize(pushedInstance, 0);
    if (stackSize <= 2)
    {
      this.movementHelper.Entity = (IPhysicsEntity) physicsState;
      float x = this.InputManager.Movement.X;
      if (physicsState.WallCollision.AnyCollided() && physicsState.WallCollision.First.Destination.Trile.ActorSettings.Type.IsPickable())
        x *= 5f;
      if (pushedInstance.Trile.ActorSettings.Type == ActorType.Couch)
        x *= 2f;
      this.movementHelper.Update((float) elapsed.TotalSeconds, x / (float) (stackSize + 1));
      if (this.pickupGroup != null)
      {
        pushedInstance.PhysicsState.Puppet = false;
        foreach (TrileInstance trile in this.pickupGroup.Triles)
        {
          if (trile != pushedInstance)
          {
            trile.PhysicsState.Velocity = pushedInstance.PhysicsState.Velocity;
            trile.PhysicsState.Puppet = true;
          }
        }
      }
      this.PlayerManager.Center = Vector3.Up * this.PlayerManager.Center + (this.CameraManager.Viewpoint.DepthMask() + b) * physicsState.Center + -vector3 * (pushedInstance.TransformedSize / 2f + this.PlayerManager.Size / 2f);
      this.eCratePush.VolumeFactor = FezMath.Saturate(Math.Abs(physicsState.Velocity.Dot(b)) / 0.024f);
      if (FezMath.AlmostEqual(physicsState.Velocity.Dot(b), 0.0f))
      {
        this.PlayerManager.Action = ActionType.Grabbing;
        return false;
      }
    }
    else
    {
      this.PlayerManager.Action = ActionType.Grabbing;
      if (!this.eCratePush.Dead)
        this.eCratePush.Cue.Pause();
      if (!this.eGomezPush.Dead)
        this.eGomezPush.Cue.Pause();
    }
    return this.PlayerManager.Action == ActionType.Pushing;
  }

  private int FindStackSize(TrileInstance instance, int stackSize)
  {
    Vector3 halfSize = instance.TransformedSize / 2f - new Vector3(0.004f);
    MultipleHits<CollisionResult> result = this.CollisionManager.CollideEdge(instance.Center, instance.Trile.Size.Y * Vector3.Up, halfSize, Direction2D.Vertical);
    if (result.AnyCollided())
    {
      TrileInstance destination = result.First.Destination;
      if (destination.PhysicsState != null && destination.PhysicsState.Grounded)
        return this.FindStackSize(destination, stackSize + 1);
    }
    return stackSize;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Pushing;
}
