// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.ICodePatternService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (ArtObjectInstance), RestrictTo = new ActorType[] {ActorType.Rumbler, ActorType.CodeMachine, ActorType.QrCode})]
public interface ICodePatternService : IScriptingBase
{
  [Description("When the right pattern is input")]
  event Action<int> Activated;

  void OnActivate(int id);
}
