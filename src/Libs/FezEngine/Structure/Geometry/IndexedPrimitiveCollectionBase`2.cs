// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.IndexedPrimitiveCollectionBase`2
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using FezEngine.Tools;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Structure.Geometry;

public abstract class IndexedPrimitiveCollectionBase<VertexType, IndexType> : 
  IIndexedPrimitiveCollection
{
  protected readonly GraphicsDevice device;
  protected readonly IGraphicsDeviceService GraphicsDeviceService;
  protected VertexType[] vertices;
  protected IndexType[] indices;
  protected PrimitiveType primitiveType;
  protected int primitiveCount;

  private IndexedPrimitiveCollectionBase()
  {
    if (!ServiceHelper.IsFull)
      return;
    this.GraphicsDeviceService = ServiceHelper.Get<IGraphicsDeviceService>();
  }

  protected IndexedPrimitiveCollectionBase(PrimitiveType type)
    : this()
  {
    this.primitiveType = type;
    if (this.GraphicsDeviceService == null)
      return;
    this.device = this.GraphicsDeviceService.GraphicsDevice;
  }

  public PrimitiveType PrimitiveType
  {
    get => this.primitiveType;
    set
    {
      this.primitiveType = value;
      this.UpdatePrimitiveCount();
    }
  }

  public virtual VertexType[] Vertices
  {
    get => this.vertices;
    set => this.vertices = value;
  }

  public virtual IndexType[] Indices
  {
    get => this.indices;
    set
    {
      this.indices = value;
      this.UpdatePrimitiveCount();
    }
  }

  public int VertexCount => this.vertices.Length;

  protected void UpdatePrimitiveCount()
  {
    this.primitiveCount = this.indices.Length;
    switch (this.primitiveType)
    {
      case PrimitiveType.TriangleList:
        this.primitiveCount /= 3;
        break;
      case PrimitiveType.TriangleStrip:
        this.primitiveCount -= 2;
        break;
      case PrimitiveType.LineList:
        this.primitiveCount /= 2;
        break;
      case PrimitiveType.LineStrip:
        --this.primitiveCount;
        break;
    }
  }

  public bool Empty => this.primitiveCount == 0;

  public abstract void Draw(BaseEffect effect);

  public abstract IIndexedPrimitiveCollection Clone();
}
