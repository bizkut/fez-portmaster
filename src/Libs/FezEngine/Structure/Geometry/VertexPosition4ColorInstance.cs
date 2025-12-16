// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.VertexPosition4ColorInstance
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
public struct VertexPosition4ColorInstance : 
  IEquatable<VertexPosition4ColorInstance>,
  IShaderInstantiatableVertex,
  IVertexType
{
  private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[3]
  {
    new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 0),
    new VertexElement(16 /*0x10*/, VertexElementFormat.Color, VertexElementUsage.Color, 0),
    new VertexElement(20, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
  });

  public VertexDeclaration VertexDeclaration => VertexPosition4ColorInstance.vertexDeclaration;

  public Vector4 Position { get; set; }

  public Color Color { get; set; }

  public float InstanceIndex { get; set; }

  public VertexPosition4ColorInstance(Vector4 position, Color color)
    : this()
  {
    this.Position = position;
    this.Color = color;
  }

  public override string ToString() => $"{{Position:{this.Position} Color:{this.Color}}}";

  public static int SizeInBytes => 24;

  public bool Equals(VertexPosition4ColorInstance other)
  {
    return other.Position == this.Position && other.Color == this.Color;
  }

  public override int GetHashCode() => this.Position.GetHashCode() ^ this.Color.GetHashCode();

  public override bool Equals(object obj)
  {
    return obj != null && this.Equals((VertexPosition4ColorInstance) obj);
  }
}
