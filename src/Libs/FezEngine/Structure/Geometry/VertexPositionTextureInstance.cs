// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.VertexPositionTextureInstance
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
public struct VertexPositionTextureInstance : 
  IEquatable<VertexPositionTextureInstance>,
  IShaderInstantiatableVertex,
  ITexturedVertex,
  IVertex,
  IVertexType
{
  private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[3]
  {
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
    new VertexElement(20, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
  });

  public VertexPositionTextureInstance(Vector3 position, Vector2 textureCoordinate)
    : this()
  {
    this.Position = position;
    this.TextureCoordinate = textureCoordinate;
  }

  public Vector3 Position { get; set; }

  public Vector2 TextureCoordinate { get; set; }

  public float InstanceIndex { get; set; }

  public override string ToString() => Util.ReflectToString((object) this);

  public VertexDeclaration VertexDeclaration => VertexPositionTextureInstance.vertexDeclaration;

  public bool Equals(VertexPositionTextureInstance other)
  {
    return other.Position == this.Position && other.TextureCoordinate == this.TextureCoordinate && (double) other.InstanceIndex == (double) this.InstanceIndex;
  }

  public override int GetHashCode()
  {
    return this.Position.GetHashCode() ^ this.TextureCoordinate.GetHashCode() ^ this.InstanceIndex.GetHashCode();
  }

  public override bool Equals(object obj)
  {
    return obj != null && this.Equals((VertexPositionTextureInstance) obj);
  }
}
