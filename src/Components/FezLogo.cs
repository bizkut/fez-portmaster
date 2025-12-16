// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FezLogo
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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

public class FezLogo : DrawableGameComponent
{
  private Mesh LogoMesh;
  private Mesh WireMesh;
  private DefaultEffect FezEffect;
  public StarField Starfield;
  public float SinceStarted;
  private bool inverted;
  public Mesh InLogoMesh;
  private SoundEffect sGlitch1;
  private SoundEffect sGlitch2;
  private SoundEffect sGlitch3;
  public bool IsDisposed;
  private float untilGlitch;
  private int forFrames;
  private float[] glitchTilt = new float[3];
  private Vector3[] glitchScale = new Vector3[3];
  private float[] glitchOpacity = new float[3];
  public float Zoom;

  public bool Inverted
  {
    get => this.inverted;
    set
    {
      this.inverted = value;
      this.Enabled = true;
      this.SinceStarted = this.inverted ? 6f : 0.0f;
    }
  }

  public bool Glitched { get; set; }

  public bool DoubleTime { get; set; }

  public bool HalfSpeed { get; set; }

  public bool IsFullscreen { get; set; }

  public Texture LogoTexture { get; set; }

  public float LogoTextureXFade { get; set; }

  public FezLogo(Game game)
    : base(game)
  {
    this.Visible = false;
    this.DrawOrder = 2006;
  }

