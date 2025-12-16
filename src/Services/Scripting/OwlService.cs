// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.OwlService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services.Scripting;
using FezEngine.Tools;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

internal class OwlService : IOwlService, IScriptingBase
{
  public void ResetEvents()
  {
    this.OwlCollected = (Action) null;
    this.OwlLanded = (Action) null;
  }

  public event Action OwlCollected;

  public event Action OwlLanded;

  public void OnOwlCollected()
  {
    if (this.OwlCollected == null)
      return;
    this.OwlCollected();
  }

  public void OnOwlLanded()
  {
    if (this.OwlLanded == null)
      return;
    this.OwlLanded();
  }

  public int OwlsCollected => this.GameState.SaveData.CollectedOwls;

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }
}
