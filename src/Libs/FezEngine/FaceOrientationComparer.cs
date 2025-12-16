// Decompiled with JetBrains decompiler
// Type: FezEngine.FaceOrientationComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine;

public class FaceOrientationComparer : IEqualityComparer<FaceOrientation>
{
  public static readonly FaceOrientationComparer Default = new FaceOrientationComparer();

  public bool Equals(FaceOrientation x, FaceOrientation y) => x == y;

  public int GetHashCode(FaceOrientation obj) => (int) obj;
}
