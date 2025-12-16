// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TempleOfLoveHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
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

internal class TempleOfLoveHost : DrawableGameComponent
{
  private static readonly Vector3 HeartCenter = new Vector3(10f, 24.5f, 10f);
  private readonly Vector3[] PieceOffsets = new Vector3[3]
  {
    new Vector3(-0.5f, -0.5f, 0.0f),
    new Vector3(0.5f, -0.5f, 0.0f),
    new Vector3(0.5f, 0.5f, 0.0f)
  };
  private TempleOfLoveHost.Phases Phase;
  private Mesh WireHeart;
  private Mesh CrumblingHeart;
  private Mesh RaysMesh;
  private Mesh FlareMesh;
  private SoundEffect sRayWhiteout;
  private float TimeAccumulator;
  private float WireHeartFactor = 1f;
  private float PhaseTime;

  public TempleOfLoveHost(Game game)
    : base(game)
  {
    this.DrawOrder = 100;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Enabled = this.Visible = this.LevelManager.Name == "TEMPLE_OF_LOVE";
    this.Destroy();
    if (this.Enabled && this.GameState.SaveData.HasDoneHeartReboot)
    {
      foreach (BackgroundPlane plane in this.LevelManager.BackgroundPlanes.Values.ToArray<BackgroundPlane>())
      {
        if (plane.ActorType == ActorType.Waterfall || plane.ActorType == ActorType.Trickle || plane.TextureName.Contains("water") || plane.TextureName.Contains("fountain") || plane.AttachedPlane.HasValue)
          this.LevelManager.RemovePlane(plane);
      }
      this.Enabled = this.Visible = false;
    }
    if (!this.Enabled)
      return;
    this.sRayWhiteout = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/HexRebuild/RayWhiteout");
    this.WireHeart = new Mesh();
    this.CrumblingHeart = new Mesh();
    Color pink = Color.Pink;
    this.WireHeart.AddGroup().Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionColor>(new FezVertexPositionColor[12]
    {
      new FezVertexPositionColor(new Vector3(-1f, -1f, -0.5f), pink),
      new FezVertexPositionColor(new Vector3(-1f, 0.0f, -0.5f), pink),
      new FezVertexPositionColor(new Vector3(0.0f, 0.0f, -0.5f), pink),
      new FezVertexPositionColor(new Vector3(0.0f, 1f, -0.5f), pink),
      new FezVertexPositionColor(new Vector3(1f, 1f, -0.5f), pink),
      new FezVertexPositionColor(new Vector3(1f, -1f, -0.5f), pink),
      new FezVertexPositionColor(new Vector3(-1f, -1f, 0.5f), pink),
      new FezVertexPositionColor(new Vector3(-1f, 0.0f, 0.5f), pink),
      new FezVertexPositionColor(new Vector3(0.0f, 0.0f, 0.5f), pink),
      new FezVertexPositionColor(new Vector3(0.0f, 1f, 0.5f), pink),
      new FezVertexPositionColor(new Vector3(1f, 1f, 0.5f), pink),
      new FezVertexPositionColor(new Vector3(1f, -1f, 0.5f), pink)
    }, new int[36]
    {
      0,
      1,
      1,
      2,
      2,
      3,
      3,
      4,
      4,
      5,
      5,
      0,
      6,
      7,
      7,
      8,
      8,
      9,
      9,
      10,
      10,
      11,
      11,
      6,
      0,
      6,
      1,
      7,
      2,
      8,
      3,
      9,
      4,
      10,
      5,
      11
    }, PrimitiveType.LineList);
    foreach (Vector3 pieceOffset in this.PieceOffsets)
      this.WireHeart.AddWireframeBox(Vector3.One, pieceOffset, new Color(new Vector4(Color.DeepPink.ToVector3(), 0.125f)), true);
    Trile[] trileArray = new Trile[8]
    {
      this.LevelManager.TrileSet.Triles[244],
      this.LevelManager.TrileSet.Triles[245],
      this.LevelManager.TrileSet.Triles[251],
      this.LevelManager.TrileSet.Triles[246],
      this.LevelManager.TrileSet.Triles[247],
      this.LevelManager.TrileSet.Triles[248],
      this.LevelManager.TrileSet.Triles[249],
      this.LevelManager.TrileSet.Triles[250]
    };
    int num = 0;
    foreach (Vector3 pieceOffset in this.PieceOffsets)
    {
      foreach (Trile trile in trileArray)
      {
        Group group = this.CrumblingHeart.AddGroup();
        group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) trile.Geometry.Vertices).ToArray<VertexPositionNormalTextureInstance>(), trile.Geometry.Indices, trile.Geometry.PrimitiveType);
        group.Position = pieceOffset;
        group.Enabled = this.GameState.SaveData.PiecesOfHeart > num;
        group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1.57079637f);
      }
      ++num;
    }
    this.WireHeart.Rotation = this.CrumblingHeart.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, -0.7853982f);
    this.WireHeart.BakeTransform<FezVertexPositionColor>();
    this.CrumblingHeart.BakeTransform<VertexPositionNormalTextureInstance>();
    foreach (Group group in this.CrumblingHeart.Groups)
    {
      IndexedUserPrimitives<VertexPositionNormalTextureInstance> geometry = group.Geometry as IndexedUserPrimitives<VertexPositionNormalTextureInstance>;
      Vector3 zero = Vector3.Zero;
      foreach (VertexPositionNormalTextureInstance vertex in geometry.Vertices)
        zero += vertex.Position;
      group.CustomData = (object) (zero / (float) geometry.Vertices.Length);
    }
    this.RaysMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      DepthWrites = false
    };
    this.FlareMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.FlareMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.WireHeart.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      Mesh crumblingHeart = this.CrumblingHeart;
      crumblingHeart.Effect = (BaseEffect) new DefaultEffect.LitTextured()
      {
        Specular = true,
        Emissive = 0.5f,
        AlphaIsEmissive = true
      };
      this.CrumblingHeart.Texture = this.LevelMaterializer.TrilesMesh.Texture;
      this.RaysMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.FlareMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.FlareMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/flare_alpha");
    }));
    this.WireHeart.Position = this.CrumblingHeart.Position = TempleOfLoveHost.HeartCenter;
  }

  private void Destroy()
  {
    this.WireHeart = this.CrumblingHeart = (Mesh) null;
    this.TimeAccumulator = 0.0f;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || this.GameState.Loading || this.GameState.InMenuCube)
      return;
    this.TimeAccumulator += (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.WireHeart.Position = this.CrumblingHeart.Position = TempleOfLoveHost.HeartCenter + Vector3.UnitY * (float) Math.Sin((double) this.TimeAccumulator / 2.0) / 2f;
    this.WireHeart.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) (gameTime.ElapsedGameTime.TotalSeconds / 2.0));
    this.CrumblingHeart.Rotation = this.WireHeart.Rotation;
    if (this.GameState.SaveData.HasDoneHeartReboot && this.Phase == TempleOfLoveHost.Phases.None)
      this.Phase = TempleOfLoveHost.Phases.CrumbleOut;
    switch (this.Phase)
    {
      case TempleOfLoveHost.Phases.CrumbleOut:
        this.PhaseTime += (float) gameTime.ElapsedGameTime.TotalSeconds;
        foreach (Group group in this.CrumblingHeart.Groups)
        {
          Vector3 customData = (Vector3) group.CustomData;
          float y = (float) Math.Pow((double) Math.Max(this.PhaseTime - (float) (1.0 - ((double) customData.Y * 2.0 + (double) customData.Z + (double) customData.X)), 0.0f) * 0.875, 2.0);
          group.Position = new Vector3(0.0f, y, 0.0f) + customData * y / 5f;
          group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(customData), y / 10f);
        }
        this.WireHeartFactor = MathHelper.Lerp(this.WireHeartFactor, (float) (1 - RandomHelper.Probability((double) this.PhaseTime / 7.0).AsNumeric()), (float) (0.125 + (double) this.PhaseTime / 7.0 * 0.20000000298023224));
        if ((double) this.PhaseTime <= 7.0)
          break;
        this.PhaseTime = 0.0f;
        this.WireHeartFactor = 0.0f;
        this.Phase = TempleOfLoveHost.Phases.ShineReboot;
        this.SoundManager.PlayNewSong((string) null, 4f);
        this.SoundManager.MuteAmbienceTracks();
        this.SoundManager.KillSounds(4f);
        this.sRayWhiteout.Emit();
        break;
      case TempleOfLoveHost.Phases.ShineReboot:
        this.PhaseTime += (float) gameTime.ElapsedGameTime.TotalSeconds;
        foreach (Group group in this.CrumblingHeart.Groups)
        {
          Vector3 customData = (Vector3) group.CustomData;
          float y = (float) Math.Pow((double) Math.Max(this.PhaseTime + 7f - (float) (1.0 - ((double) customData.Y * 2.0 + (double) customData.Z + (double) customData.X)), 0.0f) * 0.875, 2.0);
          group.Position = new Vector3(0.0f, y, 0.0f) + customData * y / 5f;
          group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Normalize(customData), y / 10f);
        }
        this.UpdateRays((float) gameTime.ElapsedGameTime.TotalSeconds);
        if ((double) this.PhaseTime <= 4.0)
          break;
        this.SmoothReboot();
        break;
    }
  }

  private void SmoothReboot()
  {
    ServiceHelper.AddComponent((IGameComponent) new Intro(this.Game)
    {
      Fake = true,
      FakeLevel = "GOMEZ_HOUSE",
      Glitch = false
    });
    Waiters.Wait(0.10000000149011612, (Action) (() =>
    {
      this.Destroy();
      this.Enabled = this.Visible = false;
    }));
    this.Enabled = false;
  }

  private void UpdateRays(float elapsedSeconds)
  {
    int num = (double) this.PhaseTime > 1.5 ? 1 : 0;
    this.MakeRay();
    if (num != 0)
      this.MakeRay();
    for (int index = this.RaysMesh.Groups.Count - 1; index >= 0; --index)
    {
      Group group = this.RaysMesh.Groups[index];
      DotHost.RayState customData = group.CustomData as DotHost.RayState;
      customData.Age += elapsedSeconds * 0.15f;
      group.Material.Diffuse = Vector3.One * FezMath.Saturate(customData.Age * 8f);
      group.Scale *= new Vector3(1.5f, 1f, 1f);
      if ((double) customData.Age > 1.0)
        this.RaysMesh.RemoveGroupAt(index);
    }
    this.RaysMesh.AlwaysOnTop = false;
    this.FlareMesh.Position = this.RaysMesh.Position = TempleOfLoveHost.HeartCenter;
    this.FlareMesh.Rotation = this.RaysMesh.Rotation = this.CameraManager.Rotation;
    this.FlareMesh.Material.Opacity = Easing.EaseIn((double) FezMath.Saturate(this.PhaseTime / 2.5f), EasingType.Cubic);
    this.FlareMesh.Scale = Vector3.One + this.RaysMesh.Scale * Easing.EaseIn(((double) this.PhaseTime - 0.25) / 1.75, EasingType.Decic) * 4f;
  }

  private void MakeRay()
  {
    if (this.RaysMesh.Groups.Count >= 150 || !RandomHelper.Probability(0.1 + (double) Easing.EaseIn((double) FezMath.Saturate(this.PhaseTime / 1.75f), EasingType.Sine) * 0.9))
      return;
    float num = RandomHelper.Probability(0.75) ? 0.1f : 0.4f;
    Group group = this.RaysMesh.AddGroup();
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionColor>(new FezVertexPositionColor[6]
    {
      new FezVertexPositionColor(new Vector3(0.0f, (float) ((double) num / 2.0 * 0.5), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, num / 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, (float) ((double) num / 2.0 * 0.5), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, (float) (-(double) num / 2.0 * 0.5), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, (float) (-(double) num / 2.0), 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(0.0f, (float) (-(double) num / 2.0 * 0.5), 0.0f), Color.White)
    }, new int[12]{ 0, 1, 2, 0, 2, 5, 5, 2, 3, 5, 3, 4 }, PrimitiveType.TriangleList);
    group.CustomData = (object) new DotHost.RayState();
    group.Material = new Material()
    {
      Diffuse = new Vector3(0.0f)
    };
    group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, RandomHelper.Between(0.0, -3.1415927410125732)) * Quaternion.CreateFromAxisAngle(Vector3.Forward, RandomHelper.Between(0.0, 6.2831854820251465));
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
    this.CrumblingHeart.AlwaysOnTop = true;
    this.CrumblingHeart.Position += this.CameraManager.InverseView.Forward * 3f;
    this.CrumblingHeart.Draw();
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    this.CrumblingHeart.AlwaysOnTop = false;
    this.CrumblingHeart.Position -= this.CameraManager.InverseView.Forward * 3f;
    this.CrumblingHeart.Draw();
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Wirecube));
    this.WireHeart.DepthWrites = false;
    this.WireHeart.Material.Opacity = (float) (0.05000000074505806 + (double) Math.Abs((float) Math.Cos((double) this.TimeAccumulator * 3.0)) * 0.20000000298023224) * this.WireHeartFactor;
    float num = (float) (1.0 / 16.0 / (double) this.CameraManager.PixelsPerTrixel * (double) Math.Abs((float) Math.Sin((double) this.TimeAccumulator)) * 8.0);
    this.WireHeart.Position += num * Vector3.UnitY;
    this.WireHeart.Draw();
    this.WireHeart.Position -= num * Vector3.UnitY;
    this.WireHeart.Position -= num * Vector3.UnitY;
    this.WireHeart.Draw();
    this.WireHeart.Position += num * Vector3.UnitY;
    this.WireHeart.Position += num * Vector3.UnitX;
    this.WireHeart.Draw();
    this.WireHeart.Position -= num * Vector3.UnitX;
    this.WireHeart.Position -= num * Vector3.UnitX;
    this.WireHeart.Draw();
    this.WireHeart.Position += num * Vector3.UnitX;
    this.WireHeart.Position += num * Vector3.UnitZ;
    this.WireHeart.Draw();
    this.WireHeart.Position -= num * Vector3.UnitZ;
    this.WireHeart.Position -= num * Vector3.UnitZ;
    this.WireHeart.Draw();
    this.WireHeart.Position += num * Vector3.UnitZ;
    this.WireHeart.Material.Opacity = this.WireHeartFactor;
    this.WireHeart.Draw();
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    if (this.Phase != TempleOfLoveHost.Phases.ShineReboot)
      return;
    this.RaysMesh.Draw();
    this.FlareMesh.Draw();
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public ITrixelParticleSystems ParticleSystems { get; set; }

  private enum Phases
  {
    None,
    CrumbleOut,
    ShineReboot,
  }
}
