// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.ILightsManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public interface ILightsManager
{
  event Action<LightEventArgs> DirectionalLightAdded;

  event Action<LightEventArgs> DirectionalLightChanged;

  event Action<LightEventArgs> DirectionalLightRemoved;

  event Action<LightEventArgs> PointLightAdded;

  event Action<LightEventArgs> PointLightChanged;

  event Action<LightEventArgs> PointLightRemoved;

  event Action GlobalAmbientChanged;

  DirectionalLight GetDirectionalLight(int lightNumber);

  PointLight GetPointLight(int lightNumber);

  int DirectionalLightsCount { get; }

  int PointLightsCount { get; }

  IList<DirectionalLight> DirectionalLights { get; }

  IList<PointLight> PointLights { get; }

  Vector3 GlobalAmbient { get; set; }

  void OnDirectionalLightAdded(int newIndex);

  void OnDirectionalLightChanged(int index);

  void OnDirectionalLightRemoved(int oldIndex);

  void OnPointLightAdded(int newIndex);

  void OnPointLightChanged(int index);

  void OnPointLightRemoved(int oldIndex);
}
