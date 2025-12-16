// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.MapNode
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization;
using ContentSerialization.Attributes;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure;

public class MapNode
{
  private static readonly string[] UpLevels = new string[7]
  {
    "TREE_ROOTS",
    "TREE",
    "TREE_SKY",
    "FOX",
    "WATER_TOWER",
    "PIVOT_WATERTOWER",
    "VILLAGEVILLE_3D"
  };
  private static readonly string[] DownLevels = new string[5]
  {
    "SEWER_START",
    "MEMORY_CORE",
    "ZU_FORK",
    "STARGATE",
    "QUANTUM"
  };
  private static readonly string[] OppositeLevels = new string[15]
  {
    "NUZU_SCHOOL",
    "NUZU_ABANDONED_A",
    "ZU_HOUSE_EMPTY_B",
    "PURPLE_LODGE",
    "ZU_HOUSE_SCAFFOLDING",
    "MINE_BOMB_PILLAR",
    "CMY_B",
    "INDUSTRIAL_HUB",
    "SUPERSPIN_CAVE",
    "GRAVE_LESSER_GATE",
    "THRONE",
    "VISITOR",
    "ORRERY",
    "LAVA_SKULL",
    "LAVA_FORK"
  };
  private static readonly string[] BackLevels = new string[2]
  {
    "ABANDONED_B",
    "LAVA"
  };
  private static readonly string[] LeftLevels = new string[0];
  private static readonly string[] FrontLevels = new string[2]
  {
    "VILLAGEVILLE_3D",
    "ZU_LIBRARY"
  };
  private static readonly string[] RightLevels = new string[5]
  {
    "WALL_SCHOOL",
    "WALL_KITCHEN",
    "WALL_INTERIOR_HOLE",
    "WALL_INTERIOR_B",
    "WALL_INTERIOR_A"
  };
  private static readonly string[] PuzzleLevels = new string[5]
  {
    "ZU_ZUISH",
    "ZU_UNFOLD",
    "BELL_TOWER",
    "CLOCK",
    "ZU_TETRIS"
  };
  private static readonly Dictionary<string, float> OversizeLinks = new Dictionary<string, float>()
  {
    {
      "SEWER_START",
      5.5f
    },
    {
      "TREE",
      1.25f
    },
    {
      "TREE_SKY",
      1f
    },
    {
      "INDUSTRIAL_HUB",
      0.5f
    },
    {
      "VILLAGEVILLE_3D",
      -0.5f
    },
    {
      "WALL_VILLAGE",
      0.5f
    },
    {
      "ZU_CITY",
      0.5f
    },
    {
      "INDUSTRIAL_CITY",
      0.5f
    },
    {
      "MEMORY_CORE",
      0.5f
    },
    {
      "BIG_TOWER",
      0.5f
    },
    {
      "STARGATE",
      -0.5f
    },
    {
      "WATERFALL",
      0.25f
    },
    {
      "BELL_TOWER",
      0.25f
    },
    {
      "LIGHTHOUSE",
      0.25f
    },
    {
      "ARCH",
      0.25f
    }
  };
  public string LevelName;
  public LevelNodeType NodeType;
  [Serialization(Optional = true)]
  public WinConditions Conditions;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool HasLesserGate;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool HasWarpGate;
  public List<MapNode.Connection> Connections = new List<MapNode.Connection>();
  [Serialization(Ignore = true)]
  public bool Valid;
  [Serialization(Ignore = true)]
  public Group Group;

  public MapNode() => this.Conditions = new WinConditions();

