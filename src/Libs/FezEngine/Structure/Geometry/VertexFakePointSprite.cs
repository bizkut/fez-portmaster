// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.VertexFakePointSprite
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
public struct VertexFakePointSprite : 
  IEquatable<VertexFakePointSprite>,
  IColoredVertex,
  IVertex,
  IVertexType,
  ITexturedVertex
{
  private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[4]
  {
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
    new VertexElement(16 /*0x10*/, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
    new VertexElement(24, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1)
  });
  private Vector3 _position;
  private Color _color;
  private Vector2 _textureCoordinate;
  private Vector2 _offset;

  public Vector3 Position
  {
    get => this._position;
    set => this._position = value;
  }

  public Color Color
  {
    get => this._color;
    set => this._color = value;
  }

  public Vector2 TextureCoordinate
  {
    get => this._textureCoordinate;
    set => this._textureCoordinate = value;
  }

  public Vector2 Offset
  {
    get => this._offset;
    set => this._offset = value;
  }

  public VertexFakePointSprite(
    Vector3 centerPosition,
    Color color,
    Vector2 texCoord,
    Vector2 offset)
    : this()
  {
    this._position = centerPosition;
    this._color = color;
    this._textureCoordinate = texCoord;
    this._offset = offset;
  }

  public VertexDeclaration VertexDeclaration => VertexFakePointSprite.vertexDeclaration;

  public bool Equals(VertexFakePointSprite other)
  {
    return other.Position == this.Position && other.Color == this.Color && other.TextureCoordinate == this.TextureCoordinate && other.Offset == this.Offset;
  }

  public override int GetHashCode()
  {
    return this.Position.GetHashCode() ^ this.Color.GetHashCode() ^ this.TextureCoordinate.GetHashCode() ^ this.Offset.GetHashCode();
  }

  public override bool Equals(object obj)
  {
    return obj != null && this.Equals((VertexFakePointSprite) obj);
  }
}
