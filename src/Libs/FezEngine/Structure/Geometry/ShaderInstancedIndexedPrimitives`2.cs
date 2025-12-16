// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.ShaderInstancedIndexedPrimitives`2
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Structure.Geometry;

public class ShaderInstancedIndexedPrimitives<TemplateType, InstanceType> : 
  IndexedPrimitiveCollectionBase<TemplateType, int>,
  IFakeDisposable
  where TemplateType : struct, IShaderInstantiatableVertex
  where InstanceType : struct
{
  public int PredictiveBatchSize = 16 /*0x10*/;
  private readonly int InstancesPerBatch;
  private VertexBuffer vertexBuffer;
  private IndexBuffer indexBuffer;
  private DynamicVertexBuffer instanceBuffer;
  public InstanceType[] Instances;
  public int InstanceCount;
  public bool InstancesDirty;
  private int oldInstanceCount;
  private int[] tempIndices = new int[0];
  private TemplateType[] tempVertices = new TemplateType[0];
  private VertexDeclaration vertexDeclaration;
  private bool appendIndex;
  private IndexedVector4[] indexedInstances = new IndexedVector4[0];
  private bool useHwInstancing;

  public ShaderInstancedIndexedPrimitives(
    PrimitiveType type,
    int instancesPerBatch,
    bool appendIndex = false)
    : base(type)
  {
    this.InstancesPerBatch = instancesPerBatch;
    this.appendIndex = appendIndex;
    this.RefreshInstancingMode(true);
    BaseEffect.InstancingModeChanged += new Action(this.RefreshInstancingModeInternal);
  }

  private void RefreshInstancingModeInternal() => this.RefreshInstancingMode();

  private void RefreshInstancingMode(bool force = false, bool skipUpdate = false)
  {
    if (!force && this.useHwInstancing == BaseEffect.UseHardwareInstancing)
      return;
    if (this.vertexDeclaration != null)
    {
      this.vertexDeclaration.Dispose();
      this.vertexDeclaration = (VertexDeclaration) null;
    }
    this.useHwInstancing = BaseEffect.UseHardwareInstancing;
    if (this.useHwInstancing)
    {
      if (typeof (InstanceType) == typeof (Matrix))
      {
        this.vertexDeclaration = new VertexDeclaration(new VertexElement[4]
        {
          new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
          new VertexElement(16 /*0x10*/, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 3),
          new VertexElement(32 /*0x20*/, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 4),
          new VertexElement(48 /*0x30*/, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 5)
        });
      }
      else
      {
        if (!(typeof (InstanceType) == typeof (Vector4)))
          throw new InvalidOperationException("Unsupported instance size!");
        if (this.appendIndex)
          this.vertexDeclaration = new VertexDeclaration(new VertexElement[2]
          {
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2),
            new VertexElement(16 /*0x10*/, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 3)
          });
        else
          this.vertexDeclaration = new VertexDeclaration(new VertexElement[1]
          {
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 2)
          });
      }
    }
    if (skipUpdate)
      return;
    this.ResetBuffers();
    this.UpdateBuffers(true);
  }

  public void MaximizeBuffers(int maxInstances)
  {
    int instanceCount = this.InstanceCount;
    this.InstanceCount = maxInstances;
    this.UpdateBuffers();
    this.InstanceCount = instanceCount;
  }

  public override TemplateType[] Vertices
  {
    get => base.Vertices;
    set
    {
      base.Vertices = value;
      this.UpdateBuffers(true);
    }
  }

  public override int[] Indices
  {
    get => base.Indices;
    set
    {
      base.Indices = value;
      this.UpdateBuffers(true);
    }
  }

  public void ResetBuffers()
  {
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.oldInstanceCount = 0;
      if (this.indexBuffer != null)
        this.indexBuffer.Dispose();
      this.indexBuffer = (IndexBuffer) null;
      if (this.vertexBuffer != null)
        this.vertexBuffer.Dispose();
      this.vertexBuffer = (VertexBuffer) null;
      if (this.instanceBuffer != null)
        this.instanceBuffer.Dispose();
      this.instanceBuffer = (DynamicVertexBuffer) null;
      Array.Resize<int>(ref this.tempIndices, 0);
      Array.Resize<TemplateType>(ref this.tempVertices, 0);
      Array.Resize<IndexedVector4>(ref this.indexedInstances, 0);
    }));
  }

  public bool IsDisposed { get; private set; }

  public void Dispose()
  {
    this.ResetBuffers();
    BaseEffect.InstancingModeChanged -= new Action(this.RefreshInstancingModeInternal);
    this.IsDisposed = true;
  }

  private void Rehydrate(bool skipUpdate)
  {
    this.IsDisposed = false;
    this.RefreshInstancingMode(true, skipUpdate);
    BaseEffect.InstancingModeChanged += new Action(this.RefreshInstancingModeInternal);
  }

  public void UpdateBuffers() => this.UpdateBuffers(false);

  private void UpdateBuffers(bool rebuild)
  {
    if (this.IsDisposed)
      this.Rehydrate(true);
    if (this.device == null || this.vertices == null || this.vertices.Length == 0 || this.indices == null || this.indices.Length == 0 || this.Instances == null || this.InstanceCount <= 0)
      return;
    int num1 = (int) Math.Ceiling((double) this.oldInstanceCount / (double) this.PredictiveBatchSize) * this.PredictiveBatchSize;
    int batchCeiling = (int) Math.Ceiling((double) this.InstanceCount / (double) this.PredictiveBatchSize) * this.PredictiveBatchSize;
    bool newInstanceBatch = batchCeiling > num1;
    bool flag1 = this.vertexBuffer == null | rebuild;
    if (!this.useHwInstancing)
      flag1 |= newInstanceBatch;
    if (flag1)
    {
      int vertexCount = this.useHwInstancing ? this.vertices.Length : batchCeiling * this.vertices.Length;
      DrawActionScheduler.Schedule((Action) (() =>
      {
        if (this.vertexBuffer != null)
        {
          this.vertexBuffer.Dispose();
          this.vertexBuffer = (VertexBuffer) null;
        }
        if (this.useHwInstancing)
        {
          this.vertexBuffer = new VertexBuffer(this.device, typeof (TemplateType), vertexCount, BufferUsage.WriteOnly);
          this.vertexBuffer.SetData<TemplateType>(this.vertices);
        }
        else
          this.vertexBuffer = (VertexBuffer) new DynamicVertexBuffer(this.device, typeof (TemplateType), vertexCount, BufferUsage.WriteOnly);
      }));
    }
    bool flag2 = this.indexBuffer == null | rebuild;
    if (!this.useHwInstancing)
      flag2 |= newInstanceBatch;
    if (flag2)
    {
      int indexCount = this.useHwInstancing ? this.indices.Length : batchCeiling * this.indices.Length;
      DrawActionScheduler.Schedule((Action) (() =>
      {
        if (this.indexBuffer != null)
        {
          this.indexBuffer.Dispose();
          this.indexBuffer = (IndexBuffer) null;
        }
        if (this.useHwInstancing)
        {
          this.indexBuffer = new IndexBuffer(this.device, IndexElementSize.ThirtyTwoBits, indexCount, BufferUsage.WriteOnly);
          this.indexBuffer.SetData<int>(this.indices);
        }
        else
          this.indexBuffer = (IndexBuffer) new DynamicIndexBuffer(this.device, IndexElementSize.ThirtyTwoBits, indexCount, BufferUsage.WriteOnly);
      }));
    }
    bool newInstances = this.InstanceCount > this.oldInstanceCount;
    if (rebuild | newInstanceBatch)
      this.oldInstanceCount = 0;
    int newInstanceCount = this.InstanceCount;
    if (!this.useHwInstancing)
    {
      DrawActionScheduler.Schedule((Action) (() =>
      {
        if (rebuild | newInstanceBatch)
        {
          Array.Resize<int>(ref this.tempIndices, batchCeiling * this.indices.Length);
          Array.Resize<TemplateType>(ref this.tempVertices, batchCeiling * this.vertices.Length);
        }
        if (this.oldInstanceCount == 0)
          Array.Copy((Array) this.indices, (Array) this.tempIndices, this.indices.Length);
        for (int oldInstanceCount = this.oldInstanceCount; oldInstanceCount < newInstanceCount; ++oldInstanceCount)
        {
          int destinationIndex = this.vertices.Length * oldInstanceCount;
          Array.Copy((Array) this.vertices, 0, (Array) this.tempVertices, destinationIndex, this.vertices.Length);
          for (int index = 0; index < this.vertices.Length; ++index)
            this.tempVertices[destinationIndex + index].InstanceIndex = (float) oldInstanceCount;
          if (oldInstanceCount != 0)
          {
            int num2 = oldInstanceCount * this.indices.Length;
            for (int index = 0; index < this.indices.Length; ++index)
              this.tempIndices[num2 + index] = this.indices[index] + destinationIndex;
          }
        }
        if (rebuild | newInstances)
        {
          this.vertexBuffer.SetData<TemplateType>(this.tempVertices);
          this.indexBuffer.SetData<int>(this.tempIndices);
        }
        this.oldInstanceCount = newInstanceCount;
      }));
    }
    else
    {
      if (!(this.instanceBuffer == null | rebuild | newInstanceBatch))
        return;
      DrawActionScheduler.Schedule((Action) (() =>
      {
        if (this.instanceBuffer != null)
        {
          this.instanceBuffer.Dispose();
          this.instanceBuffer = (DynamicVertexBuffer) null;
        }
        this.instanceBuffer = new DynamicVertexBuffer(this.device, this.vertexDeclaration, batchCeiling, BufferUsage.WriteOnly);
        if (this.appendIndex)
          Array.Resize<IndexedVector4>(ref this.indexedInstances, batchCeiling);
        this.oldInstanceCount = newInstanceCount;
        this.InstancesDirty = true;
      }));
    }
  }

  public override void Draw(BaseEffect effect)
  {
    if (this.IsDisposed)
      this.Rehydrate(false);
    if (this.device == null || this.primitiveCount <= 0 || this.vertices == null || this.vertices.Length == 0 || this.indexBuffer == null || this.vertexBuffer == null || this.Instances == null || this.InstanceCount <= 0)
      return;
    IShaderInstantiatableEffect<InstanceType> instantiatableEffect = effect as IShaderInstantiatableEffect<InstanceType>;
    if (this.useHwInstancing)
      this.device.SetVertexBuffers((VertexBufferBinding) this.vertexBuffer, new VertexBufferBinding((VertexBuffer) this.instanceBuffer, 0, 1));
    else
      this.device.SetVertexBuffer(this.vertexBuffer);
    this.device.Indices = this.indexBuffer;
    int batchInstances;
    if (this.useHwInstancing)
    {
      int num = Math.Min(this.InstanceCount, this.instanceBuffer.VertexCount);
      if (this.InstancesDirty)
      {
        if (this.appendIndex)
        {
          for (int index = 0; index < num; ++index)
          {
            this.indexedInstances[index].Data = __refvalue (__makeref (this.Instances[index]), Vector4);
            this.indexedInstances[index].Index = (float) index;
          }
          this.instanceBuffer.SetData<IndexedVector4>(this.indexedInstances, 0, num);
        }
        else
          this.instanceBuffer.SetData<InstanceType>(this.Instances, 0, num);
        this.InstancesDirty = false;
      }
      effect.Apply();
      this.device.DrawInstancedPrimitives(this.primitiveType, 0, 0, this.vertices.Length, 0, this.primitiveCount, num);
    }
    else
    {
      for (int instanceCount = this.InstanceCount; instanceCount > 0; instanceCount -= batchInstances)
      {
        batchInstances = Math.Min(instanceCount, this.InstancesPerBatch);
        int start = this.InstanceCount - instanceCount;
        instantiatableEffect.SetInstanceData(this.Instances, start, batchInstances);
        effect.Apply();
        this.device.DrawIndexedPrimitives(this.primitiveType, 0, 0, batchInstances * this.vertices.Length, 0, batchInstances * this.primitiveCount);
      }
    }
  }

  public override IIndexedPrimitiveCollection Clone()
  {
    ShaderInstancedIndexedPrimitives<TemplateType, InstanceType> indexedPrimitives = new ShaderInstancedIndexedPrimitives<TemplateType, InstanceType>(this.primitiveType, this.InstancesPerBatch);
    indexedPrimitives.Vertices = this.Vertices;
    indexedPrimitives.Indices = this.Indices;
    return (IIndexedPrimitiveCollection) indexedPrimitives;
  }
}
