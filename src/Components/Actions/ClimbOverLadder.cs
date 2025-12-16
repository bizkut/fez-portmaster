// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ClimbOverLadder
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class ClimbOverLadder(Game game) : PlayerAction(game)
{
  private Vector3 camOrigin;
  private SoundEffect climbOverSound;
  private SoundEffect sLedgeLand;
  private int lastFrame;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.climbOverSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/ClimbOverLadder");
    this.sLedgeLand = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeLand");
  }

  protected override void Begin()
  {
    this.PlayerManager.Velocity = Vector3.Zero;
    this.PlayerManager.Position += this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign() * 1f / 16f;
    this.PlayerManager.Position -= Vector3.UnitY * 0.5f / 16f;
    this.PlayerManager.Position *= 16f;
    this.PlayerManager.Position = this.PlayerManager.Position.Round();
    this.PlayerManager.Position /= 16f;
    this.PlayerManager.Position -= Vector3.UnitY * 0.5f / 16f;
    this.camOrigin = this.CameraManager.Center;
    this.climbOverSound.EmitAt(this.PlayerManager.Position);
    this.lastFrame = -1;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.HeldInstance.PhysicsState != null)
    {
      this.PlayerManager.Velocity = this.PlayerManager.HeldInstance.PhysicsState.Velocity;
      this.camOrigin += this.PlayerManager.HeldInstance.PhysicsState.Velocity;
    }
    Vector3 vector3 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign() * 10f / 16f;
    if (this.lastFrame != this.PlayerManager.Animation.Timing.Frame && this.PlayerManager.Animation.Timing.Frame == 5)
      this.sLedgeLand.EmitAt(this.PlayerManager.Position);
    this.lastFrame = this.PlayerManager.Animation.Timing.Frame;
    if (!this.CameraManager.StickyCam)
    {
      Vector3 constrainedCenter = this.CameraManager.ConstrainedCenter;
    }
    if (this.PlayerManager.Animation.Timing.Ended)
    {
      this.PlayerManager.HeldInstance = (TrileInstance) null;
      this.PlayerManager.Action = ActionType.Idle;
      this.PlayerManager.Position += vector3;
      Vector3 position = this.PlayerManager.Position;
      this.PlayerManager.Position += 0.5f * Vector3.UnitY;
      this.PlayerManager.Velocity = Vector3.Down;
      this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
      if (!this.PlayerManager.Grounded)
      {
        this.PlayerManager.Velocity = Vector3.Zero;
        this.PlayerManager.Position = position;
      }
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.ClimbOverLadder;
}
