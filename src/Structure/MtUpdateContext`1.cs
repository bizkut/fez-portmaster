// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.MtUpdateContext`1
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using System;

#nullable disable
namespace FezGame.Structure;

public struct MtUpdateContext<T>
{
  public int StartIndex;
  public int EndIndex;
  public TimeSpan Elapsed;
  public T Result;
}
