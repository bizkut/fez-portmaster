// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.ScriptActionReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Scripting;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class ScriptActionReader : ContentTypeReader<ScriptAction>
{
  protected override ScriptAction Read(ContentReader input, ScriptAction existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new ScriptAction();
    existingInstance.Object = input.ReadObject<Entity>(existingInstance.Object);
    existingInstance.Operation = input.ReadString();
    existingInstance.Arguments = input.ReadObject<string[]>(existingInstance.Arguments);
    existingInstance.Killswitch = input.ReadBoolean();
    existingInstance.Blocking = input.ReadBoolean();
    existingInstance.OnDeserialization();
    return existingInstance;
  }
}
