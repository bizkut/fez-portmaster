// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.EntityTypeDescriptor
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure.Scripting;

public struct EntityTypeDescriptor
{
  public readonly string Name;
  public readonly bool Static;
  public readonly Type Model;
  public readonly ActorType[] RestrictTo;
  public readonly Type Interface;
  public readonly IDictionary<string, OperationDescriptor> Operations;
  public readonly IDictionary<string, PropertyDescriptor> Properties;
  public readonly IDictionary<string, EventDescriptor> Events;

  public EntityTypeDescriptor(
    string name,
    bool isStatic,
    Type modelType,
    ActorType[] restrictTo,
    Type interfaceType,
    IEnumerable<OperationDescriptor> operations,
    IEnumerable<PropertyDescriptor> properties,
    IEnumerable<EventDescriptor> events)
  {
    this.Name = name;
    this.Static = isStatic;
    this.Model = modelType;
    this.RestrictTo = restrictTo;
    this.Interface = interfaceType;
    this.Operations = (IDictionary<string, OperationDescriptor>) new Dictionary<string, OperationDescriptor>();
    foreach (OperationDescriptor operation in operations)
      this.Operations.Add(operation.Name, operation);
    this.Properties = (IDictionary<string, PropertyDescriptor>) new Dictionary<string, PropertyDescriptor>();
    foreach (PropertyDescriptor property in properties)
      this.Properties.Add(property.Name, property);
    this.Events = (IDictionary<string, EventDescriptor>) new Dictionary<string, EventDescriptor>();
    foreach (EventDescriptor eventDescriptor in events)
      this.Events.Add(eventDescriptor.Name, eventDescriptor);
  }
}
