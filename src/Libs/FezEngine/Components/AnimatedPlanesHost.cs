// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.AnimatedPlanesHost
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

public class AnimatedPlanesHost(Game game) : GameComponent(game)
{
  public override void Initialize()
  {
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      foreach (BackgroundPlane backgroundPlane in (IEnumerable<BackgroundPlane>) this.LevelManager.BackgroundPlanes.Values)
        backgroundPlane.OriginalPosition = new Vector3?(backgroundPlane.Position);
    });
  }

  public override void Update(GameTime gameTime)
  {
    if (this.LevelMaterializer.LevelPlanes.Count == 0 || this.EngineState.Paused || this.EngineState.InMap || this.EngineState.Loading)
      return;
    bool flag = this.CameraManager.Viewpoint.IsOrthographic() && this.CameraManager.ActionRunning;
    bool inEditor = this.EngineState.InEditor;
    foreach (BackgroundPlane levelPlane in this.LevelMaterializer.LevelPlanes)
    {
      if (levelPlane.Visible || levelPlane.ActorType == ActorType.Bomb)
      {
        if (flag && levelPlane.Animated)
        {
          int frame = levelPlane.Timing.Frame;
          levelPlane.Timing.Update(gameTime.ElapsedGameTime);
          if (!levelPlane.Loop && frame > levelPlane.Timing.Frame)
            this.LevelManager.RemovePlane(levelPlane);
          else
            levelPlane.MarkDirty();
        }
        if (levelPlane.Billboard)
          levelPlane.Rotation = this.CameraManager.Rotation * levelPlane.OriginalRotation;
        Vector3? originalPosition;
        if (((inEditor ? 0 : ((double) levelPlane.ParallaxFactor != 0.0 ? 1 : 0)) & (flag ? 1 : 0)) != 0)
        {
          Viewpoint view = levelPlane.Orientation.AsViewpoint();
          originalPosition = levelPlane.OriginalPosition;
          if (!originalPosition.HasValue)
            levelPlane.OriginalPosition = new Vector3?(levelPlane.Position);
          float num = (float) ((double) (-4 * (this.LevelManager.Descending ? -1 : 1)) / (double) this.CameraManager.PixelsPerTrixel - 15.0 / 32.0 + 1.0);
          Vector3 interpolatedCenter = this.CameraManager.InterpolatedCenter;
          originalPosition = levelPlane.OriginalPosition;
          Vector3 vector3_1 = originalPosition.Value;
          Vector3 vector3_2 = interpolatedCenter - vector3_1 + num * Vector3.UnitY;
          BackgroundPlane backgroundPlane = levelPlane;
          originalPosition = levelPlane.OriginalPosition;
          Vector3 vector3_3 = originalPosition.Value + vector3_2 * view.ScreenSpaceMask() * levelPlane.ParallaxFactor;
          backgroundPlane.Position = vector3_3;
        }
        else if (!inEditor && (double) levelPlane.ParallaxFactor != 0.0)
        {
          originalPosition = levelPlane.OriginalPosition;
          if (originalPosition.HasValue)
          {
            Vector3 position = levelPlane.Position;
            originalPosition = levelPlane.OriginalPosition;
            Vector3 vector3_4 = originalPosition.Value;
            if (position != vector3_4)
            {
              BackgroundPlane backgroundPlane = levelPlane;
              originalPosition = levelPlane.OriginalPosition;
              Vector3 vector3_5 = originalPosition.Value;
              backgroundPlane.Position = vector3_5;
            }
          }
        }
      }
    }
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }
}
