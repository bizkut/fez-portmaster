// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.PivotService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Tools;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

internal class PivotService : IPivotService, IScriptingBase
{
  public event Action<int> RotatedRight = new Action<int>(Util.NullAction<int>);

  public event Action<int> RotatedLeft = new Action<int>(Util.NullAction<int>);

  public void ResetEvents()
  {
    this.RotatedRight = new Action<int>(Util.NullAction<int>);
    this.RotatedLeft = new Action<int>(Util.NullAction<int>);
  }

  public void OnRotateRight(int id) => this.RotatedRight(id);

  public void OnRotateLeft(int id) => this.RotatedLeft(id);

  public int get_Turns(int id)
  {
    int num;
    return !this.GameState.SaveData.ThisLevel.PivotRotations.TryGetValue(id, out num) ? 0 : num;
  }

  public void SetEnabled(int id, bool enabled)
  {
    this.LevelManager.ArtObjects[id].Enabled = enabled;
  }

  public void RotateTo(int id, int turns)
  {
    this.GameState.SaveData.ThisLevel.PivotRotations[id] = turns;
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }
}
