// Decompiled with JetBrains decompiler
// Type: FezGame.Components.LoadingScreen
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class LoadingScreen : DrawableGameComponent
{
  private const float FadeTime = 0.5f;
  private Mesh mesh;
  private float sinceBgShown;
  private float sinceCubeShown = -0.5f;
  private float bgOpacity;
  private float cubeOpacity;
  private Texture2D starBack;
  private FakeDot fakeDot;
  private SoundEffect sDrone;
  private SoundEffectInstance iDrone;
  private bool loadingCubeWasUnlit;

  public LoadingScreen(Game game)
    : base(game)
  {
    this.DrawOrder = 2100;
  }

  protected override void LoadContent()
  {
    TrileSet ts = this.CMProvider.Global.Load<TrileSet>("Trile Sets/LOADING");
    this.mesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.PointClamp,
      AlwaysOnTop = false,
      DepthWrites = true,
      Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f)
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.mesh.Texture = (Dirtyable<Texture>) (Texture) ts.TextureAtlas;
      this.loadingCubeWasUnlit = false;
      Mesh mesh = this.mesh;
      mesh.Effect = (BaseEffect) new DefaultEffect.LitTextured()
      {
        Specular = true,
        AlphaIsEmissive = true,
        Emissive = 0.5f,
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up))
      };
      this.starBack = this.CMProvider.Global.Load<Texture2D>("Other Textures/hud/starback");
    }));
    Group group = this.mesh.AddGroup();
    ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry = ts.Triles[0].Geometry;
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) geometry.Vertices).ToArray<VertexPositionNormalTextureInstance>(), geometry.Indices, geometry.PrimitiveType);
    group.Scale = new Vector3(0.95f);
    this.sDrone = this.CMProvider.Global.Load<SoundEffect>("Sounds/Intro/FezLogoDrone");
    ServiceHelper.AddComponent((IGameComponent) (this.fakeDot = new FakeDot(ServiceHelper.Game)));
    this.LevelManager.LevelChanged += (Action) (() => DrawActionScheduler.Schedule((Action) (() =>
    {
      bool flag = this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.BlinkingAlpha;
      if (this.loadingCubeWasUnlit == flag && this.mesh.Effect != null)
        return;
      if (this.mesh.Effect != null)
        this.mesh.Effect.Dispose();
      if (flag)
      {
        Mesh mesh = this.mesh;
        mesh.Effect = (BaseEffect) new DefaultEffect.Textured()
        {
          AlphaIsEmissive = true
        };
      }
      else
      {
        Mesh mesh = this.mesh;
        mesh.Effect = (BaseEffect) new DefaultEffect.LitTextured()
        {
          Specular = true,
          Emissive = 0.5f,
          AlphaIsEmissive = true
        };
      }
      this.mesh.Effect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up));
      this.mesh.TextureMatrix.Dirty = true;
      this.loadingCubeWasUnlit = flag;
    })));
  }

  private void CreateDrone()
  {
    this.iDrone = this.sDrone.CreateInstance();
    this.iDrone.IsLooped = true;
    this.iDrone.Volume = 0.0f;
    this.iDrone.Play();
  }

  private void KillDrone()
  {
    Waiters.Interpolate(1.0, (Action<float>) (s => this.iDrone.Volume = FezMath.Saturate(1f - s)), (Action) (() =>
    {
      this.iDrone.Stop();
      this.iDrone.Dispose();
      this.iDrone = (SoundEffectInstance) null;
    }));
    foreach (SoundEmitter emitter in this.SoundManager.Emitters)
      emitter.VolumeMaster = 1f;
    this.SoundManager.MusicVolumeFactor = 1f;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.SkipLoadScreen)
      return;
    if (this.GameState.SkipLoadBackground)
      this.sinceBgShown = 0.0f;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    if ((!this.GameState.Loading ? 0 : (!this.GameState.ScheduleLoadEnd ? 1 : (this.GameState.DotLoading ? 1 : 0))) != 0 || this.GameState.ForceLoadIcon)
    {
      if (!this.GameState.SkipLoadBackground && (double) this.bgOpacity < 1.0)
        this.sinceBgShown += totalSeconds;
      if ((double) this.cubeOpacity < 1.0 && !this.GameState.DotLoading)
      {
        this.sinceCubeShown += totalSeconds;
        this.cubeOpacity = FezMath.Saturate(this.sinceCubeShown / 0.5f);
      }
    }
    else
    {
      this.sinceCubeShown = -0.5f;
      if (!this.GameState.SkipLoadBackground && (double) this.bgOpacity > 0.0)
        this.sinceBgShown -= totalSeconds * 1.25f;
      if ((double) this.cubeOpacity > 0.0)
      {
        this.cubeOpacity -= (float) ((double) totalSeconds * 1.25 / 0.5);
        this.cubeOpacity = FezMath.Saturate(this.cubeOpacity);
      }
    }
    float bgOpacity = this.bgOpacity;
    this.bgOpacity = FezMath.Saturate(this.sinceBgShown / 0.5f);
    if ((double) bgOpacity == 1.0 && (double) this.bgOpacity < (double) bgOpacity && this.iDrone != null)
      this.KillDrone();
    if (!this.GameState.DotLoading)
      return;
    this.fakeDot.Update(gameTime);
  }

  public override void Draw(GameTime gameTime)
  {
    this.GameState.LoadingVisible = false;
    if (!this.GameState.DotLoading && (double) this.cubeOpacity == 0.0 || this.GameState.SkipLoadScreen || this.GameState.FarawaySettings.InTransition)
      return;
    this.GameState.LoadingVisible = true;
    if (!this.GameState.SkipLoadBackground)
    {
      if (this.GameState.DotLoading)
      {
        float m11 = Math.Max(this.GraphicsDevice.Viewport.AspectRatio / 1.77777779f, 1f);
        float m22 = Math.Max((float) (1.0 / (double) this.GraphicsDevice.Viewport.AspectRatio / (9.0 / 16.0)), 1f);
        this.GraphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
        this.TargetRenderer.DrawFullscreen((Texture) this.starBack, new Matrix(m11, 0.0f, 0.0f, 0.0f, 0.0f, m22, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f), new Color(1f, 1f, 1f, this.bgOpacity));
      }
      else
        this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, this.bgOpacity));
    }
    if (!this.GameState.DotLoading)
    {
      this.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1f, 0);
      Viewport viewport = this.GraphicsDevice.Viewport;
      double width = (double) viewport.Width;
      viewport = this.GraphicsDevice.Viewport;
      double height = (double) viewport.Height;
      float num = (float) (width / height);
      this.mesh.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(14f * num, 14f, 0.1f, 100f));
      this.mesh.Position = new Vector3(5.5f * num, -4.5f, 0.0f);
      this.mesh.Material.Opacity = this.cubeOpacity;
      this.mesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) (gameTime.ElapsedGameTime.TotalSeconds * 3.0)) * this.mesh.Rotation;
      this.mesh.FirstGroup.Position = new Vector3(0.0f, (float) (Math.Sin(gameTime.TotalGameTime.TotalSeconds * 3.1415927410125732) * 0.20000000298023224), 0.0f);
      this.mesh.Draw();
    }
    else
    {
      if (this.iDrone == null)
        this.CreateDrone();
      this.fakeDot.Opacity = this.bgOpacity;
      this.fakeDot.Draw(gameTime);
      this.iDrone.Volume = this.bgOpacity;
      this.SoundManager.MusicVolumeFactor = !this.GameState.ScheduleLoadEnd ? Math.Min(this.SoundManager.MusicVolumeFactor, 1f - this.bgOpacity) : 1f - this.bgOpacity;
      if (!this.GameState.ScheduleLoadEnd)
        return;
      foreach (SoundEmitter emitter in this.SoundManager.Emitters)
      {
        if (!emitter.Dead)
        {
          emitter.VolumeMaster = 1f - this.bgOpacity;
          emitter.Update();
        }
      }
    }
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IDotManager DotManager { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }
}
