// Decompiled with JetBrains decompiler
// Type: FezGame.Components.BombsHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class BombsHost : DrawableGameComponent
{
  private readonly Color FlashColor = new Color((int) byte.MaxValue, 0, 0, 128 /*0x80*/);
  private readonly TimeSpan FlashTime = TimeSpan.FromSeconds(4.0);
  private readonly TimeSpan ExplodeStart = TimeSpan.FromSeconds(6.0);
  private readonly TimeSpan ChainsplodeDelay = TimeSpan.FromSeconds(0.25);
  private AnimatedTexture bombAnimation;
  private AnimatedTexture bigBombAnimation;
  private AnimatedTexture tntAnimation;
  private Texture2D flare;
  private readonly Dictionary<TrileInstance, BombsHost.BombState> bombStates = new Dictionary<TrileInstance, BombsHost.BombState>();
  private readonly List<BombsHost.DestructibleGroup> destructibleGroups = new List<BombsHost.DestructibleGroup>();
  private readonly Dictionary<TrileInstance, BombsHost.DestructibleGroup> indexedDg = new Dictionary<TrileInstance, BombsHost.DestructibleGroup>();
  private Mesh flashesMesh;
  private SoundEffect explodeSound;
  private SoundEffect crystalsplodeSound;
  private SoundEffect countdownSound;
  private static readonly Point[] SmallBombOffsets = new Point[9]
  {
    new Point(0, 0),
    new Point(1, 0),
    new Point(-1, 0),
    new Point(0, 1),
    new Point(0, -1),
    new Point(1, 1),
    new Point(1, -1),
    new Point(-1, 1),
    new Point(-1, -1)
  };
  private static readonly Point[] BigBombOffsets = new Point[25]
  {
    new Point(0, 0),
    new Point(1, 0),
    new Point(2, 0),
    new Point(-1, 0),
    new Point(-2, 0),
    new Point(0, 1),
    new Point(0, -1),
    new Point(0, 2),
    new Point(0, -2),
    new Point(-2, -2),
    new Point(-1, -2),
    new Point(-2, -1),
    new Point(-1, -1),
    new Point(2, -2),
    new Point(1, -2),
    new Point(2, -1),
    new Point(1, -1),
    new Point(-2, 2),
    new Point(-1, 2),
    new Point(-2, 1),
    new Point(-1, 1),
    new Point(2, 2),
    new Point(1, 2),
    new Point(2, 1),
    new Point(1, 1)
  };
  private readonly List<TrileInstance> bsToRemove = new List<TrileInstance>();
  private readonly List<KeyValuePair<TrileInstance, BombsHost.BombState>> bsToAdd = new List<KeyValuePair<TrileInstance, BombsHost.BombState>>();
  private const float H = 0.499f;
  private static readonly Vector3[] CornerNeighbors = new Vector3[5]
  {
    new Vector3(0.499f, 1f, 0.499f),
    new Vector3(0.499f, 1f, -0.499f),
    new Vector3(-0.499f, 1f, 0.499f),
    new Vector3(-0.499f, 1f, -0.499f),
    new Vector3(0.0f, 1f, 0.0f)
  };

  public BombsHost(Game game)
    : base(game)
  {
    this.DrawOrder = 10;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
  }

  private void TryInitialize()
  {
    this.flashesMesh.ClearGroups();
    this.bombStates.Clear();
    this.indexedDg.Clear();
    this.destructibleGroups.Clear();
    foreach (TrileGroup trileGroup in (IEnumerable<TrileGroup>) this.LevelManager.Groups.Values)
    {
      if (trileGroup.Triles.Count != 0 && trileGroup.Triles[0].Trile.ActorSettings.Type.IsDestructible() && trileGroup.Triles[trileGroup.Triles.Count - 1].Trile.ActorSettings.Type.IsDestructible())
      {
        BombsHost.DestructibleGroup destructibleGroup = new BombsHost.DestructibleGroup()
        {
          AllTriles = new List<TrileInstance>((IEnumerable<TrileInstance>) trileGroup.Triles),
          Group = trileGroup
        };
        this.destructibleGroups.Add(destructibleGroup);
        FaceOrientation face = FaceOrientation.Down;
        foreach (TrileInstance trile in trileGroup.Triles)
        {
          this.indexedDg.Add(trile, destructibleGroup);
          TrileEmplacement traversal = trile.Emplacement.GetTraversal(ref face);
          TrileInstance instance = this.LevelManager.TrileInstanceAt(ref traversal);
          if (instance != null && !instance.Trile.ActorSettings.Type.IsDestructible() && instance.PhysicsState == null)
            instance.PhysicsState = new InstancePhysicsState(instance);
        }
      }
    }
  }

  protected override void LoadContent()
  {
    this.explodeSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/BombExplode");
    this.crystalsplodeSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/TntExplode");
    this.countdownSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/BombCountdown");
    this.bombAnimation = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/BombExplosion");
    this.bigBombAnimation = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/BigBombExplosion");
    this.tntAnimation = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/TntExplosion");
    this.flashesMesh = new Mesh()
    {
      AlwaysOnTop = true,
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.flare = this.CMProvider.Global.Load<Texture2D>("Background Planes/Flare");
      this.flashesMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
    }));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective || !this.CameraManager.ActionRunning || this.GameState.Paused || this.GameState.InMap || this.CameraManager.RequestedViewpoint != Viewpoint.None || this.GameState.Loading)
      return;
    foreach (BombsHost.DestructibleGroup destructibleGroup1 in this.destructibleGroups)
    {
      if (destructibleGroup1.RespawnIn.HasValue)
      {
        BombsHost.DestructibleGroup destructibleGroup2 = destructibleGroup1;
        float? respawnIn = destructibleGroup2.RespawnIn;
        float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
        destructibleGroup2.RespawnIn = respawnIn.HasValue ? new float?(respawnIn.GetValueOrDefault() - totalSeconds) : new float?();
        if ((double) destructibleGroup1.RespawnIn.Value <= 0.0)
        {
          bool flag = true;
          foreach (TrileInstance allTrile in destructibleGroup1.AllTriles)
          {
            if (!allTrile.Enabled || allTrile.Hidden || allTrile.Removed)
            {
              allTrile.Enabled = false;
              allTrile.Hidden = true;
              ServiceHelper.AddComponent((IGameComponent) new GlitchyRespawner(ServiceHelper.Game, allTrile, flag || RandomHelper.Probability(0.25)));
              flag = false;
            }
          }
          destructibleGroup1.RespawnIn = new float?();
        }
      }
    }
    TrileInstance carriedInstance = this.PlayerManager.CarriedInstance;
    if (carriedInstance != null && carriedInstance.Trile.ActorSettings.Type.IsBomb() && !this.bombStates.ContainsKey(carriedInstance))
    {
      carriedInstance.Foreign = carriedInstance.PhysicsState.Respawned = false;
      this.bombStates.Add(carriedInstance, new BombsHost.BombState());
    }
    bool flag1 = false;
    bool flag2 = false;
    foreach (TrileInstance key in this.bombStates.Keys)
    {
      BombsHost.BombState bombState = this.bombStates[key];
      if (!this.PlayerManager.Action.IsEnteringDoor())
        bombState.SincePickup += gameTime.ElapsedGameTime;
      bool flag3 = key.Trile.ActorSettings.Type == ActorType.BigBomb;
      bool flag4 = key.Trile.ActorSettings.Type == ActorType.TntBlock || key.Trile.ActorSettings.Type == ActorType.TntPickup;
      if (key.Trile.ActorSettings.Type.IsBomb() && key.Hidden)
      {
        this.bsToRemove.Add(key);
        if (bombState.Flash != null)
        {
          this.flashesMesh.RemoveGroup(bombState.Flash);
          bombState.Flash = (Group) null;
        }
        if (bombState.Emitter != null && bombState.Emitter.Cue != null)
          bombState.Emitter.Cue.Stop();
      }
      else
      {
        if (bombState.SincePickup > this.FlashTime && bombState.Explosion == null)
        {
          if (bombState.Flash == null)
          {
            bombState.Flash = this.flashesMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, this.FlashColor, true);
            if (key.Trile.ActorSettings.Type.IsBomb() && !bombState.IsChainsploding)
            {
              bombState.Emitter = this.countdownSound.EmitAt(key.Center);
              bombState.Emitter.PauseViewTransitions = true;
            }
          }
          double totalSeconds = bombState.SincePickup.TotalSeconds;
          if (totalSeconds > this.ExplodeStart.TotalSeconds - 1.0)
            totalSeconds *= 2.0;
          bombState.Flash.Enabled = FezMath.Frac(totalSeconds) < 0.5;
          if (bombState.Flash.Enabled)
          {
            bombState.Flash.Position = key.Center;
            bombState.Flash.Rotation = this.CameraManager.Rotation;
          }
        }
        if (bombState.SincePickup > this.ExplodeStart && bombState.Explosion == null)
        {
          if (flag4 && !flag1 || !flag4 && !flag2)
          {
            (flag4 ? this.crystalsplodeSound : this.explodeSound).EmitAt(key.Center, RandomHelper.Centered(0.025));
            if (flag4)
              flag1 = true;
            else
              flag2 = true;
          }
          if (bombState.ChainsplodedBy != null && bombState.ChainsplodedBy.Emitter != null)
            bombState.ChainsplodedBy.Emitter.FadeOutAndDie(0.0f);
          double num1 = flag3 ? 0.60000002384185791 : 0.30000001192092896;
          Vector3 vector3_1 = key.Center - this.PlayerManager.Center;
          double num2 = (double) FezMath.Saturate((float) (1.0 - (double) vector3_1.Length() / 15.0));
          float num3 = (float) (num1 * num2);
          if (CamShake.CurrentCamShake == null)
            ServiceHelper.AddComponent((IGameComponent) new CamShake(this.Game)
            {
              Duration = TimeSpan.FromSeconds(0.75),
              Distance = num3
            });
          else
            CamShake.CurrentCamShake.Reset();
          this.ParticleSystemManager.PropagateEnergy(key.Center, flag3 ? 6f : 3f);
          this.flashesMesh.RemoveGroup(bombState.Flash);
          bombState.Flash = (Group) null;
          switch (key.Trile.ActorSettings.Type)
          {
            case ActorType.BigBomb:
              bombState.Explosion = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, this.bigBombAnimation)
              {
                ActorType = ActorType.Bomb
              };
              break;
            case ActorType.TntBlock:
            case ActorType.TntPickup:
              bombState.Explosion = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, this.tntAnimation)
              {
                ActorType = ActorType.Bomb
              };
              break;
            default:
              bombState.Explosion = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, this.bombAnimation)
              {
                ActorType = ActorType.Bomb
              };
              break;
          }
          bombState.Explosion.Timing.Loop = false;
          bombState.Explosion.Billboard = true;
          bombState.Explosion.Fullbright = true;
          bombState.Explosion.OriginalRotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float) RandomHelper.Random.Next(0, 4) * 1.57079637f);
          bombState.Explosion.Timing.Restart();
          this.LevelManager.AddPlane(bombState.Explosion);
          bombState.Flare = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, (Texture) this.flare)
          {
            AlwaysOnTop = true,
            LightMap = true,
            AllowOverbrightness = true,
            Billboard = true
          };
          this.LevelManager.AddPlane(bombState.Flare);
          bombState.Flare.Scale = Vector3.One * (flag3 ? 3f : 1.5f);
          BackgroundPlane explosion = bombState.Explosion;
          bombState.Flare.Position = vector3_1 = key.Center + (RandomHelper.Centered(1.0 / 1000.0) - 0.5f) * this.CameraManager.Viewpoint.ForwardVector();
          Vector3 vector3_2 = vector3_1;
          explosion.Position = vector3_2;
          float num4 = flag3 ? 3f : 1.5f;
          vector3_1 = (this.PlayerManager.Position - key.Center) * this.CameraManager.Viewpoint.ScreenSpaceMask();
          float num5 = vector3_1.Length();
          if ((this.PlayerManager.CarriedInstance == key || (double) num5 < (double) num4) && this.PlayerManager.Action != ActionType.Dying)
            this.PlayerManager.Action = ActionType.Suffering;
          if ((key.Trile.ActorSettings.Type == ActorType.TntBlock || bombState.IsChainsploding) && key.InstanceId != -1)
            this.ParticleSystemManager.Add(new TrixelParticleSystem(this.Game, new TrixelParticleSystem.Settings()
            {
              ExplodingInstance = key,
              EnergySource = new Vector3?(key.Center),
              MaximumSize = 7,
              Energy = flag4 ? 3f : 1.5f,
              Darken = true,
              ParticleCount = 4 + 12 / Math.Max(1, this.TrixelParticleSystems.Count - 3)
            }));
          if (key.Trile.ActorSettings.Type.IsPickable())
          {
            key.Enabled = false;
            this.LevelMaterializer.GetTrileMaterializer(key.Trile).UpdateInstance(key);
          }
          else
            this.ClearDestructible(key, false);
          this.DropSupportedTriles(key);
          this.DestroyNeighborhood(key, bombState);
        }
        if (bombState.Explosion != null)
        {
          bombState.Flare.Filter = Color.Lerp(flag4 ? new Color(0.5f, 1f, 0.25f) : new Color(1f, 0.5f, 0.25f), Color.Black, bombState.Explosion.Timing.NormalizedStep);
          if (bombState.Explosion.Timing.Ended)
          {
            this.bsToRemove.Add(key);
            if (key.PhysicsState != null)
              key.PhysicsState.ShouldRespawn = key.Trile.ActorSettings.Type.IsPickable();
            this.LevelManager.RemovePlane(bombState.Explosion);
            this.LevelManager.RemovePlane(bombState.Flare);
          }
        }
      }
    }
    foreach (TrileInstance key in this.bsToRemove)
      this.bombStates.Remove(key);
    this.bsToRemove.Clear();
    foreach (KeyValuePair<TrileInstance, BombsHost.BombState> keyValuePair in this.bsToAdd)
    {
      if (!this.bombStates.ContainsKey(keyValuePair.Key))
        this.bombStates.Add(keyValuePair.Key, keyValuePair.Value);
    }
    this.bsToAdd.Clear();
  }

  private void ClearDestructible(TrileInstance instance, bool skipRecull)
  {
    BombsHost.DestructibleGroup destructibleGroup;
    if (this.indexedDg.TryGetValue(instance, out destructibleGroup))
    {
      int count1 = this.LevelManager.Groups.Count;
      this.LevelManager.ClearTrile(instance, skipRecull);
      int count2 = this.LevelManager.Groups.Count;
      if (count1 != count2)
      {
        foreach (TrileInstance allTrile in destructibleGroup.AllTriles)
        {
          this.GameState.SaveData.ThisLevel.DestroyedTriles.Add(allTrile.OriginalEmplacement);
          this.indexedDg.Remove(allTrile);
        }
        this.destructibleGroups.Remove(destructibleGroup);
        destructibleGroup.RespawnIn = new float?();
      }
      else
        destructibleGroup.RespawnIn = new float?(1.5f);
    }
    else
      this.LevelManager.ClearTrile(instance, skipRecull);
  }

  private void DestroyNeighborhood(TrileInstance instance, BombsHost.BombState state)
  {
    Vector3 vector3_1 = this.CameraManager.Viewpoint.SideMask();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.ForwardVector();
    bool flag1 = (double) vector3_1.X != 0.0;
    bool flag2 = flag1;
    int num = flag2 ? (int) vector3_2.Z : (int) vector3_2.X;
    Point point1 = new Point(flag1 ? instance.Emplacement.X : instance.Emplacement.Z, instance.Emplacement.Y);
    Point[] pointArray = instance.Trile.ActorSettings.Type == ActorType.BigBomb ? BombsHost.BigBombOffsets : BombsHost.SmallBombOffsets;
    this.LevelManager.WaitForScreenInvalidation();
    foreach (Point point2 in pointArray)
    {
      bool chainsploded = false;
      bool needsRecull = false;
      Point key = new Point(point1.X + point2.X, point1.Y + point2.Y);
      Limit limit;
      if (this.LevelManager.ScreenSpaceLimits.TryGetValue(key, out limit))
      {
        limit.End += num;
        TrileEmplacement id = new TrileEmplacement(flag1 ? key.X : limit.Start, key.Y, flag2 ? limit.Start : key.X);
        while ((flag2 ? id.Z : id.X) != limit.End)
        {
          TrileInstance nearestNeighbor = this.LevelManager.TrileInstanceAt(ref id);
          if (!this.TryExplodeAt(state, nearestNeighbor, ref chainsploded, ref needsRecull))
          {
            if (flag2)
              id.Z += num;
            else
              id.X += num;
          }
          else
            break;
        }
        if (needsRecull)
        {
          this.LevelManager.RecullAt(id);
          this.TrixelParticleSystems.UnGroundAll();
        }
      }
    }
  }

  private bool TryExplodeAt(
    BombsHost.BombState state,
    TrileInstance nearestNeighbor,
    ref bool chainsploded,
    ref bool needsRecull)
  {
    if (nearestNeighbor != null && nearestNeighbor.Enabled && !nearestNeighbor.Trile.Immaterial)
    {
      if (!nearestNeighbor.Trile.ActorSettings.Type.IsChainsploding() && !nearestNeighbor.Trile.ActorSettings.Type.IsDestructible())
        return true;
      if (!this.bombStates.ContainsKey(nearestNeighbor))
      {
        if (nearestNeighbor.Trile.ActorSettings.Type.IsBomb())
          nearestNeighbor.PhysicsState.Respawned = false;
        if (!chainsploded)
        {
          this.bsToAdd.Add(new KeyValuePair<TrileInstance, BombsHost.BombState>(nearestNeighbor, new BombsHost.BombState()
          {
            SincePickup = state.SincePickup - this.ChainsplodeDelay,
            IsChainsploding = true,
            ChainsplodedBy = state
          }));
          chainsploded = true;
        }
        else
        {
          this.ClearDestructible(nearestNeighbor, true);
          this.LevelMaterializer.CullInstanceOut(nearestNeighbor);
          this.DropSupportedTriles(nearestNeighbor);
          needsRecull = true;
        }
        return true;
      }
    }
    return false;
  }

  private void DropSupportedTriles(TrileInstance instance)
  {
    foreach (Vector3 cornerNeighbor in BombsHost.CornerNeighbors)
    {
      TrileInstance trileInstance = this.LevelManager.ActualInstanceAt(instance.Center + instance.TransformedSize * cornerNeighbor);
      if (trileInstance != null && trileInstance.PhysicsState != null)
      {
        MultipleHits<TrileInstance> ground = trileInstance.PhysicsState.Ground;
        MultipleHits<TrileInstance> multipleHits1;
        if (ground.NearLow == instance)
        {
          InstancePhysicsState physicsState = trileInstance.PhysicsState;
          multipleHits1 = new MultipleHits<TrileInstance>();
          multipleHits1.FarHigh = ground.FarHigh;
          MultipleHits<TrileInstance> multipleHits2 = multipleHits1;
          physicsState.Ground = multipleHits2;
        }
        if (ground.FarHigh == instance)
        {
          InstancePhysicsState physicsState = trileInstance.PhysicsState;
          multipleHits1 = new MultipleHits<TrileInstance>();
          multipleHits1.NearLow = ground.NearLow;
          MultipleHits<TrileInstance> multipleHits3 = multipleHits1;
          physicsState.Ground = multipleHits3;
        }
      }
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective || this.bombStates.Count == 0)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.Bomb);
    this.flashesMesh.Draw();
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public ITrixelParticleSystems TrixelParticleSystems { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ITrixelParticleSystems ParticleSystemManager { private get; set; }

  private class BombState
  {
    public TimeSpan SincePickup;
    public BackgroundPlane Explosion;
    public BackgroundPlane Flare;
    public Group Flash;
    public bool IsChainsploding;
    public SoundEmitter Emitter;
    public BombsHost.BombState ChainsplodedBy;
  }

  private class DestructibleGroup
  {
    public List<TrileInstance> AllTriles;
    public TrileGroup Group;
    public float? RespawnIn;
  }
}
