// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.DotDialogueLineReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class DotDialogueLineReader : ContentTypeReader<DotDialogueLine>
{
  protected override DotDialogueLine Read(ContentReader input, DotDialogueLine existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new DotDialogueLine();
    existingInstance.ResourceText = input.ReadObject<string>();
    existingInstance.Grouped = input.ReadBoolean();
    return existingInstance;
  }
}
