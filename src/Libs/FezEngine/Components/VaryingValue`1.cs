// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.VaryingValue`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Components;

public abstract class VaryingValue<T>
{
  public T Base;
  public T Variation;
  public Func<T, T, T> Function;

  public T Evaluate()
  {
    return this.Function != null ? this.Function(this.Base, this.Variation) : this.DefaultFunction(this.Base, this.Variation);
  }

  protected abstract Func<T, T, T> DefaultFunction { get; }
}
