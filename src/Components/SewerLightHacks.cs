// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SewerLightHacks
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

internal class SewerLightHacks : DrawableGameComponent
{
  private SewerHaxEffect Effect;

  public SewerLightHacks(Game game)
    : base(game)
  {
    this.DrawOrder = 49;
  }

  public override void Initialize()
  {
    base.Initialize();
    DrawActionScheduler.Schedule((Action) (() => this.Effect = new SewerHaxEffect()));
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
  }

  private void TryInitialize()
  {
    this.Visible = this.LevelManager.WaterType == LiquidType.Sewer;
    if (!this.Visible)
      return;
    this.LevelManager.BaseAmbient = 1f;
    this.LevelManager.BaseDiffuse = 0.0f;
    this.LevelManager.HaloFiltering = false;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.StereoMode)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
    this.LevelMaterializer.StaticPlanesMesh.AlwaysOnTop = true;
    this.LevelMaterializer.StaticPlanesMesh.DepthWrites = false;
    foreach (BackgroundPlane levelPlane in this.LevelMaterializer.LevelPlanes)
      levelPlane.Group.Enabled = levelPlane.Id < 0;
    this.LevelMaterializer.StaticPlanesMesh.Draw();
    this.LevelMaterializer.StaticPlanesMesh.AlwaysOnTop = false;
    this.LevelMaterializer.StaticPlanesMesh.DepthWrites = true;
    foreach (BackgroundPlane levelPlane in this.LevelMaterializer.LevelPlanes)
      levelPlane.Group.Enabled = true;
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    graphicsDevice.PrepareStencilRead(CompareFunction.LessEqual, FezEngine.Structure.StencilMask.Sky);
    this.TargetRenderer.DrawFullscreen((BaseEffect) this.Effect, (Texture) this.LightingPostProcess.LightmapTexture);
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  [ServiceDependency]
  public IGameStateManager GameState { get; private set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; private set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; private set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; private set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { get; private set; }
}
