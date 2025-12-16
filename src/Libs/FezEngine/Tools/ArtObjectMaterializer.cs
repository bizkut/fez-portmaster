// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.ArtObjectMaterializer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable disable
namespace FezEngine.Tools;

public class ArtObjectMaterializer
{
  public const int InstancesPerBatch = 60;
  private static readonly InvalidTrixelFaceComparer invalidTrixelFaceComparer = new InvalidTrixelFaceComparer();
  private readonly HashSet<TrixelFace> added;
  private readonly HashSet<TrixelFace> removed;
  private readonly ArtObject artObject;
  private readonly TrixelCluster missingTrixels;
  private readonly Vector3 size;
  private readonly List<TrixelSurface> surfaces;
  private ArtObjectMaterializer.InvalidationContext otherThreadContext;

  public bool Dirty { get; set; }

  public ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix> Geometry { get; set; }

  public ArtObjectMaterializer(
    TrixelCluster missingTrixels,
    List<TrixelSurface> surfaces,
    Vector3 size)
  {
    this.missingTrixels = missingTrixels;
    ServiceHelper.InjectServices((object) this);
    this.added = new HashSet<TrixelFace>();
    this.removed = new HashSet<TrixelFace>();
    this.surfaces = surfaces ?? new List<TrixelSurface>();
    this.size = size;
  }

  public ArtObjectMaterializer(ArtObject artObject)
  {
    this.artObject = artObject;
    ServiceHelper.InjectServices((object) this);
    artObject.Materializer = this;
    this.size = artObject.Size;
    if (artObject.MissingTrixels == null)
      return;
    this.added = new HashSet<TrixelFace>();
    this.removed = new HashSet<TrixelFace>();
    this.missingTrixels = artObject.MissingTrixels;
    if (artObject.TrixelSurfaces == null)
      artObject.TrixelSurfaces = this.surfaces = new List<TrixelSurface>();
    else
      this.surfaces = artObject.TrixelSurfaces;
  }

  public void Rebuild() => this.Rebuild(false);

  public void Rebuild(bool force)
  {
    if (force || this.surfaces.Count == 0)
    {
      this.MarkMissingCells();
      this.UpdateSurfaces();
    }
    this.RebuildGeometry();
  }

  public void Update()
  {
    this.UpdateSurfaces();
    this.RebuildGeometry();
  }

  public void MarkMissingCells()
  {
    this.added.Clear();
    this.removed.Clear();
    this.InitializeSurfaces();
    this.MarkRemoved(this.missingTrixels.Cells);
  }

  public void MarkAdded(IEnumerable<TrixelEmplacement> trixels) => this.Invalidate(trixels, true);

  public void MarkRemoved(IEnumerable<TrixelEmplacement> trixels)
  {
    this.Invalidate(trixels, false);
  }

  private void Invalidate(IEnumerable<TrixelEmplacement> trixels, bool trixelsExist)
  {
    int num = trixels.Count<TrixelEmplacement>();
    ArtObjectMaterializer.InvalidationContext context = new ArtObjectMaterializer.InvalidationContext()
    {
      Trixels = trixels.Take<TrixelEmplacement>(num / 2),
      TrixelsExist = trixelsExist,
      Added = (ICollection<TrixelFace>) new List<TrixelFace>(),
      Removed = (ICollection<TrixelFace>) new List<TrixelFace>()
    };
    this.otherThreadContext = new ArtObjectMaterializer.InvalidationContext()
    {
      Trixels = trixels.Skip<TrixelEmplacement>(num / 2),
      TrixelsExist = trixelsExist,
      Added = (ICollection<TrixelFace>) new List<TrixelFace>(),
      Removed = (ICollection<TrixelFace>) new List<TrixelFace>()
    };
    Thread thread = new Thread(new ThreadStart(this.InvalidateOtherThread));
    thread.Start();
    this.Invalidate(context);
    thread.Join();
    this.added.UnionWith((IEnumerable<TrixelFace>) context.Added);
    this.added.UnionWith((IEnumerable<TrixelFace>) this.otherThreadContext.Added);
    this.removed.UnionWith((IEnumerable<TrixelFace>) context.Removed);
    this.removed.UnionWith((IEnumerable<TrixelFace>) this.otherThreadContext.Removed);
    this.Dirty = true;
  }

