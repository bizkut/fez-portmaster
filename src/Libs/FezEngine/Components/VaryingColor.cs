// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.VaryingColor
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

public class VaryingColor : VaryingValue<Color>
{
  public static implicit operator VaryingColor(Color value)
  {
    VaryingColor varyingColor = new VaryingColor();
    varyingColor.Base = value;
    return varyingColor;
  }

  protected override Func<Color, Color, Color> DefaultFunction
  {
    get
    {
      return (Func<Color, Color, Color>) ((b, v) => v == new Color(0, 0, 0, 0) ? b : new Color((float) b.R / (float) byte.MaxValue + RandomHelper.Centered((double) v.R / (double) byte.MaxValue), (float) b.G / (float) byte.MaxValue + RandomHelper.Centered((double) v.G / (double) byte.MaxValue), (float) b.B / (float) byte.MaxValue + RandomHelper.Centered((double) v.B / (double) byte.MaxValue), (float) b.A / (float) byte.MaxValue + RandomHelper.Centered((double) v.A / (double) byte.MaxValue)));
    }
  }

  public static Func<Color, Color, Color> Uniform
  {
    get
    {
      return (Func<Color, Color, Color>) ((b, v) =>
      {
        Vector4 vector4_1 = b.ToVector4();
        Vector4 vector4_2 = v.ToVector4();
        float num = RandomHelper.Centered(1.0);
        return new Color(new Vector4(vector4_1.X + num * vector4_2.X, vector4_1.Y + num * vector4_2.Y, vector4_1.Z + num * vector4_2.Z, vector4_1.W + num * vector4_2.W));
      });
    }
  }
}
