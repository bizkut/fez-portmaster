// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.EntityTypes
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services.Scripting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#nullable disable
namespace FezEngine.Structure.Scripting;

public static class EntityTypes
{
  private const string Namespace = "FezEngine.Services.Scripting";

  public static IDictionary<string, EntityTypeDescriptor> Types { get; private set; }

  static EntityTypes()
  {
    EntityTypes.Types = (IDictionary<string, EntityTypeDescriptor>) new Dictionary<string, EntityTypeDescriptor>();
    foreach (Type interfaceType in ((IEnumerable<Type>) Assembly.GetExecutingAssembly().GetTypes()).Where<Type>((Func<Type, bool>) (x => x.Namespace == "FezEngine.Services.Scripting" && x.IsInterface && x.Implements(typeof (IScriptingBase)))))
    {
      string str = interfaceType.Name.Substring(1, interfaceType.Name.IndexOf("Service") - 1);
      EntityAttribute attribute = ((IEnumerable<object>) interfaceType.GetCustomAttributes(typeof (EntityAttribute), false)).FirstOrDefault<object>() as EntityAttribute;
      if (attribute == null)
        Logger.Log("Engine.Scripting.EntityTypes", LogSeverity.Warning, $"Entity type '{str}' did not contain any metadata, thus was not loaded.");
      else
        EntityTypes.Types.Add(str, new EntityTypeDescriptor(str, attribute.Static, attribute.Model, attribute.RestrictTo, interfaceType, ((IEnumerable<MethodInfo>) interfaceType.GetMethods()).Where<MethodInfo>((Func<MethodInfo, bool>) (m => !m.IsSpecialName && !m.Name.StartsWith("On") && !m.Name.StartsWith("get_"))).Select<MethodInfo, OperationDescriptor>((Func<MethodInfo, OperationDescriptor>) (m => new OperationDescriptor(m.Name, EntityTypes.GetDescription((MemberInfo) m), ReflectionHelper.CreateDelegate((MethodBase) m), ((IEnumerable<ParameterInfo>) m.GetParameters()).Skip<ParameterInfo>(attribute.Static ? 0 : 1).Select<ParameterInfo, ParameterDescriptor>((Func<ParameterInfo, ParameterDescriptor>) (p => new ParameterDescriptor(p.Name, p.ParameterType)))))), ((IEnumerable<PropertyInfo>) interfaceType.GetProperties()).Select<PropertyInfo, PropertyDescriptor>((Func<PropertyInfo, PropertyDescriptor>) (p => new PropertyDescriptor(p.Name, EntityTypes.GetDescription((MemberInfo) p), p.PropertyType, ReflectionHelper.CreateDelegate((MethodBase) p.GetGetMethod())))).Union<PropertyDescriptor>(((IEnumerable<MethodInfo>) interfaceType.GetMethods()).Where<MethodInfo>((Func<MethodInfo, bool>) (m => !m.IsSpecialName && m.Name.StartsWith("get_"))).Select<MethodInfo, PropertyDescriptor>((Func<MethodInfo, PropertyDescriptor>) (m => new PropertyDescriptor(m.Name.Substring(4), EntityTypes.GetDescription((MemberInfo) m), m.ReturnType, ReflectionHelper.CreateDelegate((MethodBase) m))))), ((IEnumerable<EventInfo>) interfaceType.GetEvents()).Select<EventInfo, EventDescriptor>((Func<EventInfo, EventDescriptor>) (e => new EventDescriptor(e.Name, EntityTypes.GetDescription((MemberInfo) e), ReflectionHelper.CreateDelegate((MethodBase) e.GetAddMethod()), EntityTypes.GetEndTrigger(e))))));
    }
  }

  private static string GetDescription(MemberInfo info)
  {
    return ((IEnumerable<object>) info.GetCustomAttributes(typeof (DescriptionAttribute), false)).FirstOrDefault<object>() is DescriptionAttribute descriptionAttribute ? descriptionAttribute.Description : (string) null;
  }

  private static DynamicMethodDelegate GetEndTrigger(EventInfo info)
  {
    return !(((IEnumerable<object>) info.GetCustomAttributes(typeof (EndTriggerAttribute), false)).FirstOrDefault<object>() is EndTriggerAttribute triggerAttribute) ? (DynamicMethodDelegate) null : ReflectionHelper.CreateDelegate((MethodBase) info.DeclaringType.GetEvent(triggerAttribute.Trigger).GetAddMethod());
  }
}
