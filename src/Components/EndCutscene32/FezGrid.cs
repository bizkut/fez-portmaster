// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene32.FezGrid
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components.EndCutscene32;

internal class FezGrid : DrawableGameComponent
{
  private const float SeparateDuration = 2f;
  private const float RotateZoomDuration = 12f;
  private const float GridFadeAlignDuration = 12f;
  private const float CubeRiseDuration = 12f;
  private readonly EndCutscene32Host Host;
  private Mesh GoMesh;
  private Group GomezGroup;
  private Group FezGroup;
  private Mesh TetraMesh;
  private Mesh CubeMesh;
  private Mesh StencilMesh;
  private float Time;
  private FezGrid.State ActiveState;

  public FezGrid(Game game, EndCutscene32Host host)
    : base(game)
  {
    this.Host = host;
    this.DrawOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.GoMesh = new Mesh();
    this.GomezGroup = this.GoMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, Color.White, true, false, false);
    this.FezGroup = this.GoMesh.AddFace(Vector3.One / 2f, Vector3.Zero, FaceOrientation.Front, Color.Red, true, false, false);
    this.TetraMesh = new Mesh();
    Vector3[] vector3Array = new Vector3[2560 /*0x0A00*/];
    for (int index = 0; index < 1280 /*0x0500*/; ++index)
    {
      vector3Array[index * 2] = new Vector3(-640f, 0.0f, (float) (index - 640));
      vector3Array[index * 2 + 1] = new Vector3(640f, 0.0f, (float) (index - 640));
    }
    Color[] array = Enumerable.Repeat<Color>(Color.Red, 2560 /*0x0A00*/).ToArray<Color>();
    this.TetraMesh.AddLines(array, vector3Array, true);
    this.TetraMesh.AddLines(array, ((IEnumerable<Vector3>) vector3Array).Select<Vector3, Vector3>((Func<Vector3, Vector3>) (v => new Vector3(v.Z, 0.0f, v.X))).ToArray<Vector3>(), true);
    this.CubeMesh = new Mesh();
    this.CubeMesh.AddFlatShadedBox(Vector3.One, Vector3.Zero, Color.White, true);
    this.StencilMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false
    };
    this.StencilMesh.AddFace(FezMath.XZMask * 1280f, Vector3.Zero, FaceOrientation.Top, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh goMesh = this.GoMesh;
      goMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        Fullbright = true
      };
      Mesh tetraMesh = this.TetraMesh;
      tetraMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        Fullbright = true
      };
      this.CubeMesh.Effect = (BaseEffect) new DefaultEffect.LitVertexColored();
      this.StencilMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
    }));
    this.LevelManager.ActualAmbient = new Color(0.25f, 0.25f, 0.25f);
    this.LevelManager.ActualDiffuse = Color.White;
    this.Reset();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.TetraMesh != null)
      this.TetraMesh.Dispose();
    if (this.GoMesh != null)
      this.GoMesh.Dispose();
    if (this.CubeMesh != null)
      this.CubeMesh.Dispose();
    if (this.StencilMesh != null)
      this.StencilMesh.Dispose();
    this.TetraMesh = this.GoMesh = this.CubeMesh = this.StencilMesh = (Mesh) null;
  }

  private void Reset()
  {
    this.GomezGroup.Position = Vector3.Zero;
    this.FezGroup.Position = new Vector3(-0.25f, 0.75f, 0.0f);
    this.GoMesh.Scale = Vector3.One;
    this.FezGroup.Rotation = this.GomezGroup.Rotation = Quaternion.Identity;
    float num = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    this.CameraManager.Center = Vector3.Zero;
    this.CameraManager.Direction = Vector3.UnitZ;
    this.CameraManager.Radius = 10f * this.GraphicsDevice.GetViewScale() * num;
    this.CameraManager.SnapInterpolation();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    this.Time += (float) gameTime.ElapsedGameTime.TotalSeconds;
    switch (this.ActiveState)
    {
      case FezGrid.State.Wait:
        this.Reset();
        if ((double) this.Time <= 2.0)
          break;
        this.ChangeState();
        break;
      case FezGrid.State.RotateZoom:
        float linearStep1 = FezMath.Saturate(this.Time / 12f);
        float amount1 = linearStep1;
        this.GomezGroup.Rotation = Quaternion.Slerp(Quaternion.Identity, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.7853982f), amount1);
        this.FezGroup.Rotation = Quaternion.Slerp(Quaternion.Identity, Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1.57079637f), amount1);
        float amount2 = Easing.EaseOut((double) linearStep1, EasingType.Cubic);
        this.GomezGroup.Position = Vector3.Lerp(Vector3.Zero, new Vector3(0.5f, -1f, 0.0f) / 2f, amount2);
        this.FezGroup.Position = Vector3.Lerp(new Vector3(-0.25f, 0.75f, 0.0f), new Vector3(-1.5f, 1.5f, 0.0f) / 2f, amount2);
        this.GoMesh.Scale = Vector3.Lerp(Vector3.One, new Vector3(40f), Easing.EaseIn((double) linearStep1, EasingType.Quartic));
        float amount3 = Easing.EaseInOut((double) FezMath.Saturate(linearStep1 * 2f), EasingType.Sine);
        this.CameraManager.Center = Vector3.Lerp(Vector3.Zero, Vector3.Transform(this.FezGroup.Position, this.GoMesh.WorldMatrix), amount3);
        this.CameraManager.SnapInterpolation();
        if ((double) this.Time <= 12.0)
          break;
        this.ChangeState();
        break;
      case FezGrid.State.GridFadeAlign:
        float linearStep2 = FezMath.Saturate(this.Time / 12f);
        this.TetraMesh.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreatePerspectiveFieldOfView(MathHelper.Lerp(1.57079637f, 2.3561945f, FezMath.Saturate(FezMath.Saturate(linearStep2 - 0.3f) / 0.6f)), this.CameraManager.AspectRatio, 0.1f, 2000f));
        float amount4 = Easing.EaseOut((double) linearStep2, EasingType.Quintic);
        float amount5 = Easing.EaseInOut((double) FezMath.Saturate(FezMath.Saturate(linearStep2 - 0.5f) / 0.5f), EasingType.Sine, EasingType.Sine);
        float step = Easing.EaseInOut((double) FezMath.Saturate(FezMath.Saturate(linearStep2 - 0.4f) / 0.6f), EasingType.Sine, EasingType.Sine);
        Vector3 cameraPosition1 = Vector3.Lerp(Vector3.UnitY * 360f, Vector3.UnitY * 2f, amount4) + Vector3.UnitX * linearStep2 * 100f;
        this.TetraMesh.Effect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(cameraPosition1, Vector3.Lerp(cameraPosition1 - Vector3.UnitY, cameraPosition1 + Vector3.UnitX - Vector3.UnitY, amount5), FezMath.Slerp(new Vector3((float) Math.Sin((double) linearStep2 * 3.1415927410125732 * 0.699999988079071), 0.0f, (float) Math.Cos((double) linearStep2 * 3.1415927410125732 * 0.699999988079071)), Vector3.UnitY, step)));
        if ((double) this.Time <= 12.0)
          break;
        this.ChangeState();
        break;
      case FezGrid.State.CubeRise:
        float linearStep3 = FezMath.Saturate(this.Time / 12f);
        this.CubeMesh.Position = this.CameraManager.Center;
        this.CubeMesh.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.One, Vector3.Zero, Vector3.Up));
        this.CubeMesh.Scale = Vector3.Lerp(Vector3.One, Vector3.One * 2f, Easing.EaseIn((double) FezMath.Saturate(linearStep3 - 0.6f) / 0.40000000596046448, EasingType.Quadratic));
        this.TetraMesh.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreatePerspectiveFieldOfView(2.3561945f, this.CameraManager.AspectRatio, 0.1f, 2000f));
        float amount6 = Easing.EaseIn((double) linearStep3, EasingType.Quadratic);
        Vector3 cameraPosition2 = Vector3.UnitY * 2f + Vector3.UnitX * linearStep3 * 100f;
        this.TetraMesh.Effect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(cameraPosition2, cameraPosition2 + Vector3.UnitX + Vector3.UnitY * MathHelper.Lerp(-1f, 3f, amount6), Vector3.UnitY));
        this.StencilMesh.Effect.ForcedProjectionMatrix = this.TetraMesh.Effect.ForcedProjectionMatrix;
        this.StencilMesh.Effect.ForcedViewMatrix = this.TetraMesh.Effect.ForcedViewMatrix;
        if ((double) this.Time <= 12.0)
          break;
        this.ChangeState();
        break;
    }
  }

  private void ChangeState()
  {
    if (this.ActiveState == FezGrid.State.CubeRise)
    {
      this.Host.Cycle();
    }
    else
    {
      this.Time = 0.0f;
      ++this.ActiveState;
      this.Update(new GameTime());
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    this.GraphicsDevice.Clear(EndCutscene32Host.PurpleBlack);
    switch (this.ActiveState)
    {
      case FezGrid.State.Wait:
      case FezGrid.State.RotateZoom:
        this.GoMesh.Draw();
        break;
      case FezGrid.State.GridFadeAlign:
        this.TetraMesh.Draw();
        break;
      case FezGrid.State.CubeRise:
        this.TetraMesh.Draw();
        this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.CutsceneWipe));
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
        this.StencilMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.CutsceneWipe);
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
        this.CubeMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
        break;
    }
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  private enum State
  {
    Wait,
    RotateZoom,
    GridFadeAlign,
    CubeRise,
  }
}
