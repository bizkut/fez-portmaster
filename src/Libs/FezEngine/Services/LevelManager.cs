// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.LevelManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable disable
namespace FezEngine.Services;

public abstract class LevelManager : GameComponent, ILevelManager
{
  private const bool AccurateCollision = false;
  private readonly Trile fallbackTrile;
  protected Level levelData;
  private Color actualAmbient;
  private Color actualDiffuse;
  private Dictionary<Point, Limit> screenSpaceLimits = new Dictionary<Point, Limit>((IEqualityComparer<Point>) LevelManager.FastPointComparer.Default);
  private static readonly object Mutex = new object();
  private Worker<Dictionary<Point, Limit>> screenInvalidationWorker;

  public event Action LevelChanged = new Action(Util.NullAction);

  public event Action LevelChanging = new Action(Util.NullAction);

  public event Action LightingChanged = new Action(Util.NullAction);

  public event Action SkyChanged = new Action(Util.NullAction);

  public event Action ScreenInvalidated;

  public event Action<TrileInstance> TrileRestored = new Action<TrileInstance>(Util.NullAction<TrileInstance>);

  protected LevelManager(Game game)
    : base(game)
  {
    this.levelData = new Level() { SkyName = "Blue" };
    this.fallbackTrile = new Trile(CollisionType.TopOnly)
    {
      Id = -1
    };
    this.UpdateOrder = -2;
  }

  public override void Initialize()
  {
    this.CameraManager.ViewpointChanged += new Action(this.InvalidateScreen);
    this.InvalidateScreen();
  }

  public TrileFace StartingPosition
  {
    get => this.levelData.StartingPosition;
    set => this.levelData.StartingPosition = value;
  }

  public string Name
  {
    get => this.levelData.Name;
    set => this.levelData.Name = value;
  }

  public string FullPath { get; set; }

  public bool SkipPostProcess
  {
    get => this.levelData.SkipPostProcess;
    set => this.levelData.SkipPostProcess = value;
  }

  public virtual float BaseAmbient
  {
    get => this.levelData.BaseAmbient;
    set => this.levelData.BaseAmbient = value;
  }

  public virtual float BaseDiffuse
  {
    get => this.levelData.BaseDiffuse;
    set => this.levelData.BaseDiffuse = value;
  }

  public Color ActualAmbient
  {
    get => this.actualAmbient;
    set
    {
      int num = this.actualAmbient != value ? 1 : 0;
      this.actualAmbient = value;
      if (num == 0)
        return;
      this.OnLightingChanged();
    }
  }

  public Color ActualDiffuse
  {
    get => this.actualDiffuse;
    set
    {
      int num = this.actualDiffuse != value ? 1 : 0;
      this.actualDiffuse = value;
      if (num == 0)
        return;
      this.OnLightingChanged();
    }
  }

  public bool Flat
  {
    get => this.levelData.Flat;
    set => this.levelData.Flat = value;
  }

  public Vector3 Size
  {
    get => this.levelData.Size;
    set => this.levelData.Size = value;
  }

  public string SequenceSamplesPath
  {
    get => this.levelData.SequenceSamplesPath;
    set => this.levelData.SequenceSamplesPath = value;
  }

  public bool HaloFiltering
  {
    get => this.levelData.HaloFiltering;
    set => this.levelData.HaloFiltering = value;
  }

  public string GomezHaloName
  {
    get => this.levelData.GomezHaloName;
    set => this.levelData.GomezHaloName = value;
  }

  public bool BlinkingAlpha
  {
    get => this.levelData.BlinkingAlpha;
    set => this.levelData.BlinkingAlpha = value;
  }

  public bool Loops
  {
    get => this.levelData.Loops;
    set => this.levelData.Loops = value;
  }

  public float WaterHeight
  {
    get => this.levelData.WaterHeight;
    set => this.levelData.WaterHeight = value;
  }

  public float OriginalWaterHeight { get; set; }

  public float WaterSpeed { get; set; }

  public LiquidType WaterType
  {
    get => this.levelData.WaterType;
    set => this.levelData.WaterType = value;
  }

  public bool Descending
  {
    get => this.levelData.Descending;
    set => this.levelData.Descending = value;
  }

  public bool Rainy
  {
    get => this.levelData.Rainy;
    set => this.levelData.Rainy = value;
  }

  public string SongName
  {
    get => this.levelData.SongName;
    set => this.levelData.SongName = value;
  }

  public bool LowPass
  {
    get => this.levelData.LowPass;
    set => this.levelData.LowPass = value;
  }

  public LevelNodeType NodeType
  {
    get => this.levelData.NodeType;
    set => this.levelData.NodeType = value;
  }

  public int FAPFadeOutStart
  {
    get => this.levelData.FAPFadeOutStart;
    set => this.levelData.FAPFadeOutStart = value;
  }

  public int FAPFadeOutLength
  {
    get => this.levelData.FAPFadeOutLength;
    set => this.levelData.FAPFadeOutLength = value;
  }

