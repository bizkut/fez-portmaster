// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.PropertyDescriptor
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System;

#nullable disable
namespace FezEngine.Structure.Scripting;

public struct PropertyDescriptor(
  string name,
  string description,
  Type type,
  DynamicMethodDelegate @delegate)
{
  public readonly string Name = name;
  public readonly string Description = description;
  public readonly Type Type = type;
  public readonly DynamicMethodDelegate GetValue = @delegate;
}