  public void Fill(MapTree.TreeFillContext context, MapNode parent, FaceOrientation origin)
  {
    if (this.Valid)
      return;
    TrileSet trileSet = (TrileSet) null;
    Level level;
    try
    {
      level = SdlSerializer.Deserialize<Level>($"{context.ContentRoot}\\Levels\\{this.LevelName}.lvl.sdl");
      if (level.TrileSetName != null)
      {
        if (!context.TrileSetCache.TryGetValue(level.TrileSetName, out trileSet))
          context.TrileSetCache.Add(level.TrileSetName, trileSet = SdlSerializer.Deserialize<TrileSet>($"{context.ContentRoot}\\Trile Sets\\{level.TrileSetName}.ts.sdl"));
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"Warning : Level {this.LevelName} could not be loaded because it has invalid markup. Skipping...");
      this.Valid = false;
      return;
    }
    this.NodeType = level.NodeType;
    this.Conditions.ChestCount = level.ArtObjects.Values.Count<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObjectName.IndexOf("treasure", StringComparison.InvariantCultureIgnoreCase) != -1)) / 2;
    this.Conditions.ScriptIds = level.Scripts.Values.Where<Script>((Func<Script, bool>) (x => x.IsWinCondition)).Select<Script, int>((Func<Script, int>) (x => x.Id)).ToList<int>();
    this.Conditions.SplitUpCount = level.Triles.Values.Union<TrileInstance>(level.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Overlaps)).SelectMany<TrileInstance, TrileInstance>((Func<TrileInstance, IEnumerable<TrileInstance>>) (x => (IEnumerable<TrileInstance>) x.OverlappedTriles))).Count<TrileInstance>((Func<TrileInstance, bool>) (x => x.TrileId >= 0 && trileSet[x.TrileId].ActorSettings.Type == ActorType.GoldenCube));
    this.Conditions.CubeShardCount = level.Triles.Values.Count<TrileInstance>((Func<TrileInstance, bool>) (x => x.TrileId >= 0 && trileSet[x.TrileId].ActorSettings.Type == ActorType.CubeShard));
    this.Conditions.OtherCollectibleCount = level.Triles.Values.Count<TrileInstance>((Func<TrileInstance, bool>) (x => x.TrileId >= 0 && trileSet[x.TrileId].ActorSettings.Type.IsTreasure() && trileSet[x.TrileId].ActorSettings.Type != ActorType.CubeShard)) + level.ArtObjects.Values.Count<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObjectName == "treasure_mapAO"));
    this.Conditions.LockedDoorCount = level.Triles.Values.Count<TrileInstance>((Func<TrileInstance, bool>) (x => x.TrileId >= 0 && trileSet[x.TrileId].ActorSettings.Type == ActorType.Door));
    this.Conditions.UnlockedDoorCount = level.Triles.Values.Count<TrileInstance>((Func<TrileInstance, bool>) (x => x.TrileId >= 0 && trileSet[x.TrileId].ActorSettings.Type == ActorType.UnlockedDoor));
    int num1 = level.ArtObjects.Count<KeyValuePair<int, ArtObjectInstance>>((Func<KeyValuePair<int, ArtObjectInstance>, bool>) (x => x.Value.ArtObjectName.IndexOf("fork", StringComparison.InvariantCultureIgnoreCase) != -1));
    int num2 = level.ArtObjects.Count<KeyValuePair<int, ArtObjectInstance>>((Func<KeyValuePair<int, ArtObjectInstance>, bool>) (x => x.Value.ArtObjectName.IndexOf("qr", StringComparison.InvariantCultureIgnoreCase) != -1));
    int num3 = level.Volumes.Count<KeyValuePair<int, Volume>>((Func<KeyValuePair<int, Volume>, bool>) (x => x.Value.ActorSettings != null && x.Value.ActorSettings.CodePattern != null && x.Value.ActorSettings.CodePattern.Length != 0));
    int num4 = this.LevelName == "OWL" ? 0 : level.NonPlayerCharacters.Count<KeyValuePair<int, NpcInstance>>((Func<KeyValuePair<int, NpcInstance>, bool>) (x => x.Value.Name == "Owl"));
    int num5 = level.ArtObjects.Count<KeyValuePair<int, ArtObjectInstance>>((Func<KeyValuePair<int, ArtObjectInstance>, bool>) (x => x.Value.ArtObjectName.Contains("BIT_DOOR") && !x.Value.ArtObjectName.Contains("BROKEN")));
    int num6 = level.Scripts.Values.Count<Script>((Func<Script, bool>) (s => s.Actions.Any<ScriptAction>((Func<ScriptAction, bool>) (a => a.Object.Type == "Level" && a.Operation == "ResolvePuzzle"))));
    int num7 = ((IEnumerable<string>) MapNode.PuzzleLevels).Contains<string>(this.LevelName) ? (this.LevelName == "CLOCK" ? 4 : 1) : 0;
    this.Conditions.SecretCount = num1 + num2 + num3 + num4 + num6 + num7 + num5;
    this.HasLesserGate = level.ArtObjects.Values.Any<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObjectName.IndexOf("lesser_gate", StringComparison.InvariantCultureIgnoreCase) != -1 && x.ArtObjectName.IndexOf("base", StringComparison.InvariantCultureIgnoreCase) == -1));
    this.HasWarpGate = level.ArtObjects.Values.Any<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObjectName == "GATE_GRAVEAO" || x.ArtObjectName == "GATEAO" || x.ArtObjectName == "GATE_INDUSTRIALAO" || x.ArtObjectName == "GATE_SEWERAO" || x.ArtObjectName == "ZU_GATEAO" || x.ArtObjectName == "GRAVE_GATEAO"));
    foreach (Script script in level.Scripts.Values)
    {
      foreach (ScriptAction action in script.Actions)
      {
        if (action.Object.Type == "Level" && action.Operation.Contains("Level"))
        {
          MapNode.Connection connection = new MapNode.Connection();
          bool flag = true;
          foreach (ScriptTrigger trigger in script.Triggers)
          {
            if (trigger.Object.Type == "Volume" && trigger.Event == "Enter")
            {
              int? identifier = trigger.Object.Identifier;
              if (identifier.HasValue)
              {
                identifier = trigger.Object.Identifier;
                int key = identifier.Value;
                Volume volume;
                if (!level.Volumes.TryGetValue(key, out volume))
                {
                  Console.WriteLine($"Warning : A level-changing script links to a nonexistent volume in {this.LevelName} (Volume Id #{(object) key})");
                  flag = false;
                  break;
                }
                if (volume.ActorSettings != null && volume.ActorSettings.IsSecretPassage)
                {
                  flag = false;
                  break;
                }
                connection.Face = volume.Orientations.First<FaceOrientation>();
                break;
              }
            }
          }
          if (flag)
          {
            string key = action.Operation == "ReturnToLastLevel" ? parent.LevelName : action.Arguments[0];
            if (!(key == "PYRAMID") && !(key == "CABIN_INTERIOR_A") && (!(key == "THRONE") || !(this.LevelName == "ZU_CITY_RUINS")) && (!(key == "ZU_CITY_RUINS") || !(this.LevelName == "THRONE")))
            {
              MapNode mapNode;
              if (!context.LoadedNodes.TryGetValue(key, out mapNode))
              {
                mapNode = new MapNode() { LevelName = key };
                context.LoadedNodes.Add(key, mapNode);
                connection.Node = mapNode;
                if (connection.Node != parent)
                {
                  if (parent != null && origin == connection.Face)
                    connection.Face = origin.GetOpposite();
                  if (((IEnumerable<string>) MapNode.UpLevels).Contains<string>(key))
                    connection.Face = FaceOrientation.Top;
                  else if (((IEnumerable<string>) MapNode.DownLevels).Contains<string>(key))
                    connection.Face = FaceOrientation.Down;
                  else if (((IEnumerable<string>) MapNode.OppositeLevels).Contains<string>(key))
                    connection.Face = connection.Face.GetOpposite();
                  else if (((IEnumerable<string>) MapNode.BackLevels).Contains<string>(key))
                    connection.Face = FaceOrientation.Back;
                  else if (((IEnumerable<string>) MapNode.LeftLevels).Contains<string>(key))
                    connection.Face = FaceOrientation.Left;
                  else if (((IEnumerable<string>) MapNode.RightLevels).Contains<string>(key))
                    connection.Face = FaceOrientation.Right;
                  else if (((IEnumerable<string>) MapNode.FrontLevels).Contains<string>(key))
                    connection.Face = FaceOrientation.Front;
                  float num8;
                  if (MapNode.OversizeLinks.TryGetValue(key, out num8))
                    connection.BranchOversize = num8;
                  this.Connections.Add(connection);
                  break;
                }
                break;
              }
              break;
            }
          }
        }
      }
    }
    this.Valid = true;
    foreach (MapNode.Connection connection in this.Connections)
      connection.Node.Fill(context, this, connection.Face);
  }

  public MapNode Clone()
  {
    return new MapNode()
    {
      LevelName = this.LevelName,
      NodeType = this.NodeType,
      HasWarpGate = this.HasWarpGate,
      HasLesserGate = this.HasLesserGate,
      Connections = this.Connections.Select<MapNode.Connection, MapNode.Connection>((Func<MapNode.Connection, MapNode.Connection>) (x => new MapNode.Connection()
      {
        Face = x.Face,
        Node = x.Node.Clone(),
        BranchOversize = x.BranchOversize
      })).ToList<MapNode.Connection>(),
      Conditions = this.Conditions.Clone()
    };
  }

  public class Connection
  {
    public FaceOrientation Face;
    public MapNode Node;
    [Serialization(Optional = true, DefaultValueOptional = true)]
    public float BranchOversize;
    [Serialization(Ignore = true)]
    public int MultiBranchId;
    [Serialization(Ignore = true)]
    public int MultiBranchCount;
    [Serialization(Ignore = true)]
    public List<int> LinkInstances;
  }
}
