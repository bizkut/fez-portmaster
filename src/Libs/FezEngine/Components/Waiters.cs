// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.Waiters
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

public static class Waiters
{
  public static IWaiter DoUntil(Func<bool> endCondition, Action<float> action)
  {
    return Waiters.DoUntil(endCondition, action, new Action(Util.NullAction));
  }

  public static IWaiter DoUntil(Func<bool> endCondition, Action<float> action, Action onComplete)
  {
    Waiter component = new Waiter(endCondition, (Action<TimeSpan>) (elapsed => action((float) elapsed.TotalSeconds)), onComplete);
    ServiceHelper.AddComponent((IGameComponent) component);
    return (IWaiter) component;
  }

  public static IWaiter Wait(Func<bool> endCondition, Action onValid)
  {
    Waiter component = new Waiter(endCondition, onValid);
    ServiceHelper.AddComponent((IGameComponent) component);
    return (IWaiter) component;
  }

  public static IWaiter Wait(double secondsToWait, Action onValid)
  {
    UpdateWaiter<Waiters.TimeKeeper> component = new UpdateWaiter<Waiters.TimeKeeper>((Func<Waiters.TimeKeeper, bool>) (waited => waited.Elapsed.TotalSeconds > secondsToWait), (Action<TimeSpan, Waiters.TimeKeeper>) ((elapsed, waited) => waited.Elapsed += elapsed), onValid);
    ServiceHelper.AddComponent((IGameComponent) component);
    return (IWaiter) component;
  }

  public static IWaiter Wait(
    double secondsToWait,
    Func<float, bool> earlyOutCondition,
    Action onValid)
  {
    UpdateWaiter<Waiters.TimeKeeper> component = new UpdateWaiter<Waiters.TimeKeeper>((Func<Waiters.TimeKeeper, bool>) (waited => earlyOutCondition((float) waited.Elapsed.TotalSeconds) || waited.Elapsed.TotalSeconds > secondsToWait), (Action<TimeSpan, Waiters.TimeKeeper>) ((elapsed, waited) => waited.Elapsed += elapsed), onValid);
    ServiceHelper.AddComponent((IGameComponent) component);
    return (IWaiter) component;
  }

  public static IWaiter Interpolate(double durationSeconds, Action<float> assignation)
  {
    return Waiters.Interpolate(durationSeconds, assignation, new Action(Util.NullAction));
  }

  public static IWaiter Interpolate(
    double durationSeconds,
    Action<float> assignation,
    Action onComplete)
  {
    if (durationSeconds == 0.0)
    {
      onComplete();
      return (IWaiter) null;
    }
    RenderWaiter<Waiters.TimeKeeper> component = new RenderWaiter<Waiters.TimeKeeper>((Func<Waiters.TimeKeeper, bool>) (waited => waited.Elapsed.TotalSeconds > durationSeconds), (Action<TimeSpan, Waiters.TimeKeeper>) ((elapsed, waited) =>
    {
      waited.Elapsed += elapsed;
      assignation((float) (waited.Elapsed.TotalSeconds / durationSeconds));
    }), onComplete);
    ServiceHelper.AddComponent((IGameComponent) component);
    return (IWaiter) component;
  }

  private class TimeKeeper
  {
    public TimeSpan Elapsed;
  }
}
