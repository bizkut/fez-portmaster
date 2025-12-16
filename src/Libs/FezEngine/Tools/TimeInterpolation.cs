// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.TimeInterpolation
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public static class TimeInterpolation
{
  public const double TimestepMS = 17.0;
  public static readonly TimeSpan UpdateTimestep = TimeSpan.FromTicks(170000L);
  public static TimeSpan LastUpdate;
  public static bool NeedsInterpolation;
  private static readonly List<TimeInterpolation.OrderedCallback> interpolationCallbacks = new List<TimeInterpolation.OrderedCallback>();

  public static void RegisterCallback(Action<GameTime> callback, int order)
  {
    lock (TimeInterpolation.interpolationCallbacks)
    {
      TimeInterpolation.interpolationCallbacks.Add(new TimeInterpolation.OrderedCallback()
      {
        Callback = callback,
        Order = order
      });
      TimeInterpolation.interpolationCallbacks.Sort((Comparison<TimeInterpolation.OrderedCallback>) ((a, b) => a.Order.CompareTo(b.Order)));
    }
  }

  public static void ProcessCallbacks(GameTime gameTime)
  {
    lock (TimeInterpolation.interpolationCallbacks)
    {
      foreach (TimeInterpolation.OrderedCallback interpolationCallback in TimeInterpolation.interpolationCallbacks)
        interpolationCallback.Callback(gameTime);
    }
  }

  private struct OrderedCallback
  {
    public int Order;
    public Action<GameTime> Callback;
  }
}
