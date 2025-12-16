// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.VertexPositionInstance
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
public struct VertexPositionInstance : 
  IEquatable<VertexPositionInstance>,
  IVertex,
  IVertexType,
  IShaderInstantiatableVertex
{
  private static readonly VertexDeclaration vertexDeclaration = new VertexDeclaration(new VertexElement[2]
  {
    new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
    new VertexElement(12, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 0)
  });

  public VertexPositionInstance(Vector3 position)
    : this()
  {
    this.Position = position;
  }

  public Vector3 Position { get; set; }

  public float InstanceIndex { get; set; }

  public override string ToString() => Util.ReflectToString((object) this);

  public VertexDeclaration VertexDeclaration => VertexPositionInstance.vertexDeclaration;

  public bool Equals(VertexPositionInstance other)
  {
    return other.Position == this.Position && (double) other.InstanceIndex == (double) this.InstanceIndex;
  }

  public override int GetHashCode()
  {
    return this.Position.GetHashCode() ^ this.InstanceIndex.GetHashCode();
  }

  public override bool Equals(object obj)
  {
    return obj != null && this.Equals((VertexPositionInstance) obj);
  }
}
