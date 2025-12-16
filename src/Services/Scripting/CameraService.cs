// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.CameraService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

public class CameraService : ICameraService, IScriptingBase
{
  public void ResetEvents() => this.Rotated = new Action(Util.NullAction);

  public event Action Rotated = new Action(Util.NullAction);

  public void OnRotate() => this.Rotated();

  public void SetPixelsPerTrixel(int pixelsPerTrixel)
  {
    if (this.EngineState.FarawaySettings.InTransition)
      return;
    this.CameraManager.PixelsPerTrixel = (float) pixelsPerTrixel;
  }

  public void SetCanRotate(bool canRotate) => this.PlayerManager.CanRotate = canRotate;

  public void Rotate(int distance)
  {
    this.CameraManager.ChangeViewpoint(this.CameraManager.Viewpoint.GetRotatedView(distance));
  }

  public void RotateTo(string viewName)
  {
    Viewpoint view = (Viewpoint) Enum.Parse(typeof (Viewpoint), viewName, true);
    if (view == this.CameraManager.Viewpoint)
      return;
    this.CameraManager.ChangeViewpoint(view);
  }

  public LongRunningAction FadeTo(string colorName)
  {
    Color color = Util.FromName(colorName);
    ScreenFade component = new ScreenFade(ServiceHelper.Game)
    {
      FromColor = new Color(new Vector4(color.ToVector3(), 0.0f)),
      ToColor = color,
      Duration = 2f
    };
    ServiceHelper.AddComponent((IGameComponent) component);
    return new LongRunningAction((Func<float, float, bool>) ((elapsed, since) => component.IsDisposed));
  }

  public LongRunningAction FadeFrom(string colorName)
  {
    Color color = Util.FromName(colorName);
    ScreenFade component = new ScreenFade(ServiceHelper.Game)
    {
      FromColor = color,
      ToColor = new Color(new Vector4(color.ToVector3(), 0.0f)),
      Duration = 2f
    };
    ServiceHelper.AddComponent((IGameComponent) component);
    return new LongRunningAction((Func<float, float, bool>) ((elapsed, since) => component.IsDisposed));
  }

  public void Flash(string colorName)
  {
    Color color = Util.FromName(colorName);
    ServiceHelper.AddComponent((IGameComponent) new ScreenFade(ServiceHelper.Game)
    {
      FromColor = color,
      ToColor = new Color(new Vector4(color.ToVector3(), 0.0f)),
      Duration = 0.1f
    });
  }

  public void Shake(float distance, float durationSeconds)
  {
    ServiceHelper.AddComponent((IGameComponent) new CamShake(ServiceHelper.Game)
    {
      Duration = TimeSpan.FromSeconds((double) durationSeconds),
      Distance = distance
    });
  }

  public void SetDescending(bool descending) => this.LevelManager.Descending = descending;

  public void Unconstrain() => this.CameraManager.Constrained = false;

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }
}