  public float Opacity
  {
    set
    {
      this.LogoMesh.Material.Opacity = this.WireMesh.Material.Opacity = value;
      this.Starfield.Opacity = value;
    }
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.LogoMesh.Dispose();
    this.WireMesh.Dispose();
    this.IsDisposed = true;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LogoMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false,
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    this.WireMesh = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(0.0f, 0.0f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(0.0f, 1f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(0.0f, 2f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(1f, 2f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(0.0f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(1f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(2f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(0.0f, 0.0f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(1f, 0.0f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(2f, 0.0f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(0.0f, 1f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(0.0f, 2f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(1f, 2f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(0.0f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(1f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(4f, 0.0f, 0.0f) + new Vector3(2f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(0.0f, 0.0f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(1f, 0.0f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(2f, 0.0f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(0.0f, 1f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(1f, 1f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(2f, 2f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(0.0f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(1f, 3f, 0.0f), Color.Black, false);
    this.LogoMesh.AddColoredBox(Vector3.One, new Vector3(8f, 0.0f, 0.0f) + new Vector3(2f, 3f, 0.0f), Color.Black, false);
    Group group1 = this.WireMesh.AddGroup();
    IndexedUserPrimitives<FezVertexPositionColor> indexedUserPrimitives1 = new IndexedUserPrimitives<FezVertexPositionColor>(PrimitiveType.LineList);
    IndexedUserPrimitives<FezVertexPositionColor> indexedUserPrimitives2 = indexedUserPrimitives1;
    group1.Geometry = (IIndexedPrimitiveCollection) indexedUserPrimitives2;
    indexedUserPrimitives1.Vertices = new FezVertexPositionColor[16 /*0x10*/]
    {
      new FezVertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, 0.0f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(2f, 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(2f, 3f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(3f, 3f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(3f, 4f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(0.0f, 4f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(0.0f, 0.0f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, 0.0f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(1f, 2f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(2f, 2f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(2f, 3f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(3f, 3f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(3f, 4f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(0.0f, 4f, 1f), Color.White)
    };
    indexedUserPrimitives1.Indices = new int[50]
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
      6,
      6,
      7,
      7,
      0,
      8,
      9,
      9,
      10,
      10,
      11,
      11,
      12,
      12,
      13,
      13,
      14,
      14,
      15,
      15,
      8,
      0,
      8,
      1,
      9,
      2,
      10,
      3,
      11,
      4,
      12,
      5,
      13,
      6,
      14,
      7,
      15,
      0,
      8
    };
    Group group2 = this.WireMesh.AddGroup();
    IndexedUserPrimitives<FezVertexPositionColor> indexedUserPrimitives3 = new IndexedUserPrimitives<FezVertexPositionColor>(PrimitiveType.LineList);
    IndexedUserPrimitives<FezVertexPositionColor> indexedUserPrimitives4 = indexedUserPrimitives3;
    group2.Geometry = (IIndexedPrimitiveCollection) indexedUserPrimitives4;
    indexedUserPrimitives3.Vertices = new FezVertexPositionColor[20]
    {
      new FezVertexPositionColor(new Vector3(4f, 0.0f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 0.0f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 1f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(5f, 1f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(5f, 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(6f, 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(6f, 3f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 3f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 4f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(4f, 4f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(4f, 0.0f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 0.0f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 1f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(5f, 1f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(5f, 2f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(6f, 2f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(6f, 3f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 3f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(7f, 4f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(4f, 4f, 1f), Color.White)
    };
    indexedUserPrimitives3.Indices = new int[60]
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
      6,
      6,
      7,
      7,
      8,
      8,
      9,
      9,
      0,
      10,
      11,
      11,
      12,
      12,
      13,
      13,
      14,
      14,
      15,
      15,
      16 /*0x10*/,
      16 /*0x10*/,
      17,
      17,
      18,
      18,
      19,
      19,
      10,
      0,
      10,
      1,
      11,
      2,
      12,
      3,
      13,
      4,
      14,
      5,
      15,
      6,
      16 /*0x10*/,
      7,
      17,
      8,
      18,
      9,
      19
    };
    Group group3 = this.WireMesh.AddGroup();
    IndexedUserPrimitives<FezVertexPositionColor> indexedUserPrimitives5 = new IndexedUserPrimitives<FezVertexPositionColor>(PrimitiveType.LineList);
    IndexedUserPrimitives<FezVertexPositionColor> indexedUserPrimitives6 = indexedUserPrimitives5;
    group3.Geometry = (IIndexedPrimitiveCollection) indexedUserPrimitives6;
    indexedUserPrimitives5.Vertices = new FezVertexPositionColor[22]
    {
      new FezVertexPositionColor(new Vector3(8f, 0.0f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 0.0f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 1f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(10f, 1f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(10f, 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 4f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(8f, 4f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(8f, 3f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(10f, 3f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(8f, 2f, 0.0f), Color.White),
      new FezVertexPositionColor(new Vector3(8f, 0.0f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 0.0f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 1f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(10f, 1f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(10f, 2f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 2f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(11f, 4f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(8f, 4f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(8f, 3f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(10f, 3f, 1f), Color.White),
      new FezVertexPositionColor(new Vector3(8f, 2f, 1f), Color.White)
    };
    indexedUserPrimitives5.Indices = new int[70]
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
      6,
      6,
      7,
      7,
      8,
      8,
      9,
      9,
      4,
      4,
      10,
      10,
      0,
      11,
      12,
      12,
      13,
      13,
      14,
      14,
      15,
      15,
      16 /*0x10*/,
      16 /*0x10*/,
      17,
      17,
      18,
      18,
      19,
      19,
      20,
      20,
      15,
      15,
      21,
      21,
      11,
      0,
      11,
      1,
      12,
      2,
      13,
      3,
      14,
      4,
      15,
      5,
      16 /*0x10*/,
      6,
      17,
      7,
      18,
      8,
      19,
      9,
      20,
      10,
      21
    };
    Mesh wireMesh = this.WireMesh;
    Mesh logoMesh = this.LogoMesh;
    Vector3 vector3_1 = new Vector3(-5.5f, -2f, -0.5f);
    Vector3 vector3_2 = vector3_1;
    logoMesh.Position = vector3_2;
    Vector3 vector3_3 = vector3_1;
    wireMesh.Position = vector3_3;
    this.WireMesh.BakeTransform<FezVertexPositionColor>();
    this.LogoMesh.BakeTransform<FezVertexPositionColor>();
    DrawActionScheduler.Schedule((Action) (() => this.WireMesh.Effect = this.LogoMesh.Effect = (BaseEffect) (this.FezEffect = (DefaultEffect) new DefaultEffect.VertexColored())));
    ContentManager contentManager = this.CMProvider.Get(CM.Menu);
    this.sGlitch1 = contentManager.Load<SoundEffect>("Sounds/Intro/FezLogoGlitch1");
    this.sGlitch2 = contentManager.Load<SoundEffect>("Sounds/Intro/FezLogoGlitch2");
    this.sGlitch3 = contentManager.Load<SoundEffect>("Sounds/Intro/FezLogoGlitch3");
    ServiceHelper.AddComponent((IGameComponent) (this.Starfield = new StarField(this.Game)));
    this.Starfield.Opacity = 0.0f;
    this.LogoMesh.Material.Opacity = 0.0f;
    this.untilGlitch = RandomHelper.Between(0.3333333432674408, 1.0);
  }

  public bool TransitionStarted { get; set; }

  public override void Update(GameTime gameTime)
  {
    if (this.IsDisposed || this.GameState.Paused)
      return;
    TimeSpan elapsedGameTime;
    if (this.TransitionStarted)
    {
      double sinceStarted = (double) this.SinceStarted;
      elapsedGameTime = gameTime.ElapsedGameTime;
      double num = elapsedGameTime.TotalSeconds * (this.Inverted ? -0.10000000149011612 : (this.DoubleTime ? 2.0 : (this.HalfSpeed ? 0.75 : 1.0)));
      this.SinceStarted = (float) (sinceStarted + num);
    }
    float linearStep = Math.Min(Easing.EaseIn((double) FezMath.Saturate(this.SinceStarted / 5f), EasingType.Sine), 0.999f);
    this.Zoom = linearStep;
    if ((double) this.LogoMesh.Material.Opacity == 1.0 && this.Glitched && (double) linearStep <= 0.75)
    {
      double untilGlitch = (double) this.untilGlitch;
      elapsedGameTime = gameTime.ElapsedGameTime;
      double totalSeconds = elapsedGameTime.TotalSeconds;
      this.untilGlitch = (float) (untilGlitch - totalSeconds);
      if ((double) this.untilGlitch <= 0.0)
      {
        this.untilGlitch = RandomHelper.Between(0.3333333432674408, 2.0);
        this.glitchTilt[0] = RandomHelper.Between(0.0, 1.0);
        this.glitchTilt[1] = RandomHelper.Between(0.0, 1.0);
        this.glitchTilt[2] = RandomHelper.Between(0.0, 1.0);
        this.glitchOpacity[0] = RandomHelper.Between(0.25, 1.0);
        this.glitchOpacity[1] = RandomHelper.Between(0.25, 1.0);
        this.glitchOpacity[2] = RandomHelper.Between(0.25, 1.0);
        this.glitchScale[0] = new Vector3(RandomHelper.Between(0.75, 1.5), RandomHelper.Between(0.75, 1.5), RandomHelper.Between(0.75, 1.5));
        this.glitchScale[1] = new Vector3(RandomHelper.Between(0.75, 1.5), RandomHelper.Between(0.75, 1.5), RandomHelper.Between(0.75, 1.5));
        this.glitchScale[2] = new Vector3(RandomHelper.Between(0.75, 1.5), RandomHelper.Between(0.75, 1.5), RandomHelper.Between(0.75, 1.5));
        this.forFrames = RandomHelper.Random.Next(1, 7);
        if (RandomHelper.Probability(0.3333333432674408))
          this.sGlitch1.Emit();
        if (RandomHelper.Probability(0.5))
          this.sGlitch2.Emit();
        else
          this.sGlitch3.Emit();
      }
    }
    float aspectRatio = this.GraphicsDevice.Viewport.AspectRatio;
    Mesh wireMesh = this.WireMesh;
    Mesh logoMesh = this.LogoMesh;
    Vector3 vector3_1 = new Vector3(0.0f, -linearStep, 0.0f);
    Vector3 vector3_2 = vector3_1;
    logoMesh.Position = vector3_2;
    Vector3 vector3_3 = vector3_1;
    wireMesh.Position = vector3_3;
    if (this.FezEffect != null)
      this.FezEffect.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic((float) (10.0 * (double) aspectRatio * (1.0 - (double) linearStep)), (float) (10.0 * (1.0 - (double) linearStep)), 0.1f, 100f));
    if (this.Starfield != null)
    {
      this.Starfield.AdditionalZoom = Easing.EaseInOut((double) linearStep, EasingType.Quadratic);
      this.Starfield.HasZoomed = true;
    }
    if (this.Inverted || (double) linearStep < 0.99900001287460327)
      return;
    this.IsFullscreen = true;
  }

  public override void Draw(GameTime gameTime) => this.DoDraw();

  private void DoDraw()
  {
    if (this.IsDisposed)
      return;
    if (Fez.LongScreenshot)
      this.TargetRenderer.DrawFullscreen(Color.White);
    if (this.forFrames == 0)
    {
      float amount = 1f - Easing.EaseInOut((double) FezMath.Saturate(this.SinceStarted / 5f), EasingType.Sine);
      Mesh wireMesh = this.WireMesh;
      Mesh logoMesh = this.LogoMesh;
      Vector3 vector3_1 = new Vector3(1f, 1f, 1f);
      Vector3 vector3_2 = vector3_1;
      logoMesh.Scale = vector3_2;
      Vector3 vector3_3 = vector3_1;
      wireMesh.Scale = vector3_3;
      this.FezEffect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Lerp(new Vector3(0.0f, 0.0f, 10f), new Vector3(-10f, 10f, 10f), amount), Vector3.Zero, Vector3.Up));
      this.DrawMaskedLogo();
    }
    else
    {
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.Red | ColorWriteChannels.Green);
      this.LogoMesh.Scale = this.glitchScale[0];
      this.LogoMesh.Material.Opacity = this.glitchOpacity[0];
      this.FezEffect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Lerp(new Vector3(0.0f, 0.0f, 10f), new Vector3(-10f, 10f, 10f), this.glitchTilt[0]), Vector3.Zero, Vector3.Up));
      this.DrawMaskedLogo();
      this.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0.0f, 0);
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.Green | ColorWriteChannels.Blue);
      this.LogoMesh.Scale = this.glitchScale[1];
      this.LogoMesh.Material.Opacity = this.glitchOpacity[1];
      this.FezEffect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Lerp(new Vector3(0.0f, 0.0f, 10f), new Vector3(-10f, 10f, 10f), this.glitchTilt[1]), Vector3.Zero, Vector3.Up));
      this.DrawMaskedLogo();
      this.GraphicsDevice.Clear(ClearOptions.Stencil, Color.Black, 0.0f, 0);
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.Red | ColorWriteChannels.Blue);
      this.LogoMesh.Scale = this.glitchScale[2];
      this.LogoMesh.Material.Opacity = this.glitchOpacity[2];
      this.FezEffect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Lerp(new Vector3(0.0f, 0.0f, 10f), new Vector3(-10f, 10f, 10f), this.glitchTilt[2]), Vector3.Zero, Vector3.Up));
      this.DrawMaskedLogo();
      this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
      this.LogoMesh.Material.Opacity = 1f;
      --this.forFrames;
    }
    if (this.LogoTexture != null)
    {
      this.TargetRenderer.DrawFullscreen(this.LogoTexture, new Color(1f, 1f, 1f, this.LogoTextureXFade));
      this.Starfield.Opacity = 1f - this.LogoTextureXFade;
    }
    if (this.InLogoMesh != null)
      this.InLogoMesh.Draw();
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    this.WireMesh.Draw();
  }

  private void DrawMaskedLogo()
  {
    this.GraphicsDevice.PrepareStencilReadWrite(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.BlackHoles);
    this.LogoMesh.Draw();
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.BlackHoles);
    this.Starfield.Draw();
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