  public bool Quantum
  {
    get => this.levelData.Quantum;
    set => this.levelData.Quantum = value;
  }

  public IDictionary<TrileEmplacement, TrileInstance> Triles
  {
    get => (IDictionary<TrileEmplacement, TrileInstance>) this.levelData.Triles;
  }

  public Sky Sky => this.levelData.Sky;

  public TrileSet TrileSet => this.levelData.TrileSet;

  public TrackedSong Song => this.levelData.Song;

  public IDictionary<int, Volume> Volumes => (IDictionary<int, Volume>) this.levelData.Volumes;

  public IDictionary<int, ArtObjectInstance> ArtObjects
  {
    get => (IDictionary<int, ArtObjectInstance>) this.levelData.ArtObjects;
  }

  public IDictionary<int, BackgroundPlane> BackgroundPlanes
  {
    get => (IDictionary<int, BackgroundPlane>) this.levelData.BackgroundPlanes;
  }

  public IDictionary<int, TrileGroup> Groups
  {
    get => (IDictionary<int, TrileGroup>) this.levelData.Groups;
  }

  public IDictionary<int, NpcInstance> NonPlayerCharacters
  {
    get => (IDictionary<int, NpcInstance>) this.levelData.NonPlayerCharacters;
  }

  public IDictionary<int, Script> Scripts => (IDictionary<int, Script>) this.levelData.Scripts;

  public IDictionary<int, MovementPath> Paths
  {
    get => (IDictionary<int, MovementPath>) this.levelData.Paths;
  }

  public IList<string> MutedLoops => (IList<string>) this.levelData.MutedLoops;

  public IList<AmbienceTrack> AmbienceTracks
  {
    get => (IList<AmbienceTrack>) this.levelData.AmbienceTracks;
  }

  public abstract void Load(string levelName);

  public abstract void Rebuild();

  public void ClearArtSatellites()
  {
    foreach (ArtObjectInstance levelArtObject in this.LevelMaterializer.LevelArtObjects)
      levelArtObject.Dispose(true);
    foreach (BackgroundPlane backgroundPlane in (IEnumerable<BackgroundPlane>) this.BackgroundPlanes.Values)
      backgroundPlane.Dispose();
  }

  public Trile SafeGetTrile(int trileId)
  {
    return trileId == -1 || this.TrileSet == null ? this.fallbackTrile : this.TrileSet[trileId];
  }

  public TrileInstance TrileInstanceAt(ref TrileEmplacement id)
  {
    TrileInstance trileInstance;
    return !this.Triles.TryGetValue(id, out trileInstance) ? (TrileInstance) null : trileInstance;
  }

  public bool TrileExists(TrileEmplacement emplacement)
  {
    return this.levelData.Triles.ContainsKey(emplacement);
  }

  protected void AddInstance(TrileEmplacement emplacement, TrileInstance instance)
  {
    this.levelData.Triles.Add(emplacement, instance);
    instance.Removed = false;
  }

  public virtual void RecordMoveToEnd(int groupId)
  {
  }

  public virtual bool IsPathRecorded(int groupId) => false;

  public bool IsCornerTrile(
    ref TrileEmplacement id,
    ref FaceOrientation face1,
    ref FaceOrientation face2)
  {
    TrileInstance trileInstance;
    if (!this.levelData.Triles.TryGetValue(id.GetTraversal(ref face1), out trileInstance) || trileInstance.Trile.SeeThrough || trileInstance.ForceSeeThrough)
      return true;
    TrileEmplacement traversal = id.GetTraversal(ref face2);
    if (!this.levelData.Triles.TryGetValue(traversal, out trileInstance) || trileInstance.Trile.SeeThrough || trileInstance.ForceSeeThrough)
      return true;
    traversal = traversal.GetTraversal(ref face1);
    return !this.levelData.Triles.TryGetValue(traversal, out trileInstance) || trileInstance.Trile.SeeThrough || trileInstance.ForceSeeThrough;
  }

  public bool IsBorderTrileFace(ref TrileEmplacement id, ref FaceOrientation face)
  {
    TrileInstance trileInstance;
    return !this.levelData.Triles.TryGetValue(id.GetTraversal(ref face), out trileInstance) || trileInstance.Trile.SeeThrough || trileInstance.ForceSeeThrough;
  }

  public bool IsBorderTrile(ref TrileEmplacement id)
  {
    bool flag = false;
    for (int index = 0; index < 6 && !flag; ++index)
    {
      FaceOrientation face = (FaceOrientation) index;
      flag |= this.IsBorderTrileFace(ref id, ref face);
    }
    return flag;
  }

  public bool IsInRange(ref TrileEmplacement id)
  {
    Vector3 size = this.levelData.Size;
    return id.X >= 0 && (double) id.X < (double) size.X && id.Y >= 0 && (double) id.Y < (double) size.Y && id.Z >= 0 && (double) id.Z < (double) size.Z;
  }

