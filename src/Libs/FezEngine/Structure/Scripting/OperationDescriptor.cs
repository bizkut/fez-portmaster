// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.OperationDescriptor
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure.Scripting;

public struct OperationDescriptor(
  string name,
  string description,
  DynamicMethodDelegate @delegate,
  IEnumerable<ParameterDescriptor> parameters)
{
  public readonly string Name = name;
  public readonly string Description = description;
  public readonly ParameterDescriptor[] Parameters = parameters.ToArray<ParameterDescriptor>();
  public readonly DynamicMethodDelegate Call = @delegate;
}
