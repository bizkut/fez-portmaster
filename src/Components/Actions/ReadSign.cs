// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.ReadSign
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class ReadSign(Game game) : PlayerAction(game)
{
  private string signText;
  private SoundEffect sTextNext;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sTextNext = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/TextNext");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.LookingLeft:
      case ActionType.LookingRight:
      case ActionType.LookingUp:
      case ActionType.LookingDown:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.Sliding:
      case ActionType.Landing:
      case ActionType.Teetering:
      case ActionType.IdlePlay:
      case ActionType.IdleSleep:
      case ActionType.IdleLookAround:
      case ActionType.IdleYawn:
        if (!this.IsOnSign() || this.InputManager.CancelTalk != FezButtonState.Pressed)
          break;
        this.SpeechBubble.Origin = this.PlayerManager.Position;
        this.SpeechBubble.ChangeText(GameText.GetString(this.signText));
        this.PlayerManager.Action = ActionType.ReadingSign;
        this.InputManager.PressedToDown();
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    this.GomezService.OnReadSign();
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
  }

  private bool IsOnSign()
  {
    return this.TestSignCollision(VerticalDirection.Up) || this.TestSignCollision(VerticalDirection.Down);
  }

  private bool TestSignCollision(VerticalDirection direction)
  {
    TrileInstance surface = this.PlayerManager.AxisCollision[direction].Surface;
    if (surface == null)
      return false;
    Trile trile = surface.Trile;
    FaceOrientation faceOrientation = FezMath.OrientationFromPhi(trile.ActorSettings.Face.ToPhi() + surface.Phi);
    int num = trile.ActorSettings.Type != ActorType.Sign || faceOrientation != this.CameraManager.VisibleOrientation || !(surface.ActorSettings != (InstanceActorSettings) null) ? 0 : (!string.IsNullOrEmpty(surface.ActorSettings.SignText) ? 1 : 0);
    if (num == 0)
      return num != 0;
    this.signText = surface.ActorSettings.SignText;
    return num != 0;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.InputManager.CancelTalk == FezButtonState.Pressed)
    {
      this.sTextNext.Emit();
      this.SpeechBubble.Hide();
      this.PlayerManager.Action = ActionType.Idle;
      this.InputManager.PressedToDown();
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.ReadingSign;

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }
}
