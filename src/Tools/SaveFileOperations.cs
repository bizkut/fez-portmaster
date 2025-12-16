// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.SaveFileOperations
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using System;
using System.Collections.Generic;
using System.IO;

#nullable disable
namespace FezGame.Tools;

public static class SaveFileOperations
{
  private const long Version = 6;

  public static void Write(CrcWriter w, SaveData sd)
  {
    w.Write(6L);
    w.Write(sd.CreationTime);
    w.Write(sd.Finished32);
    w.Write(sd.Finished64);
    w.Write(sd.HasFPView);
    w.Write(sd.HasStereo3D);
    w.Write(sd.CanNewGamePlus);
    w.Write(sd.IsNewGamePlus);
    w.Write(sd.OneTimeTutorials.Count);
    foreach (KeyValuePair<string, bool> oneTimeTutorial in sd.OneTimeTutorials)
    {
      w.WriteObject(oneTimeTutorial.Key);
      w.Write(oneTimeTutorial.Value);
    }
    w.WriteObject(sd.Level);
    w.Write((int) sd.View);
    w.Write(sd.Ground);
    w.Write(sd.TimeOfDay);
    w.Write(sd.UnlockedWarpDestinations.Count);
    foreach (string unlockedWarpDestination in sd.UnlockedWarpDestinations)
      w.WriteObject(unlockedWarpDestination);
    w.Write(sd.Keys);
    w.Write(sd.CubeShards);
    w.Write(sd.SecretCubes);
    w.Write(sd.CollectedParts);
    w.Write(sd.CollectedOwls);
    w.Write(sd.PiecesOfHeart);
    w.Write(sd.Maps.Count);
    foreach (string map in sd.Maps)
      w.WriteObject(map);
    w.Write(sd.Artifacts.Count);
    foreach (ActorType artifact in sd.Artifacts)
      w.Write((int) artifact);
    w.Write(sd.EarnedAchievements.Count);
    foreach (string earnedAchievement in sd.EarnedAchievements)
      w.WriteObject(earnedAchievement);
    w.Write(sd.EarnedGamerPictures.Count);
    foreach (string earnedGamerPicture in sd.EarnedGamerPictures)
      w.WriteObject(earnedGamerPicture);
    w.WriteObject(sd.ScriptingState);
    w.Write(sd.FezHidden);
    w.WriteObject(sd.GlobalWaterLevelModifier);
    w.Write(sd.HasHadMapHelp);
    w.Write(sd.CanOpenMap);
    w.Write(sd.AchievementCheatCodeDone);
    w.Write(sd.AnyCodeDeciphered);
    w.Write(sd.MapCheatCodeDone);
    w.Write(sd.World.Count);
    foreach (KeyValuePair<string, LevelSaveData> keyValuePair in sd.World)
    {
      w.WriteObject(keyValuePair.Key);
      SaveFileOperations.Write(w, keyValuePair.Value);
    }
    w.Write(sd.ScoreDirty);
    w.Write(sd.HasDoneHeartReboot);
    w.Write(sd.PlayTime);
    w.Write(sd.IsNew);
  }

  public static void Write(CrcWriter w, LevelSaveData lsd)
  {
    w.Write(lsd.DestroyedTriles.Count);
    foreach (TrileEmplacement destroyedTrile in lsd.DestroyedTriles)
      w.Write(destroyedTrile);
    w.Write(lsd.InactiveTriles.Count);
    foreach (TrileEmplacement inactiveTrile in lsd.InactiveTriles)
      w.Write(inactiveTrile);
    w.Write(lsd.InactiveArtObjects.Count);
    foreach (int inactiveArtObject in lsd.InactiveArtObjects)
      w.Write(inactiveArtObject);
    w.Write(lsd.InactiveEvents.Count);
    foreach (int inactiveEvent in lsd.InactiveEvents)
      w.Write(inactiveEvent);
    w.Write(lsd.InactiveGroups.Count);
    foreach (int inactiveGroup in lsd.InactiveGroups)
      w.Write(inactiveGroup);
    w.Write(lsd.InactiveVolumes.Count);
    foreach (int inactiveVolume in lsd.InactiveVolumes)
      w.Write(inactiveVolume);
    w.Write(lsd.InactiveNPCs.Count);
    foreach (int inactiveNpC in lsd.InactiveNPCs)
      w.Write(inactiveNpC);
    w.Write(lsd.PivotRotations.Count);
    foreach (KeyValuePair<int, int> pivotRotation in lsd.PivotRotations)
    {
      w.Write(pivotRotation.Key);
      w.Write(pivotRotation.Value);
    }
    w.WriteObject(lsd.LastStableLiquidHeight);
    w.WriteObject(lsd.ScriptingState);
    w.Write(lsd.FirstVisit);
    SaveFileOperations.Write(w, lsd.FilledConditions);
  }

