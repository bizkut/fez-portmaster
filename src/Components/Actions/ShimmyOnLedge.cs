// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ShimmyOnLedge
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class ShimmyOnLedge(Game game) : PlayerAction(game)
{
  private const float ShimmyingSpeed = 0.15f;
  private SoundEffect shimmySound;
  private int lastFrame;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.shimmySound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeShimmy");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.GrabLedgeFront:
      case ActionType.GrabLedgeBack:
        if ((double) this.InputManager.Movement.X == 0.0 || !this.PlayerManager.Animation.Timing.Ended)
          break;
        this.PlayerManager.Action = this.PlayerManager.Action.FacesBack() ? ActionType.ShimmyBack : ActionType.ShimmyFront;
        break;
    }
  }

  protected override bool Act(TimeSpan elapsed)
  {
    float num1 = FezMath.Saturate(Math.Abs(3f / (float) this.PlayerManager.Animation.Timing.EndFrame - this.PlayerManager.Animation.Timing.Step)) * 2f;
    int frame = this.PlayerManager.Animation.Timing.Frame;
    if (this.lastFrame != frame)
    {
      if (frame == 2)
        this.shimmySound.EmitAt(this.PlayerManager.Position);
      this.lastFrame = frame;
    }
    TrileInstance heldInstance = this.PlayerManager.HeldInstance;
    if (!this.PlayerManager.IsOnRotato)
      this.PlayerManager.HeldInstance = this.PlayerManager.AxisCollision[VerticalDirection.Down].Deep;
    int num2 = this.PlayerManager.HeldInstance == null ? 1 : 0;
    Vector3 b = this.CameraManager.Viewpoint.ForwardVector() * (this.PlayerManager.Background ? -1f : 1f);
    bool flag = this.PlayerManager.HeldInstance != null && this.PlayerManager.HeldInstance.GetRotatedFace(this.CameraManager.Viewpoint.VisibleOrientation()) == CollisionType.None;
    if ((num2 != 0 ? 1 : (!flag ? 0 : ((double) this.PlayerManager.HeldInstance.Position.Dot(b) > (double) heldInstance.Position.Dot(b) ? 1 : 0))) != 0)
    {
      this.PlayerManager.Action = this.PlayerManager.Action.FacesBack() ? ActionType.ToCornerBack : ActionType.ToCornerFront;
      this.PlayerManager.HeldInstance = heldInstance;
      this.PlayerManager.LookingDirection = this.PlayerManager.LookingDirection.GetOpposite();
      return false;
    }
    if (flag)
    {
      this.PlayerManager.Action = ActionType.Dropping;
      this.PlayerManager.HeldInstance = (TrileInstance) null;
      return false;
    }
    float num3 = (float) ((double) this.InputManager.Movement.X * 4.6999998092651367 * 0.15000000596046448) * (float) elapsed.TotalSeconds;
    if (this.PlayerManager.Action != ActionType.ShimmyBack && this.PlayerManager.Action != ActionType.ShimmyFront)
      num3 *= 0.6f;
    this.PlayerManager.Velocity = num3 * this.CameraManager.Viewpoint.RightVector() * (1f + num1);
    if ((double) this.InputManager.Movement.X == 0.0)
      this.PlayerManager.Action = this.PlayerManager.Action.FacesBack() ? ActionType.GrabLedgeBack : ActionType.GrabLedgeFront;
    else
      this.PlayerManager.GroundedVelocity = new Vector3?(this.PlayerManager.Velocity);
    if (this.InputManager.RotateLeft == FezButtonState.Pressed || this.InputManager.RotateRight == FezButtonState.Pressed)
      this.PlayerManager.Action = this.PlayerManager.Action.FacesBack() ? ActionType.GrabLedgeBack : ActionType.GrabLedgeFront;
    if (this.PlayerManager.Action == ActionType.ShimmyBack || this.PlayerManager.Action == ActionType.ShimmyFront)
    {
      this.PlayerManager.Animation.Timing.Update(elapsed, Math.Abs(this.InputManager.Movement.X));
      if (this.PlayerManager.HeldInstance.PhysicsState != null)
        this.PlayerManager.Position += this.PlayerManager.HeldInstance.PhysicsState.Velocity;
    }
    Vector3 vector3 = this.CameraManager.Viewpoint.DepthMask();
    this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + this.PlayerManager.HeldInstance.Center * vector3 + b * -(this.PlayerManager.HeldInstance.TransformedSize / 2f + this.PlayerManager.Size.X * vector3 / 4f);
    this.PhysicsManager.HugWalls((IPhysicsEntity) this.PlayerManager, false, false, true);
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    switch (type)
    {
      case ActionType.GrabLedgeFront:
      case ActionType.GrabLedgeBack:
        return (double) this.InputManager.Movement.X != 0.0;
      case ActionType.ShimmyFront:
      case ActionType.ShimmyBack:
        return true;
      default:
        return false;
    }
  }
}
