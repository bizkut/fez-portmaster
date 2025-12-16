// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.RenderWaiter`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Components;

internal class RenderWaiter<T> : Waiter<T>, IDrawable where T : class, new()
{
  private int drawOrder;

  internal RenderWaiter(Func<T, bool> condition, Action onValid)
    : base(condition, new Action<TimeSpan, T>(Util.NullAction<TimeSpan, T>), onValid, new T())
  {
  }

  internal RenderWaiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting)
    : base(condition, whileWaiting, new Action(Util.NullAction), new T())
  {
  }

  internal RenderWaiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting, Action onValid)
    : base(condition, whileWaiting, onValid, new T())
  {
  }

  internal RenderWaiter(Func<T, bool> condition, Action<TimeSpan, T> whileWaiting, T state)
    : base(condition, whileWaiting, new Action(Util.NullAction), state)
  {
  }

  internal RenderWaiter(
    Func<T, bool> condition,
    Action<TimeSpan, T> whileWaiting,
    Action onValid,
    T state)
    : base(condition, whileWaiting, onValid, state)
  {
  }

  public void Draw(GameTime gameTime)
  {
    if (this.AutoPause && (this.EngineState.Paused || !this.Camera.ActionRunning || !this.Camera.Viewpoint.IsOrthographic() || this.CustomPause != null && this.CustomPause()) || !this.Alive)
      return;
    this.whileWaiting(gameTime.ElapsedGameTime, this.state);
    if (!this.condition(this.state))
      return;
    this.onValid();
    this.Kill();
  }

  public bool Visible => true;

  public event EventHandler<EventArgs> VisibleChanged;

  public event EventHandler<EventArgs> DrawOrderChanged;

  public int DrawOrder
  {
    get => this.drawOrder;
    set
    {
      this.drawOrder = value;
      if (this.DrawOrderChanged == null)
        return;
      this.DrawOrderChanged((object) this, EventArgs.Empty);
    }
  }
}
