// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.ShaderInstancedIndexedPrimitivesReader`2
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Readers;

public class ShaderInstancedIndexedPrimitivesReader<TemplateType, InstanceType> : 
  ContentTypeReader<ShaderInstancedIndexedPrimitives<TemplateType, InstanceType>>
  where TemplateType : struct, IShaderInstantiatableVertex
  where InstanceType : struct
{
  protected override ShaderInstancedIndexedPrimitives<TemplateType, InstanceType> Read(
    ContentReader input,
    ShaderInstancedIndexedPrimitives<TemplateType, InstanceType> existingInstance)
  {
    PrimitiveType type = input.ReadObject<PrimitiveType>();
    if (existingInstance == null)
      existingInstance = new ShaderInstancedIndexedPrimitives<TemplateType, InstanceType>(type, typeof (InstanceType) == typeof (Matrix) ? 60 : 200, typeof (InstanceType) == typeof (Vector4));
    else if (existingInstance.PrimitiveType != type)
      existingInstance.PrimitiveType = type;
    existingInstance.Vertices = input.ReadObject<TemplateType[]>(existingInstance.Vertices);
    ushort[] numArray1 = input.ReadObject<ushort[]>();
    int[] numArray2 = existingInstance.Indices = new int[numArray1.Length];
    for (int index = 0; index < numArray1.Length; ++index)
      numArray2[index] = (int) numArray1[index];
    return existingInstance;
  }
}
