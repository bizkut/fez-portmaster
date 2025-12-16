// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Group
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Structure;

public class Group
{
  private Matrix worldMatrix;
  private Matrix translationMatrix;
  private Matrix scalingMatrix;
  private Matrix rotationMatrix;
  private Matrix scalingRotationMatrix;
  private readonly Dirtyable<Matrix?> textureMatrix = new Dirtyable<Matrix?>();
  private Texture texture;
  private Vector3 position;
  private Vector3 scale = Vector3.One;
  private Quaternion rotation = Quaternion.Identity;
  private readonly Dirtyable<Matrix> compositeWorldMatrix = new Dirtyable<Matrix>();
  private readonly Dirtyable<Matrix> inverseTransposeCompositeWorldMatrix = new Dirtyable<Matrix>();

  public int Id { get; set; }

  internal Group(Mesh mesh, int id)
  {
    this.Id = id;
    this.Mesh = mesh;
    this.Enabled = true;
    this.textureMatrix.Dirty = true;
    this.translationMatrix = this.scalingRotationMatrix = this.scalingMatrix = this.rotationMatrix = Matrix.Identity;
    this.worldMatrix = Matrix.Identity;
    this.RebuildCompositeWorld();
  }

  public Mesh Mesh { get; set; }

  public bool RotateOffCenter { get; set; }

  public bool Enabled { get; set; }

  public Material Material { get; set; }

  public Dirtyable<Matrix?> TextureMatrix
  {
    get => this.textureMatrix;
    set => this.textureMatrix.Set((Matrix?) value);
  }

  public bool EffectOwner => this.Mesh.Groups.Count == 1;

  public Microsoft.Xna.Framework.Graphics.CullMode? CullMode { get; set; }

  public bool? AlwaysOnTop { get; set; }

  public BlendingMode? Blending { get; set; }

  public SamplerState SamplerState { get; set; }

  public bool? NoAlphaWrite { get; set; }

  public TexturingType TexturingType { get; private set; }

  public Texture Texture
  {
    get => this.texture;
    set
    {
      this.texture = value;
      int num;
      switch (value)
      {
        case Texture2D _:
          num = 1;
          break;
        case TextureCube _:
          num = 2;
          break;
        default:
          num = 0;
          break;
      }
      this.TexturingType = (TexturingType) num;
    }
  }

  public Texture2D TextureMap => this.texture as Texture2D;

  public TextureCube CubeMap => this.texture as TextureCube;

  public object CustomData { get; set; }

  public IIndexedPrimitiveCollection Geometry { get; set; }

  public Vector3 Position
  {
    get => this.position;
    set
    {
      if (!(this.position != value))
        return;
      this.position = value;
      this.translationMatrix = Matrix.CreateTranslation(this.position);
      this.RebuildWorld(false);
    }
  }

  public Vector3 Scale
  {
    get => this.scale;
    set
    {
      if (!(this.scale != value))
        return;
      this.scale = value;
      this.scalingMatrix = Matrix.CreateScale(this.scale);
      this.scalingRotationMatrix = this.scalingMatrix * this.rotationMatrix;
      this.RebuildWorld(true);
    }
  }

  public void SetLazyScale(Vector3 scale)
  {
    this.scale = scale;
    this.scalingMatrix = Matrix.CreateScale(scale);
    this.scalingRotationMatrix = this.scalingMatrix * this.rotationMatrix;
  }

  public void SetLazyRotation(Quaternion r)
  {
    this.rotation = r;
    this.rotationMatrix = Matrix.CreateFromQuaternion(this.rotation);
    this.scalingRotationMatrix = this.scalingMatrix * this.rotationMatrix;
  }

  public void SetLazyPosition(Vector3 position)
  {
    this.position = position;
    this.translationMatrix = Matrix.CreateTranslation(position);
  }

  public void RecomputeMatrices() => this.RebuildWorld(true);

  public void RecomputeMatrices(bool noInvert) => this.RebuildWorld(noInvert);

  public Quaternion Rotation
  {
    get => this.rotation;
    set
    {
      if (!(this.rotation != value))
        return;
      this.rotation = value;
      this.rotationMatrix = Matrix.CreateFromQuaternion(this.rotation);
      this.scalingRotationMatrix = this.scalingMatrix * this.rotationMatrix;
      this.RebuildWorld(true);
    }
  }

  private void RebuildWorld(bool invert)
  {
    this.worldMatrix = !this.RotateOffCenter ? this.scalingRotationMatrix * this.translationMatrix : this.translationMatrix * this.scalingRotationMatrix;
    this.RebuildCompositeWorld(invert);
  }

  internal void RebuildCompositeWorld() => this.RebuildCompositeWorld(true);

  internal void RebuildCompositeWorld(bool invert)
  {
    this.compositeWorldMatrix.Set(this.worldMatrix * this.Mesh.WorldMatrix);
    if (!invert)
      return;
    this.inverseTransposeCompositeWorldMatrix.Set(Matrix.Transpose(Matrix.Invert((Matrix) this.compositeWorldMatrix)));
  }

  public Dirtyable<Matrix> WorldMatrix
  {
    get => this.compositeWorldMatrix;
    set
    {
      this.worldMatrix = (Matrix) value;
      this.worldMatrix.Decompose(out this.scale, out this.rotation, out this.position);
      this.translationMatrix = Matrix.CreateTranslation(this.position);
      this.scalingMatrix = Matrix.CreateScale(this.scale);
      this.rotationMatrix = Matrix.CreateFromQuaternion(this.rotation);
      this.scalingRotationMatrix = this.scalingMatrix * this.rotationMatrix;
      this.RebuildCompositeWorld(true);
    }
  }

