// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.SpeechLineReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class SpeechLineReader : ContentTypeReader<SpeechLine>
{
  protected override SpeechLine Read(ContentReader input, SpeechLine existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new SpeechLine();
    existingInstance.Text = input.ReadObject<string>();
    existingInstance.OverrideContent = input.ReadObject<NpcActionContent>(existingInstance.OverrideContent);
    return existingInstance;
  }
}
