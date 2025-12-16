// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.ISwitchService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Components.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (TrileGroup), RestrictTo = new ActorType[] {ActorType.PushSwitch, ActorType.ExploSwitch, ActorType.PushSwitchPermanent})]
public interface ISwitchService : IScriptingBase
{
  [Description("When a bomb explodes near this switch")]
  event Action<int> Explode;

  void OnExplode(int id);

  [Description("When this switch is pushed completely")]
  [EndTrigger("Lift")]
  event Action<int> Push;

  void OnPush(int id);

  [Description("When this switch is lifted back up")]
  event Action<int> Lift;

  void OnLift(int id);

  [Description("Activates this switch")]
  void Activate(int id);

  [Description("Changes the visual of this switch's triles")]
  LongRunningAction ChangeTrile(int id, int newTrileId);
}
