// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.ScriptReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Scripting;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class ScriptReader : ContentTypeReader<Script>
{
  protected override Script Read(ContentReader input, Script existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new Script();
    existingInstance.Name = input.ReadString();
    existingInstance.Timeout = input.ReadObject<TimeSpan?>();
    existingInstance.Triggers = input.ReadObject<List<ScriptTrigger>>(existingInstance.Triggers);
    existingInstance.Conditions = input.ReadObject<List<ScriptCondition>>(existingInstance.Conditions);
    existingInstance.Actions = input.ReadObject<List<ScriptAction>>(existingInstance.Actions);
    existingInstance.OneTime = input.ReadBoolean();
    existingInstance.Triggerless = input.ReadBoolean();
    existingInstance.IgnoreEndTriggers = input.ReadBoolean();
    existingInstance.LevelWideOneTime = input.ReadBoolean();
    existingInstance.Disabled = input.ReadBoolean();
    existingInstance.IsWinCondition = input.ReadBoolean();
    return existingInstance;
  }
}
