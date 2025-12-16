// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.VaryingSingle
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Tools;
using System;

#nullable disable
namespace FezEngine.Components;

public class VaryingSingle : VaryingValue<float>
{
  public static implicit operator VaryingSingle(float value)
  {
    VaryingSingle varyingSingle = new VaryingSingle();
    varyingSingle.Base = value;
    return varyingSingle;
  }

  protected override Func<float, float, float> DefaultFunction
  {
    get
    {
      return (Func<float, float, float>) ((b, v) => (double) v != 0.0 ? b + RandomHelper.Centered((double) v) : b);
    }
  }
}
