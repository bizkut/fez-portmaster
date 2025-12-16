// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.FezVertexPositionNormalTexture
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
public struct FezVertexPositionNormalTexture : 
  IEquatable<FezVertexPositionNormalTexture>,
  ILitVertex,
  IVertex,
  IVertexType,
  ITexturedVertex
{
  private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[3]
  {
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
  });

  public FezVertexPositionNormalTexture(Vector3 position, Vector3 normal)
    : this()
  {
    this.Position = position;
    this.Normal = normal;
  }

  public FezVertexPositionNormalTexture(Vector3 position, Vector3 normal, Vector2 texCoord)
    : this(position, normal)
  {
    this.TextureCoordinate = texCoord;
  }

  public override string ToString() => Util.ReflectToString((object) this);

  public VertexDeclaration VertexDeclaration => FezVertexPositionNormalTexture.vertexDeclaration;

  public Vector3 Position { get; set; }

  public Vector3 Normal { get; set; }

  public Vector2 TextureCoordinate { get; set; }

  public bool Equals(FezVertexPositionNormalTexture other)
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
    return obj != null && this.Equals((FezVertexPositionNormalTexture) obj);
  }
}
