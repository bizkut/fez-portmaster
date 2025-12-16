// Decompiled with JetBrains decompiler
// Type: FezGame.Services.IGameLevelManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Services;

public interface IGameLevelManager : ILevelManager
{
  IDictionary<TrileInstance, TrileGroup> PickupGroups { get; }

  string LastLevelName { get; set; }

  int? DestinationVolumeId { get; set; }

  bool DestinationIsFarAway { get; set; }

  bool WentThroughSecretPassage { get; set; }

  bool SongChanged { get; set; }

  void RemoveArtObject(ArtObjectInstance aoInstance);

  void ChangeLevel(string levelName);

  void ChangeSky(Sky sky);

  void Reset();
}
