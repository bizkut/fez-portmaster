// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.IndexedUserPrimitives`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure.Geometry;

public class IndexedUserPrimitives<T> : IndexedPrimitiveCollectionBase<T, int> where T : struct, IVertexType
{
  public IndexedUserPrimitives(PrimitiveType type)
    : this((T[]) null, (int[]) null, type)
  {
  }

  public IndexedUserPrimitives(T[] vertices, int[] indices, PrimitiveType type)
    : base(type)
  {
    this.vertices = vertices ?? new T[0];
    this.Indices = indices ?? new int[0];
  }

  public override void Draw(BaseEffect effect)
  {
    if (this.device == null || this.vertices.Length == 0 || this.Empty)
      return;
    effect.Apply();
    this.device.DrawUserIndexedPrimitives<T>(this.primitiveType, this.vertices, 0, this.vertices.Length, this.indices, 0, this.primitiveCount);
  }

  public override IIndexedPrimitiveCollection Clone()
  {
    return (IIndexedPrimitiveCollection) new IndexedUserPrimitives<T>(((IEnumerable<T>) this.vertices).ToArray<T>(), this.indices, this.primitiveType);
  }
}
