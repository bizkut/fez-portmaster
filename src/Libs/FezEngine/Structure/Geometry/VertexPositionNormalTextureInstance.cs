// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.VertexPositionNormalTextureInstance
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

#nullable disable
namespace FezEngine.Structure.Geometry;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionNormalTextureInstance : 
  IEquatable<VertexPositionNormalTextureInstance>,
  IShaderInstantiatableVertex,
  ILitVertex,
  IVertex,
  IVertexType,
  ITexturedVertex
{
  private static readonly VertexDeclaration vertexDeclaration;
  public static readonly Vector3[] ByteToNormal = new Vector3[6]
  {
    Vector3.Left,
    Vector3.Down,
    Vector3.Forward,
    Vector3.Right,
    Vector3.Up,
    Vector3.Backward
  };
  private Vector3 position;
  private Vector3 normal;
  private Vector2 textureCoordinate;
  private float instanceIndex;

  static VertexPositionNormalTextureInstance()
  {
    VertexPositionNormalTextureInstance.vertexDeclaration = new VertexDeclaration(new VertexElement[4]
    {
      new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
      new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
      new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
      new VertexElement(32 /*0x20*/, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
    });
  }

  public VertexPositionNormalTextureInstance(Vector3 position, Vector3 normal)
    : this(position, normal, -1f)
  {
  }

  public VertexPositionNormalTextureInstance(
    Vector3 position,
    byte normal,
    Vector2 textureCoordinate)
    : this()
  {
    this.position = position;
    this.normal = VertexPositionNormalTextureInstance.ByteToNormal[(int) normal];
    this.textureCoordinate = textureCoordinate;
    this.instanceIndex = -1f;
  }

  public VertexPositionNormalTextureInstance(Vector3 position, Vector3 normal, float instanceIndex)
    : this()
  {
    this.Position = position;
    this.Normal = normal;
    this.InstanceIndex = instanceIndex;
  }

  public Vector3 Position
  {
    get => this.position;
    set => this.position = value;
  }

  public Vector3 Normal
  {
    get => this.normal;
    set => this.normal = value;
  }

  public Vector2 TextureCoordinate
  {
    get => this.textureCoordinate;
    set => this.textureCoordinate = value;
  }

  public float InstanceIndex
  {
    get => this.instanceIndex;
    set => this.instanceIndex = value;
  }

  public override string ToString() => Util.ReflectToString((object) this);

  public VertexDeclaration VertexDeclaration
  {
    get => VertexPositionNormalTextureInstance.vertexDeclaration;
  }

  public bool Equals(VertexPositionNormalTextureInstance other)
  {
    return other.Position == this.Position && other.Normal == this.Normal;
  }

  public override int GetHashCode()
  {
    Vector3 vector3 = this.Position;
    int hashCode1 = vector3.GetHashCode();
    vector3 = this.Normal;
    int hashCode2 = vector3.GetHashCode();
    return hashCode1 ^ hashCode2;
  }

  public override bool Equals(object obj)
  {
    return obj != null && this.Equals((VertexPositionNormalTextureInstance) obj);
  }
}
