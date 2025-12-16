// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.NpcInstanceReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class NpcInstanceReader : ContentTypeReader<NpcInstance>
{
  protected override NpcInstance Read(ContentReader input, NpcInstance existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new NpcInstance();
    existingInstance.Name = input.ReadString();
    existingInstance.Position = input.ReadVector3();
    existingInstance.DestinationOffset = input.ReadVector3();
    existingInstance.WalkSpeed = input.ReadSingle();
    existingInstance.RandomizeSpeech = input.ReadBoolean();
    existingInstance.SayFirstSpeechLineOnce = input.ReadBoolean();
    existingInstance.AvoidsGomez = input.ReadBoolean();
    existingInstance.ActorType = input.ReadObject<ActorType>();
    existingInstance.Speech = input.ReadObject<List<SpeechLine>>(existingInstance.Speech);
    existingInstance.Actions = input.ReadObject<Dictionary<NpcAction, NpcActionContent>>(existingInstance.Actions);
    return existingInstance;
  }
}
