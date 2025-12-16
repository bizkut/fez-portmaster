// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.MapNodeConnectionReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class MapNodeConnectionReader : ContentTypeReader<MapNode.Connection>
{
  protected override MapNode.Connection Read(
    ContentReader input,
    MapNode.Connection existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new MapNode.Connection();
    existingInstance.Face = input.ReadObject<FaceOrientation>();
    existingInstance.Node = input.ReadObject<MapNode>(existingInstance.Node);
    existingInstance.BranchOversize = input.ReadSingle();
    return existingInstance;
  }
}