  public bool IsInRange(Vector3 position)
  {
    Vector3 size = this.levelData.Size;
    return (double) position.X >= 0.0 && (double) position.X < (double) size.X && (double) position.Y >= 0.0 && (double) position.Y < (double) size.Y && (double) position.Z >= 0.0 && (double) position.Z < (double) size.Z;
  }

  public bool VolumeExists(int id) => this.levelData.Volumes.ContainsKey(id);

  public void SwapTrile(TrileInstance instance, Trile newTrile)
  {
    this.LevelMaterializer.CullInstanceOut(instance);
    this.LevelMaterializer.RemoveInstance(instance);
    instance.TrileId = newTrile.Id;
    instance.RefreshTrile();
    this.LevelMaterializer.AddInstance(instance);
    this.LevelMaterializer.CullInstanceIn(instance);
  }

  public void RestoreTrile(TrileInstance instance)
  {
    if (this.TrileExists(instance.Emplacement))
      return;
    this.LevelMaterializer.AddInstance(instance);
    this.AddInstance(instance.Emplacement, instance);
    this.InvalidateScreenSpaceTile(instance.Emplacement);
    this.TrileRestored(instance);
  }

  public bool ClearTrile(TrileInstance instance) => this.ClearTrile(instance, false);

  public bool ClearTrile(TrileInstance instance, bool skipRecull)
  {
    this.LevelMaterializer.RemoveInstance(instance);
    TrileInstance trileInstance1;
    bool flag1;
    if (this.Triles.TryGetValue(instance.Emplacement, out trileInstance1) && instance != trileInstance1 && trileInstance1.OverlappedTriles != null)
    {
      flag1 = trileInstance1.OverlappedTriles.Remove(instance);
    }
    else
    {
      flag1 = this.Triles.Remove(instance.Emplacement);
      if (flag1 && instance.Overlaps)
        this.RestoreTrile(instance.PopOverlap());
    }
    if (!flag1)
    {
      foreach (TrileInstance trileInstance2 in (IEnumerable<TrileInstance>) this.Triles.Values)
      {
        if (trileInstance2.Overlaps && trileInstance2.OverlappedTriles.Contains(instance))
        {
          flag1 = trileInstance2.OverlappedTriles.Remove(instance);
          if (flag1)
            break;
        }
      }
    }
    if (!flag1)
    {
      foreach (KeyValuePair<TrileEmplacement, TrileInstance> trile in (IEnumerable<KeyValuePair<TrileEmplacement, TrileInstance>>) this.Triles)
      {
        if (trile.Value == instance)
        {
          flag1 = this.Triles.Remove(trile.Key);
          if (flag1)
            break;
        }
      }
    }
    bool flag2 = false;
    foreach (TrileGroup trileGroup in (IEnumerable<TrileGroup>) this.Groups.Values)
      flag2 |= trileGroup.Triles.Remove(instance);
    if (flag2)
    {
      foreach (int key in this.Groups.Keys.ToArray<int>())
      {
        if (this.Groups[key].Triles.Count == 0)
          this.Groups.Remove(key);
      }
    }
    if (!skipRecull)
    {
      this.LevelMaterializer.CullInstanceOut(instance, true);
      this.RecullAt(instance);
    }
    instance.Removed = true;
    return flag1;
  }

  public bool ClearTrile(TrileEmplacement emplacement)
  {
    TrileInstance trileInstance;
    if (!this.Triles.TryGetValue(emplacement, out trileInstance))
      return false;
    this.LevelMaterializer.RemoveInstance(trileInstance);
    bool flag1 = this.Triles.Remove(emplacement);
    trileInstance.Removed = true;
    this.LevelMaterializer.CullInstanceOut(trileInstance, true);
    bool flag2 = false;
    foreach (TrileGroup trileGroup in (IEnumerable<TrileGroup>) this.Groups.Values)
      flag2 |= trileGroup.Triles.Remove(trileInstance);
    if (flag2)
    {
      foreach (int key in this.Groups.Keys.ToArray<int>())
      {
        if (this.Groups[key].Triles.Count == 0)
          this.Groups.Remove(key);
      }
    }
    if (flag1 && trileInstance.Overlaps)
      this.RestoreTrile(trileInstance.PopOverlap());
    return true;
  }

  public void RecullAt(TrileInstance instance) => this.RecullAt(instance.Emplacement);

  public void RecullAt(TrileEmplacement emplacement)
  {
    Viewpoint viewpoint = this.CameraManager.Viewpoint;
    if (!viewpoint.IsOrthographic())
      return;
    this.RecullAt(new Point(viewpoint.SideMask() == Vector3.Right ? emplacement.X : emplacement.Z, emplacement.Y), false);
  }

