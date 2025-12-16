// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.ScreenshotTaker
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.IO;

#nullable disable
namespace FezEngine.Components;

public class ScreenshotTaker : DrawableGameComponent
{
  private int counter;
  private bool screenshotScheduled;
  private RenderTargetHandle rt;

  public ScreenshotTaker(Game game)
    : base(game)
  {
    this.DrawOrder = (int) short.MaxValue;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.KeyboardProvider.RegisterKey(Keys.F2);
  }

  public override void Update(GameTime gameTime)
  {
    this.screenshotScheduled |= this.KeyboardProvider.GetKeyState(Keys.F2) == FezButtonState.Pressed;
    if (!this.screenshotScheduled)
      return;
    this.rt = this.TRM.TakeTarget();
    this.TRM.ScheduleHook(this.DrawOrder, this.rt.Target);
  }

  public override void Draw(GameTime gameTime)
  {
    if (!this.screenshotScheduled || this.rt == null || !this.TRM.IsHooked(this.rt.Target))
      return;
    this.TRM.Resolve(this.rt.Target, false);
    using (FileStream fileStream = new FileStream($"C:\\Screenshot_{this.counter++:000}.png", FileMode.Create))
      this.rt.Target.SaveAsPng((Stream) fileStream, this.GraphicsDevice.Viewport.Width, this.GraphicsDevice.Viewport.Height);
    this.TRM.ReturnTarget(this.rt);
    this.rt = (RenderTargetHandle) null;
    this.screenshotScheduled = false;
  }

  [ServiceDependency]
  public IKeyboardStateManager KeyboardProvider { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TRM { private get; set; }
}
