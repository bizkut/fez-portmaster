// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.PlaneService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

internal class PlaneService : IPlaneService, IScriptingBase
{
  public void ResetEvents()
  {
  }

  public LongRunningAction FadeIn(int id, float seconds)
  {
    float wasOpacity = this.LevelManager.BackgroundPlanes[id].Opacity;
    return new LongRunningAction((Func<float, float, bool>) ((_, elapsedSeconds) =>
    {
      BackgroundPlane backgroundPlane;
      if (!this.LevelManager.BackgroundPlanes.TryGetValue(id, out backgroundPlane))
        return true;
      backgroundPlane.Opacity = MathHelper.Lerp(wasOpacity, 1f, FezMath.Saturate(elapsedSeconds / seconds));
      return (double) backgroundPlane.Opacity == 1.0;
    }));
  }

  public LongRunningAction FadeOut(int id, float seconds)
  {
    float wasOpacity = this.LevelManager.BackgroundPlanes[id].Opacity;
    return new LongRunningAction((Func<float, float, bool>) ((_, elapsedSeconds) =>
    {
      BackgroundPlane backgroundPlane;
      if (!this.LevelManager.BackgroundPlanes.TryGetValue(id, out backgroundPlane))
        return true;
      backgroundPlane.Opacity = MathHelper.Lerp(wasOpacity, 0.0f, FezMath.Saturate(elapsedSeconds / seconds));
      return (double) backgroundPlane.Opacity == 0.0;
    }));
  }

  public LongRunningAction Flicker(int id, float factor)
  {
    Vector3 baseScale = this.LevelManager.BackgroundPlanes[id].Scale;
    return new LongRunningAction((Func<float, float, bool>) ((elapsedSeconds, _) =>
    {
      if (RandomHelper.Probability(0.25))
      {
        BackgroundPlane backgroundPlane;
        if (!this.LevelManager.BackgroundPlanes.TryGetValue(id, out backgroundPlane))
          return true;
        backgroundPlane.Scale = baseScale + new Vector3(RandomHelper.Centered((double) factor));
      }
      return false;
    }));
  }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }
}
