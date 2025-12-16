// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.INpcService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Components.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (NpcInstance))]
public interface INpcService : IScriptingBase
{
  [Description("Makes the NPC say a custom text line")]
  LongRunningAction Say(int id, string line, string customSound, string customAnimation);

  [Description("CarryGeezerLetter")]
  void CarryGeezerLetter(int id);
}
