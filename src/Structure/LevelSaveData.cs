// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.LevelSaveData
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Structure;

public class LevelSaveData
{
  public static readonly LevelSaveData Default = new LevelSaveData();
  public List<TrileEmplacement> DestroyedTriles = new List<TrileEmplacement>();
  public List<TrileEmplacement> InactiveTriles = new List<TrileEmplacement>();
  public List<int> InactiveArtObjects = new List<int>();
  public List<int> InactiveEvents = new List<int>();
  public List<int> InactiveGroups = new List<int>();
  public List<int> InactiveVolumes = new List<int>();
  public List<int> InactiveNPCs = new List<int>();
  public Dictionary<int, int> PivotRotations = new Dictionary<int, int>();
  public float? LastStableLiquidHeight;
  public string ScriptingState;
  public WinConditions FilledConditions = new WinConditions();
  public bool FirstVisit;

  public void CloneInto(LevelSaveData d)
  {
    this.FilledConditions.CloneInto(d.FilledConditions);
    d.FirstVisit = this.FirstVisit;
    d.LastStableLiquidHeight = this.LastStableLiquidHeight;
    d.ScriptingState = this.ScriptingState;
    d.DestroyedTriles.Clear();
    d.DestroyedTriles.AddRange((IEnumerable<TrileEmplacement>) this.DestroyedTriles);
    d.InactiveArtObjects.Clear();
    d.InactiveArtObjects.AddRange((IEnumerable<int>) this.InactiveArtObjects);
    d.InactiveEvents.Clear();
    d.InactiveEvents.AddRange((IEnumerable<int>) this.InactiveEvents);
    d.InactiveGroups.Clear();
    d.InactiveGroups.AddRange((IEnumerable<int>) this.InactiveGroups);
    d.InactiveNPCs.Clear();
    d.InactiveNPCs.AddRange((IEnumerable<int>) this.InactiveNPCs);
    d.InactiveTriles.Clear();
    d.InactiveTriles.AddRange((IEnumerable<TrileEmplacement>) this.InactiveTriles);
    d.InactiveVolumes.Clear();
    d.InactiveVolumes.AddRange((IEnumerable<int>) this.InactiveVolumes);
    d.PivotRotations.Clear();
    foreach (int key in this.PivotRotations.Keys)
      d.PivotRotations.Add(key, this.PivotRotations[key]);
  }
}