  public static void Write(CrcWriter w, WinConditions wc)
  {
    w.Write(wc.LockedDoorCount);
    w.Write(wc.UnlockedDoorCount);
    w.Write(wc.ChestCount);
    w.Write(wc.CubeShardCount);
    w.Write(wc.OtherCollectibleCount);
    w.Write(wc.SplitUpCount);
    w.Write(wc.ScriptIds.Count);
    foreach (int scriptId in wc.ScriptIds)
      w.Write(scriptId);
    w.Write(wc.SecretCount);
  }

  public static SaveData Read(CrcReader r)
  {
    SaveData saveData = new SaveData();
    long num1 = r.ReadInt64();
    if (num1 != 6L)
      throw new IOException($"Invalid version : {(object) num1} (expected {(object) 6L})");
    saveData.CreationTime = r.ReadInt64();
    saveData.Finished32 = r.ReadBoolean();
    saveData.Finished64 = r.ReadBoolean();
    saveData.HasFPView = r.ReadBoolean();
    saveData.HasStereo3D = r.ReadBoolean();
    saveData.CanNewGamePlus = r.ReadBoolean();
    saveData.IsNewGamePlus = r.ReadBoolean();
    saveData.OneTimeTutorials.Clear();
    int num2;
    saveData.OneTimeTutorials = new Dictionary<string, bool>(num2 = r.ReadInt32());
    for (int index = 0; index < num2; ++index)
      saveData.OneTimeTutorials.Add(r.ReadNullableString(), r.ReadBoolean());
    saveData.Level = r.ReadNullableString();
    saveData.View = (Viewpoint) r.ReadInt32();
    saveData.Ground = r.ReadVector3();
    saveData.TimeOfDay = r.ReadTimeSpan();
    int num3;
    saveData.UnlockedWarpDestinations = new List<string>(num3 = r.ReadInt32());
    for (int index = 0; index < num3; ++index)
      saveData.UnlockedWarpDestinations.Add(r.ReadNullableString());
    saveData.Keys = r.ReadInt32();
    saveData.CubeShards = r.ReadInt32();
    saveData.SecretCubes = r.ReadInt32();
    saveData.CollectedParts = r.ReadInt32();
    saveData.CollectedOwls = r.ReadInt32();
    saveData.PiecesOfHeart = r.ReadInt32();
    if (saveData.SecretCubes > 32 /*0x20*/ || saveData.CubeShards > 32 /*0x20*/ || saveData.PiecesOfHeart > 3)
      saveData.ScoreDirty = true;
    saveData.SecretCubes = Math.Min(saveData.SecretCubes, 32 /*0x20*/);
    saveData.CubeShards = Math.Min(saveData.CubeShards, 32 /*0x20*/);
    saveData.PiecesOfHeart = Math.Min(saveData.PiecesOfHeart, 3);
    int num4;
    saveData.Maps = new List<string>(num4 = r.ReadInt32());
    for (int index = 0; index < num4; ++index)
      saveData.Maps.Add(r.ReadNullableString());
    int num5;
    saveData.Artifacts = new List<ActorType>(num5 = r.ReadInt32());
    for (int index = 0; index < num5; ++index)
      saveData.Artifacts.Add((ActorType) r.ReadInt32());
    int num6;
    saveData.EarnedAchievements = new List<string>(num6 = r.ReadInt32());
    for (int index = 0; index < num6; ++index)
      saveData.EarnedAchievements.Add(r.ReadNullableString());
    int num7;
    saveData.EarnedGamerPictures = new List<string>(num7 = r.ReadInt32());
    for (int index = 0; index < num7; ++index)
      saveData.EarnedGamerPictures.Add(r.ReadNullableString());
    saveData.ScriptingState = r.ReadNullableString();
    saveData.FezHidden = r.ReadBoolean();
    saveData.GlobalWaterLevelModifier = r.ReadNullableSingle();
    saveData.HasHadMapHelp = r.ReadBoolean();
    saveData.CanOpenMap = r.ReadBoolean();
    saveData.AchievementCheatCodeDone = r.ReadBoolean();
    saveData.AnyCodeDeciphered = r.ReadBoolean();
    saveData.MapCheatCodeDone = r.ReadBoolean();
    int num8;
    saveData.World = new Dictionary<string, LevelSaveData>(num8 = r.ReadInt32());
    for (int index = 0; index < num8; ++index)
    {
      try
      {
        saveData.World.Add(r.ReadNullableString(), SaveFileOperations.ReadLevel(r));
      }
      catch (Exception ex)
      {
        break;
      }
    }
    r.ReadBoolean();
    saveData.ScoreDirty = true;
    saveData.HasDoneHeartReboot = r.ReadBoolean();
    saveData.PlayTime = r.ReadInt64();
    saveData.IsNew = string.IsNullOrEmpty(saveData.Level) || saveData.CanNewGamePlus || saveData.World.Count == 0;
    saveData.HasFPView |= saveData.HasStereo3D;
    return saveData;
  }

