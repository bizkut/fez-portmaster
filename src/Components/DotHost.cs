// Decompiled with JetBrains decompiler
// Type: FezGame.Components.DotHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Components.Scripting;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

public class DotHost : DrawableGameComponent, IDotManager
{
  private List<Vector4> Vertices = new List<Vector4>();
  private int[] FaceVertexIndices;
  private Mesh DotMesh;
  private Mesh RaysMesh;
  private Mesh FlareMesh;
  private IndexedUserPrimitives<FezVertexPositionColor> DotWireGeometry;
  private IndexedUserPrimitives<FezVertexPositionColor> DotFacesGeometry;
  private float Theta;
  private Quaternion CamRotationFollow = Quaternion.Identity;
  private Vector3 InterpolatedPosition;
  private Vector3 InterpolatedScale;
  private float EightShapeStep;
  private Vector3 ToBackFollow;
  private float SinceStartedTransition;
  private float SinceStartedCameraPan;
  private Vector3 PanOrigin;
  private BackgroundPlane HaloPlane;
  private bool BurrowAfterPan;
  private Vector3 SpiralingCenter;
  private Vector3 lastRelativePosition;
  private GlyphTextRenderer GTR;
  private SpriteBatch spriteBatch;
  private Mesh BPromptMesh;
  private Mesh VignetteMesh;
  private SoundEffect sHide;
  private SoundEffect sComeOut;
  private SoundEffect sIdle;
  private SoundEffect sMove;
  private SoundEffect sHeyListen;
  private SoundEmitter eHide;
  private SoundEmitter eIdle;
  private SoundEmitter eMove;
  private SoundEmitter eComeOut;
  private SoundEmitter eHey;
  private DotHost.BehaviourType _behaviour;
  private DotHost.BehaviourType lastBehaviour;
  private RenderTarget2D bTexture;

  public bool Burrowing { get; set; }

  public bool ComingOut { get; set; }

  public bool DrawRays { get; set; }

  public object Owner { get; set; }

  public DotFaceButton FaceButton { get; set; }

  public Texture2D DestinationVignette { get; set; }

  public Texture2D DestinationVignetteSony { get; set; }

  public void Reset()
  {
    this.RotationSpeed = 1f;
    this.Opacity = 1f;
    this.Behaviour = DotHost.BehaviourType.FollowGomez;
    this.Target = new Vector3();
    this.Dialog = (string[]) null;
    this.TimeToWait = 0.0f;
    this.ScaleFactor = 1f;
    this.ScalePulsing = 1f;
    this.AlwaysShowLines = false;
    this.InnerScale = 1f;
    this.EightShapeStep = 0.0f;
    this.Hidden = true;
    this.Burrowing = this.ComingOut = false;
    this.RoamingVolume = (Volume) null;
    this.PreventPoI = false;
    this.SinceStartedCameraPan = 0.0f;
    this.DrawRays = true;
    this.Owner = (object) null;
    if (this.HaloPlane != null)
      this.HaloPlane.Hidden = true;
    this.KillSounds();
  }

  private void KillSounds()
  {
    if (this.eIdle != null && !this.eIdle.Dead)
      this.eIdle.FadeOutAndDie(0.1f, false);
    if (this.eMove != null && !this.eMove.Dead)
      this.eMove.FadeOutAndDie(0.1f, false);
    if (this.eHide != null && !this.eHide.Dead)
      this.eHide.FadeOutAndDie(0.1f, false);
    if (this.eComeOut != null && !this.eComeOut.Dead)
      this.eComeOut.FadeOutAndDie(0.1f, false);
    if (this.eHey == null || this.eHey.Dead)
      return;
    this.eHey.FadeOutAndDie(0.1f, false);
  }

  public void Burrow()
  {
    if (this.Burrowing || this.Hidden)
      return;
    if (this.eHide != null && !this.eHide.Dead)
      this.eHide.FadeOutAndDie(0.1f, false);
    this.eHide = this.sHide.EmitAt(this.Position);
    if (this.eIdle != null && !this.eIdle.Dead)
    {
      this.eIdle.FadeOutAndDie(1f, false);
      this.eIdle = (SoundEmitter) null;
    }
    if (this.eMove != null && !this.eMove.Dead)
      this.eMove.FadeOutAndDie(1f, false);
    if (this.eComeOut != null && !this.eComeOut.Dead)
      this.eComeOut.FadeOutAndDie(0.1f, false);
    if (this.eHey != null && !this.eHey.Dead)
      this.eHey.FadeOutAndDie(0.1f, false);
    this.SinceStartedTransition = !this.ComingOut ? 0.0f : 1f - FezMath.Saturate(this.SinceStartedTransition);
    this.ComingOut = false;
    this.Burrowing = true;
  }

  public void Hey()
  {
    if (this.eHey != null && !this.eHey.Dead)
      this.eHey.FadeOutAndDie(0.1f, false);
    this.eHey = this.sHeyListen.EmitAt(this.Position, RandomHelper.Centered(13.0 / 400.0));
  }

  public void ComeOut() => this.ComeOut(false);

  private void ComeOut(bool mute)
  {
    if (this.ComingOut || !this.Burrowing && !this.Hidden)
      return;
    if (this.Burrowing)
    {
      this.SinceStartedTransition = 1f - FezMath.Saturate(this.SinceStartedTransition);
    }
    else
    {
      this.Reset();
      this.InterpolatedPosition = this.PlayerManager.Position;
      this.SinceStartedTransition = 0.0f;
    }
    if (!mute)
    {
      if (this.eHide != null && !this.eHide.Dead)
        this.eHide.FadeOutAndDie(0.1f, false);
      if (this.eComeOut != null && !this.eComeOut.Dead)
        this.eComeOut.FadeOutAndDie(0.1f, false);
      if (!this.Burrowing)
        this.eComeOut = this.sComeOut.EmitAt(this.Position);
      this.eIdle = this.sIdle.EmitAt(this.Position, true);
    }
    this.EightShapeStep = 0.0f;
    this.ComingOut = true;
    this.Burrowing = false;
    this.Hidden = false;
  }

