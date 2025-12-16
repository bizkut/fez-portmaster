// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.SwitchService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

public class SwitchService : ISwitchService, IScriptingBase
{
  public event Action<int> Explode = new Action<int>(Util.NullAction<int>);

  public void OnExplode(int id) => this.Explode(id);

  public event Action<int> Push = new Action<int>(Util.NullAction<int>);

  public void OnPush(int id) => this.Push(id);

  public event Action<int> Lift = new Action<int>(Util.NullAction<int>);

  public void OnLift(int id) => this.Lift(id);

  public void Activate(int id)
  {
    this.OnExplode(id);
    this.OnPush(id);
  }

  public LongRunningAction ChangeTrile(int id, int newTrileId)
  {
    int[] oldTrileId = new int[this.LevelManager.Groups[id].Triles.Count];
    for (int index = 0; index < oldTrileId.Length; ++index)
    {
      TrileInstance trile = this.LevelManager.Groups[id].Triles[index];
      oldTrileId[index] = trile.Trile.Id;
      this.LevelManager.SwapTrile(trile, this.LevelManager.SafeGetTrile(newTrileId));
    }
    return new LongRunningAction((Action) (() =>
    {
      TrileGroup trileGroup;
      if (!this.LevelManager.Groups.TryGetValue(id, out trileGroup))
        return;
      for (int index = 0; index < oldTrileId.Length; ++index)
        this.LevelManager.SwapTrile(trileGroup.Triles[index], this.LevelManager.SafeGetTrile(oldTrileId[index]));
    }));
  }

  public void ResetEvents()
  {
    this.Explode = new Action<int>(Util.NullAction<int>);
    this.Push = new Action<int>(Util.NullAction<int>);
    this.Lift = new Action<int>(Util.NullAction<int>);
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }
}
