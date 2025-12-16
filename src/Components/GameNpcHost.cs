// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GameNpcHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

internal class GameNpcHost(Game game) : NpcHost(game)
{
  protected override NpcState CreateNpcState(NpcInstance npc)
  {
    return npc.ActorType == ActorType.Owl && this.GameState.SaveData.ThisLevel.InactiveNPCs.Contains(npc.Id) ? (NpcState) null : (NpcState) new GameNpcState(this.Game, npc);
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }
}
