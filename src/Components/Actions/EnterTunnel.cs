// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.EnterTunnel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class EnterTunnel(Game game) : PlayerAction(game)
{
  private SoundEffect SwooshRight;
  private Vector3 originalForward;
  private Vector3 originalPosition;
  private float distanceToCover;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.SwooshRight = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateCenter");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.CarryIdle:
      case ActionType.CarryWalk:
      case ActionType.CarrySlide:
      case ActionType.CarryHeavyIdle:
      case ActionType.CarryHeavyWalk:
      case ActionType.CarryHeavySlide:
      case ActionType.Sliding:
      case ActionType.Teetering:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (!this.PlayerManager.TunnelVolume.HasValue || this.PlayerManager.Background || this.InputManager.ExactUp != FezButtonState.Pressed)
          break;
        if (this.PlayerManager.CarriedInstance == null)
        {
          this.WalkTo.Destination = new Func<Vector3>(this.GetDestination);
          this.PlayerManager.Action = ActionType.WalkingTo;
          this.WalkTo.NextAction = ActionType.EnteringTunnel;
          break;
        }
        Vector3 position = this.PlayerManager.Position;
        this.PlayerManager.Position = this.GetDestination();
        this.PlayerManager.CarriedInstance.Position += this.PlayerManager.Position - position;
        this.PlayerManager.Action = this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsHeavy() ? ActionType.EnterTunnelCarryHeavy : ActionType.EnterTunnelCarry;
        break;
    }
  }

  private Vector3 GetDestination()
  {
    if (!this.PlayerManager.TunnelVolume.HasValue)
      return this.PlayerManager.Position;
    Volume volume = this.LevelManager.Volumes[this.PlayerManager.TunnelVolume.Value];
    Vector3 vector3 = (volume.From + volume.To) / 2f;
    return this.PlayerManager.Position * (Vector3.UnitY + this.CameraManager.Viewpoint.DepthMask()) + vector3 * this.CameraManager.Viewpoint.SideMask();
  }

  protected override void Begin()
  {
    this.SwooshRight.Emit();
    this.originalForward = this.CameraManager.Viewpoint.ForwardVector();
    this.CameraManager.ChangeViewpoint(this.CameraManager.Viewpoint.GetRotatedView(2));
    this.PlayerManager.LookingDirection = HorizontalDirection.Right;
    this.PlayerManager.Velocity = Vector3.Zero;
    this.originalPosition = this.PlayerManager.Position;
    this.distanceToCover = (this.LevelManager.NearestTrile(this.PlayerManager.Ground.First.Center, QueryOptions.None, new Viewpoint?(this.CameraManager.Viewpoint)).Deep.Center - this.originalPosition).Dot(this.originalForward);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if ((double) this.CameraManager.ViewTransitionStep != 0.0)
    {
      Vector3 position = this.PlayerManager.Position;
      this.PlayerManager.Position = this.originalPosition + Vector3.Lerp(Vector3.Zero, this.originalForward * this.distanceToCover, this.CameraManager.ViewTransitionStep);
      if (this.PlayerManager.CarriedInstance != null)
        this.PlayerManager.CarriedInstance.Position += this.PlayerManager.Position - position;
    }
    if (!this.CameraManager.ActionRunning)
      return true;
    this.PlayerManager.Action = this.PlayerManager.Action == ActionType.EnteringTunnel ? ActionType.Idle : (this.PlayerManager.Action == ActionType.EnterTunnelCarry ? ActionType.CarryIdle : ActionType.CarryHeavyIdle);
    this.PlayerManager.Background = false;
    return false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return this.PlayerManager.Action == ActionType.EnteringTunnel || this.PlayerManager.Action == ActionType.EnterTunnelCarry || this.PlayerManager.Action == ActionType.EnterTunnelCarryHeavy;
  }

  protected override bool ViewTransitionIndependent => true;
}
