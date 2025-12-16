// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.FezVertexPositionColor
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
public struct FezVertexPositionColor : 
  IEquatable<FezVertexPositionColor>,
  IColoredVertex,
  IVertex,
  IVertexType
{
  public static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[2]
  {
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0)
  });
  private Vector3 position;
  private Color color;

  public Vector3 Position
  {
    get => this.position;
    set => this.position = value;
  }

  public Color Color
  {
    get => this.color;
    set => this.color = value;
  }

  public FezVertexPositionColor(Vector3 position, Color color)
    : this()
  {
    this.position = position;
    this.color = color;
  }

  public override string ToString() => $"{{Position:{this.position} Color:{this.color}}}";

  public VertexDeclaration VertexDeclaration => FezVertexPositionColor.vertexDeclaration;

  public bool Equals(FezVertexPositionColor other)
  {
    return other.position == this.position && other.color == this.color;
  }

  public override int GetHashCode() => this.position.GetHashCode() ^ this.color.GetHashCode();

  public override bool Equals(object obj)
  {
    return obj != null && this.Equals((FezVertexPositionColor) obj);
  }
}
