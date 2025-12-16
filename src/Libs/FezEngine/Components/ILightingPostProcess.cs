// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.ILightingPostProcess
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Components;

public interface ILightingPostProcess
{
  event Action<GameTime> DrawGeometryLights;

  event Action DrawOnTopLights;

  Texture2D LightmapTexture { get; }
}
