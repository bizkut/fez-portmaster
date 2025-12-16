// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.TrileMaterializer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Tools;

public class TrileMaterializer : IDisposable
{
  public const int InstancesPerBatch = 200;
  private static readonly InvalidTrixelFaceComparer InvalidTrixelFaceComparer = new InvalidTrixelFaceComparer();
  protected ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry;
  protected readonly List<Vector4> tempInstances;
  protected readonly List<TrileInstance> tempInstanceIds;
  private readonly List<TrixelSurface> surfaces;
  private readonly HashSet<TrixelFace> added;
  private readonly HashSet<TrixelFace> removed;
  protected readonly Trile trile;
  protected readonly Group group;
  private static readonly Vector4 OutOfSight = new Vector4(float.MinValue);
  private const int PredictiveBatchSize = 16 /*0x10*/;

  public static bool NoTrixelsMode { get; set; }

  public ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> Geometry
  {
    get => this.geometry;
    set => this.group.Geometry = (IIndexedPrimitiveCollection) (this.geometry = value);
  }

  public TrileMaterializer(Trile trile)
    : this(trile, (Mesh) null)
  {
  }

  public TrileMaterializer(Trile trile, Mesh levelMesh)
    : this(trile, levelMesh, false)
  {
  }

  public TrileMaterializer(Trile trile, Mesh levelMesh, bool mutableSurfaces)
  {
    ServiceHelper.InjectServices((object) this);
    this.trile = trile;
    if (mutableSurfaces)
    {
      this.surfaces = new List<TrixelSurface>();
      this.added = new HashSet<TrixelFace>();
      this.removed = new HashSet<TrixelFace>();
    }
    if (levelMesh == null)
      return;
    this.group = levelMesh.AddGroup();
    this.tempInstances = new List<Vector4>();
    this.tempInstanceIds = new List<TrileInstance>();
    this.group.Geometry = (IIndexedPrimitiveCollection) this.geometry;
  }

  public void Rebuild()
  {
    this.MarkMissingCells();
    this.UpdateSurfaces();
    this.RebuildGeometry();
  }

  public void MarkMissingCells()
  {
    this.added.Clear();
    this.removed.Clear();
    this.InitializeSurfaces();
    if (TrileMaterializer.NoTrixelsMode)
      return;
    this.MarkRemoved(this.trile.MissingTrixels.Cells);
  }

  private void InitializeSurfaces()
  {
    foreach (FaceOrientation faceOrientation in Util.GetValues<FaceOrientation>())
    {
      TrixelEmplacement firstTrixel = new TrixelEmplacement(faceOrientation.IsPositive() ? faceOrientation.AsVector() * (new Vector3(16f) - Vector3.One) : Vector3.Zero);
      TrixelSurface trixelSurface = new TrixelSurface(faceOrientation, firstTrixel);
      Vector3 mask1 = faceOrientation.GetTangent().AsAxis().GetMask();
      int num1 = (int) Vector3.Dot(Vector3.One, mask1) * 16 /*0x10*/;
      Vector3 mask2 = faceOrientation.GetBitangent().AsAxis().GetMask();
      int num2 = (int) Vector3.Dot(Vector3.One, mask2) * 16 /*0x10*/;
      trixelSurface.RectangularParts.Add(new RectangularTrixelSurfacePart()
      {
        Orientation = faceOrientation,
        TangentSize = num1,
        BitangentSize = num2,
        Start = firstTrixel
      });
      for (int index1 = 0; index1 < num1; ++index1)
      {
        for (int index2 = 0; index2 < num2; ++index2)
          trixelSurface.Trixels.Add(new TrixelEmplacement(firstTrixel + mask1 * (float) index1 + mask2 * (float) index2));
      }
      this.surfaces.Add(trixelSurface);
    }
  }

  public void MarkAdded(IEnumerable<TrixelEmplacement> trixels) => this.Invalidate(trixels, true);

  public void MarkRemoved(IEnumerable<TrixelEmplacement> trixels)
  {
    this.Invalidate(trixels, false);
  }

