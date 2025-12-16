// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Geometry.SharedVertex`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Structure.Geometry;

public class SharedVertex<T> : IEquatable<SharedVertex<T>> where T : struct, IEquatable<T>, IVertex
{
  public int Index { get; set; }

  public int References { get; set; }

  public T Vertex { get; set; }

  public override int GetHashCode() => this.Vertex.GetHashCode();

  public override string ToString()
  {
    return $"{{Vertex:{this.Vertex} Index:{this.Index} References:{this.References}}}";
  }

  public override bool Equals(object obj)
  {
    return obj is SharedVertex<T> && this.Equals(obj as SharedVertex<T>);
  }

  public bool Equals(SharedVertex<T> other) => other.Vertex.Equals(this.Vertex);
}
