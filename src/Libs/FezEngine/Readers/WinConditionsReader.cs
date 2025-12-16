// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.WinConditionsReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class WinConditionsReader : ContentTypeReader<WinConditions>
{
  protected override WinConditions Read(ContentReader input, WinConditions existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new WinConditions();
    existingInstance.ChestCount = input.ReadInt32();
    existingInstance.LockedDoorCount = input.ReadInt32();
    existingInstance.UnlockedDoorCount = input.ReadInt32();
    existingInstance.ScriptIds = input.ReadObject<List<int>>(existingInstance.ScriptIds);
    existingInstance.CubeShardCount = input.ReadInt32();
    existingInstance.OtherCollectibleCount = input.ReadInt32();
    existingInstance.SplitUpCount = input.ReadInt32();
    existingInstance.SecretCount = input.ReadInt32();
    return existingInstance;
  }
}
