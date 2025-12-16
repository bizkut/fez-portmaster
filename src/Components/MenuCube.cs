// Decompiled with JetBrains decompiler
// Type: FezGame.Components.MenuCube
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
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

internal class MenuCube : DrawableGameComponent
{
  private Viewpoint OriginalViewpoint;
  private Vector3 OriginalCenter;
  private Quaternion OriginalRotation;
  private float OriginalPixPerTrix;
  private Vector3 OriginalDirection;
  private ArtObjectInstance AoInstance;
  private ArtObjectInstance ZoomedArtifact;
  private List<bool> AoVisibility;
  private Mesh GoldenCubes;
  private Mesh Maps;
  private Mesh AntiCubes;
  private Mesh HidingPlanes;
  private Mesh Highlights;
  private Mesh TomePages;
  private RenderTargetHandle InRtHandle;
  private RenderTargetHandle OutRtHandle;
  private FastBlurEffect BlurEffect;
  private bool Resolved;
  private bool ScheduleExit;
  private bool TomeZoom;
  private bool TomeOpen;
  private bool NumberZoom;
  private bool LetterZoom;
  private int TomePageIndex;
  private SoundEffect enterSound;
  private SoundEffect exitSound;
  private SoundEffect zoomInSound;
  private SoundEffect zoomOutSound;
  private SoundEffect rotateLeftSound;
  private SoundEffect rotateRightSound;
  private SoundEffect cursorSound;
  private SoundEffect sBackground;
  private SoundEmitter eBackground;
  private Texture2D oldTextureCache;
  private int Turns;
  private MenuCubeFace Face;
  private MenuCubeFace LastFace;
  private readonly Dictionary<MenuCubeFace, Vector2> HighlightPosition = new Dictionary<MenuCubeFace, Vector2>((IEqualityComparer<MenuCubeFace>) MenuCubeFaceComparer.Default)
  {
    {
      MenuCubeFace.Maps,
      new Vector2(0.0f, 0.0f)
    },
    {
      MenuCubeFace.Artifacts,
      new Vector2(0.0f, 0.0f)
    }
  };
  private readonly List<ArtObjectInstance> ArtifactAOs = new List<ArtObjectInstance>();
  private ArtObjectInstance TomeCoverAo;
  private ArtObjectInstance TomeBackAo;
  private bool wasLowPass;
  public static MenuCube Instance;
  private static readonly CodeInput[] LetterCode = new CodeInput[8]
  {
    CodeInput.SpinLeft,
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinLeft,
    CodeInput.SpinRight,
    CodeInput.SpinLeft,
    CodeInput.SpinLeft,
    CodeInput.SpinLeft
  };
  private static readonly CodeInput[] NumberCode = new CodeInput[8]
  {
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinLeft,
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinLeft,
    CodeInput.SpinLeft
  };
  private bool letterCodeDone;
  private bool numberCodeDone;
  private readonly List<CodeInput> codeInputs = new List<CodeInput>();
  private bool zoomed;
  private bool zooming;
  private MenuCubeFace zoomedFace;
  private Vector3 originalObjectPosition;
  private readonly Vector3[] originalMapPositions = new Vector3[6];
  private IWaiter tomeOpenWaiter;
  private Group[] mystery2Groups = new Group[3];
  private Texture2D mystery2Xbox;
  private Texture2D mystery2Sony;

