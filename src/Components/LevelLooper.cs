// Decompiled with JetBrains decompiler
// Type: FezGame.Components.LevelLooper
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

internal class LevelLooper : DrawableGameComponent
{
  public LevelLooper(Game game)
    : base(game)
  {
    this.UpdateOrder = -1;
    this.DrawOrder = 6;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += (Action) (() => this.CameraManager.ViewOffset = Vector3.Zero);
    this.LightingPostProcess.DrawGeometryLights += new Action<GameTime>(((DrawableGameComponent) this).Draw);
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.LevelManager.Loops || this.PlayerManager.Action == ActionType.FreeFalling || this.GameState.Loading || this.GameState.InMap || this.GameState.Paused)
      return;
    while ((double) this.PlayerManager.Position.Y < 0.0)
    {
      this.PlayerManager.Position += this.LevelManager.Size * Vector3.UnitY;
      IGameCameraManager cameraManager1 = this.CameraManager;
      cameraManager1.Center = cameraManager1.Center + this.LevelManager.Size * Vector3.UnitY;
      IGameCameraManager cameraManager2 = this.CameraManager;
      cameraManager2.ViewOffset = cameraManager2.ViewOffset + this.LevelManager.Size * Vector3.UnitY;
    }
    while ((double) this.PlayerManager.Position.Y > (double) this.LevelManager.Size.Y)
    {
      this.PlayerManager.Position -= this.LevelManager.Size * Vector3.UnitY;
      IGameCameraManager cameraManager3 = this.CameraManager;
      cameraManager3.Center = cameraManager3.Center - this.LevelManager.Size * Vector3.UnitY;
      IGameCameraManager cameraManager4 = this.CameraManager;
      cameraManager4.ViewOffset = cameraManager4.ViewOffset - this.LevelManager.Size * Vector3.UnitY;
      this.PlayerManager.IgnoreFreefall = true;
    }
  }

  public override void Draw(GameTime gameTime) => this.Draw();

  private void Draw()
  {
    if (!this.LevelManager.Loops || this.GameState.Loading)
      return;
    float num = this.LevelManager.Size.Y * ((double) this.PlayerManager.Position.Y < (double) this.LevelManager.Size.Y / 2.0 ? 1f : -1f);
    this.GameState.LoopRender = true;
    if (this.LoopVisible())
    {
      IGameCameraManager cameraManager1 = this.CameraManager;
      cameraManager1.ViewOffset = cameraManager1.ViewOffset + num * Vector3.UnitY;
      this.DrawLoop();
      IGameCameraManager cameraManager2 = this.CameraManager;
      cameraManager2.ViewOffset = cameraManager2.ViewOffset - num * Vector3.UnitY;
    }
    this.GameState.LoopRender = false;
  }

  private bool LoopVisible()
  {
    if (!this.CameraManager.Viewpoint.IsOrthographic())
      return true;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector().Abs();
    BoundingFrustum frustum = this.CameraManager.Frustum;
    BoundingBox boundingBox = new BoundingBox()
    {
      Min = {
        X = -frustum.Left.D * frustum.Left.DotNormal(vector3_1),
        Y = -frustum.Bottom.D * frustum.Bottom.Normal.Y
      },
      Max = {
        X = -frustum.Right.D * frustum.Right.DotNormal(vector3_1),
        Y = -frustum.Top.D * frustum.Top.Normal.Y
      }
    };
    Vector3 vector3_2 = FezMath.Min(boundingBox.Min, boundingBox.Max);
    Vector3 vector3_3 = FezMath.Max(boundingBox.Min, boundingBox.Max);
    Rectangle rectangle = new Rectangle()
    {
      X = (int) Math.Floor((double) vector3_2.X),
      Y = (int) Math.Floor((double) vector3_2.Y),
      Width = (int) Math.Ceiling((double) vector3_3.X - (double) vector3_2.X),
      Height = (int) Math.Ceiling((double) vector3_3.Y - (double) vector3_2.Y)
    };
    return rectangle.Y < 0 || (double) (rectangle.Y + rectangle.Height) >= (double) this.LevelManager.Size.Y;
  }

  private void DrawLoop()
  {
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    int num = this.LevelMaterializer.RenderPass == RenderPass.LightInAlphaEmitters ? 1 : 0;
    if (num == 0)
      this.LevelMaterializer.RenderPass = RenderPass.Normal;
    if (num == 0)
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    this.LevelMaterializer.TrilesMesh.Draw();
    this.LevelMaterializer.ArtObjectsMesh.Draw();
    if (num != 0)
      graphicsDevice.GetRasterCombiner().DepthBias = -0.0001f;
    this.LevelMaterializer.StaticPlanesMesh.Draw();
    this.LevelMaterializer.AnimatedPlanesMesh.Draw();
    if (num != 0)
      graphicsDevice.GetRasterCombiner().DepthBias = 0.0f;
    if (num == 0)
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.NoSilhouette));
    this.LevelMaterializer.NpcMesh.Draw();
    if (num != 0)
      return;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { private get; set; }
}
