// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.VolumeReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class VolumeReader : ContentTypeReader<Volume>
{
  protected override Volume Read(ContentReader input, Volume existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new Volume();
    existingInstance.Orientations = new HashSet<FaceOrientation>((IEnumerable<FaceOrientation>) input.ReadObject<FaceOrientation[]>(), (IEqualityComparer<FaceOrientation>) FaceOrientationComparer.Default);
    existingInstance.From = input.ReadVector3();
    existingInstance.To = input.ReadVector3();
    existingInstance.ActorSettings = input.ReadObject<VolumeActorSettings>(existingInstance.ActorSettings);
    return existingInstance;
  }
}
