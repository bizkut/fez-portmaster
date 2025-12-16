// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.SuckedIn
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

public class SuckedIn(Game game) : PlayerAction(game)
{
  private SoundEffect suckedSound;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.suckedSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/SuckedIn");
  }

  protected override void TestConditions()
  {
    if (this.PlayerManager.Action == ActionType.SuckedIn || this.PlayerManager.Action.IsEnteringDoor() || this.PlayerManager.Action == ActionType.OpeningTreasure || this.PlayerManager.Action == ActionType.FindingTreasure)
      return;
    foreach (Volume currentVolume in this.PlayerManager.CurrentVolumes)
    {
      if (currentVolume.ActorSettings != null && currentVolume.ActorSettings.IsBlackHole)
      {
        Vector3 vector3_1 = currentVolume.To - currentVolume.From;
        Vector3 vector3_2 = (currentVolume.From + currentVolume.To) / 2f - vector3_1 / 2f * this.CameraManager.Viewpoint.ForwardVector();
        this.PlayerManager.Action = ActionType.SuckedIn;
        this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + vector3_2 * this.CameraManager.Viewpoint.DepthMask() + -0.25f * this.CameraManager.Viewpoint.ForwardVector();
        currentVolume.ActorSettings.Sucking = true;
        break;
      }
    }
  }

  protected override void Begin()
  {
    base.Begin();
    this.PlayerManager.LookingDirection = this.PlayerManager.LookingDirection.GetOpposite();
    this.PlayerManager.CarriedInstance = (TrileInstance) null;
    this.PlayerManager.Action = ActionType.SuckedIn;
    this.PlayerManager.Ground = new MultipleHits<TrileInstance>();
    this.suckedSound.EmitAt(this.PlayerManager.Position);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.Animation.Timing.Ended)
      this.PlayerManager.Respawn();
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.SuckedIn;
}
