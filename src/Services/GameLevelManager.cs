// Decompiled with JetBrains decompiler
// Type: FezGame.Services.GameLevelManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Components.Scripting;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Services;

public class GameLevelManager(Game game) : LevelManager(game), IGameLevelManager, ILevelManager
{
  private readonly List<string> DotLoadLevels = new List<string>()
  {
    "MEMORY_CORE+NATURE_HUB",
    "NATURE_HUB+MEMORY_CORE",
    "MEMORY_CORE+ZU_CITY",
    "ZU_CITY+MEMORY_CORE",
    "MEMORY_CORE+WALL_VILLAGE",
    "WALL_VILLAGE+MEMORY_CORE",
    "MEMORY_CORE+INDUSTRIAL_CITY",
    "INDUSTRIAL_CITY+MEMORY_CORE",
    "PIVOT_WATERTOWER+INDUSTRIAL_HUB",
    "INDUSTRIAL_HUB+PIVOT_WATERTOWER",
    "WELL_2+SEWER_START",
    "SEWER_START+WELL_2",
    "GRAVE_CABIN+GRAVEYARD_GATE",
    "GRAVEYARD_GATE+GRAVE_CABIN",
    "TREE+TREE_SKY",
    "TREE_SKY+TREE",
    "WATERFALL+MINE_A",
    "MINE_A+WATERFALL",
    "SEWER_TO_LAVA+LAVA",
    "LAVA+SEWER_TO_LAVA"
  };
  private Level oldLevel;
  private readonly Dictionary<TrileInstance, TrileGroup> pickupGroups = new Dictionary<TrileInstance, TrileGroup>();

  public bool SongChanged { get; set; }

  public override void Load(string levelName)
  {
    levelName = levelName.Replace('\\', '/');
    string str = levelName;
    Level level;
    using (MemoryContentManager memoryContentManager = new MemoryContentManager((IServiceProvider) this.Game.Services, this.Game.Content.RootDirectory))
    {
      if (!string.IsNullOrEmpty(this.Name))
        levelName = this.Name.Substring(0, this.Name.LastIndexOf("/") + 1) + levelName.Substring(levelName.LastIndexOf("/") + 1);
      if (!MemoryContentManager.AssetExists("Levels\\" + levelName.Replace('/', '\\')))
        levelName = str;
      try
      {
        level = memoryContentManager.Load<Level>("Levels/" + levelName);
      }
      catch (Exception ex)
      {
        Logger.LogError(ex);
        this.oldLevel = new Level();
        return;
      }
    }
    level.Name = levelName;
    ContentManager forLevel = this.CMProvider.GetForLevel(levelName);
    foreach (ArtObjectInstance artObjectInstance in level.ArtObjects.Values)
      artObjectInstance.ArtObject = forLevel.Load<ArtObject>($"{"Art Objects"}/{artObjectInstance.ArtObjectName}");
    if (level.Sky == null)
      level.Sky = forLevel.Load<Sky>("Skies/" + level.SkyName);
    if (level.TrileSetName != null)
      level.TrileSet = forLevel.Load<TrileSet>("Trile Sets/" + level.TrileSetName);
    if (level.SongName != null)
    {
      level.Song = forLevel.Load<TrackedSong>("Music/" + level.SongName);
      level.Song.Initialize();
    }
    if (this.levelData != null)
      this.GameState.SaveData.ThisLevel.FirstVisit = false;
    this.ClearArtSatellites();
    this.oldLevel = this.levelData ?? new Level();
    this.levelData = level;
  }

