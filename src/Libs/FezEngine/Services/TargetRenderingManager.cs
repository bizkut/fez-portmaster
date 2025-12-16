// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.TargetRenderingManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Services;

public class TargetRenderingManager : GameComponent, ITargetRenderingManager
{
  private readonly Mesh fullscreenPlane;
  private BasicPostEffect basicPostEffect;
  private readonly List<RenderTargetHandle> fullscreenRTs = new List<RenderTargetHandle>();
  private GraphicsDevice graphicsDevice;
  private Texture2D fullWhite;
  private readonly List<TargetRenderingManager.RtHook> renderTargetsToHook = new List<TargetRenderingManager.RtHook>();
  private int currentlyHookedRtIndex = -1;

  private event Action nextFrameHooks;

  public event Action<GameTime> PreDraw = new Action<GameTime>(Util.NullAction<GameTime>);

  public TargetRenderingManager(Game game)
    : base(game)
  {
    this.fullscreenPlane = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.fullscreenPlane.AddFace(Vector3.One * 2f, Vector3.Zero, FaceOrientation.Front, true);
  }

  public void ScheduleHook(int drawOrder, RenderTarget2D rt)
  {
    if (this.renderTargetsToHook.Any<TargetRenderingManager.RtHook>((Func<TargetRenderingManager.RtHook, bool>) (x => x.Target == rt)))
    {
      if (!this.renderTargetsToHook.Any<TargetRenderingManager.RtHook>((Func<TargetRenderingManager.RtHook, bool>) (x => x.Target == rt && x.DrawOrder == drawOrder)))
        throw new InvalidOperationException("Tried to hook already-hooked RT, but with different draw order");
    }
    else if (this.currentlyHookedRtIndex != -1)
    {
      this.nextFrameHooks += (Action) (() =>
      {
        this.renderTargetsToHook.Add(new TargetRenderingManager.RtHook()
        {
          DrawOrder = drawOrder,
          Target = rt
        });
        this.renderTargetsToHook.Sort((Comparison<TargetRenderingManager.RtHook>) ((a, b) => a.DrawOrder.CompareTo(b.DrawOrder)));
      });
    }
    else
    {
      this.renderTargetsToHook.Add(new TargetRenderingManager.RtHook()
      {
        DrawOrder = drawOrder,
        Target = rt
      });
      this.renderTargetsToHook.Sort((Comparison<TargetRenderingManager.RtHook>) ((a, b) => a.DrawOrder.CompareTo(b.DrawOrder)));
    }
  }

  public void UnscheduleHook(RenderTarget2D rt)
  {
    this.renderTargetsToHook.RemoveAll((Predicate<TargetRenderingManager.RtHook>) (x => x.Target == rt));
  }

  public void Resolve(RenderTarget2D rt, bool reschedule)
  {
    if (this.currentlyHookedRtIndex == -1)
      throw new InvalidOperationException("No render target hooked right now!");
    if (this.renderTargetsToHook[this.currentlyHookedRtIndex].Target != rt)
      throw new InvalidOperationException("Not the right render target hooked, can't resolve!");
    if (!reschedule)
      this.UnscheduleHook(rt);
    else
      ++this.currentlyHookedRtIndex;
    if (this.currentlyHookedRtIndex == this.renderTargetsToHook.Count)
    {
      this.graphicsDevice.SetRenderTarget((RenderTarget2D) null);
      this.currentlyHookedRtIndex = -1;
    }
    else
      this.graphicsDevice.SetRenderTarget(this.renderTargetsToHook[this.currentlyHookedRtIndex].Target);
  }

  public bool IsHooked(RenderTarget2D rt)
  {
    return this.currentlyHookedRtIndex != -1 && this.renderTargetsToHook[this.currentlyHookedRtIndex].Target == rt;
  }

  public void OnPreDraw(GameTime gameTime) => this.PreDraw(gameTime);

  public bool HasRtInQueue => this.renderTargetsToHook.Count > 0;

  public void OnRtPrepare()
  {
    if (this.nextFrameHooks != null)
    {
      this.nextFrameHooks();
      this.nextFrameHooks = (Action) null;
    }
    if (this.renderTargetsToHook.Count == 0)
      return;
    this.graphicsDevice.SetRenderTarget(this.renderTargetsToHook[this.currentlyHookedRtIndex = 0].Target);
  }

  public override void Initialize()
  {
    this.graphicsDevice = this.GraphicsDeviceService.GraphicsDevice;
    this.graphicsDevice.DeviceReset += (EventHandler<EventArgs>) ((_, __) => this.RecreateTargets());
    this.basicPostEffect = new BasicPostEffect();
    this.fullWhite = this.CMProvider.Global.Load<Texture2D>("Other Textures/FullWhite");
  }

