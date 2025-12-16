// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.BurnInPostProcess
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

public class BurnInPostProcess : DrawableGameComponent
{
  private Texture2D oldFrameBuffer;
  private RenderTargetHandle newFrameBuffer;
  private RenderTargetHandle ownedHandle;
  private BurnInPostEffect burnInEffect;

  public BurnInPostProcess(Game game)
    : base(game)
  {
    this.DrawOrder = 902;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.Enabled = false;
    this.LevelManager.LevelChanged += new Action(this.TryCreateTargets);
    this.TryCreateTargets();
  }

  private void TryCreateTargets()
  {
    if (this.LevelManager.Name == null)
      return;
    if (this.LevelManager.BlinkingAlpha)
    {
      if (!this.Enabled)
      {
        this.ownedHandle = this.TargetRenderingManager.TakeTarget();
        this.newFrameBuffer = this.TargetRenderingManager.TakeTarget();
      }
      this.Enabled = true;
    }
    else
    {
      if (this.Enabled)
      {
        this.TargetRenderingManager.ReturnTarget(this.ownedHandle);
        this.ownedHandle = (RenderTargetHandle) null;
        this.TargetRenderingManager.ReturnTarget(this.newFrameBuffer);
        this.newFrameBuffer = (RenderTargetHandle) null;
      }
      this.Enabled = false;
    }
  }

  protected override void LoadContent()
  {
    DrawActionScheduler.Schedule((Action) (() => this.burnInEffect = new BurnInPostEffect()));
  }

  public override void Draw(GameTime gameTime)
  {
    if (!this.Enabled || this.EngineState.Loading || this.EngineState.Paused || this.EngineState.InMap || this.EngineState.InEditor)
    {
      if (this.newFrameBuffer == null || !this.TargetRenderingManager.IsHooked(this.newFrameBuffer.Target))
        return;
      this.TargetRenderingManager.Resolve(this.newFrameBuffer.Target, false);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.TargetRenderingManager.DrawFullscreen((Texture) this.newFrameBuffer.Target);
    }
    else if (!this.TargetRenderingManager.IsHooked(this.newFrameBuffer.Target))
    {
      this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.newFrameBuffer.Target);
    }
    else
    {
      this.GraphicsDevice.GetDssCombiner().StencilEnable = false;
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Opaque);
      this.TargetRenderingManager.Resolve(this.newFrameBuffer.Target, true);
      RenderTarget2D renderTarget = this.GraphicsDevice.GetRenderTargets().Length == 0 ? (RenderTarget2D) null : this.GraphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D;
      this.burnInEffect.NewFrameBuffer = (Texture) this.newFrameBuffer.Target;
      this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      RenderTargetHandle target = this.TargetRenderingManager.TakeTarget();
      this.GraphicsDevice.SetRenderTarget(target.Target);
      this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.burnInEffect);
      this.TargetRenderingManager.ReturnTarget(target);
      this.GraphicsDevice.SetRenderTarget(this.ownedHandle.Target);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.TargetRenderingManager.DrawFullscreen((Texture) target.Target);
      this.GraphicsDevice.SetRenderTarget(renderTarget);
      this.oldFrameBuffer = (Texture2D) this.ownedHandle.Target;
      this.burnInEffect.OldFrameBuffer = (Texture) this.oldFrameBuffer;
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.TargetRenderingManager.DrawFullscreen((Texture) this.newFrameBuffer.Target);
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Maximum);
      this.TargetRenderingManager.DrawFullscreen((Texture) this.oldFrameBuffer);
      this.GraphicsDevice.GetDssCombiner().StencilEnable = true;
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    }
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }
}
