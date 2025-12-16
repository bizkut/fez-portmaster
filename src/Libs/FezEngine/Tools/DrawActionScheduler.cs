// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.DrawActionScheduler
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;
using System.Collections.Concurrent;

#nullable disable
namespace FezEngine.Tools;

public static class DrawActionScheduler
{
  private static readonly ConcurrentQueue<Action> DeferredDrawActions = new ConcurrentQueue<Action>();

  public static void Schedule(Action action)
  {
    if (!PersistentThreadPool.IsOnMainThread)
      DrawActionScheduler.DeferredDrawActions.Enqueue(action);
    else
      action();
  }

  public static void Process()
  {
    Action result;
    while (DrawActionScheduler.DeferredDrawActions.TryDequeue(out result))
      result();
  }
}
