// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PickupsHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable disable
namespace FezGame.Components;

public class PickupsHost : DrawableGameComponent
{
  private const float SubmergedPortion = 0.8125f;
  private SoundEffect vaseBreakSound;
  private SoundEffect thudSound;
  private AnimatedTexture largeDust;
  private AnimatedTexture smallDust;
  private List<PickupState> PickupStates;
  private float sinceLevelChanged;
  private readonly ManualResetEvent initLock = new ManualResetEvent(false);

  public PickupsHost(Game game)
    : base(game)
  {
    this.UpdateOrder = -1;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.InitializePickups);
    this.InitializePickups();
    this.CameraManager.ViewpointChanged += (Action) (() =>
    {
      if (this.GameState.Loading || !this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.LastViewpoint == this.CameraManager.Viewpoint)
        return;
      this.PauseGroupOverlaps(false);
      this.LevelManager.ScreenInvalidated += new Action(this.DetectBackground);
    });
    this.CollisionManager.GravityChanged += (Action) (() =>
    {
      foreach (PickupState pickupState in this.PickupStates)
        pickupState.Instance.PhysicsState.Ground = new MultipleHits<TrileInstance>();
    });
  }

  private void DetectBackground()
  {
    if (this.LevelManager.Name != "LAVA")
    {
      foreach (TrileGroup trileGroup in (IEnumerable<TrileGroup>) this.LevelManager.PickupGroups.Values)
      {
        foreach (TrileInstance trile in trileGroup.Triles)
          trile.PhysicsState.UpdatingPhysics = true;
        foreach (TrileInstance trile in trileGroup.Triles)
        {
          if (!trile.PhysicsState.IgnoreCollision)
            this.PhysicsManager.DetermineInBackground((IPhysicsEntity) trile.PhysicsState, true, true, false);
        }
        foreach (TrileInstance trile in trileGroup.Triles)
          trile.PhysicsState.UpdatingPhysics = false;
      }
    }
    if (this.PickupStates == null)
      return;
    foreach (PickupState pickupState in this.PickupStates)
    {
      if (pickupState.Group == null && pickupState.Instance.PhysicsState != null)
        this.PhysicsManager.DetermineInBackground((IPhysicsEntity) pickupState.Instance.PhysicsState, true, true, false);
    }
  }

  private void PauseGroupOverlaps(bool force)
  {
    if (!force && this.GameState.Loading || !this.CameraManager.Viewpoint.IsOrthographic() || this.LevelManager.PickupGroups.Count == 0)
      return;
    Vector3 b1 = this.CameraManager.Viewpoint.ForwardVector();
    Vector3 b2 = this.CameraManager.Viewpoint.SideMask();
    Vector3 vector3 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    foreach (TrileGroup trileGroup in this.LevelManager.PickupGroups.Values.Distinct<TrileGroup>())
    {
      float num = float.MaxValue;
      float? nullable = new float?();
      foreach (TrileInstance trile in trileGroup.Triles)
      {
        num = Math.Min(num, trile.Center.Dot(b1));
        if (!trile.PhysicsState.Puppet)
          nullable = new float?(trile.Center.Dot(b2));
      }
      foreach (PickupState pickupState1 in this.PickupStates)
      {
        if (pickupState1.Group == trileGroup)
        {
          TrileInstance instance = pickupState1.Instance;
          bool flag = !FezMath.AlmostEqual(instance.Center.Dot(b1), num);
          instance.PhysicsState.Paused = flag;
          if (flag)
          {
            instance.PhysicsState.Puppet = true;
            pickupState1.LastMovement = Vector3.Zero;
          }
          else
          {
            pickupState1.VisibleOverlapper = (PickupState) null;
            foreach (PickupState pickupState2 in this.PickupStates)
            {
              if (FezMath.AlmostEqual(pickupState2.Instance.Center * vector3, pickupState1.Instance.Center * vector3))
                pickupState2.VisibleOverlapper = pickupState1;
            }
            if (nullable.HasValue && FezMath.AlmostEqual(instance.Center.Dot(b2), nullable.Value))
              instance.PhysicsState.Puppet = false;
          }
        }
      }
    }
  }

  protected override void LoadContent()
  {
    this.largeDust = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/dust_large");
    this.smallDust = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/dust_small");
    this.vaseBreakSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/VaseBreak");
    this.thudSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/HitFloor");
  }

  private void InitializePickups()
  {
    this.sinceLevelChanged = 0.0f;
    if (this.LevelManager.TrileSet == null)
    {
      this.initLock.Reset();
      this.PickupStates = (List<PickupState>) null;
      this.initLock.Set();
    }
    else
    {
      this.initLock.Reset();
      if (this.PickupStates != null)
      {
        foreach (PickupState pickupState in this.PickupStates)
          pickupState.Instance.PhysicsState.ShouldRespawn = false;
      }
      this.PickupStates = new List<PickupState>(this.LevelManager.TrileSet.Triles.Values.Where<Trile>((Func<Trile, bool>) (t => t.ActorSettings.Type.IsPickable())).SelectMany<Trile, TrileInstance>((Func<Trile, IEnumerable<TrileInstance>>) (t => (IEnumerable<TrileInstance>) t.Instances)).Select<TrileInstance, PickupState>((Func<TrileInstance, PickupState>) (t => new PickupState(t, this.LevelManager.PickupGroups.ContainsKey(t) ? this.LevelManager.PickupGroups[t] : (TrileGroup) null))));
      foreach (PickupState pickupState in this.PickupStates.Where<PickupState>((Func<PickupState, bool>) (x => x.Group != null)))
      {
        int groupId = pickupState.Group.Id;
        pickupState.AttachedAOs = this.LevelMaterializer.LevelArtObjects.Where<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x =>
        {
          int? attachedGroup = x.ActorSettings.AttachedGroup;
          int num = groupId;
          return attachedGroup.GetValueOrDefault() == num && attachedGroup.HasValue;
        })).ToArray<ArtObjectInstance>();
        if (pickupState.Group.Triles.Count == 1)
          pickupState.Group = (TrileGroup) null;
      }
      foreach (TrileInstance instance in this.PickupStates.Where<PickupState>((Func<PickupState, bool>) (x => x.Instance.PhysicsState == null)).Select<PickupState, TrileInstance>((Func<PickupState, TrileInstance>) (x => x.Instance)))
        instance.PhysicsState = new InstancePhysicsState(instance)
        {
          Ground = new MultipleHits<TrileInstance>()
          {
            NearLow = this.LevelManager.ActualInstanceAt(instance.Center - instance.Trile.Size.Y * Vector3.UnitY)
          }
        };
      bool flag = this.LevelManager.WaterType == LiquidType.Sewer && !FezMath.In<string>(this.LevelManager.Name, "SEWER_PIVOT", "SEWER_TREASURE_TWO");
      foreach (TrileGroup trileGroup in this.LevelManager.PickupGroups.Values.Where<TrileGroup>((Func<TrileGroup, bool>) (g => g.Triles.All<TrileInstance>((Func<TrileInstance, bool>) (t => !t.PhysicsState.Puppet)))).Distinct<TrileGroup>())
      {
        foreach (TrileInstance trile in trileGroup.Triles)
        {
          trile.PhysicsState.IgnoreCollision = flag;
          trile.PhysicsState.Center += 1f / 500f * FezMath.XZMask;
          trile.PhysicsState.UpdateInstance();
          this.LevelManager.UpdateInstance(trile);
          trile.PhysicsState.Puppet = true;
        }
        trileGroup.Triles[trileGroup.Triles.Count / 2].PhysicsState.Puppet = false;
        trileGroup.InMidAir = true;
      }
      this.PauseGroupOverlaps(true);
      this.DetectBackground();
      this.initLock.Set();
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective || !this.CameraManager.ActionRunning || this.GameState.Paused || this.GameState.InMap || this.GameState.Loading || this.PickupStates == null || this.PickupStates.Count == 0)
      return;
    this.sinceLevelChanged += (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.initLock.WaitOne();
    for (int index = this.PickupStates.Count - 1; index >= 0; --index)
    {
      if (this.PickupStates[index].Instance.PhysicsState == null)
        this.PickupStates.RemoveAt(index);
    }
    foreach (PickupState pickupState in this.PickupStates)
    {
      if (pickupState.Instance.PhysicsState.StaticGrounds)
        pickupState.Instance.PhysicsState.GroundMovement = Vector3.Zero;
    }
    this.PickupStates.Sort((IComparer<PickupState>) MovingGroundsPickupComparer.Default);
    this.UpdatePickups((float) gameTime.ElapsedGameTime.TotalSeconds);
    foreach (TrileGroup trileGroup in (IEnumerable<TrileGroup>) this.LevelManager.PickupGroups.Values)
    {
      if (trileGroup.InMidAir)
      {
        foreach (TrileInstance trile in trileGroup.Triles)
        {
          if (!trile.PhysicsState.Paused && trile.PhysicsState.Grounded)
          {
            trileGroup.InMidAir = false;
            if (trile.PhysicsState.Puppet)
            {
              trile.PhysicsState.Puppet = false;
              using (List<TrileInstance>.Enumerator enumerator = trileGroup.Triles.GetEnumerator())
              {
                while (enumerator.MoveNext())
                {
                  TrileInstance current = enumerator.Current;
                  if (current != trile)
                    current.PhysicsState.Puppet = true;
                }
                break;
              }
            }
            break;
          }
        }
      }
      else
      {
        trileGroup.InMidAir = true;
        foreach (TrileInstance trile in trileGroup.Triles)
          trileGroup.InMidAir &= !trile.PhysicsState.Grounded;
      }
    }
    foreach (PickupState pickupState1 in this.PickupStates)
    {
      if (pickupState1.Group != null && !pickupState1.Instance.PhysicsState.Puppet)
      {
        PickupState pickupState2 = pickupState1;
        foreach (PickupState pickupState3 in this.PickupStates)
        {
          if (pickupState3.Group == pickupState2.Group && pickupState3 != pickupState2)
          {
            pickupState3.Instance.PhysicsState.Center += pickupState2.LastMovement - pickupState3.LastMovement;
            pickupState3.Instance.PhysicsState.Background = pickupState2.Instance.PhysicsState.Background;
            pickupState3.Instance.PhysicsState.Velocity = pickupState2.Instance.PhysicsState.Velocity;
            pickupState3.Instance.PhysicsState.UpdateInstance();
            this.LevelManager.UpdateInstance(pickupState3.Instance);
            pickupState3.LastMovement = Vector3.Zero;
            pickupState3.FloatMalus = pickupState2.FloatMalus;
            pickupState3.FloatSeed = pickupState2.FloatSeed;
          }
        }
      }
      if (pickupState1.VisibleOverlapper != null)
      {
        PickupState visibleOverlapper = pickupState1.VisibleOverlapper;
        InstancePhysicsState physicsState = pickupState1.Instance.PhysicsState;
        physicsState.Background = visibleOverlapper.Instance.PhysicsState.Background;
        physicsState.Ground = visibleOverlapper.Instance.PhysicsState.Ground;
        physicsState.Floating = visibleOverlapper.Instance.PhysicsState.Floating;
        Array.Copy((Array) physicsState.CornerCollision, (Array) pickupState1.Instance.PhysicsState.CornerCollision, 4);
        physicsState.GroundMovement = visibleOverlapper.Instance.PhysicsState.GroundMovement;
        physicsState.Sticky = visibleOverlapper.Instance.PhysicsState.Sticky;
        physicsState.WallCollision = visibleOverlapper.Instance.PhysicsState.WallCollision;
        physicsState.PushedDownBy = visibleOverlapper.Instance.PhysicsState.PushedDownBy;
        pickupState1.LastGroundedCenter = visibleOverlapper.LastGroundedCenter;
        pickupState1.LastVelocity = visibleOverlapper.LastVelocity;
        pickupState1.TouchesWater = visibleOverlapper.TouchesWater;
      }
    }
    foreach (PickupState pickupState in this.PickupStates)
    {
      if (pickupState.Instance.PhysicsState != null && (pickupState.Instance.PhysicsState.Grounded || this.PlayerManager.CarriedInstance == pickupState.Instance || pickupState.Instance.PhysicsState.Floating))
      {
        pickupState.FlightApex = pickupState.Instance.Center.Y;
        pickupState.LastGroundedCenter = pickupState.Instance.Center;
      }
    }
    this.initLock.Set();
  }

  private void UpdatePickups(float elapsedSeconds)
  {
    Vector3 vector3 = Vector3.UnitY * this.CameraManager.Radius / this.CameraManager.AspectRatio;
    foreach (PickupState pickupState in this.PickupStates)
    {
      TrileInstance instance = pickupState.Instance;
      InstancePhysicsState physicsState = instance.PhysicsState;
      ActorType type = instance.Trile.ActorSettings.Type;
      if (!physicsState.Paused && (physicsState.ShouldRespawn || instance.Enabled && instance != this.PlayerManager.CarriedInstance && (!physicsState.Static || pickupState.TouchesWater)))
      {
        this.TryFloat(pickupState, elapsedSeconds);
        if (!physicsState.Vanished && (!pickupState.TouchesWater || !type.IsBuoyant()))
          physicsState.Velocity += (float) (3.1500000953674316 * (double) this.CollisionManager.GravityFactor * 0.15000000596046448) * elapsedSeconds * Vector3.Down;
        bool grounded = instance.PhysicsState.Grounded;
        Vector3 center = physicsState.Center;
        this.PhysicsManager.Update((ISimplePhysicsEntity) physicsState, false, pickupState.Group == null && (!physicsState.Floating || !FezMath.AlmostEqual(physicsState.Velocity.X, 0.0f) || !FezMath.AlmostEqual(physicsState.Velocity.Z, 0.0f)));
        pickupState.LastMovement = physicsState.Center - center;
        if (physicsState.NoVelocityClamping)
        {
          physicsState.NoVelocityClamping = false;
          physicsState.Velocity = Vector3.Zero;
        }
        if (pickupState.AttachedAOs != null)
        {
          foreach (ArtObjectInstance attachedAo in pickupState.AttachedAOs)
            attachedAo.Position += pickupState.LastMovement;
        }
        if (((double) pickupState.LastGroundedCenter.Y - (double) instance.Position.Y) * (double) Math.Sign(this.CollisionManager.GravityFactor) > (double) vector3.Y)
          physicsState.Vanished = true;
        else if (this.LevelManager.Loops)
        {
          while ((double) instance.Position.Y < 0.0)
            instance.Position += this.LevelManager.Size * Vector3.UnitY;
          while ((double) instance.Position.Y > (double) this.LevelManager.Size.Y)
            instance.Position -= this.LevelManager.Size * Vector3.UnitY;
        }
        if (physicsState.Floating && physicsState.Grounded && !physicsState.PushedUp)
          physicsState.Floating = pickupState.TouchesWater = (double) instance.Position.Y <= (double) this.LevelManager.WaterHeight - 13.0 / 16.0 + (double) pickupState.FloatMalus;
        physicsState.ForceNonStatic = false;
        if (type.IsFragile())
        {
          if (!instance.PhysicsState.Grounded)
            pickupState.FlightApex = Math.Max(pickupState.FlightApex, instance.Center.Y);
          else if (!instance.PhysicsState.Respawned && (double) pickupState.FlightApex - (double) instance.Center.Y > (double) PickupsHost.BreakHeight(type))
          {
            this.PlayBreakSound(type, instance.Position);
            instance.PhysicsState.Vanished = true;
            this.ParticleSystemManager.Add(new TrixelParticleSystem(this.Game, new TrixelParticleSystem.Settings()
            {
              ExplodingInstance = instance,
              EnergySource = new Vector3?(instance.Center - Vector3.Normalize(pickupState.LastVelocity) * instance.TransformedSize / 2f),
              ParticleCount = 30,
              MinimumSize = 1,
              MaximumSize = 8,
              GravityModifier = 1f,
              Energy = 0.25f,
              BaseVelocity = pickupState.LastVelocity * 0.5f
            }));
            this.LevelMaterializer.CullInstanceOut(instance);
            if (type == ActorType.Vase)
            {
              instance.PhysicsState = (InstancePhysicsState) null;
              this.LevelManager.ClearTrile(instance);
              break;
            }
            instance.Enabled = false;
            instance.PhysicsState.ShouldRespawn = true;
          }
        }
        this.TryPushHorizontalStack(pickupState, elapsedSeconds);
        if (physicsState.Static)
        {
          pickupState.LastMovement = pickupState.LastVelocity = physicsState.Velocity = Vector3.Zero;
          physicsState.Respawned = false;
        }
        if (physicsState.Vanished)
          physicsState.ShouldRespawn = true;
        if (physicsState.ShouldRespawn && this.PlayerManager.Action != ActionType.FreeFalling)
        {
          physicsState.Center = pickupState.OriginalCenter + new Vector3(1f / 1000f);
          physicsState.UpdateInstance();
          physicsState.Velocity = Vector3.Zero;
          physicsState.ShouldRespawn = false;
          pickupState.LastVelocity = Vector3.Zero;
          pickupState.TouchesWater = false;
          physicsState.Floating = false;
          physicsState.PushedDownBy = (TrileInstance) null;
          instance.Enabled = false;
          instance.Hidden = true;
          physicsState.Ground = new MultipleHits<TrileInstance>()
          {
            NearLow = this.LevelManager.ActualInstanceAt(physicsState.Center - instance.Trile.Size.Y * Vector3.UnitY)
          };
          ServiceHelper.AddComponent((IGameComponent) new GlitchyRespawner(ServiceHelper.Game, instance));
        }
        physicsState.UpdateInstance();
        this.LevelManager.UpdateInstance(instance);
        if (!grounded && instance.PhysicsState.Grounded && (double) Math.Abs(pickupState.LastVelocity.Y) > 0.05000000074505806)
        {
          float num1 = pickupState.LastVelocity.Dot(this.CameraManager.Viewpoint.RightVector());
          float val1 = FezMath.Saturate(pickupState.LastVelocity.Y / (-0.2f * (float) Math.Sign(this.CollisionManager.GravityFactor)));
          AnimatedTexture animation;
          if (instance.Trile.ActorSettings.Type.IsHeavy())
          {
            if ((double) val1 > 0.5)
            {
              animation = this.largeDust;
            }
            else
            {
              animation = this.smallDust;
              val1 *= 2f;
            }
          }
          else
            animation = this.smallDust;
          float num2 = Math.Max(val1, 0.4f);
          this.SpawnDust(instance, num2, animation, (double) num1 >= 0.0, (double) num1 <= 0.0);
          if (animation == this.largeDust && (double) num1 != 0.0)
            this.SpawnDust(instance, num2, this.smallDust, (double) num1 < 0.0, (double) num1 > 0.0);
          this.thudSound.EmitAt(instance.Position, (float) ((double) num2 * -0.60000002384185791 + 0.30000001192092896), num2);
        }
        if (physicsState.Grounded && physicsState.Ground.First.PhysicsState != null)
          physicsState.Ground.First.PhysicsState.PushedDownBy = instance;
        pickupState.LastVelocity = instance.PhysicsState.Velocity;
      }
    }
  }

  private void TryFloat(PickupState pickup, float elapsedSeconds)
  {
    TrileInstance instance = pickup.Instance;
    InstancePhysicsState physicsState = instance.PhysicsState;
    ActorType type = instance.Trile.ActorSettings.Type;
    if (physicsState.Grounded && physicsState.Ground.First.PhysicsState != null)
      physicsState.Ground.First.PhysicsState.PushedDownBy = (TrileInstance) null;
    if (this.LevelManager.WaterType == LiquidType.None || physicsState.Grounded)
      return;
    this.DetermineFloatMalus(pickup);
    if (!physicsState.Floating)
    {
      if ((double) instance.Position.Y <= (double) this.LevelManager.WaterHeight - 13.0 / 16.0 + (double) pickup.FloatMalus - 1.0 / 16.0)
      {
        if (!pickup.TouchesWater)
        {
          if ((double) Math.Abs(physicsState.Velocity.Y) > 0.02500000037252903 && (double) this.sinceLevelChanged > 1.0)
            this.PlaneParticleSystems.Splash((IPhysicsEntity) physicsState, false, 0.25f);
          physicsState.Velocity *= new Vector3(1f, 0.35f, 1f);
        }
        pickup.TouchesWater = true;
      }
      else
      {
        if (pickup.TouchesWater)
        {
          if ((double) Math.Abs(physicsState.Velocity.Y) < 0.005)
          {
            physicsState.Floating = true;
            float num = (float) ((double) this.LevelManager.WaterHeight - 13.0 / 16.0 + (double) pickup.FloatMalus - 1.0 / 16.0);
            float d = (float) (((double) instance.Position.Y - (double) num) / (3.0 / 32.0));
            if ((double) d > -1.0 && (double) d < 1.0)
              pickup.FloatSeed = (float) Math.Asin((double) d);
          }
          else
            physicsState.Velocity *= new Vector3(1f, 0.35f, 1f);
        }
        pickup.TouchesWater = false;
      }
    }
    if (pickup.TouchesWater && !physicsState.Floating)
    {
      physicsState.Velocity *= new Vector3(0.9f);
      if (type.IsBuoyant())
      {
        float num = this.LevelManager.WaterHeight - 13f / 16f + pickup.FloatMalus - instance.Position.Y;
        if ((double) this.sinceLevelChanged <= 1.0 && this.LevelManager.WaterType == LiquidType.Sewer && !physicsState.IgnoreClampToWater && (double) Math.Abs(num) > 0.5)
        {
          physicsState.NoVelocityClamping = true;
          physicsState.Velocity += num * Vector3.UnitY;
        }
        else
          physicsState.Velocity += num * (7f / 800f) * Vector3.UnitY;
      }
    }
    physicsState.Sticky = physicsState.Floating;
    if (physicsState.Floating && !physicsState.PushedUp)
    {
      pickup.FloatSeed = FezMath.WrapAngle(pickup.FloatSeed + elapsedSeconds * 2f);
      this.DetermineFloatMalus(pickup);
      Vector3 vector3 = instance.Position * FezMath.XZMask + Vector3.UnitY * ((float) ((double) this.LevelManager.WaterHeight - 13.0 / 16.0 + Math.Sin((double) pickup.FloatSeed) * 1.5 / 16.0) + pickup.FloatMalus);
      physicsState.Velocity = vector3 - instance.Position + physicsState.Velocity * FezMath.XZMask * 0.6f;
    }
    if ((double) this.sinceLevelChanged > 1.0 || physicsState.Floating || pickup.TouchesWater || this.LevelManager.WaterType != LiquidType.Sewer || physicsState.IgnoreClampToWater)
      return;
    float num1 = this.LevelManager.WaterHeight - 13f / 16f + pickup.FloatMalus - instance.Position.Y;
    if ((double) Math.Abs(num1) <= 0.5)
      return;
    physicsState.NoVelocityClamping = true;
    physicsState.Velocity += num1 * Vector3.UnitY;
  }

  private void TryPushHorizontalStack(PickupState state, float elapsedSeconds)
  {
    TrileInstance instance1 = state.Instance;
    TrileGroup group = state.Group;
    if (!instance1.PhysicsState.WallCollision.AnyCollided())
      return;
    Vector3 vector3_1 = -instance1.PhysicsState.WallCollision.First.Response.Sign();
    instance1.PhysicsState.Velocity = Vector3.Zero;
    TrileInstance instance2 = instance1;
    while (instance2 != null && instance2.PhysicsState.WallCollision.AnyCollided())
    {
      MultipleHits<CollisionResult> wallCollision = instance2.PhysicsState.WallCollision;
      TrileInstance destination = wallCollision.First.Destination;
      if (destination.PhysicsState != null && destination.Trile.ActorSettings.Type.IsPickable() && (group == null || !group.Triles.Contains(destination)))
      {
        Vector3 vector = -wallCollision.First.Response;
        if (vector.Sign() != vector3_1 || vector == Vector3.Zero)
        {
          instance2 = (TrileInstance) null;
        }
        else
        {
          instance2 = destination;
          Vector3 velocity = instance2.PhysicsState.Velocity;
          instance2.PhysicsState.Velocity = vector;
          Vector3 center = instance2.PhysicsState.Center;
          if (instance2.PhysicsState.Grounded)
            instance2.PhysicsState.Velocity += (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448) * elapsedSeconds * Vector3.Down;
          this.PhysicsManager.Update((ISimplePhysicsEntity) instance2.PhysicsState, false, false);
          if (instance1.PhysicsState.Grounded)
            instance2.PhysicsState.Velocity = velocity;
          instance2.PhysicsState.UpdateInstance();
          this.LevelManager.UpdateInstance(instance2);
          foreach (PickupState pickupState in this.PickupStates)
          {
            if (pickupState.Instance.PhysicsState.Ground.NearLow == instance2 || pickupState.Instance.PhysicsState.Ground.FarHigh == instance2)
            {
              Vector3 vector3_2 = (instance2.PhysicsState.Center - center) / 0.85f;
              pickupState.Instance.PhysicsState.Velocity = vector3_2;
            }
          }
        }
      }
      else
        instance2 = (TrileInstance) null;
    }
  }

  private void SpawnDust(
    TrileInstance instance,
    float opacity,
    AnimatedTexture animation,
    bool onRight,
    bool onLeft)
  {
    float num1 = (float) ((double) instance.Center.Y - (double) instance.TransformedSize.Y / 2.0 * (double) Math.Sign(this.CollisionManager.GravityFactor) + (double) animation.FrameHeight / 32.0 * (double) Math.Sign(this.CollisionManager.GravityFactor));
    float num2 = (float) ((double) instance.TransformedSize.Dot(this.CameraManager.Viewpoint.SideMask()) / 2.0 + (double) animation.FrameWidth / 32.0 * 2.0 / 3.0);
    if (instance.Trile.ActorSettings.Type.IsBomb())
      num2 -= 0.25f;
    opacity = 1f;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.ForwardVector();
    bool b = (double) this.CollisionManager.GravityFactor < 0.0;
    if (onRight)
    {
      IGameLevelManager levelManager = this.LevelManager;
      BackgroundPlane plane = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, animation);
      plane.OriginalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float) b.AsNumeric() * 3.14159274f);
      plane.Doublesided = true;
      plane.Loop = false;
      plane.Opacity = opacity;
      plane.Timing.Step = 0.0f;
      BackgroundPlane backgroundPlane = plane;
      levelManager.AddPlane(plane);
      backgroundPlane.Position = instance.Center * FezMath.XZMask + vector3_1 * num2 + num1 * Vector3.UnitY - vector3_2;
      backgroundPlane.Billboard = true;
    }
    if (!onLeft)
      return;
    IGameLevelManager levelManager1 = this.LevelManager;
    BackgroundPlane plane1 = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, animation);
    plane1.OriginalRotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f) * Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float) b.AsNumeric() * 3.14159274f);
    plane1.Doublesided = true;
    plane1.Loop = false;
    plane1.Opacity = opacity;
    plane1.Timing.Step = 0.0f;
    BackgroundPlane backgroundPlane1 = plane1;
    levelManager1.AddPlane(plane1);
    backgroundPlane1.Position = instance.Center * FezMath.XZMask - vector3_1 * num2 + num1 * Vector3.UnitY - vector3_2;
    backgroundPlane1.Billboard = true;
  }

  private void DetermineFloatMalus(PickupState pickup)
  {
    TrileInstance instance = pickup.Instance;
    int num = 0;
    Vector3 vector3 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    TrileInstance trileInstance = instance;
    Vector3 b = instance.Center * vector3;
    do
    {
      TrileInstance nearLow = this.PlayerManager.Ground.NearLow;
      TrileInstance farHigh = this.PlayerManager.Ground.FarHigh;
      TrileInstance heldInstance = this.PlayerManager.HeldInstance;
      if (nearLow == trileInstance || nearLow != null && FezMath.AlmostEqual(nearLow.Center * vector3, b) || farHigh == trileInstance || farHigh != null && FezMath.AlmostEqual(farHigh.Center * vector3, b) || heldInstance == trileInstance || heldInstance != null && FezMath.AlmostEqual(heldInstance.Center * vector3, b))
        ++num;
      if (instance.PhysicsState.PushedDownBy != null)
      {
        ++num;
        trileInstance = trileInstance.PhysicsState.PushedDownBy;
      }
      else
        trileInstance = (TrileInstance) null;
    }
    while (trileInstance != null);
    pickup.FloatMalus = MathHelper.Lerp(pickup.FloatMalus, -0.25f * (float) num, 0.1f);
    if (num == 0 || pickup.Group == null)
      return;
    foreach (TrileInstance trile in pickup.Group.Triles)
      trile.PhysicsState.Puppet = true;
    pickup.Instance.PhysicsState.Puppet = false;
  }

  private static float BreakHeight(ActorType type)
  {
    if (type <= ActorType.Vase)
    {
      if (type != ActorType.PickUp)
      {
        if (type == ActorType.Vase)
          return 1f;
        goto label_6;
      }
    }
    else if (type != ActorType.TntPickup && type != ActorType.SinkPickup)
      goto label_6;
    return 7f;
label_6:
    throw new InvalidOperationException();
  }

  private void PlayBreakSound(ActorType type, Vector3 position)
  {
    if (type == ActorType.PickUp)
      return;
    if (type == ActorType.Vase)
      this.vaseBreakSound.EmitAt(position);
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPhysicsManager PhysicsManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public ITrixelParticleSystems ParticleSystemManager { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public IPlaneParticleSystems PlaneParticleSystems { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency(Optional = true)]
  public IDebuggingBag DebuggingBag { private get; set; }
}
