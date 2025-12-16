// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.IThreadPool
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Tools;

public interface IThreadPool
{
  Worker<TContext> Take<TContext>(Action<TContext> task);

  Worker<TContext> TakeShared<TContext>(Action<TContext> task);

  void Return<TContext>(Worker<TContext> thread);
}
