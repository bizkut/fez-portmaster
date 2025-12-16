// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.IScriptService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure.Scripting;
using System;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Model = typeof (Script))]
public interface IScriptService : IScriptingBase
{
  [Description("When the script timeouts or terminates")]
  event Action<int> Complete;

  void OnComplete(int id);

  [Description("Enables or disables a script")]
  void SetEnabled(int id, bool enabled);

  [Description("Evaluates a script")]
  void Evaluate(int id);
}
