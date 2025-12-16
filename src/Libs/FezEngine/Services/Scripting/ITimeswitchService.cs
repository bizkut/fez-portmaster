// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.ITimeswitchService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (ArtObjectInstance), RestrictTo = new ActorType[] {ActorType.Timeswitch})]
public interface ITimeswitchService : IScriptingBase
{
  [Description("When the screw minimally sticks out from the base (it's been screwed out)")]
  event Action<int> ScrewedOut;

  void OnScrewedOut(int id);

  [Description("When it stop winding back in (hits the base)")]
  event Action<int> HitBase;

  void OnHitBase(int id);
}
