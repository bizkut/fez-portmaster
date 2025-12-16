// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.DropDown
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;

#nullable disable
namespace FezGame.Components.Actions;

public class DropDown : PlayerAction
{
  public const float DroppingSpeed = 0.05f;
  private SoundEffect dropLedgeSound;
  private SoundEffect dropVineSound;
  private SoundEffect dropLadderSound;

  public DropDown(Game game)
    : base(game)
  {
    this.UpdateOrder = 2;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.dropLedgeSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/LedgeDrop");
    this.dropVineSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/DropFromVine");
    this.dropLadderSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/DropFromLadder");
  }

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.FrontClimbingLadder:
      case ActionType.BackClimbingLadder:
      case ActionType.SideClimbingLadder:
      case ActionType.FrontClimbingVine:
      case ActionType.SideClimbingVine:
      case ActionType.BackClimbingVine:
      case ActionType.GrabCornerLedge:
      case ActionType.GrabLedgeFront:
      case ActionType.GrabLedgeBack:
      case ActionType.LowerToLedge:
      case ActionType.ToCornerFront:
      case ActionType.ToCornerBack:
      case ActionType.FromCornerBack:
        if ((this.InputManager.Jump != FezButtonState.Pressed || !this.InputManager.Down.IsDown()) && (!this.PlayerManager.Action.IsOnLedge() || this.InputManager.Down != FezButtonState.Pressed) || !FezMath.AlmostEqual(this.InputManager.Movement.X, 0.0f))
          break;
        this.PlayerManager.HeldInstance = (TrileInstance) null;
        this.PlayerManager.Action = ActionType.Dropping;
        this.PlayerManager.CanDoubleJump = false;
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    if (this.PlayerManager.LastAction.IsClimbingLadder())
      this.dropLadderSound.EmitAt(this.PlayerManager.Position);
    else if (this.PlayerManager.LastAction.IsClimbingVine())
      this.dropVineSound.EmitAt(this.PlayerManager.Position);
    else if (this.PlayerManager.LastAction.IsOnLedge())
    {
      this.dropLedgeSound.EmitAt(this.PlayerManager.Position);
      this.GomezService.OnDropLedge();
      Vector3 position = this.PlayerManager.Position;
      if (this.PlayerManager.LastAction == ActionType.GrabCornerLedge || this.PlayerManager.LastAction == ActionType.LowerToCornerLedge)
        this.PlayerManager.Position += this.CameraManager.Viewpoint.RightVector() * (float) -this.PlayerManager.LookingDirection.Sign() * 0.5f;
      this.PhysicsManager.DetermineInBackground((IPhysicsEntity) this.PlayerManager, true, false, false);
      this.PlayerManager.Position = position;
    }
    if (this.PlayerManager.Grounded)
    {
      this.PlayerManager.Position -= Vector3.UnitY * 0.01f;
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity - 0.0075000003f * Vector3.UnitY;
    }
    else
      this.PlayerManager.Velocity = Vector3.Zero;
    if (this.PlayerManager.LastAction != ActionType.GrabCornerLedge)
      return;
    this.PlayerManager.Position += this.CameraManager.Viewpoint.RightVector() * (float) -this.PlayerManager.LookingDirection.Sign() * (15f / 32f);
    this.PlayerManager.ForceOverlapsDetermination();
    NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.PlayerManager.Position - 1f / 500f * Vector3.UnitY);
    if (nearestTriles.Surface == null || nearestTriles.Surface.Trile.ActorSettings.Type != ActorType.Vine)
      return;
    this.PlayerManager.Action = ActionType.SideClimbingVine;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Dropping;
}
