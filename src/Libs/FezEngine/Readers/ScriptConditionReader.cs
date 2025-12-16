// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.ScriptConditionReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Scripting;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class ScriptConditionReader : ContentTypeReader<ScriptCondition>
{
  protected override ScriptCondition Read(ContentReader input, ScriptCondition existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new ScriptCondition();
    existingInstance.Object = input.ReadObject<Entity>(existingInstance.Object);
    existingInstance.Operator = input.ReadObject<ComparisonOperator>();
    existingInstance.Property = input.ReadString();
    existingInstance.Value = input.ReadString();
    existingInstance.OnDeserialization();
    return existingInstance;
  }
}
