// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IFogManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public interface IFogManager
{
  event Action FogSettingsChanged;

  FogType Type { get; set; }

  Color Color { get; set; }

  float Density { get; set; }

  float Start { get; set; }

  float End { get; set; }
}
