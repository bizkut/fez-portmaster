// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.Waiter`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

internal class Waiter<T> : IGameComponent, IWaiter where T : class, new()
{
  protected readonly IEngineStateManager EngineState;
  protected readonly IDefaultCameraManager Camera;
  protected Func<T, bool> condition;
  protected Action<TimeSpan, T> whileWaiting;
  protected Action onValid;
  protected readonly T state;

  public bool Alive { get; private set; }

  public object Tag { get; set; }

  public bool AutoPause { get; set; }

  public Func<bool> CustomPause { get; set; }

  internal Waiter(Func<T, bool> condition, Action onValid)
    : this(condition, new Action<TimeSpan, T>(Util.NullAction<TimeSpan, T>), onValid, new T())
  {
  }

  internal Waiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting)
    : this(condition, whileWaiting, new Action(Util.NullAction), new T())
  {
  }

  internal Waiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting, Action onValid)
    : this(condition, whileWaiting, onValid, new T())
  {
  }

  internal Waiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting, T state)
    : this(condition, whileWaiting, new Action(Util.NullAction), state)
  {
  }

  internal Waiter(
    Func<T, bool> condition,
    Action<TimeSpan, T> whileWaiting,
    Action onValid,
    T state)
  {
    this.condition = condition;
    this.whileWaiting = whileWaiting;
    this.onValid = onValid;
    this.state = state;
    this.Alive = true;
    this.EngineState = ServiceHelper.Get<IEngineStateManager>();
    this.Camera = ServiceHelper.Get<IDefaultCameraManager>();
  }

  public void Cancel()
  {
    if (!this.Alive)
      return;
    this.Kill();
  }

  protected void Kill()
  {
    this.Alive = false;
    this.whileWaiting = (Action<TimeSpan, T>) null;
    this.onValid = (Action) null;
    this.condition = (Func<T, bool>) null;
    ServiceHelper.RemoveComponent<Waiter<T>>(this);
  }

  public void Initialize()
  {
  }
}
