// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrixelCluster
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

public class TrixelCluster : ISpatialStructure<TrixelEmplacement>, IDeserializationCallback
{
  private static readonly Vector3[] Directions = new Vector3[6]
  {
    Vector3.Up,
    Vector3.Down,
    Vector3.Left,
    Vector3.Right,
    Vector3.Forward,
    Vector3.Backward
  };
  private List<TrixelCluster.Box> deserializedBoxes;
  private List<TrixelEmplacement> deserializedOrphans;

  public List<TrixelCluster.Chunk> Chunks { get; private set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public List<TrixelCluster.Box> Boxes
  {
    get
    {
      return this.deserializedBoxes ?? this.Chunks.SelectMany<TrixelCluster.Chunk, TrixelCluster.Box>((Func<TrixelCluster.Chunk, IEnumerable<TrixelCluster.Box>>) (c => (IEnumerable<TrixelCluster.Box>) c.Boxes)).ToList<TrixelCluster.Box>();
    }
    set => this.deserializedBoxes = value;
  }

  [Serialization(Optional = true, CollectionItemName = "content", DefaultValueOptional = true)]
  public List<TrixelEmplacement> Orphans
  {
    get
    {
      return this.deserializedOrphans ?? this.Chunks.SelectMany<TrixelCluster.Chunk, TrixelEmplacement>((Func<TrixelCluster.Chunk, IEnumerable<TrixelEmplacement>>) (c => (IEnumerable<TrixelEmplacement>) c.Trixels)).ToList<TrixelEmplacement>();
    }
    set => this.deserializedOrphans = value;
  }

  public void OnDeserialization()
  {
    if (this.deserializedOrphans != null)
    {
      foreach (TrixelEmplacement deserializedOrphan in this.deserializedOrphans)
      {
        TrixelEmplacement trixel = deserializedOrphan;
        TrixelCluster.Chunk chunk = this.Chunks.FirstOrDefault<TrixelCluster.Chunk>((Func<TrixelCluster.Chunk, bool>) (c => c.IsNeighbor(trixel)));
        if (chunk == null)
          this.Chunks.Add(chunk = new TrixelCluster.Chunk());
        chunk.Trixels.Add(trixel);
      }
      this.deserializedOrphans = (List<TrixelEmplacement>) null;
    }
    if (this.deserializedBoxes == null)
      return;
    foreach (TrixelCluster.Box deserializedBox in this.deserializedBoxes)
    {
      TrixelCluster.Box box = deserializedBox;
      TrixelCluster.Chunk chunk = this.Chunks.FirstOrDefault<TrixelCluster.Chunk>((Func<TrixelCluster.Chunk, bool>) (c => c.IsNeighbor(box)));
      if (chunk == null)
        this.Chunks.Add(chunk = new TrixelCluster.Chunk());
      chunk.Boxes.Add(box);
    }
    this.deserializedBoxes = (List<TrixelCluster.Box>) null;
  }

  public TrixelCluster() => this.Chunks = new List<TrixelCluster.Chunk>();

  public bool Empty => this.Chunks.Count == 0;

  public void Clear() => this.Chunks.Clear();

  public void Fill(TrixelEmplacement trixel)
  {
    this.Fill(Enumerable.Repeat<TrixelEmplacement>(trixel, 1));
  }

  public void Fill(IEnumerable<TrixelEmplacement> trixels)
  {
    foreach (TrixelEmplacement trixel1 in trixels)
    {
      TrixelEmplacement trixel = trixel1;
      if (!this.Chunks.Any<TrixelCluster.Chunk>((Func<TrixelCluster.Chunk, bool>) (c => c.TryAdd(trixel))))
      {
        TrixelCluster.Chunk chunk = new TrixelCluster.Chunk();
        this.Chunks.Add(chunk);
        chunk.Trixels.Add(trixel);
      }
    }
    this.ConsolidateTrixels();
  }

  public void FillAsChunk(IEnumerable<TrixelEmplacement> trixels)
  {
    TrixelEmplacement firstTrixel = trixels.First<TrixelEmplacement>();
    TrixelCluster.Chunk chunk = this.Chunks.FirstOrDefault<TrixelCluster.Chunk>((Func<TrixelCluster.Chunk, bool>) (c => c.TryAdd(firstTrixel)));
    if (chunk == null)
    {
      chunk = new TrixelCluster.Chunk();
      this.Chunks.Add(chunk);
    }
    chunk.Trixels.UnionWith(trixels);
    chunk.ConsolidateTrixels();
  }

  public void Free(TrixelEmplacement trixel)
  {
    this.Free(Enumerable.Repeat<TrixelEmplacement>(trixel, 1));
  }

  public void Free(IEnumerable<TrixelEmplacement> trixels)
  {
    foreach (TrixelEmplacement trixel1 in trixels)
    {
      TrixelEmplacement trixel = trixel1;
      this.Chunks.Any<TrixelCluster.Chunk>((Func<TrixelCluster.Chunk, bool>) (c => c.TryRemove(trixel)));
    }
    this.ConsolidateTrixels();
  }

  public void ConsolidateTrixels()
  {
    this.Chunks.RemoveAll((Predicate<TrixelCluster.Chunk>) (c => c.Empty));
    foreach (TrixelCluster.Chunk chunk in this.Chunks.Where<TrixelCluster.Chunk>((Func<TrixelCluster.Chunk, bool>) (c => c.Dirty)))
      chunk.ConsolidateTrixels();
  }

  public bool IsFilled(TrixelEmplacement trixel)
  {
    for (int index = 0; index < this.Chunks.Count; ++index)
    {
      if (this.Chunks[index].Contains(trixel))
        return true;
    }
    return false;
  }

  public IEnumerable<TrixelEmplacement> Cells
  {
    get
    {
      return this.Chunks.SelectMany<TrixelCluster.Chunk, TrixelEmplacement>((Func<TrixelCluster.Chunk, IEnumerable<TrixelEmplacement>>) (c => c.Trixels.Concat<TrixelEmplacement>(c.Boxes.SelectMany<TrixelCluster.Box, TrixelEmplacement>((Func<TrixelCluster.Box, IEnumerable<TrixelEmplacement>>) (b => b.Cells)))));
    }
  }

  public class Chunk
  {
    [Serialization(Optional = true)]
    public List<TrixelCluster.Box> Boxes { get; set; }

    [Serialization(Optional = true, CollectionItemName = "content")]
    public HashSet<TrixelEmplacement> Trixels { get; set; }

    [Serialization(Ignore = true)]
    internal bool Dirty { get; set; }

    public Chunk()
    {
      this.Boxes = new List<TrixelCluster.Box>();
      this.Trixels = new HashSet<TrixelEmplacement>();
    }

    internal bool IsNeighbor(TrixelCluster.Box box)
    {
      return this.Boxes.Any<TrixelCluster.Box>((Func<TrixelCluster.Box, bool>) (b => b.IsNeighbor(box))) || this.Trixels.Any<TrixelEmplacement>(new Func<TrixelEmplacement, bool>(box.IsNeighbor));
    }

    internal bool IsNeighbor(TrixelEmplacement trixel)
    {
      return this.Boxes.Any<TrixelCluster.Box>((Func<TrixelCluster.Box, bool>) (b => b.IsNeighbor(trixel))) || this.Trixels.Any<TrixelEmplacement>((Func<TrixelEmplacement, bool>) (t => t.IsNeighbor(trixel)));
    }

    internal bool Contains(TrixelEmplacement trixel)
    {
      for (int index = 0; index < this.Boxes.Count; ++index)
      {
        if (this.Boxes[index].Contains(trixel))
          return true;
      }
      return this.Trixels.Contains(trixel);
    }

    internal bool TryAdd(TrixelEmplacement trixel)
    {
      bool flag = false;
      if (this.Boxes.Count > 0)
      {
        foreach (TrixelCluster.Box box in this.Boxes.Where<TrixelCluster.Box>((Func<TrixelCluster.Box, bool>) (b => b.IsNeighbor(trixel))).ToArray<TrixelCluster.Box>())
        {
          flag = true;
          this.Dismantle(box);
          this.Boxes.Remove(box);
        }
      }
      if (!flag)
      {
        foreach (TrixelEmplacement trixel1 in this.Trixels)
        {
          if (trixel1.IsNeighbor(trixel))
          {
            flag = true;
            break;
          }
        }
      }
      if (flag)
        this.Trixels.Add(trixel);
      this.Dirty |= flag;
      return flag;
    }

    internal bool TryRemove(TrixelEmplacement trixel)
    {
      bool flag1 = false;
      foreach (TrixelCluster.Box box in this.Boxes.Where<TrixelCluster.Box>((Func<TrixelCluster.Box, bool>) (b => b.Contains(trixel) || b.IsNeighbor(trixel))))
      {
        flag1 = true;
        this.Dismantle(box);
      }
      if (flag1)
        this.Boxes.RemoveAll((Predicate<TrixelCluster.Box>) (b => b.Contains(trixel) || b.IsNeighbor(trixel)));
      bool flag2 = flag1 | this.Trixels.Remove(trixel);
      this.Dirty |= flag2;
      return flag2;
    }

    private void Dismantle(TrixelCluster.Box box)
    {
      for (int x = box.Start.X; x < box.End.X; ++x)
      {
        for (int y = box.Start.Y; y < box.End.Y; ++y)
        {
          for (int z = box.Start.Z; z < box.End.Z; ++z)
            this.Trixels.Add(new TrixelEmplacement(x, y, z));
        }
      }
    }

    internal bool Empty => this.Boxes.Count == 0 && this.Trixels.Count == 0;

    internal void ConsolidateTrixels()
    {
      if (this.Trixels.Count <= 1)
        return;
      Stack<HashSet<TrixelEmplacement>> trixelEmplacementSetStack = new Stack<HashSet<TrixelEmplacement>>();
      trixelEmplacementSetStack.Push(new HashSet<TrixelEmplacement>((IEnumerable<TrixelEmplacement>) this.Trixels));
      while (trixelEmplacementSetStack.Count > 0)
      {
        HashSet<TrixelEmplacement> trixelEmplacementSet = trixelEmplacementSetStack.Pop();
        TrixelEmplacement center = new TrixelEmplacement();
        foreach (TrixelEmplacement trixelEmplacement in trixelEmplacementSet)
          center.Offset(trixelEmplacement.X, trixelEmplacement.Y, trixelEmplacement.Z);
        center.Position = (center.Position / (float) trixelEmplacementSet.Count).Round(3).Floor();
        if (!trixelEmplacementSet.Contains(center))
          center = trixelEmplacementSet.First<TrixelEmplacement>();
        TrixelCluster.Box box;
        List<TrixelEmplacement> biggestBox = TrixelCluster.Chunk.FindBiggestBox(center, (ICollection<TrixelEmplacement>) trixelEmplacementSet, out box);
        this.Boxes.Add(box);
        trixelEmplacementSet.ExceptWith((IEnumerable<TrixelEmplacement>) biggestBox);
        this.Trixels.ExceptWith((IEnumerable<TrixelEmplacement>) biggestBox);
        while (trixelEmplacementSet.Count > 1)
        {
          HashSet<TrixelEmplacement> other = TrixelCluster.Chunk.VisitChunk(trixelEmplacementSet.First<TrixelEmplacement>(), (ICollection<TrixelEmplacement>) trixelEmplacementSet);
          trixelEmplacementSetStack.Push(other);
          trixelEmplacementSet.ExceptWith((IEnumerable<TrixelEmplacement>) other);
        }
      }
      this.Dirty = false;
    }

    private static List<TrixelEmplacement> FindBiggestBox(
      TrixelEmplacement center,
      ICollection<TrixelEmplacement> subChunk,
      out TrixelCluster.Box box)
    {
      List<TrixelEmplacement> boxTrixels = new List<TrixelEmplacement>()
      {
        center
      };
      box = new TrixelCluster.Box()
      {
        Start = center,
        End = center
      };
      int trixelsToRollback;
      do
      {
        box.Start -= Vector3.One;
        box.End += Vector3.One;
        trixelsToRollback = 0;
      }
      while (TrixelCluster.Chunk.TestFace(subChunk, (ICollection<TrixelEmplacement>) boxTrixels, Vector3.UnitZ, false, box, ref trixelsToRollback) && TrixelCluster.Chunk.TestFace(subChunk, (ICollection<TrixelEmplacement>) boxTrixels, -Vector3.UnitZ, false, box, ref trixelsToRollback) && TrixelCluster.Chunk.TestFace(subChunk, (ICollection<TrixelEmplacement>) boxTrixels, Vector3.UnitX, true, box, ref trixelsToRollback) && TrixelCluster.Chunk.TestFace(subChunk, (ICollection<TrixelEmplacement>) boxTrixels, -Vector3.UnitX, true, box, ref trixelsToRollback) && TrixelCluster.Chunk.TestFace(subChunk, (ICollection<TrixelEmplacement>) boxTrixels, Vector3.UnitY, true, box, ref trixelsToRollback) && TrixelCluster.Chunk.TestFace(subChunk, (ICollection<TrixelEmplacement>) boxTrixels, -Vector3.UnitY, true, box, ref trixelsToRollback));
      boxTrixels.RemoveRange(boxTrixels.Count - trixelsToRollback, trixelsToRollback);
      box.Start += Vector3.One;
      box.End -= Vector3.One;
      if (boxTrixels.Count < subChunk.Count)
      {
        foreach (Vector3 direction in TrixelCluster.Directions)
          TrixelCluster.Chunk.ExpandSide(box, subChunk, boxTrixels, direction);
      }
      box.End += Vector3.One;
      return boxTrixels;
    }

    private static void ExpandSide(
      TrixelCluster.Box box,
      ICollection<TrixelEmplacement> subChunk,
      List<TrixelEmplacement> boxTrixels,
      Vector3 normal)
    {
      int trixelsToRollback = 0;
      bool flag = (double) Vector3.Dot(normal, Vector3.One) > 0.0;
      if (flag)
        box.End += normal;
      else
        box.Start += normal;
      for (; TrixelCluster.Chunk.TestFace(subChunk, (ICollection<TrixelEmplacement>) boxTrixels, normal, false, box, ref trixelsToRollback); trixelsToRollback = 0)
      {
        if (flag)
          box.End += normal;
        else
          box.Start += normal;
      }
      boxTrixels.RemoveRange(boxTrixels.Count - trixelsToRollback, trixelsToRollback);
      if (flag)
        box.End -= normal;
      else
        box.Start -= normal;
    }

    private static bool TestFace(
      ICollection<TrixelEmplacement> subChunk,
      ICollection<TrixelEmplacement> boxTrixels,
      Vector3 normal,
      bool partial,
      TrixelCluster.Box box,
      ref int trixelsToRollback)
    {
      Vector3 vector3_1 = normal.Abs();
      Vector3 vector2_1 = (double) normal.Z != 0.0 ? Vector3.UnitX : Vector3.UnitZ;
      Vector3 vector2_2 = (double) normal.Z != 0.0 ? Vector3.UnitY : new Vector3(1f, 1f, 0.0f) - vector3_1;
      TrixelEmplacement trixelEmplacement1;
      Vector3 position;
      if ((double) Vector3.Dot(normal, Vector3.One) <= 0.0)
      {
        trixelEmplacement1 = box.Start;
        position = trixelEmplacement1.Position;
      }
      else
      {
        trixelEmplacement1 = box.End;
        position = trixelEmplacement1.Position;
      }
      Vector3 vector3_2 = vector3_1;
      Vector3 vector3_3 = position * vector3_2;
      trixelEmplacement1 = box.Start;
      int num1 = (int) Vector3.Dot(trixelEmplacement1.Position, vector2_1);
      trixelEmplacement1 = box.End;
      int num2 = (int) Vector3.Dot(trixelEmplacement1.Position, vector2_1);
      trixelEmplacement1 = box.Start;
      int num3 = (int) Vector3.Dot(trixelEmplacement1.Position, vector2_2);
      trixelEmplacement1 = box.End;
      int num4 = (int) Vector3.Dot(trixelEmplacement1.Position, vector2_2);
      if (partial)
      {
        ++num1;
        --num2;
      }
      for (int index1 = num1; index1 <= num2; ++index1)
      {
        for (int index2 = num3; index2 <= num4; ++index2)
        {
          TrixelEmplacement trixelEmplacement2 = new TrixelEmplacement((float) index1 * vector2_1 + (float) index2 * vector2_2 + vector3_3);
          if (!subChunk.Contains(trixelEmplacement2))
            return false;
          ++trixelsToRollback;
          boxTrixels.Add(trixelEmplacement2);
        }
      }
      return true;
    }

    private static HashSet<TrixelEmplacement> VisitChunk(
      TrixelEmplacement origin,
      ICollection<TrixelEmplacement> subChunk)
    {
      HashSet<TrixelEmplacement> trixelEmplacementSet = new HashSet<TrixelEmplacement>()
      {
        origin
      };
      Queue<TrixelCluster.Chunk.TrixelToVisit> trixelToVisitQueue = new Queue<TrixelCluster.Chunk.TrixelToVisit>();
      trixelToVisitQueue.Enqueue(new TrixelCluster.Chunk.TrixelToVisit()
      {
        Trixel = origin
      });
      while (trixelToVisitQueue.Count != 0)
      {
        TrixelCluster.Chunk.TrixelToVisit toTraverse = trixelToVisitQueue.Dequeue();
        TrixelEmplacement trixel = toTraverse.Trixel;
        using (IEnumerator<Vector3> enumerator = ((IEnumerable<Vector3>) TrixelCluster.Directions).Where<Vector3>((Func<Vector3, bool>) (x => !toTraverse.Except.HasValue || toTraverse.Except.Value != x)).GetEnumerator())
        {
label_8:
          while (enumerator.MoveNext())
          {
            Vector3 current = enumerator.Current;
            TrixelEmplacement trixelEmplacement = trixel + current;
            while (true)
            {
              if (!trixelEmplacementSet.Contains(trixelEmplacement) && subChunk.Contains(trixelEmplacement))
              {
                trixelEmplacementSet.Add(trixelEmplacement);
                if (trixelEmplacementSet.Count != subChunk.Count)
                {
                  trixelToVisitQueue.Enqueue(new TrixelCluster.Chunk.TrixelToVisit()
                  {
                    Trixel = trixelEmplacement,
                    Except = new Vector3?(current)
                  });
                  trixelEmplacement += current;
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

    private struct TrixelToVisit
    {
      public TrixelEmplacement Trixel;
      public Vector3? Except;
    }
  }

  public class Box
  {
    public TrixelEmplacement Start { get; set; }

    public TrixelEmplacement End { get; set; }

    internal bool Contains(TrixelEmplacement trixel)
    {
      return trixel.X >= this.Start.X && trixel.Y >= this.Start.Y && trixel.Z >= this.Start.Z && trixel.X < this.End.X && trixel.Y < this.End.Y && trixel.Z < this.End.Z;
    }

    internal bool IsNeighbor(TrixelCluster.Box other)
    {
      BoundingBox boundingBox;
      ref BoundingBox local1 = ref boundingBox;
      TrixelEmplacement trixelEmplacement1 = this.Start;
      Vector3 position1 = trixelEmplacement1.Position;
      trixelEmplacement1 = this.End;
      Vector3 position2 = trixelEmplacement1.Position;
      local1 = new BoundingBox(position1, position2);
      BoundingBox box;
      ref BoundingBox local2 = ref box;
      TrixelEmplacement trixelEmplacement2 = other.Start;
      Vector3 position3 = trixelEmplacement2.Position;
      trixelEmplacement2 = other.End;
      Vector3 position4 = trixelEmplacement2.Position;
      local2 = new BoundingBox(position3, position4);
      return boundingBox.Intersects(box);
    }

    internal bool IsNeighbor(TrixelEmplacement trixel)
    {
      BoundingBox boundingBox;
      ref BoundingBox local = ref boundingBox;
      TrixelEmplacement trixelEmplacement = this.Start;
      Vector3 position1 = trixelEmplacement.Position;
      trixelEmplacement = this.End;
      Vector3 position2 = trixelEmplacement.Position;
      local = new BoundingBox(position1, position2);
      BoundingBox box = new BoundingBox(trixel.Position, trixel.Position + Vector3.One);
      return boundingBox.Intersects(box);
    }

    internal IEnumerable<TrixelEmplacement> Cells
    {
      get
      {
        for (int x = this.Start.X; x < this.End.X; ++x)
        {
          for (int y = this.Start.Y; y < this.End.Y; ++y)
          {
            for (int z = this.Start.Z; z < this.End.Z; ++z)
              yield return new TrixelEmplacement(x, y, z);
          }
        }
      }
    }
  }
}
