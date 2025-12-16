// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene64.MulticoloredSpace
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
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components.EndCutscene64;

internal class MulticoloredSpace : DrawableGameComponent
{
  private static readonly Color MainCubeColor = new Color(0.270588249f, 0.9764706f, 1f);
  private static readonly Color[] Colors = new Color[10]
  {
    new Color(20, 1, 28),
    new Color(108, 27, 44),
    new Color(225, 125, 53),
    new Color(246, 231, 108),
    new Color(155, 226, 177),
    new Color(67, 246, (int) byte.MaxValue),
    new Color(100, 154, 224 /*0xE0*/),
    new Color(214, 133, 180),
    new Color(189, 63 /*0x3F*/, 117),
    new Color(98, 21, 88)
  };
  private readonly EndCutscene64Host Host;
  private Mesh PointsMesh;
  private Mesh CubesMesh;
  private DefaultEffect CubesEffect;
  private SoundEffect sBlueZoomOut;
  private SoundEffect sProgressiveAppear;
  private SoundEffect sFadeOut;
  private bool sBluePlayed;
  private bool sProgPlayed;
  private bool sFadePlayed;
  private float preWaitTime;
  private float StepTime;
  private MulticoloredSpace.State ActiveState;

  public MulticoloredSpace(Game game, EndCutscene64Host host)
    : base(game)
  {
    this.Host = host;
    this.DrawOrder = 1000;
    this.UpdateOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.sBlueZoomOut = this.CMProvider.Get(CM.EndCutscene).Load<SoundEffect>("Sounds/Ending/Cutscene64/BlueZoomOut");
    this.sProgressiveAppear = this.CMProvider.Get(CM.EndCutscene).Load<SoundEffect>("Sounds/Ending/Cutscene64/CubesProgressiveAppear");
    this.sFadeOut = this.CMProvider.Get(CM.EndCutscene).Load<SoundEffect>("Sounds/Ending/Cutscene64/CubesFadeOut");
    this.LevelManager.ActualAmbient = new Color(0.25f, 0.25f, 0.25f);
    this.LevelManager.ActualDiffuse = Color.White;
    Random random = RandomHelper.Random;
    float y1 = 1f / (float) Math.Sqrt(6.0);
    float z1 = (float) Math.Sqrt(3.0) / 2f;
    float y2 = (float) Math.Sqrt(2.0 / 3.0);
    float z2 = (float) (1.0 / (2.0 * Math.Sqrt(3.0)));
    float x = 1f / (float) Math.Sqrt(2.0);
    VertexPositionNormalColor[] vertices = new VertexPositionNormalColor[1350000];
    int[] indices = new int[2025000];
    int num1 = 0;
    int num2 = 0;
    for (int index1 = -125; index1 < 125; ++index1)
    {
      for (int index2 = -225; index2 < 225; ++index2)
      {
        Color color = index2 != 0 || index1 != 0 ? MulticoloredSpace.Colors[random.Next(0, MulticoloredSpace.Colors.Length)] : MulticoloredSpace.MainCubeColor;
        Vector3 vector3 = new Vector3((float) (index2 * 6), (float) (index1 * 6 + (Math.Abs(index2) % 2 == 0 ? 0 : 3)), 0.0f);
        int num3 = num1;
        VertexPositionNormalColor[] positionNormalColorArray1 = vertices;
        int index3 = num1;
        int num4 = index3 + 1;
        VertexPositionNormalColor positionNormalColor1 = new VertexPositionNormalColor(new Vector3(x, -y1, -z2) + vector3, new Vector3(-1f, 0.0f, 0.0f), color);
        positionNormalColorArray1[index3] = positionNormalColor1;
        VertexPositionNormalColor[] positionNormalColorArray2 = vertices;
        int index4 = num4;
        int num5 = index4 + 1;
        VertexPositionNormalColor positionNormalColor2 = new VertexPositionNormalColor(new Vector3(x, y1, z2) + vector3, new Vector3(-1f, 0.0f, 0.0f), color);
        positionNormalColorArray2[index4] = positionNormalColor2;
        VertexPositionNormalColor[] positionNormalColorArray3 = vertices;
        int index5 = num5;
        int num6 = index5 + 1;
        VertexPositionNormalColor positionNormalColor3 = new VertexPositionNormalColor(new Vector3(0.0f, 0.0f, z1) + vector3, new Vector3(-1f, 0.0f, 0.0f), color);
        positionNormalColorArray3[index5] = positionNormalColor3;
        VertexPositionNormalColor[] positionNormalColorArray4 = vertices;
        int index6 = num6;
        int num7 = index6 + 1;
        VertexPositionNormalColor positionNormalColor4 = new VertexPositionNormalColor(new Vector3(0.0f, -y2, z2) + vector3, new Vector3(-1f, 0.0f, 0.0f), color);
        positionNormalColorArray4[index6] = positionNormalColor4;
        VertexPositionNormalColor[] positionNormalColorArray5 = vertices;
        int index7 = num7;
        int num8 = index7 + 1;
        VertexPositionNormalColor positionNormalColor5 = new VertexPositionNormalColor(new Vector3(0.0f, -y2, z2) + vector3, new Vector3(0.0f, 0.0f, -1f), color);
        positionNormalColorArray5[index7] = positionNormalColor5;
        VertexPositionNormalColor[] positionNormalColorArray6 = vertices;
        int index8 = num8;
        int num9 = index8 + 1;
        VertexPositionNormalColor positionNormalColor6 = new VertexPositionNormalColor(new Vector3(0.0f, 0.0f, z1) + vector3, new Vector3(0.0f, 0.0f, -1f), color);
        positionNormalColorArray6[index8] = positionNormalColor6;
        VertexPositionNormalColor[] positionNormalColorArray7 = vertices;
        int index9 = num9;
        int num10 = index9 + 1;
        VertexPositionNormalColor positionNormalColor7 = new VertexPositionNormalColor(new Vector3(-x, y1, z2) + vector3, new Vector3(0.0f, 0.0f, -1f), color);
        positionNormalColorArray7[index9] = positionNormalColor7;
        VertexPositionNormalColor[] positionNormalColorArray8 = vertices;
        int index10 = num10;
        int num11 = index10 + 1;
        VertexPositionNormalColor positionNormalColor8 = new VertexPositionNormalColor(new Vector3(-x, -y1, -z2) + vector3, new Vector3(0.0f, 0.0f, -1f), color);
        positionNormalColorArray8[index10] = positionNormalColor8;
        VertexPositionNormalColor[] positionNormalColorArray9 = vertices;
        int index11 = num11;
        int num12 = index11 + 1;
        VertexPositionNormalColor positionNormalColor9 = new VertexPositionNormalColor(new Vector3(0.0f, y2, -z2) + vector3, new Vector3(0.0f, 1f, 0.0f), color);
        positionNormalColorArray9[index11] = positionNormalColor9;
        VertexPositionNormalColor[] positionNormalColorArray10 = vertices;
        int index12 = num12;
        int num13 = index12 + 1;
        VertexPositionNormalColor positionNormalColor10 = new VertexPositionNormalColor(new Vector3(-x, y1, z2) + vector3, new Vector3(0.0f, 1f, 0.0f), color);
        positionNormalColorArray10[index12] = positionNormalColor10;
        VertexPositionNormalColor[] positionNormalColorArray11 = vertices;
        int index13 = num13;
        int num14 = index13 + 1;
        VertexPositionNormalColor positionNormalColor11 = new VertexPositionNormalColor(new Vector3(0.0f, 0.0f, z1) + vector3, new Vector3(0.0f, 1f, 0.0f), color);
        positionNormalColorArray11[index13] = positionNormalColor11;
        VertexPositionNormalColor[] positionNormalColorArray12 = vertices;
        int index14 = num14;
        num1 = index14 + 1;
        VertexPositionNormalColor positionNormalColor12 = new VertexPositionNormalColor(new Vector3(x, y1, z2) + vector3, new Vector3(0.0f, 1f, 0.0f), color);
        positionNormalColorArray12[index14] = positionNormalColor12;
        int[] numArray1 = indices;
        int index15 = num2;
        int num15 = index15 + 1;
        int num16 = num3;
        numArray1[index15] = num16;
        int[] numArray2 = indices;
        int index16 = num15;
        int num17 = index16 + 1;
        int num18 = 2 + num3;
        numArray2[index16] = num18;
        int[] numArray3 = indices;
        int index17 = num17;
        int num19 = index17 + 1;
        int num20 = 1 + num3;
        numArray3[index17] = num20;
        int[] numArray4 = indices;
        int index18 = num19;
        int num21 = index18 + 1;
        int num22 = num3;
        numArray4[index18] = num22;
        int[] numArray5 = indices;
        int index19 = num21;
        int num23 = index19 + 1;
        int num24 = 3 + num3;
        numArray5[index19] = num24;
        int[] numArray6 = indices;
        int index20 = num23;
        int num25 = index20 + 1;
        int num26 = 2 + num3;
        numArray6[index20] = num26;
        int[] numArray7 = indices;
        int index21 = num25;
        int num27 = index21 + 1;
        int num28 = 4 + num3;
        numArray7[index21] = num28;
        int[] numArray8 = indices;
        int index22 = num27;
        int num29 = index22 + 1;
        int num30 = 6 + num3;
        numArray8[index22] = num30;
        int[] numArray9 = indices;
        int index23 = num29;
        int num31 = index23 + 1;
        int num32 = 5 + num3;
        numArray9[index23] = num32;
        int[] numArray10 = indices;
        int index24 = num31;
        int num33 = index24 + 1;
        int num34 = 4 + num3;
        numArray10[index24] = num34;
        int[] numArray11 = indices;
        int index25 = num33;
        int num35 = index25 + 1;
        int num36 = 7 + num3;
        numArray11[index25] = num36;
        int[] numArray12 = indices;
        int index26 = num35;
        int num37 = index26 + 1;
        int num38 = 6 + num3;
        numArray12[index26] = num38;
        int[] numArray13 = indices;
        int index27 = num37;
        int num39 = index27 + 1;
        int num40 = 8 + num3;
        numArray13[index27] = num40;
        int[] numArray14 = indices;
        int index28 = num39;
        int num41 = index28 + 1;
        int num42 = 10 + num3;
        numArray14[index28] = num42;
        int[] numArray15 = indices;
        int index29 = num41;
        int num43 = index29 + 1;
        int num44 = 9 + num3;
        numArray15[index29] = num44;
        int[] numArray16 = indices;
        int index30 = num43;
        int num45 = index30 + 1;
        int num46 = 8 + num3;
        numArray16[index30] = num46;
        int[] numArray17 = indices;
        int index31 = num45;
        int num47 = index31 + 1;
        int num48 = 11 + num3;
        numArray17[index31] = num48;
        int[] numArray18 = indices;
        int index32 = num47;
        num2 = index32 + 1;
        int num49 = 10 + num3;
        numArray18[index32] = num49;
      }
    }
    this.CubesMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true
    };
    Group group = this.CubesMesh.AddGroup();
    BufferedIndexedPrimitives<VertexPositionNormalColor> indexedPrimitives1 = new BufferedIndexedPrimitives<VertexPositionNormalColor>(vertices, indices, PrimitiveType.TriangleList);
    indexedPrimitives1.UpdateBuffers();
    indexedPrimitives1.CleanUp();
    BufferedIndexedPrimitives<VertexPositionNormalColor> indexedPrimitives2 = indexedPrimitives1;
    group.Geometry = (IIndexedPrimitiveCollection) indexedPrimitives2;
    this.PointsMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true
    };
    Color[] colors = new Color[32640];
    Vector3[] points = new Vector3[32640];
    int index33 = 0;
    for (int index34 = -68; index34 < 68; ++index34)
    {
      for (int index35 = -120; index35 < 120; ++index35)
      {
        points[index33] = new Vector3((float) index35 / 8f, (float) ((double) index34 / 8.0 + (Math.Abs(index35) % 2 == 0 ? 0.0 : 1.0 / 16.0)), 0.0f);
        colors[index33++] = RandomHelper.InList<Color>(MulticoloredSpace.Colors);
      }
    }
    this.PointsMesh.AddPoints((IList<Color>) colors, (IEnumerable<Vector3>) points, true);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.CubesMesh.Effect = (BaseEffect) (this.CubesEffect = (DefaultEffect) new DefaultEffect.LitVertexColored());
      this.PointsMesh.Effect = (BaseEffect) new PointsFromLinesEffect();
    }));
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.CubesMesh.Dispose();
    this.PointsMesh.Dispose();
  }

  private void Reset()
  {
    this.CameraManager.Center = Vector3.Zero;
    this.CameraManager.Direction = Vector3.UnitZ;
    this.CameraManager.Radius = 1.25f;
    this.CameraManager.SnapInterpolation();
    this.PointsMesh.Scale = Vector3.One;
    this.sBluePlayed = this.sProgPlayed = this.sFadePlayed = false;
    this.preWaitTime = 0.0f;
    this.StepTime = 0.0f;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    if ((double) totalSeconds == 0.0 || (double) this.StepTime == 0.0)
      this.Reset();
    this.GameState.SkipRendering = true;
    if ((double) this.preWaitTime > 2.0)
    {
      this.StepTime += totalSeconds;
      this.CameraManager.Radius *= 1.005f;
      this.PointsMesh.Scale *= 1.00125f;
    }
    else
    {
      this.StepTime = 1f / 1000f;
      this.preWaitTime += totalSeconds;
    }
    this.CameraManager.SnapInterpolation();
    this.GameState.SkipRendering = false;
    this.CubesEffect.Emissive = 1f - Easing.EaseInOut((double) FezMath.Saturate(this.StepTime / 10f), EasingType.Quadratic);
    this.CubesMesh.Material.Opacity = 1f - Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.StepTime - 23.0) / 3.0)), EasingType.Sine);
    this.PointsMesh.Material.Opacity = 1f - Easing.EaseIn((double) FezMath.Saturate((float) (((double) this.StepTime - 5.0) / 10.0)), EasingType.Sine);
    if (!this.sBluePlayed && (double) this.StepTime > 0.25)
    {
      this.sBlueZoomOut.Emit();
      this.sBluePlayed = true;
    }
    if (!this.sProgPlayed && (double) this.StepTime > 7.5)
    {
      this.sProgressiveAppear.Emit();
      this.sProgPlayed = true;
    }
    if (!this.sFadePlayed && (double) this.StepTime > 24.0)
    {
      this.sFadeOut.Emit();
      this.sFadePlayed = true;
    }
    if ((double) this.StepTime <= 26.0)
      return;
    this.ChangeState();
  }

  private void ChangeState()
  {
    if (this.ActiveState == MulticoloredSpace.State.Zooming)
    {
      this.Host.Cycle();
    }
    else
    {
      this.StepTime = 0.0f;
      ++this.ActiveState;
      this.Update(new GameTime());
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    this.GraphicsDevice.Clear(EndCutscene64Host.PurpleBlack);
    if ((double) this.PointsMesh.Material.Opacity > 0.0)
      this.PointsMesh.Draw();
    this.CubesMesh.Draw();
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private enum State
  {
    Zooming,
  }
}
