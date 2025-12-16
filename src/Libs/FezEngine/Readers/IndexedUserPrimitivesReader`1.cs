// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.IndexedUserPrimitivesReader`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Geometry;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Readers;

public class IndexedUserPrimitivesReader<T> : ContentTypeReader<IndexedUserPrimitives<T>> where T : struct, IVertexType
{
  protected override IndexedUserPrimitives<T> Read(
    ContentReader input,
    IndexedUserPrimitives<T> existingInstance)
  {
    PrimitiveType type = input.ReadObject<PrimitiveType>();
    return new IndexedUserPrimitives<T>(input.ReadObject<T[]>(), input.ReadObject<int[]>(), type);
  }
}
