// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.FreeFall
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components.Actions;

public class FreeFall(Game game) : PlayerAction(game)
{
  private const float FreeFallEnd = 36f;
  private const float CamPanUp = 5f;
  private const float CamFollowEnd = 27f;
  private SoundEffect thudSound;
  private SoundEffect panicSound;
  private SoundEmitter panicEmitter;
  private bool WasConstrained;
  private Vector3 OldConstrainedCenter;
  private int? CapEnd;
  private static readonly Dictionary<string, int> EndCaps = new Dictionary<string, int>()
  {
    {
      "INDUSTRIAL_SUPERSPIN",
      0
    },
    {
      "PIVOT_ONE",
      0
    },
    {
      "PIVOT_TWO",
      7
    },
    {
      "INDUSTRIAL_HUB",
      5
    },
    {
      "GRAVE_TREASURE_A",
      0
    },
    {
      "WELL_2",
      0
    },
    {
      "TREE_SKY",
      0
    },
    {
      "PIVOT_THREE_CAVE",
      40
    },
    {
      "ZU_BRIDGE",
      22
    },
    {
      "LIGHTHOUSE_SPIN",
      4
    },
    {
      "FRACTAL",
      0
    },
    {
      "MINE_A",
      0
    },
    {
      "MINE_WRAP",
      4
    },
    {
      "BIG_TOWER",
      0
    },
    {
      "ZU_CITY_RUINS",
      5
    },
    {
      "CODE_MACHINE",
      3
    },
    {
      "TELESCOPE",
      5
    },
    {
      "GLOBE",
      5
    },
    {
      "MEMORY_CORE",
      10
    },
    {
      "ZU_CITY",
      6
    }
  };

  public float FreeFallStart
  {
    get
    {
      return !(this.LevelManager.Name == "CLOCK") || (double) this.PlayerManager.LeaveGroundPosition.Y < 68.0 ? 8f : 10f;
    }
  }

  public override void Initialize()
  {
    base.Initialize();
    this.thudSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/CrashLand");
    this.panicSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/AirPanic");
  }

  protected override void TestConditions()
  {
    float num = this.PlayerManager.Position.Y - this.CameraManager.ViewOffset.Y;
    if (this.PlayerManager.IgnoreFreefall || this.PlayerManager.Grounded || this.PlayerManager.Action.PreventsFall() || this.PlayerManager.Action == ActionType.SuckedIn || (double) Math.Sign(this.CollisionManager.GravityFactor) * ((double) this.PlayerManager.LeaveGroundPosition.Y - (double) this.PlayerManager.OffsetAtLeaveGround - (double) num) <= (double) this.FreeFallStart)
      return;
    this.PlayerManager.Action = ActionType.FreeFalling;
  }

  protected override void Begin()
  {
    base.Begin();
    if (this.panicEmitter != null)
    {
      this.panicEmitter.FadeOutAndDie(0.0f);
      this.panicEmitter = (SoundEmitter) null;
    }
    (this.panicEmitter = this.panicSound.EmitAt(this.PlayerManager.Position)).NoAttenuation = true;
    if (this.PlayerManager.CarriedInstance != null)
    {
      this.PlayerManager.CarriedInstance.PhysicsState.Velocity = this.PlayerManager.Velocity * 0.95f;
      this.PlayerManager.CarriedInstance = (TrileInstance) null;
    }
    this.WasConstrained = this.CameraManager.Constrained;
    if (this.WasConstrained)
      this.OldConstrainedCenter = this.CameraManager.Center;
    this.CameraManager.Constrained = true;
    int num;
    if (FreeFall.EndCaps.TryGetValue(this.LevelManager.Name, out num))
      this.CapEnd = new int?(num);
    else
      this.CapEnd = new int?();
  }

  protected override void End()
  {
    base.End();
    if (!this.WasConstrained)
      this.CameraManager.Constrained = false;
    else
      this.CameraManager.Center = this.OldConstrainedCenter;
  }

  protected override bool Act(TimeSpan elapsed)
  {
    float num1 = this.PlayerManager.Position.Y - this.CameraManager.ViewOffset.Y;
    float num2 = (float) Math.Sign(this.CollisionManager.GravityFactor) * (this.PlayerManager.LeaveGroundPosition.Y - this.PlayerManager.OffsetAtLeaveGround - num1);
    float num3 = this.CameraManager.Radius / this.CameraManager.AspectRatio;
    float num4 = 36f;
    if (this.CapEnd.HasValue)
      num4 = Math.Min(36f, this.PlayerManager.RespawnPosition.Y - (float) this.CapEnd.Value);
    if (!this.GameState.SkipFadeOut && (double) num2 < 27.0 && (!this.CapEnd.HasValue || (double) this.CameraManager.Center.Y - (double) num3 / 2.0 > (double) (this.CapEnd.Value + 1)))
      this.CameraManager.Center = this.CameraManager.Center * (this.CameraManager.Viewpoint.SideMask() + this.CameraManager.Viewpoint.DepthMask()) + (this.PlayerManager.Position.Y - (float) (((double) num2 - (double) this.FreeFallStart) / 27.0 * 5.0)) * Vector3.UnitY;
    if (this.PlayerManager.Grounded)
    {
      this.panicEmitter.FadeOutAndDie(0.0f);
      this.panicEmitter = (SoundEmitter) null;
      this.thudSound.EmitAt(this.PlayerManager.Position).NoAttenuation = true;
      this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.RightHigh, 1.0, TimeSpan.FromSeconds(0.5), EasingType.Quadratic);
      this.InputManager.ActiveGamepad.Vibrate(VibrationMotor.LeftLow, 1.0, TimeSpan.FromSeconds(0.34999999403953552));
      this.PlayerManager.Action = ActionType.Dying;
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
    }
    if (!this.GameState.SkipFadeOut && (double) num2 > (double) num4)
    {
      if (!this.WasConstrained)
        this.CameraManager.Constrained = false;
      else
        this.CameraManager.Center = this.OldConstrainedCenter;
      this.PlayerManager.Respawn();
    }
    return true;
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.FreeFalling;
}
