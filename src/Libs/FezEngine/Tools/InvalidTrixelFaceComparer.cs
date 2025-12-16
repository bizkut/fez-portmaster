// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.InvalidTrixelFaceComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class InvalidTrixelFaceComparer : IComparer<TrixelFace>
{
  public int Compare(TrixelFace x, TrixelFace y)
  {
    if (x.Face != y.Face)
      return x.Face.CompareTo((object) y.Face);
    switch (x.Face)
    {
      case FaceOrientation.Down:
      case FaceOrientation.Top:
        int num1 = x.Id.X.CompareTo(y.Id.X);
        return num1 != 0 ? num1 : x.Id.Z.CompareTo(y.Id.Z);
      case FaceOrientation.Back:
      case FaceOrientation.Front:
        int num2 = x.Id.Y.CompareTo(y.Id.Y);
        return num2 != 0 ? num2 : x.Id.X.CompareTo(y.Id.X);
      default:
        int num3 = x.Id.Z.CompareTo(y.Id.Z);
        return num3 != 0 ? num3 : x.Id.Y.CompareTo(y.Id.Y);
    }
  }
}
