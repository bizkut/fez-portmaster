// Decompiled with JetBrains decompiler
// Type: FezGame.Components.WorldMap
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

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
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class WorldMap : DrawableGameComponent
{
  private const float LinkThickness = 0.05375f;
  private static readonly float[] ZoomCycle = new float[5]
  {
    80f,
    40f,
    20f,
    10f,
    5f
  };
  private static readonly string[] DotDialogue = new string[6]
  {
    "DOT_MAP_A",
    "DOT_MAP_B",
    "DOT_MAP_C",
    "DOT_MAP_D",
    "DOT_MAP_E",
    "DOT_MAP_F"
  };
  private bool ShowAll;
  private bool AllVisited;
  private Vector3 OriginalCenter;
  private Quaternion OriginalRotation;
  private float OriginalPixPerTrix;
  private Vector3 OriginalDirection;
  private Viewpoint OriginalViewpoint;
  private ProjectedNodeEffect NodeEffect;
  private MapTree MapTree;
  private Mesh NodesMesh;
  private Mesh LinksMesh;
  private Mesh ButtonsMesh;
  private Mesh WavesMesh;
  private Mesh IconsMesh;
  private Mesh LegendMesh;
  private ShaderInstancedIndexedPrimitives<VertexPositionColorTextureInstance, Matrix> GoldenHighlightsGeometry;
  private ShaderInstancedIndexedPrimitives<VertexPositionColorTextureInstance, Matrix> WhiteHighlightsGeometry;
  private ShaderInstancedIndexedPrimitives<VertexPositionColorTextureInstance, Matrix> LinksGeometry;
  private ShaderInstancedIndexedPrimitives<VertexPositionTextureInstance, Matrix> IconsGeometry;
  private List<float> IconsTrailingOffset;
  private Matrix[] IconsOriginalInstances;
  private MapNode CurrentNode;
  private MapNode LastFocusedNode;
  private MapNode FocusNode;
  private GlyphTextRenderer GTR;
  private TimeSpan SinceStarted;
  private RenderTargetHandle FadeInRtHandle;
  private RenderTargetHandle FadeOutRtHandle;
  private SpriteBatch SpriteBatch;
  private static StarField Starfield;
  private SoundEffect sTextNext;
  private SoundEffect sRotateLeft;
  private SoundEffect sRotateRight;
  private SoundEffect sBackground;
  private SoundEffect sZoomIn;
  private SoundEffect sZoomOut;
  private SoundEffect sEnter;
  private SoundEffect sExit;
  private SoundEffect sMagnet;
  private SoundEffect sBeacon;
  private SoundEmitter eBackground;
  private Texture2D ShineTex;
  private Texture2D GrabbedCursor;
  private Texture2D CanClickCursor;
  private Texture2D ClickedCursor;
  private Texture2D PointerCursor;
  private bool CursorSelectable;
  private float SinceMouseMoved = 3f;
  private bool Resolved;
  private int ZoomLevel = WorldMap.ZoomCycle.Length / 2;
  private int DotDialogueIndex;
  private bool FinishedInTransition;
  private bool ScheduleExit;
  private string CurrentLevelName;
  private bool wasLowPass;
  public static WorldMap Instance;
  private static readonly Vector3 GoldColor = new Color((int) byte.MaxValue, 190, 36).ToVector3();
  private readonly List<WorldMap.QualifiedNode> closestNodes = new List<WorldMap.QualifiedNode>();
  private bool chosenByMouseClick;
  private bool blockViewPicking;
  private readonly List<MapNode> nextToCover = new List<MapNode>();
  private readonly List<MapNode> toCover = new List<MapNode>();
  private readonly HashSet<MapNode> hasCovered = new HashSet<MapNode>();
  private Group sewerQRGroup;
  private Group zuhouseQRGroup;
  private Texture sewerQRXbox;
  private Texture zuhouseQRXbox;
  private Texture sewerQRSony;
  private Texture zuhouseQRSony;

  public WorldMap(Game game)
    : base(game)
  {
    this.UpdateOrder = -10;
    this.DrawOrder = 1000;
    WorldMap.Instance = this;
  }

  public static void PreInitialize()
  {
    StarField component = new StarField(ServiceHelper.Game);
    component.FollowCamera = true;
    WorldMap.Starfield = component;
    ServiceHelper.AddComponent((IGameComponent) component);
  }

  public override void Initialize()
  {
    base.Initialize();
    string str = this.LevelManager.Name.Replace('\\', '/');
    this.CurrentLevelName = str.Substring(str.LastIndexOf('/') + 1);
    if (this.CurrentLevelName == "CABIN_INTERIOR_A")
      this.CurrentLevelName = "CABIN_INTERIOR_B";
    this.MapTree = this.CMProvider.Global.Load<MapTree>("MapTree").Clone();
    if (WorldMap.Starfield == null)
    {
      StarField component = new StarField(this.Game);
      component.FollowCamera = true;
      WorldMap.Starfield = component;
      ServiceHelper.AddComponent((IGameComponent) component);
    }
    this.LastFocusedNode = this.FocusNode = this.CurrentNode = this.MapTree.Root;
    this.NodeEffect = new ProjectedNodeEffect();
    this.NodesMesh = new Mesh()
    {
      Effect = (BaseEffect) this.NodeEffect,
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerStates.PointMipClamp
    };
    this.LinksMesh = new Mesh()
    {
      Effect = (BaseEffect) new InstancedMapEffect(),
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      Culling = CullMode.None
    };
    this.LinksGeometry = this.CreateLinksGroup(Color.White, false, CullMode.CullCounterClockwiseFace);
    List<Matrix> instances = new List<Matrix>();
    this.BuildNodes(this.MapTree.Root, (MapNode.Connection) null, (MapNode) null, Vector3.Zero, instances);
    this.GoldenHighlightsGeometry = this.CreateLinksGroup(new Color(WorldMap.GoldColor), true, CullMode.CullClockwiseFace);
    List<Matrix> matrixList1 = new List<Matrix>();
    this.WhiteHighlightsGeometry = this.CreateLinksGroup(Color.White, false, CullMode.CullClockwiseFace);
    List<Matrix> matrixList2 = new List<Matrix>();
    foreach (Group group in this.NodesMesh.Groups)
    {
      NodeGroupData customData = group.CustomData as NodeGroupData;
      MapNode node = customData.Node;
      Vector3 vector3 = new Vector3(node.NodeType.GetSizeFactor() + 0.125f);
      Vector3 position = customData.Node.Group.Position;
      Vector3 one = Vector3.One;
      LevelSaveData levelSaveData;
      if (this.GameState.SaveData.World.TryGetValue(this.GameState.IsTrialMode ? "trial/" + node.LevelName : node.LevelName, out levelSaveData) && levelSaveData.FilledConditions.Fullfills(node.Conditions))
      {
        customData.Complete = true;
        Vector3 goldColor = WorldMap.GoldColor;
        customData.HighlightInstance = matrixList1.Count;
        matrixList1.Add(new Matrix(position.X, position.Y, position.Z, 0.0f, goldColor.X, goldColor.Y, goldColor.Z, 1f, vector3.X, vector3.Y, vector3.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
      }
      else
      {
        customData.HighlightInstance = matrixList2.Count;
        matrixList2.Add(new Matrix(position.X, position.Y, position.Z, 0.0f, one.X, one.Y, one.Z, 1f, vector3.X, vector3.Y, vector3.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
      }
    }
    this.LinksGeometry.Instances = instances.ToArray();
    this.LinksGeometry.InstanceCount = instances.Count;
    this.LinksGeometry.UpdateBuffers();
    this.GoldenHighlightsGeometry.Instances = matrixList1.ToArray();
    this.GoldenHighlightsGeometry.InstanceCount = matrixList1.Count;
    this.GoldenHighlightsGeometry.UpdateBuffers();
    this.WhiteHighlightsGeometry.Instances = matrixList2.ToArray();
    this.WhiteHighlightsGeometry.InstanceCount = matrixList2.Count;
    this.WhiteHighlightsGeometry.UpdateBuffers();
    this.CreateIcons();
    this.PlayerManager.CanControl = false;
    this.OriginalCenter = this.CameraManager.Center;
    this.OriginalPixPerTrix = this.CameraManager.PixelsPerTrixel;
    this.OriginalViewpoint = this.CameraManager.Viewpoint;
    this.OriginalRotation = Quaternion.Identity;
    this.OriginalDirection = this.CameraManager.Direction;
    this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
    this.GameService.CloseScroll((string) null);
    Viewport viewport = this.GraphicsDevice.Viewport;
    float aspectRatio = viewport.AspectRatio;
    viewport = this.GraphicsDevice.Viewport;
    float num = (float) viewport.Height / (720f * this.GraphicsDevice.GetViewScale());
    float width = 22.5f * aspectRatio;
    float height = 22.5f;
    Mesh mesh1 = new Mesh();
    AnimatedPlaneEffect animatedPlaneEffect = new AnimatedPlaneEffect();
    animatedPlaneEffect.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(width, height, 0.1f, 100f));
    animatedPlaneEffect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up));
    mesh1.Effect = (BaseEffect) animatedPlaneEffect;
    mesh1.AlwaysOnTop = true;
    mesh1.DepthWrites = false;
    mesh1.SamplerState = SamplerState.PointClamp;
    mesh1.Position = new Vector3((float) ((double) width / 2.0 * 0.75), (float) ((double) height / 2.0 * 0.85000002384185791 - 1.0), 0.0f);
    this.ButtonsMesh = mesh1;
    Group group1 = this.ButtonsMesh.AddFace(new Vector3((float) (0.33300000429153442 + 0.66600000858306885 / (double) num), 7.375f, 1f), new Vector3(FezMath.Saturate(num - 1f) * 0.333f, -5.875f, 0.0f), FaceOrientation.Front, false);
    group1.Material = new Material()
    {
      Diffuse = new Vector3(1f / 16f)
    };
    group1.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/fullwhite");
    Mesh mesh2 = new Mesh();
    DefaultEffect.Textured textured = new DefaultEffect.Textured();
    textured.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(width, height, 0.1f, 100f));
    textured.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up));
    mesh2.Effect = (BaseEffect) textured;
    mesh2.AlwaysOnTop = true;
    mesh2.DepthWrites = false;
    mesh2.SamplerState = SamplerState.PointClamp;
    mesh2.Position = new Vector3((float) (-(double) width / 2.0 * 0.89999997615814209), (float) (-(double) height / 2.0 * 0.89999997615814209), 0.0f);
    mesh2.Blending = new BlendingMode?(BlendingMode.Alphablending);
    this.LegendMesh = mesh2;
    Group group2 = this.LegendMesh.AddFace(new Vector3((float) (0.33300000429153442 + 0.66600000858306885 / (double) num), 5.75f, 1f), new Vector3(0.0f, 0.0f, 0.0f), FaceOrientation.Front, false);
    group2.Material = new Material()
    {
      Diffuse = new Vector3(1f / 16f)
    };
    group2.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/FullWhite");
    this.LegendMesh.AddFace(new Vector3(1f, 8f, 1f), new Vector3(0.25f, -2.5f, 0.0f), FaceOrientation.Front, false).Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/map_controls/icons_legend");
    this.WavesMesh = new Mesh()
    {
      Effect = (BaseEffect) new DefaultEffect.Textured(),
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.LinearClamp,
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/map_controls/cube_outline")
    };
    for (int index = 0; index < 4; ++index)
      this.WavesMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, true).Material = new Material();
    this.GTR = new GlyphTextRenderer(this.Game);
    this.ZoomLevel = 2;
    if (!this.GameState.SaveData.HasHadMapHelp)
    {
      this.DotManager.ForceDrawOrder(this.DrawOrder + 1);
      this.SpeechBubble.ForceDrawOrder(this.DrawOrder + 1);
      this.DotManager.Behaviour = DotHost.BehaviourType.ClampToTarget;
      this.DotManager.Hidden = false;
    }
    else
      this.DotManager.Hidden = true;
    this.sTextNext = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/TextNext");
    this.sRotateLeft = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateLeft");
    this.sRotateRight = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/RotateRight");
    this.sEnter = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/EnterMenucubeOrMap");
    this.sExit = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/ExitMenucubeOrMap");
    this.sZoomIn = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/ZoomIn");
    this.sZoomOut = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/ZoomOut");
    this.sMagnet = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/WorldMapMagnet");
    this.sBeacon = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/MapBeacon");
    this.sBackground = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/MapBackground");
    this.eBackground = this.sBackground.Emit(true);
    this.sEnter.Emit();
    this.CameraManager.OriginalDirection = this.CameraManager.Direction;
    this.ShineTex = this.CMProvider.Global.Load<Texture2D>("Other Textures/map_screens/shine_rays");
    this.PointerCursor = this.CMProvider.Global.Load<Texture2D>("Other Textures/cursor/CURSOR_POINTER");
    this.CanClickCursor = this.CMProvider.Global.Load<Texture2D>("Other Textures/cursor/CURSOR_CLICKER_A");
    this.ClickedCursor = this.CMProvider.Global.Load<Texture2D>("Other Textures/cursor/CURSOR_CLICKER_B");
    this.GrabbedCursor = this.CMProvider.Global.Load<Texture2D>("Other Textures/cursor/CURSOR_GRABBER");
    this.wasLowPass = this.SoundManager.IsLowPass;
    if (!this.wasLowPass)
      this.SoundManager.FadeFrequencies(true);
    this.FadeOutRtHandle = this.TargetRenderingManager.TakeTarget();
    this.FadeInRtHandle = this.TargetRenderingManager.TakeTarget();
    this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.FadeInRtHandle.Target);
    WorldMap.Starfield.Opacity = this.LinksMesh.Material.Opacity = this.NodesMesh.Material.Opacity = this.LegendMesh.Material.Opacity = 0.0f;
  }

  private ShaderInstancedIndexedPrimitives<VertexPositionColorTextureInstance, Matrix> CreateLinksGroup(
    Color color,
    bool isComplete,
    CullMode cullMode)
  {
    Group group = this.LinksMesh.AddGroup();
    ShaderInstancedIndexedPrimitives<VertexPositionColorTextureInstance, Matrix> linksGroup = new ShaderInstancedIndexedPrimitives<VertexPositionColorTextureInstance, Matrix>(PrimitiveType.TriangleList, 58);
    group.Geometry = (IIndexedPrimitiveCollection) linksGroup;
    linksGroup.Vertices = new VertexPositionColorTextureInstance[8]
    {
      new VertexPositionColorTextureInstance(new Vector3(-1f, -1f, -1f) / 2f, color, Vector2.Zero),
      new VertexPositionColorTextureInstance(new Vector3(1f, -1f, -1f) / 2f, color, Vector2.Zero),
      new VertexPositionColorTextureInstance(new Vector3(1f, 1f, -1f) / 2f, color, Vector2.Zero),
      new VertexPositionColorTextureInstance(new Vector3(-1f, 1f, -1f) / 2f, color, Vector2.Zero),
      new VertexPositionColorTextureInstance(new Vector3(-1f, -1f, 1f) / 2f, color, Vector2.Zero),
      new VertexPositionColorTextureInstance(new Vector3(1f, -1f, 1f) / 2f, color, Vector2.Zero),
      new VertexPositionColorTextureInstance(new Vector3(1f, 1f, 1f) / 2f, color, Vector2.Zero),
      new VertexPositionColorTextureInstance(new Vector3(-1f, 1f, 1f) / 2f, color, Vector2.Zero)
    };
    linksGroup.Indices = new int[36]
    {
      0,
      1,
      2,
      0,
      2,
      3,
      1,
      5,
      6,
      1,
      6,
      2,
      0,
      7,
      4,
      0,
      3,
      7,
      3,
      2,
      6,
      3,
      6,
      7,
      4,
      6,
      5,
      4,
      7,
      6,
      0,
      5,
      1,
      0,
      4,
      5
    };
    group.CustomData = (object) isComplete;
    group.CullMode = new CullMode?(cullMode);
    return linksGroup;
  }

  private void CreateIcons()
  {
    this.IconsMesh = new Mesh()
    {
      Effect = (BaseEffect) new InstancedMapEffect()
      {
        Billboard = true
      },
      AlwaysOnTop = true,
      DepthWrites = false,
      SamplerState = SamplerState.PointClamp,
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/map_controls/icons")
    };
    Group group1 = this.IconsMesh.AddGroup();
    this.IconsGeometry = new ShaderInstancedIndexedPrimitives<VertexPositionTextureInstance, Matrix>(PrimitiveType.TriangleList, 58);
    ShaderInstancedIndexedPrimitives<VertexPositionTextureInstance, Matrix> iconsGeometry = this.IconsGeometry;
    group1.Geometry = (IIndexedPrimitiveCollection) iconsGeometry;
    this.IconsGeometry.Vertices = new VertexPositionTextureInstance[4]
    {
      new VertexPositionTextureInstance(new Vector3(-0.5f, 0.0f, 0.0f), new Vector2(0.0f, 0.0f)),
      new VertexPositionTextureInstance(new Vector3(0.5f, 0.0f, 0.0f), new Vector2(0.625f, 0.0f)),
      new VertexPositionTextureInstance(new Vector3(0.5f, -1f, 0.0f), new Vector2(0.625f, 1f)),
      new VertexPositionTextureInstance(new Vector3(-0.5f, -1f, 0.0f), new Vector2(0.0f, 1f))
    };
    this.IconsGeometry.Indices = new int[6]
    {
      0,
      1,
      3,
      3,
      1,
      2
    };
    List<Matrix> matrixList = new List<Matrix>();
    this.IconsTrailingOffset = new List<float>();
    foreach (Group group2 in this.NodesMesh.Groups)
    {
      NodeGroupData customData = (NodeGroupData) group2.CustomData;
      MapNode node = customData.Node;
      LevelSaveData levelSaveData;
      if (!this.GameState.SaveData.World.TryGetValue(this.GameState.IsTrialMode ? "trial/" + node.LevelName : node.LevelName, out levelSaveData))
      {
        if (this.ShowAll && this.AllVisited)
          levelSaveData = new LevelSaveData();
        else
          continue;
      }
      float num = 0.0f;
      Vector3 vector3 = group2.Position + node.NodeType.GetSizeFactor() / 2f * new Vector3(1f, 1f, -1f) + 0.2f * new Vector3(1f, 0.0f, -1f);
      if (node.HasWarpGate)
      {
        customData.IconInstances.Add(matrixList.Count);
        matrixList.Add(new Matrix(vector3.X, vector3.Y, vector3.Z, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.25f, 0.225f, 0.25f, 0.0f, 0.0f, 0.0f, 1f, 9f / 64f));
        this.IconsTrailingOffset.Add(num);
        num += 0.9f;
      }
      if (node.HasLesserGate)
      {
        customData.IconInstances.Add(matrixList.Count);
        matrixList.Add(new Matrix(vector3.X, vector3.Y, vector3.Z, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.25f, 0.175f, 0.25f, 0.0f, 0.0f, 9f / 64f, 1f, 7f / 64f));
        this.IconsTrailingOffset.Add(num);
        num += 0.7f;
      }
      if (customData.Node.Conditions.ChestCount > levelSaveData.FilledConditions.ChestCount)
      {
        customData.IconInstances.Add(matrixList.Count);
        matrixList.Add(new Matrix(vector3.X, vector3.Y, vector3.Z, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.25f, 0.15f, 0.25f, 0.0f, 0.0f, 0.25f, 1f, 3f / 32f));
        this.IconsTrailingOffset.Add(num);
        num += 0.6f;
      }
      if (customData.Node.Conditions.LockedDoorCount > levelSaveData.FilledConditions.LockedDoorCount)
      {
        customData.IconInstances.Add(matrixList.Count);
        matrixList.Add(new Matrix(vector3.X, vector3.Y, vector3.Z, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.25f, 0.225f, 0.25f, 0.0f, 0.0f, 11f / 32f, 1f, 9f / 64f));
        this.IconsTrailingOffset.Add(num);
        num += 0.9f;
      }
      if (customData.Node.Conditions.CubeShardCount > levelSaveData.FilledConditions.CubeShardCount)
      {
        customData.IconInstances.Add(matrixList.Count);
        matrixList.Add(new Matrix(vector3.X, vector3.Y, vector3.Z, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.25f, 0.225f, 0.25f, 0.0f, 0.0f, 31f / 64f, 1f, 9f / 64f));
        this.IconsTrailingOffset.Add(num);
        num += 0.9f;
      }
      if (customData.Node.Conditions.SplitUpCount > levelSaveData.FilledConditions.SplitUpCount)
      {
        customData.IconInstances.Add(matrixList.Count);
        matrixList.Add(new Matrix(vector3.X, vector3.Y, vector3.Z, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.25f, 0.275f, 0.25f, 0.0f, 0.0f, 0.625f, 1f, 11f / 64f));
        this.IconsTrailingOffset.Add(num);
        num += 1.1f;
      }
      if (customData.Node.Conditions.SecretCount + customData.Node.Conditions.ScriptIds.Count > levelSaveData.FilledConditions.SecretCount + levelSaveData.FilledConditions.ScriptIds.Count)
      {
        customData.IconInstances.Add(matrixList.Count);
        matrixList.Add(new Matrix(vector3.X, vector3.Y, vector3.Z, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.25f, 0.275f, 0.25f, 0.0f, 0.0f, 51f / 64f, 1f, 11f / 64f));
        this.IconsTrailingOffset.Add(num);
      }
    }
    this.IconsGeometry.Instances = matrixList.ToArray();
    this.IconsOriginalInstances = matrixList.ToArray();
    this.IconsGeometry.InstanceCount = matrixList.Count;
    this.IconsGeometry.UpdateBuffers();
  }

  protected override void Dispose(bool disposing)
  {
    this.GameState.InMap = false;
    this.GameState.SkipRendering = false;
    this.PlayerManager.CanControl = true;
    if (this.FadeOutRtHandle != null)
      this.TargetRenderingManager.ReturnTarget(this.FadeOutRtHandle);
    this.FadeOutRtHandle = (RenderTargetHandle) null;
    if (this.FadeInRtHandle != null)
      this.TargetRenderingManager.ReturnTarget(this.FadeInRtHandle);
    this.FadeInRtHandle = (RenderTargetHandle) null;
    if (this.eBackground != null && !this.eBackground.Dead)
    {
      this.eBackground.FadeOutAndDie(0.25f);
      this.eBackground = (SoundEmitter) null;
    }
    this.LinksMesh.Dispose();
    this.NodesMesh.Dispose();
    this.IconsMesh.Dispose();
    this.ButtonsMesh.Dispose();
    this.InputManager.StrictRotation = false;
    if (!this.wasLowPass)
      this.SoundManager.FadeFrequencies(false);
    WorldMap.Instance = (WorldMap) null;
    base.Dispose(disposing);
  }

  private void BuildNodes(
    MapNode node,
    MapNode.Connection parentConnection,
    MapNode parentNode,
    Vector3 offset,
    List<Matrix> instances)
  {
    Group group = (Group) null;
    bool flag = this.GameState.SaveData.World.ContainsKey(this.GameState.IsTrialMode ? "trial/" + node.LevelName : node.LevelName);
    if (((parentNode == null || !this.GameState.SaveData.World.ContainsKey(this.GameState.IsTrialMode ? "trial/" + parentNode.LevelName : parentNode.LevelName) ? (node.Connections.Any<MapNode.Connection>((Func<MapNode.Connection, bool>) (connection => this.GameState.SaveData.World.ContainsKey(this.GameState.IsTrialMode ? "trial/" + connection.Node.LevelName : connection.Node.LevelName))) ? 1 : 0) : 1) | (flag ? 1 : 0)) != 0 || this.AllVisited || this.ShowAll)
    {
      group = this.NodesMesh.AddFlatShadedBox(new Vector3(node.NodeType.GetSizeFactor()), Vector3.Zero, Color.White, true);
      group.Position = offset;
      group.CustomData = (object) new NodeGroupData()
      {
        Node = node,
        LevelName = node.LevelName
      };
      group.Material = new Material();
      node.Group = group;
    }
    if (node.LevelName == this.CurrentLevelName)
      this.LastFocusedNode = this.FocusNode = this.CurrentNode = node;
    if ((flag || this.AllVisited || this.ShowAll) && group != null)
    {
      if (MemoryContentManager.AssetExists("Other Textures/map_screens/" + node.LevelName))
        group.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/map_screens/" + node.LevelName);
      if (node.LevelName == "SEWER_QR")
      {
        this.sewerQRGroup = group;
        this.sewerQRXbox = group.Texture;
        this.sewerQRSony = (Texture) this.CMProvider.Global.Load<Texture2D>($"Other Textures/map_screens/{node.LevelName}_SONY");
        GamepadState.OnLayoutChanged += new EventHandler(this.UpdateControllerTexture);
        this.UpdateControllerTexture((object) null, (EventArgs) null);
      }
      else if (node.LevelName == "ZU_HOUSE_QR")
      {
        this.zuhouseQRGroup = group;
        this.zuhouseQRXbox = group.Texture;
        this.zuhouseQRSony = (Texture) this.CMProvider.Global.Load<Texture2D>($"Other Textures/map_screens/{node.LevelName}_SONY");
        GamepadState.OnLayoutChanged += new EventHandler(this.UpdateControllerTexture);
        this.UpdateControllerTexture((object) null, (EventArgs) null);
      }
    }
    foreach (MapNode.Connection connection in node.Connections)
    {
      MapNode.Connection c = connection;
      if (c.Node.NodeType == LevelNodeType.Lesser && node.Connections.Any<MapNode.Connection>((Func<MapNode.Connection, bool>) (x => x.Face == c.Face && c.Node.NodeType != LevelNodeType.Lesser)))
      {
        if (!node.Connections.Any<MapNode.Connection>((Func<MapNode.Connection, bool>) (x => x.Face == FaceOrientation.Top)))
          c.Face = FaceOrientation.Top;
        else if (!node.Connections.Any<MapNode.Connection>((Func<MapNode.Connection, bool>) (x => x.Face == FaceOrientation.Down)))
          c.Face = FaceOrientation.Down;
      }
    }
    foreach (MapNode.Connection connection in node.Connections)
    {
      MapNode.Connection c = connection;
      c.MultiBranchId = node.Connections.Where<MapNode.Connection>((Func<MapNode.Connection, bool>) (x => x.Face == c.Face)).Max<MapNode.Connection>((Func<MapNode.Connection, int>) (x => x.MultiBranchId)) + 1;
      c.MultiBranchCount = node.Connections.Count<MapNode.Connection>((Func<MapNode.Connection, bool>) (x => x.Face == c.Face));
    }
    float val1 = 0.0f;
    foreach (MapNode.Connection connection in (IEnumerable<MapNode.Connection>) node.Connections.OrderByDescending<MapNode.Connection, float>((Func<MapNode.Connection, float>) (x => x.Node.NodeType.GetSizeFactor())))
    {
      if (parentConnection != null && connection.Face == parentConnection.Face.GetOpposite())
        connection.Face = connection.Face.GetOpposite();
      int num1 = this.AllVisited | flag ? 1 : (this.GameState.SaveData.World.ContainsKey(this.GameState.IsTrialMode ? "trial/" + connection.Node.LevelName : connection.Node.LevelName) ? 1 : 0);
      float num2 = (float) (3.0 + ((double) node.NodeType.GetSizeFactor() + (double) connection.Node.NodeType.GetSizeFactor()) / 2.0);
      if ((node.NodeType == LevelNodeType.Hub || connection.Node.NodeType == LevelNodeType.Hub) && node.NodeType != LevelNodeType.Lesser && connection.Node.NodeType != LevelNodeType.Lesser)
        ++num2;
      if ((node.NodeType == LevelNodeType.Lesser || connection.Node.NodeType == LevelNodeType.Lesser) && connection.MultiBranchCount == 1)
        num2 -= connection.Face.IsSide() ? 1f : 2f;
      float sizeFactor = num2 * (1.25f + connection.BranchOversize);
      float num3 = sizeFactor * 0.375f;
      if (connection.Node.NodeType == LevelNodeType.Node && node.NodeType == LevelNodeType.Node)
        num3 *= 1.5f;
      Vector3 faceVector = connection.Face.AsVector();
      Vector3 vector3_1 = Vector3.Zero;
      if (connection.MultiBranchCount > 1)
        vector3_1 = ((float) (connection.MultiBranchId - 1) - (float) (connection.MultiBranchCount - 1) / 2f) * (FezMath.XZMask - connection.Face.AsVector().Abs()) * num3;
      this.BuildNodes(connection.Node, connection, node, offset + faceVector * sizeFactor + vector3_1, instances);
      if (num1 != 0)
      {
        if (connection.LinkInstances == null)
          connection.LinkInstances = new List<int>();
        if (connection.MultiBranchCount > 1)
        {
          val1 = Math.Max(val1, sizeFactor / 2f);
          Vector3 vector3_2 = faceVector * val1 + new Vector3(0.05375f);
          Vector3 vector3_3 = faceVector * val1 / 2f + offset;
          connection.LinkInstances.Add(instances.Count);
          instances.Add(new Matrix(vector3_3.X, vector3_3.Y, vector3_3.Z, 0.0f, 1f, 1f, 1f, 1f, vector3_2.X, vector3_2.Y, vector3_2.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
          Vector3 vector3_4 = vector3_1 + new Vector3(0.05375f);
          Vector3 vector3_5 = vector3_1 / 2f + offset + faceVector * val1;
          connection.LinkInstances.Add(instances.Count);
          instances.Add(new Matrix(vector3_5.X, vector3_5.Y, vector3_5.Z, 0.0f, 1f, 1f, 1f, 1f, vector3_4.X, vector3_4.Y, vector3_4.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
          float num4 = sizeFactor - val1;
          Vector3 vector3_6 = faceVector * num4 + new Vector3(0.05375f);
          Vector3 vector3_7 = faceVector * num4 / 2f + offset + faceVector * val1 + vector3_1;
          connection.LinkInstances.Add(instances.Count);
          instances.Add(new Matrix(vector3_7.X, vector3_7.Y, vector3_7.Z, 0.0f, 1f, 1f, 1f, 1f, vector3_6.X, vector3_6.Y, vector3_6.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
        }
        else
        {
          Vector3 vector3_8 = faceVector * sizeFactor + new Vector3(0.05375f);
          Vector3 vector3_9 = faceVector * sizeFactor / 2f + offset;
          connection.LinkInstances.Add(instances.Count);
          instances.Add(new Matrix(vector3_9.X, vector3_9.Y, vector3_9.Z, 0.0f, 1f, 1f, 1f, 1f, vector3_8.X, vector3_8.Y, vector3_8.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
        }
        WorldMap.DoSpecial(connection, offset, faceVector, sizeFactor, instances);
      }
      connection.Node.Connections.Add(new MapNode.Connection()
      {
        Node = node,
        Face = connection.Face.GetOpposite(),
        BranchOversize = connection.BranchOversize,
        LinkInstances = connection.LinkInstances
      });
    }
  }

  private static void DoSpecial(
    MapNode.Connection c,
    Vector3 offset,
    Vector3 faceVector,
    float sizeFactor,
    List<Matrix> instances)
  {
    if (c.Node.LevelName == "LIGHTHOUSE_SPIN")
    {
      Vector3 backward = Vector3.Backward;
      float num = 3.425f;
      Vector3 vector3_1 = backward * num + new Vector3(0.05375f);
      Vector3 vector3_2 = backward * num / 2f + offset + faceVector * sizeFactor;
      c.LinkInstances.Add(instances.Count);
      instances.Add(new Matrix(vector3_2.X, vector3_2.Y, vector3_2.Z, 0.0f, 1f, 1f, 1f, 1f, vector3_1.X, vector3_1.Y, vector3_1.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
    }
    if (!(c.Node.LevelName == "LIGHTHOUSE_HOUSE_A"))
      return;
    Vector3 right = Vector3.Right;
    float num1 = 5f;
    Vector3 vector3_3 = right * num1 + new Vector3(0.05375f);
    Vector3 vector3_4 = right * num1 / 2f + offset + faceVector * sizeFactor;
    c.LinkInstances.Add(instances.Count);
    instances.Add(new Matrix(vector3_4.X, vector3_4.Y, vector3_4.Z, 0.0f, 1f, 1f, 1f, 1f, vector3_3.X, vector3_3.Y, vector3_3.Z, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading || !this.Resolved && !this.ScheduleExit)
      return;
    this.SinceStarted += gameTime.ElapsedGameTime;
    this.SinceMouseMoved += (float) gameTime.ElapsedGameTime.TotalSeconds;
    if (this.MouseState.Movement.X != 0 || this.MouseState.Movement.Y != 0)
      this.SinceMouseMoved = 0.0f;
    Vector3 right = this.CameraManager.InverseView.Right;
    Vector3 up = this.CameraManager.InverseView.Up;
    Vector3 forward = this.CameraManager.InverseView.Forward;
    if (!this.GameState.InMap)
      ServiceHelper.RemoveComponent<WorldMap>(this);
    else if (this.InputManager.Back == FezButtonState.Pressed || this.InputManager.CancelTalk == FezButtonState.Pressed && this.SpeechBubble.Hidden)
    {
      this.Exit();
    }
    else
    {
      float viewScale = this.GraphicsDevice.GetViewScale();
      float num1 = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
      this.CameraManager.Radius = WorldMap.ZoomCycle[this.ZoomLevel] * num1;
      float radius = this.CameraManager.Radius;
      bool flag = this.MouseState.RightButton.State == MouseButtonStates.Dragging;
      if (this.FinishedInTransition)
      {
        if (this.InputManager.RotateRight == FezButtonState.Pressed)
          this.sRotateRight.Emit();
        if (this.InputManager.RotateLeft == FezButtonState.Pressed)
          this.sRotateLeft.Emit();
        if (!FezMath.AlmostEqual(this.InputManager.Movement, Vector2.Zero))
        {
          IGameCameraManager cameraManager = this.CameraManager;
          cameraManager.Center = cameraManager.Center + (this.InputManager.Movement.X * right * 0.015f * radius / viewScale + this.InputManager.Movement.Y * up * 0.015f * radius / viewScale);
        }
        if (flag)
        {
          IGameCameraManager cameraManager = this.CameraManager;
          cameraManager.Center = cameraManager.Center + ((float) -this.MouseState.Movement.X * right * 0.0008f * radius / (viewScale * viewScale) + (float) this.MouseState.Movement.Y * up * 0.0008f * radius / (viewScale * viewScale));
        }
        else if (this.MouseState.RightButton.State == MouseButtonStates.DragEnded)
          this.blockViewPicking = false;
        this.blockViewPicking |= flag;
        if (this.InputManager.MapZoomIn == FezButtonState.Pressed && this.ZoomLevel != WorldMap.ZoomCycle.Length - 1)
        {
          ++this.ZoomLevel;
          this.sZoomIn.Emit();
          this.ShadeNeighbourNodes();
        }
        if (this.InputManager.MapZoomOut == FezButtonState.Pressed && this.ZoomLevel != 0)
        {
          --this.ZoomLevel;
          this.sZoomOut.Emit();
          this.ShadeNeighbourNodes();
        }
        if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
          this.MousePicking();
        this.ViewPicking();
      }
      foreach (Group group in this.ButtonsMesh.Groups)
      {
        if (group.CustomData is AnimatedTexture customData)
        {
          customData.Timing.Update(gameTime.ElapsedGameTime);
          int width = customData.Texture.Width;
          int height = customData.Texture.Height;
          int frame = customData.Timing.Frame;
          Rectangle offset = customData.Offsets[frame];
          group.TextureMatrix.Set(new Matrix?(new Matrix((float) offset.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset.Height / (float) height, 0.0f, 0.0f, (float) offset.X / (float) width, (float) offset.Y / (float) height, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)));
        }
      }
      if (this.CurrentNode.Group != null)
      {
        double num2 = this.SinceStarted.TotalSeconds * 0.44999998807907104;
        for (int index = 0; index < this.WavesMesh.Groups.Count; ++index)
        {
          float num3 = Easing.Ease((double) FezMath.Frac((float) num2 + (float) index / (float) (this.WavesMesh.Groups.Count * 2)), -0.5f, EasingType.Sine);
          float x = this.WavesMesh.Groups[index].Scale.X;
          this.WavesMesh.Groups[index].Scale = new Vector3(num3 * 5f * this.CurrentNode.NodeType.GetSizeFactor());
          if (index == 0 && (double) x > (double) this.WavesMesh.Groups[index].Scale.X)
            this.sBeacon.EmitAt(this.WavesMesh.Position).OverrideMap = true;
          this.WavesMesh.Groups[index].Material.Opacity = (float) (1.0 - ((double) num3 - 0.40000000596046448) / 0.60000002384185791);
        }
      }
      float num4 = (float) this.GraphicsDevice.Viewport.Width / (viewScale / 1280f);
      this.ScaleIcons();
      foreach (Group group in this.NodesMesh.Groups)
      {
        NodeGroupData customData = group.CustomData as NodeGroupData;
        ShaderInstancedIndexedPrimitives<VertexPositionColorTextureInstance, Matrix> indexedPrimitives = customData.Complete ? this.GoldenHighlightsGeometry : this.WhiteHighlightsGeometry;
        float num5 = customData.Node.NodeType.GetSizeFactor() + 0.125f;
        Matrix instance = indexedPrimitives.Instances[customData.HighlightInstance];
        Vector3 vector3_1 = new Vector3(instance.M31, instance.M32, instance.M33);
        if (this.FocusNode == customData.Node)
          vector3_1 = Vector3.Lerp(vector3_1, new Vector3(1.25f * num5), 0.25f);
        else if (!FezMath.AlmostEqual(vector3_1.X, 1f))
          vector3_1 = Vector3.Lerp(vector3_1, new Vector3(num5), 0.25f);
        Vector3 vector3_2 = group.Position + customData.Node.NodeType.GetSizeFactor() / 2f * new Vector3(1f, 1f, -1f) * (vector3_1 / num5) + 0.2f * new Vector3(1f, 0.0f, -1f);
        float iconScale = this.GetIconScale(viewScale, radius / viewScale / num4);
        indexedPrimitives.Instances[customData.HighlightInstance].M31 = vector3_1.X;
        indexedPrimitives.Instances[customData.HighlightInstance].M32 = vector3_1.Y;
        indexedPrimitives.Instances[customData.HighlightInstance].M33 = vector3_1.Z;
        indexedPrimitives.InstancesDirty = true;
        foreach (int iconInstance in customData.IconInstances)
        {
          Vector3 vector3_3 = this.IconsTrailingOffset[iconInstance] * (iconScale / 4f) * this.CameraManager.InverseView.Down;
          this.IconsGeometry.Instances[iconInstance].M11 = vector3_2.X + vector3_3.X;
          this.IconsGeometry.Instances[iconInstance].M12 = vector3_2.Y + vector3_3.Y;
          this.IconsGeometry.Instances[iconInstance].M13 = vector3_2.Z + vector3_3.Z;
        }
        this.IconsGeometry.InstancesDirty = true;
      }
      if (this.FocusNode != null && this.FocusNode.Group != null && !flag)
      {
        Vector3 vector3_4 = Vector3.Transform(Vector3.Zero, (Matrix) this.FocusNode.Group.WorldMatrix);
        Vector3 vector3_5 = this.CameraManager.Center - vector3_4;
        float a = vector3_5.Length();
        if (!FezMath.AlmostEqual(a, 0.0f))
        {
          Vector3 vector3_6 = vector3_5 / a;
          float num6 = MathHelper.Clamp(a * 2f, 0.0f, this.chosenByMouseClick ? 3f : 1f);
          IGameCameraManager cameraManager1 = this.CameraManager;
          cameraManager1.Center = cameraManager1.Center - vector3_6 * num6 * radius * 0.005f;
          IGameCameraManager cameraManager2 = this.CameraManager;
          cameraManager2.Center = cameraManager2.Center + (vector3_4 - this.CameraManager.Center).Dot(forward) * forward;
        }
        if (this.blockViewPicking && (double) (this.CameraManager.InterpolatedCenter - vector3_4).Length() < 0.5)
          this.blockViewPicking = false;
      }
      this.ShadeNeighbourNodes();
      if (this.DotDialogueIndex >= WorldMap.DotDialogue.Length || this.DotManager.Hidden || this.GameState.SaveData.HasHadMapHelp)
        return;
      if (this.SpeechBubble.Hidden)
      {
        this.SpeechBubble.ChangeText(GameText.GetString(WorldMap.DotDialogue[this.DotDialogueIndex]));
      }
      else
      {
        if (this.InputManager.CancelTalk != FezButtonState.Pressed)
          return;
        this.sTextNext.Emit();
        ++this.DotDialogueIndex;
        if (this.DotDialogueIndex == WorldMap.DotDialogue.Length)
        {
          this.DotManager.Burrow();
          this.GameState.SaveData.HasHadMapHelp = true;
        }
        this.SpeechBubble.Hide();
      }
    }
  }

  private void ViewPicking()
  {
    this.CursorSelectable = false;
    Vector3 right = this.CameraManager.InverseView.Right;
    Vector3 up = this.CameraManager.InverseView.Up;
    Vector3 forward = this.CameraManager.InverseView.Forward;
    this.closestNodes.Clear();
    float minDepth = float.MaxValue;
    float val2_1 = float.MinValue;
    float minDist = float.MaxValue;
    float val2_2 = float.MinValue;
    Ray ray1 = new Ray(this.GraphicsDevice.Viewport.Unproject(new Vector3((float) this.MouseState.Position.X, (float) this.MouseState.Position.Y, 0.0f), this.CameraManager.Projection, this.CameraManager.View, Matrix.Identity), forward);
    Ray ray2 = new Ray(this.CameraManager.Position - forward * this.CameraManager.Radius, forward);
    foreach (Group group in this.NodesMesh.Groups)
    {
      float opacity = group.Material.Opacity;
      if ((double) opacity >= 0.0099999997764825821)
      {
        NodeGroupData customData = group.CustomData as NodeGroupData;
        float sizeFactor = customData.Node.NodeType.GetSizeFactor();
        float num = (float) (((double) sizeFactor - 0.5) / 2.0 + 1.0) * MathHelper.Lerp(opacity, 1f, 0.5f);
        Vector3 vector3 = Vector3.Transform(Vector3.Zero, (Matrix) group.WorldMatrix);
        BoundingBox box = new BoundingBox(vector3 - new Vector3(num), vector3 + new Vector3(num));
        float val1_1 = (vector3 - this.CameraManager.Position).Dot(forward);
        customData.Depth = val1_1;
        float? nullable = ray1.Intersects(box);
        if (nullable.HasValue)
          this.CursorSelectable = true;
        if (!this.blockViewPicking)
        {
          nullable = ray2.Intersects(box);
          if (nullable.HasValue)
          {
            minDepth = Math.Min(val1_1, minDepth);
            val2_1 = Math.Max(val1_1, val2_1);
            Vector3 a = vector3 - this.CameraManager.Position;
            float val1_2 = new Vector2(a.Dot(right), a.Dot(up)).Length() * sizeFactor;
            minDist = Math.Min(val1_2, minDist);
            val2_2 = Math.Max(val1_2, val2_2);
            this.closestNodes.Add(new WorldMap.QualifiedNode()
            {
              Node = customData.Node,
              Depth = val1_1,
              ScreenDistance = val1_2,
              Transparency = FezMath.AlmostClamp(1f - opacity)
            });
          }
        }
      }
    }
    if (this.blockViewPicking)
      return;
    if (this.closestNodes.Count > 0)
    {
      float depthRange = val2_1 - minDepth;
      float distRange = val2_2 - minDist;
      WorldMap.QualifiedNode qualifiedNode = this.closestNodes.OrderBy<WorldMap.QualifiedNode, float>((Func<WorldMap.QualifiedNode, float>) (n => (float) ((double) n.Transparency * 2.0 + ((double) n.ScreenDistance - (double) minDist) / (double) distRange + ((double) n.Depth - (double) minDepth) / (double) depthRange / 2.0))).FirstOrDefault<WorldMap.QualifiedNode>();
      MapNode focusNode = this.FocusNode;
      this.LastFocusedNode = this.FocusNode = qualifiedNode.Node;
      if (this.FocusNode != null && this.FocusNode != focusNode)
        this.sMagnet.Emit();
      this.chosenByMouseClick = false;
    }
    else
      this.FocusNode = (MapNode) null;
    this.NodesMesh.Groups.Sort((IComparer<Group>) WorldMap.NodeGroupDataComparer.Default);
  }

  private void MousePicking()
  {
    Vector3 right = this.CameraManager.InverseView.Right;
    Vector3 up = this.CameraManager.InverseView.Up;
    Vector3 forward = this.CameraManager.InverseView.Forward;
    this.closestNodes.Clear();
    float minDepth = float.MaxValue;
    float val2_1 = float.MinValue;
    float minDist = float.MaxValue;
    float val2_2 = float.MinValue;
    Ray ray = new Ray(this.GraphicsDevice.Viewport.Unproject(new Vector3((float) this.MouseState.Position.X, (float) this.MouseState.Position.Y, 0.0f), this.CameraManager.Projection, this.CameraManager.View, Matrix.Identity), forward);
    foreach (Group group in this.NodesMesh.Groups)
    {
      float opacity = group.Material.Opacity;
      if ((double) opacity >= 0.0099999997764825821)
      {
        NodeGroupData customData = group.CustomData as NodeGroupData;
        float sizeFactor = customData.Node.NodeType.GetSizeFactor();
        float num = sizeFactor * 0.625f;
        Vector3 vector3 = Vector3.Transform(Vector3.Zero, (Matrix) group.WorldMatrix);
        BoundingBox box = new BoundingBox(vector3 - new Vector3(num), vector3 + new Vector3(num));
        float val1_1 = (vector3 - this.CameraManager.Position).Dot(forward);
        customData.Depth = val1_1;
        if (ray.Intersects(box).HasValue)
        {
          minDepth = Math.Min(val1_1, minDepth);
          val2_1 = Math.Max(val1_1, val2_1);
          Vector3 a = vector3 - this.CameraManager.Position;
          float val1_2 = new Vector2(a.Dot(right), a.Dot(up)).Length() * sizeFactor;
          minDist = Math.Min(val1_2, minDist);
          val2_2 = Math.Max(val1_2, val2_2);
          this.closestNodes.Add(new WorldMap.QualifiedNode()
          {
            Node = customData.Node,
            Depth = val1_1,
            ScreenDistance = val1_2,
            Transparency = FezMath.AlmostClamp(1f - opacity)
          });
        }
      }
    }
    if (this.closestNodes.Count > 0)
    {
      float depthRange = val2_1 - minDepth;
      float distRange = val2_2 - minDist;
      WorldMap.QualifiedNode qualifiedNode = this.closestNodes.OrderBy<WorldMap.QualifiedNode, float>((Func<WorldMap.QualifiedNode, float>) (n => (float) ((double) n.Transparency * 2.0 + ((double) n.ScreenDistance - (double) minDist) / (double) distRange + ((double) n.Depth - (double) minDepth) / (double) depthRange / 2.0))).FirstOrDefault<WorldMap.QualifiedNode>();
      MapNode focusNode = this.FocusNode;
      this.LastFocusedNode = this.FocusNode = qualifiedNode.Node;
      if (this.FocusNode != null && this.FocusNode != focusNode)
        this.sMagnet.Emit();
      this.chosenByMouseClick = true;
      this.blockViewPicking = true;
    }
    this.NodesMesh.Groups.Sort((IComparer<Group>) WorldMap.NodeGroupDataComparer.Default);
  }

  private float GetIconScale(float viewScale, float radius)
  {
    return (double) viewScale <= 1.0 ? ((double) radius <= 16.0 || (double) radius > 40.0 ? ((double) radius <= 40.0 ? 1f : (float) (((double) radius - 40.0) / 40.0 * 2.5 + 2.5)) : (float) (((double) radius - 16.0) / 24.0 * 1.5 + 1.0)) : ((double) radius <= 16.0 || (double) radius > 40.0 ? ((double) radius <= 40.0 ? 1f : (float) (((double) radius - 40.0) / 40.0 * 1.25 + 1.25)) : (float) (((double) radius - 16.0) / 24.0 * 0.25 + 1.0));
  }

  private void ScaleIcons()
  {
    float viewScale = this.GraphicsDevice.GetViewScale();
    float num = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
    float radius = this.CameraManager.Radius / viewScale / num;
    float iconScale = this.GetIconScale(viewScale, radius);
    for (int index = 0; index < this.IconsGeometry.Instances.Length; ++index)
    {
      this.IconsGeometry.Instances[index].M31 = this.IconsOriginalInstances[index].M31 * iconScale;
      this.IconsGeometry.Instances[index].M32 = this.IconsOriginalInstances[index].M32 * iconScale;
      this.IconsGeometry.Instances[index].M33 = this.IconsOriginalInstances[index].M33 * iconScale;
      this.IconsGeometry.InstancesDirty = true;
    }
  }

  private void ShadeNeighbourNodes()
  {
    int num1 = 0;
    MapNode mapNode1 = this.FocusNode ?? this.LastFocusedNode;
    this.nextToCover.Clear();
    this.nextToCover.Add(mapNode1);
    this.toCover.Clear();
    this.hasCovered.Clear();
    this.hasCovered.Add(mapNode1);
    float num2 = 5f - (float) this.ZoomLevel;
    float num3 = (float) (1.0 + Math.Round((double) num2 > 3.0 ? Math.Pow((double) num2 - 1.75, 2.5) : (double) num2));
    while (this.nextToCover.Count > 0)
    {
      this.toCover.Clear();
      this.toCover.AddRange((IEnumerable<MapNode>) this.nextToCover);
      this.nextToCover.Clear();
      foreach (MapNode mapNode2 in this.toCover)
      {
        Group group = mapNode2.Group;
        if (group != null)
        {
          float num4 = mapNode2 == this.CurrentNode ? 1f : FezMath.Saturate((num3 - (float) num1) / num3);
          group.Material.Opacity = MathHelper.Lerp(group.Material.Opacity, num4, 0.2f);
          group.Enabled = (double) group.Material.Opacity > 0.0099999997764825821;
          NodeGroupData customData = (NodeGroupData) group.CustomData;
          (customData.Complete ? this.GoldenHighlightsGeometry : this.WhiteHighlightsGeometry).Instances[customData.HighlightInstance].M24 = group.Material.Opacity;
          (customData.Complete ? this.GoldenHighlightsGeometry : this.WhiteHighlightsGeometry).InstancesDirty = true;
          foreach (int iconInstance in customData.IconInstances)
            this.IconsGeometry.Instances[iconInstance].M24 = MathHelper.Lerp(this.IconsGeometry.Instances[iconInstance].M24, group.Material.Opacity, 0.2f);
          this.IconsGeometry.InstancesDirty = true;
          float num5 = FezMath.Saturate((num3 - (num1 == 0 ? 0.0f : (float) (num1 + 1))) / num3);
          foreach (MapNode.Connection connection in mapNode2.Connections)
          {
            if (!this.hasCovered.Contains(connection.Node))
            {
              if (connection.LinkInstances != null)
              {
                foreach (int linkInstance in connection.LinkInstances)
                  this.LinksGeometry.Instances[linkInstance].M24 = MathHelper.Lerp(this.LinksGeometry.Instances[linkInstance].M24, num5, 0.2f);
              }
              this.nextToCover.Add(connection.Node);
              this.hasCovered.Add(connection.Node);
            }
          }
          this.LinksGeometry.InstancesDirty = true;
        }
      }
      ++num1;
    }
  }

  private void Exit()
  {
    this.ScheduleExit = true;
    this.Resolved = false;
    this.TargetRenderingManager.ScheduleHook(this.DrawOrder, this.FadeOutRtHandle.Target);
    this.GameService.CloseScroll((string) null);
  }

  private void StartInTransition()
  {
    this.GameState.SkipRendering = true;
    this.CameraManager.PixelsPerTrixel = 3f;
    WorldMap.Starfield.Opacity = this.LinksMesh.Material.Opacity = this.NodesMesh.Material.Opacity = this.LegendMesh.Material.Opacity = 0.0f;
    this.CameraManager.ChangeViewpoint(Viewpoint.Front, 0.0f);
    Quaternion phi180 = this.OriginalRotation * Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f);
    Vector3 aoMaxPos = this.PlayerManager.Center + 6f * Vector3.UnitY;
    Waiters.Interpolate(0.75, (Action<float>) (s =>
    {
      if (!this.Enabled)
        return;
      float amount = Easing.EaseOut((double) s, EasingType.Cubic);
      WorldMap.Starfield.Opacity = this.LinksMesh.Material.Opacity = this.NodesMesh.Material.Opacity = this.LegendMesh.Material.Opacity = amount;
      Mesh iconsMesh = this.IconsMesh;
      Mesh linksMesh = this.LinksMesh;
      Mesh nodesMesh = this.NodesMesh;
      Vector3 vector3_1 = new Vector3((float) (0.5 + (double) amount / 2.0));
      Vector3 vector3_2 = vector3_1;
      nodesMesh.Scale = vector3_2;
      Vector3 vector3_3;
      Vector3 vector3_4 = vector3_3 = vector3_1;
      linksMesh.Scale = vector3_3;
      Vector3 vector3_5 = vector3_4;
      iconsMesh.Scale = vector3_5;
      this.IconsMesh.Position = this.LinksMesh.Position = this.NodesMesh.Position = this.PlayerManager.Center + amount * 6f * Vector3.UnitY;
      this.IconsMesh.Rotation = this.LinksMesh.Rotation = this.NodesMesh.Rotation = Quaternion.Slerp(phi180, this.OriginalRotation, amount);
      this.CameraManager.Center = Vector3.Lerp(this.OriginalCenter, aoMaxPos + Vector3.Transform(this.CurrentNode.Group == null ? Vector3.Zero : this.CurrentNode.Group.Position, this.NodesMesh.Rotation), amount);
    }), (Action) (() => this.FinishedInTransition = true));
  }

  private void StartOutTransition()
  {
    this.GameState.SkipRendering = false;
    if (!this.GameState.SaveData.HasHadMapHelp)
    {
      this.DotManager.RevertDrawOrder();
      this.SpeechBubble.RevertDrawOrder();
      this.DotManager.Burrow();
      this.SpeechBubble.Hide();
    }
    this.sExit.Emit();
    this.CameraManager.PixelsPerTrixel = this.OriginalPixPerTrix;
    this.GameState.InMap = false;
    this.CameraManager.ChangeViewpoint(this.OriginalViewpoint, 0.0f);
    this.CameraManager.Center = this.OriginalCenter + 6f * Vector3.UnitY;
    this.CameraManager.Direction = this.OriginalDirection;
    this.CameraManager.SnapInterpolation();
    Waiters.Interpolate(0.75, (Action<float>) (s =>
    {
      this.CameraManager.Center = Vector3.Lerp(this.OriginalCenter, this.OriginalCenter + 6f * Vector3.UnitY, 1f - Easing.EaseInOut((double) s, EasingType.Sine, EasingType.Quadratic));
      this.NodesMesh.Material.Opacity = 1f - Easing.EaseOut((double) s, EasingType.Quadratic);
    }), (Action) (() => ServiceHelper.RemoveComponent<WorldMap>(this)));
    this.Enabled = false;
  }

  public override void Draw(GameTime gameTime)
  {
    if (WorldMap.Instance == null)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    if (this.GameState.StereoMode)
      BaseEffect.EyeSign = Vector3.Zero;
    if (this.TargetRenderingManager.IsHooked(this.FadeInRtHandle.Target) && !this.Resolved && !this.ScheduleExit)
    {
      this.TargetRenderingManager.Resolve(this.FadeInRtHandle.Target, false);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.Resolved = true;
      this.GameState.InMap = true;
      this.InputManager.StrictRotation = true;
      this.StartInTransition();
    }
    if (this.ScheduleExit && this.Resolved)
    {
      graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      graphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
      this.TargetRenderingManager.DrawFullscreen((Texture) this.FadeOutRtHandle.Target, new Color(1f, 1f, 1f, this.NodesMesh.Material.Opacity));
    }
    else
    {
      if (!this.SpeechBubble.Hidden)
        this.SpeechBubble.Origin = this.DotManager.Position - new Vector3(0.0f, (float) (1.0 / (double) this.CameraManager.Radius * 2.0) * this.GraphicsDevice.GetViewScale(), 0.0f);
      if (this.ScheduleExit && !this.Resolved)
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.Red | ColorWriteChannels.Green | ColorWriteChannels.Blue);
      float opacity = this.NodesMesh.Material.Opacity;
      foreach (Group group in this.ButtonsMesh.Groups)
        group.Material.Opacity = opacity;
      if (this.Enabled)
      {
        if ((double) opacity < 1.0)
        {
          this.GraphicsDevice.Clear(ClearOptions.Target, Color.Black, 1f, 0);
          graphicsDevice.SetBlendingMode(BlendingMode.Opaque);
          this.GraphicsDevice.SetupViewport();
          this.TargetRenderingManager.DrawFullscreen((Texture) this.FadeInRtHandle.Target);
        }
      }
      else
      {
        for (int index = 0; index < this.WavesMesh.Groups.Count; ++index)
          this.WavesMesh.Groups[index].Material.Opacity *= 0.9f;
      }
      graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      this.TargetRenderingManager.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, opacity));
      this.GraphicsDevice.SetupViewport();
      WorldMap.Starfield.Draw();
      this.GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1f, 0);
      this.NodesMesh.Draw();
      this.LinksMesh.Draw();
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.Trails);
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Additive);
      this.TargetRenderingManager.DrawFullscreen((Texture) this.ShineTex, new Color(0.5f, 0.435f, 0.285f));
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      if (this.CurrentNode.Group != null)
      {
        this.WavesMesh.Position = Vector3.Transform(Vector3.Zero, (Matrix) this.CurrentNode.Group.WorldMatrix) - this.CameraManager.Direction;
        this.WavesMesh.Rotation = this.CameraManager.Rotation;
      }
      this.WavesMesh.Draw();
      this.IconsMesh.Draw();
      this.ButtonsMesh.Draw();
      this.LegendMesh.Draw();
      if (Culture.IsCJK)
        this.SpriteBatch.BeginLinear();
      else
        this.SpriteBatch.BeginPoint();
      bool flag = this.GraphicsDevice.DisplayMode.Width < 1280 /*0x0500*/;
      float viewScale = this.GraphicsDevice.GetViewScale();
      float num1 = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
      float num2 = (float) this.GraphicsDevice.Viewport.Height / (720f * viewScale);
      SpriteFont font = Culture.IsCJK ? this.FontManager.Small : this.FontManager.Big;
      float scale1 = (Culture.IsCJK ? this.FontManager.SmallFactor * 0.8f : (flag ? 1.5f : 1f)) * viewScale;
      string[] strArray1 = new string[5]
      {
        StaticText.GetString("MapPan"),
        StaticText.GetString("MapZoom"),
        StaticText.GetString("MapSpin"),
        StaticText.GetString("MapLook"),
        StaticText.GetString("MapBack")
      };
      float[] numArray = new float[5]
      {
        48f,
        48f,
        41f,
        37f,
        45f
      };
      float num3 = (Culture.IsCJK ? -15f : -25f) * num2;
      float val1_1 = 0.0f;
      int num4 = 0;
      foreach (string text in strArray1)
      {
        val1_1 = Math.Max(val1_1, font.MeasureString(text).X * scale1);
        this.GTR.DrawStringLFLeftAlign(this.SpriteBatch, font, text, new Color(1f, 1f, 1f, opacity), new Vector2((float) (1280.0 * (double) num1 - 150.0 * (double) num1 - (GamepadState.AnyConnected ? 0.0 : 30.0)), (float) (30.0 + 100.0 * (double) num2) + num3) * viewScale, scale1);
        num3 += numArray[num4++] * num2;
      }
      this.ButtonsMesh.Groups[0].Scale = new Vector3((float) ((double) val1_1 / 1280.0 * 40.0 / (double) viewScale + (GamepadState.AnyConnected ? 3.125 : 4.0)), 0.975f, 1f);
      this.ButtonsMesh.Groups[0].Position = new Vector3(2.7f - this.ButtonsMesh.Groups[0].Scale.X, 0.0f, 0.0f);
      string[] strArray2 = new string[7]
      {
        StaticText.GetString("MapLegendWarpGate"),
        StaticText.GetString("MapLegendSmallGate"),
        StaticText.GetString("MapLegendTreasure"),
        StaticText.GetString("MapLegendLockedDoor"),
        StaticText.GetString("MapLegendCube"),
        StaticText.GetString("MapLegendBits"),
        StaticText.GetString("MapLegendSecret")
      };
      float num5 = Culture.IsCJK ? 3f : 0.0f;
      float val1_2 = 0.0f;
      float num6 = (float) (500.0 + (double) num2 * 7.0);
      float num7 = 24f;
      foreach (string text in strArray2)
      {
        val1_2 = Math.Max(val1_2, font.MeasureString(text).X * scale1);
        this.GTR.DrawString(this.SpriteBatch, font, text, new Vector2((float) (64.0 * (double) num1 + 40.0 * (double) num2), (num6 + num5) * num2) * viewScale, new Color(1f, 1f, 1f, opacity), scale1);
        num5 += num7;
      }
      this.LegendMesh.Groups[0].Scale = new Vector3((float) ((double) val1_2 / 1280.0 * 40.0 / (double) viewScale + 1.75), 1f, 1f);
      float num8 = (GamepadState.AnyConnected ? 2f : 1.5f) * num2;
      float num9 = (GamepadState.AnyConnected ? 170f : 180f) * num1 - FezMath.Max<float>(num1 - 1f, 0.0f) * 20f;
      if (Culture.IsCJK)
      {
        this.SpriteBatch.End();
        this.SpriteBatch.BeginPoint();
      }
      Texture2D replacedGlyphTexture1 = this.GTR.GetReplacedGlyphTexture("{LS}");
      this.SpriteBatch.Draw(replacedGlyphTexture1, new Vector2((float) (1280.0 * (double) num1 - (double) num9 + (double) (64 /*0x40*/ - replacedGlyphTexture1.Width) / 2.0 * 1.5), 57f * num2) * viewScale, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, num8 * viewScale, SpriteEffects.None, 0.0f);
      Texture2D replacedGlyphTexture2 = this.GTR.GetReplacedGlyphTexture("{RB}");
      this.SpriteBatch.Draw(replacedGlyphTexture2, new Vector2((float) (1280.0 * (double) num1 - (double) num9 + (double) (64 /*0x40*/ - replacedGlyphTexture2.Width) / 2.0 * 1.5), (float) (107.0 * (double) num2 - 6.0 * (double) num8)) * viewScale, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, num8 * viewScale, SpriteEffects.None, 0.0f);
      Texture2D replacedGlyphTexture3 = this.GTR.GetReplacedGlyphTexture("{LB}");
      this.SpriteBatch.Draw(replacedGlyphTexture3, new Vector2((float) (1280.0 * (double) num1 - (double) num9 + (double) (64 /*0x40*/ - replacedGlyphTexture3.Width) / 2.0 * 1.5), (float) (107.0 * (double) num2 + 6.0 * (double) num8)) * viewScale, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, num8 * viewScale, SpriteEffects.None, 0.0f);
      Texture2D replacedGlyphTexture4 = this.GTR.GetReplacedGlyphTexture("{LT}");
      this.SpriteBatch.Draw(replacedGlyphTexture4, new Vector2((float) (1280.0 * (double) num1 - (double) num9 + (double) (64 /*0x40*/ - replacedGlyphTexture4.Width) / 2.0 * 1.5 + (GamepadState.AnyConnected ? -5.0 * (double) num8 : 0.0)), (float) (155.0 * (double) num2 + (GamepadState.AnyConnected ? 0.0 : -6.0 * (double) num8))) * viewScale, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, num8 * viewScale, SpriteEffects.None, 0.0f);
      Texture2D replacedGlyphTexture5 = this.GTR.GetReplacedGlyphTexture("{RT}");
      this.SpriteBatch.Draw(replacedGlyphTexture5, new Vector2((float) (1280.0 * (double) num1 - (double) num9 + (double) (64 /*0x40*/ - replacedGlyphTexture5.Width) / 2.0 * 1.5 + (GamepadState.AnyConnected ? 5.0 * (double) num8 : 0.0)), (float) (155.0 * (double) num2 + (GamepadState.AnyConnected ? 0.0 : 6.0 * (double) num8))) * viewScale, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, num8 * viewScale, SpriteEffects.None, 0.0f);
      Texture2D replacedGlyphTexture6 = this.GTR.GetReplacedGlyphTexture("{RS}");
      this.SpriteBatch.Draw(replacedGlyphTexture6, new Vector2((float) (1280.0 * (double) num1 - (double) num9 + (double) (64 /*0x40*/ - replacedGlyphTexture6.Width) / 2.0 * 1.5), 195f * num2) * viewScale, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, num8 * viewScale, SpriteEffects.None, 0.0f);
      Texture2D replacedGlyphTexture7 = this.GTR.GetReplacedGlyphTexture("{B}");
      this.SpriteBatch.Draw(replacedGlyphTexture7, new Vector2((float) (1280.0 * (double) num1 - (double) num9 + (double) (64 /*0x40*/ - replacedGlyphTexture7.Width) / 2.0 * 1.5), (float) (233 + (GamepadState.AnyConnected ? -5 : 0)) * num2) * viewScale, new Rectangle?(), Color.White, 0.0f, Vector2.Zero, num8 * viewScale, SpriteEffects.None, 0.0f);
      this.SpriteBatch.End();
      this.SpriteBatch.BeginPoint();
      float scale2 = viewScale * 2f;
      Point point = this.MouseState.PositionInViewport();
      this.SpriteBatch.Draw(this.MouseState.LeftButton.State == MouseButtonStates.Dragging || this.MouseState.RightButton.State == MouseButtonStates.Dragging ? this.GrabbedCursor : (this.CursorSelectable ? (this.MouseState.LeftButton.State == MouseButtonStates.Down ? this.ClickedCursor : this.CanClickCursor) : this.PointerCursor), new Vector2((float) point.X - scale2 * 11.5f, (float) point.Y - scale2 * 8.5f), new Rectangle?(), new Color(1f, 1f, 1f, FezMath.Saturate((float) (1.0 - ((double) this.SinceMouseMoved - 2.0)))), 0.0f, Vector2.Zero, scale2, SpriteEffects.None, 0.0f);
      this.SpriteBatch.End();
      if (!this.TargetRenderingManager.IsHooked(this.FadeOutRtHandle.Target) || this.Resolved || !this.ScheduleExit)
        return;
      this.TargetRenderingManager.Resolve(this.FadeOutRtHandle.Target, false);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.TargetRenderingManager.DrawFullscreen((Texture) this.FadeOutRtHandle.Target);
      this.Resolved = true;
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
      this.StartOutTransition();
    }
  }

  private void UpdateControllerTexture(object sender, EventArgs e)
  {
    bool flag = GamepadState.Layout == GamepadState.GamepadLayout.Xbox360;
    if (this.sewerQRGroup != null)
      this.sewerQRGroup.Texture = !flag ? this.sewerQRSony : this.sewerQRXbox;
    if (this.zuhouseQRGroup == null)
      return;
    if (flag)
      this.zuhouseQRGroup.Texture = this.zuhouseQRXbox;
    else
      this.zuhouseQRGroup.Texture = this.zuhouseQRSony;
  }

  [ServiceDependency]
  public IMouseStateManager MouseState { protected get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { get; set; }

  [ServiceDependency]
  public IInputManager InputManager { get; set; }

  [ServiceDependency]
  public IGameService GameService { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IFontManager FontManager { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IDotManager DotManager { get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private struct QualifiedNode
  {
    public MapNode Node;
    public float Depth;
    public float ScreenDistance;
    public float Transparency;
  }

  public class NodeGroupDataComparer : IComparer<Group>
  {
    public static readonly WorldMap.NodeGroupDataComparer Default = new WorldMap.NodeGroupDataComparer();

    public int Compare(Group x, Group y)
    {
      return -((NodeGroupData) x.CustomData).Depth.CompareTo(((NodeGroupData) y.CustomData).Depth);
    }
  }
}
