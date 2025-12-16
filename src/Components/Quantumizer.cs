// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Quantumizer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class Quantumizer : GameComponent
{
  private readonly List<TrileInstance> BatchedInstances = new List<TrileInstance>();
  private readonly List<TrileInstance> RandomInstances = new List<TrileInstance>();
  private readonly List<TrileInstance> CleanInstances = new List<TrileInstance>();
  private readonly List<Vector4> AllEmplacements = new List<Vector4>();
  private static readonly Random Random = new Random();
  private int[] RandomTrileIds;
  private int FreezeFrames;
  private readonly HashSet<Point> SsPosToRecull = new HashSet<Point>();

  public Quantumizer(Game game)
    : base(game)
  {
    this.UpdateOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.BatchedInstances.Clear();
    this.CleanInstances.Clear();
    this.AllEmplacements.Clear();
    this.AllEmplacements.TrimExcess();
    this.CleanInstances.TrimExcess();
    this.BatchedInstances.TrimExcess();
    this.RandomTrileIds = (int[]) null;
    if (this.Enabled)
      this.LevelMaterializer.TrileInstanceBatched -= new Action<TrileInstance>(this.BatchInstance);
    this.Enabled = false;
    if (!this.LevelManager.Quantum || this.LevelManager.TrileSet == null)
      return;
    this.Enabled = true;
    List<int> list = this.LevelMaterializer.MaterializedTriles.Where<Trile>((Func<Trile, bool>) (x => x.Geometry != null && !x.Geometry.Empty && !x.ActorSettings.Type.IsTreasure() && x.ActorSettings.Type != ActorType.SplitUpCube)).Select<Trile, int>((Func<Trile, int>) (x => x.Id)).ToList<int>();
    this.RandomTrileIds = new int[250];
    int num = 0;
    for (int index1 = 0; index1 < 250; ++index1)
    {
      int index2 = Quantumizer.Random.Next(0, list.Count);
      int id = list[index2];
      this.RandomTrileIds[num++] = id;
      this.LevelManager.TrileSet[id].ForceKeep = true;
      list.RemoveAt(index2);
    }
    Trile trile = this.LevelManager.TrileSet.Triles.Values.FirstOrDefault<Trile>((Func<Trile, bool>) (x => x.Name == "__QIPT"));
    if (trile == null)
    {
      trile = new Trile(CollisionType.None)
      {
        Name = "__QIPT",
        Immaterial = true,
        SeeThrough = true,
        Thin = true,
        TrileSet = this.LevelManager.TrileSet,
        MissingTrixels = (TrixelCluster) null,
        Id = IdentifierPool.FirstAvailable<Trile>((IDictionary<int, Trile>) this.LevelManager.TrileSet.Triles)
      };
      this.LevelManager.TrileSet.Triles.Add(trile.Id, trile);
      this.LevelMaterializer.RebuildTrile(trile);
    }
    List<int> intList = new List<int>();
    bool flag = (double) this.LevelManager.Size.X > (double) this.LevelManager.Size.Z;
    float[] numArray = new float[4]
    {
      0.0f,
      1.57079637f,
      3.14159274f,
      4.712389f
    };
    for (int y = 0; (double) y < (double) this.LevelManager.Size.Y; ++y)
    {
      if (flag)
      {
        intList.Clear();
        intList.AddRange(Enumerable.Range(0, (int) this.LevelManager.Size.Z));
        for (int x = 0; (double) x < (double) this.LevelManager.Size.X; ++x)
        {
          int z;
          if (intList.Count > 0)
          {
            int index = RandomHelper.Random.Next(0, intList.Count);
            z = intList[index];
            intList.RemoveAt(index);
          }
          else
            z = RandomHelper.Random.Next(0, (int) this.LevelManager.Size.Z);
          this.LevelManager.RestoreTrile(new TrileInstance(new TrileEmplacement(x, y, z), trile.Id)
          {
            Phi = numArray[Quantumizer.Random.Next(0, 4)]
          });
        }
        while (intList.Count > 0)
        {
          int index = RandomHelper.Random.Next(0, intList.Count);
          int z = intList[index];
          intList.RemoveAt(index);
          this.LevelManager.RestoreTrile(new TrileInstance(new TrileEmplacement(RandomHelper.Random.Next(0, (int) this.LevelManager.Size.X), y, z), trile.Id)
          {
            Phi = numArray[Quantumizer.Random.Next(0, 4)]
          });
        }
      }
      else
      {
        intList.Clear();
        intList.AddRange(Enumerable.Range(0, (int) this.LevelManager.Size.X));
        for (int z = 0; (double) z < (double) this.LevelManager.Size.Z; ++z)
        {
          int x;
          if (intList.Count > 0)
          {
            int index = RandomHelper.Random.Next(0, intList.Count);
            x = intList[index];
            intList.RemoveAt(index);
          }
          else
            x = RandomHelper.Random.Next(0, (int) this.LevelManager.Size.X);
          this.LevelManager.RestoreTrile(new TrileInstance(new TrileEmplacement(x, y, z), trile.Id)
          {
            Phi = numArray[Quantumizer.Random.Next(0, 4)]
          });
        }
        while (intList.Count > 0)
        {
          int index = RandomHelper.Random.Next(0, intList.Count);
          int x = intList[index];
          intList.RemoveAt(index);
          this.LevelManager.RestoreTrile(new TrileInstance(new TrileEmplacement(x, y, RandomHelper.Random.Next(0, (int) this.LevelManager.Size.Z)), trile.Id)
          {
            Phi = numArray[Quantumizer.Random.Next(0, 4)]
          });
        }
      }
    }
    foreach (TrileInstance trileInstance in (IEnumerable<TrileInstance>) this.LevelManager.Triles.Values)
    {
      trileInstance.VisualTrileId = new int?(this.RandomTrileIds[Quantumizer.Random.Next(0, this.RandomTrileIds.Length)]);
      trileInstance.RefreshTrile();
      trileInstance.NeedsRandomCleanup = true;
      trileInstance.RandomTracked = false;
    }
    this.LevelMaterializer.CleanUp();
    this.LevelMaterializer.TrileInstanceBatched += new Action<TrileInstance>(this.BatchInstance);
  }

  private void BatchInstance(TrileInstance instance)
  {
    if (instance.RandomTracked)
      return;
    this.BatchedInstances.Add(instance);
    instance.RandomTracked = true;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || this.GameState.InFpsMode || this.GameState.InMenuCube || !this.CameraManager.Viewpoint.IsOrthographic())
      return;
    int viewpoint = (int) this.CameraManager.Viewpoint;
    Vector3 vector3_1 = ((Viewpoint) viewpoint).ScreenSpaceMask();
    Vector3 vector3_2 = Vector3.One - vector3_1 + Vector3.UnitY;
    bool flag1 = ((Viewpoint) viewpoint).SideMask() == Vector3.Right;
    Vector3 position = this.PlayerManager.Position;
    bool transitionReached = this.CameraManager.ViewTransitionReached;
    if (this.CameraManager.ProjectionTransitionNewlyReached)
      this.LevelMaterializer.CullInstances();
    this.RandomInstances.Clear();
    this.CleanInstances.Clear();
    this.AllEmplacements.Clear();
    for (int index = this.BatchedInstances.Count - 1; index >= 0; --index)
    {
      TrileInstance batchedInstance = this.BatchedInstances[index];
      if (batchedInstance.InstanceId == -1)
      {
        this.BatchedInstances.RemoveAt(index);
        batchedInstance.RandomTracked = false;
      }
      else
      {
        Vector4 positionPhi = batchedInstance.Data.PositionPhi;
        Vector3 vector3_3 = position - new Vector3(positionPhi.X, positionPhi.Y, positionPhi.Z);
        if ((double) (vector3_3 * vector3_1).LengthSquared() > 30.0 && (transitionReached ? 1 : ((double) (vector3_3 * vector3_2).LengthSquared() > 30.0 ? 1 : 0)) != 0)
        {
          if (transitionReached)
          {
            this.AllEmplacements.Add(positionPhi);
            this.RandomInstances.Add(batchedInstance);
          }
        }
        else
          this.CleanInstances.Add(batchedInstance);
      }
    }
    if (this.BatchedInstances.Count == 0)
      return;
    bool flag2 = false;
    if (this.FreezeFrames-- < 0)
    {
      if (RandomHelper.Probability(0.019999999552965164))
        this.FreezeFrames = Quantumizer.Random.Next(0, 15);
    }
    else
      flag2 = true;
    int? nullable1;
    if (RandomHelper.Probability(0.89999997615814209) & transitionReached)
    {
      int num = Quantumizer.Random.Next(0, flag2 ? this.RandomInstances.Count / 50 : this.RandomInstances.Count);
      while (num-- >= 0 && this.RandomInstances.Count > 0)
      {
        int count = this.RandomInstances.Count;
        int index1 = Quantumizer.Random.Next(0, count);
        TrileInstance randomInstance = this.RandomInstances[index1];
        this.RandomInstances.RemoveAt(index1);
        nullable1 = randomInstance.VisualTrileId;
        if (nullable1.HasValue)
        {
          int trileId = randomInstance.TrileId;
          nullable1 = randomInstance.VisualTrileId;
          int valueOrDefault = nullable1.GetValueOrDefault();
          if ((trileId == valueOrDefault ? (nullable1.HasValue ? 1 : 0) : 0) == 0)
            goto label_25;
        }
        if (!this.LevelMaterializer.CullInstanceOut(randomInstance))
          this.LevelMaterializer.CullInstanceOut(randomInstance, true);
        randomInstance.VisualTrileId = new int?(RandomHelper.InList<int>(this.RandomTrileIds));
        randomInstance.RefreshTrile();
        this.LevelMaterializer.CullInstanceIn(randomInstance, true);
label_25:
        randomInstance.NeedsRandomCleanup = true;
        if (randomInstance.InstanceId != -1)
        {
          int index2 = Quantumizer.Random.Next(0, count);
          Vector4 allEmplacement = this.AllEmplacements[index2];
          this.AllEmplacements.RemoveAt(index2);
          this.LevelMaterializer.GetTrileMaterializer(randomInstance.VisualTrile).FakeUpdate(randomInstance.InstanceId, allEmplacement);
        }
      }
    }
    this.SsPosToRecull.Clear();
    foreach (TrileInstance cleanInstance in this.CleanInstances)
    {
      nullable1 = cleanInstance.VisualTrileId;
      if (nullable1.HasValue)
      {
        if (!this.LevelMaterializer.CullInstanceOut(cleanInstance))
          this.LevelMaterializer.CullInstanceOut(cleanInstance, true);
        TrileInstance trileInstance = cleanInstance;
        nullable1 = new int?();
        int? nullable2 = nullable1;
        trileInstance.VisualTrileId = nullable2;
        cleanInstance.RefreshTrile();
        if (transitionReached)
        {
          TrileEmplacement emplacement = cleanInstance.Emplacement;
          this.SsPosToRecull.Add(new Point(flag1 ? emplacement.X : emplacement.Z, emplacement.Y));
        }
        else
          this.LevelMaterializer.CullInstanceIn(cleanInstance, true);
      }
      else if (cleanInstance.NeedsRandomCleanup)
      {
        this.LevelMaterializer.GetTrileMaterializer(cleanInstance.Trile).UpdateInstance(cleanInstance);
        cleanInstance.NeedsRandomCleanup = false;
      }
    }
    if (this.SsPosToRecull.Count > 0)
    {
      foreach (Point ssPos in this.SsPosToRecull)
        this.LevelManager.RecullAt(ssPos);
    }
    base.Update(gameTime);
  }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }
}
