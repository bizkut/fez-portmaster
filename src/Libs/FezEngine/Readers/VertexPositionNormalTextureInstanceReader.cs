// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.VertexPositionNormalTextureInstanceReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Geometry;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class VertexPositionNormalTextureInstanceReader : 
  ContentTypeReader<VertexPositionNormalTextureInstance>
{
  protected override VertexPositionNormalTextureInstance Read(
    ContentReader input,
    VertexPositionNormalTextureInstance existingInstance)
  {
    return new VertexPositionNormalTextureInstance(input.ReadVector3(), input.ReadByte(), input.ReadVector2());
  }
}