  public void RecullAt(Point ssPos, bool skipCommit)
  {
    this.WaitForScreenInvalidation();
    this.InvalidateScreenSpaceTile(ssPos);
    this.LevelMaterializer.FreeScreenSpace(ssPos.X, ssPos.Y);
    this.LevelMaterializer.FillScreenSpace(ssPos.X, ssPos.Y);
    this.LevelMaterializer.CommitBatchesIfNeeded();
  }

  protected void OnLevelChanged() => this.LevelChanged();

  protected void OnLevelChanging()
  {
    this.LevelChanging();
    this.OnLightingChanged();
  }

  protected void OnSkyChanged() => this.SkyChanged();

  protected virtual void OnLightingChanged() => this.LightingChanged();

  public void AddPlane(BackgroundPlane plane)
  {
    lock (this.BackgroundPlanes)
    {
      int key = IdentifierPool.FirstAvailable<BackgroundPlane>(this.BackgroundPlanes);
      plane.Id = key;
      this.BackgroundPlanes.Add(key, plane);
    }
  }

  public void RemovePlane(BackgroundPlane plane)
  {
    lock (this.BackgroundPlanes)
    {
      this.BackgroundPlanes.Remove(plane.Id);
      plane.Dispose();
    }
  }

  public TrileInstance ActualInstanceAt(Vector3 position)
  {
    Vector3 vector3 = this.CameraManager.Viewpoint.ForwardVector();
    bool depthIsZ = (double) vector3.Z != 0.0;
    bool flag = depthIsZ;
    int forwardSign = depthIsZ ? (int) vector3.Z : (int) vector3.X;
    Vector3 screenSpacePosition = new Vector3(flag ? position.X : position.Z, position.Y, depthIsZ ? position.Z : position.X);
    TrileEmplacement emplacement = new TrileEmplacement((int) Math.Floor((double) position.X), (int) Math.Floor((double) position.Y), (int) Math.Floor((double) position.Z));
    float num = FezMath.Frac(screenSpacePosition.Z);
    LevelManager.QueryResult queryResult;
    TrileInstance trileInstance = this.OffsetInstanceAt(emplacement, screenSpacePosition, depthIsZ, forwardSign, false, false, QueryOptions.None, out queryResult);
    if (trileInstance != null)
      return trileInstance;
    return (double) num >= 0.5 ? this.OffsetInstanceAt(emplacement.GetOffset(depthIsZ ? 0 : 1, 0, depthIsZ ? 1 : 0), screenSpacePosition, depthIsZ, forwardSign, false, false, QueryOptions.None, out queryResult) : this.OffsetInstanceAt(emplacement.GetOffset(depthIsZ ? 0 : -1, 0, depthIsZ ? -1 : 0), screenSpacePosition, depthIsZ, forwardSign, false, false, QueryOptions.None, out queryResult);
  }

  public NearestTriles NearestTrile(Vector3 position)
  {
    return this.NearestTrile(position, QueryOptions.None);
  }

  public NearestTriles NearestTrile(Vector3 position, QueryOptions options)
  {
    return this.NearestTrile(position, options, new Viewpoint?());
  }

