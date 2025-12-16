// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.GammaCorrection
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Components;

public class GammaCorrection : DrawableGameComponent
{
  private RenderTargetHandle rtHandle;
  private GammaCorrectionEffect gammaCorrectionEffect;
  private bool isHooked;

  public GammaCorrection(Game game)
    : base(game)
  {
    this.DrawOrder = int.MaxValue;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.gammaCorrectionEffect = new GammaCorrectionEffect();
      this.isHooked = false;
    }));
  }

  public override void Update(GameTime gameTime)
  {
    base.Update(gameTime);
    if (this.EngineState.Loading)
      return;
    if (this.isHooked && (double) SettingsManager.Settings.Brightness == 0.5)
    {
      this.TargetRenderingManager.UnscheduleHook(this.rtHandle.Target);
      this.TargetRenderingManager.ReturnTarget(this.rtHandle);
      this.rtHandle = (RenderTargetHandle) null;
      this.isHooked = false;
    }
    else
    {
      if (this.isHooked || (double) SettingsManager.Settings.Brightness == 0.5)
        return;
      this.rtHandle = this.TargetRenderingManager.TakeTarget();
      this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.rtHandle.Target);
      this.isHooked = true;
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.rtHandle == null || !this.TargetRenderingManager.IsHooked(this.rtHandle.Target))
      return;
    this.TargetRenderingManager.Resolve(this.rtHandle.Target, true);
    this.gammaCorrectionEffect.MainBufferTexture = (Texture) this.rtHandle.Target;
    this.gammaCorrectionEffect.Brightness = SettingsManager.Settings.Brightness;
    this.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 0.0f, 0);
    this.GraphicsDevice.SetupViewport();
    this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.gammaCorrectionEffect);
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { protected get; set; }
}
