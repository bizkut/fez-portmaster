// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.TrileReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class TrileReader : ContentTypeReader<Trile>
{
  protected override Trile Read(ContentReader input, Trile existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new Trile();
    existingInstance.Name = input.ReadString();
    existingInstance.CubemapPath = input.ReadString();
    existingInstance.Size = input.ReadVector3();
    existingInstance.Offset = input.ReadVector3();
    existingInstance.Immaterial = input.ReadBoolean();
    existingInstance.SeeThrough = input.ReadBoolean();
    existingInstance.Thin = input.ReadBoolean();
    existingInstance.ForceHugging = input.ReadBoolean();
    existingInstance.Faces = input.ReadObject<Dictionary<FaceOrientation, CollisionType>>(existingInstance.Faces);
    existingInstance.Geometry = input.ReadObject<ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4>>(existingInstance.Geometry);
    existingInstance.ActorSettings.Type = input.ReadObject<ActorType>();
    existingInstance.ActorSettings.Face = input.ReadObject<FaceOrientation>();
    existingInstance.SurfaceType = input.ReadObject<SurfaceType>();
    existingInstance.AtlasOffset = input.ReadVector2();
    return existingInstance;
  }
}
