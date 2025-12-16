// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.MapTree
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class MapTree
{
  private const string Hub = "NATURE_HUB";
  public MapNode Root;

  public void Fill(string contentRoot)
  {
    MapTree.TreeFillContext context = new MapTree.TreeFillContext()
    {
      ContentRoot = contentRoot
    };
    this.Root = new MapNode() { LevelName = "NATURE_HUB" };
    context.LoadedNodes.Add("NATURE_HUB", this.Root);
    this.Root.Fill(context, (MapNode) null, FaceOrientation.Front);
    SdlSerializer.Serialize<MapTree>(contentRoot + "\\MapTree.map.sdl", this);
  }

  public MapTree Clone()
  {
    return new MapTree() { Root = this.Root.Clone() };
  }

  public class TreeFillContext
  {
    public string ContentRoot;
    public readonly Dictionary<string, MapNode> LoadedNodes = new Dictionary<string, MapNode>();
    public readonly Dictionary<string, TrileSet> TrileSetCache = new Dictionary<string, TrileSet>();
  }
}
