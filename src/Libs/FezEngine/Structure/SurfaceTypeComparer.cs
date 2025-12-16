// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.SurfaceTypeComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class SurfaceTypeComparer : IEqualityComparer<SurfaceType>
{
  public static readonly SurfaceTypeComparer Default = new SurfaceTypeComparer();

  public bool Equals(SurfaceType x, SurfaceType y) => x == y;

  public int GetHashCode(SurfaceType obj) => (int) obj;
}
