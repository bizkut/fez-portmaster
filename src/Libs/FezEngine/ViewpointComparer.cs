// Decompiled with JetBrains decompiler
// Type: FezEngine.ViewpointComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine;

public class ViewpointComparer : IEqualityComparer<Viewpoint>
{
  public static readonly ViewpointComparer Default = new ViewpointComparer();

  public bool Equals(Viewpoint x, Viewpoint y) => x == y;

  public int GetHashCode(Viewpoint obj) => (int) obj;
}