  public NearestTriles NearestTrile(Vector3 position, QueryOptions options, Viewpoint? vp)
  {
    NearestTriles nearestTriles = new NearestTriles();
    int num1 = vp.HasValue ? 1 : 0;
    Viewpoint view = num1 != 0 ? vp.Value : this.CameraManager.Viewpoint;
    if (num1 == 0)
      this.WaitForScreenInvalidation();
    bool flag1 = view == Viewpoint.Front || view == Viewpoint.Back;
    bool flag2 = (options & QueryOptions.Background) == QueryOptions.Background;
    bool simpleTest = (options & QueryOptions.Simple) == QueryOptions.Simple;
    TrileEmplacement emplacement = new TrileEmplacement((int) position.X, (int) position.Y, (int) position.Z);
    Vector3 vector3 = view.ForwardVector();
    int forwardSign = flag1 ? (int) vector3.Z : (int) vector3.X;
    Vector3 screenSpacePosition = new Vector3(flag1 ? position.X : position.Z, position.Y, -1f);
    Point key = !flag1 ? new Point(emplacement.Z, emplacement.Y) : new Point(emplacement.X, emplacement.Y);
    int num2;
    if (num1 != 0)
    {
      forwardSign = flag1 ? (int) vector3.Z : (int) vector3.X;
      if (flag2)
        forwardSign *= -1;
      float num3 = (float) (((flag1 ? (double) this.Size.Z : (double) this.Size.X) - 1.0) / 2.0);
      if (flag1)
        emplacement.Z = (int) ((double) num3 - (double) forwardSign * (double) num3);
      else
        emplacement.X = (int) ((double) num3 - (double) forwardSign * (double) num3);
      num2 = (int) ((double) num3 + (double) forwardSign * (double) num3);
    }
    else if (simpleTest)
    {
      Limit limit;
      if (!this.screenSpaceLimits.TryGetValue(key, out limit))
        return nearestTriles;
      int num4 = flag2 ? limit.End : limit.Start;
      if (flag1)
        emplacement.Z = num4;
      else
        emplacement.X = num4;
      num2 = flag2 ? limit.Start : limit.End;
      if (flag2)
        forwardSign *= -1;
    }
    else
    {
      Limit limit1;
      bool flag3 = this.screenSpaceLimits.TryGetValue(key, out limit1);
      int num5 = (double) FezMath.Frac(screenSpacePosition.X) > 0.5 ? 1 : -1;
      int num6 = (double) FezMath.Frac(screenSpacePosition.Y) > 0.5 ? 1 : -1;
      key.X += num5;
      Limit limit2;
      bool flag4 = this.screenSpaceLimits.TryGetValue(key, out limit2);
      key.X -= num5;
      key.Y += num6;
      Limit limit3;
      bool flag5 = this.screenSpaceLimits.TryGetValue(key, out limit3);
      if (!flag3 && !flag5 && !flag4)
        return nearestTriles;
      Limit limit4;
      if (flag3)
      {
        limit4 = limit1;
        if (!flag4 && !flag5)
          simpleTest = true;
      }
      else
      {
        limit4.Start = forwardSign == 1 ? int.MaxValue : int.MinValue;
        limit4.End = forwardSign == 1 ? int.MinValue : int.MaxValue;
        limit4.NoOffset = true;
      }
      if (flag4)
      {
        limit4.Start = forwardSign == 1 ? Math.Min(limit4.Start, limit2.Start) : Math.Max(limit4.Start, limit2.Start);
        limit4.End = forwardSign == 1 ? Math.Max(limit4.End, limit2.End) : Math.Min(limit4.End, limit2.End);
      }
      if (flag5)
      {
        limit4.Start = forwardSign == 1 ? Math.Min(limit4.Start, limit3.Start) : Math.Max(limit4.Start, limit3.Start);
        limit4.End = forwardSign == 1 ? Math.Max(limit4.End, limit3.End) : Math.Min(limit4.End, limit3.End);
      }
      int num7 = flag2 ? limit4.End : limit4.Start;
      if (flag1)
        emplacement.Z = num7;
      else
        emplacement.X = num7;
      num2 = flag2 ? limit4.Start : limit4.End;
      if (flag2)
        forwardSign *= -1;
    }
    int num8 = num2 + forwardSign;
    bool flag6 = flag1 ? emplacement.Z != num8 : emplacement.X != num8;
    if (flag1)
    {
      for (; flag6; flag6 = emplacement.Z != num8)
      {
        LevelManager.QueryResult nearestQueryResult;
        TrileInstance trileInstance = this.OffsetInstanceAt(ref emplacement, ref screenSpacePosition, true, forwardSign, true, false, simpleTest, options, out nearestQueryResult);
        if (trileInstance != null)
        {
          if (nearestQueryResult == LevelManager.QueryResult.Full)
          {
            nearestTriles.Deep = trileInstance;
            break;
          }
          if (nearestTriles.Surface == null)
            nearestTriles.Surface = trileInstance;
        }
        emplacement.Z += forwardSign;
      }
    }
    else
    {
      for (; flag6; flag6 = emplacement.X != num8)
      {
        LevelManager.QueryResult nearestQueryResult;
        TrileInstance trileInstance = this.OffsetInstanceAt(ref emplacement, ref screenSpacePosition, false, forwardSign, true, false, simpleTest, options, out nearestQueryResult);
        if (trileInstance != null)
        {
          if (nearestQueryResult == LevelManager.QueryResult.Full)
          {
            nearestTriles.Deep = trileInstance;
            break;
          }
          if (nearestTriles.Surface == null)
            nearestTriles.Surface = trileInstance;
        }
        emplacement.X += forwardSign;
      }
    }
    return nearestTriles;
  }

  private TrileInstance OffsetInstanceAt(
    TrileEmplacement emplacement,
    Vector3 screenSpacePosition,
    bool depthIsZ,
    int forwardSign,
    bool useSelector,
    bool keepNearest,
    QueryOptions context,
    out LevelManager.QueryResult queryResult)
  {
    return this.OffsetInstanceAt(ref emplacement, ref screenSpacePosition, depthIsZ, forwardSign, useSelector, keepNearest, false, context, out queryResult);
  }

