// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.VaryingVector3
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

public class VaryingVector3 : VaryingValue<Vector3>
{
  public static implicit operator VaryingVector3(Vector3 value)
  {
    VaryingVector3 varyingVector3 = new VaryingVector3();
    varyingVector3.Base = value;
    return varyingVector3;
  }

  protected override Func<Vector3, Vector3, Vector3> DefaultFunction
  {
    get
    {
      return (Func<Vector3, Vector3, Vector3>) ((b, v) => v == Vector3.Zero ? b : b + new Vector3(RandomHelper.Centered((double) v.X), RandomHelper.Centered((double) v.Y), RandomHelper.Centered((double) v.Z)));
    }
  }

  public static Func<Vector3, Vector3, Vector3> Uniform
  {
    get
    {
      return (Func<Vector3, Vector3, Vector3>) ((b, v) =>
      {
        float num = RandomHelper.Centered(1.0);
        return new Vector3(b.X + num * v.X, b.Y + num * v.Y, b.Z + num * v.Z);
      });
    }
  }

  public static Func<Vector3, Vector3, Vector3> ClampToTrixels
  {
    get
    {
      return (Func<Vector3, Vector3, Vector3>) ((b, v) => ((b + new Vector3(RandomHelper.Centered((double) v.X), RandomHelper.Centered((double) v.Y), RandomHelper.Centered((double) v.Z))) * 16f).Round() / 16f);
    }
  }
}
