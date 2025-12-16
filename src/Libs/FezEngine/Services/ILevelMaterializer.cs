// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.ILevelMaterializer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using FezEngine.Structure;
using FezEngine.Tools;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public interface ILevelMaterializer
{
  event Action<TrileInstance> TrileInstanceBatched;

  void CullEverything();

  void InitializeArtObjects();

  void DestroyMaterializers(TrileSet trileSet);

  void RebuildTriles(bool quick);

  void RebuildTriles(TrileSet trileSet, bool quick);

  void RebuildTrile(Trile trile);

  void ClearBatches();

  void AddInstance(TrileInstance instance);

  void RemoveInstance(TrileInstance trileInstance);

  void RebuildInstances();

  void CullInstances();

  void CleanUp();

  void Rowify();

  void UnRowify();

  void UpdateRow(TrileEmplacement oldEmplacement, TrileInstance instance);

  void FreeScreenSpace(int i, int j);

  void FillScreenSpace(int i, int j);

  void CommitBatchesIfNeeded();

  bool CullInstanceOut(TrileInstance toRemove);

  bool CullInstanceOut(TrileInstance toRemove, bool skipUnregister);

  void CullInstanceIn(TrileInstance toAdd);

  void CullInstanceIn(TrileInstance instance, bool forceAdd);

  void CullInstanceInNoRegister(TrileInstance instance);

  void UpdateInstance(TrileInstance instance);

  bool UnregisterViewedInstance(TrileInstance instance);

  Mesh TrilesMesh { get; }

  Mesh ArtObjectsMesh { get; }

  Mesh StaticPlanesMesh { get; }

  Mesh AnimatedPlanesMesh { get; }

  Mesh NpcMesh { get; }

  TrileEffect TrilesEffect { get; }

  InstancedArtObjectEffect ArtObjectsEffect { get; }

  TrileMaterializer GetTrileMaterializer(Trile trile);

  IEnumerable<Trile> MaterializedTriles { get; }

  List<BackgroundPlane> LevelPlanes { get; }

  List<ArtObjectInstance> LevelArtObjects { get; }

  void RegisterSatellites();

  void PrepareFullCull();

  RenderPass RenderPass { get; set; }

  void ForceCull();

  void CleanFallbackTriles();
}
