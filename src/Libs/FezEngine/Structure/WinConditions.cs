// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.WinConditions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure;

public class WinConditions
{
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int LockedDoorCount;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int UnlockedDoorCount;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int ChestCount;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int CubeShardCount;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int OtherCollectibleCount;
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int SplitUpCount;
  [Serialization(Optional = true)]
  public List<int> ScriptIds = new List<int>();
  [Serialization(Optional = true, DefaultValueOptional = true)]
  public int SecretCount;

  public bool Fullfills(WinConditions wonditions)
  {
    return this.UnlockedDoorCount >= wonditions.UnlockedDoorCount && this.LockedDoorCount >= wonditions.LockedDoorCount && this.ChestCount >= wonditions.ChestCount && this.CubeShardCount >= wonditions.CubeShardCount && this.OtherCollectibleCount >= wonditions.OtherCollectibleCount && this.SplitUpCount >= wonditions.SplitUpCount && this.SecretCount >= wonditions.SecretCount && wonditions.ScriptIds.All<int>((Func<int, bool>) (x => this.ScriptIds.Contains(x)));
  }

  public WinConditions Clone()
  {
    return new WinConditions()
    {
      LockedDoorCount = this.LockedDoorCount,
      UnlockedDoorCount = this.UnlockedDoorCount,
      ChestCount = this.ChestCount,
      CubeShardCount = this.CubeShardCount,
      OtherCollectibleCount = this.OtherCollectibleCount,
      SplitUpCount = this.SplitUpCount,
      ScriptIds = new List<int>((IEnumerable<int>) this.ScriptIds),
      SecretCount = this.SecretCount
    };
  }

  public void CloneInto(WinConditions w)
  {
    w.LockedDoorCount = this.LockedDoorCount;
    w.UnlockedDoorCount = this.UnlockedDoorCount;
    w.ChestCount = this.ChestCount;
    w.CubeShardCount = this.CubeShardCount;
    w.OtherCollectibleCount = this.OtherCollectibleCount;
    w.SplitUpCount = this.SplitUpCount;
    w.SecretCount = this.SecretCount;
    w.ScriptIds.Clear();
    w.ScriptIds.AddRange((IEnumerable<int>) this.ScriptIds);
  }
}
