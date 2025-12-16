// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.EngineStateManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public abstract class EngineStateManager : IEngineStateManager
{
  protected bool paused;
  protected bool inMap;
  protected bool inMenuCube;
  private bool loading;

  public event Action PauseStateChanged;

  public EngineStateManager()
  {
    this.FarawaySettings = new FarawayTransitionSettings();
    this.SkyOpacity = 1f;
  }

  protected void OnPauseStateChanged() => this.PauseStateChanged();

  public bool Paused => this.paused;

  public bool InMap => this.inMap;

  public bool InMenuCube => this.inMenuCube;

  public abstract bool TimePaused { get; }

  public float FramesPerSecond { get; set; }

  public bool LoopRender { get; set; }

  public bool SkyRender { get; set; }

  public virtual bool Loading
  {
    get => this.loading;
    set => this.loading = value;
  }

  public virtual float WaterLevelOffset => 0.0f;

  public Vector3 WaterBodyColor { get; set; }

  public Vector3 WaterFoamColor { get; set; }

  public bool InEditor { get; set; }

  public float SkyOpacity { get; set; }

  public bool SkipRendering { get; set; }

  public bool StereoMode { get; set; }

  public bool DotLoading { get; set; }

  public FarawayTransitionSettings FarawaySettings { get; private set; }

  public bool InFpsMode { get; set; }
}
