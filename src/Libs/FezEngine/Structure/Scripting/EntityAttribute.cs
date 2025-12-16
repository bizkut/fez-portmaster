// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.EntityAttribute
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Structure.Scripting;

[AttributeUsage(AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public class EntityAttribute : Attribute
{
  public Type Model { get; set; }

  public ActorType[] RestrictTo { get; set; }

  public bool Static { get; set; }
}
