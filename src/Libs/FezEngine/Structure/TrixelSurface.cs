// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrixelSurface
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization;
using ContentSerialization.Attributes;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure;

public class TrixelSurface : IDeserializationCallback
{
  private Vector3 normal;
  private FaceOrientation tangentFace;
  private FaceOrientation bitangentFace;
  private FaceOrientation[] tangentFaces;
  private int depth;

  public FaceOrientation Orientation { get; set; }

  [Serialization(Name = "parts", CollectionItemName = "part")]
  public List<RectangularTrixelSurfacePart> RectangularParts { get; set; }

  public Vector3 Tangent { get; private set; }

  public Vector3 Bitangent { get; private set; }

  public HashSet<TrixelEmplacement> Trixels { get; private set; }

  public bool Dirty { get; private set; }

  public TrixelSurface()
  {
  }

  public TrixelSurface(FaceOrientation orientation, TrixelEmplacement firstTrixel)
  {
    this.RectangularParts = new List<RectangularTrixelSurfacePart>();
    this.Orientation = orientation;
    this.Initialize();
    this.Trixels.Add(firstTrixel);
    this.MarkAsDirty();
    this.InitializeDepth();
  }

  public void OnDeserialization()
  {
    this.Initialize();
    this.RebuildFromParts();
    this.InitializeDepth();
  }

  private void Initialize()
  {
    this.Trixels = new HashSet<TrixelEmplacement>();
    this.tangentFace = this.Orientation.GetTangent();
    this.bitangentFace = this.Orientation.GetBitangent();
    this.tangentFaces = new FaceOrientation[4]
    {
      this.tangentFace,
      this.bitangentFace,
      this.tangentFace.GetOpposite(),
      this.bitangentFace.GetOpposite()
    };
    this.normal = this.Orientation.AsVector();
    this.Tangent = this.tangentFace.AsVector();
    this.Bitangent = this.bitangentFace.AsVector();
  }

  private void InitializeDepth()
  {
    this.depth = (int) Vector3.Dot(this.Trixels.First<TrixelEmplacement>().Position, this.normal);
  }

  public bool CanContain(TrixelEmplacement trixel, FaceOrientation face)
  {
    return face == this.Orientation && (int) Vector3.Dot(trixel.Position, this.normal) == this.depth && (this.Trixels.Contains(trixel + this.Tangent) || this.Trixels.Contains(trixel + this.Bitangent) || this.Trixels.Contains(trixel - this.Tangent) || this.Trixels.Contains(trixel - this.Bitangent));
  }

  public void MarkAsDirty() => this.Dirty = true;

  public void RebuildFromParts()
  {
    foreach (RectangularTrixelSurfacePart rectangularPart in this.RectangularParts)
    {
      rectangularPart.Orientation = this.Orientation;
      for (int index1 = 0; index1 < rectangularPart.TangentSize; ++index1)
      {
        for (int index2 = 0; index2 < rectangularPart.BitangentSize; ++index2)
          this.Trixels.Add(new TrixelEmplacement(rectangularPart.Start + this.Tangent * (float) index1 + this.Bitangent * (float) index2));
      }
    }
    this.MarkAsDirty();
  }

  public void RebuildParts()
  {
    this.Dirty = false;
    this.RectangularParts.Clear();
    Queue<HashSet<TrixelEmplacement>> trixelEmplacementSetQueue = new Queue<HashSet<TrixelEmplacement>>();
    if (this.Trixels.Count > 0)
      trixelEmplacementSetQueue.Enqueue(new HashSet<TrixelEmplacement>((IEnumerable<TrixelEmplacement>) this.Trixels));
    while (trixelEmplacementSetQueue.Count > 0)
    {
      HashSet<TrixelEmplacement> trixelEmplacementSet = trixelEmplacementSetQueue.Dequeue();
      TrixelEmplacement center = new TrixelEmplacement();
      foreach (TrixelEmplacement trixelEmplacement in trixelEmplacementSet)
        center.Position += trixelEmplacement.Position;
      center.Position = (center.Position / (float) trixelEmplacementSet.Count).Floor();
      if ((double) Vector3.Dot(center.Position, this.normal) != (double) this.depth)
        center.Position = center.Position * (Vector3.One - this.normal.Abs()) + (float) this.depth * this.normal;
      if (!trixelEmplacementSet.Contains(center))
        center = this.FindNearestTrixel(center, (ICollection<TrixelEmplacement>) trixelEmplacementSet);
      Rectangle rectangle;
      List<TrixelEmplacement> biggestRectangle = this.FindBiggestRectangle(center, (ICollection<TrixelEmplacement>) trixelEmplacementSet, out rectangle);
      rectangle.Offset((int) Vector3.Dot(center.Position, this.Tangent), (int) Vector3.Dot(center.Position, this.Bitangent));
      this.RectangularParts.Add(new RectangularTrixelSurfacePart()
      {
        Orientation = this.Orientation,
        Start = new TrixelEmplacement((float) rectangle.X * this.Tangent + (float) rectangle.Y * this.Bitangent + (float) this.depth * this.normal),
        TangentSize = rectangle.Width,
        BitangentSize = rectangle.Height
      });
      trixelEmplacementSet.ExceptWith((IEnumerable<TrixelEmplacement>) biggestRectangle);
      while (trixelEmplacementSet.Count > 0)
      {
        HashSet<TrixelEmplacement> other = this.TraverseSurface(trixelEmplacementSet.First<TrixelEmplacement>(), (ICollection<TrixelEmplacement>) trixelEmplacementSet);
        trixelEmplacementSetQueue.Enqueue(other);
        if (trixelEmplacementSet.Count == other.Count)
          trixelEmplacementSet.Clear();
        else
          trixelEmplacementSet.ExceptWith((IEnumerable<TrixelEmplacement>) other);
      }
    }
  }