  private TrileInstance OffsetInstanceAt(
    ref TrileEmplacement emplacement,
    ref Vector3 screenSpacePosition,
    bool depthIsZ,
    int forwardSign,
    bool useSelector,
    bool keepNearest,
    bool simpleTest,
    QueryOptions context,
    out LevelManager.QueryResult nearestQueryResult)
  {
    LevelManager.QueryResult queryResult = LevelManager.QueryResult.Nothing;
    TrileInstance instance1;
    if (this.Triles.TryGetValue(emplacement, out instance1))
      instance1 = this.OffsetInstanceOrOverlapsContain(instance1, screenSpacePosition, depthIsZ, forwardSign, useSelector, context, out queryResult);
    if (simpleTest || instance1 != null && !keepNearest)
    {
      nearestQueryResult = queryResult;
      return instance1;
    }
    TrileInstance nearest = instance1;
    nearestQueryResult = queryResult;
    int num1 = depthIsZ ? 1 : 0;
    int num2 = (double) FezMath.Frac(screenSpacePosition.X) > 0.5 ? 1 : -1;
    TrileEmplacement id = num1 != 0 ? emplacement.GetOffset(num2, 0, 0) : emplacement.GetOffset(0, 0, num2);
    TrileInstance instance2 = this.TrileInstanceAt(ref id);
    if (instance2 != null)
    {
      TrileInstance contender = this.OffsetInstanceOrOverlapsContain(instance2, screenSpacePosition, depthIsZ, forwardSign, useSelector, context, out queryResult);
      if (contender != null)
      {
        nearestQueryResult = queryResult;
        if (!keepNearest)
          return contender;
        nearest = LevelManager.KeepNearestInstance(nearest, contender, depthIsZ, forwardSign);
      }
    }
    int offsetY = (double) FezMath.Frac(screenSpacePosition.Y) > 0.5 ? 1 : -1;
    TrileEmplacement offset = emplacement.GetOffset(0, offsetY, 0);
    TrileInstance instance3 = this.TrileInstanceAt(ref offset);
    if (instance3 != null)
    {
      TrileInstance contender = this.OffsetInstanceOrOverlapsContain(instance3, screenSpacePosition, depthIsZ, forwardSign, useSelector, context, out queryResult);
      if (contender != null)
      {
        nearestQueryResult = queryResult;
        if (!keepNearest)
          return contender;
        nearest = LevelManager.KeepNearestInstance(nearest, contender, depthIsZ, forwardSign);
      }
    }
    offset = id.GetOffset(0, offsetY, 0);
    TrileInstance instance4 = this.TrileInstanceAt(ref offset);
    if (instance4 != null)
    {
      TrileInstance contender = this.OffsetInstanceOrOverlapsContain(instance4, screenSpacePosition, depthIsZ, forwardSign, useSelector, context, out queryResult);
      if (contender != null)
      {
        nearestQueryResult = queryResult;
        if (!keepNearest)
          return contender;
        nearest = LevelManager.KeepNearestInstance(nearest, contender, depthIsZ, forwardSign);
      }
    }
    return nearest;
  }

  private TrileInstance OffsetInstanceOrOverlapsContain(
    TrileInstance instance,
    Vector3 screenSpacePosition,
    bool depthIsZ,
    int forwardSign,
    bool useSelector,
    QueryOptions context,
    out LevelManager.QueryResult queryResult)
  {
    TrileInstance nearest = (TrileInstance) null;
    queryResult = LevelManager.QueryResult.Full;
    LevelManager.QueryResult queryResult1 = LevelManager.QueryResult.Nothing;
    if (LevelManager.OffsetInstanceContains(screenSpacePosition, instance, depthIsZ) && (!useSelector || this.InstanceMaterialForQuery(instance, context, out queryResult)))
    {
      nearest = instance;
      queryResult1 = queryResult;
    }
    if (instance.Overlaps)
    {
      foreach (TrileInstance overlappedTrile in instance.OverlappedTriles)
      {
        if (LevelManager.OffsetInstanceContains(screenSpacePosition, overlappedTrile, depthIsZ) && (!useSelector || this.InstanceMaterialForQuery(overlappedTrile, context, out queryResult)))
        {
          nearest = LevelManager.KeepNearestInstance(nearest, overlappedTrile, depthIsZ, forwardSign);
          queryResult1 = queryResult;
        }
      }
    }
    queryResult = queryResult1;
    return nearest;
  }

  private static bool OffsetInstanceContains(
    Vector3 screenSpacePosition,
    TrileInstance instance,
    bool depthIsZ)
  {
    Vector3 center = instance.Center;
    Vector3 transformedSize = instance.TransformedSize;
    Vector3 vector3_1 = new Vector3(depthIsZ ? center.X : center.Z, center.Y, depthIsZ ? center.Z : center.X);
    Vector3 vector3_2 = new Vector3(depthIsZ ? transformedSize.X / 2f : transformedSize.Z / 2f, transformedSize.Y / 2f, depthIsZ ? transformedSize.Z / 2f : transformedSize.X / 2f);
    if ((double) screenSpacePosition.X <= (double) vector3_1.X - (double) vector3_2.X || (double) screenSpacePosition.X >= (double) vector3_1.X + (double) vector3_2.X || (double) screenSpacePosition.Y < (double) vector3_1.Y - (double) vector3_2.Y || (double) screenSpacePosition.Y >= (double) vector3_1.Y + (double) vector3_2.Y)
      return false;
    if ((double) screenSpacePosition.Z == -1.0)
      return true;
    return (double) screenSpacePosition.Z > (double) vector3_1.Z - (double) vector3_2.Z && (double) screenSpacePosition.Z < (double) vector3_1.Z + (double) vector3_2.Z;
  }

