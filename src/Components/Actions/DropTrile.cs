// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.DropTrile
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
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

internal class DropTrile(Game game) : PlayerAction(game)
{
  private SoundEffect dropHeavySound;
  private SoundEffect dropLightSound;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.dropHeavySound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/DropHeavyPickup");
    this.dropLightSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/DropLightPickup");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.CarryIdle:
      case ActionType.CarryWalk:
      case ActionType.CarryJump:
      case ActionType.CarrySlide:
      case ActionType.CarryHeavyIdle:
      case ActionType.CarryHeavyWalk:
      case ActionType.CarryHeavyJump:
      case ActionType.CarryHeavySlide:
        if ((this.PlayerManager.Background || this.InputManager.GrabThrow != FezButtonState.Pressed ? 0 : ((double) this.InputManager.Movement.Y < -0.5 ? 1 : ((double) Math.Abs(this.InputManager.Movement.X) < 0.25 ? 1 : 0))) == 0)
          break;
        TrileInstance carriedInstance = this.PlayerManager.CarriedInstance;
        this.PlayerManager.Action = carriedInstance.Trile.ActorSettings.Type.IsLight() ? ActionType.DropTrile : ActionType.DropHeavyTrile;
        Vector3 vector3_1 = this.CameraManager.Viewpoint.SideMask();
        Vector3 vector3_2 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
        Vector3 vector3_3 = this.PlayerManager.Center + this.PlayerManager.Size / 2f * (Vector3.Down + vector3_2) - carriedInstance.TransformedSize / 2f * vector3_2 + carriedInstance.Trile.Size / 2f * (Vector3.UnitY + vector3_2) + 0.125f * vector3_2;
        carriedInstance.Enabled = false;
        MultipleHits<CollisionResult> result = this.CollisionManager.CollideEdge(carriedInstance.Center, vector3_3 - carriedInstance.Center, carriedInstance.TransformedSize / 2f, Direction2D.Horizontal);
        if (result.AnyCollided())
        {
          CollisionResult collisionResult = result.NearLow;
          if (!collisionResult.Collided || collisionResult.Destination.GetRotatedFace(this.CameraManager.VisibleOrientation) != CollisionType.AllSides || (double) Math.Abs(collisionResult.Destination.Center.Y - vector3_3.Y) >= 1.0)
            collisionResult = result.FarHigh;
          if (collisionResult.Collided && collisionResult.Destination.GetRotatedFace(this.CameraManager.VisibleOrientation) == CollisionType.AllSides && (double) Math.Abs(collisionResult.Destination.Center.Y - vector3_3.Y) < 1.0)
          {
            TrileInstance destination = collisionResult.Destination;
            Vector3 vector3_4 = destination.Center - vector3_2 * destination.TransformedSize / 2f;
            Vector3 vector3_5 = vector3_3 + vector3_2 * carriedInstance.TransformedSize / 2f;
            this.PlayerManager.Position -= vector3_1 * (vector3_5 - vector3_4);
          }
        }
        carriedInstance.Enabled = true;
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    if (this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsHeavy())
      this.dropHeavySound.EmitAt(this.PlayerManager.Position);
    else
      this.dropLightSound.EmitAt(this.PlayerManager.Position);
    this.GomezService.OnDropObject();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.CarriedInstance == null)
    {
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    TrileInstance carriedInstance = this.PlayerManager.CarriedInstance;
    Vector3 vector3_2 = this.PlayerManager.Center + this.PlayerManager.Size / 2f * (Vector3.Down + vector3_1) - carriedInstance.TransformedSize / 2f * vector3_1 + carriedInstance.Trile.Size / 2f * (Vector3.UnitY + vector3_1) + 0.125f * vector3_1;
    bool flag = carriedInstance.Trile.ActorSettings.Type.IsLight();
    Vector2[] vector2Array = flag ? Lift.LightTrilePositioning : Lift.HeavyTrilePositioning;
    int index = (flag ? 4 : 7) - this.PlayerManager.Animation.Timing.Frame;
    Vector3 vector3_3 = (vector2Array[index].X * -vector3_1 + vector2Array[index].Y * Vector3.Up) * 1f / 16f;
    Vector3 vector3_4 = vector3_2 + vector3_3;
    carriedInstance.PhysicsState.Center = vector3_4;
    carriedInstance.PhysicsState.UpdateInstance();
    this.PlayerManager.Position -= vector3_4 - carriedInstance.Center;
    this.PlayerManager.CarriedInstance.PhysicsState.UpdateInstance();
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      Vector3 impulse = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448) * (float) elapsed.TotalSeconds * Vector3.Down;
      if ((double) this.PlayerManager.GroundMovement.Y < 0.0)
        impulse += this.PlayerManager.GroundMovement;
      MultipleHits<CollisionResult> result = this.CollisionManager.CollideEdge(carriedInstance.PhysicsState.Center, impulse, carriedInstance.TransformedSize / 2f, Direction2D.Vertical);
      if (result.AnyCollided())
      {
        carriedInstance.PhysicsState.Ground = new MultipleHits<TrileInstance>()
        {
          NearLow = result.NearLow.Collided ? result.NearLow.Destination : (TrileInstance) null,
          FarHigh = result.FarHigh.Collided ? result.FarHigh.Destination : (TrileInstance) null
        };
        MultipleHits<TrileInstance> ground = carriedInstance.PhysicsState.Ground;
        if (ground.First.PhysicsState != null)
        {
          InstancePhysicsState physicsState = carriedInstance.PhysicsState;
          ground = carriedInstance.PhysicsState.Ground;
          Vector3 velocity = ground.First.PhysicsState.Velocity;
          physicsState.GroundMovement = velocity;
          carriedInstance.PhysicsState.Center += carriedInstance.PhysicsState.GroundMovement;
        }
      }
      carriedInstance.PhysicsState.Velocity = impulse;
      carriedInstance.PhysicsState.UpdateInstance();
      if (flag)
      {
        this.PlayerManager.Action = ActionType.Idle;
      }
      else
      {
        this.PlayerManager.PushedInstance = this.PlayerManager.CarriedInstance;
        this.PlayerManager.Action = ActionType.Grabbing;
      }
      this.PlayerManager.CarriedInstance = (TrileInstance) null;
      this.PhysicsManager.HugWalls((IPhysicsEntity) this.PlayerManager, false, false, true);
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.DropTrile || type == ActionType.DropHeavyTrile;
  }
}
