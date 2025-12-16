// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.ScreenFade
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Components;

public class ScreenFade : DrawableGameComponent
{
  public Action Faded;
  public Action ScreenCaptured;
  private Texture capturedScreen;
  private RenderTargetHandle RtHandle;
  private TimeSpan Elapsed;

  public Color FromColor { get; set; }

  public Color ToColor { get; set; }

  public float Duration { get; set; }

  public bool EaseOut { get; set; }

  public EasingType EasingType { get; set; }

  public bool IsDisposed { get; set; }

  public Func<bool> WaitUntil { private get; set; }

  public bool CaptureScreen { get; set; }

  public ScreenFade(Game game)
    : base(game)
  {
    this.DrawOrder = 1000;
    this.EasingType = EasingType.Cubic;
  }

  public override void Initialize()
  {
    base.Initialize();
    if (!this.CaptureScreen)
      return;
    this.RtHandle = this.TargetRenderer.TakeTarget();
    this.TargetRenderer.ScheduleHook(this.DrawOrder, this.RtHandle.Target);
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.RtHandle != null)
    {
      this.TargetRenderer.ReturnTarget(this.RtHandle);
      this.RtHandle = (RenderTargetHandle) null;
    }
    this.Faded = (Action) null;
    this.ScreenCaptured = (Action) null;
    this.WaitUntil = (Func<bool>) null;
    this.capturedScreen = (Texture) null;
    this.IsDisposed = true;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.RtHandle != null && this.TargetRenderer.IsHooked(this.RtHandle.Target))
    {
      this.TargetRenderer.Resolve(this.RtHandle.Target, false);
      this.capturedScreen = (Texture) this.RtHandle.Target;
      if (this.ScreenCaptured != null)
        this.ScreenCaptured();
    }
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    if (this.capturedScreen != null)
    {
      this.GraphicsDevice.SetupViewport();
      this.TargetRenderer.DrawFullscreen(this.capturedScreen);
    }
    this.Elapsed += gameTime.ElapsedGameTime;
    float linearStep = (float) this.Elapsed.TotalSeconds / this.Duration;
    float amount = FezMath.Saturate(this.EaseOut ? Easing.EaseOut((double) linearStep, this.EasingType) : Easing.EaseIn((double) linearStep, this.EasingType));
    if ((double) amount == 1.0 && (this.WaitUntil == null || this.WaitUntil()))
    {
      if (this.Faded != null)
      {
        this.Faded();
        this.Faded = (Action) null;
      }
      this.WaitUntil = (Func<bool>) null;
      ServiceHelper.RemoveComponent<ScreenFade>(this);
    }
    this.TargetRenderer.DrawFullscreen(Color.Lerp(this.FromColor, this.ToColor, amount));
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }
}
