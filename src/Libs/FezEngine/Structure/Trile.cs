// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Trile
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Structure.Geometry;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class Trile : ITrixelObject
{
  [Serialization(Ignore = true)]
  public int Id { get; set; }

  public string Name { get; set; }

  public string CubemapPath { get; set; }

  [Serialization(Ignore = true)]
  public List<TrileInstance> Instances { get; set; }

  public Dictionary<FaceOrientation, CollisionType> Faces { get; set; }

  [Serialization(Optional = true)]
  public ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> Geometry { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public TrixelCluster MissingTrixels { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public TrileActorSettings ActorSettings { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Immaterial { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool SeeThrough { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Thin { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool ForceHugging { get; set; }

  [Serialization(Ignore = true)]
  public TrileSet TrileSet { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public Vector3 Size { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public Vector3 Offset { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public SurfaceType SurfaceType { get; set; }

  [Serialization(Ignore = true)]
  public Vector2 AtlasOffset { get; set; }

  [Serialization(Ignore = true)]
  public bool ForceKeep { get; set; }

  public Trile()
  {
    this.MissingTrixels = new TrixelCluster();
    this.Instances = new List<TrileInstance>();
    this.ActorSettings = new TrileActorSettings();
    this.Name = "Untitled";
    this.Size = Vector3.One;
    this.Faces = new Dictionary<FaceOrientation, CollisionType>(4, (IEqualityComparer<FaceOrientation>) FaceOrientationComparer.Default);
  }

  public Trile(CollisionType faceType)
    : this()
  {
    this.Faces.Add(FaceOrientation.Back, faceType);
    this.Faces.Add(FaceOrientation.Front, faceType);
    this.Faces.Add(FaceOrientation.Left, faceType);
    this.Faces.Add(FaceOrientation.Right, faceType);
    this.MissingTrixels.OnDeserialization();
  }

  public override string ToString() => this.Name;

  public bool TrixelExists(TrixelEmplacement trixelIdentifier)
  {
    return this.MissingTrixels.Empty || !this.MissingTrixels.IsFilled(trixelIdentifier);
  }

  public bool CanContain(TrixelEmplacement trixel) => Trile.TrixelInRange(trixel);

  public static bool TrixelInRange(TrixelEmplacement trixel)
  {
    return trixel.X < 16 /*0x10*/ && trixel.Y < 16 /*0x10*/ && trixel.Z < 16 /*0x10*/ && trixel.X >= 0 && trixel.Y >= 0 && trixel.Z >= 0;
  }

  public bool IsBorderTrixelFace(TrixelEmplacement id, FaceOrientation face)
  {
    return this.IsBorderTrixelFace(id.GetTraversal(face));
  }

  public bool IsBorderTrixelFace(TrixelEmplacement traversed)
  {
    return !Trile.TrixelInRange(traversed) || !this.TrixelExists(traversed);
  }

  public Trile Clone()
  {
    return new Trile()
    {
      CubemapPath = this.CubemapPath,
      Faces = new Dictionary<FaceOrientation, CollisionType>((IDictionary<FaceOrientation, CollisionType>) this.Faces),
      Id = this.Id,
      Immaterial = this.Immaterial,
      Name = this.Name,
      SeeThrough = this.SeeThrough,
      Thin = this.Thin,
      ForceHugging = this.ForceHugging,
      ActorSettings = new TrileActorSettings(this.ActorSettings),
      SurfaceType = this.SurfaceType
    };
  }

  public void CopyFrom(Trile copy)
  {
    this.ActorSettings = new TrileActorSettings(copy.ActorSettings);
    this.Faces = new Dictionary<FaceOrientation, CollisionType>((IDictionary<FaceOrientation, CollisionType>) copy.Faces);
    this.Immaterial = copy.Immaterial;
    this.Thin = copy.Thin;
    this.Name = copy.Name;
    this.ForceHugging = copy.ForceHugging;
    this.SeeThrough = copy.SeeThrough;
    this.SurfaceType = copy.SurfaceType;
  }

  public void Dispose()
  {
    if (this.Geometry != null)
    {
      this.Geometry.Dispose();
      this.Geometry = (ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4>) null;
    }
    this.TrileSet = (TrileSet) null;
  }
}
