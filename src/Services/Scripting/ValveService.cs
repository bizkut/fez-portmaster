// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.ValveService
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

internal class ValveService : IValveService, IScriptingBase
{
  public event Action<int> Screwed = new Action<int>(Util.NullAction<int>);

  public event Action<int> Unscrewed = new Action<int>(Util.NullAction<int>);

  public void ResetEvents()
  {
    this.Screwed = new Action<int>(Util.NullAction<int>);
    this.Unscrewed = new Action<int>(Util.NullAction<int>);
  }

  public void OnScrew(int id) => this.Screwed(id);

  public void OnUnscrew(int id) => this.Unscrewed(id);

  public void SetEnabled(int id, bool enabled)
  {
    this.LevelManager.ArtObjects[id].Enabled = enabled;
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }
}
