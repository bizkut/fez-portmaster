// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.EntityReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Scripting;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class EntityReader : ContentTypeReader<Entity>
{
  protected override Entity Read(ContentReader input, Entity existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new Entity();
    existingInstance.Type = input.ReadString();
    existingInstance.Identifier = input.ReadObject<int?>();
    return existingInstance;
  }
}
