// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SpinningTreasuresHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

public class SpinningTreasuresHost(Game game) : GameComponent(game)
{
  private readonly List<TrileInstance> TrackedTreasures = new List<TrileInstance>();
  private TimeSpan SinceCreated;

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.LevelManager.TrileRestored += (Action<TrileInstance>) (t =>
    {
      if (!t.Enabled || !t.Trile.ActorSettings.Type.IsTreasure() && t.Trile.ActorSettings.Type != ActorType.GoldenCube)
        return;
      this.TrackedTreasures.Add(t);
    });
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.TrackedTreasures.Clear();
    foreach (TrileInstance trileInstance in (IEnumerable<TrileInstance>) this.LevelManager.Triles.Values)
    {
      if (trileInstance.Enabled && (trileInstance.Trile.ActorSettings.Type.IsTreasure() || trileInstance.Trile.ActorSettings.Type == ActorType.GoldenCube))
        this.TrackedTreasures.Add(trileInstance);
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMenuCube || this.GameState.InMap || this.GameState.InFpsMode || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning || this.GameState.Loading)
      return;
    this.SinceCreated += gameTime.ElapsedGameTime;
    for (int index = 0; index < this.TrackedTreasures.Count; ++index)
    {
      TrileInstance trackedTreasure = this.TrackedTreasures[index];
      float num1 = (float) Math.Sin(this.SinceCreated.TotalSeconds * 3.1415927410125732 + (double) index / 0.14285700023174286) * 0.1f;
      float num2 = num1 - trackedTreasure.LastTreasureSin;
      trackedTreasure.LastTreasureSin = num1;
      if (trackedTreasure.Enabled && !trackedTreasure.Removed)
      {
        if (!trackedTreasure.Hidden)
        {
          if (trackedTreasure.Trile.ActorSettings.Type != ActorType.GoldenCube)
            trackedTreasure.Phi += (float) (gameTime.ElapsedGameTime.TotalSeconds * 2.0);
          trackedTreasure.Position += num2 * Vector3.UnitY;
          this.LevelManager.UpdateInstance(trackedTreasure);
          this.LevelMaterializer.GetTrileMaterializer(trackedTreasure.Trile).UpdateInstance(trackedTreasure);
        }
      }
      else
        this.TrackedTreasures.RemoveAt(index--);
    }
  }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }
}