  public bool Hidden
  {
    get => !this.Visible && !this.Enabled;
    set
    {
      this.Visible = this.Enabled = !value;
      if (!value)
        this.Burrowing = false;
      if (this.HaloPlane == null)
        return;
      this.HaloPlane.Hidden = value;
    }
  }

  public void MoveWithCamera(Vector3 target, bool burrowAfter)
  {
    this.PanOrigin = this.Hidden ? this.PlayerManager.Position : this.DotMesh.Position;
    this.ComeOut();
    this.SinceStartedCameraPan = 0.0f;
    this.Behaviour = DotHost.BehaviourType.MoveToTargetWithCamera;
    this.CameraManager.Constrained = true;
    this.Target = target;
    this.BurrowAfterPan = burrowAfter;
    this.eMove = this.sMove.EmitAt(this.Position, true, 0.0f, 0.0f);
  }

  public void SpiralAround(Volume volume, Vector3 center, bool hideDot)
  {
    this.PlayerManager.CanControl = false;
    this.ComeOut(hideDot);
    if (hideDot)
    {
      this.HaloPlane.Hidden = true;
      this.Visible = false;
    }
    volume.From = new Vector3(volume.From.X, Math.Max(volume.From.Y, this.PlayerManager.Position.Y + 4f / this.CameraManager.PixelsPerTrixel), volume.From.Z);
    this.PreventPoI = true;
    this.SinceStartedCameraPan = 0.0f;
    this.Behaviour = DotHost.BehaviourType.SpiralAroundWithCamera;
    this.CameraManager.Constrained = true;
    this.RoamingVolume = volume;
    this.SpiralingCenter = center;
    this.InterpolatedScale = new Vector3(50f);
    Vector3 vector3 = (this.RoamingVolume.BoundingBox.Max - this.RoamingVolume.BoundingBox.Min).Abs();
    this.InterpolatedPosition = new Vector3(vector3.X / 2f, vector3.Y, vector3.Z / 2f) + this.RoamingVolume.BoundingBox.Min;
    if (!hideDot)
      this.eMove = this.sMove.EmitAt(this.Position, true, 0.0f, 0.0f);
    this.Update(new GameTime(), true);
    this.CameraManager.SnapInterpolation();
    this.LevelMaterializer.Rowify();
  }

  public void ForceDrawOrder(int drawOrder)
  {
    this.DrawOrder = drawOrder;
    this.OnDrawOrderChanged((object) this, EventArgs.Empty);
  }

  public void RevertDrawOrder()
  {
    this.DrawOrder = 900;
    this.OnDrawOrderChanged((object) this, EventArgs.Empty);
  }

  public Vector3 Position => this.DotMesh.Position;

  public float RotationSpeed { get; set; }

  public float Opacity { get; set; }

  public DotHost.BehaviourType Behaviour
  {
    get => this._behaviour;
    set
    {
      if (this._behaviour != value && this.lastBehaviour != value && value == DotHost.BehaviourType.ThoughtBubble)
        this.UpdateBTexture();
      this.lastBehaviour = this._behaviour;
      this._behaviour = value;
    }
  }

  public Vector3 Target { get; set; }

  public string[] Dialog { get; set; }

  public float TimeToWait { get; set; }

  public Volume RoamingVolume { get; set; }

  public float ScaleFactor { get; set; }

  public float ScalePulsing { get; set; }

  public bool AlwaysShowLines { get; set; }

  public float InnerScale { get; set; }

  public bool PreventPoI { get; set; }

  public DotHost(Game game)
    : base(game)
  {
    this.DrawOrder = 900;
    this.Reset();
  }

