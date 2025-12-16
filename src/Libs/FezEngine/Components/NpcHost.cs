// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.NpcHost
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Components;

public class NpcHost : GameComponent
{
  protected readonly List<NpcState> NpcStates = new List<NpcState>();

  protected NpcHost(Game game)
    : base(game)
  {
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.LoadCharacters);
    this.LoadCharacters();
  }

  private void LoadCharacters()
  {
    foreach (NpcState npcState in this.NpcStates)
      ServiceHelper.RemoveComponent<NpcState>(npcState);
    this.NpcStates.Clear();
    foreach (NpcInstance npc in (IEnumerable<NpcInstance>) this.LevelManager.NonPlayerCharacters.Values)
    {
      NpcState npcState = this.CreateNpcState(npc);
      if (npcState != null)
      {
        ServiceHelper.AddComponent((IGameComponent) npcState);
        npcState.Initialize();
        this.NpcStates.Add(npcState);
      }
    }
  }

  protected virtual NpcState CreateNpcState(NpcInstance npc) => new NpcState(this.Game, npc);

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }
}
