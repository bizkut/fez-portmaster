// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.MissingSetterException
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Tools;

public class MissingSetterException(Type requiringType, Type requiredType) : Exception($"The service dependency for {requiredType} in {requiringType} could not be injected because a setter could not be found.")
{
  private const string messageFormat = "The service dependency for {0} in {1} could not be injected because a setter could not be found.";
}
