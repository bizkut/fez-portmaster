// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Dirtyable`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Structure;

public class Dirtyable<T>
{
  public T Value;
  public bool Dirty;

  public void Clean() => this.Dirty = false;

  public void Set(T newValue)
  {
    this.Value = newValue;
    this.Dirty = true;
  }

  public static implicit operator T(Dirtyable<T> dirtyable) => dirtyable.Value;

  public static implicit operator Dirtyable<T>(T dirtyable)
  {
    return new Dirtyable<T>() { Value = dirtyable };
  }
}