  public override void Initialize()
  {
    base.Initialize();
    this.GTR = new GlyphTextRenderer(this.Game);
    this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
    this.Scripting.CutsceneSkipped += new Action(this.OnCutsceneSkipped);
    this.Vertices = new List<Vector4>()
    {
      new Vector4(-1f, -1f, -1f, -1f),
      new Vector4(1f, -1f, -1f, -1f),
      new Vector4(-1f, 1f, -1f, -1f),
      new Vector4(1f, 1f, -1f, -1f),
      new Vector4(-1f, -1f, 1f, -1f),
      new Vector4(1f, -1f, 1f, -1f),
      new Vector4(-1f, 1f, 1f, -1f),
      new Vector4(1f, 1f, 1f, -1f),
      new Vector4(-1f, -1f, -1f, 1f),
      new Vector4(1f, -1f, -1f, 1f),
      new Vector4(-1f, 1f, -1f, 1f),
      new Vector4(1f, 1f, -1f, 1f),
      new Vector4(-1f, -1f, 1f, 1f),
      new Vector4(1f, -1f, 1f, 1f),
      new Vector4(-1f, 1f, 1f, 1f),
      new Vector4(1f, 1f, 1f, 1f)
    };
    this.DotMesh = new Mesh()
    {
      Effect = (BaseEffect) new DotEffect(),
      Blending = new BlendingMode?(BlendingMode.Additive),
      DepthWrites = false,
      Culling = CullMode.None,
      AlwaysOnTop = true,
      Material = {
        Opacity = 0.333333343f
      }
    };
    this.RaysMesh = new Mesh()
    {
      Effect = (BaseEffect) new DefaultEffect.Textured(),
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/smooth_ray"),
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.FlareMesh = new Mesh()
    {
      Effect = (BaseEffect) new DefaultEffect.Textured(),
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/rainbow_flare"),
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    Mesh mesh = new Mesh();
    DefaultEffect.Textured textured = new DefaultEffect.Textured();
    textured.IgnoreCache = true;
    mesh.Effect = (BaseEffect) textured;
    mesh.Blending = new BlendingMode?(BlendingMode.Alphablending);
    mesh.SamplerState = SamplerStates.PointMipClamp;
    mesh.DepthWrites = false;
    mesh.AlwaysOnTop = true;
    this.VignetteMesh = mesh;
    this.VignetteMesh.AddFace(new Vector3(1f), Vector3.Zero, FaceOrientation.Front, true);
    this.BPromptMesh = new Mesh()
    {
      AlwaysOnTop = true,
      SamplerState = SamplerState.PointClamp,
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      Effect = (BaseEffect) new DefaultEffect.Textured()
    };
    this.BPromptMesh.AddFace(new Vector3(1f, 1f, 0.0f), Vector3.Zero, FaceOrientation.Front, false);
    this.FlareMesh.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, true);
    this.DotMesh.AddGroup().Geometry = (IIndexedPrimitiveCollection) (this.DotWireGeometry = new IndexedUserPrimitives<FezVertexPositionColor>(PrimitiveType.LineList));
    this.DotMesh.AddGroup().Geometry = (IIndexedPrimitiveCollection) (this.DotFacesGeometry = new IndexedUserPrimitives<FezVertexPositionColor>(PrimitiveType.TriangleList));
    this.DotWireGeometry.Vertices = new FezVertexPositionColor[16 /*0x10*/];
    for (int index = 0; index < 16 /*0x10*/; ++index)
      this.DotWireGeometry.Vertices[index].Color = new Color(1f, 1f, 1f, 1f);
    this.DotWireGeometry.Indices = new int[64 /*0x40*/]
    {
      0,
      1,
      0,
      2,
      2,
      3,
      3,
      1,
      4,
      5,
      6,
      7,
      4,
      6,
      5,
      7,
      4,
      0,
      6,
      2,
      3,
      7,
      1,
      5,
      10,
      11,
      8,
      9,
      8,
      10,
      9,
      11,
      12,
      14,
      14,
      15,
      15,
      13,
      12,
      13,
      12,
      8,
      14,
      10,
      15,
      11,
      13,
      9,
      2,
      10,
      3,
      11,
      0,
      8,
      1,
      9,
      6,
      14,
      7,
      15,
      4,
      12,
      5,
      13
    };
    this.DotFacesGeometry.Vertices = new FezVertexPositionColor[96 /*0x60*/];
    for (int index1 = 0; index1 < 4; ++index1)
    {
      for (int index2 = 0; index2 < 6; ++index2)
      {
        Vector3 vector3 = Vector3.Zero;
        switch ((index2 + index1 * 6) % 6)
        {
          case 0:
            vector3 = new Vector3(0.0f, 1f, 0.75f);
            break;
          case 1:
            vector3 = new Vector3(0.166666672f, 1f, 0.75f);
            break;
          case 2:
            vector3 = new Vector3(0.333333343f, 1f, 0.75f);
            break;
          case 3:
            vector3 = new Vector3(0.5f, 1f, 0.75f);
            break;
          case 4:
            vector3 = new Vector3(0.6666667f, 1f, 0.75f);
            break;
          case 5:
            vector3 = new Vector3(0.8333333f, 1f, 0.75f);
            break;
        }
        for (int index3 = 0; index3 < 4; ++index3)
          this.DotFacesGeometry.Vertices[index3 + index2 * 4 + index1 * 24].Color = new Color(vector3.X, vector3.Y, vector3.Z);
      }
    }
    this.FaceVertexIndices = new int[96 /*0x60*/]
    {
      0,
      2,
      3,
      1,
      1,
      3,
      7,
      5,
      5,
      7,
      6,
      4,
      4,
      6,
      2,
      0,
      0,
      4,
      5,
      1,
      2,
      6,
      7,
      3,
      8,
      10,
      11,
      9,
      9,
      11,
      15,
      13,
      13,
      15,
      14,
      12,
      12,
      14,
      10,
      8,
      8,
      12,
      13,
      9,
      10,
      14,
      15,
      11,
      0,
      1,
      9,
      8,
      0,
      2,
      10,
      8,
      2,
      3,
      11,
      10,
      3,
      1,
      9,
      11,
      4,
      5,
      13,
      12,
      6,
      7,
      15,
      14,
      4,
      6,
      14,
      12,
      5,
      7,
      15,
      13,
      4,
      0,
      8,
      12,
      6,
      2,
      10,
      14,
      3,
      7,
      15,
      11,
      1,
      5,
      13,
      9
    };
    this.DotFacesGeometry.Indices = new int[144 /*0x90*/]
    {
      0,
      2,
      1,
      0,
      3,
      2,
      4,
      6,
      5,
      4,
      7,
      6,
      8,
      10,
      9,
      8,
      11,
      10,
      12,
      14,
      13,
      12,
      15,
      14,
      16 /*0x10*/,
      17,
      18,
      16 /*0x10*/,
      18,
      19,
      20,
      22,
      21,
      20,
      23,
      22,
      24,
      26,
      25,
      24,
      27,
      26,
      28,
      30,
      29,
      28,
      31 /*0x1F*/,
      30,
      32 /*0x20*/,
      34,
      33,
      32 /*0x20*/,
      35,
      34,
      36,
      38,
      37,
      36,
      39,
      38,
      40,
      41,
      42,
      40,
      42,
      43,
      44,
      46,
      45,
      44,
      47,
      46,
      48 /*0x30*/,
      50,
      49,
      48 /*0x30*/,
      51,
      50,
      52,
      54,
      53,
      52,
      55,
      54,
      56,
      58,
      57,
      56,
      59,
      58,
      60,
      62,
      61,
      60,
      63 /*0x3F*/,
      62,
      64 /*0x40*/,
      65,
      66,
      64 /*0x40*/,
      66,
      67,
      68,
      70,
      69,
      68,
      71,
      70,
      72,
      74,
      73,
      72,
      75,
      74,
      76,
      78,
      77,
      76,
      79,
      78,
      80 /*0x50*/,
      82,
      81,
      80 /*0x50*/,
      83,
      82,
      84,
      86,
      85,
      84,
      87,
      86,
      88,
      89,
      90,
      88,
      90,
      91,
      92,
      94,
      93,
      92,
      95,
      94
    };
    this.sHide = this.CMProvider.Global.Load<SoundEffect>("Sounds/Dot/Hide");
    this.sComeOut = this.CMProvider.Global.Load<SoundEffect>("Sounds/Dot/ComeOut");
    this.sMove = this.CMProvider.Global.Load<SoundEffect>("Sounds/Dot/Move");
    this.sIdle = this.CMProvider.Global.Load<SoundEffect>("Sounds/Dot/Idle");
    this.sHeyListen = this.CMProvider.Global.Load<SoundEffect>("Sounds/Dot/HeyListen");
    this.LevelManager.LevelChanged += new Action(this.RebuildFlare);
    GamepadState.OnLayoutChanged += (EventHandler) ((o, e) =>
    {
      if (this.Behaviour != DotHost.BehaviourType.ThoughtBubble)
        return;
      this.UpdateBTexture();
    });
  }

  private void UpdateBTexture()
  {
    SpriteFont small = this.Fonts.Small;
    Vector2 vector2 = small.MeasureString(this.GTR.FillInGlyphs(" {B} ")) * FezMath.Saturate(this.Fonts.SmallFactor);
    if (this.bTexture != null)
      this.bTexture.Dispose();
    this.bTexture = new RenderTarget2D(this.GraphicsDevice, (int) vector2.X, (int) vector2.Y, false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, this.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
    this.GraphicsDevice.SetRenderTarget(this.bTexture);
    this.GraphicsDevice.PrepareDraw();
    this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
    this.spriteBatch.BeginPoint();
    this.GTR.DrawString(this.spriteBatch, small, " {B} ", new Vector2(0.0f, 0.0f), Color.White, FezMath.Saturate(this.Fonts.SmallFactor));
    this.spriteBatch.End();
    this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
    this.BPromptMesh.Texture = (Dirtyable<Texture>) (Texture) this.bTexture;
    this.BPromptMesh.Scale = new Vector3(vector2.X / 32f, vector2.Y / 32f, 1f);
    if (!Culture.IsCJK)
      return;
    this.BPromptMesh.Scale *= 0.75f;
  }

  private void OnCutsceneSkipped()
  {
    if (this.Behaviour != DotHost.BehaviourType.SpiralAroundWithCamera)
      return;
    this.EndSpiral();
  }

  private void RebuildFlare()
  {
    lock (this.LevelManager.BackgroundPlanes)
    {
      if (this.LevelManager.BackgroundPlanes.ContainsKey(-2))
        return;
      string textureName = this.LevelManager.GomezHaloName ?? "flare";
      IDictionary<int, BackgroundPlane> backgroundPlanes = this.LevelManager.BackgroundPlanes;
      BackgroundPlane backgroundPlane1 = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, textureName, false);
      backgroundPlane1.Id = -2;
      backgroundPlane1.LightMap = true;
      backgroundPlane1.AlwaysOnTop = true;
      backgroundPlane1.Billboard = true;
      backgroundPlane1.Hidden = false;
      backgroundPlane1.Filter = this.LevelManager.HaloFiltering ? new Color(0.425f, 0.425f, 0.425f, 1f) : new Color(0.5f, 0.5f, 0.5f, 1f);
      backgroundPlane1.PixelatedLightmap = !this.LevelManager.HaloFiltering;
      BackgroundPlane backgroundPlane2 = backgroundPlane1;
      this.HaloPlane = backgroundPlane1;
      BackgroundPlane backgroundPlane3 = backgroundPlane2;
      backgroundPlanes.Add(-2, backgroundPlane3);
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.Visible)
      return;
    this.Update(gameTime, false);
  }

  private void Update(GameTime gameTime, bool force)
  {
    if (!force && (this.GameState.Paused || this.GameState.Loading || this.GameState.InMenuCube || this.GameState.InFpsMode || this.GameState.TimePaused && !this.GameState.InMap))
      return;
    if (Fez.LongScreenshot)
      this.HaloPlane.Hidden = true;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    if (this.Visible)
    {
      if ((double) this.RotationSpeed == 0.0)
        this.Theta = 0.0f;
      this.Theta += (float) gameTime.ElapsedGameTime.TotalSeconds * this.RotationSpeed;
      float num1 = (float) Math.Cos((double) this.Theta);
      float m14 = (float) Math.Sin((double) this.Theta);
      Matrix matrix = new Matrix(num1, 0.0f, 0.0f, m14, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, -m14, 0.0f, 0.0f, num1);
      for (int index = 0; index < this.Vertices.Count; ++index)
      {
        Vector4 vector4 = Vector4.Transform(this.Vertices[index], matrix);
        float num2 = (float) ((((double) vector4.W + 1.0) / 3.0 * (double) this.InnerScale + 0.5) * 0.3333333432674408);
        this.DotWireGeometry.Vertices[index].Position = new Vector3(vector4.X, vector4.Y, vector4.Z) * num2;
      }
      for (int index = 0; index < this.FaceVertexIndices.Length; ++index)
        this.DotFacesGeometry.Vertices[index].Position = this.DotWireGeometry.Vertices[this.FaceVertexIndices[index]].Position;
      this.CamRotationFollow = Quaternion.Slerp(this.CamRotationFollow, this.CameraManager.Rotation, FezMath.GetReachFactor(0.05f, totalSeconds));
      float num3 = (float) (Math.Sin(gameTime.TotalGameTime.TotalSeconds / 3.0) * 0.5 + 1.0);
      this.EightShapeStep += (float) gameTime.ElapsedGameTime.TotalSeconds * num3;
      this.ToBackFollow = Vector3.Lerp(this.ToBackFollow, (float) ((this.PlayerManager.Action == ActionType.RunTurnAround ? -1 : 1) * this.PlayerManager.LookingDirection.Sign()) * this.CameraManager.Viewpoint.RightVector() * 1.5f, FezMath.GetReachFactor(0.05f, totalSeconds));
    }
    Vector3 vector2 = Vector3.Zero;
    Matrix matrix1;
    switch (this.Behaviour)
    {
      case DotHost.BehaviourType.FollowGomez:
        vector2 = this.PlayerManager.Position + this.PlayerManager.Size * Vector3.UnitY * 0.5f + Vector3.UnitY * 1.5f + (float) Math.Sin((double) this.EightShapeStep * 2.0) * Vector3.UnitY * 0.5f + (float) Math.Cos((double) this.EightShapeStep) * this.CameraManager.View.Right - this.ToBackFollow;
        break;
      case DotHost.BehaviourType.ReadyToTalk:
        vector2 = this.PlayerManager.Position + this.PlayerManager.Size * Vector3.UnitY * 0.75f + Vector3.UnitY * 0.5f + (float) Math.Sin((double) this.EightShapeStep * 2.0) * Vector3.UnitY * 0.1f + (float) Math.Cos((double) this.EightShapeStep) * this.CameraManager.View.Right * 0.1f + this.ToBackFollow;
        break;
      case DotHost.BehaviourType.ClampToTarget:
        this.InterpolatedPosition = vector2 = this.DotMesh.Position = this.Target;
        break;
      case DotHost.BehaviourType.RoamInVolume:
        Vector3 vector3_1 = (this.RoamingVolume.BoundingBox.Max - this.RoamingVolume.BoundingBox.Min).Abs() / 2f + Vector3.One;
        Vector3 vector3_2 = this.RoamingVolume.From + vector3_1;
        Vector3 vector3_3 = vector3_1;
        Vector3 vector3_4 = (float) Math.Sin((double) this.EightShapeStep * 3.0 / (double) vector3_1.Y) * Vector3.UnitY;
        double num4 = Math.Cos((double) this.EightShapeStep * 1.5 / (((double) vector3_1.X + (double) vector3_1.Z) / 3.1428570747375488));
        matrix1 = this.CameraManager.View;
        Vector3 right = matrix1.Right;
        Vector3 vector3_5 = (float) num4 * right;
        Vector3 vector3_6 = vector3_4 + vector3_5;
        Vector3 vector3_7 = vector3_3 * vector3_6;
        vector2 = vector3_2 + vector3_7;
        break;
      case DotHost.BehaviourType.MoveToTargetWithCamera:
        float num5 = Vector3.Distance(this.PanOrigin, this.Target);
        if ((double) num5 == 0.0)
          num5 = 1f;
        int num6 = this.BurrowAfterPan ? 2 : 1;
        bool flag1 = (double) this.SinceStartedCameraPan == 0.0;
        this.SinceStartedCameraPan += (float) (gameTime.ElapsedGameTime.TotalSeconds / ((double) num5 / 5.0)) * (float) num6;
        this.SinceStartedCameraPan = FezMath.Saturate(this.SinceStartedCameraPan);
        Vector3 vector3_8 = Vector3.Lerp(this.PanOrigin, this.Target, Easing.EaseInOut((double) this.SinceStartedCameraPan, EasingType.Sine));
        if (this.BurrowAfterPan)
          this.CameraManager.Center = vector3_8;
        else
          this.CameraManager.Center = Vector3.Lerp(this.CameraManager.Center, vector3_8, FezMath.GetReachFactor(0.05f, totalSeconds));
        if ((double) this.SinceStartedCameraPan >= 1.0)
          this.EndMoveTo();
        vector2 = vector3_8 + (float) Math.Sin((double) this.EightShapeStep * 2.0) * Vector3.UnitY / 2f + (float) Math.Cos((double) this.EightShapeStep) * this.CameraManager.View.Right / 2f;
        if (this.eMove != null && !this.eMove.Dead)
        {
          Vector3 vector3_9 = vector2;
          float num7 = ((vector3_9 - this.lastRelativePosition) * (this.CameraManager.InverseView.Right + Vector3.UnitY)).Length();
          if (!flag1)
            this.eMove.VolumeFactor = MathHelper.Lerp(this.eMove.VolumeFactor, (float) ((double) FezMath.Saturate(num7 * 10f) * 0.75 + 0.25), FezMath.GetReachFactor(0.1f, totalSeconds));
          this.lastRelativePosition = vector3_9;
          break;
        }
        break;
      case DotHost.BehaviourType.WaitAtTarget:
        vector2 = this.Target + (float) Math.Sin((double) this.EightShapeStep * 2.0) * Vector3.UnitY / 3f + (float) Math.Cos((double) this.EightShapeStep) * this.CameraManager.View.Right / 3f;
        this.CameraManager.Center = Vector3.Lerp(this.CameraManager.Center, this.Target, FezMath.GetReachFactor(0.075f, totalSeconds));
        break;
      case DotHost.BehaviourType.SpiralAroundWithCamera:
        Vector3 vector3_10 = (this.RoamingVolume.BoundingBox.Max - this.RoamingVolume.BoundingBox.Min).Abs();
        float num8 = (float) (((double) this.RoamingVolume.BoundingBox.Max.Y - (double) this.PlayerManager.Position.Y) * 0.89999997615814209);
        if ((double) num8 == 0.0)
          num8 = 1f;
        bool flag2 = (double) this.SinceStartedCameraPan == 0.0;
        this.SinceStartedCameraPan += (float) (gameTime.ElapsedGameTime.TotalSeconds / (double) num8 * 2.0);
        float linearStep = Easing.EaseOut((double) FezMath.Saturate(this.SinceStartedCameraPan), EasingType.Sine);
        double num9 = Math.Round((double) num8 / 20.0) * 6.2831854820251465;
        int distance = this.CameraManager.Viewpoint.GetDistance(Viewpoint.Front);
        vector2 = new Vector3((float) (Math.Sin((double) Easing.EaseIn((double) linearStep, EasingType.Sine) * num9 - (double) distance * 1.5707963705062866) * (double) vector3_10.X / 2.0 + (double) vector3_10.X / 2.0), vector3_10.Y * (1f - linearStep), (float) (Math.Cos((double) Easing.EaseIn((double) linearStep, EasingType.Sine) * num9 - (double) distance * 1.5707963705062866) * (double) vector3_10.Z / 2.0 + (double) vector3_10.Z / 2.0)) + this.RoamingVolume.BoundingBox.Min;
        this.Target = new Vector3(this.SpiralingCenter.X, vector2.Y, this.SpiralingCenter.Z);
        Vector3 vector3_11 = Vector3.Normalize(new Vector3(vector2.X, 0.0f, vector2.Z) - (this.RoamingVolume.BoundingBox.Min + vector3_10 / 2f) * FezMath.XZMask);
        if ((double) linearStep > 0.75)
        {
          float amount = Easing.EaseInOut(((double) linearStep - 0.75) / 0.25, EasingType.Sine);
          this.Target = Vector3.Lerp(this.Target, this.PlayerManager.Position + Vector3.Up * 4f / this.CameraManager.PixelsPerTrixel, amount);
          vector2 = this.Target + this.CameraManager.Viewpoint.RightVector() * amount;
        }
        this.CameraManager.Center = this.Target;
        this.CameraManager.Direction = vector3_11;
        if ((double) linearStep < 0.1)
        {
          this.Target = new Vector3(vector3_10.X / 2f, vector3_10.Y * (1f - linearStep), vector3_10.Z / 2f) + this.RoamingVolume.BoundingBox.Min;
          vector2 = this.Target;
        }
        if ((double) linearStep < 0.75)
        {
          Vector3 vector3_12 = vector2;
          matrix1 = this.CameraManager.InverseView;
          Vector3 vector3_13 = matrix1.Right * 5f;
          vector2 = vector3_12 + vector3_13;
        }
        if (this.eMove != null && !this.eMove.Dead)
        {
          Vector3 vector3_14 = vector2 - this.CameraManager.InterpolatedCenter;
          Vector3 vector3_15 = vector3_14 - this.lastRelativePosition;
          matrix1 = this.CameraManager.InverseView;
          Vector3 vector3_16 = matrix1.Right + Vector3.UnitY;
          float num10 = (vector3_15 * vector3_16).Length();
          if (!flag2)
            this.eMove.VolumeFactor = MathHelper.Lerp(this.eMove.VolumeFactor, (float) ((double) FezMath.Saturate(num10 * 3f) * 0.75 + 0.25), FezMath.GetReachFactor(0.1f, totalSeconds));
          this.lastRelativePosition = vector3_14;
        }
        if ((double) linearStep >= 1.0)
        {
          this.EndSpiral();
          break;
        }
        break;
      case DotHost.BehaviourType.ThoughtBubble:
        vector2 = this.PlayerManager.Position + this.PlayerManager.Size * Vector3.UnitY * 0.75f + Vector3.UnitY * 0.5f + (float) Math.Sin((double) this.EightShapeStep * 2.0) * Vector3.UnitY * 0.1f + (float) Math.Cos((double) this.EightShapeStep) * this.CameraManager.View.Right * 0.1f + this.ToBackFollow * 1.25f;
        break;
    }
    if (this.Burrowing || this.ComingOut)
    {
      float num11 = Vector3.Distance(this.PlayerManager.Position, vector2);
      if ((double) num11 == 0.0)
        num11 = 1f;
      this.SinceStartedTransition += (float) (gameTime.ElapsedGameTime.TotalSeconds / ((double) num11 / 20.0));
    }
    Vector3 vector3_17 = this.Behaviour != DotHost.BehaviourType.ThoughtBubble ? new Vector3(MathHelper.Lerp(this.ScaleFactor * 0.75f, (float) ((double) this.ScaleFactor * 0.75 + Math.Sin((double) this.EightShapeStep * 4.0 / 3.0) * 0.20000000298023224 * ((double) this.ScaleFactor + 1.0) / 2.0), this.ScalePulsing)) : (this.DestinationVignette == null ? (this.FaceButton != DotFaceButton.Up ? new Vector3(0.825f * this.ScaleFactor) : new Vector3(1f * this.ScaleFactor)) : new Vector3(2f));
    if (this.Visible)
      this.DotMesh.Rotation = this.CamRotationFollow * Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
    float amount1 = Easing.EaseInOut((double) FezMath.Saturate(this.SinceStartedTransition), EasingType.Linear);
    if (this.Burrowing)
    {
      vector2 = Vector3.Lerp(vector2, this.PlayerManager.Position, amount1);
      vector3_17 = Vector3.Lerp(vector3_17, Vector3.Zero, amount1);
      if (this.eIdle != null)
        this.eIdle.VolumeFactor = amount1;
      if ((double) this.InterpolatedScale.X <= 0.01)
      {
        if (this.eIdle != null && this.eIdle.Cue != null && !this.eIdle.Cue.IsDisposed)
          this.eIdle.Cue.Stop();
        this.eIdle = (SoundEmitter) null;
        this.SinceStartedTransition = 0.0f;
        this.Reset();
        this.Burrowing = false;
      }
    }
    else if (this.ComingOut && this.Behaviour != DotHost.BehaviourType.SpiralAroundWithCamera)
    {
      vector2 = Vector3.Lerp(vector2, this.PlayerManager.Position, 1f - amount1);
      vector3_17 = Vector3.Lerp(vector3_17, Vector3.Zero, 1f - amount1);
      if (this.eIdle != null)
        this.eIdle.VolumeFactor = amount1;
      if ((double) amount1 >= 1.0)
      {
        this.ComingOut = false;
        this.SinceStartedTransition = 0.0f;
      }
    }
    float oldFactor1 = this.Behaviour == DotHost.BehaviourType.ThoughtBubble ? 0.2f : 0.05f;
    this.InterpolatedPosition = Vector3.Lerp(this.InterpolatedPosition, vector2, FezMath.GetReachFactor(oldFactor1, totalSeconds));
    float oldFactor2 = this.Behaviour == DotHost.BehaviourType.ThoughtBubble ? 0.1f : (this.lastBehaviour == DotHost.BehaviourType.ThoughtBubble ? 0.075f : 0.05f);
    this.InterpolatedScale = Vector3.Lerp(this.InterpolatedScale, vector3_17, FezMath.GetReachFactor(oldFactor2, totalSeconds));
    if (this.Visible)
    {
      this.DotMesh.Position = this.InterpolatedPosition;
      this.DotMesh.Scale = this.InterpolatedScale;
    }
    float viewScale = this.GraphicsDevice.GetViewScale();
    float num12 = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
    if (this.GameState.InMap)
    {
      this.DotMesh.Scale *= this.CameraManager.Radius / 16f / viewScale / num12;
      float y = this.CameraManager.Radius / 6f / viewScale / num12;
      Mesh dotMesh = this.DotMesh;
      Vector3 center = this.CameraManager.Center;
      matrix1 = this.CameraManager.InverseView;
      Vector3 vector3_18 = matrix1.Left * y * this.CameraManager.AspectRatio;
      Vector3 vector3_19 = center + vector3_18 - new Vector3(0.0f, y, 0.0f);
      dotMesh.Position = vector3_19;
    }
    if (this.Behaviour == DotHost.BehaviourType.ThoughtBubble)
    {
      this.VignetteMesh.Position = this.DotMesh.Position;
      this.VignetteMesh.Rotation = this.CameraManager.Rotation;
      if (this.DestinationVignette != null)
      {
        this.VignetteMesh.Scale = new Vector3(2.65f);
        this.VignetteMesh.SamplerState = SamplerState.PointClamp;
        this.VignetteMesh.Texture = this.DestinationVignetteSony == null || GamepadState.Layout == GamepadState.GamepadLayout.Xbox360 ? (Dirtyable<Texture>) (Texture) this.DestinationVignette : (Dirtyable<Texture>) (Texture) this.DestinationVignetteSony;
        this.VignetteMesh.TextureMatrix.Set(Matrix.Identity);
      }
      else if (this.FaceButton == DotFaceButton.B)
      {
        this.BPromptMesh.Position = this.DotMesh.Position + this.CameraManager.Viewpoint.RightVector() * 0.25f + new Vector3(0.0f, -0.925f, 0.0f);
        this.BPromptMesh.Rotation = this.CameraManager.Rotation;
      }
    }
    if (this.eMove != null && !this.eMove.Dead)
      this.eMove.Position = this.Position;
    if (this.eIdle != null && !this.eIdle.Dead)
      this.eIdle.Position = this.Position;
    if (this.eComeOut != null && !this.eComeOut.Dead)
      this.eComeOut.Position = this.Position;
    if (this.eHide != null && !this.eHide.Dead)
      this.eHide.Position = this.Position;
    if (this.eHey != null && !this.eHey.Dead)
      this.eHey.Position = this.Position;
    if (!this.DrawRays || !this.Visible)
      return;
    this.UpdateRays((float) gameTime.ElapsedGameTime.TotalSeconds);
  }

  private void EndSpiral()
  {
    Vector3 vector3 = this.PlayerManager.Position + Vector3.Up * 4f / this.CameraManager.PixelsPerTrixel + this.CameraManager.Viewpoint.RightVector();
    this.PlayerManager.CanControl = true;
    if (this.eMove != null && !this.eMove.Dead)
    {
      this.eMove.Cue.Stop();
      this.eMove = (SoundEmitter) null;
    }
    this.RoamingVolume = (Volume) null;
    if (!this.Visible)
      this.Hidden = true;
    this.Target = vector3;
    this.Behaviour = DotHost.BehaviourType.ReadyToTalk;
    this.CameraManager.Direction = -this.CameraManager.Viewpoint.ForwardVector().MaxClampXZ();
    this.LevelMaterializer.CullInstances();
    this.CameraManager.Constrained = false;
    this.LevelMaterializer.UnRowify();
  }

  private void EndMoveTo()
  {
    this.eMove.FadeOutAndDie(2f, false);
    this.eMove = (SoundEmitter) null;
    if (this.BurrowAfterPan)
      this.Burrow();
    this.Behaviour = DotHost.BehaviourType.WaitAtTarget;
  }

  private void UpdateRays(float elapsedSeconds)
  {
    if (RandomHelper.Probability(30.0 / 17.0 * (double) elapsedSeconds))
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
    bool flag = (double) this.CameraManager.ViewTransitionStep != 0.0 && this.CameraManager.Viewpoint.IsOrthographic() && this.CameraManager.LastViewpoint.IsOrthographic();
    float num1 = Easing.EaseOut(1.0 - (double) this.CameraManager.ViewTransitionStep, EasingType.Quadratic) * (float) this.CameraManager.Viewpoint.GetDistance(this.CameraManager.LastViewpoint);
    for (int index = this.RaysMesh.Groups.Count - 1; index >= 0; --index)
    {
      Group group = this.RaysMesh.Groups[index];
      DotHost.RayState customData = group.CustomData as DotHost.RayState;
      customData.Age += elapsedSeconds * 0.15f;
      float num2 = Easing.EaseOut(Math.Sin((double) customData.Age * 6.2831854820251465 - 1.5707963705062866) * 0.5 + 0.5, EasingType.Quadratic);
      group.Material.Diffuse = new Vector3(num2 * 0.0375f) + customData.Tint.ToVector3() * 0.075f * num2;
      float speed = customData.Speed;
      if (flag)
        speed *= (float) (1.0 + 10.0 * (double) num1);
      group.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Forward, (float) ((double) elapsedSeconds * (double) speed * 0.30000001192092896));
      group.Scale = new Vector3((float) ((double) num2 * 0.75 + 0.25), (float) ((double) num2 * 0.5 + 0.5), 1f);
      if ((double) customData.Age > 1.0)
        this.RaysMesh.RemoveGroupAt(index);
    }
    this.FlareMesh.Position = this.RaysMesh.Position = this.DotMesh.Position;
    this.FlareMesh.Rotation = this.RaysMesh.Rotation = this.CameraManager.Rotation;
    this.RaysMesh.Scale = this.DotMesh.Scale * 0.5f;
    float num3 = MathHelper.Lerp(this.DotMesh.Scale.X, 1f, 0.325f);
    this.FlareMesh.Scale = new Vector3(MathHelper.Lerp(num3, (float) Math.Pow((double) num3 * 2.0, 1.5), this.Opacity));
    this.FlareMesh.Material.Diffuse = new Vector3(0.25f * MathHelper.Lerp(this.DotMesh.Scale.X, 0.75f, 0.75f) * FezMath.Saturate(this.Opacity * 2f));
    this.HaloPlane.Position = this.DotMesh.Position;
    this.HaloPlane.Scale = this.Visible ? this.DotMesh.Scale * 2f : new Vector3(0.0f);
    if (this.LevelManager.HaloFiltering)
      return;
    this.HaloPlane.Position = (this.HaloPlane.Position * 16f).Round() / 16f;
    this.HaloPlane.Scale = Vector3.Clamp(this.HaloPlane.Scale, Vector3.Zero, Vector3.One);
  }

