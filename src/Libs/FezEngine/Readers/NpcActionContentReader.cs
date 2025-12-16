// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.NpcActionContentReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class NpcActionContentReader : ContentTypeReader<NpcActionContent>
{
  protected override NpcActionContent Read(ContentReader input, NpcActionContent existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new NpcActionContent();
    existingInstance.AnimationName = input.ReadObject<string>();
    existingInstance.SoundName = input.ReadObject<string>();
    return existingInstance;
  }
}