  private void Invalidate(IEnumerable<TrixelEmplacement> trixels, bool trixelExists)
  {
    foreach (TrixelEmplacement trixel1 in trixels)
    {
      TrixelEmplacement trixel = trixel1;
      for (int index = 0; index < 6; ++index)
      {
        FaceOrientation face = (FaceOrientation) index;
        TrixelEmplacement traversed = trixel.GetTraversal(face);
        if (this.Trile.IsBorderTrixelFace(traversed))
        {
          if (this.surfaces.Any<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.Orientation == face && x.Trixels.Contains(trixel))))
            this.removed.Add(new TrixelFace(trixel, face));
          if (trixelExists)
            this.added.Add(new TrixelFace(trixel, face));
        }
        else
        {
          FaceOrientation oppositeFace = face.GetOpposite();
          if (this.surfaces.Any<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.Orientation == oppositeFace && x.Trixels.Contains(traversed))))
            this.removed.Add(new TrixelFace(traversed, oppositeFace));
          if (!trixelExists)
            this.added.Add(new TrixelFace(traversed, oppositeFace));
        }
      }
    }
  }

  public void UpdateSurfaces()
  {
    TrixelFace[] array1 = this.removed.ToArray<TrixelFace>();
    Array.Sort<TrixelFace>(array1, (IComparer<TrixelFace>) TrileMaterializer.InvalidTrixelFaceComparer);
    foreach (TrixelFace trixelFace in array1)
    {
      TrixelFace tf = trixelFace;
      TrixelSurface trixelSurface1 = (TrixelSurface) null;
      foreach (TrixelSurface trixelSurface2 in this.surfaces.Where<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.Orientation == tf.Face && x.Trixels.Contains(tf.Id))))
      {
        trixelSurface2.Trixels.Remove(tf.Id);
        if (trixelSurface2.Trixels.Count == 0)
          trixelSurface1 = trixelSurface2;
        else
          trixelSurface2.MarkAsDirty();
      }
      if (trixelSurface1 != null)
        this.surfaces.Remove(trixelSurface1);
    }
    this.removed.Clear();
    TrixelFace[] array2 = this.added.ToArray<TrixelFace>();
    Array.Sort<TrixelFace>(array2, (IComparer<TrixelFace>) TrileMaterializer.InvalidTrixelFaceComparer);
    foreach (TrixelFace trixelFace in array2)
    {
      TrixelFace tf = trixelFace;
      TrixelSurface[] array3 = this.surfaces.Where<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.CanContain(tf.Id, tf.Face))).ToArray<TrixelSurface>();
      if (array3.Length != 0)
      {
        TrixelSurface trixelSurface3 = array3[0];
        trixelSurface3.Trixels.Add(tf.Id);
        trixelSurface3.MarkAsDirty();
        if (array3.Length > 1)
        {
          foreach (TrixelSurface trixelSurface4 in ((IEnumerable<TrixelSurface>) array3).Skip<TrixelSurface>(1))
          {
            trixelSurface3.Trixels.UnionWith((IEnumerable<TrixelEmplacement>) trixelSurface4.Trixels);
            this.surfaces.Remove(trixelSurface4);
          }
        }
      }
      else
        this.surfaces.Add(new TrixelSurface(tf.Face, tf.Id));
    }
    this.added.Clear();
    foreach (TrixelSurface trixelSurface in this.surfaces.Where<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.Dirty)))
      trixelSurface.RebuildParts();
  }

  public void RebuildGeometry()
  {
    int capacity = this.surfaces.Sum<TrixelSurface>((Func<TrixelSurface, int>) (x => x.RectangularParts.Count));
    VertexGroup<VertexPositionNormalTextureInstance> vertexGroup = new VertexGroup<VertexPositionNormalTextureInstance>(capacity * 4);
    Dictionary<RectangularTrixelSurfacePart, FaceMaterialization<VertexPositionNormalTextureInstance>> dictionary = new Dictionary<RectangularTrixelSurfacePart, FaceMaterialization<VertexPositionNormalTextureInstance>>(capacity);
    Vector3 vector3_1 = new Vector3(0.5f);
    foreach (RectangularTrixelSurfacePart key in this.surfaces.SelectMany<TrixelSurface, RectangularTrixelSurfacePart>((Func<TrixelSurface, IEnumerable<RectangularTrixelSurfacePart>>) (x => (IEnumerable<RectangularTrixelSurfacePart>) x.RectangularParts)))
    {
      Vector3 normal = key.Orientation.AsVector();
      Vector3 vector3_2 = key.Orientation.GetTangent().AsVector() * (float) key.TangentSize / 16f;
      Vector3 vector3_3 = key.Orientation.GetBitangent().AsVector() * (float) key.BitangentSize / 16f;
      Vector3 position = key.Start.Position / 16f + (key.Orientation >= FaceOrientation.Right ? 1f : 0.0f) * normal / 16f - vector3_1;
      if (!dictionary.ContainsKey(key))
      {
        FaceMaterialization<VertexPositionNormalTextureInstance> faceMaterialization = new FaceMaterialization<VertexPositionNormalTextureInstance>()
        {
          V0 = vertexGroup.Reference(new VertexPositionNormalTextureInstance(position, normal)),
          V1 = vertexGroup.Reference(new VertexPositionNormalTextureInstance(position + vector3_2, normal)),
          V2 = vertexGroup.Reference(new VertexPositionNormalTextureInstance(position + vector3_2 + vector3_3, normal)),
          V3 = vertexGroup.Reference(new VertexPositionNormalTextureInstance(position + vector3_3, normal))
        };
        faceMaterialization.SetupIndices(key.Orientation);
        dictionary.Add(key, faceMaterialization);
      }
    }
    VertexPositionNormalTextureInstance[] normalTextureInstanceArray = new VertexPositionNormalTextureInstance[vertexGroup.Vertices.Count];
    int index = 0;
    foreach (SharedVertex<VertexPositionNormalTextureInstance> vertex in (IEnumerable<SharedVertex<VertexPositionNormalTextureInstance>>) vertexGroup.Vertices)
    {
      normalTextureInstanceArray[index] = vertex.Vertex;
      normalTextureInstanceArray[index].TextureCoordinate = normalTextureInstanceArray[index].ComputeTexCoord<VertexPositionNormalTextureInstance>() * (this.EngineState == null || !this.EngineState.InEditor ? Vector2.One : new Vector2(1.33333337f, 1f));
      vertex.Index = index++;
    }
    int[] numArray = new int[dictionary.Count * 6];
    int num = 0;
    foreach (FaceMaterialization<VertexPositionNormalTextureInstance> faceMaterialization in dictionary.Values)
    {
      for (ushort relativeIndex = 0; relativeIndex < (ushort) 6; ++relativeIndex)
        numArray[num++] = faceMaterialization.GetIndex(relativeIndex);
    }
    if (this.geometry == null)
    {
      this.geometry = new ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4>(PrimitiveType.TriangleList, 200, true);
      if (this.group != null)
        this.group.Geometry = (IIndexedPrimitiveCollection) this.geometry;
    }
    this.geometry.Vertices = normalTextureInstanceArray;
    this.geometry.Indices = numArray;
    this.geometry.InstancesDirty = true;
    this.DetermineFlags();
  }

  public void DetermineFlags()
  {
    if (this.group == null)
      return;
    ActorType type = this.trile.ActorSettings.Type;
    this.group.CustomData = (object) new TrileCustomData()
    {
      Unstable = (type == ActorType.GoldenCube),
      TiltTwoAxis = (type == ActorType.CubeShard || type == ActorType.SecretCube || type == ActorType.PieceOfHeart),
      Shiny = (type == ActorType.CubeShard || type == ActorType.SkeletonKey || type == ActorType.SecretCube || type == ActorType.PieceOfHeart)
    };
    (this.group.CustomData as TrileCustomData).DetermineCustom();
  }

  public bool BatchNeedsCommit { private set; get; }

  public void ClearBatch()
  {
    this.tempInstances.Clear();
    this.tempInstanceIds.Clear();
    this.tempInstances.TrimExcess();
    this.tempInstanceIds.TrimExcess();
    if (this.geometry == null)
      return;
    this.geometry.ResetBuffers();
  }

  public void ResetBatch()
  {
    this.BatchNeedsCommit = true;
    foreach (TrileInstance tempInstanceId in this.tempInstanceIds)
      tempInstanceId.InstanceId = -1;
    this.tempInstances.Clear();
    this.tempInstanceIds.Clear();
  }

  public void AddToBatch(TrileInstance instance)
  {
    this.BatchNeedsCommit = true;
    instance.InstanceId = this.tempInstances.Count;
    this.tempInstances.Add(instance.Enabled ? instance.Data.PositionPhi : TrileMaterializer.OutOfSight);
    this.tempInstanceIds.Add(instance);
  }

  public void RemoveFromBatch(TrileInstance instance)
  {
    int instanceId = instance.InstanceId;
    if (instance != this.tempInstanceIds[instanceId])
    {
      int num;
      while ((num = this.tempInstanceIds.IndexOf(instance)) != -1)
      {
        instance.InstanceId = num;
        this.RemoveFromBatch(instance);
      }
    }
    else
    {
      this.BatchNeedsCommit = true;
      for (int index = instanceId + 1; index < this.tempInstanceIds.Count; ++index)
      {
        TrileInstance tempInstanceId = this.tempInstanceIds[index];
        if (tempInstanceId.InstanceId >= 0)
          --tempInstanceId.InstanceId;
      }
      this.tempInstances.RemoveAt(instanceId);
      this.tempInstanceIds.RemoveAt(instanceId);
      instance.InstanceId = -1;
    }
  }

  public void CommitBatch()
  {
    if (this.geometry == null)
      return;
    this.BatchNeedsCommit = false;
    int length = (int) Math.Ceiling((double) this.tempInstances.Count / 16.0) * 16 /*0x10*/;
    if (this.geometry.Instances == null || length > this.geometry.Instances.Length)
      this.geometry.Instances = new Vector4[length];
    this.tempInstances.CopyTo(this.geometry.Instances, 0);
    this.geometry.InstanceCount = this.tempInstances.Count;
    this.geometry.InstancesDirty = true;
    this.geometry.UpdateBuffers();
  }

  public void UpdateInstance(TrileInstance instance)
  {
    int instanceId = instance.InstanceId;
    if (instanceId == -1 || this.geometry == null)
      return;
    Vector4 vector4 = instance.Enabled ? instance.Data.PositionPhi : TrileMaterializer.OutOfSight;
    if (instanceId < this.geometry.Instances.Length)
    {
      this.geometry.Instances[instanceId] = vector4;
      this.geometry.InstancesDirty = true;
    }
    if (instanceId >= this.tempInstances.Count)
      return;
    this.tempInstances[instanceId] = vector4;
  }

  public void FakeUpdate(int instanceId, Vector4 data)
  {
    if (instanceId == -1 || this.geometry.Instances == null || instanceId >= this.geometry.Instances.Length)
      return;
    this.geometry.Instances[instanceId] = data;
    this.geometry.InstancesDirty = true;
  }

  public Group Group => this.group;

  internal Trile Trile => this.trile;

  public IEnumerable<TrixelSurface> TrixelSurfaces => (IEnumerable<TrixelSurface>) this.surfaces;

  public void Dispose()
  {
    this.ClearBatch();
    this.group.Mesh.RemoveGroup(this.group, true);
  }

  [ServiceDependency(Optional = true)]
  public IContentManagerProvider CMProvider { protected get; set; }

  [ServiceDependency(Optional = true)]
  public ILevelManager LevelManager { protected get; set; }

  [ServiceDependency(Optional = true)]
  public ILevelMaterializer LevelMaterializer { protected get; set; }

  [ServiceDependency(Optional = true)]
  public IDefaultCameraManager CameraManager { protected get; set; }

  [ServiceDependency(Optional = true)]
  public IDebuggingBag DebuggingBag { protected get; set; }

  [ServiceDependency(Optional = true)]
  public IEngineStateManager EngineState { protected get; set; }
}
