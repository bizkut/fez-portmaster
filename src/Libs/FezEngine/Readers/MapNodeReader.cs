// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.MapNodeReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class MapNodeReader : ContentTypeReader<MapNode>
{
  protected override MapNode Read(ContentReader input, MapNode existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new MapNode();
    existingInstance.LevelName = input.ReadString();
    existingInstance.Connections = input.ReadObject<List<MapNode.Connection>>(existingInstance.Connections);
    existingInstance.NodeType = input.ReadObject<LevelNodeType>();
    existingInstance.Conditions = input.ReadObject<WinConditions>(existingInstance.Conditions);
    existingInstance.HasLesserGate = input.ReadBoolean();
    existingInstance.HasWarpGate = input.ReadBoolean();
    return existingInstance;
  }
}
