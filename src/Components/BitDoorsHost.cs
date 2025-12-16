// Decompiled with JetBrains decompiler
// Type: FezGame.Components.BitDoorsHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class BitDoorsHost(Game game) : GameComponent(game)
{
  private readonly List<BitDoorState> BitDoors = new List<BitDoorState>();
  private readonly List<int> ToReactivate = new List<int>();

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanging += new Action(this.InitBitDoors);
    if (this.LevelManager.Name == null)
      return;
    this.InitBitDoors();
  }

  private void InitBitDoors()
  {
    this.ToReactivate.Clear();
    this.BitDoors.Clear();
    foreach (ArtObjectInstance artObject in this.LevelManager.ArtObjects.Values.Where<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType.IsBitDoor())))
      this.BitDoors.Add(new BitDoorState(artObject));
    foreach (int inactiveArtObject in this.GameState.SaveData.ThisLevel.InactiveArtObjects)
    {
      ArtObjectInstance door;
      if (inactiveArtObject >= 0 && this.LevelManager.ArtObjects.TryGetValue(inactiveArtObject, out door) && door.ArtObject.ActorType.IsBitDoor())
      {
        door.Position += this.BitDoors.First<BitDoorState>((Func<BitDoorState, bool>) (x => x.AoInstance == door)).GetOpenOffset();
        door.ActorSettings.Inactive = true;
        this.ToReactivate.Add(inactiveArtObject);
      }
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || this.GameState.Loading || !this.CameraManager.ActionRunning || !this.CameraManager.Viewpoint.IsOrthographic() || this.BitDoors.Count == 0)
      return;
    if (this.ToReactivate.Count > 0)
    {
      foreach (int id in this.ToReactivate)
        this.BitDoorService.OnOpen(id);
      this.ToReactivate.Clear();
    }
    foreach (BitDoorState bitDoor in this.BitDoors)
      bitDoor.Update(gameTime.ElapsedGameTime);
  }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IBitDoorService BitDoorService { private get; set; }
}