  public bool AnyRectangleContains(TrixelEmplacement trixel)
  {
    foreach (RectangularTrixelSurfacePart rectangularPart in this.RectangularParts)
    {
      Vector3 vector1 = trixel.Position - rectangularPart.Start.Position;
      if ((double) vector1.X >= 0.0 && (double) vector1.Y >= 0.0 && (double) vector1.Z >= 0.0)
      {
        int num1 = (int) Vector3.Dot(vector1, this.Tangent);
        int num2 = (int) Vector3.Dot(vector1, this.Bitangent);
        int tangentSize = rectangularPart.TangentSize;
        if (num1 < tangentSize && num2 < rectangularPart.BitangentSize)
          return true;
      }
    }
    return false;
  }

  private HashSet<TrixelEmplacement> TraverseSurface(
    TrixelEmplacement origin,
    ICollection<TrixelEmplacement> subSurface)
  {
    HashSet<TrixelEmplacement> trixelEmplacementSet = new HashSet<TrixelEmplacement>()
    {
      origin
    };
    Queue<TrixelSurface.TrixelToTraverse> trixelToTraverseQueue = new Queue<TrixelSurface.TrixelToTraverse>();
    trixelToTraverseQueue.Enqueue(new TrixelSurface.TrixelToTraverse()
    {
      Trixel = origin
    });
    while (trixelToTraverseQueue.Count != 0)
    {
      TrixelSurface.TrixelToTraverse toTraverse = trixelToTraverseQueue.Dequeue();
      TrixelEmplacement trixel = toTraverse.Trixel;
      using (IEnumerator<FaceOrientation> enumerator = ((IEnumerable<FaceOrientation>) this.tangentFaces).Where<FaceOrientation>((Func<FaceOrientation, bool>) (x => !toTraverse.Except.HasValue || toTraverse.Except.Value != x)).GetEnumerator())
      {
label_8:
        while (enumerator.MoveNext())
        {
          FaceOrientation current = enumerator.Current;
          TrixelEmplacement traversal = trixel.GetTraversal(current);
          while (true)
          {
            if (!trixelEmplacementSet.Contains(traversal) && subSurface.Contains(traversal))
            {
              trixelEmplacementSet.Add(traversal);
              if (trixelEmplacementSet.Count != subSurface.Count)
              {
                trixelToTraverseQueue.Enqueue(new TrixelSurface.TrixelToTraverse()
                {
                  Trixel = traversal,
                  Except = new FaceOrientation?(current)
                });
                traversal = traversal.GetTraversal(current);
              }
              else
                break;
            }
            else
              goto label_8;
          }
          return trixelEmplacementSet;
        }
      }
    }
    return trixelEmplacementSet;
  }

