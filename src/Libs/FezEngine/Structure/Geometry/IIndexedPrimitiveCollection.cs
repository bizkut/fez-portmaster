// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.IIndexedPrimitiveCollection
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;

#nullable disable
namespace FezEngine.Structure.Geometry;

public interface IIndexedPrimitiveCollection
{
  void Draw(BaseEffect effect);

  bool Empty { get; }

  IIndexedPrimitiveCollection Clone();

  int VertexCount { get; }
}
