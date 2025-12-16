// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrileInstanceData
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Structure;

public struct TrileInstanceData(Vector3 position, float phi)
{
  private static readonly VertexDeclaration declaration = new VertexDeclaration(new VertexElement[1]
  {
    new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1)
  });
  public Vector4 PositionPhi = new Vector4(position, phi);

  public override string ToString() => $"{{PositionPhi:{this.PositionPhi}}}";

  public static int SizeInBytes => 16 /*0x10*/;

  public bool Equals(TrileInstanceData other) => other.PositionPhi.Equals(this.PositionPhi);

  public VertexDeclaration VertexDeclaration => TrileInstanceData.declaration;
}
