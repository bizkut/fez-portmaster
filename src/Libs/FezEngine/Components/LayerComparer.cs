// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.LayerComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Components;

internal class LayerComparer : IEqualityComparer<Layer>
{
  public static readonly LayerComparer Default = new LayerComparer();

  public bool Equals(Layer x, Layer y) => x == y;

  public int GetHashCode(Layer obj) => (int) obj;
}
