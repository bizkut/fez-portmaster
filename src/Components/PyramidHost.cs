// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PyramidHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;

#nullable disable
namespace FezGame.Components;

internal class PyramidHost : DrawableGameComponent
{
  private static readonly Vector2 DoorCenter = new Vector2(25f, 168f);
  private ArtObjectInstance MotherCubeAo;
  private Vector3 OriginalPosition;
  private float TimeAccumulator;
  private bool DoCapture;
  private Vector3 OriginalCenter;
  private SoundEffect sRotationDrone;
  private SoundEffect sWhiteOut;
  private SoundEmitter eRotationDrone;
  private Mesh RaysMesh;
  private Mesh FlareMesh;

  public PyramidHost(Game game)
    : base(game)
  {
    this.DrawOrder = 500;
    this.Enabled = this.Visible = false;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.KeyboardManager.RegisterKey(Keys.R);
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Visible = this.Enabled = this.LevelManager.Name == "PYRAMID";
    this.Clear();
    if (!this.Enabled)
      return;
    this.MotherCubeAo = this.LevelManager.ArtObjects[217];
    this.OriginalPosition = this.MotherCubeAo.Position;
    this.RaysMesh = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
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
      this.RaysMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.RaysMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/smooth_ray");
      this.FlareMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.FlareMesh.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/flare_alpha");
    }));
    this.sRotationDrone = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Ending/Pyramid/MothercubeRotateDrone");
    this.sWhiteOut = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ending/Pyramid/WhiteOut");
    this.eRotationDrone = this.sRotationDrone.EmitAt(this.OriginalPosition, true);
  }

  private void Clear()
  {
    this.MotherCubeAo = (ArtObjectInstance) null;
    this.DoCapture = false;
    this.TimeAccumulator = 0.0f;
    if (this.RaysMesh != null)
      this.RaysMesh.Dispose();
    if (this.FlareMesh != null)
      this.FlareMesh.Dispose();
    this.FlareMesh = this.RaysMesh = (Mesh) null;
    this.sRotationDrone = this.sWhiteOut = (SoundEffect) null;
    if (this.eRotationDrone == null || this.eRotationDrone.Dead)
      return;
    this.eRotationDrone.FadeOutAndDie(0.0f);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || !this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.Loading)
      return;
    if (this.DoCapture)
    {
      this.TimeAccumulator += (float) gameTime.ElapsedGameTime.TotalSeconds;
      this.MotherCubeAo.Position = Vector3.Lerp(this.MotherCubeAo.Position, this.OriginalPosition, 0.025f);
      this.MotherCubeAo.Rotation = Quaternion.Slerp(this.MotherCubeAo.Rotation, Quaternion.CreateFromYawPitchRoll(this.CameraManager.Viewpoint.VisibleOrientation().ToPhi(), 0.0f, 0.0f), 0.025f);
      this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.PlayerManager.Position * this.CameraManager.Viewpoint.DepthMask() + PyramidHost.DoorCenter.X * this.CameraManager.Viewpoint.SideMask() + PyramidHost.DoorCenter.Y * Vector3.UnitY - Vector3.UnitY * 0.125f, 0.025f);
      this.GameState.SkipRendering = true;
      this.CameraManager.Center = Vector3.Lerp(this.OriginalCenter, this.PlayerManager.Position, 0.025f);
      this.GameState.SkipRendering = false;
      this.UpdateRays((float) gameTime.ElapsedGameTime.TotalSeconds);
      if ((double) this.TimeAccumulator <= 6.0)
        return;
      this.GameState.SkipLoadScreen = true;
      this.LevelManager.ChangeLevel("HEX_REBUILD");
      Waiters.Wait((Func<bool>) (() => !this.GameState.Loading), (Action) (() =>
      {
        this.GameState.SkipLoadScreen = false;
        this.Clear();
        this.Visible = false;
      }));
      this.Enabled = false;
    }
    else
    {
      this.TimeAccumulator += (float) (gameTime.ElapsedGameTime.TotalSeconds / 2.0);
      this.TimeAccumulator = FezMath.WrapAngle(this.TimeAccumulator);
      this.MotherCubeAo.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) (-gameTime.ElapsedGameTime.TotalSeconds * 0.375));
      Vector3 vector3 = new Vector3(0.0f, (float) Math.Sin((double) this.TimeAccumulator), 0.0f) / 2f;
      this.MotherCubeAo.Position = this.OriginalPosition + vector3;
      Vector2 vector2 = new Vector2(this.PlayerManager.Center.Dot(this.CameraManager.Viewpoint.SideMask()), this.PlayerManager.Center.Y);
      if ((double) Math.Abs(vector2.X - PyramidHost.DoorCenter.X) >= 1.0 || (double) Math.Abs(vector2.Y - (PyramidHost.DoorCenter.Y + vector3.Y)) >= 1.0 || (double) FezMath.AngleBetween(Vector3.Transform(-Vector3.UnitZ, this.MotherCubeAo.Rotation), this.CameraManager.Viewpoint.ForwardVector()) >= 0.25)
        return;
      this.DoCapture = true;
      this.TimeAccumulator = 0.0f;
      this.PlayerManager.CanControl = false;
      this.PlayerManager.Action = ActionType.Floating;
      this.PlayerManager.Velocity = Vector3.Zero;
      this.OriginalCenter = this.CameraManager.Center;
      this.eRotationDrone.FadeOutAndDie(1.5f);
      this.SoundManager.PlayNewSong(5f);
      this.sWhiteOut.Emit().Persistent = true;
    }
  }

  private void UpdateRays(float elapsedSeconds)
  {
    if (this.RaysMesh.Groups.Count < 50 && RandomHelper.Probability(0.25))
    {
      float x = 6f + RandomHelper.Centered(4.0);
      float num = RandomHelper.Between(0.5, (double) x / 2.5);
      Group group = this.RaysMesh.AddGroup();
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
    for (int index = this.RaysMesh.Groups.Count - 1; index >= 0; --index)
    {
      Group group = this.RaysMesh.Groups[index];
      DotHost.RayState customData = group.CustomData as DotHost.RayState;
      customData.Age += elapsedSeconds * 0.15f;
      float num = Easing.EaseOut((double) Easing.EaseOut(Math.Sin((double) customData.Age * 6.2831854820251465 - 1.5707963705062866) * 0.5 + 0.5, EasingType.Quintic), EasingType.Quintic);
      group.Material.Diffuse = Vector3.Lerp(Vector3.One, customData.Tint.ToVector3(), 0.05f) * 0.15f * num;
      float speed = customData.Speed;
      group.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Forward, (float) ((double) elapsedSeconds * (double) speed * (0.10000000149011612 + (double) Easing.EaseIn((double) this.TimeAccumulator / 3.0, EasingType.Quadratic) * 0.20000000298023224)));
      group.Scale = new Vector3((float) ((double) num * 0.75 + 0.25), (float) ((double) num * 0.5 + 0.5), 1f);
      if ((double) customData.Age > 1.0)
        this.RaysMesh.RemoveGroupAt(index);
    }
    this.FlareMesh.Position = this.RaysMesh.Position = this.PlayerManager.Center;
    this.FlareMesh.Rotation = this.RaysMesh.Rotation = this.CameraManager.Rotation;
    this.RaysMesh.Scale = new Vector3(Easing.EaseIn((double) this.TimeAccumulator / 2.0, EasingType.Quadratic) + 1f);
    this.FlareMesh.Material.Opacity = (float) (0.125 + (double) Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.TimeAccumulator - 2.0) / 3.0)), EasingType.Cubic) * 0.875);
    this.FlareMesh.Scale = Vector3.One + this.RaysMesh.Scale * Easing.EaseIn((double) Math.Max(this.TimeAccumulator - 2.5f, 0.0f) / 1.5, EasingType.Cubic) * 4f;
    if (this.KeyboardManager.GetKeyState(Keys.R) != FezButtonState.Pressed)
      return;
    this.TimeAccumulator = 0.0f;
    this.RaysMesh.ClearGroups();
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || this.GameState.Loading)
      return;
    base.Draw(gameTime);
    this.RaysMesh.Draw();
    this.FlareMesh.Draw();
    if (!this.DoCapture)
      return;
    this.TargetRenderer.DrawFullscreen(new Color(1f, 1f, 1f, FezMath.Saturate(Easing.EaseIn(((double) this.TimeAccumulator - 6.0) / 1.0, EasingType.Quintic))));
  }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardManager { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }
}
