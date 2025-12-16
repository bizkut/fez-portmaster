// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PolytronLogo
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class PolytronLogo : DrawableGameComponent
{
  private const int Detail = 100;
  private const float StripThickness = 0.091f;
  private const float Zoom = 0.7f;
  private static readonly Color[] StripColors = new Color[4]
  {
    new Color(0, 170, (int) byte.MaxValue),
    new Color((int) byte.MaxValue, (int) byte.MaxValue, 0),
    new Color((int) byte.MaxValue, 106, 0),
    new Color(0, 0, 0)
  };
  private Mesh LogoMesh;
  private Texture2D PolytronText;
  private SoundEffect sPolytron;
  private SoundEmitter iPolytron;
  private SpriteBatch spriteBatch;
  private float SinceStarted;

  public PolytronLogo(Game game)
    : base(game)
  {
    this.Visible = false;
    this.Enabled = false;
  }

  public float Opacity { get; set; }

  public override void Initialize()
  {
    base.Initialize();
    this.LogoMesh = new Mesh() { AlwaysOnTop = true };
    for (int index1 = 0; index1 < 4; ++index1)
    {
      FezVertexPositionColor[] vertices = new FezVertexPositionColor[202];
      for (int index2 = 0; index2 < vertices.Length; ++index2)
        vertices[index2] = new FezVertexPositionColor(Vector3.Zero, PolytronLogo.StripColors[index1]);
      this.LogoMesh.AddGroup().Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionColor>(vertices, Enumerable.Range(0, vertices.Length).ToArray<int>(), PrimitiveType.TriangleStrip);
    }
    float viewScale = this.GraphicsDevice.GetViewScale();
    float num1 = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
    Viewport viewport = this.GraphicsDevice.Viewport;
    float num2 = (float) viewport.Height / (720f * viewScale);
    viewport = this.GraphicsDevice.Viewport;
    int width = viewport.Width;
    viewport = this.GraphicsDevice.Viewport;
    int height = viewport.Height;
    this.LogoMesh.Position = new Vector3(-0.1975f / num1, -0.25f / num2, 0.0f);
    this.LogoMesh.Scale = new Vector3(new Vector2(500f) * viewScale / new Vector2((float) width, (float) height), 1f);
    bool is1440 = (double) viewScale >= 1.5;
    this.sPolytron = this.CMProvider.Get(CM.Intro).Load<SoundEffect>("Sounds/Intro/PolytronJingle");
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.PolytronText = this.CMProvider.Get(CM.Intro).Load<Texture2D>("Other Textures/splash/polytron" + (is1440 ? "_1440" : ""));
      this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
      Mesh logoMesh = this.LogoMesh;
      logoMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(1.42857146f, 1.42857146f, 0.1f, 100f)),
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.UnitZ, -Vector3.UnitZ, Vector3.Up))
      };
    }));
  }

  protected override void Dispose(bool disposing)
  {
    this.LogoMesh.Dispose();
    this.spriteBatch.Dispose();
  }

  private void UpdateStripe(int i, float step)
  {
    FezVertexPositionColor[] vertices = (this.LogoMesh.Groups[i].Geometry as IndexedUserPrimitives<FezVertexPositionColor>).Vertices;
    Vector3 vector3_1 = Vector3.Zero;
    Vector3 vector3_2 = Vector3.Zero;
    Vector3 vector3_3;
    for (int index = 0; index <= 100; ++index)
    {
      if (index < 20)
      {
        float num = (float) index / 20f * FezMath.Saturate(step / 0.2f);
        ref FezVertexPositionColor local1 = ref vertices[index * 2];
        vector3_3 = new Vector3((float) (i + 1) * 0.091f, num * 0.5f, 0.0f);
        Vector3 vector3_4 = vector3_3;
        local1.Position = vector3_4;
        vector3_1 = vector3_3;
        ref FezVertexPositionColor local2 = ref vertices[index * 2 + 1];
        vector3_3 = new Vector3((float) i * 0.091f, num * 0.5f, 0.0f);
        Vector3 vector3_5 = vector3_3;
        local2.Position = vector3_5;
        vector3_2 = vector3_3;
      }
      else if (index > 80 /*0x50*/ && (double) step > 0.800000011920929)
      {
        float num = (float) (((double) index - 80.0) / 20.0) * FezMath.Saturate((float) (((double) step - 0.800000011920929) / 0.20000000298023224 / 0.2720000147819519));
        ref FezVertexPositionColor local3 = ref vertices[index * 2];
        vector3_3 = new Vector3((float) (0.5 - (double) num * 0.13600000739097595), (float) (i + 1) * 0.091f, 0.0f);
        Vector3 vector3_6 = vector3_3;
        local3.Position = vector3_6;
        vector3_1 = vector3_3;
        ref FezVertexPositionColor local4 = ref vertices[index * 2 + 1];
        vector3_3 = new Vector3((float) (0.5 - (double) num * 0.13600000739097595), (float) i * 0.091f, 0.0f);
        Vector3 vector3_7 = vector3_3;
        local4.Position = vector3_7;
        vector3_2 = vector3_3;
      }
      else if (index >= 20 && index <= 80 /*0x50*/ && (double) step > 0.20000000298023224)
      {
        float num = (float) (((double) index - 20.0) / 60.0 * (double) FezMath.Saturate((float) (((double) step - 0.20000000298023224) / 0.60000002384185791)) * 1.5707963705062866 * 3.0 - 1.5707963705062866);
        ref FezVertexPositionColor local5 = ref vertices[index * 2];
        vector3_3 = new Vector3((float) (Math.Sin((double) num) * (0.5 - (double) (i + 1) * 0.090999998152256012) + 0.5), (float) (Math.Cos((double) num) * (0.5 - (double) (i + 1) * 0.090999998152256012) + 0.5), 0.0f);
        Vector3 vector3_8 = vector3_3;
        local5.Position = vector3_8;
        vector3_1 = vector3_3;
        ref FezVertexPositionColor local6 = ref vertices[index * 2 + 1];
        vector3_3 = new Vector3((float) (Math.Sin((double) num) * (0.5 - (double) i * 0.090999998152256012) + 0.5), (float) (Math.Cos((double) num) * (0.5 - (double) i * 0.090999998152256012) + 0.5), 0.0f);
        Vector3 vector3_9 = vector3_3;
        local6.Position = vector3_9;
        vector3_2 = vector3_3;
      }
      else
      {
        vertices[index * 2].Position = vector3_1;
        vertices[index * 2 + 1].Position = vector3_2;
      }
    }
  }

  public void End()
  {
    if (!this.iPolytron.Dead)
      this.iPolytron.FadeOutAndDie(0.1f);
    this.iPolytron = (SoundEmitter) null;
  }

  public override void Update(GameTime gameTime)
  {
    TimeSpan elapsedGameTime;
    if ((double) this.SinceStarted == 0.0)
    {
      elapsedGameTime = gameTime.ElapsedGameTime;
      if (elapsedGameTime.Ticks != 0L && this.sPolytron != null)
      {
        this.iPolytron = this.sPolytron.Emit();
        this.sPolytron = (SoundEffect) null;
      }
    }
    double sinceStarted = (double) this.SinceStarted;
    elapsedGameTime = gameTime.ElapsedGameTime;
    double totalSeconds = elapsedGameTime.TotalSeconds;
    this.SinceStarted = (float) (sinceStarted + totalSeconds);
    float linearStep = FezMath.Saturate(this.SinceStarted / 1.75f);
    this.UpdateStripe(3, Easing.EaseOut((double) Easing.EaseIn((double) linearStep, EasingType.Quadratic), EasingType.Quartic) * 0.86f);
    this.UpdateStripe(2, Easing.EaseOut((double) Easing.EaseIn((double) linearStep, EasingType.Cubic), EasingType.Quartic) * 0.86f);
    this.UpdateStripe(1, Easing.EaseOut((double) Easing.EaseIn((double) linearStep, EasingType.Quartic), EasingType.Quartic) * 0.86f);
    this.UpdateStripe(0, Easing.EaseOut((double) Easing.EaseIn((double) linearStep, EasingType.Quintic), EasingType.Quartic) * 0.86f);
  }

  public override void Draw(GameTime gameTime)
  {
    this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    Viewport viewport = this.GraphicsDevice.Viewport;
    double width = (double) viewport.Width;
    viewport = this.GraphicsDevice.Viewport;
    double height = (double) viewport.Height;
    Vector2 vector2 = (new Vector2((float) width, (float) height) / 2f).Round();
    float viewScale = this.GraphicsDevice.GetViewScale();
    this.LogoMesh.Material.Opacity = this.Opacity;
    this.LogoMesh.Draw();
    this.spriteBatch.Begin();
    float num = Easing.EaseOut((double) FezMath.Saturate((float) (((double) this.SinceStarted - 1.5) / 0.25)), EasingType.Quadratic);
    this.spriteBatch.Draw(this.PolytronText, vector2 + new Vector2((float) -this.PolytronText.Width / 2f, 120f * viewScale).Round(), new Color(1f, 1f, 1f, this.Opacity * num));
    this.spriteBatch.End();
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