  public override void Draw(GameTime gameTime)
  {
    this.Update(gameTime, false);
    if (Fez.LongScreenshot && this.eIdle != null)
      this.eIdle.VolumeFactor = 0.0f;
    if (this.GameState.Loading || this.GameState.InMenuCube || Fez.LongScreenshot)
      return;
    int num1 = this.Behaviour == DotHost.BehaviourType.ThoughtBubble ? 1 : 0;
    this.FlareMesh.Draw();
    if ((double) this.Opacity == 1.0 && this.DrawRays)
      this.RaysMesh.Draw();
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Dot));
    (this.DotMesh.Effect as DotEffect).UpdateHueOffset(gameTime.ElapsedGameTime);
    this.DotMesh.Blending = new BlendingMode?(BlendingMode.Alphablending);
    this.DotMesh.Material.Diffuse = new Vector3(0.0f);
    if (num1 != 0 && (this.FaceButton == DotFaceButton.Up || this.DestinationVignette != null))
    {
      this.DotMesh.Material.Opacity = 1f;
      this.DotMesh.Draw();
    }
    else
    {
      this.DotMesh.Material.Opacity = (double) this.Opacity > 0.5 ? this.Opacity * 0.25f : 0.0f;
      this.DotMesh.Draw();
    }
    TimeSpan timeSpan;
    if (num1 != 0 && !this.GameState.InMap)
    {
      if (this.DestinationVignette != null)
      {
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.Dot);
        this.VignetteMesh.Draw();
      }
      else if (this.FaceButton == DotFaceButton.B)
      {
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
        this.BPromptMesh.Draw();
        Material material = this.BPromptMesh.Material;
        double opacity = (double) material.Opacity;
        timeSpan = gameTime.ElapsedGameTime;
        double num2 = timeSpan.TotalSeconds * 3.0;
        material.Opacity = (float) (opacity + num2);
        if ((double) this.BPromptMesh.Material.Opacity > 1.0)
          this.BPromptMesh.Material.Opacity = 1f;
      }
      else if (this.FaceButton == DotFaceButton.Up)
      {
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.Dot);
        this.VignetteMesh.Texture = (Dirtyable<Texture>) (Texture) this.GTR.GetReplacedGlyphTexture("{UP}");
        this.VignetteMesh.Scale = new Vector3(0.75f);
        this.VignetteMesh.Draw();
      }
    }
    else
      this.BPromptMesh.Material.Opacity = -1.5f;
    if (num1 == 0 || this.FaceButton != DotFaceButton.Up && this.DestinationVignette == null)
    {
      this.DotMesh.Groups[0].Enabled = true;
      this.DotMesh.Groups[1].Enabled = false;
      this.DotMesh.Blending = new BlendingMode?(BlendingMode.Additive);
      timeSpan = gameTime.TotalGameTime;
      float num3 = (float) Math.Pow(Math.Sin(timeSpan.TotalSeconds * 2.0) * 0.5 + 0.5, 3.0);
      this.DotMesh.Material.Opacity = 1f;
      this.DotMesh.Material.Diffuse = new Vector3(this.AlwaysShowLines ? this.Opacity : num3 * 0.5f * this.Opacity);
      this.DotMesh.Draw();
      this.DotMesh.Groups[0].Enabled = false;
      this.DotMesh.Groups[1].Enabled = true;
      this.DotMesh.Material.Diffuse = new Vector3(this.Opacity);
      this.DotMesh.Draw();
    }
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public IFontManager Fonts { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  internal IScriptingManager Scripting { get; set; }

  public enum BehaviourType
  {
    FollowGomez,
    ReadyToTalk,
    ClampToTarget,
    RoamInVolume,
    MoveToTargetWithCamera,
    WaitAtTarget,
    SpiralAroundWithCamera,
    ThoughtBubble,
  }

  internal class RayState
  {
    public float Age;
    public readonly float Speed = RandomHelper.Between(0.10000000149011612, 1.5);
    public readonly Color Tint = Util.ColorFromHSV((double) RandomHelper.Between(0.0, 360.0), 1.0, 1.0);
  }
}
