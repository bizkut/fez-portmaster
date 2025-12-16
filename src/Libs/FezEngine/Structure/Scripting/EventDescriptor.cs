// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.EventDescriptor
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;

#nullable disable
namespace FezEngine.Structure.Scripting;

public struct EventDescriptor(
  string name,
  string description,
  DynamicMethodDelegate @delegate,
  DynamicMethodDelegate endTrigger)
{
  public readonly string Name = name;
  public readonly string Description = description;
  public readonly DynamicMethodDelegate AddHandler = @delegate;
  public readonly DynamicMethodDelegate AddEndTriggerHandler = endTrigger;
}
