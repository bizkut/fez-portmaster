// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IEngineStateManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public interface IEngineStateManager
{
  event Action PauseStateChanged;

  bool Paused { get; }

  bool InMap { get; }

  bool InMenuCube { get; }

  float FramesPerSecond { get; set; }

  bool LoopRender { get; set; }

  bool SkyRender { get; set; }

  bool Loading { get; set; }

  bool InEditor { get; set; }

  bool TimePaused { get; }

  float SkyOpacity { get; set; }

  bool SkipRendering { get; set; }

  float WaterLevelOffset { get; }

  bool StereoMode { get; set; }

  bool DotLoading { get; set; }

  Vector3 WaterBodyColor { get; set; }

  Vector3 WaterFoamColor { get; set; }

  bool InFpsMode { get; set; }

  FarawayTransitionSettings FarawaySettings { get; }
}
