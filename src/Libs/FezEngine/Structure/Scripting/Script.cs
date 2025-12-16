// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.Script
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure.Scripting;

public class Script
{
  internal const string MemberSeparator = ".";

  public Script()
  {
    this.Name = "Untitled";
    this.Triggers = new List<ScriptTrigger>();
    this.Actions = new List<ScriptAction>();
  }

  [Serialization(Ignore = true)]
  public int Id { get; set; }

  public string Name { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool OneTime { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool LevelWideOneTime { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Disabled { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Triggerless { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool IgnoreEndTriggers { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool IsWinCondition { get; set; }

  [Serialization(Optional = true)]
  public TimeSpan? Timeout { get; set; }

  [Serialization(CollectionItemName = "Trigger")]
  public List<ScriptTrigger> Triggers { get; set; }

  [Serialization(CollectionItemName = "Action")]
  public List<ScriptAction> Actions { get; set; }

  [Serialization(Optional = true, CollectionItemName = "Condition")]
  public List<ScriptCondition> Conditions { get; set; }

  [Serialization(Ignore = true)]
  public bool ScheduleEvalulation { get; set; }

  public Script Clone()
  {
    List<ScriptTrigger> list1 = this.Triggers.Select<ScriptTrigger, ScriptTrigger>((Func<ScriptTrigger, ScriptTrigger>) (t => t.Clone())).ToList<ScriptTrigger>();
    List<ScriptAction> list2 = this.Actions.Select<ScriptAction, ScriptAction>((Func<ScriptAction, ScriptAction>) (a => a.Clone())).ToList<ScriptAction>();
    List<ScriptCondition> list3 = this.Conditions == null ? (List<ScriptCondition>) null : this.Conditions.Select<ScriptCondition, ScriptCondition>((Func<ScriptCondition, ScriptCondition>) (c => c.Clone())).ToList<ScriptCondition>();
    return new Script()
    {
      Id = -1,
      Name = this.Name,
      Triggers = list1,
      Actions = list2,
      Conditions = list3,
      OneTime = this.OneTime,
      LevelWideOneTime = this.LevelWideOneTime,
      Disabled = this.Disabled,
      Triggerless = this.Triggerless,
      IgnoreEndTriggers = this.IgnoreEndTriggers,
      Timeout = this.Timeout,
      ScheduleEvalulation = this.ScheduleEvalulation
    };
  }
}
