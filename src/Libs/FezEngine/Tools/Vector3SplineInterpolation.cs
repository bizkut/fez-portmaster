// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.Vector3SplineInterpolation
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Tools;

public class Vector3SplineInterpolation(TimeSpan duration, params Vector3[] points) : 
  SplineInterpolation<Vector3>(duration, points)
{
  protected override void Interpolate(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
  {
    if (SplineInterpolation<Vector3>.LongScreenshot)
      this.Current = FezMath.Slerp(p1, p2, t);
    else
      this.Current = Vector3.CatmullRom(p0, p1, p2, p3, t);
  }
}
