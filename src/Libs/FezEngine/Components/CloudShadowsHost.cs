// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.CloudShadowsHost
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
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Components;

public class CloudShadowsHost : DrawableGameComponent
{
  private readonly Dictionary<Group, Axis> axisPerGroup = new Dictionary<Group, Axis>();
  private CloudShadowEffect shadowEffect;
  private Mesh shadowMesh;
  private SkyHost Host;
  private float SineAccumulator;
  private float SineSpeed;

  public CloudShadowsHost(Game game, SkyHost host)
    : base(game)
  {
    this.DrawOrder = 100;
    this.Host = host;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.shadowMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true,
      SamplerState = SamplerState.LinearWrap
    };
    foreach (FaceOrientation faceOrientation in Util.GetValues<FaceOrientation>())
    {
      if (faceOrientation.IsSide())
        this.axisPerGroup.Add(this.shadowMesh.AddFace(Vector3.One, Vector3.Zero, faceOrientation, true), faceOrientation.AsAxis() == Axis.X ? Axis.Z : Axis.X);
    }
    DrawActionScheduler.Schedule((Action) (() => this.shadowMesh.Effect = (BaseEffect) (this.shadowEffect = new CloudShadowEffect())));
    this.LevelManager.SkyChanged += new Action(this.InitializeShadows);
    this.InitializeShadows();
    this.LightingPostProcess.DrawOnTopLights += new Action(this.DrawLights);
  }

  private void InitializeShadows()
  {
    if (this.LevelManager.Name == null || this.LevelManager.Sky == null || this.LevelManager.Sky.Shadows == null)
    {
      this.shadowMesh.Texture.Set((Texture) null);
      this.shadowMesh.Enabled = false;
    }
    else
    {
      this.shadowMesh.Enabled = true;
      DrawActionScheduler.Schedule((Action) (() => this.shadowMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>($"Skies/{this.LevelManager.Sky.Name}/" + this.LevelManager.Sky.Shadows)));
      this.shadowMesh.Scale = this.LevelManager.Size + new Vector3(65f, 65f, 65f);
      this.shadowMesh.Position = this.LevelManager.Size / 2f;
      int num1 = 0;
      foreach (Group key in this.axisPerGroup.Keys)
      {
        key.Material = new Material();
        Axis axis = this.axisPerGroup[key];
        float m11 = this.shadowMesh.Scale.Dot(axis.GetMask()) / 32f;
        float num2 = this.shadowMesh.Scale.Y / this.shadowMesh.Scale.Dot(axis.GetMask());
        key.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(m11, 0.0f, 0.0f, 0.0f, 0.0f, m11 * num2, 0.0f, 0.0f, (float) num1 / 2f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f));
        ++num1;
      }
      this.SineAccumulator = 0.0f;
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.shadowMesh.Enabled || this.EngineState.Paused || this.EngineState.InMap || this.EngineState.Loading)
      return;
    if (this.LevelManager.Sky != null && !this.LevelManager.Sky.FoliageShadows)
    {
      float m31 = (float) (-gameTime.ElapsedGameTime.TotalSeconds * 0.0099999997764825821 * (double) this.TimeManager.TimeFactor / 360.0) * this.LevelManager.Sky.WindSpeed;
      foreach (Group key in this.axisPerGroup.Keys)
      {
        if (this.axisPerGroup[key] != this.CameraManager.Viewpoint.VisibleAxis())
          key.TextureMatrix.Set(new Matrix?(key.TextureMatrix.Value.Value + new Matrix(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, m31, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)));
      }
    }
    else
    {
      this.SineSpeed = MathHelper.Lerp(this.SineSpeed, RandomHelper.Between(0.0, gameTime.ElapsedGameTime.TotalSeconds), 0.1f);
      this.SineAccumulator += this.SineSpeed;
    }
    foreach (Group key in this.axisPerGroup.Keys)
    {
      Vector3 mask = this.axisPerGroup[key].GetMask();
      key.Material.Opacity = (1f - Math.Abs(this.CameraManager.View.Forward.Dot(mask))) * this.LevelManager.Sky.ShadowOpacity;
      if (!this.LevelManager.Sky.FoliageShadows)
        key.Material.Opacity *= (float) this.LevelManager.ActualDiffuse.G / (float) byte.MaxValue;
      if (this.CameraManager.ProjectionTransition)
        key.Material.Opacity *= this.CameraManager.Viewpoint.IsOrthographic() ? this.CameraManager.ViewTransitionStep : 1f - this.CameraManager.ViewTransitionStep;
      else if (this.CameraManager.Viewpoint == Viewpoint.Perspective)
        key.Material.Opacity = 0.0f;
    }
  }

  private void DrawLights()
  {
    if (!this.shadowMesh.Enabled || this.EngineState.Loading)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilRead(CompareFunction.LessEqual, FezEngine.Structure.StencilMask.Level);
    this.EngineState.SkyRender = true;
    Vector3 viewOffset = this.CameraManager.ViewOffset;
    this.CameraManager.ViewOffset -= viewOffset;
    if (this.LevelManager.Sky != null && this.LevelManager.Sky.FoliageShadows)
    {
      this.shadowEffect.Pass = CloudShadowPasses.Canopy;
      graphicsDevice.SetBlendingMode(BlendingMode.Minimum);
      float num1 = (float) Math.Sin((double) this.SineAccumulator);
      foreach (Group key in this.axisPerGroup.Keys)
        key.TextureMatrix.Set(new Matrix?(new Matrix(key.TextureMatrix.Value.Value.M11, 0.0f, 0.0f, 0.0f, 0.0f, key.TextureMatrix.Value.Value.M11, 0.0f, 0.0f, num1 / 100f, num1 / 100f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
      this.shadowMesh.Draw();
      float num2 = (float) Math.Cos((double) this.SineAccumulator);
      foreach (Group key in this.axisPerGroup.Keys)
        key.TextureMatrix.Set(new Matrix?(new Matrix(key.TextureMatrix.Value.Value.M11, 0.0f, 0.0f, 0.0f, 0.0f, key.TextureMatrix.Value.Value.M11, 0.0f, 0.0f, (float) (-(double) num2 / 100.0 + 0.10000000149011612), (float) ((double) num2 / 100.0 + 0.10000000149011612), 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
      this.shadowMesh.Draw();
    }
    else
    {
      this.shadowEffect.Pass = CloudShadowPasses.Standard;
      float depthBias = graphicsDevice.GetRasterCombiner().DepthBias;
      float slopeScaleDepthBias = graphicsDevice.GetRasterCombiner().SlopeScaleDepthBias;
      graphicsDevice.GetRasterCombiner().DepthBias = 0.0f;
      graphicsDevice.GetRasterCombiner().SlopeScaleDepthBias = 0.0f;
      Color color = new Color(this.LevelManager.ActualAmbient.ToVector3() / 2f);
      graphicsDevice.SetBlendingMode(BlendingMode.Subtract);
      this.TargetRenderer.DrawFullscreen(color);
      graphicsDevice.SetBlendingMode(BlendingMode.Multiply);
      this.shadowMesh.Draw();
      graphicsDevice.SetBlendingMode(BlendingMode.Additive);
      this.TargetRenderer.DrawFullscreen(color);
      graphicsDevice.GetRasterCombiner().DepthBias = depthBias;
      graphicsDevice.GetRasterCombiner().SlopeScaleDepthBias = slopeScaleDepthBias;
    }
    graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    this.CameraManager.ViewOffset += viewOffset;
    this.EngineState.SkyRender = false;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.EngineState.Loading || this.LevelManager.Sky == null || this.Host.BgLayers == null)
      return;
    this.GraphicsDevice.SamplerStates[0] = this.LevelManager.Sky.VerticalTiling ? SamplerState.PointWrap : SamplerStates.PointUWrapVClamp;
    foreach (Group group in this.Host.BgLayers.Groups)
      group.Enabled = ((int) group.AlwaysOnTop ?? 0) != 0;
    this.Host.BgLayers.Draw();
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { private get; set; }
}
