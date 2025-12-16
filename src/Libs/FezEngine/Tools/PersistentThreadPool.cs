// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.PersistentThreadPool
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

#nullable disable
namespace FezEngine.Tools;

public class PersistentThreadPool : GameComponent, IThreadPool
{
  private const int InitialMaxThreads = 1;
  private static int MainThreadId;
  private readonly ConcurrentStack<PersistentThread> stack;
  private readonly ConcurrentDictionary<IWorker, WorkerInternal> taken = new ConcurrentDictionary<IWorker, WorkerInternal>();
  private bool disposed;
  public static bool SingleThreaded;

  public static bool IsOnMainThread
  {
    get => PersistentThreadPool.MainThreadId == Thread.CurrentThread.ManagedThreadId;
  }

  public PersistentThreadPool(Game game)
    : base(game)
  {
    Logger.Log("Threading", LogSeverity.Information, "Multithreading is " + (PersistentThreadPool.SingleThreaded ? "disabled" : "enabled"));
    PersistentThreadPool.MainThreadId = Thread.CurrentThread.ManagedThreadId;
    this.stack = new ConcurrentStack<PersistentThread>();
    for (int index = 0; index < 1; ++index)
      this.stack.Push(this.CreateThread());
  }

  private PersistentThread CreateThread() => new PersistentThread();

  public Worker<TContext> Take<TContext>(Action<TContext> task)
  {
    PersistentThread result;
    if (!this.stack.TryPop(out result))
      result = this.CreateThread();
    Worker<TContext> key = new Worker<TContext>(result, task);
    this.taken.TryAdd((IWorker) key, (WorkerInternal) key);
    return key;
  }

  public Worker<TContext> TakeShared<TContext>(Action<TContext> task)
  {
    PersistentThread result;
    if (!this.stack.TryPeek(out result))
      result = this.CreateThread();
    Worker<TContext> key = new Worker<TContext>(result, task);
    this.taken.TryAdd((IWorker) key, (WorkerInternal) key);
    return key;
  }

  public void Return<TContext>(Worker<TContext> worker)
  {
    if (worker == null || !this.taken.TryRemove((IWorker) worker, out WorkerInternal _))
      return;
    worker.Dispose();
    if (this.disposed)
      worker.UnderlyingThread.Dispose();
    else
      this.stack.Push(worker.UnderlyingThread);
  }

  protected override void Dispose(bool disposing)
  {
    foreach (WorkerInternal workerInternal in (IEnumerable<WorkerInternal>) this.taken.Values)
    {
      workerInternal.Abort();
      workerInternal.Dispose();
      this.stack.Push(workerInternal.UnderlyingThread);
    }
    this.taken.Clear();
    PersistentThread result;
    while (this.stack.TryPop(out result))
      result.Dispose();
    this.disposed = true;
    base.Dispose(disposing);
  }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }
}
