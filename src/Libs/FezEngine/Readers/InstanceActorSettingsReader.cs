// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.InstanceActorSettingsReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class InstanceActorSettingsReader : ContentTypeReader<InstanceActorSettings>
{
  protected override InstanceActorSettings Read(
    ContentReader input,
    InstanceActorSettings existingInstance)
  {
    if (existingInstance == (InstanceActorSettings) null)
      existingInstance = new InstanceActorSettings();
    existingInstance.ContainedTrile = input.ReadObject<int?>();
    existingInstance.SignText = input.ReadObject<string>(existingInstance.SignText);
    existingInstance.Sequence = input.ReadObject<bool[]>(existingInstance.Sequence);
    existingInstance.SequenceSampleName = input.ReadObject<string>(existingInstance.SequenceSampleName);
    existingInstance.SequenceAlternateSampleName = input.ReadObject<string>(existingInstance.SequenceAlternateSampleName);
    existingInstance.HostVolume = input.ReadObject<int?>();
    return existingInstance;
  }
}
