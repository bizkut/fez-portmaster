// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.ScriptTriggerReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Scripting;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class ScriptTriggerReader : ContentTypeReader<ScriptTrigger>
{
  protected override ScriptTrigger Read(ContentReader input, ScriptTrigger existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new ScriptTrigger();
    existingInstance.Object = input.ReadObject<Entity>(existingInstance.Object);
    existingInstance.Event = input.ReadString();
    return existingInstance;
  }
}
