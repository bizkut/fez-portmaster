// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.ISpatialStructure`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

internal interface ISpatialStructure<T>
{
  bool Empty { get; }

  void Clear();

  void Free(IEnumerable<T> cells);

  void Free(T cell);

  void Fill(IEnumerable<T> cells);

  void Fill(T cell);

  bool IsFilled(T cell);

  IEnumerable<T> Cells { get; }
}
