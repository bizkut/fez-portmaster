// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.MultipleHits`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Structure;

public struct MultipleHits<T>
{
  public T NearLow;
  public T FarHigh;

  public T First
  {
    get
    {
      return !object.Equals((object) this.NearLow, (object) default (T)) ? this.NearLow : this.FarHigh;
    }
  }

  public override string ToString() => $"{{Near/Low: {this.NearLow} Far/High: {this.FarHigh}}}";
}
