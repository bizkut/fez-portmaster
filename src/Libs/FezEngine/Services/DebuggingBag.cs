// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.DebuggingBag
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Services;

public class DebuggingBag : IDebuggingBag
{
  private readonly Dictionary<string, DebuggingBag.DebuggingLine> items;
  private static readonly TimeSpan ExpirationTime = new TimeSpan(0, 0, 5);

  public DebuggingBag() => this.items = new Dictionary<string, DebuggingBag.DebuggingLine>();

  public void Add(string name, object item)
  {
  }

  public void Empty()
  {
    List<string> stringList = new List<string>();
    foreach (string key in this.items.Keys)
    {
      if (this.items[key].Expired)
        stringList.Add(key);
    }
    foreach (string key in stringList)
      this.items.Remove(key);
  }

  public object this[string name]
  {
    get => this.items.ContainsKey(name) ? this.items[name].Value : (object) null;
  }

  public float GetAge(string name) => this.items.ContainsKey(name) ? this.items[name].Age : 0.0f;

  public IEnumerable<string> Keys
  {
    get
    {
      return (IEnumerable<string>) this.items.Keys.OrderBy<string, string>((Func<string, string>) (x => x));
    }
  }

  private class DebuggingLine
  {
    private readonly object value;
    private DateTime lastUpdateTime;

    public DebuggingLine(object value)
    {
      this.value = value;
      this.lastUpdateTime = DateTime.Now;
    }

    public object Value => this.value;

    public void Refresh() => this.lastUpdateTime = DateTime.Now;

    public bool Expired => DateTime.Now - this.lastUpdateTime >= DebuggingBag.ExpirationTime;

    public float Age
    {
      get
      {
        TimeSpan timeSpan = DateTime.Now - this.lastUpdateTime;
        double ticks1 = (double) timeSpan.Ticks;
        timeSpan = DebuggingBag.ExpirationTime;
        double ticks2 = (double) timeSpan.Ticks;
        return MathHelper.Clamp((float) (ticks1 / ticks2), 0.0f, 1f);
      }
    }
  }
}
