// Decompiled with JetBrains decompiler
// Type: Common.ReflectionHelper
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

#nullable disable
namespace Common;

public static class ReflectionHelper
{
  private static readonly Type[] EmptyTypes = new Type[0];
  public const BindingFlags PublicInstanceMembers = BindingFlags.Instance | BindingFlags.Public;
  private static readonly Dictionary<HandlePair<Type, Type>, Attribute> typeAttributeCache = new Dictionary<HandlePair<Type, Type>, Attribute>();
  private static readonly Dictionary<HandlePair<PropertyInfo, Type>, Attribute> propertyAttributeCache = new Dictionary<HandlePair<PropertyInfo, Type>, Attribute>();
  private static readonly Dictionary<HandlePair<FieldInfo, Type>, Attribute> fieldAttributeCache = new Dictionary<HandlePair<FieldInfo, Type>, Attribute>();
  private static readonly Dictionary<Type, MemberInfo[]> propertyCache = new Dictionary<Type, MemberInfo[]>();
  private static readonly Dictionary<Type, MemberInfo[]> serviceCache = new Dictionary<Type, MemberInfo[]>();
  private static readonly Dictionary<MethodInfo, DynamicMethodDelegate> methodCache = new Dictionary<MethodInfo, DynamicMethodDelegate>();
  private static readonly Dictionary<Type, DynamicMethodDelegate> constructorCache = new Dictionary<Type, DynamicMethodDelegate>();

  public static T GetFirstAttribute<T>(Type type) where T : Attribute, new()
  {
    Type second = typeof (T);
    HandlePair<Type, Type> key = new HandlePair<Type, Type>(type, second);
    Attribute firstAttribute;
    lock (ReflectionHelper.typeAttributeCache)
    {
      if (!ReflectionHelper.typeAttributeCache.TryGetValue(key, out firstAttribute))
        ReflectionHelper.typeAttributeCache.Add(key, firstAttribute = (Attribute) ((IEnumerable<object>) type.GetCustomAttributes(typeof (T), false)).FirstOrDefault<object>());
    }
    return firstAttribute as T;
  }

  public static T GetFirstAttribute<T>(PropertyInfo propInfo) where T : Attribute, new()
  {
    Type second = typeof (T);
    HandlePair<PropertyInfo, Type> key = new HandlePair<PropertyInfo, Type>(propInfo, second);
    Attribute firstAttribute;
    lock (ReflectionHelper.propertyAttributeCache)
    {
      if (!ReflectionHelper.propertyAttributeCache.TryGetValue(key, out firstAttribute))
        ReflectionHelper.propertyAttributeCache.Add(key, firstAttribute = (Attribute) ((IEnumerable<object>) propInfo.GetCustomAttributes(typeof (T), false)).FirstOrDefault<object>());
    }
    return firstAttribute as T;
  }

  public static T GetFirstAttribute<T>(FieldInfo fieldInfo) where T : Attribute, new()
  {
    Type second = typeof (T);
    HandlePair<FieldInfo, Type> key = new HandlePair<FieldInfo, Type>(fieldInfo, second);
    Attribute firstAttribute;
    lock (ReflectionHelper.fieldAttributeCache)
    {
      if (!ReflectionHelper.fieldAttributeCache.TryGetValue(key, out firstAttribute))
        ReflectionHelper.fieldAttributeCache.Add(key, firstAttribute = (Attribute) ((IEnumerable<object>) fieldInfo.GetCustomAttributes(typeof (T), false)).FirstOrDefault<object>());
    }
    return firstAttribute as T;
  }

  public static T GetFirstAttribute<T>(MemberInfo memberInfo) where T : Attribute, new()
  {
    return (object) (memberInfo as PropertyInfo) == null ? ReflectionHelper.GetFirstAttribute<T>(memberInfo as FieldInfo) : ReflectionHelper.GetFirstAttribute<T>(memberInfo as PropertyInfo);
  }

