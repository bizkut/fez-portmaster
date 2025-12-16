// Decompiled with JetBrains decompiler
// Type: FezGame.Components.StargateHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class StargateHost : DrawableGameComponent
{
  private readonly List<bool> AoVisibility = new List<bool>();
  private float SinceStarted;
  private float SpinSpeed;
  private ArtObjectInstance[] Rings;
  private readonly Texture2D[] OriginalTextures = new Texture2D[4];
  private Texture2D WhiteTex;
  private Mesh TrialRaysMesh;
  private Mesh TrialFlareMesh;
  private float TrialTimeAccumulator;
  private StargateHost.MaskRenderer maskRenderer;

  public StargateHost(Game game)
    : base(game)
  {
    this.DrawOrder = 200;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.WhiteTex = this.CMProvider.Global.Load<Texture2D>("Other Textures/FullWhite");
    this.Enabled = this.Visible = false;
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
  }

  private void TryInitialize()
  {
    this.Rings = (ArtObjectInstance[]) null;
    if (this.TrialRaysMesh != null)
      this.TrialRaysMesh.Dispose();
    if (this.TrialFlareMesh != null)
      this.TrialFlareMesh.Dispose();
    this.TrialRaysMesh = this.TrialFlareMesh = (Mesh) null;
    this.TrialTimeAccumulator = this.SinceStarted = 0.0f;
    this.SpinSpeed = 0.0f;
    if (this.maskRenderer != null)
    {
      ServiceHelper.RemoveComponent<StargateHost.MaskRenderer>(this.maskRenderer);
      this.maskRenderer = (StargateHost.MaskRenderer) null;
    }
    this.Enabled = this.Visible = this.LevelManager.Name == "STARGATE";
    if (!this.Enabled)
      return;
    this.TrialRaysMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.TrialFlareMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.TrialFlareMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Right, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.TrialRaysMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.TrialRaysMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/smooth_ray");
      this.TrialFlareMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.TrialFlareMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/flare_alpha");
    }));
    this.Rings = new ArtObjectInstance[4]
    {
      this.LevelManager.ArtObjects[5],
      this.LevelManager.ArtObjects[6],
      this.LevelManager.ArtObjects[7],
      this.LevelManager.ArtObjects[8]
    };
    foreach (ArtObjectInstance ring in this.Rings)
      ring.Material = new Material();
    ServiceHelper.AddComponent((IGameComponent) (this.maskRenderer = new StargateHost.MaskRenderer(this.Game)));
    this.maskRenderer.Center = this.Rings[0].Position;
    if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(0))
    {
      this.Enabled = false;
      this.LevelManager.Scripts[4].Disabled = false;
      this.LevelManager.Scripts[5].Disabled = false;
      foreach (ArtObjectInstance ring in this.Rings)
        this.LevelManager.RemoveArtObject(ring);
    }
    else
    {
      this.LevelManager.Scripts[4].Disabled = true;
      this.LevelManager.Scripts[5].Disabled = true;
      this.maskRenderer.Visible = false;
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || this.GameState.InMenuCube)
      return;
    this.SinceStarted += (float) gameTime.ElapsedGameTime.TotalSeconds;
    if ((double) this.SinceStarted > 8.0 && (double) this.SinceStarted < 19.0)
      this.SpinSpeed = Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.SinceStarted - 8.0) / 5.0)), EasingType.Sine) * 0.005f;
    else if ((double) this.SinceStarted > 19.0)
      this.SpinSpeed = (float) (0.004999999888241291 + (double) Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.SinceStarted - 19.0) / 20.0)), EasingType.Quadratic) * 0.5);
    if ((double) this.SinceStarted > 33.0 && this.Rings != null)
    {
      this.TrialTimeAccumulator += (float) gameTime.ElapsedGameTime.TotalSeconds;
      this.UpdateRays((float) gameTime.ElapsedGameTime.TotalSeconds);
    }
    if (this.Rings == null)
      return;
    this.Rings[0].Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, this.SpinSpeed) * this.Rings[0].Rotation;
    this.Rings[1].Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, this.SpinSpeed) * this.Rings[1].Rotation;
    this.Rings[2].Rotation = Quaternion.CreateFromAxisAngle(Vector3.Left, this.SpinSpeed) * this.Rings[2].Rotation;
    this.Rings[3].Rotation = Quaternion.CreateFromAxisAngle(Vector3.Down, this.SpinSpeed) * this.Rings[3].Rotation;
  }

  private void UpdateRays(float elapsedSeconds)
  {
    if (this.TrialRaysMesh.Groups.Count < 50 && RandomHelper.Probability(0.2))
    {
      float x = 6f + RandomHelper.Centered(4.0);
      float num = RandomHelper.Between(0.5, (double) x / 2.5);
      Group group = this.TrialRaysMesh.AddGroup();
      group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionTexture>(new FezVertexPositionTexture[6]
      {
        new FezVertexPositionTexture(new Vector3(0.0f, (float) ((double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(0.0f, 0.0f)),
        new FezVertexPositionTexture(new Vector3(x, num / 2f, 0.0f), new Vector2(1f, 0.0f)),
        new FezVertexPositionTexture(new Vector3(x, (float) ((double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(1f, 0.45f)),
        new FezVertexPositionTexture(new Vector3(x, (float) (-(double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(1f, 0.55f)),
        new FezVertexPositionTexture(new Vector3(x, (float) (-(double) num / 2.0), 0.0f), new Vector2(1f, 1f)),
        new FezVertexPositionTexture(new Vector3(0.0f, (float) (-(double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(0.0f, 1f))
      }, new int[12]{ 0, 1, 2, 0, 2, 5, 5, 2, 3, 5, 3, 4 }, PrimitiveType.TriangleList);
      group.CustomData = (object) new DotHost.RayState();
      group.Material = new Material()
      {
        Diffuse = new Vector3(0.0f)
      };
      group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Forward, RandomHelper.Between(0.0, 6.2831854820251465));
    }
    for (int index = this.TrialRaysMesh.Groups.Count - 1; index >= 0; --index)
    {
      Group group = this.TrialRaysMesh.Groups[index];
      DotHost.RayState customData = group.CustomData as DotHost.RayState;
      customData.Age += elapsedSeconds * 0.15f;
      float num = Easing.EaseOut((double) Easing.EaseOut(Math.Sin((double) customData.Age * 6.2831854820251465 - 1.5707963705062866) * 0.5 + 0.5, EasingType.Quintic), EasingType.Quintic);
      group.Material.Diffuse = Vector3.Lerp(Vector3.One, customData.Tint.ToVector3(), 0.05f) * 0.15f * num;
      float speed = customData.Speed;
      group.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Forward, (float) ((double) elapsedSeconds * (double) speed * (0.10000000149011612 + (double) Easing.EaseIn((double) this.TrialTimeAccumulator / 3.0, EasingType.Quadratic) * 0.20000000298023224)));
      group.Scale = new Vector3((float) ((double) num * 0.75 + 0.25), (float) ((double) num * 0.5 + 0.5), 1f);
      if ((double) customData.Age > 1.0)
        this.TrialRaysMesh.RemoveGroupAt(index);
    }
    this.TrialFlareMesh.Position = this.TrialRaysMesh.Position = this.Rings[0].Position;
    this.TrialFlareMesh.Rotation = this.TrialRaysMesh.Rotation = this.CameraManager.Rotation;
    this.TrialRaysMesh.Scale = new Vector3(Easing.EaseIn((double) this.TrialTimeAccumulator / 2.0, EasingType.Quadratic) + 1f);
    this.TrialFlareMesh.Material.Opacity = (float) (0.125 + (double) Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.TrialTimeAccumulator - 2.0) / 3.0)), EasingType.Cubic) * 0.875);
    this.TrialFlareMesh.Scale = Vector3.One + this.TrialRaysMesh.Scale * Easing.EaseIn((double) Math.Max(this.TrialTimeAccumulator - 2.5f, 0.0f) / 1.5, EasingType.Cubic) * 4f;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    if ((double) this.SinceStarted > 19.0 && this.Rings != null)
    {
      this.AoVisibility.Clear();
      foreach (ArtObjectInstance levelArtObject in this.LevelMaterializer.LevelArtObjects)
      {
        this.AoVisibility.Add(levelArtObject.Visible);
        levelArtObject.Visible = false;
        levelArtObject.ArtObject.Group.Enabled = false;
      }
      for (int index = 0; index < 4; ++index)
      {
        ArtObjectInstance ring = this.Rings[index];
        this.OriginalTextures[index] = ring.ArtObject.Group.TextureMap;
        ring.Visible = true;
        ring.ArtObject.Group.Enabled = true;
        ring.ArtObject.Group.Texture = (Texture) this.WhiteTex;
        ring.Material.Opacity = Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.SinceStarted - 19.0) / 18.0)), EasingType.Cubic);
        ring.Update();
      }
      this.LevelMaterializer.ArtObjectsMesh.Draw();
      for (int index = 0; index < 4; ++index)
      {
        this.Rings[index].ArtObject.Group.Texture = (Texture) this.OriginalTextures[index];
        this.OriginalTextures[index] = (Texture2D) null;
        this.Rings[index].Material.Opacity = 1f;
      }
      int num = 0;
      foreach (ArtObjectInstance levelArtObject in this.LevelMaterializer.LevelArtObjects)
      {
        levelArtObject.Visible = this.AoVisibility[num++];
        if (levelArtObject.Visible)
          levelArtObject.ArtObject.Group.Enabled = true;
      }
    }
    if ((double) this.SinceStarted > 36.75)
    {
      if (this.Rings != null)
      {
        foreach (ArtObjectInstance ring in this.Rings)
          this.LevelManager.RemoveArtObject(ring);
        this.maskRenderer.Visible = true;
        this.LevelManager.Scripts[4].Disabled = false;
        this.LevelManager.Scripts[5].Disabled = false;
        this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(0);
      }
      this.Rings = (ArtObjectInstance[]) null;
      float num = Easing.EaseIn(1.0 - (double) FezMath.Saturate((float) (((double) this.SinceStarted - 36.75) / 6.0)), EasingType.Sine);
      if (FezMath.AlmostEqual(num, 0.0f))
        return;
      this.TargetRenderer.DrawFullscreen(new Color(1f, 1f, 1f, num));
    }
    else
    {
      if ((double) this.SinceStarted <= 33.0)
        return;
      this.TargetRenderer.DrawFullscreen(new Color(1f, 1f, 1f, FezMath.Saturate(Easing.EaseIn(((double) this.TrialTimeAccumulator - 3.0) / 0.75, EasingType.Quintic))));
      this.TrialRaysMesh.Draw();
      this.TrialFlareMesh.Draw();
    }
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  private class MaskRenderer : DrawableGameComponent
  {
    private Texture2D PyramidWarp;
    private Mesh PyramidMask;
    private Mesh WarpCube;
    public Vector3 Center;

    public MaskRenderer(Game game)
      : base(game)
    {
      this.DrawOrder = 6;
    }

    protected override void LoadContent()
    {
      base.LoadContent();
      this.PyramidMask = new Mesh() { DepthWrites = false };
      this.PyramidMask.AddFace(new Vector3(8f), Vector3.Zero, FaceOrientation.Front, true, true);
      this.WarpCube = new Mesh() { DepthWrites = false };
      this.WarpCube.AddFace(new Vector3(16f), 8f * Vector3.UnitZ, FaceOrientation.Front, true, false);
      this.WarpCube.AddFace(new Vector3(16f), 8f * Vector3.Right, FaceOrientation.Right, true, false);
      this.WarpCube.AddFace(new Vector3(16f), 8f * Vector3.Left, FaceOrientation.Left, true, false);
      this.WarpCube.AddFace(new Vector3(16f), 8f * -Vector3.UnitZ, FaceOrientation.Back, true, false);
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.PyramidMask.Effect = (BaseEffect) new DefaultEffect.Textured();
        this.WarpCube.Effect = (BaseEffect) new DefaultEffect.Textured();
        this.PyramidWarp = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/warp/pyramid");
        this.WarpCube.Texture = (Dirtyable<Texture>) (Texture) this.PyramidWarp;
      }));
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this.PyramidMask != null)
        this.PyramidMask.Dispose();
      this.PyramidMask = (Mesh) null;
      if (this.WarpCube != null)
        this.WarpCube.Dispose();
      this.WarpCube = (Mesh) null;
      this.PyramidWarp = (Texture2D) null;
    }

    public override void Draw(GameTime gameTime)
    {
      if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || this.GameState.InMenuCube)
        return;
      Vector3 a = this.Center - this.CameraManager.InterpolatedCenter;
      Vector2 vector2 = a.Dot(this.CameraManager.View.Right) * Vector2.UnitX + a.Y * Vector2.UnitY;
      this.WarpCube.Position = this.PyramidMask.Position = this.Center;
      this.WarpCube.TextureMatrix = (Dirtyable<Matrix>) new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, vector2.X / 48f, (float) (-(double) vector2.Y / 48.0 + 0.10000000149011612), 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.WarpGate));
      this.PyramidMask.Draw();
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.WarpGate);
      this.WarpCube.Draw();
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }

    [ServiceDependency]
    public IContentManagerProvider CMProvider { private get; set; }

    [ServiceDependency]
    public IGameCameraManager CameraManager { private get; set; }
  }
}
