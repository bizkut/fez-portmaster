// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IDebuggingBag
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public interface IDebuggingBag
{
  void Add(string name, object item);

  IEnumerable<string> Keys { get; }

  void Empty();

  object this[string index] { get; }

  float GetAge(string name);
}
