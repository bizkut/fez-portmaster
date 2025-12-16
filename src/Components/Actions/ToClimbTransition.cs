// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ToClimbTransition
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class ToClimbTransition(Game game) : PlayerAction(game)
{
  private TimeSpan? sinceGrabbed;
  private SoundEffect grabLadderSound;
  private SoundEffect grabVineSound;

  protected override void LoadContent()
  {
    this.grabLadderSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/GrabLadder");
    this.grabVineSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/GrabVine");
  }

  protected override void Begin()
  {
    this.PlayerManager.Velocity = Vector3.Zero;
    this.sinceGrabbed = new TimeSpan?(TimeSpan.Zero);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.sinceGrabbed.HasValue)
    {
      TimeSpan? sinceGrabbed = this.sinceGrabbed;
      TimeSpan timeSpan = elapsed;
      this.sinceGrabbed = sinceGrabbed.HasValue ? new TimeSpan?(sinceGrabbed.GetValueOrDefault() + timeSpan) : new TimeSpan?();
      if (this.sinceGrabbed.Value.TotalSeconds >= (this.PlayerManager.Action == ActionType.JumpToClimb || this.PlayerManager.Action == ActionType.JumpToSideClimb ? 0.16 : 0.32))
      {
        this.PlayerManager.Velocity = Vector3.Zero;
        if (this.PlayerManager.NextAction.IsClimbingLadder())
          this.grabLadderSound.EmitAt(this.PlayerManager.Position);
        else if (this.PlayerManager.NextAction.IsClimbingVine())
          this.grabVineSound.EmitAt(this.PlayerManager.Position);
        this.sinceGrabbed = new TimeSpan?();
      }
    }
    if (this.PlayerManager.NextAction.IsClimbingLadder() || this.PlayerManager.NextAction == ActionType.SideClimbingVine)
      this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.PlayerManager.Position * Vector3.UnitY + (this.PlayerManager.HeldInstance.Position + FezMath.HalfVector) * FezMath.XZMask, Easing.EaseIn((double) FezMath.Saturate(this.PlayerManager.Animation.Timing.NormalizedStep * 2f), EasingType.Quadratic));
    if ((double) this.PlayerManager.Velocity.Y > 0.0)
    {
      Vector3 vector3 = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448) * (float) elapsed.TotalSeconds * -Vector3.UnitY;
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity + vector3;
    }
    if (!this.PlayerManager.Animation.Timing.Ended)
      return true;
    this.PlayerManager.Action = this.PlayerManager.NextAction != ActionType.None ? this.PlayerManager.NextAction : throw new InvalidOperationException();
    this.PlayerManager.NextAction = ActionType.None;
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.IdleToClimb || type == ActionType.JumpToClimb || type == ActionType.IdleToFrontClimb || type == ActionType.IdleToSideClimb || type == ActionType.JumpToSideClimb;
  }
}
