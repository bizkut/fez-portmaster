// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.GroupService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Services.Scripting;

internal class GroupService : IGroupService, IScriptingBase
{
  public void MovePathToEnd(int id) => this.LevelManager.Groups[id].MoveToEnd = true;

  public void StartPath(int id, bool backwards)
  {
    MovementPath path = this.LevelManager.Groups[id].Path;
    path.Backwards = backwards;
    path.NeedsTrigger = false;
    if ((!path.SaveTrigger ? 0 : (this.LevelManager.IsPathRecorded(id) ? 1 : 0)) != 0 || !path.SaveTrigger)
      return;
    this.LevelManager.RecordMoveToEnd(id);
  }

  public void RunPathOnce(int id, bool backwards)
  {
    MovementPath path = this.LevelManager.Groups[id].Path;
    path.Backwards = backwards;
    path.NeedsTrigger = false;
    path.RunOnce = true;
    if ((!path.SaveTrigger ? 0 : (this.LevelManager.IsPathRecorded(id) ? 1 : 0)) != 0 || !path.SaveTrigger)
      return;
    this.LevelManager.RecordMoveToEnd(id);
  }

  public void RunSingleSegment(int id, bool backwards)
  {
    this.LevelManager.Groups[id].Path.Backwards = backwards;
    this.LevelManager.Groups[id].Path.NeedsTrigger = false;
    this.LevelManager.Groups[id].Path.RunSingleSegment = true;
  }

  public void Stop(int id) => this.LevelManager.Groups[id].Path.NeedsTrigger = true;

  public void SetEnabled(int id, bool enabled)
  {
    foreach (TrileInstance trile in this.LevelManager.Groups[id].Triles)
      trile.Enabled = enabled;
    this.LevelMaterializer.CullInstances();
  }

  public void GlitchyDespawn(int id, bool permanent)
  {
    foreach (TrileInstance trile in this.LevelManager.Groups[id].Triles)
    {
      if (permanent)
        this.GameState.SaveData.ThisLevel.DestroyedTriles.Add(trile.OriginalEmplacement);
      ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(ServiceHelper.Game, trile));
    }
  }

  public LongRunningAction Move(int id, float dX, float dY, float dZ)
  {
    TrileGroup group = this.LevelManager.Groups[id];
    group.Triles.Sort((IComparer<TrileInstance>) new MovingTrileInstanceComparer(new Vector3(dX, dY, dZ)));
    foreach (TrileInstance trile in group.Triles)
    {
      if (trile.PhysicsState == null)
        trile.PhysicsState = new InstancePhysicsState(trile);
    }
    List<ArtObjectInstance> attachedAos = new List<ArtObjectInstance>();
    foreach (ArtObjectInstance artObjectInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      int? attachedGroup = artObjectInstance.ActorSettings.AttachedGroup;
      int num = id;
      if ((attachedGroup.GetValueOrDefault() == num ? (attachedGroup.HasValue ? 1 : 0) : 0) != 0)
        attachedAos.Add(artObjectInstance);
    }
    Vector3 velocity = new Vector3(dX, dY, dZ);
    return new LongRunningAction((Func<float, float, bool>) ((elapsedSeconds, _) =>
    {
      foreach (TrileInstance trile in group.Triles)
      {
        trile.PhysicsState.Velocity = velocity * elapsedSeconds;
        trile.Position += velocity * elapsedSeconds;
        this.LevelManager.UpdateInstance(trile);
      }
      foreach (ArtObjectInstance artObjectInstance in attachedAos)
        artObjectInstance.Position += velocity * elapsedSeconds;
      return false;
    }));
  }

  public void ResetEvents()
  {
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }
}
