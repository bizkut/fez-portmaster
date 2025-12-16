// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.ArtObjectReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

#nullable disable
namespace FezEngine.Readers;

public class ArtObjectReader : ContentTypeReader<ArtObject>
{
  protected override ArtObject Read(ContentReader input, ArtObject existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new ArtObject();
    existingInstance.Name = input.ReadString();
    FutureTexture2D futureCubemap = input.ReadObject<FutureTexture2D>((ContentTypeReader) FutureTexture2DReader.Instance);
    DrawActionScheduler.Schedule((Action) (() => existingInstance.Cubemap = futureCubemap.Create()));
    existingInstance.Size = input.ReadVector3();
    existingInstance.Geometry = input.ReadObject<ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix>>(existingInstance.Geometry);
    existingInstance.ActorType = input.ReadObject<ActorType>();
    existingInstance.NoSihouette = input.ReadBoolean();
    return existingInstance;
  }
}
