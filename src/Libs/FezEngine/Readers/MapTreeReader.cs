// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.MapTreeReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class MapTreeReader : ContentTypeReader<MapTree>
{
  protected override MapTree Read(ContentReader input, MapTree existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new MapTree();
    existingInstance.Root = input.ReadObject<MapNode>(existingInstance.Root);
    return existingInstance;
  }
}