  private bool InstanceMaterialForQuery(
    TrileInstance instance,
    QueryOptions options,
    out LevelManager.QueryResult queryResult)
  {
    Trile trile = instance.Trile;
    CollisionType rotatedFace = instance.GetRotatedFace((options & QueryOptions.Background) == QueryOptions.Background ? this.CameraManager.VisibleOrientation.GetOpposite() : this.CameraManager.VisibleOrientation);
    queryResult = trile.Immaterial || instance.PhysicsState != null && instance.PhysicsState.UpdatingPhysics || rotatedFace == CollisionType.Immaterial ? LevelManager.QueryResult.Nothing : (trile.Thin ? LevelManager.QueryResult.Thin : LevelManager.QueryResult.Full);
    return queryResult != 0;
  }

  private static TrileInstance KeepNearestInstance(
    TrileInstance nearest,
    TrileInstance contender,
    bool depthIsZ,
    int forwardSign)
  {
    Vector3 b = (depthIsZ ? Vector3.UnitZ : Vector3.UnitX) * (float) -forwardSign;
    return (nearest == null ? -3.4028234663852886E+38 : (double) (nearest.Center + nearest.TransformedSize * b / 2f).Dot(b)) <= (double) (contender.Center + contender.TransformedSize * b / 2f).Dot(b) ? contender : nearest;
  }

  public IEnumerable<Trile> ActorTriles(ActorType type)
  {
    return this.TrileSet != null ? this.TrileSet.Triles.Values.Where<Trile>((Func<Trile, bool>) (x => x.ActorSettings.Type == type)) : Enumerable.Repeat<Trile>((Trile) null, 1);
  }

  public IEnumerable<string> LinkedLevels()
  {
    return this.levelData.Scripts.Values.Select<Script, IEnumerable<string>>((Func<Script, IEnumerable<string>>) (script => script.Actions.Where<ScriptAction>((Func<ScriptAction, bool>) (action => action.Object.Type == "Level")).Select<ScriptAction, string>((Func<ScriptAction, string>) (action => ((IEnumerable<string>) action.Arguments).FirstOrDefault<string>())))).SelectMany<IEnumerable<string>, string>((Func<IEnumerable<string>, IEnumerable<string>>) (x => x));
  }

  public void UpdateInstance(TrileInstance instance)
  {
    if (instance.LastUpdatePosition.Round() != instance.Position.Round())
    {
      TrileEmplacement trileEmplacement = new TrileEmplacement(instance.LastUpdatePosition);
      TrileInstance trileInstance;
      TrileInstance instance1;
      if (this.Triles.TryGetValue(trileEmplacement, out trileInstance))
      {
        if (trileInstance == instance)
        {
          this.Triles.Remove(trileEmplacement);
          if (instance.Overlaps)
          {
            instance1 = instance.PopOverlap();
            this.AddInstance(trileEmplacement, instance1);
          }
        }
        else if (trileInstance.Overlaps && trileInstance.OverlappedTriles.Contains(instance))
          trileInstance.OverlappedTriles.Remove(instance);
      }
      if (this.Triles.TryGetValue(instance.Emplacement, out instance1))
      {
        instance.PushOverlap(instance1);
        this.Triles.Remove(instance.Emplacement);
      }
      this.LevelMaterializer.UpdateInstance(instance);
      this.Triles.Add(instance.Emplacement, instance);
      instance.Update();
      this.LevelMaterializer.UpdateRow(trileEmplacement, instance);
      if (!this.IsInvalidatingScreen)
      {
        this.InvalidateScreenSpaceTile(trileEmplacement);
        this.InvalidateScreenSpaceTile(instance.Emplacement);
      }
    }
    if (instance.InstanceId == -1)
      return;
    this.LevelMaterializer.GetTrileMaterializer(instance.VisualTrile).UpdateInstance(instance);
  }

  public Dictionary<Point, Limit> ScreenSpaceLimits => this.screenSpaceLimits;

  public bool SkipInvalidation { get; set; }

  public override void Update(GameTime gameTime)
  {
    if (this.screenInvalidationWorker != null || this.ScreenInvalidated == null || this.EngineState.Loading)
      return;
    if (this.SkipInvalidation)
    {
      this.ScreenInvalidated = (Action) null;
    }
    else
    {
      this.ScreenInvalidated();
      this.ScreenInvalidated = (Action) null;
    }
  }

  public void WaitForScreenInvalidation()
  {
    while (this.screenInvalidationWorker != null)
      Thread.Sleep(0);
  }

  public void AbortInvalidation()
  {
    if (this.screenInvalidationWorker == null)
      return;
    this.screenInvalidationWorker.Abort();
  }

