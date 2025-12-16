// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.PathService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Tools;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

internal class PathService : IPathService, IScriptingBase
{
  public LongRunningAction Start(int id, bool inTransition, bool outTransition)
  {
    this.LevelManager.Paths[id].NeedsTrigger = false;
    this.LevelManager.Paths[id].RunOnce = true;
    this.LevelManager.Paths[id].InTransition = inTransition;
    this.LevelManager.Paths[id].OutTransition = outTransition;
    return new LongRunningAction((Func<float, float, bool>) ((elapsed, sinceStarted) => this.LevelManager.Paths[id].NeedsTrigger));
  }

  public void ResetEvents()
  {
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }
}
