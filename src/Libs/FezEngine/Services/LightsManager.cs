// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.LightsManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public class LightsManager : ILightsManager
{
  private readonly List<PointLight> pointLights;
  private readonly List<DirectionalLight> directionalLights;
  private Vector3 globalAmbient;

  public event Action<LightEventArgs> DirectionalLightAdded = new Action<LightEventArgs>(Util.NullAction<LightEventArgs>);

  public event Action<LightEventArgs> DirectionalLightRemoved = new Action<LightEventArgs>(Util.NullAction<LightEventArgs>);

  public event Action<LightEventArgs> DirectionalLightChanged = new Action<LightEventArgs>(Util.NullAction<LightEventArgs>);

  public event Action<LightEventArgs> PointLightAdded = new Action<LightEventArgs>(Util.NullAction<LightEventArgs>);

  public event Action<LightEventArgs> PointLightChanged = new Action<LightEventArgs>(Util.NullAction<LightEventArgs>);

  public event Action<LightEventArgs> PointLightRemoved = new Action<LightEventArgs>(Util.NullAction<LightEventArgs>);

  public event Action GlobalAmbientChanged = new Action(Util.NullAction);

  public LightsManager()
  {
    this.directionalLights = new List<DirectionalLight>();
    this.pointLights = new List<PointLight>();
  }

  public IList<DirectionalLight> DirectionalLights
  {
    get => (IList<DirectionalLight>) this.directionalLights;
  }

  public IList<PointLight> PointLights => (IList<PointLight>) this.pointLights;

  public DirectionalLight GetDirectionalLight(int lightNumber)
  {
    return this.directionalLights[lightNumber];
  }

  public PointLight GetPointLight(int lightNumber) => this.pointLights[lightNumber];

  public int DirectionalLightsCount => this.directionalLights.Count;

  public int PointLightsCount => this.directionalLights.Count;

  public Vector3 GlobalAmbient
  {
    get => this.globalAmbient;
    set
    {
      this.globalAmbient = value;
      this.GlobalAmbientChanged();
    }
  }

  public void OnDirectionalLightAdded(int newIndex)
  {
    this.DirectionalLightAdded(new LightEventArgs(newIndex));
  }

  public void OnDirectionalLightChanged(int index)
  {
    this.DirectionalLightChanged(new LightEventArgs(index));
  }

  public void OnDirectionalLightRemoved(int oldIndex)
  {
    this.DirectionalLightRemoved(new LightEventArgs(oldIndex));
  }

  public void OnPointLightAdded(int newIndex) => this.PointLightAdded(new LightEventArgs(newIndex));

  public void OnPointLightChanged(int index) => this.PointLightChanged(new LightEventArgs(index));

  public void OnPointLightRemoved(int oldIndex)
  {
    this.PointLightRemoved(new LightEventArgs(oldIndex));
  }
}