  private void InvalidateScreenSpaceTile(TrileEmplacement emplacement)
  {
    Vector3 b = this.CameraManager.Viewpoint.SideMask();
    this.InvalidateScreenSpaceTile(new Point((int) emplacement.AsVector.Dot(b), emplacement.Y));
  }

  private void InvalidateScreenSpaceTile(Point ssPos)
  {
    this.WaitForScreenInvalidation();
    this.screenSpaceLimits.Remove(ssPos);
    this.FillScreenSpaceTile(ssPos, (IDictionary<Point, Limit>) this.screenSpaceLimits);
  }

  public bool IsInvalidatingScreen => this.screenInvalidationWorker != null;

  private void InvalidateScreen()
  {
    if (this.SkipInvalidation)
      return;
    lock (LevelManager.Mutex)
    {
      if (this.screenInvalidationWorker != null)
        this.screenInvalidationWorker.Abort();
    }
    this.WaitForScreenInvalidation();
    Dictionary<Point, Limit> newLimits = new Dictionary<Point, Limit>(this.screenSpaceLimits.Count, (IEqualityComparer<Point>) LevelManager.FastPointComparer.Default);
    this.screenInvalidationWorker = this.ThreadPool.Take<Dictionary<Point, Limit>>(new Action<Dictionary<Point, Limit>>(this.DoInvalidateScreen));
    this.screenInvalidationWorker.Priority = ThreadPriority.Normal;
    this.screenInvalidationWorker.Finished += (Action) (() =>
    {
      lock (LevelManager.Mutex)
      {
        if (this.screenInvalidationWorker == null)
          return;
        if (!this.screenInvalidationWorker.Aborted)
          this.screenSpaceLimits = newLimits;
        this.ThreadPool.Return<Dictionary<Point, Limit>>(this.screenInvalidationWorker);
        this.screenInvalidationWorker = (Worker<Dictionary<Point, Limit>>) null;
      }
    });
    this.screenInvalidationWorker.Start(newLimits);
  }

  private void DoInvalidateScreen(Dictionary<Point, Limit> newLimits)
  {
    float num = this.Size.Dot(this.CameraManager.Viewpoint.SideMask());
    for (int x = 0; (double) x < (double) num; ++x)
    {
      for (int y = 0; (double) y < (double) this.Size.Y; ++y)
      {
        if (this.screenInvalidationWorker.Aborted)
          return;
        this.FillScreenSpaceTile(new Point(x, y), (IDictionary<Point, Limit>) newLimits);
      }
    }
  }

  private void FillScreenSpaceTile(Point p, IDictionary<Point, Limit> newLimits)
  {
    Vector3 vector3 = this.CameraManager.Viewpoint.ForwardVector();
    bool flag1 = (double) vector3.Z != 0.0;
    bool flag2 = flag1;
    int num1 = flag1 ? (int) vector3.Z : (int) vector3.X;
    float num2 = (float) (((flag1 ? (double) this.Size.Z : (double) this.Size.X) - 1.0) / 2.0);
    int num3 = (int) ((double) num2 + (double) num1 * (double) num2) + num1;
    Limit limit = new Limit()
    {
      Start = (int) ((double) num2 - (double) num1 * (double) num2),
      End = num3,
      NoOffset = true
    };
    TrileEmplacement key = new TrileEmplacement(flag2 ? p.X : limit.Start, p.Y, flag1 ? limit.Start : p.X);
    bool flag3 = true;
    bool flag4 = false;
    while (flag3)
    {
      TrileInstance trileInstance;
      if (this.Triles.TryGetValue(key, out trileInstance))
      {
        limit.NoOffset &= trileInstance.Position == trileInstance.Emplacement.AsVector;
        int num4 = flag1 ? key.Z : key.X;
        if (!flag4)
        {
          limit.Start = num4;
          flag4 = true;
        }
        limit.End = num4;
      }
      if (flag1)
      {
        key.Z += num1;
        flag3 = key.Z != num3;
      }
      else
      {
        key.X += num1;
        flag3 = key.X != num3;
      }
    }
    if (limit.End == num3)
      return;
    if (newLimits.ContainsKey(p))
      newLimits.Remove(p);
    newLimits.Add(p, limit);
  }

  public void PrepareFullCull() => this.LevelMaterializer.PrepareFullCull();

  public abstract bool WasPathSupposedToBeRecorded(int id);

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { protected get; set; }

  [ServiceDependency]
  public IFogManager FogManager { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { protected get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { protected get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { protected get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { protected get; set; }

  private enum QueryResult
  {
    Nothing,
    Thin,
    Full,
  }

  private class FastPointComparer : IEqualityComparer<Point>
  {
    public static readonly LevelManager.FastPointComparer Default = new LevelManager.FastPointComparer();

    public bool Equals(Point x, Point y) => x.X == y.X && x.Y == y.Y;

    public int GetHashCode(Point obj) => obj.X | obj.Y << 16 /*0x10*/;
  }
}
