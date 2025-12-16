// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Carry
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

public class Carry(Game game) : PlayerAction(game)
{
  private static readonly Vector2[] LightWalkOffset = new Vector2[8]
  {
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, 2f),
    new Vector2(0.0f, 3f),
    new Vector2(0.0f, 2f),
    new Vector2(0.0f, -1f),
    new Vector2(0.0f, 2f),
    new Vector2(0.0f, 3f),
    new Vector2(0.0f, 2f)
  };
  private static readonly Vector2[] HeavyWalkOffset = new Vector2[8]
  {
    new Vector2(0.0f, -1f),
    new Vector2(0.0f, -3f),
    new Vector2(0.0f, -2f),
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, -1f),
    new Vector2(0.0f, -3f),
    new Vector2(0.0f, -2f),
    new Vector2(0.0f, 0.0f)
  };
  private static readonly Vector2[] LightJumpOffset = new Vector2[8]
  {
    new Vector2(1f, -3f),
    new Vector2(0.0f, 3f),
    new Vector2(1f, 2f),
    new Vector2(1f, -2f),
    new Vector2(1f, 0.0f),
    new Vector2(1f, 2f),
    new Vector2(1f, -3f),
    new Vector2(1f, -2f)
  };
  private static readonly Vector2[] HeavyJumpOffset = new Vector2[8]
  {
    new Vector2(-1f, -3f),
    new Vector2(0.0f, 3f),
    new Vector2(0.0f, 2f),
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, 0.0f),
    new Vector2(0.0f, 3f),
    new Vector2(-1f, 3f),
    new Vector2(-1f, -3f)
  };
  private const float CarryJumpStrength = 0.885f;
  private const float CarryWalkSpeed = 4.0869565f;
  private const float CarryHeavyWalkSpeed = 2.35f;
  private readonly MovementHelper movementHelper = new MovementHelper(4.0869565f, 0.0f, float.MaxValue);
  private SoundEffect jumpSound;
  private SoundEffect landSound;
  private bool jumpIsFall;
  private bool wasNotGrounded;
  private Vector3 offsetFromGomez;

  public override void Initialize()
  {
    base.Initialize();
    this.movementHelper.Entity = (IPhysicsEntity) this.PlayerManager;
    TimeInterpolation.RegisterCallback(new Action<GameTime>(this.AdjustCarriedInstance), 30);
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.jumpSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Jump");
    this.landSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Land");
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
        if (this.PlayerManager.CarriedInstance == null)
          break;
        bool isLight = this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsLight();
        bool flag1 = this.PlayerManager.Action == ActionType.CarryHeavyJump || this.PlayerManager.Action == ActionType.CarryJump;
        bool flag2 = flag1 && !this.PlayerManager.Animation.Timing.Ended;
        if (this.PlayerManager.Grounded && this.InputManager.Jump == FezButtonState.Pressed && this.InputManager.Down.IsDown() && this.PlayerManager.Ground.First.GetRotatedFace(this.CameraManager.VisibleOrientation) == CollisionType.TopOnly)
        {
          this.PlayerManager.Position -= Vector3.UnitY * this.CollisionManager.DistanceEpsilon * 2f;
          IPlayerManager playerManager = this.PlayerManager;
          playerManager.Velocity = playerManager.Velocity - 0.0075000003f * Vector3.UnitY;
          this.PlayerManager.Action = isLight ? ActionType.CarryJump : ActionType.CarryHeavyJump;
          this.PlayerManager.CanDoubleJump = false;
          break;
        }
        if ((this.PlayerManager.Grounded || this.PlayerManager.CanDoubleJump) && (!flag1 || this.PlayerManager.Animation.Timing.Frame != 0) && this.InputManager.Jump == FezButtonState.Pressed)
        {
          this.jumpIsFall = false;
          this.Jump(isLight);
          break;
        }
        if (this.PlayerManager.Grounded && (double) this.InputManager.Movement.X != 0.0 && !flag2)
        {
          this.PlayerManager.Action = isLight ? ActionType.CarryWalk : ActionType.CarryHeavyWalk;
          break;
        }
        if (this.PlayerManager.Action != ActionType.CarryHeavyJump && this.PlayerManager.Action != ActionType.CarryJump && !this.PlayerManager.Grounded)
        {
          this.jumpIsFall = true;
          this.PlayerManager.Action = isLight ? ActionType.CarryJump : ActionType.CarryHeavyJump;
        }
        if (this.wasNotGrounded && this.PlayerManager.Grounded)
          this.landSound.EmitAt(this.PlayerManager.Position);
        this.wasNotGrounded = !this.PlayerManager.Grounded;
        if (this.PlayerManager.Action != ActionType.CarryIdle && this.PlayerManager.Action != ActionType.CarryHeavyIdle && this.PlayerManager.Grounded && FezMath.AlmostEqual(this.PlayerManager.Velocity.XZ(), Vector2.Zero) && !flag2)
        {
          this.PlayerManager.Action = isLight ? ActionType.CarryIdle : ActionType.CarryHeavyIdle;
          break;
        }
        if (this.PlayerManager.Action == ActionType.CarrySlide || !this.PlayerManager.Grounded || FezMath.AlmostEqual(this.PlayerManager.Velocity.XZ(), Vector2.Zero) || !FezMath.AlmostEqual(this.InputManager.Movement, Vector2.Zero) || flag2)
          break;
        this.PlayerManager.Action = isLight ? ActionType.CarrySlide : ActionType.CarryHeavySlide;
        this.PlayerManager.Animation.Timing.Paused = false;
        break;
    }
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (this.PlayerManager.CarriedInstance == null)
    {
      this.PlayerManager.Action = ActionType.Idle;
      return false;
    }
    bool flag1 = this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsLight();
    bool flag2 = this.PlayerManager.Action == ActionType.CarryHeavyJump || this.PlayerManager.Action == ActionType.CarryJump;
    if (this.PlayerManager.Action == ActionType.CarryWalk || this.PlayerManager.Action == ActionType.CarryHeavyWalk || flag2 && this.PlayerManager.Grounded)
    {
      this.movementHelper.WalkAcceleration = flag1 ? 4.0869565f : 2.35f;
      this.movementHelper.Update((float) elapsed.TotalSeconds);
    }
    float timeFactor = 1.2f;
    if (this.PlayerManager.Action == ActionType.CarryJump || this.PlayerManager.Action == ActionType.CarryHeavyJump)
      timeFactor = 1f;
    this.PlayerManager.Animation.Timing.Update(elapsed, timeFactor);
    if (this.PlayerManager.Action == ActionType.CarryJump || this.PlayerManager.Action == ActionType.CarryHeavyJump)
    {
      if (this.PlayerManager.Animation.Timing.Frame == 1 && this.PlayerManager.Grounded && !this.jumpIsFall)
      {
        this.jumpSound.EmitAt(this.PlayerManager.Position);
        IPlayerManager playerManager1 = this.PlayerManager;
        playerManager1.Velocity = playerManager1.Velocity * FezMath.XZMask;
        IPlayerManager playerManager2 = this.PlayerManager;
        playerManager2.Velocity = playerManager2.Velocity + 0.13275f * Vector3.UnitY * (flag1 ? 1f : 0.75f);
      }
      else
        this.JumpAftertouch((float) elapsed.TotalSeconds);
    }
    this.MoveCarriedInstance();
    return false;
  }

  private void Jump(bool isLight)
  {
    this.PlayerManager.Action = isLight ? ActionType.CarryJump : ActionType.CarryHeavyJump;
    this.PlayerManager.Animation.Timing.Restart();
    this.PlayerManager.CanDoubleJump = false;
  }

  private void JumpAftertouch(float secondsElapsed)
  {
    int frame = this.PlayerManager.Animation.Timing.Frame;
    int num = this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsHeavy() ? 7 : 6;
    if (!this.PlayerManager.Grounded && (double) this.PlayerManager.Velocity.Y < 0.0)
      this.PlayerManager.Animation.Timing.Step = Math.Max(this.PlayerManager.Animation.Timing.Step, 0.5f);
    if (frame != 0 && frame < num && this.PlayerManager.Grounded)
      this.PlayerManager.Animation.Timing.Step = (float) num / 8f;
    else if (!this.PlayerManager.Grounded && (double) this.PlayerManager.Velocity.Y < 0.0)
      this.PlayerManager.Animation.Timing.Step = Math.Min(this.PlayerManager.Animation.Timing.Step, (float) ((double) num / 8.0 - 1.0 / 1000.0));
    else if (frame < num)
      this.PlayerManager.Animation.Timing.Step = Math.Min(this.PlayerManager.Animation.Timing.Step, 0.499f);
    if (frame == 0 || frame >= num || this.InputManager.Jump != FezButtonState.Down)
      return;
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity + (float) ((double) secondsElapsed * 0.88499999046325684 / 4.0) * Vector3.UnitY;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.CarryWalk || type == ActionType.CarryIdle || type == ActionType.CarryJump || type == ActionType.CarrySlide || type == ActionType.CarryHeavyWalk || type == ActionType.CarryHeavyIdle || type == ActionType.CarryHeavyJump || type == ActionType.CarryHeavySlide;
  }

  private void MoveCarriedInstance()
  {
    int view = this.CameraManager.ActionRunning ? (int) this.CameraManager.Viewpoint : (int) this.CameraManager.LastViewpoint;
    Vector3 mask = ((Viewpoint) view).VisibleAxis().GetMask();
    Vector3 vector3_1 = ((Viewpoint) view).RightVector().Abs();
    float num1 = (float) this.PlayerManager.LookingDirection.Sign() * Vector3.Dot(this.CameraManager.Viewpoint.RightVector(), FezMath.XZMask);
    float num2 = -0.5f;
    float num3 = (float) (2.0 - ((double) this.PlayerManager.Size.Y / 2.0 + 1.0 / 1000.0));
    bool flag = this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsLight();
    int frame = this.PlayerManager.Animation.Timing.Frame;
    Vector3 vector3_2;
    switch (this.PlayerManager.Action)
    {
      case ActionType.CarryIdle:
        vector3_2 = Vector3.UnitY * (num3 - 5f / 16f);
        num2 += (float) ((double) num1 * 1.0 / 16.0);
        break;
      case ActionType.CarryJump:
        Vector2 vector2_1 = Carry.LightJumpOffset[frame];
        vector3_2 = new Vector3(0.0f, (float) ((double) num3 - 5.0 / 16.0 + (double) vector2_1.Y * 1.0 / 16.0), 0.0f);
        num2 += 1f / 16f * vector2_1.X * num1;
        break;
      case ActionType.CarryHeavyIdle:
        vector3_2 = Vector3.UnitY * (num3 - 0.625f);
        break;
      case ActionType.CarryHeavyJump:
        Vector2 vector2_2 = Carry.HeavyJumpOffset[frame];
        vector3_2 = new Vector3(0.0f, (float) ((double) num3 - 0.625 + (double) vector2_2.Y * 1.0 / 16.0), 0.0f);
        num2 += 1f / 16f * vector2_2.X * num1;
        break;
      default:
        Vector2 vector2_3 = flag ? Carry.LightWalkOffset[frame] : Carry.HeavyWalkOffset[frame];
        vector3_2 = Vector3.UnitY * (num3 + (float) (((double) vector2_3.Y - 7.0) / 16.0));
        if (flag)
        {
          num2 += 1f / 16f * num1;
          vector3_2 += Vector3.UnitY * 2f / 16f;
        }
        num2 += vector2_3.X / 16f * num1;
        break;
    }
    this.offsetFromGomez = vector3_1 * num2 + mask * -0.5f + vector3_2;
  }

  private void AdjustCarriedInstance(GameTime _)
  {
    if (this.GameState.Paused || this.GameState.Loading || this.GameState.InCutscene || this.GameState.InMap || this.GameState.InFpsMode || this.GameState.InMenuCube || this.PlayerManager.CarriedInstance == null || !this.IsActionAllowed(this.PlayerManager.Action))
      return;
    this.PlayerManager.CarriedInstance.Position = GomezHost.Instance.InterpolatedPosition + this.offsetFromGomez;
    this.LevelManager.UpdateInstance(this.PlayerManager.CarriedInstance);
  }
}
