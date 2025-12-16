// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.VertexPositionNormalColor
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
public struct VertexPositionNormalColor : 
  IEquatable<VertexPositionNormalColor>,
  ILitVertex,
  IVertex,
  IVertexType
{
  public static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[3]
  {
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
    new VertexElement(24, VertexElementFormat.Color, VertexElementUsage.Color, 0)
  });

  public VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color)
    : this()
  {
    this.Position = position;
    this.Normal = normal;
    this.Color = color;
  }

  public Vector3 Position { get; set; }

  public Vector3 Normal { get; set; }

  public Color Color { get; set; }

  public override string ToString()
  {
    return $"{{Position:{this.Position} Normal:{this.Normal} Color:{this.Color}}}";
  }

  public VertexDeclaration VertexDeclaration => VertexPositionNormalColor.vertexDeclaration;

  public bool Equals(VertexPositionNormalColor other)
  {
    return other.Position.Equals(this.Position) && other.Normal.Equals(this.Normal) && other.Color.Equals(this.Color);
  }
}
