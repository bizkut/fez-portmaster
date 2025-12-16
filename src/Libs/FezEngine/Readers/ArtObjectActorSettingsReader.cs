// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.ArtObjectActorSettingsReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Structure.Input;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class ArtObjectActorSettingsReader : ContentTypeReader<ArtObjectActorSettings>
{
  protected override ArtObjectActorSettings Read(
    ContentReader input,
    ArtObjectActorSettings existingInstance)
  {
    if (existingInstance == (ArtObjectActorSettings) null)
      existingInstance = new ArtObjectActorSettings();
    existingInstance.Inactive = input.ReadBoolean();
    existingInstance.ContainedTrile = input.ReadObject<ActorType>();
    existingInstance.AttachedGroup = input.ReadObject<int?>();
    existingInstance.SpinView = input.ReadObject<Viewpoint>();
    existingInstance.SpinEvery = input.ReadSingle();
    existingInstance.SpinOffset = input.ReadSingle();
    existingInstance.OffCenter = input.ReadBoolean();
    existingInstance.RotationCenter = input.ReadVector3();
    existingInstance.VibrationPattern = input.ReadObject<VibrationMotor[]>();
    existingInstance.CodePattern = input.ReadObject<CodeInput[]>();
    existingInstance.Segment = input.ReadObject<PathSegment>();
    existingInstance.NextNode = input.ReadObject<int?>();
    existingInstance.DestinationLevel = input.ReadObject<string>();
    existingInstance.TreasureMapName = input.ReadObject<string>();
    existingInstance.InvisibleSides = new HashSet<FaceOrientation>((IEnumerable<FaceOrientation>) input.ReadObject<FaceOrientation[]>(), (IEqualityComparer<FaceOrientation>) FaceOrientationComparer.Default);
    existingInstance.TimeswitchWindBackSpeed = input.ReadSingle();
    return existingInstance;
  }
}
