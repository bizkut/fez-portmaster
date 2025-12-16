// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Teeter
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Teeter(Game game) : PlayerAction(game)
{
  private SoundEffect sBegin;
  private SoundEffect sMouthOpen;
  private SoundEffect sMouthClose;
  private SoundEmitter eLast;
  private int lastFrame;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sBegin = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TeeterBegin");
    this.sMouthOpen = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TeeterMouthOpen");
    this.sMouthClose = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/TeeterMouthClose");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.Dropping:
      case ActionType.Sliding:
      case ActionType.Grabbing:
      case ActionType.Pushing:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (this.PlayerManager.PushedInstance != null || this.PlayerManager.CarriedInstance != null || !this.PlayerManager.Grounded || this.PlayerManager.Ground.FarHigh != null || (double) this.InputManager.Movement.X != 0.0)
          break;
        Vector3 b = this.CameraManager.Viewpoint.SideMask();
        TrileInstance nearLow = this.PlayerManager.Ground.NearLow;
        float num = Math.Abs(nearLow.Center.Dot(b) - this.PlayerManager.Position.Dot(b));
        if ((double) num > 1.0 || (double) num <= 0.44999998807907104 || this.CollisionManager.CollideEdge(nearLow.Center, Vector3.Down * (float) Math.Sign(this.CollisionManager.GravityFactor), this.PlayerManager.Size * FezMath.XZMask / 2f, Direction2D.Vertical).AnyHit())
          break;
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity * new Vector3(0.5f, 1f, 0.5f);
        this.PlayerManager.Action = ActionType.Teetering;
        break;
    }
  }

  protected override void Begin()
  {
    this.lastFrame = -1;
    base.Begin();
  }

  protected override bool Act(TimeSpan elapsed)
  {
    int frame = this.PlayerManager.Animation.Timing.Frame;
    if (this.lastFrame != frame)
    {
      switch (frame)
      {
        case 0:
          this.eLast = this.sBegin.EmitAt(this.PlayerManager.Position);
          break;
        case 6:
          this.eLast = this.sMouthOpen.EmitAt(this.PlayerManager.Position);
          break;
        case 9:
          this.eLast = this.sMouthClose.EmitAt(this.PlayerManager.Position);
          break;
      }
    }
    this.lastFrame = frame;
    if (this.eLast != null && !this.eLast.Dead)
      this.eLast.Position = this.PlayerManager.Position;
    return true;
  }

  protected override void End()
  {
    if (this.eLast != null && !this.eLast.Dead)
    {
      this.eLast.Cue.Stop();
      this.eLast = (SoundEmitter) null;
    }
    base.End();
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Teetering;
}
