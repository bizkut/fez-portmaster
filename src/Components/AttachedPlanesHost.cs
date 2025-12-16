// Decompiled with JetBrains decompiler
// Type: FezGame.Components.AttachedPlanesHost
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

public class AttachedPlanesHost(Game game) : GameComponent(game)
{
  private readonly List<AttachedPlanesHost.AttachedPlaneState> TrackedPlanes = new List<AttachedPlanesHost.AttachedPlaneState>();

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.TrackedPlanes.Clear();
    foreach (BackgroundPlane backgroundPlane in (IEnumerable<BackgroundPlane>) this.LevelManager.BackgroundPlanes.Values)
    {
      if (backgroundPlane.AttachedGroup.HasValue)
      {
        TrileInstance trileInstance = this.LevelManager.Groups[backgroundPlane.AttachedGroup.Value].Triles.FirstOrDefault<TrileInstance>();
        if (trileInstance != null)
        {
          Vector3 vector3 = backgroundPlane.Position - trileInstance.Position;
          this.TrackedPlanes.Add(new AttachedPlanesHost.AttachedPlaneState()
          {
            FirstTrile = trileInstance,
            Offset = vector3,
            Plane = backgroundPlane
          });
        }
      }
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Paused || this.EngineState.InMap || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning || this.EngineState.Loading)
      return;
    foreach (AttachedPlanesHost.AttachedPlaneState trackedPlane in this.TrackedPlanes)
      trackedPlane.Plane.Position = trackedPlane.FirstTrile.Position + trackedPlane.Offset;
  }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { get; set; }

  private class AttachedPlaneState
  {
    public BackgroundPlane Plane;
    public TrileInstance FirstTrile;
    public Vector3 Offset;
  }
}
