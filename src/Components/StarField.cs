// Decompiled with JetBrains decompiler
// Type: FezGame.Components.StarField
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
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
namespace FezGame.Components;

public class StarField : DrawableGameComponent
{
  private static readonly Color[] Colors = new Color[11]
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
    new Color(98, 21, 88),
    new Color((int) byte.MaxValue, (int) byte.MaxValue, (int) byte.MaxValue)
  };
  private static IIndexedPrimitiveCollection StarGeometry;
  private Mesh StarsMesh;
  private Mesh TrailsMesh;
  private FakePointSpritesEffect StarEffect;
  private bool Done;
  public float Opacity = 1f;
  private TimeSpan sinceStarted;
  private Matrix? savedViewMatrix;

  public bool HasHorizontalTrails { get; set; }

  public bool ReverseTiming { get; set; }

  public bool FollowCamera { get; set; }

  public bool HasZoomed { get; set; }

  public float AdditionalZoom { get; set; }

  public float AdditionalScale { get; set; }

  public StarField(Game game)
    : base(game)
  {
    this.DrawOrder = 2006;
    this.Enabled = false;
    this.Visible = false;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.StarsMesh = new Mesh()
    {
      AlwaysOnTop = true,
      DepthWrites = false,
      Blending = new BlendingMode?(BlendingMode.Additive),
      Culling = CullMode.None
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh starsMesh = this.StarsMesh;
      FakePointSpritesEffect pointSpritesEffect1 = new FakePointSpritesEffect();
      pointSpritesEffect1.ForcedProjectionMatrix = new Matrix?(Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(75f), this.CameraManager.AspectRatio, 0.1f, 1000f));
      FakePointSpritesEffect pointSpritesEffect2 = pointSpritesEffect1;
      this.StarEffect = pointSpritesEffect1;
      FakePointSpritesEffect pointSpritesEffect3 = pointSpritesEffect2;
      starsMesh.Effect = (BaseEffect) pointSpritesEffect3;
    }));
    if (this.HasHorizontalTrails)
    {
      this.TrailsMesh = new Mesh()
      {
        AlwaysOnTop = true,
        DepthWrites = false,
        Blending = new BlendingMode?(BlendingMode.Additive)
      };
      DrawActionScheduler.Schedule((Action) (() =>
      {
        Mesh trailsMesh = this.TrailsMesh;
        trailsMesh.Effect = (BaseEffect) new HorizontalTrailsEffect()
        {
          ForcedProjectionMatrix = this.StarEffect.ForcedProjectionMatrix
        };
      }));
    }
    this.AddStars();
    if (this.FollowCamera)
      return;
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.StarEffect.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(Vector3.Zero, Vector3.UnitZ, Vector3.Up));
      if (!this.HasHorizontalTrails)
        return;
      (this.TrailsMesh.Effect as HorizontalTrailsEffect).ForcedViewMatrix = this.StarEffect.ForcedViewMatrix;
    }));
  }

  private void AddStars()
  {
    Texture2D texture2D = this.CMProvider.Global.Load<Texture2D>("Other Textures/FullWhite");
    if (StarField.StarGeometry != null && !this.HasHorizontalTrails)
    {
      Group group = this.StarsMesh.AddGroup();
      group.Texture = (Texture) texture2D;
      group.Geometry = StarField.StarGeometry;
    }
    else
    {
      Color[] pointColors = (Color[]) null;
      Vector3[] pointPairs = (Vector3[]) null;
      float num1 = 49f;
      float num2 = num1;
      Vector3[] pointCenters = new Vector3[(int) ((double) num2 * (double) num1 * (double) num2)];
      if (this.HasHorizontalTrails)
      {
        pointColors = new Color[(int) ((double) num2 * (double) num1 * (double) num2) * 2];
        pointPairs = new Vector3[(int) ((double) num2 * (double) num1 * (double) num2) * 2];
      }
      Random random = RandomHelper.Random;
      int num3 = 0;
      int index1 = 0;
      for (int index2 = 0; (double) index2 < (double) num2; ++index2)
      {
        for (int index3 = 0; (double) index3 < (double) num1; ++index3)
        {
          for (int index4 = 0; (double) index4 < (double) num2; ++index4)
          {
            Vector3 vector3 = new Vector3((float) (((double) index2 - (double) num2 / 2.0) * 100.0), (float) (((double) index3 - (double) num1 / 2.0) * 100.0), (float) (((double) index4 - (double) num2 / 2.0) * 100.0));
            pointCenters[num3++] = vector3;
            if (this.HasHorizontalTrails)
            {
              pointPairs[index1] = vector3;
              pointPairs[index1 + 1] = vector3;
              byte b = (byte) random.Next(0, 256 /*0x0100*/);
              byte r = (byte) random.Next(0, 256 /*0x0100*/);
              pointColors[index1] = new Color((int) r, 0, (int) b, 0);
              pointColors[index1 + 1] = new Color((int) r, 0, (int) b, (int) byte.MaxValue);
              index1 += 2;
            }
          }
        }
      }
      Group g = this.StarsMesh.AddGroup();
      StarField.AddPoints(g, pointCenters, (Texture) texture2D, 2f);
      StarField.StarGeometry = g.Geometry;
      if (!this.HasHorizontalTrails)
        return;
      this.TrailsMesh.AddLines(pointColors, pointPairs, true);
    }
  }

  private static void AddPoints(Group g, Vector3[] pointCenters, Texture texture, float size)
  {
    BufferedIndexedPrimitives<VertexFakePointSprite> indexedPrimitives = new BufferedIndexedPrimitives<VertexFakePointSprite>(PrimitiveType.TriangleList);
    g.Geometry = (IIndexedPrimitiveCollection) indexedPrimitives;
    indexedPrimitives.Vertices = new VertexFakePointSprite[pointCenters.Length * 4];
    indexedPrimitives.Indices = new int[pointCenters.Length * 6];
    Random random = RandomHelper.Random;
    int length = StarField.Colors.Length;
    VertexFakePointSprite[] vertices = indexedPrimitives.Vertices;
    int[] indices = indexedPrimitives.Indices;
    for (int index1 = 0; index1 < pointCenters.Length; ++index1)
    {
      Color color = StarField.Colors[random.Next(0, length)];
      int index2 = index1 * 4;
      vertices[index2] = new VertexFakePointSprite(pointCenters[index1], color, new Vector2(0.0f, 0.0f), new Vector2(-size, -size));
      vertices[index2 + 1] = new VertexFakePointSprite(pointCenters[index1], color, new Vector2(1f, 0.0f), new Vector2(size, -size));
      vertices[index2 + 2] = new VertexFakePointSprite(pointCenters[index1], color, new Vector2(1f, 1f), new Vector2(size, size));
      vertices[index2 + 3] = new VertexFakePointSprite(pointCenters[index1], color, new Vector2(0.0f, 1f), new Vector2(-size, size));
      int index3 = index1 * 6;
      indices[index3] = index2;
      indices[index3 + 1] = index2 + 1;
      indices[index3 + 2] = index2 + 2;
      indices[index3 + 3] = index2;
      indices[index3 + 4] = index2 + 2;
      indices[index3 + 5] = index2 + 3;
    }
    indexedPrimitives.UpdateBuffers();
    indexedPrimitives.CleanUp();
    g.Texture = texture;
  }

  public bool IsDisposed { get; private set; }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.TrailsMesh != null)
      this.TrailsMesh.Dispose();
    this.TrailsMesh = (Mesh) null;
    if (this.StarsMesh != null)
      this.StarsMesh.Effect.Dispose();
    this.StarsMesh = (Mesh) null;
    this.IsDisposed = true;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.ReverseTiming)
    {
      this.sinceStarted -= gameTime.ElapsedGameTime;
      this.sinceStarted -= gameTime.ElapsedGameTime;
    }
    else
      this.sinceStarted += gameTime.ElapsedGameTime;
    float num = Easing.EaseIn(this.sinceStarted.TotalSeconds / 3.0, EasingType.Quartic);
    if (this.HasHorizontalTrails)
      (this.TrailsMesh.Effect as HorizontalTrailsEffect).Timing = (float) this.sinceStarted.TotalSeconds;
    if (!this.FollowCamera)
    {
      this.AdditionalZoom = (float) this.HasZoomed.AsNumeric() + num / 3f;
      this.AdditionalScale = num / 6f;
    }
    if (!this.HasHorizontalTrails && (double) num > 40.0 && !this.Done)
    {
      this.Done = true;
      ServiceHelper.RemoveComponent<StarField>(this);
    }
    if (!this.HasHorizontalTrails || !(this.sinceStarted <= TimeSpan.Zero))
      return;
    this.Enabled = false;
    this.sinceStarted = TimeSpan.Zero;
  }

  public override void Draw(GameTime gameTime)
  {
    this.TargetRenderer.DrawFullscreen(Color.Black);
    this.Draw();
  }

  public void Draw()
  {
    float viewScale = this.GraphicsDevice.GetViewScale();
    float num = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
    if (!this.FollowCamera)
    {
      this.StarsMesh.Position = this.AdditionalZoom * Vector3.Forward * 125f - 2400f * Vector3.Forward;
      this.StarsMesh.Scale = new Vector3(1f + this.AdditionalScale, 1f + this.AdditionalScale, 1f);
    }
    else if (!this.GameState.InFpsMode)
    {
      this.StarEffect.ForcedViewMatrix = this.savedViewMatrix = new Matrix?();
      this.StarsMesh.Position = this.CameraManager.InterpolatedCenter * 0.5f;
      this.StarsMesh.Scale = new Vector3((float) (112.5 / ((double) this.CameraManager.Radius / (double) viewScale / (double) num + 40.0)));
      if (this.HasHorizontalTrails)
      {
        this.TrailsMesh.Position = this.StarsMesh.Position;
        Mesh trailsMesh = this.TrailsMesh;
        Mesh starsMesh = this.StarsMesh;
        Vector3 vector3_1 = new Vector3((float) (65.0 / ((double) this.CameraManager.Radius / (double) viewScale / (double) num + 25.0)));
        Vector3 vector3_2 = vector3_1;
        starsMesh.Scale = vector3_2;
        Vector3 vector3_3 = vector3_1;
        trailsMesh.Scale = vector3_3;
      }
    }
    else if (this.CameraManager.ProjectionTransition)
    {
      if (this.CameraManager.Viewpoint != Viewpoint.Perspective)
        this.StarEffect.ForcedViewMatrix = new Matrix?(Matrix.Lerp(this.CameraManager.View, this.CameraManager.View, Easing.EaseOut((double) this.CameraManager.ViewTransitionStep, EasingType.Quadratic)));
      else if (!this.savedViewMatrix.HasValue)
        this.savedViewMatrix = new Matrix?(this.CameraManager.View);
    }
    this.StarsMesh.Material.Opacity = this.Opacity;
    this.StarsMesh.Draw();
    if (!this.Enabled || !this.HasHorizontalTrails)
      return;
    this.TrailsMesh.Draw();
  }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
