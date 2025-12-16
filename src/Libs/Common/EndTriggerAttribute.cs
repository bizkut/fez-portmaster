// Decompiled with JetBrains decompiler
// Type: Common.EndTriggerAttribute
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

using System;

#nullable disable
namespace Common;

public class EndTriggerAttribute : Attribute
{
  public EndTriggerAttribute(string trigger) => this.Trigger = trigger;

  public string Trigger { get; set; }
}