  private void InvalidateOtherThread() => this.Invalidate(this.otherThreadContext);

  private bool TrixelExists(TrixelEmplacement trixelIdentifier)
  {
    return this.missingTrixels.Empty || !this.missingTrixels.IsFilled(trixelIdentifier);
  }

  private bool CanContain(TrixelEmplacement trixel)
  {
    return (double) trixel.X < (double) this.size.X * 16.0 && (double) trixel.Y < (double) this.size.Y * 16.0 && (double) trixel.Z < (double) this.size.Z * 16.0 && trixel.X >= 0 && trixel.Y >= 0 && trixel.Z >= 0;
  }

  private bool IsBorderTrixelFace(TrixelEmplacement traversed)
  {
    return !this.CanContain(traversed) || !this.TrixelExists(traversed);
  }

  private void Invalidate(ArtObjectMaterializer.InvalidationContext context)
  {
    foreach (TrixelEmplacement trixel1 in context.Trixels)
    {
      TrixelEmplacement trixel = trixel1;
      for (int index = 0; index < 6; ++index)
      {
        FaceOrientation face = (FaceOrientation) index;
        TrixelEmplacement traversed = trixel.GetTraversal(face);
        if (this.IsBorderTrixelFace(traversed))
        {
          if (this.surfaces.Any<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.Orientation == face && x.AnyRectangleContains(trixel))))
            context.Removed.Add(new TrixelFace(trixel, face));
          if (context.TrixelsExist)
            context.Added.Add(new TrixelFace(trixel, face));
        }
        else
        {
          FaceOrientation oppositeFace = face.GetOpposite();
          if (this.surfaces.Any<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.Orientation == oppositeFace && x.AnyRectangleContains(traversed))))
            context.Removed.Add(new TrixelFace(traversed, oppositeFace));
          if (!context.TrixelsExist)
            context.Added.Add(new TrixelFace(traversed, oppositeFace));
        }
      }
    }
  }

  public void UpdateSurfaces()
  {
    TrixelFace[] array1 = this.removed.ToArray<TrixelFace>();
    Array.Sort<TrixelFace>(array1, (IComparer<TrixelFace>) ArtObjectMaterializer.invalidTrixelFaceComparer);
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
    Array.Sort<TrixelFace>(array2, (IComparer<TrixelFace>) ArtObjectMaterializer.invalidTrixelFaceComparer);
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
    this.RebuildParts();
  }

  private void RebuildParts()
  {
    foreach (TrixelSurface trixelSurface in this.surfaces.Where<TrixelSurface>((Func<TrixelSurface, bool>) (x => x.Dirty)))
      trixelSurface.RebuildParts();
  }

  private void InitializeSurfaces()
  {
    this.surfaces.Clear();
    foreach (FaceOrientation faceOrientation in Util.GetValues<FaceOrientation>())
    {
      TrixelEmplacement firstTrixel = new TrixelEmplacement(faceOrientation.IsPositive() ? faceOrientation.AsVector() * (this.size * 16f - Vector3.One) : Vector3.Zero);
      TrixelSurface trixelSurface = new TrixelSurface(faceOrientation, firstTrixel);
      Vector3 mask1 = faceOrientation.GetTangent().AsAxis().GetMask();
      int num1 = (int) Vector3.Dot(this.size, mask1) * 16 /*0x10*/;
      Vector3 mask2 = faceOrientation.GetBitangent().AsAxis().GetMask();
      int num2 = (int) Vector3.Dot(this.size, mask2) * 16 /*0x10*/;
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

  public void RebuildGeometry()
  {
    if (this.surfaces == null)
      return;
    if (this.Geometry == null)
      this.Geometry = new ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix>(PrimitiveType.TriangleList, 60);
    int capacity = this.surfaces.Sum<TrixelSurface>((Func<TrixelSurface, int>) (x => x.RectangularParts.Count));
    Dictionary<RectangularTrixelSurfacePart, FaceMaterialization<VertexPositionNormalTextureInstance>> dictionary = new Dictionary<RectangularTrixelSurfacePart, FaceMaterialization<VertexPositionNormalTextureInstance>>(capacity * 4);
    VertexGroup<VertexPositionNormalTextureInstance> vertexGroup = new VertexGroup<VertexPositionNormalTextureInstance>(capacity);
    Vector3 vector3_1 = this.size / 2f;
    foreach (RectangularTrixelSurfacePart key in this.surfaces.SelectMany<TrixelSurface, RectangularTrixelSurfacePart>((Func<TrixelSurface, IEnumerable<RectangularTrixelSurfacePart>>) (x => (IEnumerable<RectangularTrixelSurfacePart>) x.RectangularParts)))
    {
      if (!dictionary.ContainsKey(key))
      {
        Vector3 normal = key.Orientation.AsVector();
        Vector3 vector3_2 = key.Orientation.GetTangent().AsVector() * (float) key.TangentSize / 16f;
        Vector3 vector3_3 = key.Orientation.GetBitangent().AsVector() * (float) key.BitangentSize / 16f;
        Vector3 v = key.Start.Position / 16f + (key.Orientation.IsPositive() ? 1f : 0.0f) * normal / 16f - vector3_1;
        FaceMaterialization<VertexPositionNormalTextureInstance> faceMaterialization = new FaceMaterialization<VertexPositionNormalTextureInstance>()
        {
          V0 = vertexGroup.Reference(new VertexPositionNormalTextureInstance(v.Round(4), normal)),
          V1 = vertexGroup.Reference(new VertexPositionNormalTextureInstance((v + vector3_2).Round(4), normal)),
          V2 = vertexGroup.Reference(new VertexPositionNormalTextureInstance((v + vector3_2 + vector3_3).Round(4), normal)),
          V3 = vertexGroup.Reference(new VertexPositionNormalTextureInstance((v + vector3_3).Round(4), normal))
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
      normalTextureInstanceArray[index].TextureCoordinate = normalTextureInstanceArray[index].ComputeTexCoord<VertexPositionNormalTextureInstance>(this.size) * new Vector2(1.33333337f, 1f);
      vertex.Index = index++;
    }
    int[] numArray = new int[dictionary.Count * 6];
    int num = 0;
    foreach (FaceMaterialization<VertexPositionNormalTextureInstance> faceMaterialization in dictionary.Values)
    {
      for (ushort relativeIndex = 0; relativeIndex < (ushort) 6; ++relativeIndex)
        numArray[num++] = faceMaterialization.GetIndex(relativeIndex);
    }
    this.Geometry.Vertices = normalTextureInstanceArray;
    this.Geometry.Indices = numArray;
    if (this.artObject == null)
      return;
    this.PostInitialize();
  }

  public void PostInitialize()
  {
    if (this.CMProvider != null && this.artObject.CubemapPath != null && this.artObject.Cubemap == null)
      this.artObject.Cubemap = this.CMProvider.CurrentLevel.Load<Texture2D>("Art Objects/" + this.artObject.CubemapPath);
    else if (this.CMProvider != null && this.artObject.Name == "SEWER_QR_CUBEAO")
    {
      DrawActionScheduler.Schedule((Action) (() => this.artObject.CubemapSony = this.CMProvider.CurrentLevel.Load<ArtObject>("Art Objects/SEWER_QR_CUBE_SONYAO").Cubemap));
      GamepadState.OnLayoutChanged += new EventHandler(this.artObject.UpdateControllerTexture);
    }
    if (this.artObject.Geometry != null || this.Geometry == null)
      return;
    this.artObject.Geometry = this.Geometry;
  }

  public void RecomputeTexCoords(bool widen)
  {
    for (int index = 0; index < this.artObject.Geometry.Vertices.Length; ++index)
      this.artObject.Geometry.Vertices[index].TextureCoordinate = this.artObject.Geometry.Vertices[index].ComputeTexCoord<VertexPositionNormalTextureInstance>(this.artObject.Size) * (!widen ? Vector2.One : new Vector2(1.33333337f, 1f));
  }

  [ServiceDependency(Optional = true)]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency(Optional = true)]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency(Optional = true)]
  public ILevelManager LevelManager { protected get; set; }

  private struct InvalidationContext
  {
    public IEnumerable<TrixelEmplacement> Trixels;
    public bool TrixelsExist;
    public ICollection<TrixelFace> Added;
    public ICollection<TrixelFace> Removed;
  }
}
