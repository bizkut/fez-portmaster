// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.FezVertexPositionTexture
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Runtime.InteropServices;

#nullable disable
namespace FezEngine.Structure.Geometry;

[Serializable]
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct FezVertexPositionTexture : 
  IEquatable<FezVertexPositionTexture>,
  ITexturedVertex,
  IVertex,
  IVertexType
{
  public static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[2]
  {
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
  });

  public Vector3 Position { get; set; }

  public Vector2 TextureCoordinate { get; set; }

  public FezVertexPositionTexture(Vector3 position, Vector2 textureCoordinate)
    : this()
  {
    this.Position = position;
    this.TextureCoordinate = textureCoordinate;
  }

  public override string ToString()
  {
    return $"{{Position:{this.Position} TextureCoordinate:{this.TextureCoordinate}}}";
  }

  public VertexDeclaration VertexDeclaration => FezVertexPositionTexture.vertexDeclaration;

  public bool Equals(FezVertexPositionTexture other)
  {
    return other.Position.Equals(this.Position) && other.TextureCoordinate.Equals(this.TextureCoordinate);
  }
}
