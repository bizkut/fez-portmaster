// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Swim
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;

#nullable disable
namespace FezGame.Components.Actions;

internal class Swim(Game game) : PlayerAction(game)
{
  private const float PulseDelay = 0.5f;
  private const float Buoyancy = 0.006f;
  private const float MaxSubmergedPortion = 0.5f;
  private TimeSpan sincePulsed;
  private SoundEmitter treadInstance;
  private SoundEffect swimSound;
  private readonly MovementHelper movementHelper = new MovementHelper(4.7f, 4.7f, 0.0f);

  protected override void LoadContent()
  {
    base.LoadContent();
    this.swimSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Swim");
  }

  private float SubmergedPortion => !this.LevelManager.WaterType.IsWater() ? 0.25f : 0.5f;

  public override void Initialize()
  {
    base.Initialize();
    this.movementHelper.Entity = (IPhysicsEntity) this.PlayerManager;
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      if (this.LevelManager.WaterType == LiquidType.None)
        return;
      this.treadInstance = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/Tread").Emit(true, true);
    });
  }

  protected override void TestConditions()
  {
    TrileGroup trileGroup;
    if (this.PlayerManager.Action == ActionType.Swimming || this.PlayerManager.Action == ActionType.Dying || this.PlayerManager.Action == ActionType.Sinking || this.PlayerManager.Action == ActionType.Treading || this.PlayerManager.Action == ActionType.HurtSwim || this.PlayerManager.Action == ActionType.SuckedIn || this.PlayerManager.Grounded && this.LevelManager.PickupGroups.TryGetValue(this.PlayerManager.Ground.First, out trileGroup) && trileGroup.ActorType == ActorType.Geyser || this.LevelManager.WaterType == LiquidType.None || (double) this.PlayerManager.Position.Y >= (double) this.LevelManager.WaterHeight - (double) this.SubmergedPortion || this.PlayerManager.Action == ActionType.Jumping)
      return;
    this.PlayerManager.RecordRespawnInformation();
    int action = (int) this.PlayerManager.Action;
    this.PlayerManager.Action = ActionType.Treading;
    if (action != 32 /*0x20*/)
      this.PlaneParticleSystems.Splash((IPhysicsEntity) this.PlayerManager, false);
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * new Vector3(1f, 0.5f, 1f);
  }

  protected override void Begin() => this.PlayerManager.CarriedInstance = (TrileInstance) null;

  protected override void End()
  {
    this.sincePulsed = TimeSpan.Zero;
    if (this.PlayerManager.Action == ActionType.Suffering || this.PlayerManager.Action == ActionType.Sinking || this.LevelManager.WaterType == LiquidType.None || this.PlayerManager.Action == ActionType.Flying)
      return;
    if (this.PlayerManager.Action != ActionType.Jumping)
    {
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity * new Vector3(1f, 0.5f, 1f);
    }
    this.PlaneParticleSystems.Splash((IPhysicsEntity) this.PlayerManager, true);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if ((double) this.PlayerManager.Position.Y < (double) this.LevelManager.WaterHeight - (double) this.SubmergedPortion)
    {
      if (this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.WaterType == LiquidType.Sewer)
      {
        this.PlayerManager.Action = ActionType.Sinking;
        return false;
      }
      float num1 = this.LevelManager.WaterHeight - this.SubmergedPortion;
      float num2 = num1 - this.PlayerManager.Position.Y;
      IPlayerManager playerManager1 = this.PlayerManager;
      playerManager1.Velocity = playerManager1.Velocity + 0.472500026f * (float) elapsed.TotalSeconds * Vector3.UnitY;
      if ((double) num2 > 0.02500000037252903)
      {
        IPlayerManager playerManager2 = this.PlayerManager;
        playerManager2.Velocity = playerManager2.Velocity + num2 * (3f / 500f) * Vector3.UnitY;
      }
      else
        this.PlayerManager.Position = this.PlayerManager.Position * FezMath.XZMask + Vector3.UnitY * num1;
    }
    else if ((double) Math.Abs(this.PlayerManager.Velocity.Y) > 0.019999999552965164 || this.PlayerManager.Grounded)
    {
      this.PlayerManager.Action = ActionType.Falling;
      return true;
    }
    this.sincePulsed -= elapsed;
    if ((double) this.InputManager.Movement.X == 0.0)
    {
      if (this.PlayerManager.Action != ActionType.HurtSwim)
        this.PlayerManager.Action = ActionType.Treading;
    }
    else
    {
      if (this.PlayerManager.Action != ActionType.HurtSwim)
        this.PlayerManager.Action = ActionType.Swimming;
      if (this.sincePulsed.TotalSeconds <= 0.0)
      {
        this.PlaneParticleSystems.Splash((IPhysicsEntity) this.PlayerManager, true);
        this.sincePulsed = TimeSpan.FromSeconds(0.5);
        this.swimSound.EmitAt(this.PlayerManager.Position);
      }
      float num = Easing.EaseIn(this.sincePulsed.TotalSeconds, EasingType.Sine);
      this.movementHelper.Update((float) elapsed.TotalSeconds * num);
      TrileInstance destination = this.PlayerManager.WallCollision.First.Destination;
      if (destination != null && destination.Trile.ActorSettings.Type.IsPickable())
      {
        IPlayerManager playerManager = this.PlayerManager;
        playerManager.Velocity = playerManager.Velocity * new Vector3(0.9f, 1f, 0.9f);
        this.DebuggingBag.Add("##. player vel", (object) this.PlayerManager.Velocity);
        destination.PhysicsState.Velocity += this.PlayerManager.Velocity * FezMath.XZMask;
      }
      this.PlayerManager.GroundedVelocity = new Vector3?(this.PlayerManager.Velocity);
    }
    if (this.PlayerManager.Action == ActionType.Treading && this.treadInstance != null && !this.treadInstance.Dead)
    {
      if (this.treadInstance.Cue.State != SoundState.Playing)
        this.treadInstance.Cue.Resume();
      this.treadInstance.Position = this.PlayerManager.Position;
    }
    return true;
  }

  public override void Update(GameTime gameTime)
  {
    base.Update(gameTime);
    if (this.GameState.Loading || this.PlayerManager.Action == ActionType.Treading || this.treadInstance == null || this.treadInstance.Dead || this.treadInstance.Cue.State != SoundState.Playing)
      return;
    this.treadInstance.Cue.Pause();
  }

  protected override bool IsActionAllowed(ActionType type) => type.IsSwimming();

  [ServiceDependency]
  public IPlaneParticleSystems PlaneParticleSystems { get; set; }
}
