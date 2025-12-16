// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GameSequencer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

internal class GameSequencer(Game game) : Sequencer(game)
{
  protected override void OnDisappear(TrileInstance crystal)
  {
    if (this.PlayerManager.HeldInstance != crystal)
      return;
    this.PlayerManager.HeldInstance = (TrileInstance) null;
    this.PlayerManager.Action = ActionType.Idle;
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }
}
