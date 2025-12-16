// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.RandomHelper
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

#nullable disable
namespace FezEngine.Tools;

public static class RandomHelper
{
  public static Random Random
  {
    get
    {
      LocalDataStoreSlot namedDataSlot = Thread.GetNamedDataSlot(nameof (Random));
      object random;
      if ((random = Thread.GetData(namedDataSlot)) == null)
        Thread.SetData(namedDataSlot, (object) (Random) (random = (object) new Random()));
      return random as Random;
    }
  }

  public static bool Probability(double p) => p > RandomHelper.Random.NextDouble();

  public static int Sign() => !RandomHelper.Probability(0.5) ? 1 : -1;

  public static float Centered(double distance)
  {
    return (float) ((RandomHelper.Random.NextDouble() - 0.5) * distance * 2.0);
  }

  public static float Centered(double distance, double around)
  {
    return (float) ((RandomHelper.Random.NextDouble() - 0.5) * distance * 2.0 + around);
  }

  public static float Between(double min, double max)
  {
    return (float) (RandomHelper.Random.NextDouble() * (max - min) + min);
  }

  public static float Unit() => (float) RandomHelper.Random.NextDouble();

  public static T EnumField<T>(bool excludeFirst) where T : struct
  {
    IEnumerable<T> values = Util.GetValues<T>();
    return values.ElementAt<T>(RandomHelper.Random.Next(excludeFirst ? 1 : 0, values.Count<T>()));
  }

  public static T EnumField<T>() where T : struct => RandomHelper.EnumField<T>(false);

  public static T InList<T>(T[] list) => list[RandomHelper.Random.Next(0, list.Length)];

  public static T InList<T>(List<T> list) => list[RandomHelper.Random.Next(0, list.Count)];

  public static T InList<T>(IEnumerable<T> list)
  {
    return list.ElementAt<T>(RandomHelper.Random.Next(0, list.Count<T>()));
  }

  public static Vector3 NormalizedVector()
  {
    return Vector3.Normalize(new Vector3((float) ((double) RandomHelper.Unit() * 2.0 - 1.0), (float) ((double) RandomHelper.Unit() * 2.0 - 1.0), (float) ((double) RandomHelper.Unit() * 2.0 - 1.0)));
  }
}
