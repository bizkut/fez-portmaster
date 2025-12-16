// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.AmbienceTrackReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class AmbienceTrackReader : ContentTypeReader<AmbienceTrack>
{
  protected override AmbienceTrack Read(ContentReader input, AmbienceTrack existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new AmbienceTrack();
    existingInstance.Name = input.ReadObject<string>();
    existingInstance.Dawn = input.ReadBoolean();
    existingInstance.Day = input.ReadBoolean();
    existingInstance.Dusk = input.ReadBoolean();
    existingInstance.Night = input.ReadBoolean();
    return existingInstance;
  }
}
