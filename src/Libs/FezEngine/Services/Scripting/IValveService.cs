// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.IValveService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (ArtObjectInstance), RestrictTo = new ActorType[] {ActorType.Valve, ActorType.BoltHandle})]
public interface IValveService : IScriptingBase
{
  [Description("When it's unscrewed")]
  event Action<int> Screwed;

  void OnScrew(int id);

  [Description("When it's screwed in")]
  event Action<int> Unscrewed;

  void OnUnscrew(int id);

  [Description("Enables or disables a valve's rotatability")]
  void SetEnabled(int id, bool enabled);
}
