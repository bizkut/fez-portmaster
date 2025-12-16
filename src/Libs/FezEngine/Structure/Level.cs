// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Level
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization;
using ContentSerialization.Attributes;
using FezEngine.Structure.Scripting;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class Level : IDeserializationCallback
{
  [Serialization(CollectionItemName = "Trile")]
  public Dictionary<TrileEmplacement, TrileInstance> Triles;

  public Level()
  {
    this.Triles = new Dictionary<TrileEmplacement, TrileInstance>();
    this.Volumes = new Dictionary<int, Volume>();
    this.ArtObjects = new Dictionary<int, ArtObjectInstance>();
    this.BackgroundPlanes = new Dictionary<int, BackgroundPlane>();
    this.Groups = new Dictionary<int, TrileGroup>();
    this.Scripts = new Dictionary<int, Script>();
    this.NonPlayerCharacters = new Dictionary<int, NpcInstance>();
    this.Paths = new Dictionary<int, MovementPath>();
    this.MutedLoops = new List<string>();
    this.AmbienceTracks = new List<AmbienceTrack>();
    this.BaseDiffuse = 1f;
    this.BaseAmbient = 0.35f;
    this.HaloFiltering = true;
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Flat { get; set; }

  public string Name { get; set; }

  public TrileFace StartingPosition { get; set; }

  public Vector3 Size { get; set; }

  [Serialization(Optional = true)]
  public string SequenceSamplesPath { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool SkipPostProcess { get; set; }

  [Serialization(Optional = true)]
  public float BaseDiffuse { get; set; }

  [Serialization(Optional = true)]
  public float BaseAmbient { get; set; }

  [Serialization(Optional = true)]
  public string GomezHaloName { get; set; }

  [Serialization(Optional = true)]
  public bool HaloFiltering { get; set; }

  [Serialization(Optional = true)]
  public bool BlinkingAlpha { get; set; }

  [Serialization(Optional = true)]
  public bool Loops { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public LiquidType WaterType { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public float WaterHeight { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Descending { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Rainy { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool LowPass { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public LevelNodeType NodeType { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int FAPFadeOutStart { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int FAPFadeOutLength { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Quantum { get; set; }

  public string SkyName { get; set; }

  public string TrileSetName { get; set; }

  [Serialization(Optional = true)]
  public string SongName { get; set; }

  [Serialization(Ignore = true)]
  public Sky Sky { get; set; }

  [Serialization(Ignore = true)]
  public TrileSet TrileSet { get; set; }

  [Serialization(Ignore = true)]
  public TrackedSong Song { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public Dictionary<int, Volume> Volumes { get; set; }

  [Serialization(CollectionItemName = "Object", Optional = true, DefaultValueOptional = true)]
  public Dictionary<int, ArtObjectInstance> ArtObjects { get; set; }

  [Serialization(CollectionItemName = "Plane", Optional = true, DefaultValueOptional = true)]
  public Dictionary<int, BackgroundPlane> BackgroundPlanes { get; set; }

  [Serialization(CollectionItemName = "Script", Optional = true, DefaultValueOptional = true)]
  public Dictionary<int, Script> Scripts { get; set; }

  [Serialization(CollectionItemName = "Group", Optional = true, DefaultValueOptional = true)]
  public Dictionary<int, TrileGroup> Groups { get; set; }

  [Serialization(CollectionItemName = "Npc", Optional = true, DefaultValueOptional = true)]
  public Dictionary<int, NpcInstance> NonPlayerCharacters { get; set; }

  [Serialization(CollectionItemName = "Path", Optional = true, DefaultValueOptional = true)]
  public Dictionary<int, MovementPath> Paths { get; set; }

  [Serialization(CollectionItemName = "Loop", Optional = true, DefaultValueOptional = true)]
  public List<string> MutedLoops { get; set; }

  [Serialization(CollectionItemName = "Track", Optional = true, DefaultValueOptional = true)]
  public List<AmbienceTrack> AmbienceTracks { get; set; }

  public void OnDeserialization()
  {
    foreach (TrileEmplacement key in this.Triles.Keys)
    {
      TrileInstance trile = this.Triles[key];
      if (this.Triles[key].Emplacement != key)
        this.Triles[key].Emplacement = key;
      trile.Update();
      trile.OriginalEmplacement = key;
      if (trile.Overlaps)
      {
        foreach (TrileInstance overlappedTrile in trile.OverlappedTriles)
          overlappedTrile.OriginalEmplacement = key;
      }
    }
    foreach (int key in this.Scripts.Keys)
      this.Scripts[key].Id = key;
    foreach (int key in this.Volumes.Keys)
      this.Volumes[key].Id = key;
    foreach (int key in this.NonPlayerCharacters.Keys)
      this.NonPlayerCharacters[key].Id = key;
    foreach (int key in this.ArtObjects.Keys)
      this.ArtObjects[key].Id = key;
    foreach (int key in this.BackgroundPlanes.Keys)
      this.BackgroundPlanes[key].Id = key;
    foreach (int key in this.Paths.Keys)
      this.Paths[key].Id = key;
    foreach (int key1 in this.Groups.Keys)
    {
      TrileGroup group = this.Groups[key1];
      group.Id = key1;
      TrileEmplacement[] trileEmplacementArray = new TrileEmplacement[group.Triles.Count];
      for (int index = 0; index < trileEmplacementArray.Length; ++index)
        trileEmplacementArray[index] = group.Triles[index].Emplacement;
      group.Triles.Clear();
      foreach (TrileEmplacement key2 in trileEmplacementArray)
        group.Triles.Add(this.Triles[key2]);
    }
  }
}
