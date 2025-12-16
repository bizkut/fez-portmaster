// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.TrileInstanceReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class TrileInstanceReader : ContentTypeReader<TrileInstance>
{
  protected override TrileInstance Read(ContentReader input, TrileInstance existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new TrileInstance();
    existingInstance.Position = input.ReadVector3();
    existingInstance.TrileId = input.ReadInt32();
    byte orientation = input.ReadByte();
    existingInstance.SetPhiLight(orientation);
    if (input.ReadBoolean())
      existingInstance.ActorSettings = input.ReadObject<InstanceActorSettings>(existingInstance.ActorSettings);
    existingInstance.OverlappedTriles = input.ReadObject<List<TrileInstance>>(existingInstance.OverlappedTriles);
    return existingInstance;
  }
}