  public Dirtyable<Matrix> InverseTransposeWorldMatrix => this.inverseTransposeCompositeWorldMatrix;

  public void Draw(BaseEffect effect)
  {
    if (this.Geometry == null)
      return;
    GraphicsDevice graphicsDevice = this.Mesh.GraphicsDevice;
    bool? nullable = this.AlwaysOnTop;
    if (nullable.HasValue)
    {
      DepthStencilCombiner dssCombiner = graphicsDevice.GetDssCombiner();
      nullable = this.AlwaysOnTop;
      int num = nullable.Value ? 0 : 3;
      dssCombiner.DepthBufferFunction = (CompareFunction) num;
    }
    BlendingMode? blending = this.Blending;
    if (blending.HasValue)
    {
      GraphicsDevice _ = graphicsDevice;
      blending = this.Blending;
      int num = (int) blending.Value;
      _.SetBlendingMode((BlendingMode) num);
    }
    if (this.SamplerState != null)
    {
      for (int index = 0; index < this.Mesh.UsedSamplers; ++index)
        graphicsDevice.SamplerStates[index] = this.SamplerState;
    }
    Microsoft.Xna.Framework.Graphics.CullMode? cullMode = this.CullMode;
    if (cullMode.HasValue)
    {
      GraphicsDevice _ = graphicsDevice;
      cullMode = this.CullMode;
      int num = (int) cullMode.Value;
      _.SetCullMode((Microsoft.Xna.Framework.Graphics.CullMode) num);
    }
    nullable = this.NoAlphaWrite;
    if (nullable.HasValue)
      graphicsDevice.GetBlendCombiner().ColorWriteChannels = ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue;
    effect.Prepare(this);
    graphicsDevice.ApplyCombiners();
    this.Geometry.Draw(effect);
  }

  internal Group Clone(Mesh mesh)
  {
    return new Group(mesh, mesh.Groups.Count)
    {
      Geometry = this.Geometry.Clone(),
      Material = this.Material == null ? (Material) null : this.Material.Clone(),
      Texture = this.Texture,
      WorldMatrix = (Dirtyable<Matrix>) this.worldMatrix,
      SamplerState = this.SamplerState,
      Blending = this.Blending,
      AlwaysOnTop = this.AlwaysOnTop,
      CullMode = this.CullMode,
      TexturingType = this.TexturingType,
      TextureMatrix = this.TextureMatrix,
      RotateOffCenter = this.RotateOffCenter,
      Enabled = this.Enabled
    };
  }

  public void BakeTransformInstanced<VertexType, InstanceType>()
    where VertexType : struct, IShaderInstantiatableVertex, IVertex
    where InstanceType : struct
  {
    VertexType[] vertices = (this.Geometry as ShaderInstancedIndexedPrimitives<VertexType, InstanceType>).Vertices;
    for (int index = 0; index < vertices.Length; ++index)
      vertices[index].Position = Vector3.Transform(vertices[index].Position, (Matrix) this.compositeWorldMatrix);
    this.WorldMatrix = (Dirtyable<Matrix>) Matrix.Identity;
  }

  public void BakeTransform<T>() where T : struct, IVertex
  {
    T[] vertices = (this.Geometry as IndexedUserPrimitives<T>).Vertices;
    for (int index = 0; index < vertices.Length; ++index)
      vertices[index].Position = Vector3.Transform(vertices[index].Position, (Matrix) this.compositeWorldMatrix);
    this.WorldMatrix = (Dirtyable<Matrix>) Matrix.Identity;
  }

  public void BakeTransformWithNormal<T>() where T : struct, ILitVertex
  {
    T[] vertices = (this.Geometry as IndexedUserPrimitives<T>).Vertices;
    for (int index = 0; index < vertices.Length; ++index)
    {
      vertices[index].Position = Vector3.Transform(vertices[index].Position, (Matrix) this.compositeWorldMatrix);
      vertices[index].Normal = Vector3.Normalize(Vector3.TransformNormal(vertices[index].Normal, (Matrix) this.compositeWorldMatrix));
    }
    this.WorldMatrix = (Dirtyable<Matrix>) Matrix.Identity;
  }

  public void BakeTransformWithNormalTexture<T>() where T : struct, ILitVertex, ITexturedVertex
  {
    T[] vertices = (this.Geometry as IndexedUserPrimitives<T>).Vertices;
    Matrix transform = this.textureMatrix.Value.HasValue ? this.textureMatrix.Value.Value : this.Mesh.TextureMatrix.Value;
    for (int index = 0; index < vertices.Length; ++index)
    {
      vertices[index].Position = Vector3.Transform(vertices[index].Position, (Matrix) this.compositeWorldMatrix);
      vertices[index].Normal = Vector3.Normalize(Vector3.TransformNormal(vertices[index].Normal, (Matrix) this.compositeWorldMatrix));
      vertices[index].TextureCoordinate = FezMath.TransformTexCoord(vertices[index].TextureCoordinate, transform);
    }
    this.TextureMatrix.Set(new Matrix?());
    this.WorldMatrix = (Dirtyable<Matrix>) Matrix.Identity;
  }

  public void InvertNormals<T>() where T : struct, ILitVertex
  {
    T[] vertices = (this.Geometry as IndexedUserPrimitives<T>).Vertices;
    for (int index = 0; index < vertices.Length; ++index)
      vertices[index].Normal = -vertices[index].Normal;
  }
}
