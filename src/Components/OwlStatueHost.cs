// Decompiled with JetBrains decompiler
// Type: FezGame.Components.OwlStatueHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable
namespace FezGame.Components;

internal class OwlStatueHost(Game game) : GameComponent(game)
{
  public override void Initialize()
  {
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Enabled = this.LevelManager.Name == "OWL";
    if (!this.Enabled)
      return;
    int num1;
    try
    {
      num1 = int.Parse(this.GameState.SaveData.ThisLevel.ScriptingState);
    }
    catch (Exception ex)
    {
      num1 = 0;
    }
    int collectedOwls = this.GameState.SaveData.CollectedOwls;
    int num2 = 0;
    foreach (NpcInstance npcInstance in (IEnumerable<NpcInstance>) this.LevelManager.NonPlayerCharacters.Values)
    {
      if (npcInstance.ActorType == ActorType.Owl)
      {
        if (collectedOwls <= num2)
        {
          ServiceHelper.RemoveComponent<NpcState>(npcInstance.State);
        }
        else
        {
          (npcInstance.State as GameNpcState).ForceVisible = true;
          (npcInstance.State as GameNpcState).IsNightForOwl = num2 < num1;
        }
        ++num2;
      }
    }
    if (num1 == 4 && this.GameState.SaveData.ThisLevel.FilledConditions.SecretCount == 0)
    {
      Waiters.Wait((Func<bool>) (() => !this.GameState.Loading && !this.GameState.FarawaySettings.InTransition), (Action) (() => this.OwlService.OnOwlLanded()));
      this.Enabled = false;
    }
    this.GameState.SaveData.ThisLevel.ScriptingState = collectedOwls.ToString((IFormatProvider) CultureInfo.InvariantCulture);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap)
      return;
    foreach (NpcInstance npcInstance in (IEnumerable<NpcInstance>) this.LevelManager.NonPlayerCharacters.Values)
    {
      if (npcInstance.State.CurrentAction == NpcAction.Land)
      {
        this.OwlService.OnOwlLanded();
        this.Enabled = false;
        break;
      }
    }
  }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IOwlService OwlService { get; set; }
}