  public override void Rebuild()
  {
    this.OnSkyChanged();
    this.LevelMaterializer.ClearBatches();
    this.LevelMaterializer.RebuildTriles(this.levelData.TrileSet, this.levelData.TrileSet == this.oldLevel.TrileSet);
    this.LevelMaterializer.RebuildInstances();
    if (!this.Quantum)
      this.LevelMaterializer.CleanUp();
    this.LevelMaterializer.InitializeArtObjects();
    foreach (BackgroundPlane backgroundPlane in this.levelData.BackgroundPlanes.Values)
    {
      backgroundPlane.HostMesh = backgroundPlane.Animated ? this.LevelMaterializer.AnimatedPlanesMesh : this.LevelMaterializer.StaticPlanesMesh;
      backgroundPlane.Initialize();
    }
    lock (this.levelData.BackgroundPlanes)
    {
      if (!this.levelData.BackgroundPlanes.ContainsKey(-1))
      {
        if (this.GomezHaloName != null)
          this.levelData.BackgroundPlanes.Add(-1, new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, this.GomezHaloName, false)
          {
            Id = -1,
            LightMap = true,
            AlwaysOnTop = true,
            Billboard = true,
            Filter = this.HaloFiltering ? new Color(0.425f, 0.425f, 0.425f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f),
            PixelatedLightmap = !this.HaloFiltering
          });
      }
    }
    this.pickupGroups.Clear();
    foreach (TrileGroup trileGroup in (IEnumerable<TrileGroup>) this.Groups.Values)
    {
      if (trileGroup.ActorType != ActorType.SuckBlock && trileGroup.Triles.All<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type.IsPickable() && x.Trile.ActorSettings.Type != ActorType.Couch)))
      {
        foreach (TrileInstance trile in trileGroup.Triles)
          this.pickupGroups.Add(trile, trileGroup);
      }
    }
    this.SongChanged = this.Song == null != (this.SoundManager.CurrentlyPlayingSong == null) || this.Song != null && this.SoundManager.CurrentlyPlayingSong != null && this.Song.Name != this.SoundManager.CurrentlyPlayingSong.Name;
    if (this.SongChanged)
    {
      this.SoundManager.ScriptChangedSong = false;
      if (!this.GameState.InCutscene || this.GameState.IsTrialMode)
        Waiters.Wait((Func<bool>) (() => !this.GameState.Loading && !this.GameState.FarawaySettings.InTransition), (Action) (() =>
        {
          if (!this.SoundManager.ScriptChangedSong)
            this.SoundManager.PlayNewSong(8f);
          this.SoundManager.ScriptChangedSong = false;
        }));
    }
    else if (this.Song != null)
    {
      if (!this.GameState.DotLoading)
        this.SoundManager.UpdateSongActiveTracks();
      else
        Waiters.Wait((Func<bool>) (() => !this.GameState.Loading), (Action) (() => this.SoundManager.UpdateSongActiveTracks()));
    }
    this.SoundManager.FadeFrequencies(this.LowPass, 2f);
    this.SoundManager.UnmuteAmbienceTracks();
    if (!this.GameState.InCutscene || this.GameState.IsTrialMode)
      Waiters.Wait((Func<bool>) (() => !this.GameState.Loading && !this.GameState.FarawaySettings.InTransition), (Action) (() => this.SoundManager.PlayNewAmbience()));
    this.oldLevel = (Level) null;
    this.FullPath = this.Name;
  }

  public void ChangeLevel(string levelName)
  {
    this.GameState.DotLoading = this.DotLoadLevels.Contains($"{this.Name}+{levelName}") || this.PlayerManager.Action == ActionType.LesserWarp || this.PlayerManager.Action == ActionType.GateWarp;
    if (this.GameState.DotLoading)
    {
      this.SoundManager.PlayNewSong((string) null, 1f);
      List<AmbienceTrack> ambienceTracks = this.levelData.AmbienceTracks;
      this.levelData.AmbienceTracks = new List<AmbienceTrack>();
      this.SoundManager.PlayNewAmbience();
      this.levelData.AmbienceTracks = ambienceTracks;
    }
    this.GameService.CloseScroll((string) null);
    if (levelName == this.Name && this.DestinationVolumeId.HasValue)
    {
      IDictionary<int, Volume> volumes1 = this.Volumes;
      int? destinationVolumeId = this.DestinationVolumeId;
      int key1 = destinationVolumeId.Value;
      if (volumes1.ContainsKey(key1))
      {
        this.LastLevelName = this.Name;
        IDictionary<int, Volume> volumes2 = this.Volumes;
        destinationVolumeId = this.DestinationVolumeId;
        int key2 = destinationVolumeId.Value;
        Volume volume = volumes2[key2];
        Viewpoint view = volume.Orientations.FirstOrDefault<FaceOrientation>().AsViewpoint();
        this.CameraManager.ChangeViewpoint(view, 1.5f);
        TrileInstance deep = this.NearestTrile(((volume.BoundingBox.Min + volume.BoundingBox.Max) / 2f + new Vector3(1f / 1000f)) with
        {
          Y = volume.BoundingBox.Min.Y - 0.25f
        }, QueryOptions.None, new Viewpoint?(view)).Deep;
        this.GameState.SaveData.Ground = deep.Center;
        this.GameState.SaveData.View = view;
        float y = this.PlayerManager.Position.Y;
        this.PlayerManager.Position = deep.Center + (deep.TransformedSize / 2f + this.PlayerManager.Size / 2f) * Vector3.UnitY * (float) Math.Sign(this.CollisionManager.GravityFactor);
        this.PlayerManager.WallCollision = new MultipleHits<CollisionResult>();
        this.PlayerManager.Ground = new MultipleHits<TrileInstance>();
        this.PlayerManager.Velocity = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448 * 0.01666666753590107) * -Vector3.UnitY;
        this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
        this.PlayerManager.Velocity = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448 * 0.01666666753590107) * -Vector3.UnitY;
        Vector3 originalCenter = this.CameraManager.Center;
        float diff = this.PlayerManager.Position.Y - y;
        Waiters.Interpolate(1.5, (Action<float>) (s => this.CameraManager.Center = new Vector3(originalCenter.X, originalCenter.Y + diff / 2f * Easing.EaseInOut((double) s, EasingType.Sine), originalCenter.Z)));
        this.OnLevelChanging();
        this.OnLevelChanged();
        return;
      }
    }
    bool flag1 = this.GameState.SaveData.World.Count > 0;
    string level = this.GameState.SaveData.Level;
    this.LastLevelName = !flag1 ? (string) null : this.Name;
    this.Load(levelName);
    this.Rebuild();
    if (!this.GameState.SaveData.World.ContainsKey(this.Name))
      this.GameState.SaveData.World.Add(this.Name, new LevelSaveData()
      {
        FirstVisit = true
      });
    this.GameState.SaveData.Level = this.Name;
    this.OnLevelChanging();
    LevelSaveData thisLevel = this.GameState.SaveData.ThisLevel;
    foreach (TrileEmplacement destroyedTrile in thisLevel.DestroyedTriles)
      this.ClearTrile(destroyedTrile);
    foreach (int inactiveArtObject in thisLevel.InactiveArtObjects)
    {
      if (inactiveArtObject < 0)
        this.RemoveArtObject(this.ArtObjects[-(inactiveArtObject + 1)]);
    }
    TrileInstance trileInstance = !flag1 || !(level == levelName) ? (TrileInstance) null : this.ActualInstanceAt(this.GameState.SaveData.Ground);
    float? nullable1 = new float?();
    Viewpoint spawnView = !flag1 || !(level == levelName) ? Viewpoint.Left : this.GameState.SaveData.View;
    bool flag2 = false;
    if (this.LastLevelName != null)
    {
      Volume volume = (Volume) null;
      int? nullable2 = this.DestinationVolumeId;
      if (nullable2.HasValue)
      {
        nullable2 = this.DestinationVolumeId;
        if (nullable2.Value != -1)
        {
          IDictionary<int, Volume> volumes3 = this.Volumes;
          nullable2 = this.DestinationVolumeId;
          int key3 = nullable2.Value;
          if (volumes3.ContainsKey(key3))
          {
            IDictionary<int, Volume> volumes4 = this.Volumes;
            nullable2 = this.DestinationVolumeId;
            int key4 = nullable2.Value;
            volume = volumes4[key4];
            flag2 = true;
            nullable2 = new int?();
            this.DestinationVolumeId = nullable2;
            goto label_31;
          }
        }
      }
      string str = this.LastLevelName.Replace('\\', '/');
      string trimmedLln = str.Substring(str.LastIndexOf('/') + 1);
      foreach (Script script in this.Scripts.Values.Where<Script>((Func<Script, bool>) (s => s.Triggers.Any<ScriptTrigger>((Func<ScriptTrigger, bool>) (t => t.Object.Type == "Volume")))).Where<Script>((Func<Script, bool>) (s => s.Actions.Any<ScriptAction>((Func<ScriptAction, bool>) (a =>
      {
        if (!(a.Object.Type == "Level") || !a.Operation.Contains(nameof (ChangeLevel)))
          return false;
        return a.Arguments[0] == this.LastLevelName || a.Arguments[0] == trimmedLln;
      })))))
      {
        nullable2 = script.Triggers.Where<ScriptTrigger>((Func<ScriptTrigger, bool>) (x => x.Object.Type == "Volume")).First<ScriptTrigger>().Object.Identifier;
        int key = nullable2.Value;
        if (this.Volumes.ContainsKey(key))
        {
          volume = this.Volumes[key];
          flag2 = true;
        }
      }
label_31:
      if (flag2 && volume != null)
      {
        Vector3 vector3 = ((volume.BoundingBox.Min + volume.BoundingBox.Max) / 2f + new Vector3(1f / 1000f)) with
        {
          Y = volume.BoundingBox.Min.Y - 0.25f
        };
        spawnView = volume.Orientations.FirstOrDefault<FaceOrientation>().AsViewpoint();
        nullable1 = new float?(vector3.Dot(spawnView.SideMask()));
        float num = (float) ((double) (volume.BoundingBox.Max - volume.BoundingBox.Min).Dot(spawnView.DepthMask()) / 2.0 + 0.5);
        Vector3 position = vector3 + num * -spawnView.ForwardVector();
        foreach (TrileEmplacement trileEmplacement in thisLevel.InactiveTriles.Union<TrileEmplacement>((IEnumerable<TrileEmplacement>) thisLevel.DestroyedTriles))
        {
          if ((double) Vector3.DistanceSquared(trileEmplacement.AsVector, position) < 2.0)
          {
            position -= spawnView.ForwardVector();
            break;
          }
        }
        trileInstance = this.ActualInstanceAt(position) ?? this.NearestTrile(vector3, QueryOptions.None, new Viewpoint?(spawnView)).Deep;
      }
    }
    InstanceFace instanceFace = new InstanceFace();
    if (!flag1 || trileInstance == null)
    {
      if (this.StartingPosition != (TrileFace) null)
      {
        instanceFace.Instance = this.TrileInstanceAt(ref this.StartingPosition.Id);
        instanceFace.Face = this.StartingPosition.Face;
      }
      else
        instanceFace.Face = spawnView.VisibleOrientation();
      if (instanceFace.Instance == null)
        instanceFace.Instance = this.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => !FezMath.In<CollisionType>(x.GetRotatedFace(spawnView.VisibleOrientation()), CollisionType.None, CollisionType.Immaterial, (IEqualityComparer<CollisionType>) CollisionTypeComparer.Default))).OrderBy<TrileInstance, float>((Func<TrileInstance, float>) (x => Math.Abs((x.Center - this.Size / 2f).Dot(spawnView.ScreenSpaceMask())))).FirstOrDefault<TrileInstance>();
      trileInstance = instanceFace.Instance;
      spawnView = instanceFace.Face.AsViewpoint();
    }
    this.CameraManager.Constrained = false;
    this.CameraManager.PanningConstraints = new Vector2?();
    if (trileInstance != null)
      this.GameState.SaveData.Ground = trileInstance.Center;
    this.GameState.SaveData.View = spawnView;
    this.GameState.SaveData.TimeOfDay = this.TimeManager.CurrentTime.TimeOfDay;
    if (flag2)
      this.PlayerManager.CheckpointGround = (TrileInstance) null;
    this.PlayerManager.RespawnAtCheckpoint();
    if (nullable1.HasValue)
      this.PlayerManager.Position = this.PlayerManager.Position * (Vector3.One - spawnView.SideMask()) + nullable1.Value * spawnView.SideMask();
    this.PlayerManager.Action = ActionType.Idle;
    this.PlayerManager.WallCollision = new MultipleHits<CollisionResult>();
    this.PlayerManager.Ground = new MultipleHits<TrileInstance>();
    this.PlayerManager.Velocity = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448 * 0.01666666753590107) * -Vector3.UnitY;
    this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
    this.PlayerManager.Velocity = (float) (3.1500000953674316 * (double) Math.Sign(this.CollisionManager.GravityFactor) * 0.15000000596046448 * 0.01666666753590107) * -Vector3.UnitY;
    this.CameraManager.InterpolatedCenter = this.CameraManager.Center = this.PlayerManager.Center;
    this.OnLevelChanged();
    this.LevelService.OnStart();
    ScriptingHost.Instance.ForceUpdate(new GameTime());
    if (!this.PlayerManager.SpinThroughDoor)
    {
      if (!this.CameraManager.Constrained)
      {
        this.CameraManager.Center = this.PlayerManager.Center + (float) (4.0 * (this.Descending ? -1.0 : 1.0)) / this.CameraManager.PixelsPerTrixel * Vector3.UnitY;
        this.CameraManager.SnapInterpolation();
      }
      if (!this.GameState.FarawaySettings.InTransition)
        this.LevelMaterializer.ForceCull();
    }
    else if (!this.CameraManager.Constrained)
    {
      IGameCameraManager cameraManager1 = this.CameraManager;
      IGameCameraManager cameraManager2 = this.CameraManager;
      Vector3 center = this.PlayerManager.Center;
      Vector3 vector3_1 = (float) (4.0 * (this.Descending ? -1.0 : 1.0)) / this.CameraManager.PixelsPerTrixel * Vector3.UnitY;
      Vector3 vector3_2;
      Vector3 vector3_3 = vector3_2 = center + vector3_1;
      cameraManager2.Center = vector3_2;
      Vector3 vector3_4 = vector3_3;
      cameraManager1.InterpolatedCenter = vector3_4;
    }
    if (this.Name != "HEX_REBUILD" && this.Name != "DRUM" && this.Name != "VILLAGEVILLE_3D_END_64" && this.Name != "VILLAGEVILLE_3D_END_32")
      this.GameState.Save();
    GC.Collect(3);
  }

  public void ChangeSky(Sky sky)
  {
    this.levelData.Sky = sky;
    this.OnSkyChanged();
  }

  public override void RecordMoveToEnd(int groupId)
  {
    this.GameState.SaveData.ThisLevel.InactiveGroups.Add(groupId);
    this.GameState.Save();
  }

  public override bool IsPathRecorded(int groupId)
  {
    return this.GameState.SaveData.ThisLevel.InactiveGroups.Contains(groupId);
  }

  public string LastLevelName { get; set; }

  public bool DestinationIsFarAway { get; set; }

  public int? DestinationVolumeId { get; set; }

  public bool WentThroughSecretPassage { get; set; }

  public IDictionary<TrileInstance, TrileGroup> PickupGroups
  {
    get => (IDictionary<TrileInstance, TrileGroup>) this.pickupGroups;
  }

  public void Reset()
  {
    this.ClearArtSatellites();
    if (this.levelData.TrileSet != null)
      this.LevelMaterializer.DestroyMaterializers(this.levelData.TrileSet);
    this.levelData = new Level() { Name = string.Empty };
    this.levelData.Sky = this.CMProvider.Global.Load<Sky>("Skies/Default");
    this.OnSkyChanged();
    this.levelData.TrileSet = (TrileSet) null;
    this.LevelMaterializer.RebuildInstances();
    this.LevelMaterializer.CullInstances();
    this.LastLevelName = (string) null;
    this.OnLevelChanging();
    this.OnLevelChanged();
    this.LevelMaterializer.TrilesMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/FullWhite");
  }

  public void RemoveArtObject(ArtObjectInstance aoInstance)
  {
    if (aoInstance.ActorSettings.AttachedGroup.HasValue)
    {
      int key = aoInstance.ActorSettings.AttachedGroup.Value;
      foreach (TrileInstance instance in this.Groups[aoInstance.ActorSettings.AttachedGroup.Value].Triles.ToArray())
        this.ClearTrile(instance);
      this.Groups.Remove(key);
    }
    this.ArtObjects.Remove(aoInstance.Id);
    aoInstance.Dispose();
    this.LevelMaterializer.RegisterSatellites();
  }

  public override bool WasPathSupposedToBeRecorded(int id)
  {
    switch (this.Name)
    {
      case "OWL":
        if (id == 0 && this.GameState.SaveData.ThisLevel.ScriptingState == "4")
        {
          this.RecordMoveToEnd(id);
          return true;
        }
        break;
      case "ARCH":
        if (id == 3 && this.GameState.SaveData.ThisLevel.InactiveGroups.Contains(4))
        {
          this.RecordMoveToEnd(id);
          return true;
        }
        break;
      case "WATERFALL":
        if (id == 1 && this.GameState.SaveData.ThisLevel.InactiveVolumes.Contains(19))
        {
          this.RecordMoveToEnd(id);
          return true;
        }
        break;
    }
    return false;
  }

  [ServiceDependency]
  public IDotManager DotManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IPhysicsManager PhysicsManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  [ServiceDependency]
  public IGameService GameService { private get; set; }

  [ServiceDependency]
  public ILevelService LevelService { private get; set; }
}
