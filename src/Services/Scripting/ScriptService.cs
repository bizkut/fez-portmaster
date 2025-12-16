// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.ScriptService
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

public class ScriptService : IScriptService, IScriptingBase
{
  public event Action<int> Complete = new Action<int>(Util.NullAction<int>);

  public void OnComplete(int id) => this.Complete(id);

  public void SetEnabled(int id, bool enabled) => this.LevelManager.Scripts[id].Disabled = !enabled;

  public void Evaluate(int id) => this.LevelManager.Scripts[id].ScheduleEvalulation = true;

  public void ResetEvents() => this.Complete = new Action<int>(Util.NullAction<int>);

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }
}
