// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.PersistentThread
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System;
using System.Threading;

#nullable disable
namespace FezEngine.Tools;

internal class PersistentThread : IDisposable
{
  private readonly Thread thread;
  private readonly ManualResetEventSlim startEvent;
  private readonly ManualResetEventSlim joinEvent;

  public bool Started { get; private set; }

  public bool Disposed { get; private set; }

  public IWorker CurrentWorker { private get; set; }

  public PersistentThread()
  {
    if (PersistentThreadPool.SingleThreaded)
      return;
    this.startEvent = new ManualResetEventSlim(false);
    this.joinEvent = new ManualResetEventSlim(false);
    this.thread = new Thread(new ThreadStart(this.DoWork))
    {
      Priority = ThreadPriority.Lowest
    };
    this.thread.Start();
  }

  public void Start()
  {
    this.Started = true;
    if (PersistentThreadPool.SingleThreaded)
    {
      this.CurrentWorker.Act();
      this.CurrentWorker.OnFinished();
    }
    else
      this.startEvent.Set();
  }

  public void Join()
  {
    if (!PersistentThreadPool.SingleThreaded && this.thread != Thread.CurrentThread)
    {
      this.joinEvent.Wait();
      this.joinEvent.Reset();
    }
    this.Started = false;
  }

  private void DoWork() => Logger.Try(new Action(this.DoActualWork));

  private void DoActualWork()
  {
    if (PersistentThreadPool.SingleThreaded)
      return;
    this.startEvent.Wait();
    this.startEvent.Reset();
    while (!this.Disposed)
    {
      this.CurrentWorker.Act();
      this.CurrentWorker.OnFinished();
      this.joinEvent.Set();
      this.startEvent.Wait();
      this.startEvent.Reset();
    }
  }

  public ThreadPriority Priority
  {
    set
    {
      if (PersistentThreadPool.SingleThreaded)
        return;
      this.thread.Priority = value;
    }
  }

  public void Dispose()
  {
    if (!this.Disposed)
      GC.SuppressFinalize((object) this);
    this.DisposeInternal();
  }

  private void DisposeInternal()
  {
    if (this.Disposed)
      return;
    this.Disposed = true;
    if (PersistentThreadPool.SingleThreaded)
      return;
    this.startEvent.Set();
  }

  ~PersistentThread() => this.DisposeInternal();
}
