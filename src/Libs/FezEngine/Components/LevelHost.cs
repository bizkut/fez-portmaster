// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.LevelHost
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

public abstract class LevelHost : DrawableGameComponent
{
  protected LevelHost(Game game)
    : base(game)
  {
    this.DrawOrder = 5;
  }

  protected override void LoadContent()
  {
    DrawActionScheduler.Schedule(new Action(this.RefreshEffects));
    BaseEffect.InstancingModeChanged += (Action) (() => DrawActionScheduler.Schedule(new Action(this.RefreshEffects)));
    this.CameraManager.PixelsPerTrixel = 3f;
  }

  private void RefreshEffects()
  {
    this.LevelMaterializer.TrilesMesh.Effect = (BaseEffect) new TrileEffect();
    this.LevelMaterializer.ArtObjectsMesh.Effect = (BaseEffect) new InstancedArtObjectEffect();
    this.LevelMaterializer.StaticPlanesMesh.Effect = (BaseEffect) new InstancedStaticPlaneEffect();
    this.LevelMaterializer.AnimatedPlanesMesh.Effect = (BaseEffect) new InstancedAnimatedPlaneEffect();
    this.LevelMaterializer.NpcMesh.Effect = (BaseEffect) new AnimatedPlaneEffect()
    {
      IgnoreShading = true
    };
  }

  public override void Draw(GameTime gameTime) => this.DoDraw();

  protected void DoDraw()
  {
    if (this.LevelManager.Sky != null && this.LevelManager.Sky.Name == "GRAVE")
    {
      this.LevelMaterializer.RenderPass = RenderPass.Ghosts;
      this.LevelMaterializer.NpcMesh.DepthWrites = false;
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Ghosts));
      this.LevelMaterializer.NpcMesh.Draw();
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
      this.LevelMaterializer.NpcMesh.DepthWrites = true;
    }
    this.LevelMaterializer.RenderPass = RenderPass.Normal;
    this.GraphicsDevice.GetDssCombiner().DepthBufferEnable = true;
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    this.LevelMaterializer.TrilesMesh.Draw();
    this.LevelMaterializer.ArtObjectsMesh.Draw();
    this.GraphicsDevice.GetRasterCombiner().SlopeScaleDepthBias = -0.1f;
    this.GraphicsDevice.GetRasterCombiner().DepthBias = this.CameraManager.Viewpoint.IsOrthographic() ? -1E-07f : (float) (-9.9999997473787516E-05 / ((double) this.CameraManager.FarPlane - (double) this.CameraManager.NearPlane));
    this.LevelMaterializer.StaticPlanesMesh.Draw();
    this.LevelMaterializer.AnimatedPlanesMesh.Draw();
    this.GraphicsDevice.GetRasterCombiner().DepthBias = 0.0f;
    this.GraphicsDevice.GetRasterCombiner().SlopeScaleDepthBias = 0.0f;
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.NoSilhouette));
    this.LevelMaterializer.NpcMesh.Draw();
    this.LevelMaterializer.RenderPass = RenderPass.Normal;
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { protected get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { protected get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { protected get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardProvider { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }
}
