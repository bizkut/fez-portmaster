// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.LightingPostProcess
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Components;

public class LightingPostProcess : DrawableGameComponent, ILightingPostProcess
{
  protected RenderTargetHandle lightMapsRth;
  private LightingPostEffect lightingPostEffect;
  private bool hadRt;

  public event Action<GameTime> DrawGeometryLights = new Action<GameTime>(Util.NullAction<GameTime>);

  public event Action DrawOnTopLights = new Action(Util.NullAction);

  public LightingPostProcess(Game game)
    : base(game)
  {
    this.DrawOrder = 100;
    ServiceHelper.AddService((object) this);
  }

  protected override void LoadContent()
  {
    this.Enabled = false;
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.lightingPostEffect = new LightingPostEffect();
      this.Enabled = true;
    }));
    this.lightMapsRth = this.TargetRenderingManager.TakeTarget();
    this.TargetRenderingManager.PreDraw += new Action<GameTime>(this.PreDraw);
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.TargetRenderingManager.ReturnTarget(this.lightMapsRth);
    this.lightMapsRth = (RenderTargetHandle) null;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.LevelManager.SkipPostProcess)
      return;
    this.UpdateLightFilter();
  }

  private void UpdateLightFilter()
  {
    this.lightingPostEffect.DawnContribution = this.TimeManager.DawnContribution;
    this.lightingPostEffect.DuskContribution = this.TimeManager.DuskContribution;
    this.lightingPostEffect.NightContribution = this.TimeManager.NightContribution;
  }

  protected virtual void DoSetup()
  {
  }

  private void PreDraw(GameTime gameTime)
  {
    this.LevelMaterializer.RegisterSatellites();
    foreach (BackgroundPlane levelPlane in this.LevelMaterializer.LevelPlanes)
      levelPlane.Update();
    if (this.EngineState.StereoMode || this.LevelManager.Quantum)
      return;
    this.LevelManager.ActualDiffuse = new Color(this.LevelManager.BaseDiffuse, this.LevelManager.BaseDiffuse, this.LevelManager.BaseDiffuse);
    this.LevelManager.ActualAmbient = new Color(this.LevelManager.BaseAmbient, this.LevelManager.BaseAmbient, this.LevelManager.BaseAmbient);
    this.DoSetup();
    if (this.SkipLighting)
      return;
    this.hadRt = this.TargetRenderingManager.HasRtInQueue || this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava;
    this.GraphicsDevice.SetRenderTarget(this.lightMapsRth.Target);
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareDraw();
    this.GraphicsDevice.SetupViewport();
    if (!this.LevelManager.SkipPostProcess && (double) this.TimeManager.NightContribution != 0.0)
    {
      this.LevelManager.ActualDiffuse = Color.Lerp(this.LevelManager.ActualDiffuse, this.FogManager.Color, this.TimeManager.NightContribution * 0.4f);
      this.LevelManager.ActualAmbient = this.LevelManager.Sky == null || !this.LevelManager.Sky.FoliageShadows ? Color.Lerp(this.LevelManager.ActualAmbient, Color.White, this.TimeManager.NightContribution * 0.5f) : Color.Lerp(this.LevelManager.ActualAmbient, Color.Lerp(this.FogManager.Color, Color.White, 0.5f), this.TimeManager.NightContribution * 0.5f);
    }
    if (!this.LevelManager.SkipPostProcess)
      this.LevelManager.ActualAmbient = Color.Lerp(this.LevelManager.ActualAmbient, this.FogManager.Color, 0.14375f);
    if (this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.BlinkingAlpha)
      this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, new Color(0.0f, 0.0f, 0.0f, 0.0f), 1f, 0);
    else if (this.LevelManager.Sky != null && this.LevelManager.Sky.Name == "INDUS_CITY" && !SkyHost.Instance.flickering)
      this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, new Color(0, 0, 0, 0), 1f, 0);
    else
      this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, new Color(0.5f, 0.5f, 0.5f, 0.0f), 1f, 0);
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    this.LevelMaterializer.RenderPass = RenderPass.Occluders;
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
    this.DrawLightOccluders(gameTime);
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    this.LevelMaterializer.RenderPass = RenderPass.LightInAlphaEmitters;
    graphicsDevice.GetBlendCombiner().AlphaBlendFunction = BlendFunction.Add;
    graphicsDevice.GetBlendCombiner().AlphaSourceBlend = Blend.One;
    graphicsDevice.GetBlendCombiner().AlphaDestinationBlend = Blend.One;
    this.LevelMaterializer.TrilesMesh.Draw();
    this.LevelMaterializer.ArtObjectsMesh.Draw();
    graphicsDevice.GetRasterCombiner().SlopeScaleDepthBias = -0.1f;
    graphicsDevice.GetRasterCombiner().DepthBias = this.CameraManager.Viewpoint.IsOrthographic() ? -1E-07f : (float) (-9.9999997473787516E-05 / ((double) this.CameraManager.FarPlane - (double) this.CameraManager.NearPlane));
    graphicsDevice.GetBlendCombiner().BlendingMode = BlendingMode.Opaque;
    this.LevelMaterializer.AnimatedPlanesMesh.Draw();
    this.LevelMaterializer.StaticPlanesMesh.Draw();
    this.LevelMaterializer.NpcMesh.Draw();
    this.DrawGeometryLights(gameTime);
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    this.DrawOnTopLights();
    graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    graphicsDevice.ResetAlphaBlending();
    this.LevelMaterializer.RenderPass = RenderPass.WorldspaceLightmaps;
    this.LevelMaterializer.StaticPlanesMesh.Draw();
    this.LevelMaterializer.AnimatedPlanesMesh.Draw();
    this.LevelMaterializer.RenderPass = RenderPass.ScreenspaceLightmaps;
    this.LevelMaterializer.StaticPlanesMesh.Draw();
    this.LevelMaterializer.AnimatedPlanesMesh.Draw();
    graphicsDevice.GetRasterCombiner().DepthBias = 0.0f;
    graphicsDevice.GetRasterCombiner().SlopeScaleDepthBias = 0.0f;
    this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
  }

  protected virtual void DrawLightOccluders(GameTime gameTime)
  {
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.EngineState.StereoMode || this.LevelManager.Quantum || this.EngineState.Loading || this.LevelManager.WaterType == LiquidType.Sewer || this.EngineState.SkipRendering)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.GetDssCombiner().DepthBufferEnable = false;
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue);
    if (!this.LevelManager.SkipPostProcess)
    {
      graphicsDevice.PrepareStencilRead(CompareFunction.LessEqual, FezEngine.Structure.StencilMask.Level);
      this.DrawLightFilter();
    }
    graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    graphicsDevice.SetBlendingMode(BlendingMode.Multiply2X);
    this.TargetRenderingManager.DrawFullscreen((Texture) this.lightMapsRth.Target);
    graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    graphicsDevice.GetDssCombiner().DepthBufferEnable = true;
  }

  private void DrawLightFilter()
  {
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    if (!FezMath.AlmostEqual(this.lightingPostEffect.DawnContribution, 0.0f))
    {
      this.lightingPostEffect.Pass = LightingPostEffect.Passes.Dawn;
      graphicsDevice.SetBlendingMode(BlendingMode.Screen);
      this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.lightingPostEffect);
    }
    if (!FezMath.AlmostEqual(this.lightingPostEffect.DuskContribution, 0.0f))
    {
      this.lightingPostEffect.Pass = LightingPostEffect.Passes.Dusk_Multiply;
      graphicsDevice.SetBlendingMode(BlendingMode.Multiply);
      this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.lightingPostEffect);
      this.lightingPostEffect.Pass = LightingPostEffect.Passes.Dusk_Screen;
      graphicsDevice.SetBlendingMode(BlendingMode.Screen);
      this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.lightingPostEffect);
    }
    if (FezMath.AlmostEqual(this.lightingPostEffect.NightContribution, 0.0f))
      return;
    this.lightingPostEffect.Pass = LightingPostEffect.Passes.Night;
    graphicsDevice.SetBlendingMode(BlendingMode.Multiply);
    this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.lightingPostEffect);
  }

  protected virtual bool SkipLighting => false;

  public Texture2D LightmapTexture => (Texture2D) this.lightMapsRth.Target;

  [ServiceDependency]
  public IEngineStateManager EngineState { protected get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { protected get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { protected get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { protected get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { protected get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { protected get; set; }

  [ServiceDependency]
  public IFogManager FogManager { private get; set; }
}
