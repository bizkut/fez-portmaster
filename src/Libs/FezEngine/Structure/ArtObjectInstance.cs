// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.ArtObjectInstance
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Services;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Structure;

public class ArtObjectInstance
{
  [Serialization(Ignore = true)]
  public BoundingBox Bounds;
  private static readonly ILevelMaterializer LevelMaterializer;
  private Mesh hostMesh;
  private Vector3 position;
  private Vector3 scale = Vector3.One;
  private Quaternion rotation = Quaternion.Identity;
  private bool forceShading;
  private bool visible = true;
  private bool drawDirty = true;
  private bool boundsDirty = true;
  private string artObjectName;

  [Serialization(Ignore = true)]
  public int Id { get; set; }

  [Serialization(Ignore = true)]
  public bool Enabled { get; set; }

  [Serialization(Ignore = true)]
  public bool Hidden { get; set; }

  [Serialization(Ignore = true)]
  public bool Visible
  {
    get => this.visible;
    set
    {
      this.drawDirty |= this.visible != value;
      this.visible = value;
    }
  }

  [Serialization(Ignore = true)]
  public int InstanceIndex { get; private set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public ArtObjectActorSettings ActorSettings { get; set; }

  static ArtObjectInstance()
  {
    if (!ServiceHelper.IsFull)
      return;
    ArtObjectInstance.LevelMaterializer = ServiceHelper.Get<ILevelMaterializer>();
  }

  public ArtObjectInstance()
  {
    this.ActorSettings = new ArtObjectActorSettings();
    this.Enabled = true;
  }

  public ArtObjectInstance(string artObjectName)
    : this()
  {
    this.artObjectName = artObjectName;
  }

  public ArtObjectInstance(ArtObject artObject)
    : this()
  {
    this.ArtObject = artObject;
  }

  public void Initialize()
  {
    this.hostMesh = ArtObjectInstance.LevelMaterializer.ArtObjectsMesh;
    if (this.ArtObject.Group == null)
    {
      this.ArtObject.Group = this.hostMesh.AddGroup();
      this.ArtObject.Group.Geometry = (IIndexedPrimitiveCollection) this.ArtObject.Geometry;
      DrawActionScheduler.Schedule((Action) (() =>
      {
        if (this.ArtObject == null)
          return;
        if (this.ArtObject.CubemapSony != null && GamepadState.Layout != GamepadState.GamepadLayout.Xbox360)
          this.ArtObject.Group.Texture = (Texture) this.ArtObject.CubemapSony;
        else
          this.ArtObject.Group.Texture = (Texture) this.ArtObject.Cubemap;
      }));
      this.ArtObject.Group.CustomData = (object) new ArtObjectCustomData()
      {
        ArtObject = this.ArtObject
      };
    }
    this.InstanceIndex = this.ArtObject.InstanceCount++;
    this.ArtObject.Geometry.PredictiveBatchSize = 1;
    if (this.ArtObject.Geometry.Instances != null)
      return;
    this.ArtObject.Geometry.Instances = new Matrix[4];
  }

  public void RebuildBounds()
  {
    if (!this.boundsDirty)
      return;
    Vector3 vector3 = this.ArtObject.Size / 2f * this.scale;
    this.Bounds = new BoundingBox(this.position - vector3, this.position + vector3);
    FezMath.RotateOnCenter(ref this.Bounds, ref this.rotation);
    this.boundsDirty = false;
  }

  public void Update()
  {
    if (!this.drawDirty)
      return;
    if (this.ArtObject.Geometry.Instances.Length < this.ArtObject.InstanceCount)
      Array.Resize<Matrix>(ref this.ArtObject.Geometry.Instances, this.ArtObject.InstanceCount + 4);
    this.ArtObject.Geometry.UpdateBuffers();
    Vector3 vector3 = this.visible ? this.position : new Vector3(float.MinValue);
    this.ArtObject.Geometry.Instances[this.InstanceIndex] = new Matrix(vector3.X, vector3.Y, vector3.Z, this.Material == null ? 1f : this.Material.Opacity, this.Scale.X, this.Scale.Y, this.Scale.Z, this.ForceShading ? 1f : 0.0f, this.rotation.X, this.rotation.Y, this.rotation.Z, this.rotation.W, this.ActorSettings.InvisibleSides.Contains(FaceOrientation.Front) ? 1f : 0.0f, this.ActorSettings.InvisibleSides.Contains(FaceOrientation.Right) ? 1f : 0.0f, this.ActorSettings.InvisibleSides.Contains(FaceOrientation.Back) ? 1f : 0.0f, this.ActorSettings.InvisibleSides.Contains(FaceOrientation.Left) ? 1f : 0.0f);
    this.ArtObject.Geometry.InstancesDirty = true;
    this.drawDirty = false;
  }

  public Vector3 Position
  {
    get => this.position;
    set
    {
      this.position = value;
      this.boundsDirty = this.drawDirty = true;
    }
  }

  [Serialization(Optional = true)]
  public Vector3 Scale
  {
    get => this.scale;
    set
    {
      this.scale = value;
      this.boundsDirty = this.drawDirty = true;
    }
  }

  [Serialization(Optional = true)]
  public Quaternion Rotation
  {
    get => this.rotation;
    set
    {
      this.rotation = value;
      this.boundsDirty = this.drawDirty = true;
    }
  }

  [Serialization(Ignore = true)]
  public Material Material { get; set; }

  [Serialization(Ignore = true)]
  public bool ForceShading
  {
    get => this.forceShading;
    set
    {
      this.forceShading = value;
      this.drawDirty = true;
    }
  }

  public void MarkDirty() => this.boundsDirty = this.drawDirty = true;

  public string ArtObjectName
  {
    get => this.ArtObject != null ? this.ArtObject.Name : this.artObjectName;
    set => this.artObjectName = value;
  }

  [Serialization(Ignore = true)]
  public ArtObject ArtObject { get; set; }

  public void Dispose(bool final)
  {
    int num = 0;
    if (!final)
    {
      foreach (ArtObjectInstance levelArtObject in ArtObjectInstance.LevelMaterializer.LevelArtObjects)
      {
        if (levelArtObject != this && levelArtObject.ArtObject == this.ArtObject)
        {
          levelArtObject.InstanceIndex = num++;
          levelArtObject.drawDirty = true;
          levelArtObject.Update();
        }
      }
      ArtObjectInstance.LevelMaterializer.LevelArtObjects.Remove(this);
      this.ArtObject.InstanceCount = num;
    }
    else
      this.ArtObject.InstanceCount = 0;
    if (num != 0 || this.ArtObject.Group == null)
      return;
    this.ArtObject.Geometry.ResetBuffers();
    ArtObjectInstance.LevelMaterializer.ArtObjectsMesh.RemoveGroup(this.ArtObject.Group);
    this.ArtObject.Group = (Group) null;
    this.ArtObject = (ArtObject) null;
  }

  public void Dispose() => this.Dispose(false);

  public void SoftDispose()
  {
    ArtObjectInstance.LevelMaterializer.LevelArtObjects.Remove(this);
    if (this.ArtObject == null)
      return;
    if (this.ArtObject.Group != null)
      ArtObjectInstance.LevelMaterializer.ArtObjectsMesh.RemoveGroup(this.ArtObject.Group, true);
    this.ArtObject.InstanceCount = 0;
    this.ArtObject.Group = (Group) null;
    if (this.ArtObject.Geometry == null)
      return;
    this.ArtObject.Geometry.ResetBuffers();
  }
}
