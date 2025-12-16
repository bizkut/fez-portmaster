// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.NpcMetadataReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class NpcMetadataReader : ContentTypeReader<NpcMetadata>
{
  protected override NpcMetadata Read(ContentReader input, NpcMetadata existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new NpcMetadata();
    existingInstance.WalkSpeed = input.ReadSingle();
    existingInstance.AvoidsGomez = input.ReadBoolean();
    existingInstance.SoundPath = input.ReadObject<string>();
    existingInstance.SoundActions = input.ReadObject<List<NpcAction>>(existingInstance.SoundActions);
    return existingInstance;
  }
}
