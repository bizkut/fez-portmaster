// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Fall
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

public class Fall(Game game) : PlayerAction(game)
{
  private static readonly TimeSpan DoubleJumpTime = TimeSpan.FromSeconds(0.1);
  private const float MaxVelocity = 5.09362459f;
  public const float AirControl = 0.15f;
  public const float Gravity = 3.15f;
  private SoundEffect sFall;
  private SoundEmitter eFall;

  protected override void LoadContent()
  {
    base.LoadContent();
    this.sFall = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/FallThroughAir");
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    if (FezMath.AlmostEqual(this.PlayerManager.Velocity.Y, 0.0f))
    {
      if (this.eFall != null && !this.eFall.Dead)
      {
        this.eFall.FadeOutAndDie(0.1f);
        this.eFall = (SoundEmitter) null;
      }
    }
    else
    {
      if (this.eFall == null || this.eFall.Dead)
        this.eFall = this.sFall.EmitAt(this.PlayerManager.Position, true, 0.0f, 0.0f);
      this.eFall.Position = this.PlayerManager.Position;
      this.eFall.VolumeFactor = Easing.EaseIn((double) FezMath.Saturate((float) (-(double) this.PlayerManager.Velocity.Y / 0.40000000596046448)), EasingType.Quadratic);
    }
    base.Update(gameTime);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    this.PlayerManager.AirTime += elapsed;
    bool flag1 = (double) this.CollisionManager.GravityFactor < 0.0;
    Vector3 vector3_1 = (float) (3.1500000953674316 * (double) this.CollisionManager.GravityFactor * 0.15000000596046448) * (float) elapsed.TotalSeconds * -Vector3.UnitY;
    if (this.PlayerManager.Action == ActionType.Suffering)
      vector3_1 /= 2f;
    IPlayerManager playerManager1 = this.PlayerManager;
    playerManager1.Velocity = playerManager1.Velocity + vector3_1;
    bool flag2 = this.PlayerManager.CarriedInstance != null;
    if (!this.PlayerManager.Grounded && this.PlayerManager.Action != ActionType.Suffering)
    {
      float x = this.InputManager.Movement.X;
      IPlayerManager playerManager2 = this.PlayerManager;
      playerManager2.Velocity = playerManager2.Velocity + Vector3.Transform(Vector3.UnitX * x, this.CameraManager.Rotation) * 0.15f * 4.7f * (float) elapsed.TotalSeconds * 0.15f;
      if ((flag1 ? ((double) this.PlayerManager.Velocity.Y > 0.0 ? 1 : 0) : ((double) this.PlayerManager.Velocity.Y < 0.0 ? 1 : 0)) != 0)
        this.PlayerManager.CanDoubleJump &= this.PlayerManager.AirTime < Fall.DoubleJumpTime;
    }
    else
    {
      this.PlayerManager.CanDoubleJump = true;
      this.PlayerManager.AirTime = TimeSpan.Zero;
    }
    if (!this.PlayerManager.Grounded && (flag1 ? ((double) this.PlayerManager.Velocity.Y > 0.0 ? 1 : 0) : ((double) this.PlayerManager.Velocity.Y < 0.0 ? 1 : 0)) != 0 && !flag2 && !this.PlayerManager.Action.PreventsFall() && this.PlayerManager.Action != ActionType.Falling)
      this.PlayerManager.Action = ActionType.Falling;
    if (this.PlayerManager.GroundedVelocity.HasValue)
    {
      float num1 = 5.09362459f * (float) elapsed.TotalSeconds;
      float val2_1 = (float) ((double) num1 / 1.5 * (0.5 + (double) Math.Abs(this.CollisionManager.GravityFactor) * 1.5) / 2.0);
      if (this.PlayerManager.CarriedInstance != null && this.PlayerManager.CarriedInstance.Trile.ActorSettings.Type.IsHeavy())
      {
        num1 *= 0.7f;
        val2_1 *= 0.7f;
      }
      Vector3 vector3_2;
      ref Vector3 local = ref vector3_2;
      double val1_1 = (double) num1;
      Vector3? groundedVelocity = this.PlayerManager.GroundedVelocity;
      double val1_2 = (double) Math.Max(Math.Abs(groundedVelocity.Value.X), val2_1);
      groundedVelocity = this.PlayerManager.GroundedVelocity;
      double val2_2 = (double) Math.Max(Math.Abs(groundedVelocity.Value.Z), val2_1);
      double val2_3 = (double) Math.Max((float) val1_2, (float) val2_2);
      double num2 = (double) Math.Min((float) val1_1, (float) val2_3);
      local = new Vector3((float) num2);
      this.PlayerManager.Velocity = new Vector3(MathHelper.Clamp(this.PlayerManager.Velocity.X, -vector3_2.X, vector3_2.X), this.PlayerManager.Velocity.Y, MathHelper.Clamp(this.PlayerManager.Velocity.Z, -vector3_2.Z, vector3_2.Z));
    }
    return this.PlayerManager.Action == ActionType.Falling;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return !type.DefiesGravity() && !this.PlayerManager.Hidden;
  }
}
