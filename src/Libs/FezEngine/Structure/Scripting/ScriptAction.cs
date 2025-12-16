// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.ScriptAction
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using ContentSerialization;
using ContentSerialization.Attributes;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable disable
namespace FezEngine.Structure.Scripting;

public class ScriptAction : ScriptPart, IDeserializationCallback
{
  public string Operation { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Killswitch { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Blocking { get; set; }

  [Serialization(Optional = true)]
  public string[] Arguments { get; set; }

  public object[] ProcessedArguments { get; private set; }

  public DynamicMethodDelegate Invoke { get; private set; }

  public override string ToString()
  {
    if (this.Arguments == null)
      return $"{(this.Object == null ? "(none)" : this.Object.ToString())}.{this.Operation ?? "(none)"}()";
    return $"{(this.Object == null ? "(none)" : this.Object.ToString())}.{this.Operation ?? "(none)"}({Util.DeepToString<string>((IEnumerable<string>) this.Arguments, true)})";
  }

  public void OnDeserialization() => this.Process();

  public void Process()
  {
    EntityTypeDescriptor type = EntityTypes.Types[this.Object.Type];
    OperationDescriptor operation = type.Operations[this.Operation];
    int num = type.Static ? 0 : 1;
    this.ProcessedArguments = new object[num + operation.Parameters.Length];
    if (!type.Static)
      this.ProcessedArguments[0] = (object) this.Object.Identifier.Value;
    for (int index = 0; index < operation.Parameters.Length; ++index)
    {
      ParameterDescriptor parameter = operation.Parameters[index];
      this.ProcessedArguments[num + index] = this.Arguments.Length > index ? Convert.ChangeType((object) this.Arguments[index], parameter.Type, (IFormatProvider) CultureInfo.InvariantCulture) : (!(parameter.Type == typeof (string)) ? Activator.CreateInstance(parameter.Type) : (object) string.Empty);
    }
    this.Invoke = operation.Call;
  }

  public ScriptAction Clone()
  {
    ScriptAction scriptAction = new ScriptAction();
    scriptAction.Operation = this.Operation;
    scriptAction.Killswitch = this.Killswitch;
    scriptAction.Blocking = this.Blocking;
    scriptAction.Arguments = this.Arguments == null ? (string[]) null : ((IEnumerable<string>) this.Arguments).ToArray<string>();
    scriptAction.Object = this.Object == null ? (Entity) null : this.Object.Clone();
    return scriptAction;
  }
}