  public MenuCube(Game game)
    : base(game)
  {
    this.UpdateOrder = -9;
    this.DrawOrder = 1000;
    MenuCube.Instance = this;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.GameState.MenuCubeIsZoomed = false;
    this.PlayerManager.CanControl = false;
    ArtObject artObject = this.CMProvider.Global.Load<ArtObject>("Art Objects/MENU_CUBEAO");
    bool flag = true;
    if (this.LevelManager.WaterType == LiquidType.Sewer)
    {
      this.oldTextureCache = artObject.Cubemap;
      artObject.Cubemap = this.CMProvider.Global.Load<Texture2D>("Art Objects/MENU_CUBE_GB");
    }
    else if (this.LevelManager.WaterType == LiquidType.Lava)
    {
      this.oldTextureCache = artObject.Cubemap;
      artObject.Cubemap = this.CMProvider.Global.Load<Texture2D>("Art Objects/MENU_CUBE_VIRTUAL");
    }
    else if (this.LevelManager.BlinkingAlpha)
    {
      this.oldTextureCache = artObject.Cubemap;
      artObject.Cubemap = this.CMProvider.Global.Load<Texture2D>("Art Objects/MENU_CUBE_CMY");
    }
    else
      flag = false;
    if (flag)
      new ArtObjectMaterializer(artObject).RecomputeTexCoords(false);
    int key = IdentifierPool.FirstAvailable<ArtObjectInstance>(this.LevelManager.ArtObjects);
    this.AoInstance = new ArtObjectInstance(artObject)
    {
      Id = key,
      Position = this.PlayerManager.Center
    };
    this.AoInstance.Initialize();
    this.AoInstance.Material = new Material();
    this.LevelManager.ArtObjects.Add(key, this.AoInstance);
    this.AoInstance.Scale = new Vector3(0.0f);
    this.OriginalViewpoint = this.CameraManager.Viewpoint;
    this.OriginalCenter = this.CameraManager.Center;
    this.OriginalPixPerTrix = this.CameraManager.PixelsPerTrixel;
    this.OriginalRotation = FezMath.QuaternionFromPhi(this.CameraManager.Viewpoint.ToPhi());
    RenderTarget2D renderTarget = this.GraphicsDevice.GetRenderTargets().Length == 0 ? (RenderTarget2D) null : this.GraphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D;
    this.FillInPlanes();
    this.CreateGoldenCubeFace();
    this.CreateMapsFace();
    this.CreateArtifactsFace();
    this.CreateAntiCubeFace();
    this.CreateHighlights();
    this.CreateTomePages();
    this.GraphicsDevice.SetRenderTarget(renderTarget);
    this.AntiCubes.Position = this.Maps.Position = this.HidingPlanes.Position = this.GoldenCubes.Position = this.AoInstance.Position;
    Mesh antiCubes = this.AntiCubes;
    Mesh maps = this.Maps;
    Mesh hidingPlanes = this.HidingPlanes;
    Mesh goldenCubes = this.GoldenCubes;
    ArtObjectInstance aoInstance = this.AoInstance;
    Vector3 vector3_1 = new Vector3(0.0f);
    Vector3 vector3_2 = vector3_1;
    aoInstance.Scale = vector3_2;
    Vector3 vector3_3;
    Vector3 vector3_4 = vector3_3 = vector3_1;
    goldenCubes.Scale = vector3_3;
    Vector3 vector3_5;
    Vector3 vector3_6 = vector3_5 = vector3_4;
    hidingPlanes.Scale = vector3_5;
    Vector3 vector3_7;
    Vector3 vector3_8 = vector3_7 = vector3_6;
    maps.Scale = vector3_7;
    Vector3 vector3_9 = vector3_8;
    antiCubes.Scale = vector3_9;
    this.TransformArtifacts();
    this.BlurEffect = new FastBlurEffect();
    this.enterSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/EnterMenucubeOrMap");
    this.exitSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/ExitMenucubeOrMap");
    this.zoomInSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/ZoomIn");
    this.zoomOutSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/ZoomOut");
    this.rotateLeftSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateLeft");
    this.rotateRightSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateRight");
    this.cursorSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/MoveCursorMenucube");
    this.sBackground = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/MenuCubeBackground");
    this.eBackground = this.sBackground.Emit(true);
    this.enterSound.Emit();
    this.AoVisibility = new List<bool>();
    this.AoInstance.Hidden = true;
    this.AoInstance.Visible = false;
    this.GameService.CloseScroll((string) null);
    this.GameState.ShowScroll(this.Face.GetTitle(), 0.0f, true);
    this.wasLowPass = this.SoundManager.IsLowPass;
    if (!this.wasLowPass)
      this.SoundManager.FadeFrequencies(true);
    this.InRtHandle = this.TargetRenderingManager.TakeTarget();
    this.OutRtHandle = this.TargetRenderingManager.TakeTarget();
    this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.InRtHandle.Target);
  }

  private void CreateTomePages()
  {
    this.TomePages = new Mesh()
    {
      Effect = (BaseEffect) new DefaultEffect.LitTextured(),
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/PAGES/tome_pages"),
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.PointClamp
    };
    Vector3 origin = new Vector3(0.0f, -0.875f, 0.0f);
    Vector3 size = new Vector3(0.875f, 0.875f, 0.0f);
    this.TomePages.AddFace(size, origin, FaceOrientation.Front, false);
    this.TomePages.AddFace(size, origin, FaceOrientation.Back, false);
    this.TomePages.CollapseWithNormalTexture<FezVertexPositionNormalTexture>();
    this.TomePages.Groups[0].Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/PAGES/blank");
    for (int index1 = 0; index1 < 2; ++index1)
    {
      for (int index2 = 0; index2 < 4; ++index2)
      {
        this.TomePages.AddFace(size, origin, FaceOrientation.Front, false).TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(0.25f, 0.0f, 0.0f, 0.0f, 0.0f, 0.25f, 0.0f, 0.0f, (float) index1 / 2f, (float) index2 / 4f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
        this.TomePages.AddFace(size, origin, FaceOrientation.Back, false).TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(0.25f, 0.0f, 0.0f, 0.0f, 0.0f, 0.25f, 0.0f, 0.0f, (float) (((double) index1 + 0.5) / 2.0), (float) index2 / 4f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
        this.TomePages.CollapseWithNormalTexture<FezVertexPositionNormalTexture>(this.TomePages.Groups.Count - 2, 2).Enabled = false;
      }
    }
  }

  private void FillInPlanes()
  {
    bool flag = true;
    Color color;
    if (this.LevelManager.WaterType == LiquidType.Sewer)
      color = new Color(32 /*0x20*/, 70, 49);
    else if (this.LevelManager.WaterType == LiquidType.Lava)
      color = Color.Black;
    else if (this.LevelManager.BlinkingAlpha)
    {
      color = Color.Black;
    }
    else
    {
      flag = false;
      color = new Color(56, 40, 95);
    }
    Mesh mesh = new Mesh();
    DefaultEffect.LitVertexColored litVertexColored = new DefaultEffect.LitVertexColored();
    litVertexColored.Fullbright = flag;
    mesh.Effect = (BaseEffect) litVertexColored;
    mesh.Material = this.AoInstance.Material;
    mesh.Blending = new BlendingMode?(BlendingMode.Alphablending);
    this.HidingPlanes = mesh;
    Vector3 size = this.AoInstance.ArtObject.Size;
    int fromGroup = 0;
    foreach (MenuCubeFace face in Util.GetValues<MenuCubeFace>())
    {
      if (face != MenuCubeFace.AntiCubes || this.GameState.SaveData.SecretCubes > 0)
      {
        Vector3 vector3_1 = face.GetForward().Abs();
        for (int index = 0; index < face.GetCount() && index < 32 /*0x20*/; ++index)
        {
          int num1 = index;
          if (num1 >= 14)
            num1 += 2;
          if (num1 >= 20)
            num1 += 2;
          Vector3 vector3_2 = (float) face.GetDepth() * face.GetForward() / 16f;
          Vector3 vector3_3 = (float) face.GetSize() * (Vector3.One - vector3_1) / 16f;
          Vector3 vector3_4 = -vector3_2 / 2f + vector3_3 * face.GetRight() / 2f;
          Vector3 vector3_5 = -vector3_2 / 2f + vector3_3 * -face.GetRight() / 2f;
          Vector3 vector3_6 = -vector3_2 / 2f + vector3_3 * Vector3.Up / 2f;
          Vector3 vector3_7 = -vector3_2 / 2f + vector3_3 * Vector3.Down / 2f;
          Vector3 vector3_8 = -face.GetForward() * (float) face.GetDepth() / 32f;
          Vector3 vector3_9 = Vector3.Up * vector3_3 / 2f;
          Vector3 vector3_10 = -face.GetForward() * (float) face.GetDepth() / 32f;
          Vector3 vector3_11 = Vector3.Up * vector3_3 / 2f;
          Vector3 vector3_12 = -face.GetRight() * vector3_3 / 2f;
          Vector3 vector3_13 = face.GetForward() * (float) face.GetDepth() / 32f;
          Vector3 vector3_14 = -face.GetRight() * vector3_3 / 2f;
          Vector3 vector3_15 = face.GetForward() * (float) face.GetDepth() / 32f;
          Group group = this.HidingPlanes.AddGroup();
          group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalColor>(new VertexPositionNormalColor[16 /*0x10*/]
          {
            new VertexPositionNormalColor(vector3_4 - vector3_8 + vector3_9, -face.GetRight(), color),
            new VertexPositionNormalColor(vector3_4 - vector3_8 - vector3_9, -face.GetRight(), color),
            new VertexPositionNormalColor(vector3_4 + vector3_8 + vector3_9, -face.GetRight(), color),
            new VertexPositionNormalColor(vector3_4 + vector3_8 - vector3_9, -face.GetRight(), color),
            new VertexPositionNormalColor(vector3_5 - vector3_10 + vector3_11, face.GetRight(), color),
            new VertexPositionNormalColor(vector3_5 - vector3_10 - vector3_11, face.GetRight(), color),
            new VertexPositionNormalColor(vector3_5 + vector3_10 + vector3_11, face.GetRight(), color),
            new VertexPositionNormalColor(vector3_5 + vector3_10 - vector3_11, face.GetRight(), color),
            new VertexPositionNormalColor(vector3_6 - vector3_12 + vector3_13, Vector3.Down, color),
            new VertexPositionNormalColor(vector3_6 - vector3_12 - vector3_13, Vector3.Down, color),
            new VertexPositionNormalColor(vector3_6 + vector3_12 + vector3_13, Vector3.Down, color),
            new VertexPositionNormalColor(vector3_6 + vector3_12 - vector3_13, Vector3.Down, color),
            new VertexPositionNormalColor(vector3_7 - vector3_14 + vector3_15, Vector3.Up, color),
            new VertexPositionNormalColor(vector3_7 - vector3_14 - vector3_15, Vector3.Up, color),
            new VertexPositionNormalColor(vector3_7 + vector3_14 + vector3_15, Vector3.Up, color),
            new VertexPositionNormalColor(vector3_7 + vector3_14 - vector3_15, Vector3.Up, color)
          }, new int[24]
          {
            0,
            1,
            2,
            2,
            1,
            3,
            4,
            6,
            5,
            5,
            6,
            7,
            8,
            9,
            10,
            10,
            9,
            11,
            13,
            12,
            14,
            13,
            14,
            15
          }, PrimitiveType.TriangleList);
          int num2 = (int) Math.Sqrt((double) face.GetCount());
          int num3 = num1 % num2;
          int num4 = num1 / num2;
          int spacing = face.GetSpacing();
          Vector3 vector3_16 = (float) (num3 * spacing) / 16f * face.GetRight() + (float) (num4 * face.GetSpacing()) / 16f * -Vector3.UnitY + size / 2f * (face.GetForward() + Vector3.Up - face.GetRight()) + face.GetForward() * -8f / 16f + (Vector3.Down + face.GetRight()) * (float) face.GetOffset() / 16f;
          group.Position = vector3_16;
        }
        this.HidingPlanes.CollapseToBufferWithNormal<VertexPositionNormalColor>(fromGroup, this.HidingPlanes.Groups.Count - fromGroup).CustomData = (object) face;
        ++fromGroup;
      }
    }
  }

  private void CreateGoldenCubeFace()
  {
    Vector3 size1 = this.AoInstance.ArtObject.Size;
    Trile trile = this.LevelManager.ActorTriles(ActorType.CubeShard).FirstOrDefault<Trile>();
    bool flag = this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.BlinkingAlpha;
    this.GoldenCubes = new Mesh()
    {
      Effect = flag ? (BaseEffect) new DefaultEffect.Textured() : (BaseEffect) new DefaultEffect.LitTextured(),
      Texture = this.LevelMaterializer.TrilesMesh.Texture,
      Blending = new BlendingMode?(BlendingMode.Opaque),
      Material = this.AoInstance.Material
    };
    if (trile == null)
      return;
    ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry = trile.Geometry;
    int offset = MenuCubeFace.CubeShards.GetOffset();
    int spacing = MenuCubeFace.CubeShards.GetSpacing();
    for (int index = 0; index < this.GameState.SaveData.CubeShards; ++index)
    {
      int num1 = index;
      if (num1 >= 14)
        num1 += 2;
      if (num1 >= 20)
        num1 += 2;
      Group group = this.GoldenCubes.AddGroup();
      group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) geometry.Vertices).ToArray<VertexPositionNormalTextureInstance>(), geometry.Indices, geometry.PrimitiveType);
      int num2 = num1 % 6;
      int num3 = num1 / 6;
      group.Position = size1 / 2f * (Vector3.UnitZ + Vector3.UnitY - Vector3.UnitX) + (float) (offset + num2 * spacing) / 16f * Vector3.UnitX + (float) (offset + num3 * spacing) / 16f * -Vector3.UnitY + 0.5f * -Vector3.UnitZ;
      group.Scale = new Vector3(0.5f);
    }
    Group cubesGroup = this.GoldenCubes.CollapseToBufferWithNormal<VertexPositionNormalTextureInstance>();
    this.GoldenCubes.CustomRenderingHandler = (Mesh.RenderingHandler) ((m, e) =>
    {
      foreach (Group group in m.Groups)
      {
        (e as DefaultEffect).AlphaIsEmissive = group == cubesGroup;
        group.Draw(e);
      }
    });
    Group group1 = this.GoldenCubes.AddFace(new Vector3(0.5f, 0.5f, 1f), Vector3.Zero, FaceOrientation.Front, true);
    group1.Position = size1 / 2f * (Vector3.UnitZ + Vector3.UnitY) + 21f / 16f * -Vector3.UnitY + 3f / 16f * -Vector3.UnitX + 0.499f * -Vector3.UnitZ;
    group1.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/hud/tiny_key");
    group1.Blending = new BlendingMode?(BlendingMode.Alphablending);
    string text1 = this.GameState.SaveData.Keys.ToString();
    Vector2 vector2_1 = this.FontManager.Small.MeasureString(text1);
    Vector3 size2 = new Vector3(vector2_1 / 16f / 2f - 1f / 16f * Vector2.One, 1f);
    if (Culture.IsCJK)
      size2 /= 3f;
    Group group2 = this.GoldenCubes.AddFace(size2, Vector3.Zero, FaceOrientation.Front, true);
    group2.Position = size1 / 2f * (Vector3.UnitZ + Vector3.UnitY) + 1.25f * -Vector3.UnitY + 7f / 32f * Vector3.UnitX + 0.499f * -Vector3.UnitZ;
    group2.Blending = new BlendingMode?(BlendingMode.Alphablending);
    if (Culture.IsCJK)
      group2.SamplerState = SamplerState.AnisotropicClamp;
    RenderTarget2D renderTarget1 = new RenderTarget2D(this.GraphicsDevice, (int) vector2_1.X, (int) vector2_1.Y, false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, this.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
    using (SpriteBatch spriteBatch = new SpriteBatch(this.GraphicsDevice))
    {
      this.GraphicsDevice.SetRenderTarget(renderTarget1);
      this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
      spriteBatch.BeginPoint();
      float num = Culture.IsCJK ? this.FontManager.TopSpacing * 2f : this.FontManager.TopSpacing;
      spriteBatch.DrawString(this.FontManager.Small, text1, Vector2.Zero, Color.White, 0.0f, new Vector2(0.0f, (float) (-(double) num * 4.0 / 5.0)), 1f, SpriteEffects.None, 0.0f);
      spriteBatch.End();
      this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
      group2.Texture = (Texture) renderTarget1;
    }
    string text2 = this.GameState.SaveData.CubeShards.ToString();
    int scale = 2;
    Vector2 vector2_2 = this.FontManager.Small.MeasureString(text2) * (float) scale;
    size2 = new Vector3(vector2_2 / 16f / 2f - 1f / 16f * Vector2.One, 1f);
    if (Culture.IsCJK)
      size2 /= 3.25f;
    Group group3 = this.GoldenCubes.AddFace(size2, Vector3.Zero, FaceOrientation.Front, true);
    group3.Position = size1 / 2f * Vector3.UnitZ + 0.499f * -Vector3.UnitZ;
    group3.Blending = new BlendingMode?(BlendingMode.Alphablending);
    if (Culture.IsCJK)
      group3.SamplerState = SamplerState.AnisotropicClamp;
    RenderTarget2D renderTarget2 = new RenderTarget2D(this.GraphicsDevice, (int) Math.Ceiling((double) vector2_2.X), (int) Math.Ceiling((double) vector2_2.Y), false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, this.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
    using (SpriteBatch spriteBatch = new SpriteBatch(this.GraphicsDevice))
    {
      this.GraphicsDevice.SetRenderTarget(renderTarget2);
      this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
      spriteBatch.BeginPoint();
      spriteBatch.DrawString(this.FontManager.Small, text2, Vector2.Zero, Color.White, 0.0f, new Vector2(0.0f, (float) (-(double) this.FontManager.TopSpacing * 4.0 / 5.0)), (float) scale, SpriteEffects.None, 0.0f);
      spriteBatch.End();
      this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
      group3.Texture = (Texture) renderTarget2;
    }
  }

  private void CreateMapsFace()
  {
    bool flag = this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.BlinkingAlpha;
    Mesh mesh = new Mesh();
    DefaultEffect.LitTextured litTextured = new DefaultEffect.LitTextured();
    litTextured.Fullbright = flag;
    mesh.Effect = (BaseEffect) litTextured;
    mesh.Blending = new BlendingMode?(BlendingMode.Alphablending);
    mesh.Material = this.AoInstance.Material;
    mesh.SamplerState = SamplerState.PointClamp;
    this.Maps = mesh;
    Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.Up, -1.57079637f);
    int num1 = 0;
    foreach (string map in this.GameState.SaveData.Maps)
    {
      Texture2D texture2D1 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{map}_1");
      Texture2D texture2D2 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{map}_2");
      int num2 = (int) Math.Sqrt((double) MenuCubeFace.Maps.GetCount());
      Vector2 vector2 = new Vector2((float) (num1 % num2), (float) (num1 / num2));
      Vector3 size = this.AoInstance.ArtObject.Size;
      Vector3 vector3 = (float) ((double) vector2.X * (double) MenuCubeFace.Maps.GetSpacing() / 16.0) * MenuCubeFace.Maps.GetRight() + (float) ((double) vector2.Y * (double) MenuCubeFace.Maps.GetSpacing() / 16.0) * -Vector3.UnitY + size / 2f * (MenuCubeFace.Maps.GetForward() + Vector3.Up - MenuCubeFace.Maps.GetRight()) + -MenuCubeFace.Maps.GetForward() * (float) MenuCubeFace.Maps.GetDepth() / 16f / 2f + (Vector3.Down + MenuCubeFace.Maps.GetRight()) * (float) MenuCubeFace.Maps.GetOffset() / 16f;
      Group group1 = this.Maps.AddGroup();
      group1.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionNormalTexture>(new FezVertexPositionNormalTexture[4]
      {
        new FezVertexPositionNormalTexture(new Vector3(-1f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(1f, 0.0f)),
        new FezVertexPositionNormalTexture(new Vector3(-1f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(1f, 1f)),
        new FezVertexPositionNormalTexture(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.625f, 0.0f)),
        new FezVertexPositionNormalTexture(new Vector3(0.0f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.625f, 1f))
      }, new int[6]{ 0, 1, 2, 2, 1, 3 }, PrimitiveType.TriangleList);
      group1.Scale = new Vector3(0.375f, 1f, 1f) * 1.5f;
      group1.Texture = (Texture) texture2D1;
      group1.Position = vector3 + MenuCubeFace.Maps.GetRight() * 0.125f * 1.5f;
      group1.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 0.3926991f) * fromAxisAngle;
      Group group2 = this.Maps.CloneGroup(group1);
      group2.InvertNormals<FezVertexPositionNormalTexture>();
      group2.Texture = (Texture) texture2D2;
      group2.CullMode = new CullMode?(CullMode.CullClockwiseFace);
      group2.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(-1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 1f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
      Group group3 = this.Maps.AddGroup();
      group3.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionNormalTexture>(new FezVertexPositionNormalTexture[4]
      {
        new FezVertexPositionNormalTexture(new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.625f, 0.0f)),
        new FezVertexPositionNormalTexture(new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.625f, 1f)),
        new FezVertexPositionNormalTexture(new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.375f, 0.0f)),
        new FezVertexPositionNormalTexture(new Vector3(0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.375f, 1f))
      }, new int[6]{ 0, 1, 2, 2, 1, 3 }, PrimitiveType.TriangleList);
      group3.Scale = new Vector3(0.25f, 1f, 1f) * 1.5f;
      group3.Texture = (Texture) texture2D1;
      group3.Position = vector3;
      group3.Rotation = fromAxisAngle;
      Group group4 = this.Maps.CloneGroup(group3);
      group4.InvertNormals<FezVertexPositionNormalTexture>();
      group4.Texture = (Texture) texture2D2;
      group4.CullMode = new CullMode?(CullMode.CullClockwiseFace);
      group4.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(-1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 1f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
      Group group5 = this.Maps.AddGroup();
      group5.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionNormalTexture>(new FezVertexPositionNormalTexture[4]
      {
        new FezVertexPositionNormalTexture(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.375f, 0.0f)),
        new FezVertexPositionNormalTexture(new Vector3(0.0f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.375f, 1f)),
        new FezVertexPositionNormalTexture(new Vector3(1f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.0f, 0.0f)),
        new FezVertexPositionNormalTexture(new Vector3(1f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1.5f), new Vector2(0.0f, 1f))
      }, new int[6]{ 0, 1, 2, 2, 1, 3 }, PrimitiveType.TriangleList);
      group5.Scale = new Vector3(0.375f, 1f, 1f) * 1.5f;
      group5.Texture = (Texture) texture2D1;
      group5.Position = vector3 - MenuCubeFace.Maps.GetRight() * 0.125f * 1.5f;
      group5.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 0.3926991f) * fromAxisAngle;
      Group group6 = this.Maps.CloneGroup(group5);
      group6.InvertNormals<FezVertexPositionNormalTexture>();
      group6.Texture = (Texture) texture2D2;
      group6.CullMode = new CullMode?(CullMode.CullClockwiseFace);
      group6.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(-1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 1f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
      if (map == "MAP_MYSTERY")
      {
        this.mystery2Groups[0] = this.Maps.Groups[this.Maps.Groups.Count - 5];
        this.mystery2Groups[1] = this.Maps.Groups[this.Maps.Groups.Count - 3];
        this.mystery2Groups[2] = this.Maps.Groups[this.Maps.Groups.Count - 1];
        this.mystery2Xbox = texture2D2;
        this.mystery2Sony = this.CMProvider.Global.Load<Texture2D>("Other Textures/maps/MAP_MYSTERY_2_SONY");
        GamepadState.OnLayoutChanged += new EventHandler(this.UpdateControllerTexture);
        this.UpdateControllerTexture((object) null, (EventArgs) null);
      }
      ++num1;
    }
  }

  private void CreateArtifactsFace()
  {
    foreach (ActorType artifact in this.GameState.SaveData.Artifacts)
    {
      if (artifact == ActorType.Tome)
      {
        ArtObject artObject1 = this.CMProvider.Global.Load<ArtObject>("Art Objects/TOME_BAO");
        int key1 = IdentifierPool.FirstAvailable<ArtObjectInstance>(this.LevelManager.ArtObjects);
        this.TomeBackAo = new ArtObjectInstance(artObject1)
        {
          Id = key1,
          Position = this.PlayerManager.Center
        };
        this.TomeBackAo.Initialize();
        this.TomeBackAo.Material = this.AoInstance.Material;
        this.TomeBackAo.Hidden = true;
        this.LevelManager.ArtObjects.Add(key1, this.TomeBackAo);
        this.ArtifactAOs.Add(this.TomeBackAo);
        ArtObject artObject2 = this.CMProvider.Global.Load<ArtObject>("Art Objects/TOME_COVERAO");
        int key2 = IdentifierPool.FirstAvailable<ArtObjectInstance>(this.LevelManager.ArtObjects);
        this.TomeCoverAo = new ArtObjectInstance(artObject2)
        {
          Id = key2,
          Position = this.PlayerManager.Center
        };
        this.TomeCoverAo.Initialize();
        this.TomeCoverAo.Material = this.AoInstance.Material;
        this.TomeCoverAo.Hidden = true;
        this.LevelManager.ArtObjects.Add(key2, this.TomeCoverAo);
        this.ArtifactAOs.Add(this.TomeCoverAo);
      }
      else
      {
        ArtObject artObject = this.CMProvider.Global.Load<ArtObject>("Art Objects/" + artifact.GetArtObjectName());
        int key = IdentifierPool.FirstAvailable<ArtObjectInstance>(this.LevelManager.ArtObjects);
        ArtObjectInstance artObjectInstance = new ArtObjectInstance(artObject)
        {
          Id = key,
          Position = this.PlayerManager.Center
        };
        artObjectInstance.Initialize();
        artObjectInstance.Material = this.AoInstance.Material;
        artObjectInstance.Hidden = true;
        this.LevelManager.ArtObjects.Add(key, artObjectInstance);
        this.ArtifactAOs.Add(artObjectInstance);
      }
    }
  }

  private void CreateAntiCubeFace()
  {
    Vector3 size = this.AoInstance.ArtObject.Size;
    Vector3 forward = MenuCubeFace.AntiCubes.GetForward();
    Vector3 right = MenuCubeFace.AntiCubes.GetRight();
    int offset = MenuCubeFace.AntiCubes.GetOffset();
    int spacing = MenuCubeFace.AntiCubes.GetSpacing();
    bool flag = this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.BlinkingAlpha;
    if (this.GameState.SaveData.SecretCubes == 0)
    {
      this.AntiCubes = new Mesh()
      {
        Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/MENU_CUBE_COVER"),
        Blending = new BlendingMode?(BlendingMode.Alphablending),
        Material = this.AoInstance.Material,
        SamplerState = SamplerState.PointClamp
      };
      this.AntiCubes.Texture = this.LevelManager.WaterType != LiquidType.Sewer ? (this.LevelManager.WaterType != LiquidType.Lava ? (!this.LevelManager.BlinkingAlpha ? (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/MENU_CUBE_COVER") : (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/MENU_CUBE_COVER_CMY")) : (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/MENU_CUBE_COVER_VIRTUAL")) : (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/MENU_CUBE_COVER_GB");
      this.AntiCubes.Effect = flag ? (BaseEffect) new DefaultEffect.Textured() : (BaseEffect) new DefaultEffect.LitTextured();
      this.AntiCubes.AddFace(size, size * forward / 2f, FezMath.OrientationFromDirection(forward), true);
    }
    else
    {
      Trile trile = this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>();
      if (trile == null)
      {
        this.AntiCubes = new Mesh()
        {
          Effect = (BaseEffect) new DefaultEffect.LitTextured(),
          Texture = this.LevelMaterializer.TrilesMesh.Texture,
          Blending = new BlendingMode?(BlendingMode.Alphablending),
          Material = this.AoInstance.Material
        };
        Logger.Log(nameof (MenuCube), "No anti-cube trile in " + this.LevelManager.TrileSet.Name);
      }
      else
      {
        ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry = trile.Geometry;
        this.AntiCubes = new Mesh()
        {
          Effect = flag ? (BaseEffect) new DefaultEffect.Textured() : (BaseEffect) new DefaultEffect.LitTextured(),
          Texture = this.LevelMaterializer.TrilesMesh.Texture,
          Blending = new BlendingMode?(BlendingMode.Opaque),
          Material = this.AoInstance.Material
        };
        for (int index = 0; index < this.GameState.SaveData.SecretCubes; ++index)
        {
          int num1 = index;
          if (num1 >= 14)
            num1 += 2;
          if (num1 >= 20)
            num1 += 2;
          Group group = this.AntiCubes.AddGroup();
          group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) geometry.Vertices).ToArray<VertexPositionNormalTextureInstance>(), geometry.Indices, geometry.PrimitiveType);
          int num2 = num1 % 6;
          int num3 = num1 / 6;
          group.Position = size / 2f * (forward + Vector3.UnitY - right) + (float) (offset + num2 * spacing) / 16f * right + (float) (offset + num3 * spacing) / 16f * -Vector3.UnitY + 0.5f * -forward;
          group.Scale = new Vector3(0.5f);
          group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f * (float) RandomHelper.Random.Next(0, 4));
        }
        Group cubesGroup = this.AntiCubes.CollapseToBufferWithNormal<VertexPositionNormalTextureInstance>();
        this.AntiCubes.CustomRenderingHandler = (Mesh.RenderingHandler) ((m, e) =>
        {
          foreach (Group group in m.Groups)
          {
            (e as DefaultEffect).AlphaIsEmissive = group == cubesGroup;
            group.Draw(e);
          }
        });
      }
      string text = this.GameState.SaveData.SecretCubes.ToString();
      int scale = 2;
      Vector2 vector2_1 = this.FontManager.Small.MeasureString(text) * (float) scale;
      Vector2 vector2_2 = vector2_1 / 16f / 2f - 1f / 16f * Vector2.One;
      if (Culture.IsCJK)
        vector2_2 /= 3.25f;
      Group group1 = this.GoldenCubes.AddFace(new Vector3(1f, vector2_2.Y, vector2_2.X), Vector3.Zero, FezMath.OrientationFromDirection(forward), true);
      group1.Position = size / 2f * forward + 0.499f * -forward;
      group1.Blending = new BlendingMode?(BlendingMode.Alphablending);
      if (Culture.IsCJK)
        group1.SamplerState = SamplerState.AnisotropicClamp;
      RenderTarget2D renderTarget = new RenderTarget2D(this.GraphicsDevice, (int) Math.Ceiling((double) vector2_1.X), (int) Math.Ceiling((double) vector2_1.Y), false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, this.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
      using (SpriteBatch spriteBatch = new SpriteBatch(this.GraphicsDevice))
      {
        this.GraphicsDevice.SetRenderTarget(renderTarget);
        this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
        spriteBatch.BeginPoint();
        spriteBatch.DrawString(this.FontManager.Small, text, Vector2.Zero, Color.White, 0.0f, new Vector2(0.0f, (float) (-(double) this.FontManager.TopSpacing * 4.0 / 5.0)), (float) scale, SpriteEffects.None, 0.0f);
        spriteBatch.End();
        this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
        group1.Texture = (Texture) renderTarget;
      }
    }
  }

  private void CreateHighlights()
  {
    this.Highlights = new Mesh()
    {
      Effect = (BaseEffect) new DefaultEffect.VertexColored(),
      Material = this.AoInstance.Material,
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    Color color = this.LevelManager.BlinkingAlpha ? Color.Yellow : Color.White;
    for (int index = 0; index < 4; ++index)
      this.Highlights.AddGroup();
    this.CreateFaceHighlights(MenuCubeFace.Maps, color);
    this.CreateFaceHighlights(MenuCubeFace.Artifacts, color);
  }

  private void CreateFaceHighlights(MenuCubeFace cf, Color color)
  {
    Vector3 vector3 = color.ToVector3();
    Vector3 size = this.AoInstance.ArtObject.Size;
    for (int index = 0; index < 4; ++index)
      this.Highlights.AddWireframeFace(new Vector3((float) ((double) cf.GetSize() * 1.25 / 16.0)) * ((Vector3.UnitY + cf.GetRight().Abs()) * (float) (0.949999988079071 - (double) index * 0.05000000074505806)), Vector3.Zero, FezMath.OrientationFromDirection(cf.GetForward()), new Color(vector3.X, vector3.Y, vector3.Z, (float) (1.0 - (double) index / 4.0)), true).Position = size / 2f * (cf.GetForward() + Vector3.Up - cf.GetRight()) + cf.GetForward() * (-7f / 16f) + (Vector3.Down + cf.GetRight()) * (float) cf.GetOffset() / 16f;
  }

  private void StartInTransition()
  {
    this.GameState.SkipRendering = true;
    this.LevelManager.SkipInvalidation = true;
    float num = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    this.GraphicsDevice.SetupViewport();
    this.CameraManager.Radius = 26.25f * num;
    this.CameraManager.ChangeViewpoint(Viewpoint.Perspective, 1.5f);
    this.GameState.SkyOpacity = 0.0f;
    Quaternion phi180 = this.OriginalRotation * Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f);
    Vector3 aoMaxPos = this.PlayerManager.Center + (this.AoInstance.ArtObject.Size * Vector3.UnitY / 2f + Vector3.UnitY);
    this.BlurEffect.BlurWidth = 0.0f;
    Waiters.Interpolate(0.75, (Action<float>) (s =>
    {
      if (!this.Enabled)
        return;
      float amount = Easing.EaseOut((double) s, EasingType.Cubic);
      this.AoInstance.Material.Opacity = amount;
      this.AoInstance.MarkDirty();
      Mesh antiCubes = this.AntiCubes;
      Mesh maps = this.Maps;
      Mesh hidingPlanes = this.HidingPlanes;
      Mesh highlights = this.Highlights;
      Mesh goldenCubes = this.GoldenCubes;
      ArtObjectInstance aoInstance = this.AoInstance;
      Vector3 vector3_1 = new Vector3(amount);
      Vector3 vector3_2 = vector3_1;
      aoInstance.Scale = vector3_2;
      Vector3 vector3_3;
      Vector3 vector3_4 = vector3_3 = vector3_1;
      goldenCubes.Scale = vector3_3;
      Vector3 vector3_5;
      Vector3 vector3_6 = vector3_5 = vector3_4;
      highlights.Scale = vector3_5;
      Vector3 vector3_7;
      Vector3 vector3_8 = vector3_7 = vector3_6;
      hidingPlanes.Scale = vector3_7;
      Vector3 vector3_9;
      Vector3 vector3_10 = vector3_9 = vector3_8;
      maps.Scale = vector3_9;
      Vector3 vector3_11 = vector3_10;
      antiCubes.Scale = vector3_11;
      this.GoldenCubes.Position = this.AntiCubes.Position = this.Maps.Position = this.HidingPlanes.Position = this.Highlights.Position = this.AoInstance.Position = this.PlayerManager.Center + amount * (this.AoInstance.ArtObject.Size * Vector3.UnitY / 2f + Vector3.UnitY);
      this.AntiCubes.Rotation = this.Maps.Rotation = this.HidingPlanes.Rotation = this.Highlights.Rotation = this.GoldenCubes.Rotation = this.AoInstance.Rotation = Quaternion.Slerp(phi180, this.OriginalRotation, amount);
      this.CameraManager.Center = Vector3.Lerp(this.PlayerManager.Center, aoMaxPos, amount);
      this.CameraManager.SnapInterpolation();
      this.BlurEffect.BlurWidth = amount;
      this.TransformArtifacts();
    }), (Action) (() => this.BlurEffect.BlurWidth = 1f));
  }

  private void StartOutTransition()
  {
    this.CameraManager.PixelsPerTrixel = this.OriginalPixPerTrix;
    this.CameraManager.Center = this.OriginalCenter;
    this.CameraManager.ChangeViewpoint(this.OriginalViewpoint == Viewpoint.None ? this.CameraManager.LastViewpoint : this.OriginalViewpoint, 0.0f);
    this.CameraManager.SnapInterpolation();
    this.GameState.SkipRendering = false;
    this.LevelManager.SkipInvalidation = false;
    this.GameState.SkyOpacity = 1f;
    foreach (ArtObjectInstance artifactAo in this.ArtifactAOs)
    {
      artifactAo.SoftDispose();
      this.LevelManager.ArtObjects.Remove(artifactAo.Id);
    }
    this.exitSound.Emit();
    this.GameService.CloseScroll((string) null);
    this.GameState.InMenuCube = false;
    this.GameState.DisallowRotation = false;
    Waiters.Interpolate(0.5, (Action<float>) (s =>
    {
      float num = 1f - Easing.EaseOut((double) s, EasingType.Cubic);
      this.AoInstance.Material.Opacity = num;
      this.BlurEffect.BlurWidth = num;
    }), (Action) (() => ServiceHelper.RemoveComponent<MenuCube>(this)));
  }

  protected override void Dispose(bool disposing)
  {
    if (this.oldTextureCache != null)
    {
      this.AoInstance.ArtObject.Cubemap = this.oldTextureCache;
      new ArtObjectMaterializer(this.AoInstance.ArtObject).RecomputeTexCoords(true);
    }
    this.LevelManager.ArtObjects.Remove(this.AoInstance.Id);
    this.AoInstance.SoftDispose();
    this.GameState.SkyOpacity = 1f;
    this.PlayerManager.CanControl = true;
    if (this.InRtHandle != null)
    {
      this.TargetRenderingManager.UnscheduleHook(this.InRtHandle.Target);
      this.TargetRenderingManager.ReturnTarget(this.InRtHandle);
    }
    this.InRtHandle = (RenderTargetHandle) null;
    if (this.OutRtHandle != null)
    {
      this.TargetRenderingManager.UnscheduleHook(this.OutRtHandle.Target);
      this.TargetRenderingManager.ReturnTarget(this.OutRtHandle);
    }
    this.OutRtHandle = (RenderTargetHandle) null;
    this.HidingPlanes.Dispose();
    this.GoldenCubes.Dispose();
    this.AntiCubes.Dispose();
    this.TomePages.Dispose();
    this.Maps.Dispose();
    this.Highlights.Dispose();
    this.BlurEffect.Dispose();
    if (this.eBackground != null && !this.eBackground.Dead)
    {
      this.eBackground.FadeOutAndDie(0.25f, false);
      this.eBackground = (SoundEmitter) null;
    }
    this.GameService.CloseScroll((string) null);
    if (!this.wasLowPass)
      this.SoundManager.FadeFrequencies(false);
    MenuCube.Instance = (MenuCube) null;
    base.Dispose(disposing);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading || this.GameState.InMap)
      return;
    if (!this.GameState.InMenuCube)
      ServiceHelper.RemoveComponent<MenuCube>(this);
    else if (!this.GameState.MenuCubeIsZoomed && !this.CameraManager.ProjectionTransition && (this.InputManager.Back == FezButtonState.Pressed || this.InputManager.CancelTalk == FezButtonState.Pressed || this.InputManager.OpenInventory == FezButtonState.Pressed))
    {
      this.ScheduleExit = true;
      this.Enabled = false;
      this.Resolved = false;
      this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.OutRtHandle.Target);
    }
    else
    {
      bool flag = this.CameraManager.Viewpoint.IsOrthographic();
      bool menuCubeIsZoomed = this.GameState.MenuCubeIsZoomed;
      if (this.InputManager.RotateRight == FezButtonState.Pressed)
      {
        if (this.TomeOpen)
        {
          if (this.TomePageIndex > 0)
          {
            if (this.TomePageIndex != 0)
              --this.TomePageIndex;
            int tpi = this.TomePageIndex;
            Waiters.Interpolate(0.25, (Action<float>) (s =>
            {
              s = Easing.EaseOut((double) FezMath.Saturate(1f - s), EasingType.Quadratic);
              this.TomePages.Groups[tpi].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) (-2.3561944961547852 * (double) s * (1.0 - (double) tpi / 8.0 * 0.05000000074505806)));
            }), (Action) (() =>
            {
              this.TomePages.Groups[tpi].Rotation = Quaternion.Identity;
              if (tpi == 8)
                return;
              this.TomePages.Groups[tpi + 1].Enabled = false;
            }));
          }
        }
        else
        {
          this.rotateRightSound.Emit();
          if (!flag && !menuCubeIsZoomed)
          {
            this.LastFace = this.Face;
            ++this.Face;
            if (this.Face > MenuCubeFace.AntiCubes)
              this.Face = MenuCubeFace.CubeShards;
            ++this.Turns;
            this.GameService.CloseScroll((string) null);
            if (this.Face != MenuCubeFace.AntiCubes || this.GameState.SaveData.SecretCubes > 0)
              this.GameState.ShowScroll(this.Face.GetTitle(), 0.0f, true);
            foreach (Group group in this.HidingPlanes.Groups)
            {
              MenuCubeFace customData = (MenuCubeFace) group.CustomData;
              group.Enabled = customData == this.Face || customData == this.LastFace;
            }
          }
          if (this.CameraManager.Viewpoint.IsOrthographic() && !menuCubeIsZoomed)
            this.OriginalRotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, -1.57079637f);
        }
      }
      else if (this.InputManager.RotateLeft == FezButtonState.Pressed)
      {
        if (this.TomeOpen)
        {
          if (this.TomePageIndex <= 8)
          {
            int tpi = this.TomePageIndex;
            if (this.TomePageIndex <= 8)
              ++this.TomePageIndex;
            Waiters.Interpolate(0.25, (Action<float>) (s =>
            {
              s = Easing.EaseOut((double) FezMath.Saturate(s), EasingType.Quadratic);
              if (tpi != 8)
                this.TomePages.Groups[tpi + 1].Enabled = true;
              this.TomePages.Groups[tpi].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) (-2.3561944961547852 * (double) s * (1.0 - (double) tpi / 8.0 * 0.05000000074505806)));
            }), (Action) (() => this.TomePages.Groups[tpi].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) (-2.3561944961547852 * (1.0 - (double) tpi / 8.0 * 0.05000000074505806)))));
          }
        }
        else
        {
          this.rotateLeftSound.Emit();
          if (!flag && !menuCubeIsZoomed)
          {
            this.LastFace = this.Face;
            --this.Face;
            if (this.Face < MenuCubeFace.CubeShards)
              this.Face = MenuCubeFace.AntiCubes;
            --this.Turns;
            this.GameService.CloseScroll((string) null);
            if (this.Face != MenuCubeFace.AntiCubes || this.GameState.SaveData.SecretCubes > 0)
              this.GameState.ShowScroll(this.Face.GetTitle(), 0.0f, true);
            foreach (Group group in this.HidingPlanes.Groups)
            {
              MenuCubeFace customData = (MenuCubeFace) group.CustomData;
              group.Enabled = customData == this.Face || customData == this.LastFace;
            }
          }
          if (this.CameraManager.Viewpoint.IsOrthographic() && !menuCubeIsZoomed)
            this.OriginalRotation *= Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f);
        }
      }
      this.UpdateHighlights((float) gameTime.TotalGameTime.TotalSeconds);
      this.AntiCubes.Rotation = this.Maps.Rotation = this.HidingPlanes.Rotation = this.Highlights.Rotation = this.AoInstance.Rotation = this.GoldenCubes.Rotation = Quaternion.Slerp(this.GoldenCubes.Rotation, this.OriginalRotation, 0.0875f);
      this.AntiCubes.Position = this.Maps.Position = this.HidingPlanes.Position = this.GoldenCubes.Position = this.AoInstance.Position;
      this.TransformArtifacts();
      this.HandleSelection();
      this.TestForTempleOfLove();
    }
  }

  private void TestForTempleOfLove()
  {
    if (this.LevelManager.Name != "TEMPLE_OF_LOVE" || this.GameState.SaveData.PiecesOfHeart < 3 || this.GameState.SaveData.HasDoneHeartReboot || this.Face != MenuCubeFace.Artifacts || !this.GameState.MenuCubeIsZoomed || !this.LetterZoom && !this.NumberZoom || this.letterCodeDone && this.numberCodeDone)
      return;
    CodeInput codeInput = CodeInput.None;
    if (this.InputManager.Jump == FezButtonState.Pressed)
      codeInput = CodeInput.Jump;
    else if (this.InputManager.RotateRight == FezButtonState.Pressed)
      codeInput = CodeInput.SpinRight;
    else if (this.InputManager.RotateLeft == FezButtonState.Pressed)
      codeInput = CodeInput.SpinLeft;
    else if (this.InputManager.Left == FezButtonState.Pressed)
      codeInput = CodeInput.Left;
    else if (this.InputManager.Right == FezButtonState.Pressed)
      codeInput = CodeInput.Right;
    else if (this.InputManager.Up == FezButtonState.Pressed)
      codeInput = CodeInput.Up;
    else if (this.InputManager.Down == FezButtonState.Pressed)
      codeInput = CodeInput.Down;
    if (codeInput == CodeInput.None)
      return;
    this.codeInputs.Add(codeInput);
    if (this.codeInputs.Count > 8)
      this.codeInputs.RemoveAt(0);
    if (!this.letterCodeDone && this.LetterZoom)
      this.letterCodeDone = PatternTester.Test((IList<CodeInput>) this.codeInputs, MenuCube.LetterCode);
    if (!this.numberCodeDone && this.NumberZoom)
      this.numberCodeDone = PatternTester.Test((IList<CodeInput>) this.codeInputs, MenuCube.NumberCode);
    if (!this.letterCodeDone || !this.numberCodeDone)
      return;
    this.GameState.SaveData.HasDoneHeartReboot = true;
    this.LevelService.ResolvePuzzleSoundOnly();
    this.zooming = true;
    this.zoomed = false;
    this.DoArtifactZoom(this.ZoomedArtifact);
    Waiters.Wait((Func<bool>) (() => !this.GameState.MenuCubeIsZoomed), (Action) (() =>
    {
      this.ScheduleExit = true;
      this.Enabled = false;
      this.Resolved = false;
      this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.OutRtHandle.Target);
    }));
  }

  private void UpdateHighlights(float elapsedSeconds)
  {
    if (this.GameState.MenuCubeIsZoomed || this.Face == MenuCubeFace.CubeShards || this.Face == MenuCubeFace.AntiCubes)
      return;
    MenuCubeFace face1 = this.Face;
    int num = (int) Math.Sqrt((double) face1.GetCount());
    if (this.InputManager.Right == FezButtonState.Pressed && (double) this.HighlightPosition[this.Face].X + 1.0 < (double) num)
      this.MoveAndRotate(face1, Vector2.UnitX);
    if (this.InputManager.Left == FezButtonState.Pressed && (double) this.HighlightPosition[this.Face].X - 1.0 >= 0.0)
      this.MoveAndRotate(face1, -Vector2.UnitX);
    if (this.InputManager.Up == FezButtonState.Pressed && (double) this.HighlightPosition[this.Face].Y - 1.0 >= 0.0)
      this.MoveAndRotate(face1, -Vector2.UnitY);
    if (this.InputManager.Down == FezButtonState.Pressed && (double) this.HighlightPosition[this.Face].Y + 1.0 < (double) num)
      this.MoveAndRotate(face1, Vector2.UnitY);
    int face2 = (int) this.Face;
    for (int index = 0; index < 4; ++index)
      this.Highlights.Groups[face2 * 4 + index].Scale = (float) (Math.Sin((double) elapsedSeconds * 5.0) * 0.10000000149011612 * (1.0 / (double) (index + 1)) + 1.0) * (Vector3.UnitY + this.Face.GetRight().Abs()) + this.Face.GetForward().Abs();
  }

  private void MoveAndRotate(MenuCubeFace cf, Vector2 diff)
  {
    Vector2 op = this.HighlightPosition[cf];
    this.HighlightPosition[cf] = (this.HighlightPosition[cf] + diff).Round();
    int sgn = Math.Sign(Vector2.Dot(diff, Vector2.One));
    Vector3 axis = (double) diff.X != 0.0 ? Vector3.Up : cf.GetRight();
    Vector3 scale = this.AoInstance.ArtObject.Size;
    this.cursorSound.Emit();
    Waiters.Interpolate(0.15, (Action<float>) (s =>
    {
      for (int index = 0; index < 4; ++index)
      {
        Group group = this.Highlights.Groups[(int) cf * 4 + index];
        Vector2 vector2 = this.HighlightPosition[cf] - diff;
        if (op != vector2)
          break;
        s = Easing.EaseOut((double) FezMath.Saturate(s), EasingType.Sine);
        group.Position = (float) (((double) vector2.X + (double) diff.X * (double) s) * (double) cf.GetSpacing() / 16.0) * cf.GetRight() + (float) (((double) vector2.Y + (double) diff.Y * (double) s) * (double) cf.GetSpacing() / 16.0) * -Vector3.UnitY + scale / 2f * (cf.GetForward() + Vector3.Up - cf.GetRight()) + cf.GetForward() * (float) ((double) cf.GetSize() / 2.0 / 16.0 * Math.Sin((double) s * 3.1415927410125732) - 7.0 / 16.0) + (Vector3.Down + cf.GetRight()) * (float) cf.GetOffset() / 16f;
        group.Rotation = Quaternion.CreateFromAxisAngle(axis, s * 3.14159274f * (float) sgn);
      }
    }), (Action) (() =>
    {
      for (int index = 0; index < 4; ++index)
        this.Highlights.Groups[(int) cf * 4 + index].Rotation = Quaternion.Identity;
    }));
  }

  private void TransformArtifacts()
  {
    if (this.GameState.MenuCubeIsZoomed)
      return;
    int num1 = 0;
    foreach (ArtObjectInstance artifactAo in this.ArtifactAOs)
    {
      int num2 = (int) Math.Sqrt((double) MenuCubeFace.Artifacts.GetCount());
      Vector2 vector2 = new Vector2((float) (num1 % num2), (float) (num1 / num2));
      Vector3 size = this.AoInstance.ArtObject.Size;
      Vector2 artifactOffset = artifactAo.ArtObject.ActorType.GetArtifactOffset();
      Vector3 vector3 = (float) (((double) vector2.X * (double) MenuCubeFace.Artifacts.GetSpacing() - (double) artifactOffset.X) / 16.0) * MenuCubeFace.Artifacts.GetRight() + (float) (((double) vector2.Y * (double) MenuCubeFace.Artifacts.GetSpacing() - (double) artifactOffset.Y) / 16.0) * -Vector3.UnitY + size / 2f * (MenuCubeFace.Artifacts.GetForward() + Vector3.Up - MenuCubeFace.Artifacts.GetRight()) + -MenuCubeFace.Artifacts.GetForward() * (float) MenuCubeFace.Artifacts.GetDepth() / 16f * 1.25f + (Vector3.Down + MenuCubeFace.Artifacts.GetRight()) * (float) MenuCubeFace.Artifacts.GetOffset() / 16f;
      artifactAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f) * this.AoInstance.Rotation;
      artifactAo.Position = this.AoInstance.Position + Vector3.Transform(Vector3.Transform(vector3, this.AoInstance.Rotation), Matrix.CreateScale(this.AoInstance.Scale));
      artifactAo.Scale = this.AoInstance.Scale;
      if (artifactAo.ArtObjectName != "TOME_BAO")
        ++num1;
    }
  }

  private void HandleSelection()
  {
    float OriginalRadius = 17.82f * ((float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale()));
    if (!this.zooming && this.Face == MenuCubeFace.CubeShards || this.Face == MenuCubeFace.AntiCubes)
      return;
    if (this.TomeZoom && this.InputManager.GrabThrow == FezButtonState.Pressed || this.TomeOpen && this.InputManager.CancelTalk == FezButtonState.Pressed)
    {
      this.TomeOpen = !this.TomeOpen;
      if (this.TomeOpen)
      {
        this.CameraManager.OriginalDirection = this.OriginalDirection;
        this.GameState.DisallowRotation = true;
      }
      else
      {
        this.GameState.DisallowRotation = false;
        for (int index = this.TomePageIndex - 1; index >= 0; --index)
        {
          int i1 = index;
          Waiters.Interpolate(0.25 + (double) (8 - index) / 8.0 * 0.15000000596046448, (Action<float>) (s =>
          {
            s = Easing.EaseOut((double) FezMath.Saturate(1f - s), EasingType.Quadratic);
            this.TomePages.Groups[i1].Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) (-2.3561944961547852 * (double) s * (1.0 - (double) i1 / 8.0 * 0.05000000074505806)));
          }), (Action) (() =>
          {
            this.TomePages.Groups[i1].Rotation = Quaternion.Identity;
            if (i1 == 8)
              return;
            this.TomePages.Groups[i1 + 1].Enabled = false;
          }));
        }
        this.TomePageIndex = 0;
      }
      IWaiter thisWaiter = (IWaiter) null;
      thisWaiter = this.tomeOpenWaiter = Waiters.Interpolate(this.TomeOpen ? 0.875 : 0.42500001192092896, (Action<float>) (s =>
      {
        if (this.tomeOpenWaiter != thisWaiter)
          return;
        if (!this.TomeOpen)
          s = 1f - s;
        s = Easing.EaseOut((double) FezMath.Saturate(s), EasingType.Quadratic);
        this.DoTomeOpen(this.TomeCoverAo, s);
      }), (Action) (() =>
      {
        if (this.tomeOpenWaiter != thisWaiter)
          return;
        this.DoTomeOpen(this.TomeCoverAo, this.TomeOpen ? 1f : 0.0f);
        this.tomeOpenWaiter = (IWaiter) null;
      }));
    }
    else
    {
      if (this.TomeOpen || this.zooming || (this.GameState.MenuCubeIsZoomed || this.InputManager.Jump != FezButtonState.Pressed) && (!this.GameState.MenuCubeIsZoomed || this.InputManager.Back != FezButtonState.Pressed && this.InputManager.CancelTalk != FezButtonState.Pressed))
        return;
      this.zoomed = !this.zoomed;
      if (this.zoomed)
      {
        this.GameService.CloseScroll((string) null);
        this.GameState.MenuCubeIsZoomed = true;
        this.zoomedFace = this.Face;
        this.OriginalDirection = this.CameraManager.OriginalDirection;
        Mesh antiCubes = this.AntiCubes;
        Mesh goldenCubes = this.GoldenCubes;
        BlendingMode? nullable1 = new BlendingMode?(BlendingMode.Alphablending);
        BlendingMode? nullable2 = nullable1;
        goldenCubes.Blending = nullable2;
        BlendingMode? nullable3 = nullable1;
        antiCubes.Blending = nullable3;
      }
      else
      {
        Mesh antiCubes = this.AntiCubes;
        Mesh goldenCubes = this.GoldenCubes;
        BlendingMode? nullable4 = new BlendingMode?(BlendingMode.Opaque);
        BlendingMode? nullable5 = nullable4;
        goldenCubes.Blending = nullable5;
        BlendingMode? nullable6 = nullable4;
        antiCubes.Blending = nullable6;
        this.CameraManager.OriginalDirection = this.OriginalDirection;
        this.GameState.ShowScroll(this.Face.GetTitle(), 0.0f, true);
        this.TomeOpen = false;
      }
      int oid = (int) ((double) this.HighlightPosition[this.zoomedFace].X + (double) this.HighlightPosition[this.zoomedFace].Y * Math.Sqrt((double) this.zoomedFace.GetCount()));
      this.zooming = true;
      switch (this.zoomedFace)
      {
        case MenuCubeFace.Maps:
          if (this.GameState.SaveData.Maps.Count <= oid)
          {
            this.zooming = false;
            this.zoomed = false;
            this.GameState.MenuCubeIsZoomed = false;
            return;
          }
          if (this.zoomed)
          {
            for (int index = 0; index < 6; ++index)
            {
              this.Maps.Groups[oid * 6 + index].Material = new Material();
              this.originalMapPositions[index] = this.Maps.Groups[oid * 6 + index].Position;
            }
          }
          Vector2 vector2 = this.HighlightPosition[this.zoomedFace];
          Vector3 middleOffset = (float) ((double) vector2.X * (double) this.zoomedFace.GetSpacing() / 16.0) * this.zoomedFace.GetRight() + (float) ((double) vector2.Y * (double) this.zoomedFace.GetSpacing() / 16.0) * Vector3.Down + (Vector3.Down + this.zoomedFace.GetRight()) * (float) this.zoomedFace.GetOffset() / 16f;
          middleOffset = this.AoInstance.ArtObject.Size / 2f * (this.zoomedFace.GetRight() + Vector3.Down) - middleOffset;
          Waiters.Interpolate(0.25, (Action<float>) (s =>
          {
            s = Easing.EaseOut((double) s, EasingType.Quadratic);
            if (!this.zoomed)
              s = 1f - s;
            Vector3 zero = Vector3.Zero;
            for (int index = 0; index < 6; ++index)
            {
              Group group = this.Maps.Groups[oid * 6 + index];
              this.AoInstance.Material.Opacity = FezMath.Saturate((float) (1.0 - (double) s * 1.5));
              this.AoInstance.MarkDirty();
              group.Position = Vector3.Lerp(this.originalMapPositions[index], this.originalMapPositions[index] + this.zoomedFace.GetForward() * 12f + middleOffset, s);
              zero += group.Position;
            }
            this.CameraManager.Center = Vector3.Lerp(this.AoInstance.Position, this.AoInstance.Position + Vector3.Transform(zero / 6f, this.AoInstance.Rotation), s);
            this.CameraManager.Radius = MathHelper.Lerp(OriginalRadius, OriginalRadius / 7f, Easing.EaseIn((double) s, EasingType.Cubic));
          }), (Action) (() =>
          {
            if (!this.zoomed)
            {
              for (int index = 0; index < 6; ++index)
                this.Maps.Groups[oid * 6 + index].Material = this.AoInstance.Material;
              this.GameState.MenuCubeIsZoomed = false;
            }
            this.zooming = false;
          }));
          break;
        case MenuCubeFace.Artifacts:
          int count = this.ArtifactAOs.Count;
          if (this.GameState.SaveData.Artifacts.Contains(ActorType.Tome))
            --count;
          if (count <= oid)
          {
            this.zooming = false;
            this.zoomed = false;
            this.GameState.MenuCubeIsZoomed = false;
            return;
          }
          for (int index = 0; index <= oid; ++index)
          {
            if (index != oid && this.ArtifactAOs[index].ArtObjectName == "TOME_BAO")
              oid++;
          }
          this.DoArtifactZoom(this.ArtifactAOs[oid]);
          if (this.ArtifactAOs[oid].ArtObjectName == "TOME_BAO")
          {
            this.DoArtifactZoom(this.ArtifactAOs[oid + 1]);
            break;
          }
          break;
      }
      if (this.zoomed)
        this.zoomInSound.Emit();
      else
        this.zoomOutSound.Emit();
    }
  }

  private void DoTomeOpen(ArtObjectInstance ao, float s)
  {
    Vector3 vector3_1 = Vector3.Transform(this.zoomedFace.GetForward(), this.AoInstance.Rotation);
    Vector3 vector3_2 = Vector3.Transform(this.zoomedFace.GetRight(), this.AoInstance.Rotation);
    Vector3 position = -vector3_1 * 13f / 16f + vector3_2 * 2f / 16f;
    Quaternion rotation;
    Vector3 translation;
    (Matrix.CreateFromQuaternion(Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f) * this.AoInstance.Rotation) * Matrix.CreateTranslation(position) * Matrix.CreateFromAxisAngle(Vector3.UnitY, -2.3561945f * s) * Matrix.CreateTranslation(this.AoInstance.Position + vector3_1 * 12f - position)).Decompose(out Vector3 _, out rotation, out translation);
    ao.Position = translation;
    ao.Rotation = rotation;
    this.TomePages.Position = translation;
  }

  private void DoArtifactZoom(ArtObjectInstance ao)
  {
    float OriginalRadius = 17.82f * ((float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale()));
    if (this.zoomed)
    {
      ao.Material = new Material();
      this.originalObjectPosition = ao.Position;
      this.TomeZoom |= ao.ArtObjectName == "TOME_BAO";
      this.NumberZoom |= ao.ArtObjectName == "NUMBER_CUBEAO";
      this.LetterZoom |= ao.ArtObjectName == "LETTER_CUBEAO";
      this.ZoomedArtifact = ao;
    }
    else
    {
      this.NumberZoom = this.LetterZoom = this.TomeZoom = false;
      this.codeInputs.Clear();
      this.ZoomedArtifact = (ArtObjectInstance) null;
    }
    Waiters.Interpolate(0.25, (Action<float>) (s =>
    {
      s = Easing.EaseOut((double) s, EasingType.Quadratic);
      if (!this.zoomed)
        s = 1f - s;
      Vector2 artifactOffset = ao.ArtObject.ActorType.GetArtifactOffset();
      this.AoInstance.Material.Opacity = FezMath.Saturate((float) (1.0 - (double) s * 1.5));
      this.AoInstance.MarkDirty();
      ao.Position = Vector3.Lerp(this.originalObjectPosition, this.AoInstance.Position + Vector3.Transform(this.zoomedFace.GetForward(), this.AoInstance.Rotation) * 12f, s);
      this.CameraManager.Center = Vector3.Lerp(this.AoInstance.Position, ao.Position - Vector3.Transform(FezMath.XZMask * artifactOffset.X / 16f, this.AoInstance.Rotation) - Vector3.UnitY * artifactOffset.Y / 16f, s);
      this.CameraManager.Radius = MathHelper.Lerp(OriginalRadius, OriginalRadius / 4.25f, Easing.EaseIn((double) s, EasingType.Quadratic));
    }), (Action) (() =>
    {
      if (!this.zoomed)
      {
        ao.Material = this.AoInstance.Material;
        this.GameState.MenuCubeIsZoomed = false;
      }
      this.zooming = false;
    }));
  }

  public override void Draw(GameTime gameTime)
  {
    if (MenuCube.Instance == null)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue);
    if (this.TargetRenderingManager.IsHooked(this.InRtHandle.Target) && !this.Resolved)
    {
      this.TargetRenderingManager.Resolve(this.InRtHandle.Target, false);
      this.Resolved = true;
      this.StartInTransition();
    }
    if (this.ScheduleExit && this.Resolved)
    {
      graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      this.TargetRenderingManager.DrawFullscreen((Texture) this.OutRtHandle.Target, new Color(1f, 1f, 1f, this.AoInstance.Material.Opacity));
    }
    else
    {
      this.AoVisibility.Clear();
      foreach (ArtObjectInstance levelArtObject in this.LevelMaterializer.LevelArtObjects)
      {
        this.AoVisibility.Add(levelArtObject.Visible);
        levelArtObject.Visible = false;
        levelArtObject.ArtObject.Group.Enabled = false;
      }
      graphicsDevice.SetBlendingMode(BlendingMode.Opaque);
      RenderTargetHandle handle = (RenderTargetHandle) null;
      if (this.GameState.StereoMode)
        handle = this.TargetRenderingManager.TakeTarget();
      RenderTarget2D renderTarget = this.GraphicsDevice.GetRenderTargets().Length == 0 ? (this.GameState.StereoMode ? handle.Target : (RenderTarget2D) null) : this.GraphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D;
      this.BlurEffect.Pass = BlurPass.Horizontal;
      RenderTargetHandle target = this.TargetRenderingManager.TakeTarget();
      this.GraphicsDevice.SetRenderTarget(target.Target);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
      this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.BlurEffect, (Texture) this.InRtHandle.Target);
      this.BlurEffect.Pass = BlurPass.Vertical;
      this.GraphicsDevice.SetRenderTarget(renderTarget);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
      this.TargetRenderingManager.DrawFullscreen((BaseEffect) this.BlurEffect, (Texture) target.Target);
      this.TargetRenderingManager.ReturnTarget(target);
      if (this.GameState.StereoMode)
      {
        this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
        this.GraphicsDevice.Clear(Color.Black);
      }
      this.GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Black, 1f, 0);
      if (this.GameState.StereoMode)
        GameLevelHost.Instance.DoStereo(this.CameraManager.Center, this.AoInstance.ArtObject.Size * 1.5f, new Action<GameTime>(this.DrawMenuCube), gameTime, (Texture) handle.Target);
      else
        this.DrawMenuCube(gameTime);
      if (this.GameState.StereoMode)
        this.TargetRenderingManager.ReturnTarget(handle);
      int num = 0;
      foreach (ArtObjectInstance levelArtObject in this.LevelMaterializer.LevelArtObjects)
      {
        levelArtObject.Visible = this.AoVisibility[num++];
        if (levelArtObject.Visible)
          levelArtObject.ArtObject.Group.Enabled = true;
      }
      if (this.TargetRenderingManager.IsHooked(this.OutRtHandle.Target) && !this.Resolved)
      {
        this.TargetRenderingManager.Resolve(this.OutRtHandle.Target, false);
        this.GraphicsDevice.Clear(Color.Black);
        this.GraphicsDevice.SetupViewport();
        this.TargetRenderingManager.DrawFullscreen((Texture) this.OutRtHandle.Target);
        this.Resolved = true;
        this.StartOutTransition();
      }
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    }
  }

  private void DrawMenuCube(GameTime gameTime)
  {
    if (this.GameState.StereoMode)
    {
      this.LevelManager.ActualDiffuse = new Color(new Vector3(0.8f));
      this.LevelManager.ActualAmbient = new Color(new Vector3(0.2f));
    }
    RasterizerCombiner rasterCombiner = this.GraphicsDevice.GetRasterCombiner();
    bool flag = (double) this.LevelManager.BaseDiffuse != 0.0;
    foreach (ArtObjectInstance artifactAo in this.ArtifactAOs)
    {
      artifactAo.Visible = true;
      artifactAo.ArtObject.Group.Enabled = true;
      if (flag)
        artifactAo.ForceShading = true;
    }
    this.AoInstance.Visible = true;
    this.AoInstance.ArtObject.Group.Enabled = true;
    if (flag)
      this.AoInstance.ForceShading = true;
    this.LevelMaterializer.ArtObjectsMesh.Draw();
    if (flag)
      this.AoInstance.ForceShading = false;
    foreach (ArtObjectInstance artifactAo in this.ArtifactAOs)
    {
      if (flag)
        artifactAo.ForceShading = false;
    }
    rasterCombiner.DepthBias = this.CameraManager.Viewpoint.IsOrthographic() ? -1E-06f : (float) (-1.0 / 1000.0 / ((double) this.CameraManager.FarPlane - (double) this.CameraManager.NearPlane));
    this.HidingPlanes.Draw();
    rasterCombiner.DepthBias = 0.0f;
    this.GoldenCubes.Draw();
    this.Highlights.Draw();
    this.Maps.Draw();
    rasterCombiner.DepthBias = this.CameraManager.Viewpoint.IsOrthographic() ? -1E-06f : (float) (-1.0 / 1000.0 / ((double) this.CameraManager.FarPlane - (double) this.CameraManager.NearPlane));
    if (this.TomeOpen || this.tomeOpenWaiter != null && this.TomeZoom)
    {
      this.TomePages.Rotation = this.TomeBackAo.Rotation;
      this.TomePages.Position = this.TomeBackAo.Position + Vector3.Transform(this.zoomedFace.GetForward(), this.AoInstance.Rotation) * 13f / 16f;
      this.TomePages.Draw();
    }
    this.AntiCubes.Draw();
    rasterCombiner.DepthBias = 0.0f;
  }

  private void UpdateControllerTexture(object sender, EventArgs e)
  {
    if (GamepadState.Layout == GamepadState.GamepadLayout.Xbox360)
    {
      foreach (Group mystery2Group in this.mystery2Groups)
        mystery2Group.Texture = (Texture) this.mystery2Xbox;
    }
    else
    {
      foreach (Group mystery2Group in this.mystery2Groups)
        mystery2Group.Texture = (Texture) this.mystery2Sony;
    }
  }

  [ServiceDependency]
  public ILevelService LevelService { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IGameService GameService { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IFontManager FontManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