  private List<TrixelEmplacement> FindBiggestRectangle(
    TrixelEmplacement center,
    ICollection<TrixelEmplacement> subSurface,
    out Rectangle rectangle)
  {
    List<TrixelEmplacement> rectangleTrixels = new List<TrixelEmplacement>();
    TrixelEmplacement other = new TrixelEmplacement(center);
    int num1 = 1;
    int num2 = 0;
    int num3 = 1;
    int num4 = 0;
    int num5 = 1;
    int num6 = -1;
    do
    {
      rectangleTrixels.Add(new TrixelEmplacement(other));
      if (num3 > 0)
      {
        other.Position += this.Tangent * (float) num5;
        if (--num3 == 0)
        {
          num6 *= -1;
          num4 = ++num2;
        }
      }
      else if (num4 > 0)
      {
        other.Position += this.Bitangent * (float) num6;
        if (--num4 == 0)
        {
          num5 *= -1;
          num3 = ++num1;
        }
      }
    }
    while (subSurface.Contains(other));
    int rectangleSpiral = TrixelSurface.ClampToRectangleSpiral(rectangleTrixels.Count);
    if (rectangleSpiral != rectangleTrixels.Count)
      rectangleTrixels.RemoveRange(rectangleSpiral, rectangleTrixels.Count - rectangleSpiral);
    rectangle = TrixelSurface.GetRectangleSpiralLimits(rectangleSpiral);
    if (rectangleTrixels.Count < subSurface.Count)
    {
      this.ExpandSide(ref rectangle, center, subSurface, rectangleTrixels, true, 1);
      this.ExpandSide(ref rectangle, center, subSurface, rectangleTrixels, true, -1);
      this.ExpandSide(ref rectangle, center, subSurface, rectangleTrixels, false, 1);
      this.ExpandSide(ref rectangle, center, subSurface, rectangleTrixels, false, -1);
    }
    return rectangleTrixels;
  }

  private static int ClampToRectangleSpiral(int trixelCount)
  {
    int num1 = (int) Math.Floor(Math.Sqrt((double) trixelCount));
    int num2 = num1 * num1;
    int num3 = num2 + num1;
    return num3 >= trixelCount ? num2 : num3;
  }

  private static Rectangle GetRectangleSpiralLimits(int trixelCount)
  {
    double d = Math.Sqrt((double) trixelCount);
    int num = (int) Math.Floor(d);
    Point point1;
    point1.X = point1.Y = (int) Math.Floor(d / 2.0) + 1;
    Point point2;
    point2.X = point2.Y = (int) Math.Ceiling(-(d - 1.0) / 2.0);
    if ((double) num != d)
    {
      if (d % 2.0 == 0.0)
        --point2.X;
      else
        ++point1.X;
    }
    return new Rectangle(point2.X, point2.Y, point1.X - point2.X, point1.Y - point2.Y);
  }

  private void ExpandSide(
    ref Rectangle rectangle,
    TrixelEmplacement center,
    ICollection<TrixelEmplacement> subSurface,
    List<TrixelEmplacement> rectangleTrixels,
    bool useTangent,
    int sign)
  {
    TrixelEmplacement other1 = center + (float) rectangle.X * this.Tangent + (float) rectangle.Y * this.Bitangent;
    if (sign > 0)
      other1 += useTangent ? this.Tangent * (float) (rectangle.Width - 1) : this.Bitangent * (float) (rectangle.Height - 1);
    int num = useTangent ? rectangle.Height : rectangle.Width;
    bool flag;
    do
    {
      other1.Position += (useTangent ? this.Tangent : this.Bitangent) * (float) sign;
      TrixelEmplacement other2 = new TrixelEmplacement(other1);
      int count = 0;
      for (flag = subSurface.Contains(other2); flag; flag = subSurface.Contains(other2))
      {
        rectangleTrixels.Add(new TrixelEmplacement(other2));
        if (++count != num)
          other2.Position += useTangent ? this.Bitangent : this.Tangent;
        else
          break;
      }
      if (flag)
      {
        if (useTangent)
        {
          if (sign < 0)
            --rectangle.X;
          ++rectangle.Width;
        }
        else
        {
          if (sign < 0)
            --rectangle.Y;
          ++rectangle.Height;
        }
      }
      else if (count > 0)
        rectangleTrixels.RemoveRange(rectangleTrixels.Count - count, count);
    }
    while (flag);
  }

  private TrixelEmplacement FindNearestTrixel(
    TrixelEmplacement center,
    ICollection<TrixelEmplacement> subSurface)
  {
    TrixelEmplacement nearestTrixel = new TrixelEmplacement(center);
    int num1 = 1;
    int num2 = 0;
    int num3 = 1;
    int num4 = 0;
    int num5 = 1;
    int num6 = -1;
    do
    {
      if (num3 > 0)
      {
        nearestTrixel.Position += this.Tangent * (float) num5;
        if (--num3 == 0)
        {
          num6 *= -1;
          num4 = ++num2;
        }
      }
      else if (num4 > 0)
      {
        nearestTrixel.Position += this.Bitangent * (float) num6;
        if (--num4 == 0)
        {
          num5 *= -1;
          num3 = ++num1;
        }
      }
    }
    while (!subSurface.Contains(nearestTrixel));
    return nearestTrixel;
  }

  private struct TrixelToTraverse
  {
    public TrixelEmplacement Trixel;
    public FaceOrientation? Except;
  }
}
