// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.MovementPathReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class MovementPathReader : ContentTypeReader<MovementPath>
{
  protected override MovementPath Read(ContentReader input, MovementPath existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new MovementPath();
    existingInstance.Segments = input.ReadObject<List<PathSegment>>(existingInstance.Segments);
    existingInstance.NeedsTrigger = input.ReadBoolean();
    existingInstance.EndBehavior = input.ReadObject<PathEndBehavior>();
    existingInstance.SoundName = input.ReadObject<string>();
    existingInstance.IsSpline = input.ReadBoolean();
    existingInstance.OffsetSeconds = input.ReadSingle();
    existingInstance.SaveTrigger = input.ReadBoolean();
    return existingInstance;
  }
}
