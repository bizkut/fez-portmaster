// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.IOwlService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Static = true)]
public interface IOwlService : IScriptingBase
{
  event Action OwlCollected;

  event Action OwlLanded;

  void OnOwlCollected();

  void OnOwlLanded();

  [Description("Number of owls collected up to now")]
  int OwlsCollected { get; }
}
