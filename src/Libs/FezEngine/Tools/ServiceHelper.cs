// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.ServiceHelper
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Reflection;

#nullable disable
namespace FezEngine.Tools;

public static class ServiceHelper
{
  private static readonly List<object> services = new List<object>();
  private static readonly object Mutex = new object();
  public static bool IsFull;
  public static bool FirstLoadDone;

  public static void Clear()
  {
    foreach (object service in ServiceHelper.services)
    {
      foreach (Type type in service.GetType().GetInterfaces())
        ServiceHelper.Game.Services.RemoveService(type);
      if (service is IDisposable)
        (service as IDisposable).Dispose();
    }
    ServiceHelper.services.Clear();
  }

  public static void AddComponent(IGameComponent component)
  {
    ServiceHelper.AddComponent(component, false);
  }

  public static void AddComponent(IGameComponent component, bool addServices)
  {
    if (!addServices)
      ServiceHelper.InjectServices((object) component);
    lock (ServiceHelper.Mutex)
      ServiceHelper.Game.Components.Add(component);
    if (addServices)
      ServiceHelper.AddService((object) component);
    if (!TraceFlags.TraceContentLoad)
      return;
    Logger.Log(nameof (ServiceHelper), LogSeverity.Information, component.GetType().Name + " loaded");
  }

  public static T Get<T>() where T : class
  {
    return ServiceHelper.Game.Services.GetService(typeof (T)) as T;
  }

  public static object Get(Type type) => ServiceHelper.Game.Services.GetService(type);

  public static void AddService(object service)
  {
    foreach (Type type in service.GetType().GetInterfaces())
    {
      if (type != typeof (IDisposable) && type != typeof (IUpdateable) && type != typeof (IDrawable) && type != typeof (IGameComponent) && !type.Name.StartsWith("IComparable") && type.GetCustomAttributes(typeof (DisabledServiceAttribute), false).Length == 0)
        ServiceHelper.Game.Services.AddService(type, service);
    }
    ServiceHelper.services.Add(service);
  }

  public static void InitializeServices()
  {
    foreach (object service in ServiceHelper.services)
      ServiceHelper.InjectServices(service);
  }

  public static void InjectServices(object componentOrService)
  {
    Type type = componentOrService.GetType();
    do
    {
      foreach (PropertyInfo settableProperty in ReflectionHelper.GetSettableProperties(type))
      {
        ServiceDependencyAttribute firstAttribute = ReflectionHelper.GetFirstAttribute<ServiceDependencyAttribute>(settableProperty);
        if (firstAttribute != null)
        {
          Type propertyType = settableProperty.PropertyType;
          object service = ServiceHelper.Game == null ? (object) null : ServiceHelper.Game.Services.GetService(propertyType);
          if (service == null)
          {
            if (!firstAttribute.Optional)
              throw new MissingServiceException(type, propertyType);
          }
          else
            settableProperty.GetSetMethod(true).Invoke(componentOrService, new object[1]
            {
              service
            });
        }
      }
      type = type.BaseType;
    }
    while (type != typeof (object));
  }

  public static Game Game { get; set; }

  public static void RemoveComponent<T>(T component) where T : IGameComponent
  {
    if ((object) component is IDisposable)
      ((object) component as IDisposable).Dispose();
    lock (ServiceHelper.Mutex)
      ServiceHelper.Game.Components.Remove((IGameComponent) component);
  }
}
