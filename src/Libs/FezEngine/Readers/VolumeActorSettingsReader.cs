// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.VolumeActorSettingsReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Structure.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class VolumeActorSettingsReader : ContentTypeReader<VolumeActorSettings>
{
  protected override VolumeActorSettings Read(
    ContentReader input,
    VolumeActorSettings existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new VolumeActorSettings();
    existingInstance.FarawayPlaneOffset = input.ReadVector2();
    existingInstance.IsPointOfInterest = input.ReadBoolean();
    existingInstance.DotDialogue = input.ReadObject<List<DotDialogueLine>>(existingInstance.DotDialogue);
    existingInstance.WaterLocked = input.ReadBoolean();
    existingInstance.CodePattern = input.ReadObject<CodeInput[]>();
    existingInstance.IsBlackHole = input.ReadBoolean();
    existingInstance.NeedsTrigger = input.ReadBoolean();
    existingInstance.IsSecretPassage = input.ReadBoolean();
    return existingInstance;
  }
}