  public static MemberInfo[] GetSerializableMembers(Type type)
  {
    MemberInfo[] array;
    lock (ReflectionHelper.propertyCache)
    {
      if (!ReflectionHelper.propertyCache.TryGetValue(type, out array))
      {
        array = ((IEnumerable<PropertyInfo>) type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy)).Where<PropertyInfo>((Func<PropertyInfo, bool>) (p => p.GetGetMethod() != (MethodInfo) null && p.GetSetMethod() != (MethodInfo) null && p.GetGetMethod().GetParameters().Length == 0)).Cast<MemberInfo>().Union<MemberInfo>(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.FlattenHierarchy).Cast<MemberInfo>()).ToArray<MemberInfo>();
        ReflectionHelper.propertyCache.Add(type, array);
      }
    }
    return array;
  }

  public static MemberInfo[] GetSettableProperties(Type type)
  {
    MemberInfo[] array;
    lock (ReflectionHelper.serviceCache)
    {
      if (!ReflectionHelper.serviceCache.TryGetValue(type, out array))
      {
        array = (MemberInfo[]) ((IEnumerable<PropertyInfo>) type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)).Where<PropertyInfo>((Func<PropertyInfo, bool>) (p => p.GetSetMethod(true) != (MethodInfo) null)).ToArray<PropertyInfo>();
        ReflectionHelper.serviceCache.Add(type, array);
      }
    }
    return array;
  }

  public static Type GetMemberType(MemberInfo member)
  {
    if ((object) (member as PropertyInfo) != null)
      return (member as PropertyInfo).PropertyType;
    return (object) (member as FieldInfo) != null ? (member as FieldInfo).FieldType : throw new NotImplementedException();
  }

  public static bool IsGenericSet(Type type)
  {
    return ((IEnumerable<Type>) type.GetInterfaces()).Any<Type>((Func<Type, bool>) (i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ISet<>)));
  }

  public static bool IsGenericList(Type type)
  {
    return ((IEnumerable<Type>) type.GetInterfaces()).Any<Type>((Func<Type, bool>) (i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IList<>)));
  }

  public static bool IsGenericCollection(Type type)
  {
    return ((IEnumerable<Type>) type.GetInterfaces()).Any<Type>((Func<Type, bool>) (i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (ICollection<>)));
  }

  public static bool IsGenericDictionary(Type type)
  {
    return ((IEnumerable<Type>) type.GetInterfaces()).Any<Type>((Func<Type, bool>) (i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof (IDictionary<,>)));
  }

  public static bool IsNullable(Type type)
  {
    return type.IsGenericType && type.GetGenericTypeDefinition() == typeof (Nullable<>);
  }

  public static DynamicMethodDelegate CreateDelegate(MethodBase method)
  {
    ParameterInfo[] parameters = method.GetParameters();
    int length = parameters.Length;
    DynamicMethod dynamicMethod = new DynamicMethod("", typeof (object), new Type[2]
    {
      typeof (object),
      typeof (object[])
    }, typeof (ReflectionHelper).Module, true);
    ILGenerator ilGenerator = dynamicMethod.GetILGenerator();
    Label label = ilGenerator.DefineLabel();
    ilGenerator.Emit(OpCodes.Ldarg_1);
    ilGenerator.Emit(OpCodes.Ldlen);
    ilGenerator.Emit(OpCodes.Ldc_I4, length);
    ilGenerator.Emit(OpCodes.Beq, label);
    ilGenerator.Emit(OpCodes.Newobj, typeof (TargetParameterCountException).GetConstructor(Type.EmptyTypes));
    ilGenerator.Emit(OpCodes.Throw);
    ilGenerator.MarkLabel(label);
    if (!method.IsStatic && !method.IsConstructor)
    {
      ilGenerator.Emit(OpCodes.Ldarg_0);
      if (method.DeclaringType.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox, method.DeclaringType);
    }
    for (int index = 0; index < length; ++index)
    {
      ilGenerator.Emit(OpCodes.Ldarg_1);
      ilGenerator.Emit(OpCodes.Ldc_I4, index);
      ilGenerator.Emit(OpCodes.Ldelem_Ref);
      Type parameterType = parameters[index].ParameterType;
      if (parameterType.IsValueType)
        ilGenerator.Emit(OpCodes.Unbox_Any, parameterType);
    }
    if (method.IsConstructor)
      ilGenerator.Emit(OpCodes.Newobj, method as ConstructorInfo);
    else if (method.IsFinal || !method.IsVirtual)
      ilGenerator.Emit(OpCodes.Call, method as MethodInfo);
    else
      ilGenerator.Emit(OpCodes.Callvirt, method as MethodInfo);
    Type cls = method.IsConstructor ? method.DeclaringType : (method as MethodInfo).ReturnType;
    if (cls != typeof (void))
    {
      if (cls.IsValueType)
        ilGenerator.Emit(OpCodes.Box, cls);
    }
    else
      ilGenerator.Emit(OpCodes.Ldnull);
    ilGenerator.Emit(OpCodes.Ret);
    return (DynamicMethodDelegate) dynamicMethod.CreateDelegate(typeof (DynamicMethodDelegate));
  }

  public static object Instantiate(Type type)
  {
    if (type.IsValueType)
      return Activator.CreateInstance(type);
    if (type.IsArray)
      return (object) Array.CreateInstance(type.GetElementType(), 0);
    DynamicMethodDelegate dynamicMethodDelegate;
    lock (ReflectionHelper.constructorCache)
    {
      if (!ReflectionHelper.constructorCache.TryGetValue(type, out dynamicMethodDelegate))
      {
        dynamicMethodDelegate = ReflectionHelper.CreateDelegate((MethodBase) type.GetConstructor(ReflectionHelper.EmptyTypes));
        ReflectionHelper.constructorCache.Add(type, dynamicMethodDelegate);
      }
    }
    return dynamicMethodDelegate((object) null);
  }

  public static DynamicMethodDelegate GetDelegate(MethodInfo info)
  {
    DynamicMethodDelegate dynamicMethodDelegate;
    lock (ReflectionHelper.methodCache)
    {
      if (!ReflectionHelper.methodCache.TryGetValue(info, out dynamicMethodDelegate))
      {
        dynamicMethodDelegate = ReflectionHelper.CreateDelegate((MethodBase) info);
        ReflectionHelper.methodCache.Add(info, dynamicMethodDelegate);
      }
    }
    return dynamicMethodDelegate;
  }

  public static object InvokeMethod(
    MethodInfo info,
    object targetInstance,
    params object[] arguments)
  {
    return ReflectionHelper.GetDelegate(info)(targetInstance, arguments);
  }

  public static object GetValue(PropertyInfo member, object instance)
  {
    return ReflectionHelper.InvokeMethod(member.GetGetMethod(true), instance);
  }

  public static object GetValue(MemberInfo member, object instance)
  {
    if ((object) (member as PropertyInfo) != null)
      return ReflectionHelper.GetValue(member as PropertyInfo, instance);
    return (object) (member as FieldInfo) != null ? (member as FieldInfo).GetValue(instance) : throw new NotImplementedException();
  }

  public static void SetValue(PropertyInfo member, object instance, object value)
  {
    ReflectionHelper.InvokeMethod(member.GetSetMethod(true), instance, value);
  }

  public static void SetValue(MemberInfo member, object instance, object value)
  {
    if ((object) (member as PropertyInfo) != null)
    {
      ReflectionHelper.SetValue(member as PropertyInfo, instance, value);
    }
    else
    {
      if ((object) (member as FieldInfo) == null)
        throw new NotImplementedException();
      (member as FieldInfo).SetValue(instance, value);
    }
  }

  public static string GetShortAssemblyQualifiedName<T>()
  {
    return ReflectionHelper.GetShortAssemblyQualifiedName(typeof (T));
  }

  public static string GetShortAssemblyQualifiedName(Type type)
  {
    return ReflectionHelper.GetShortAssemblyQualifiedName(type.AssemblyQualifiedName);
  }

  public static string GetShortAssemblyQualifiedName(string assemblyQName)
  {
    int num;
    int startIndex;
    for (; assemblyQName.Contains(", Version"); assemblyQName = assemblyQName.Substring(0, num) + assemblyQName.Substring(startIndex))
    {
      num = assemblyQName.IndexOf(", Version");
      startIndex = assemblyQName.IndexOf("],", num);
      if (startIndex == -1)
        startIndex = assemblyQName.Length;
      if (assemblyQName[startIndex - 1] == ']')
        --startIndex;
    }
    return assemblyQName;
  }
}
