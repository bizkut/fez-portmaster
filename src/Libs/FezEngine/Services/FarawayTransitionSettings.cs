// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.FarawayTransitionSettings
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Services;

public class FarawayTransitionSettings
{
  public bool InTransition;
  public bool LoadingAllowed;
  public float TransitionStep;
  public float OriginFadeOutStep;
  public float DestinationCrossfadeStep;
  public float InterpolatedFakeRadius;
  public float DestinationRadius;
  public float DestinationPixelsPerTrixel;
  public Vector2 DestinationOffset;
  public RenderTarget2D SkyRt;

  public void Reset()
  {
    this.OriginFadeOutStep = 0.0f;
    this.DestinationCrossfadeStep = 0.0f;
    this.TransitionStep = 0.0f;
    this.LoadingAllowed = this.InTransition = false;
    this.InterpolatedFakeRadius = 0.0f;
    this.DestinationRadius = 0.0f;
    this.DestinationPixelsPerTrixel = 0.0f;
    this.DestinationOffset = Vector2.Zero;
    this.SkyRt = (RenderTarget2D) null;
  }
}
