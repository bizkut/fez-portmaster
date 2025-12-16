// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.ArtObject
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class ArtObject : ITrixelObject, IDisposable
{
  public string Name { get; set; }

  public string CubemapPath { get; set; }

  [Serialization(Ignore = true)]
  public Texture2D Cubemap { get; set; }

  public Vector3 Size { get; set; }

  [Serialization(Optional = true)]
  public ActorType ActorType { get; set; }

  [Serialization(Optional = true)]
  public bool NoSihouette { get; set; }

  [Serialization(Optional = true)]
  public TrixelCluster MissingTrixels { get; set; }

  [Serialization(Optional = true, CollectionItemName = "surface")]
  public List<TrixelSurface> TrixelSurfaces { get; set; }

  [Serialization(Optional = true)]
  public ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix> Geometry { get; set; }

  [Serialization(Ignore = true)]
  public ArtObjectMaterializer Materializer { get; set; }

  [Serialization(Ignore = true)]
  public Group Group { get; set; }

  [Serialization(Ignore = true)]
  public int InstanceCount { get; set; }

  public bool TrixelExists(TrixelEmplacement trixelIdentifier)
  {
    return this.MissingTrixels.Empty || !this.MissingTrixels.IsFilled(trixelIdentifier);
  }

  public bool CanContain(TrixelEmplacement trixel)
  {
    return (double) trixel.X < (double) this.Size.X * 16.0 && (double) trixel.Y < (double) this.Size.Y * 16.0 && (double) trixel.Z < (double) this.Size.Z * 16.0 && trixel.X >= 0 && trixel.Y >= 0 && trixel.Z >= 0;
  }

  public bool IsBorderTrixelFace(TrixelEmplacement id, FaceOrientation face)
  {
    return this.IsBorderTrixelFace(id.GetTraversal(face));
  }

  public bool IsBorderTrixelFace(TrixelEmplacement traversed)
  {
    return !this.CanContain(traversed) || !this.TrixelExists(traversed);
  }

  [Serialization(Ignore = true)]
  public Texture2D CubemapSony { get; set; }

  internal void UpdateControllerTexture(object sender, EventArgs e)
  {
    if (GamepadState.Layout == GamepadState.GamepadLayout.Xbox360)
      this.Group.Texture = (Texture) this.Cubemap;
    else
      this.Group.Texture = (Texture) this.CubemapSony;
  }

  public void Dispose()
  {
    if (this.Cubemap != null)
      this.Cubemap.Dispose();
    this.Cubemap = (Texture2D) null;
    if (this.CubemapSony != null)
    {
      GamepadState.OnLayoutChanged -= new EventHandler(this.UpdateControllerTexture);
      this.CubemapSony.Dispose();
      this.CubemapSony = (Texture2D) null;
    }
    if (this.Geometry != null)
      this.Geometry.Dispose();
    this.Geometry = (ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix>) null;
    this.Group = (Group) null;
    this.Materializer = (ArtObjectMaterializer) null;
  }
}
