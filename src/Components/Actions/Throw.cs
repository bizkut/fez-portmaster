// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Throw
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

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

public class Throw(Game game) : PlayerAction(game)
{
  private static readonly Vector2[] LightOffsets = new Vector2[2]
  {
    new Vector2(-1f, -1f),
    new Vector2(1f, 2f)
  };
  private static readonly Vector2[] HeavyOffsets = new Vector2[4]
  {
    new Vector2(1f, 1f),
    new Vector2(1f, 0.0f),
    new Vector2(2f, 0.0f),
    new Vector2(7f, 4f)
  };
  private const float ThrowStrength = 0.08f;
  private bool thrown;
  private SoundEffect throwHeavySound;
  private SoundEffect throwLightSound;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.throwHeavySound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/ThrowHeavyPickup");
    this.throwLightSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/ThrowLightPickup");
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
        if (this.PlayerManager.Background || this.InputManager.GrabThrow != FezButtonState.Pressed || this.InputManager.Down == FezButtonState.Down && !FezMath.AlmostEqual(this.InputManager.Movement, Vector2.Zero, 0.5f))
          break;
        this.PlayerManager.Action = this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsLight() ? ActionType.Throwing : ActionType.ThrowingHeavy;
        this.thrown = false;
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    if (this.PlayerManager.CarriedInstance == null)
    {
      this.PlayerManager.Action = ActionType.Idle;
    }
    else
    {
      if (this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsHeavy())
        this.throwHeavySound.EmitAt(this.PlayerManager.Position);
      else
        this.throwLightSound.EmitAt(this.PlayerManager.Position);
      this.GomezService.OnThrowObject();
    }
  }

  protected override bool Act(TimeSpan elapsed)
  {
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    this.PlayerManager.Animation.Timing.Update(elapsed);
    if (this.PlayerManager.CarriedInstance != null)
    {
      if (this.PlayerManager.CarriedInstance.PhysicsState == null)
        this.PlayerManager.CarriedInstance = (TrileInstance) null;
      bool flag = this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsLight();
      Vector2[] vector2Array = flag ? Throw.LightOffsets : Throw.HeavyOffsets;
      if (!flag)
      {
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
      }
      if (this.PlayerManager.Animation.Timing.Frame < vector2Array.Length)
      {
        int frame = this.PlayerManager.Animation.Timing.Frame;
        TrileInstance carriedInstance = this.PlayerManager.CarriedInstance;
        Vector2 vector2 = vector2Array[frame];
        Vector3 vector3_2 = this.PlayerManager.Center + this.PlayerManager.Size / 2f * (Vector3.Down + vector3_1) - carriedInstance.TransformedSize / 2f * vector3_1 + carriedInstance.Trile.Size / 2f * (Vector3.UnitY + vector3_1) - vector3_1 * 8f / 16f + Vector3.UnitY * 9f / 16f;
        if (flag)
          vector3_2 += vector3_1 * 1f / 16f + Vector3.UnitY * 2f / 16f;
        Vector3 vector3_3 = vector3_2 + (vector2.X * vector3_1 + vector2.Y * Vector3.Up) * (1f / 16f);
        Vector3 vector3_4 = vector3_3 - carriedInstance.Center;
        carriedInstance.PhysicsState.Velocity = vector3_4;
        carriedInstance.PhysicsState.UpdatingPhysics = true;
        this.PhysicsManager.Update((ISimplePhysicsEntity) carriedInstance.PhysicsState, false, false);
        carriedInstance.PhysicsState.UpdatingPhysics = false;
        carriedInstance.PhysicsState.UpdateInstance();
        carriedInstance.PhysicsState.Velocity = Vector3.Zero;
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity - (vector3_3 - carriedInstance.Center);
      }
      else if (!this.thrown)
      {
        this.thrown = true;
        this.PlayerManager.CarriedInstance.Phi = FezMath.SnapPhi(this.PlayerManager.CarriedInstance.Phi);
        this.PlayerManager.CarriedInstance.PhysicsState.Background = false;
        this.PlayerManager.CarriedInstance.PhysicsState.Velocity = this.PlayerManager.Velocity * 0.5f + ((float) this.PlayerManager.LookingDirection.Sign() * this.CameraManager.Viewpoint.RightVector() + Vector3.Up) * 0.08f;
        this.PlayerManager.CarriedInstance = (TrileInstance) null;
      }
    }
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      this.thrown = false;
      this.PlayerManager.SyncCollisionSize();
      this.PlayerManager.Action = ActionType.Idle;
    }
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.Throwing || type == ActionType.ThrowingHeavy;
  }
}
