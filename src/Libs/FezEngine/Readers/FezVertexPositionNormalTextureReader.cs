// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.FezVertexPositionNormalTextureReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Geometry;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class FezVertexPositionNormalTextureReader : ContentTypeReader<FezVertexPositionNormalTexture>
{
  protected override FezVertexPositionNormalTexture Read(
    ContentReader input,
    FezVertexPositionNormalTexture existingInstance)
  {
    return new FezVertexPositionNormalTexture(input.ReadVector3(), input.ReadVector3())
    {
      TextureCoordinate = input.ReadVector2()
    };
  }
}
