// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene32.VibratingMembrane
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components.EndCutscene32;

internal class VibratingMembrane : DrawableGameComponent
{
  private const float GridFadeAlignDuration = 12f;
  private const float PanDownDuration = 6f;
  private const float RotateDuration = 6f;
  private const float CubeRevealDuration = 10f;
  private const float ZoomCubeDuration = 12f;
  private readonly EndCutscene32Host Host;
  private Mesh LinesMesh;
  private Mesh VibratingMesh;
  private Mesh CubeMesh;
  private Mesh PointsMesh;
  private VibratingEffect VibratingEffect;
  private VibratingEffect StaticEffect;
  private float Time;
  private float GridsCameraDistance = 1f;
  private VibratingMembrane.State ActiveState;
  private Color BackgroundColor = EndCutscene32Host.PurpleBlack;

  public VibratingMembrane(Game game, EndCutscene32Host host)
    : base(game)
  {
    this.Host = host;
    this.DrawOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LinesMesh = new Mesh();
    FezVertexPositionColor[] vertices1 = new FezVertexPositionColor[5120];
    int[] indices1 = new int[5120];
    for (int index1 = 0; index1 < 1280 /*0x0500*/; ++index1)
    {
      int index2 = 2 * index1;
      indices1[index2] = index2;
      vertices1[index2] = new FezVertexPositionColor(new Vector3(-640f, 0.0f, (float) (index1 - 640)), Color.White);
      int index3 = index2 + 1;
      indices1[index3] = index3;
      vertices1[index3] = new FezVertexPositionColor(new Vector3(640f, 0.0f, (float) (index1 - 640)), Color.White);
      int index4 = index3 + 2559 /*0x09FF*/;
      indices1[index4] = index4;
      vertices1[index4] = new FezVertexPositionColor(new Vector3((float) (index1 - 640), 0.0f, -640f), Color.White);
      int index5 = index4 + 1;
      indices1[index5] = index5;
      vertices1[index5] = new FezVertexPositionColor(new Vector3((float) (index1 - 640), 0.0f, 640f), Color.White);
    }
    Group group1 = this.LinesMesh.AddGroup();
    BufferedIndexedPrimitives<FezVertexPositionColor> indexedPrimitives1 = new BufferedIndexedPrimitives<FezVertexPositionColor>(vertices1, indices1, PrimitiveType.LineList);
    indexedPrimitives1.UpdateBuffers();
    indexedPrimitives1.CleanUp();
    BufferedIndexedPrimitives<FezVertexPositionColor> indexedPrimitives2 = indexedPrimitives1;
    group1.Geometry = (IIndexedPrimitiveCollection) indexedPrimitives2;
    Random random = RandomHelper.Random;
    this.VibratingMesh = new Mesh();
    FezVertexPositionColor[] vertices2 = new FezVertexPositionColor[500000];
    int[] indices2 = new int[998000];
    int num1 = 0;
    int num2 = 0;
    for (int index6 = 0; index6 < 500; ++index6)
    {
      int[] numArray1 = indices2;
      int index7 = num2;
      int num3 = index7 + 1;
      int num4 = num1;
      numArray1[index7] = num4;
      for (int index8 = 0; index8 < 500; ++index8)
      {
        if (index8 != 0)
          indices2[num3++] = num1;
        vertices2[num1++] = new FezVertexPositionColor(new Vector3((float) (index6 - 250), 0.0f, (float) (index8 - 250)), new Color((int) (byte) random.Next(0, 256 /*0x0100*/), (int) (byte) random.Next(0, 256 /*0x0100*/), (int) (byte) random.Next(0, 256 /*0x0100*/)));
        if (index8 < 498)
          indices2[num3++] = num1;
      }
      int[] numArray2 = indices2;
      int index9 = num3;
      num2 = index9 + 1;
      int num5 = num1;
      numArray2[index9] = num5;
      for (int index10 = 0; index10 < 500; ++index10)
      {
        if (index10 != 0)
          indices2[num2++] = num1;
        vertices2[num1++] = new FezVertexPositionColor(new Vector3((float) (index10 - 250), 0.0f, (float) (index6 - 250)), new Color((int) (byte) random.Next(0, 256 /*0x0100*/), (int) (byte) random.Next(0, 256 /*0x0100*/), (int) (byte) random.Next(0, 256 /*0x0100*/)));
        if (index10 < 498)
          indices2[num2++] = num1;
      }
    }
    Group group2 = this.VibratingMesh.AddGroup();
    BufferedIndexedPrimitives<FezVertexPositionColor> indexedPrimitives3 = new BufferedIndexedPrimitives<FezVertexPositionColor>(vertices2, indices2, PrimitiveType.LineList);
    indexedPrimitives3.UpdateBuffers();
    indexedPrimitives3.CleanUp();
    BufferedIndexedPrimitives<FezVertexPositionColor> indexedPrimitives4 = indexedPrimitives3;
    group2.Geometry = (IIndexedPrimitiveCollection) indexedPrimitives4;
    this.CubeMesh = new Mesh();
    this.CubeMesh.AddColoredBox(Vector3.One, Vector3.One, Color.White, true);
    FezVertexPositionColor[] vertices3 = new FezVertexPositionColor[200000];
    int[] indices3 = new int[200000];
    for (int index = 0; index < 100000; ++index)
    {
      vertices3[index * 2] = new FezVertexPositionColor(new Vector3((float) (random.NextDouble() * 2.0 - 1.0), (float) (random.NextDouble() * 2.0 - 1.0), 0.0f), ColorEx.TransparentWhite);
      vertices3[index * 2 + 1] = new FezVertexPositionColor(vertices3[index * 2].Position, Color.White);
      indices3[index * 2] = index * 2;
      indices3[index * 2 + 1] = index * 2 + 1;
    }
    this.PointsMesh = new Mesh() { AlwaysOnTop = true };
    Group group3 = this.PointsMesh.AddGroup();
    BufferedIndexedPrimitives<FezVertexPositionColor> indexedPrimitives5 = new BufferedIndexedPrimitives<FezVertexPositionColor>(vertices3, indices3, PrimitiveType.LineList);
    indexedPrimitives5.UpdateBuffers();
    indexedPrimitives5.CleanUp();
    BufferedIndexedPrimitives<FezVertexPositionColor> indexedPrimitives6 = indexedPrimitives5;
    group3.Geometry = (IIndexedPrimitiveCollection) indexedPrimitives6;
    this.LevelManager.ActualAmbient = new Color(0.25f, 0.25f, 0.25f);
    this.LevelManager.ActualDiffuse = Color.White;
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.LinesMesh.Effect = (BaseEffect) (this.StaticEffect = new VibratingEffect());
      this.VibratingMesh.Effect = (BaseEffect) (this.VibratingEffect = new VibratingEffect());
      this.CubeMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.PointsMesh.Effect = (BaseEffect) new PointsFromLinesEffect();
    }));
    this.Reset();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.LinesMesh != null)
      this.LinesMesh.Dispose();
    if (this.VibratingMesh != null)
      this.VibratingMesh.Dispose();
    if (this.PointsMesh != null)
      this.PointsMesh.Dispose();
    if (this.CubeMesh != null)
      this.CubeMesh.Dispose();
    this.LinesMesh = this.VibratingMesh = this.PointsMesh = this.CubeMesh = (Mesh) null;
  }

  private void Reset()
  {
    this.CameraManager.Center = Vector3.Zero;
    this.CameraManager.Direction = Vector3.UnitZ;
    this.CameraManager.Radius = 10f;
    this.CameraManager.SnapInterpolation();
    DrawActionScheduler.Schedule((Action) (() => this.StaticEffect.FogDensity = 1f / 400f));
    this.LinesMesh.Material.Opacity = 0.0f;
    this.BackgroundColor = EndCutscene32Host.PurpleBlack;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.Time += totalSeconds;
    switch (this.ActiveState)
    {
      case VibratingMembrane.State.GridFadeAlign:
        if ((double) this.Time == 0.0)
          this.Reset();
        float linearStep1 = FezMath.Saturate(this.Time / 12f);
        float amount1 = FezMath.Saturate(FezMath.Saturate(linearStep1 - 0.3f) / 0.6f);
        Mesh mesh = (double) linearStep1 < 0.25 ? this.LinesMesh : this.VibratingMesh;
        mesh.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreatePerspectiveFieldOfView(MathHelper.Lerp(1.57079637f, 2.3561945f, amount1), this.CameraManager.AspectRatio, 0.1f, 2000f));
        float amount2 = Easing.EaseOut((double) linearStep1, EasingType.Quintic);
        float amount3 = Easing.EaseInOut((double) FezMath.Saturate(FezMath.Saturate(linearStep1 - 0.5f) / 0.5f), EasingType.Sine, EasingType.Sine);
        float step1 = Easing.EaseInOut((double) FezMath.Saturate(FezMath.Saturate(linearStep1 - 0.4f) / 0.6f), EasingType.Sine, EasingType.Sine);
        mesh.Material.Opacity = Easing.EaseIn((double) FezMath.Saturate(linearStep1 * 2.5f), EasingType.Sine);
        Vector3 cameraPosition1 = Vector3.Lerp(Vector3.UnitY * 360f, Vector3.UnitY * 1.5f, amount2) + Vector3.UnitX * linearStep1 * 100f;
        mesh.Effect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(cameraPosition1, Vector3.Lerp(cameraPosition1 - Vector3.UnitY, cameraPosition1 + Vector3.UnitX - Vector3.UnitY * 3f, amount3), FezMath.Slerp(new Vector3((float) Math.Sin((double) linearStep1 * 3.1415927410125732 * 0.699999988079071), 0.0f, (float) Math.Cos((double) linearStep1 * 3.1415927410125732 * 0.699999988079071)), Vector3.UnitY, step1)));
        this.VibratingEffect.FogDensity = this.StaticEffect.FogDensity = MathHelper.Lerp(1f / 400f, 0.1f, Easing.EaseIn((double) linearStep1, EasingType.Cubic));
        this.VibratingEffect.TimeStep = this.Time;
        this.VibratingEffect.Intensity = FezMath.Saturate((float) (((double) linearStep1 - 0.25) / 0.75 * 0.5));
        if ((double) this.Time <= 12.0)
          break;
        this.ChangeState();
        break;
      case VibratingMembrane.State.PanDown:
        float linearStep2 = FezMath.Saturate(this.Time / 6f);
        this.VibratingEffect.TimeStep += totalSeconds;
        this.VibratingEffect.FogDensity = 0.1f;
        this.VibratingEffect.Intensity = FezMath.Saturate((float) ((double) linearStep2 / 2.0 + 0.5));
        this.VibratingMesh.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreatePerspectiveFieldOfView(2.3561945f, this.CameraManager.AspectRatio, 0.1f, 2000f));
        Vector3 cameraPosition2 = Vector3.UnitY * 1.5f + Vector3.UnitX * linearStep2 * 50f;
        this.VibratingMesh.Effect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(cameraPosition2, cameraPosition2 + Vector3.UnitX - Vector3.UnitY * MathHelper.Lerp(3f, 0.0f, Easing.EaseInOut((double) linearStep2, EasingType.Quadratic)), Vector3.Up));
        if ((double) this.Time <= 6.0)
          break;
        this.ChangeState();
        break;
      case VibratingMembrane.State.Rotate:
        float linearStep3 = FezMath.Saturate(this.Time / 6f);
        this.VibratingEffect.TimeStep += totalSeconds;
        this.VibratingEffect.Intensity = 1f;
        float step2 = Easing.EaseInOut((double) linearStep3, EasingType.Quadratic);
        Vector3 cameraPosition3 = Vector3.UnitY * 1.5f + Vector3.UnitX * (float) ((double) linearStep3 * 50.0 + 50.0);
        this.VibratingMesh.Effect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(cameraPosition3, cameraPosition3 + Vector3.UnitX, FezMath.Slerp(Vector3.Up, -Vector3.UnitZ, step2)));
        if ((double) this.Time <= 6.0)
          break;
        this.ChangeState();
        break;
      case VibratingMembrane.State.CubeReveal:
        if ((double) this.Time == 0.0)
        {
          this.PointsMesh.Blending = new BlendingMode?(BlendingMode.Alphablending);
          Mesh pointsMesh = this.PointsMesh;
          Mesh cubeMesh = this.CubeMesh;
          Vector3 vector3_1 = new Vector3(1f / 1000f, 1f / 1000f, 1f / 1000f);
          Vector3 vector3_2 = vector3_1;
          cubeMesh.Scale = vector3_2;
          Vector3 vector3_3 = vector3_1;
          pointsMesh.Scale = vector3_3;
          this.CubeMesh.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.One, Vector3.Zero, Vector3.Up));
        }
        this.CameraManager.Center = Vector3.Zero;
        this.CameraManager.Direction = Vector3.UnitZ;
        this.CameraManager.Radius = 10f;
        this.CameraManager.SnapInterpolation();
        float linearStep4 = FezMath.Saturate(this.Time / 6f);
        this.VibratingEffect.TimeStep += totalSeconds;
        this.VibratingEffect.Intensity = MathHelper.Lerp(1f, 5f, Easing.EaseIn((double) linearStep4, EasingType.Quadratic));
        this.VibratingEffect.FogDensity = MathHelper.Lerp(0.1f, 1f, Easing.EaseIn((double) linearStep4, EasingType.Cubic));
        this.GridsCameraDistance = MathHelper.Lerp(1f, 10f, Easing.EaseIn((double) linearStep4, EasingType.Quadratic));
        this.CubeMesh.Position = new Vector3(0.0f, 0.0f, 100f);
        this.CubeMesh.Scale *= MathHelper.Lerp(1.05f, 1.01f, Easing.EaseOut((double) FezMath.Saturate(this.Time / 3f), EasingType.Quadratic));
        if ((double) linearStep4 > 0.5)
          this.CubeMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float) (1.0 / 400.0 * (((double) linearStep4 - 0.5) * 2.0))) * this.CubeMesh.Rotation;
        this.PointsMesh.Position = this.CubeMesh.Position;
        this.PointsMesh.Scale = this.CubeMesh.Scale;
        this.PointsMesh.Rotation = this.CubeMesh.Rotation;
        Vector3 cameraPosition4 = Vector3.UnitY * 1.5f * this.GridsCameraDistance + Vector3.UnitX * (float) ((double) linearStep4 * 50.0 + 100.0);
        this.VibratingMesh.Effect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(cameraPosition4, cameraPosition4 + Vector3.UnitX, -Vector3.UnitZ));
        if ((double) this.Time <= 10.0)
          break;
        this.ChangeState();
        break;
      case VibratingMembrane.State.ZoomOnCube:
        this.CubeMesh.Scale *= 1.01f;
        this.CubeMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 1f / 400f) * this.CubeMesh.Rotation;
        this.PointsMesh.Position = this.CubeMesh.Position;
        this.PointsMesh.Rotation = this.CubeMesh.Rotation;
        this.PointsMesh.Scale = this.CubeMesh.Scale;
        this.BackgroundColor = Color.Lerp(EndCutscene32Host.PurpleBlack, Color.Black, this.Time / 12f);
        if ((double) this.Time <= 12.0)
          break;
        this.ChangeState();
        break;
    }
  }

  private void ChangeState()
  {
    if (this.ActiveState == VibratingMembrane.State.ZoomOnCube)
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
    this.GraphicsDevice.Clear(this.BackgroundColor);
    switch (this.ActiveState)
    {
      case VibratingMembrane.State.GridFadeAlign:
        if ((double) FezMath.Saturate(this.Time / 12f) < 0.25)
        {
          this.LinesMesh.Draw();
          break;
        }
        this.VibratingMesh.Draw();
        break;
      case VibratingMembrane.State.PanDown:
      case VibratingMembrane.State.Rotate:
        this.VibratingMesh.Rotation = Quaternion.Identity;
        this.VibratingMesh.Draw();
        this.VibratingMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 3.14159274f);
        this.VibratingMesh.Position += new Vector3(0.0f, 3f, 0.0f);
        this.VibratingMesh.Draw();
        this.VibratingMesh.Position -= new Vector3(0.0f, 3f, 0.0f);
        break;
      case VibratingMembrane.State.CubeReveal:
        this.VibratingMesh.Rotation = Quaternion.Identity;
        this.VibratingMesh.Draw();
        this.VibratingMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, 3.14159274f);
        this.VibratingMesh.Position += new Vector3(0.0f, 3f * this.GridsCameraDistance, 0.0f);
        this.VibratingMesh.Draw();
        this.VibratingMesh.Position -= new Vector3(0.0f, 3f * this.GridsCameraDistance, 0.0f);
        this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.CutsceneWipe));
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
        this.CubeMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.CutsceneWipe);
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
        this.PointsMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
        break;
      case VibratingMembrane.State.ZoomOnCube:
        if ((double) this.CubeMesh.Scale.X < 9.0)
        {
          this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.CutsceneWipe));
          this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
          this.CubeMesh.Draw();
          this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.CutsceneWipe);
          this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
        }
        this.PointsMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
        break;
    }
  }

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
    GridFadeAlign,
    PanDown,
    Rotate,
    CubeReveal,
    ZoomOnCube,
  }
}