  private void RecreateTargets()
  {
    List<Action<RenderTarget2D>> actionList = new List<Action<RenderTarget2D>>();
    foreach (RenderTargetHandle fullscreenRt in this.fullscreenRTs)
    {
      actionList.Clear();
      foreach (TargetRenderingManager.RtHook rtHook in this.renderTargetsToHook)
      {
        if (fullscreenRt.Target == rtHook.Target)
        {
          TargetRenderingManager.RtHook _ = rtHook;
          actionList.Add((Action<RenderTarget2D>) (t => _.Target = t));
        }
      }
      fullscreenRt.Target.Dispose();
      fullscreenRt.Target = this.CreateFullscreenTarget();
      foreach (Action<RenderTarget2D> action in actionList)
        action(fullscreenRt.Target);
    }
  }

  public RenderTargetHandle TakeTarget()
  {
    RenderTargetHandle handle = (RenderTargetHandle) null;
    foreach (RenderTargetHandle fullscreenRt in this.fullscreenRTs)
    {
      if (!fullscreenRt.Locked)
      {
        handle = fullscreenRt;
        break;
      }
    }
    if (handle == null)
    {
      this.fullscreenRTs.Add(handle = new RenderTargetHandle());
      DrawActionScheduler.Schedule((Action) (() => handle.Target = this.CreateFullscreenTarget()));
    }
    handle.Locked = true;
    return handle;
  }

  private RenderTarget2D CreateFullscreenTarget()
  {
    this.Game.GraphicsDevice.SetupViewport();
    return new RenderTarget2D(this.graphicsDevice, this.graphicsDevice.Viewport.Width, this.graphicsDevice.Viewport.Height, false, this.graphicsDevice.PresentationParameters.BackBufferFormat, this.graphicsDevice.PresentationParameters.DepthStencilFormat, this.graphicsDevice.PresentationParameters.MultiSampleCount, RenderTargetUsage.PlatformContents);
  }

  public void ReturnTarget(RenderTargetHandle handle)
  {
    if (handle == null)
      return;
    if (this.IsHooked(handle.Target))
      this.Resolve(handle.Target, false);
    else
      this.UnscheduleHook(handle.Target);
    handle.Locked = false;
  }

  public void DrawFullscreen(Color color)
  {
    this.DrawFullscreen((BaseEffect) this.basicPostEffect, (Texture) this.fullWhite, new Matrix?(), color);
  }

  public void DrawFullscreen(Texture texture)
  {
    this.DrawFullscreen((BaseEffect) this.basicPostEffect, texture, new Matrix?(), Color.White);
  }

  public void DrawFullscreen(Texture texture, Color color)
  {
    this.DrawFullscreen((BaseEffect) this.basicPostEffect, texture, new Matrix?(), color);
  }

  public void DrawFullscreen(Texture texture, Matrix textureMatrix)
  {
    this.DrawFullscreen((BaseEffect) this.basicPostEffect, texture, new Matrix?(textureMatrix), Color.White);
  }

  public void DrawFullscreen(Texture texture, Matrix textureMatrix, Color color)
  {
    this.DrawFullscreen((BaseEffect) this.basicPostEffect, texture, new Matrix?(textureMatrix), color);
  }

  public void DrawFullscreen(BaseEffect effect)
  {
    this.DrawFullscreen(effect, (Texture) null, new Matrix?(), Color.White);
  }

  public void DrawFullscreen(BaseEffect effect, Color color)
  {
    this.DrawFullscreen(effect, (Texture) this.fullWhite, new Matrix?(), color);
  }

  public void DrawFullscreen(BaseEffect effect, Texture texture)
  {
    this.DrawFullscreen(effect, texture, new Matrix?(), Color.White);
  }

  public void DrawFullscreen(BaseEffect effect, Texture texture, Matrix? textureMatrix)
  {
    this.DrawFullscreen(effect, texture, textureMatrix, Color.White);
  }

  public void DrawFullscreen(
    BaseEffect effect,
    Texture texture,
    Matrix? textureMatrix,
    Color color)
  {
    bool ignoreCache = effect.IgnoreCache;
    effect.IgnoreCache = true;
    if (texture != null)
      this.fullscreenPlane.Texture.Set(texture);
    if (textureMatrix.HasValue)
      this.fullscreenPlane.TextureMatrix.Set(textureMatrix.Value);
    if (color != Color.White)
    {
      this.fullscreenPlane.Material.Diffuse = color.ToVector3();
      this.fullscreenPlane.Material.Opacity = (float) color.A / (float) byte.MaxValue;
    }
    this.fullscreenPlane.Effect = effect;
    this.fullscreenPlane.Draw();
    if (color != Color.White)
    {
      this.fullscreenPlane.Material.Diffuse = Vector3.One;
      this.fullscreenPlane.Material.Opacity = 1f;
    }
    if (texture != null)
      this.fullscreenPlane.Texture.Set((Texture) null);
    if (textureMatrix.HasValue)
      this.fullscreenPlane.TextureMatrix.Set(Matrix.Identity);
    effect.IgnoreCache = ignoreCache;
  }

  [ServiceDependency]
  public IGraphicsDeviceService GraphicsDeviceService { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { protected get; set; }

  private class RtHook
  {
    public int DrawOrder;
    public RenderTarget2D Target;
  }
}
