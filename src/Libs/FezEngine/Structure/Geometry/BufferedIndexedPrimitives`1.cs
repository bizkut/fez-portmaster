// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.BufferedIndexedPrimitives`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using FezEngine.Tools;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Structure.Geometry;

public class BufferedIndexedPrimitives<T> : IndexedPrimitiveCollectionBase<T, int>, IDisposable where T : struct, IVertexType
{
  private VertexBuffer vertexBuffer;
  private IndexBuffer indexBuffer;
  private int vertexCount;
  private int pendingUpdates;

  public BufferedIndexedPrimitives(PrimitiveType type)
    : this((T[]) null, (int[]) null, type)
  {
  }

  public BufferedIndexedPrimitives(T[] vertices, int[] indices, PrimitiveType type)
    : base(type)
  {
    this.vertices = vertices ?? new T[0];
    this.Indices = indices ?? new int[0];
  }

  public void UpdateBuffers()
  {
    this.vertexCount = this.VertexCount;
    if (this.vertexBuffer != null)
    {
      this.vertexBuffer.Dispose();
      this.vertexBuffer = (VertexBuffer) null;
    }
    if (this.indexBuffer != null)
    {
      this.indexBuffer.Dispose();
      this.indexBuffer = (IndexBuffer) null;
    }
    ++this.pendingUpdates;
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.vertexBuffer = new VertexBuffer(this.device, typeof (T), this.vertexCount, BufferUsage.WriteOnly);
      this.indexBuffer = new IndexBuffer(this.device, IndexElementSize.ThirtyTwoBits, this.indices.Length, BufferUsage.WriteOnly);
      this.vertexBuffer.SetData<T>(this.vertices);
      this.indexBuffer.SetData<int>(this.indices);
      --this.pendingUpdates;
    }));
  }

  public void CleanUp()
  {
    if (this.pendingUpdates > 0)
    {
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.indices = (int[]) null;
        this.vertices = (T[]) null;
      }));
    }
    else
    {
      this.indices = (int[]) null;
      this.vertices = (T[]) null;
    }
  }

  public void Dispose()
  {
    this.CleanUp();
    if (this.indexBuffer != null)
      this.indexBuffer.Dispose();
    this.indexBuffer = (IndexBuffer) null;
    if (this.vertexBuffer != null)
      this.vertexBuffer.Dispose();
    this.vertexBuffer = (VertexBuffer) null;
  }

  public override void Draw(BaseEffect effect)
  {
    if (this.device == null || this.primitiveCount <= 0 || this.indexBuffer == null || this.vertexBuffer == null)
      return;
    this.device.SetVertexBuffer(this.vertexBuffer);
    this.device.Indices = this.indexBuffer;
    effect.Apply();
    this.device.DrawIndexedPrimitives(this.primitiveType, 0, 0, this.vertexCount, 0, this.primitiveCount);
  }

  public override IIndexedPrimitiveCollection Clone() => throw new NotImplementedException();
}
