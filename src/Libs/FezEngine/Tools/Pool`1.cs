// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.Pool`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class Pool<T> where T : class, new()
{
  private readonly Stack<T> stack;
  private int size;

  public Pool()
    : this(0)
  {
  }

  public Pool(int size)
  {
    this.stack = new Stack<T>(size);
    this.Size = size;
  }

  public T Take() => this.stack.Count <= 0 ? new T() : this.stack.Pop();

  public void Return(T item) => this.stack.Push(item);

  public int Size
  {
    get => this.size;
    set
    {
      int num = value - this.size;
      if (num > 0)
      {
        for (int index = 0; index < num; ++index)
          this.stack.Push(new T());
      }
      this.size = value;
    }
  }

  public int Available => this.stack.Count;
}
