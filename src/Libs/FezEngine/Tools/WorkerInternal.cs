// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.WorkerInternal
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Tools;

public abstract class WorkerInternal : IWorker
{
  internal abstract void Abort();

  internal abstract void Dispose();

  public abstract void Act();

  public abstract void OnFinished();

  internal abstract PersistentThread UnderlyingThread { get; }
}
