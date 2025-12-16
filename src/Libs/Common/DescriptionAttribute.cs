// Decompiled with JetBrains decompiler
// Type: Common.DescriptionAttribute
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

using System;

#nullable disable
namespace Common;

public class DescriptionAttribute : Attribute
{
  public DescriptionAttribute(string description) => this.Description = description;

  public string Description { get; set; }
}
