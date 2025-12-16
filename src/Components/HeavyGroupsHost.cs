// Decompiled with JetBrains decompiler
// Type: FezGame.Components.HeavyGroupsHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class HeavyGroupsHost : GameComponent
{
  private readonly List<HeavyGroupState> trackedGroups = new List<HeavyGroupState>();

  public HeavyGroupsHost(Game game)
    : base(game)
  {
    this.UpdateOrder = -2;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.Enabled = false;
    this.LevelManager.LevelChanging += new Action(this.TrackNewGroups);
    this.TrackNewGroups();
  }

  private void TrackNewGroups()
  {
    this.trackedGroups.Clear();
    foreach (TrileGroup group in this.LevelManager.Groups.Values.Where<TrileGroup>((Func<TrileGroup, bool>) (x => x.Heavy)))
      this.trackedGroups.Add(new HeavyGroupState(group));
    this.Enabled = this.trackedGroups.Count > 0;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Paused || this.EngineState.InMap || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning)
      return;
    foreach (HeavyGroupState trackedGroup in this.trackedGroups)
      trackedGroup.Update(gameTime.ElapsedGameTime);
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }
}
