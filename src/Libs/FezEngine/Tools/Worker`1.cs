// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.Worker`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;
using System.Threading;

#nullable disable
namespace FezEngine.Tools;

public class Worker<TContext> : WorkerInternal
{
  internal Action<TContext> task;
  private readonly PersistentThread thread;
  public Action OnAbort;
  private TContext cachedContext;

  public event Action Finished;

  public override void OnFinished()
  {
    if (this.Finished == null)
      return;
    this.Finished();
  }

  internal Worker(PersistentThread thread, Action<TContext> task)
  {
    this.task = task;
    this.thread = thread;
  }

  public override void Act() => this.task(this.cachedContext);

  public void Start(TContext context)
  {
    if (this.thread.Started)
      throw new InvalidOperationException("Thread is already started");
    if (this.thread.Disposed)
      throw new ObjectDisposedException("PersistentThread");
    this.cachedContext = context;
    this.thread.CurrentWorker = (IWorker) this;
    this.thread.Start();
  }

  public void Join()
  {
    if (!this.thread.Started)
      return;
    if (this.thread.Disposed)
      throw new ObjectDisposedException("PersistentThread");
    this.thread.Join();
  }

  internal override void Dispose()
  {
    if (this.thread.Started)
      this.thread.Join();
    this.thread.Priority = ThreadPriority.Lowest;
    this.Finished = (Action) null;
    this.task = (Action<TContext>) null;
  }

  internal override void Abort()
  {
    this.Aborted = true;
    if (this.OnAbort == null)
      return;
    this.OnAbort();
  }

  public bool Aborted { get; private set; }

  public ThreadPriority Priority
  {
    set => this.thread.Priority = value;
  }

  internal override PersistentThread UnderlyingThread => this.thread;
}
