// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.FogManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public class FogManager(Game game) : GameComponent(game), IFogManager
{
  private FogType type;
  private Color color;
  private float density;
  private float start;
  private float end;

  public event Action FogSettingsChanged = new Action(Util.NullAction);

  public override void Initialize()
  {
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      this.type = FogType.ExponentialSquared;
      if (this.LevelManager.Sky == null)
        return;
      this.density = this.LevelManager.Sky.FogDensity;
      this.FogSettingsChanged();
    });
  }

  public FogType Type
  {
    get => this.type;
    set
    {
      this.type = value;
      this.FogSettingsChanged();
    }
  }

  public Color Color
  {
    get => this.color;
    set
    {
      if (!(this.color != value))
        return;
      this.color = value;
      this.FogSettingsChanged();
    }
  }

  public float Density
  {
    get => this.density;
    set
    {
      this.density = value;
      this.FogSettingsChanged();
    }
  }

  public float Start
  {
    get => this.start;
    set
    {
      this.start = value;
      this.FogSettingsChanged();
    }
  }

  public float End
  {
    get => this.end;
    set
    {
      this.end = value;
      this.FogSettingsChanged();
    }
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }
}
