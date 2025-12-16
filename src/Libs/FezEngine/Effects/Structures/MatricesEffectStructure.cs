// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.MatricesEffectStructure
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects.Structures;

internal class MatricesEffectStructure
{
  private readonly SemanticMappedMatrix worldViewProjection;
  private readonly SemanticMappedMatrix worldInverseTranspose;
  private readonly SemanticMappedMatrix world;
  private readonly SemanticMappedMatrix textureMatrix;
  private readonly SemanticMappedMatrix viewProjection;

  public MatricesEffectStructure(EffectParameterCollection parameters)
  {
    this.worldViewProjection = new SemanticMappedMatrix(parameters, "Matrices_WorldViewProjection");
    this.worldInverseTranspose = new SemanticMappedMatrix(parameters, "Matrices_WorldInverseTranspose");
    this.world = new SemanticMappedMatrix(parameters, "Matrices_World");
    this.textureMatrix = new SemanticMappedMatrix(parameters, "Matrices_Texture");
    this.viewProjection = new SemanticMappedMatrix(parameters, "Matrices_ViewProjection");
  }

  public Matrix WorldViewProjection
  {
    set => this.worldViewProjection.Set(value);
  }

  public Matrix WorldInverseTranspose
  {
    set => this.worldInverseTranspose.Set(value);
  }

  public Matrix ViewProjection
  {
    set => this.viewProjection.Set(value);
  }

  public Matrix World
  {
    set => this.world.Set(value);
  }

  public Matrix TextureMatrix
  {
    set => this.textureMatrix.Set(value);
  }
}
