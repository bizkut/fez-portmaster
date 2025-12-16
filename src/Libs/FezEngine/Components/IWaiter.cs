// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.IWaiter
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Components;

public interface IWaiter
{
  void Cancel();

  bool Alive { get; }

  object Tag { get; set; }

  bool AutoPause { get; set; }

  Func<bool> CustomPause { get; set; }
}
