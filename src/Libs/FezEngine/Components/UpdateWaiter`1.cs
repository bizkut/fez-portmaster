// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.UpdateWaiter`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

internal class UpdateWaiter<T> : Waiter<T>, IUpdateable where T : class, new()
{
  private int updateOrder;

  internal UpdateWaiter(Func<T, bool> condition, Action onValid)
    : base(condition, new Action<TimeSpan, T>(Util.NullAction<TimeSpan, T>), onValid, new T())
  {
  }

  internal UpdateWaiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting)
    : base(condition, whileWaiting, new Action(Util.NullAction), new T())
  {
  }

  internal UpdateWaiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting, Action onValid)
    : base(condition, whileWaiting, onValid, new T())
  {
  }

  internal UpdateWaiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting, T state)
    : base(condition, whileWaiting, new Action(Util.NullAction), state)
  {
  }

  internal UpdateWaiter(
    Func<T, bool> condition,
    Action<TimeSpan, T> whileWaiting,
    Action onValid,
    T state)
    : base(condition, whileWaiting, onValid, state)
  {
  }

  public void Update(GameTime gameTime)
  {
    if (this.AutoPause && (this.EngineState.Paused || !this.Camera.ActionRunning || !this.Camera.Viewpoint.IsOrthographic() || this.CustomPause != null && this.CustomPause()) || !this.Alive)
      return;
    this.whileWaiting(gameTime.ElapsedGameTime, this.state);
    if (!this.condition(this.state))
      return;
    this.onValid();
    this.Kill();
  }

  public bool Enabled => true;

  public event EventHandler<EventArgs> EnabledChanged;

  public event EventHandler<EventArgs> UpdateOrderChanged;

  public int UpdateOrder
  {
    get => this.updateOrder;
    set
    {
      this.updateOrder = value;
      if (this.UpdateOrderChanged == null)
        return;
      this.UpdateOrderChanged((object) this, EventArgs.Empty);
    }
  }
}