  public static LevelSaveData ReadLevel(CrcReader r)
  {
    LevelSaveData levelSaveData = new LevelSaveData();
    int num1;
    levelSaveData.DestroyedTriles = new List<TrileEmplacement>(num1 = r.ReadInt32());
    for (int index = 0; index < num1; ++index)
      levelSaveData.DestroyedTriles.Add(r.ReadTrileEmplacement());
    int num2;
    levelSaveData.InactiveTriles = new List<TrileEmplacement>(num2 = r.ReadInt32());
    for (int index = 0; index < num2; ++index)
      levelSaveData.InactiveTriles.Add(r.ReadTrileEmplacement());
    int num3;
    levelSaveData.InactiveArtObjects = new List<int>(num3 = r.ReadInt32());
    for (int index = 0; index < num3; ++index)
      levelSaveData.InactiveArtObjects.Add(r.ReadInt32());
    int num4;
    levelSaveData.InactiveEvents = new List<int>(num4 = r.ReadInt32());
    for (int index = 0; index < num4; ++index)
      levelSaveData.InactiveEvents.Add(r.ReadInt32());
    int num5;
    levelSaveData.InactiveGroups = new List<int>(num5 = r.ReadInt32());
    for (int index = 0; index < num5; ++index)
      levelSaveData.InactiveGroups.Add(r.ReadInt32());
    int num6;
    levelSaveData.InactiveVolumes = new List<int>(num6 = r.ReadInt32());
    for (int index = 0; index < num6; ++index)
      levelSaveData.InactiveVolumes.Add(r.ReadInt32());
    int num7;
    levelSaveData.InactiveNPCs = new List<int>(num7 = r.ReadInt32());
    for (int index = 0; index < num7; ++index)
      levelSaveData.InactiveNPCs.Add(r.ReadInt32());
    int num8;
    levelSaveData.PivotRotations = new Dictionary<int, int>(num8 = r.ReadInt32());
    for (int index = 0; index < num8; ++index)
      levelSaveData.PivotRotations.Add(r.ReadInt32(), r.ReadInt32());
    levelSaveData.LastStableLiquidHeight = r.ReadNullableSingle();
    levelSaveData.ScriptingState = r.ReadNullableString();
    levelSaveData.FirstVisit = r.ReadBoolean();
    levelSaveData.FilledConditions = SaveFileOperations.ReadWonditions(r);
    return levelSaveData;
  }

  public static WinConditions ReadWonditions(CrcReader r)
  {
    WinConditions winConditions = new WinConditions();
    winConditions.LockedDoorCount = r.ReadInt32();
    winConditions.UnlockedDoorCount = r.ReadInt32();
    winConditions.ChestCount = r.ReadInt32();
    winConditions.CubeShardCount = r.ReadInt32();
    winConditions.OtherCollectibleCount = r.ReadInt32();
    winConditions.SplitUpCount = r.ReadInt32();
    int num;
    winConditions.ScriptIds = new List<int>(num = r.ReadInt32());
    for (int index = 0; index < num; ++index)
      winConditions.ScriptIds.Add(r.ReadInt32());
    winConditions.SecretCount = r.ReadInt32();
    return winConditions;
  }
}
