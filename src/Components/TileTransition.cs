// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TileTransition
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

#nullable disable
namespace FezGame.Components;

public class TileTransition : DrawableGameComponent
{
  private const int TilesWide = 1;
  private static SoundEffect sTransition;
  private static readonly object Mutex = new object();
  private static TileTransition CurrentTransition;
  private RenderTargetHandle textureA;
  private RenderTargetHandle textureB;
  private bool taCaptured;
  private bool tbCaptured;
  private float sinceStarted;
  private Mesh mesh;

  public Action ScreenCaptured { private get; set; }

  public Func<bool> WaitFor { private get; set; }

  public bool IsDisposed { get; private set; }

  public TileTransition(Game game)
    : base(game)
  {
    if (TileTransition.CurrentTransition != null)
    {
      ServiceHelper.RemoveComponent<TileTransition>(TileTransition.CurrentTransition);
      TileTransition.CurrentTransition = (TileTransition) null;
    }
    TileTransition.CurrentTransition = this;
    this.DrawOrder = 2099;
  }

  public override void Initialize()
  {
    base.Initialize();
    lock (TileTransition.Mutex)
    {
      if (TileTransition.sTransition == null)
        TileTransition.sTransition = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/CubeTransition");
    }
    Mesh mesh = new Mesh();
    DefaultEffect.Textured textured = new DefaultEffect.Textured();
    textured.ForcedProjectionMatrix = new Matrix?(Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60f), 1f, 0.1f, 100f));
    textured.ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, -1.365f), new Vector3(0.0f, 0.0f, 0.0f), Vector3.Up));
    mesh.Effect = (BaseEffect) textured;
    mesh.DepthWrites = false;
    mesh.AlwaysOnTop = true;
    mesh.Blending = new BlendingMode?(BlendingMode.Opaque);
    this.mesh = mesh;
    for (int index1 = 0; index1 < 1; ++index1)
    {
      for (int index2 = 0; index2 < 1; ++index2)
      {
        float x = (float) index1 / 1f;
        float num1 = (float) (index1 + 1) / 1f;
        float y = (float) index2 / 1f;
        float num2 = (float) (index2 + 1) / 1f;
        bool flag1 = RandomHelper.Probability(0.5);
        bool flag2 = RandomHelper.Probability(0.5);
        Group group1 = this.mesh.AddGroup();
        if (flag2)
          group1.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionTexture>(new VertexPositionTexture[4]
          {
            new VertexPositionTexture(new Vector3(-0.5f, 0.5f, -0.5f), new Vector2(1f - x, 1f - num2)),
            new VertexPositionTexture(new Vector3(0.5f, 0.5f, -0.5f), new Vector2(1f - num1, 1f - num2)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1f - x, 1f - y)),
            new VertexPositionTexture(new Vector3(0.5f, -0.5f, -0.5f), new Vector2(1f - num1, 1f - y))
          }, new int[6]{ 0, 2, 1, 2, 3, 1 }, PrimitiveType.TriangleList);
        else
          group1.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionTexture>(new VertexPositionTexture[4]
          {
            new VertexPositionTexture(new Vector3(-0.5f, 0.5f, -0.5f), new Vector2(1f - x, 1f - num2)),
            new VertexPositionTexture(new Vector3(0.5f, 0.5f, -0.5f), new Vector2(1f - num1, 1f - num2)),
            new VertexPositionTexture(new Vector3(-0.5f, -0.5f, -0.5f), new Vector2(1f - x, 1f - y)),
            new VertexPositionTexture(new Vector3(0.5f, -0.5f, -0.5f), new Vector2(1f - num1, 1f - y))
          }, new int[6]{ 0, 2, 1, 2, 3, 1 }, PrimitiveType.TriangleList);
        group1.Scale = new Vector3(1f, 1f, (float) (1.0 / (flag2 ? 1.0 : 1.0)));
        group1.Position = new Vector3(x, y, 0.0f);
        Group group2 = group1;
        TileTransition.TileData tileData = new TileTransition.TileData();
        tileData.X = x + RandomHelper.Centered(0.15000000596046448);
        tileData.Y = y + RandomHelper.Centered(0.15000000596046448);
        tileData.B = false;
        tileData.Inverted = flag1;
        tileData.Vertical = flag2;
        // ISSUE: variable of a boxed type
        __Boxed<TileTransition.TileData> local1 = (ValueType) tileData;
        group2.CustomData = (object) local1;
        group1.Material = new Material();
        Group group3 = this.mesh.AddGroup();
        if (flag2)
        {
          Group group4 = group3;
          VertexPositionTexture[] vertices = new VertexPositionTexture[4]
          {
            new VertexPositionTexture(new Vector3(-0.5f, flag1 ? 0.5f : -0.5f, -0.5f), new Vector2(1f - x, flag1 ? 1f - y : 1f - num2)),
            new VertexPositionTexture(new Vector3(0.5f, flag1 ? 0.5f : -0.5f, -0.5f), new Vector2(1f - num1, flag1 ? 1f - y : 1f - num2)),
            new VertexPositionTexture(new Vector3(-0.5f, flag1 ? 0.5f : -0.5f, 0.5f), new Vector2(1f - x, flag1 ? 1f - num2 : 1f - y)),
            new VertexPositionTexture(new Vector3(0.5f, flag1 ? 0.5f : -0.5f, 0.5f), new Vector2(1f - num1, flag1 ? 1f - num2 : 1f - y))
          };
          int[] indices;
          if (!flag1)
            indices = new int[6]{ 0, 2, 1, 2, 3, 1 };
          else
            indices = new int[6]{ 0, 1, 2, 2, 1, 3 };
          IndexedUserPrimitives<VertexPositionTexture> indexedUserPrimitives = new IndexedUserPrimitives<VertexPositionTexture>(vertices, indices, PrimitiveType.TriangleList);
          group4.Geometry = (IIndexedPrimitiveCollection) indexedUserPrimitives;
        }
        else
        {
          Group group5 = group3;
          VertexPositionTexture[] vertices = new VertexPositionTexture[4]
          {
            new VertexPositionTexture(new Vector3(flag1 ? 0.5f : -0.5f, 0.5f, 0.5f), new Vector2(flag1 ? 1f - num1 : 1f - x, 1f - num2)),
            new VertexPositionTexture(new Vector3(flag1 ? 0.5f : -0.5f, 0.5f, -0.5f), new Vector2(flag1 ? 1f - x : 1f - num1, 1f - num2)),
            new VertexPositionTexture(new Vector3(flag1 ? 0.5f : -0.5f, -0.5f, 0.5f), new Vector2(flag1 ? 1f - num1 : 1f - x, 1f - y)),
            new VertexPositionTexture(new Vector3(flag1 ? 0.5f : -0.5f, -0.5f, -0.5f), new Vector2(flag1 ? 1f - x : 1f - num1, 1f - y))
          };
          int[] indices;
          if (!flag1)
            indices = new int[6]{ 0, 2, 1, 2, 3, 1 };
          else
            indices = new int[6]{ 0, 1, 2, 2, 1, 3 };
          IndexedUserPrimitives<VertexPositionTexture> indexedUserPrimitives = new IndexedUserPrimitives<VertexPositionTexture>(vertices, indices, PrimitiveType.TriangleList);
          group5.Geometry = (IIndexedPrimitiveCollection) indexedUserPrimitives;
        }
        group3.Scale = new Vector3(1f, 1f, (float) (1.0 / (flag2 ? 1.0 : 1.0)));
        group3.Position = new Vector3(x, y, 0.0f);
        Group group6 = group3;
        tileData = new TileTransition.TileData();
        tileData.X = x + RandomHelper.Centered(0.15000000596046448);
        tileData.Y = y + RandomHelper.Centered(0.15000000596046448);
        tileData.B = true;
        tileData.Inverted = flag1;
        tileData.Vertical = flag2;
        // ISSUE: variable of a boxed type
        __Boxed<TileTransition.TileData> local2 = (ValueType) tileData;
        group6.CustomData = (object) local2;
        group3.Material = new Material();
      }
    }
    this.mesh.Position = new Vector3(0.0f, 0.0f, 0.0f);
    this.textureA = this.TargetRenderer.TakeTarget();
    this.textureB = this.TargetRenderer.TakeTarget();
    foreach (Group group in this.mesh.Groups)
      group.Texture = !((TileTransition.TileData) group.CustomData).B ? (Texture) this.textureA.Target : (Texture) this.textureB.Target;
    this.TargetRenderer.ScheduleHook(this.DrawOrder, this.textureA.Target);
    TileTransition.sTransition.Emit();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.textureA != null)
    {
      this.TargetRenderer.ReturnTarget(this.textureA);
      this.TargetRenderer.UnscheduleHook(this.textureA.Target);
    }
    if (this.textureB != null)
    {
      this.TargetRenderer.ReturnTarget(this.textureB);
      this.TargetRenderer.UnscheduleHook(this.textureB.Target);
    }
    this.textureA = this.textureB = (RenderTargetHandle) null;
    this.mesh.Dispose();
    this.IsDisposed = true;
    TileTransition.CurrentTransition = (TileTransition) null;
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.tbCaptured)
      return;
    this.sinceStarted += (float) (gameTime.ElapsedGameTime.TotalSeconds * 1.5);
    int num = 0;
    foreach (Group group in this.mesh.Groups)
    {
      TileTransition.TileData customData = (TileTransition.TileData) group.CustomData;
      float a = Easing.EaseOut((double) FezMath.Saturate(this.sinceStarted), EasingType.Quadratic) * 1.57079637f;
      group.Rotation = Quaternion.CreateFromAxisAngle(customData.Vertical ? Vector3.Left : Vector3.Up, customData.Inverted ? a : -a);
      group.Material.Diffuse = new Vector3(0.125f) + 0.875f * (customData.B ? new Vector3((float) Math.Sin((double) a)) : new Vector3((float) (1.0 - Math.Sin((double) a))));
      if ((double) a >= 1.5707963705062866)
        ++num;
    }
    if (num != this.mesh.Groups.Count)
      return;
    ServiceHelper.RemoveComponent<TileTransition>(this);
  }

  public override void Draw(GameTime gameTime)
  {
    if (!this.taCaptured && this.TargetRenderer.IsHooked(this.textureA.Target))
    {
      this.TargetRenderer.Resolve(this.textureA.Target, false);
      this.taCaptured = true;
      if (this.ScreenCaptured != null)
      {
        this.ScreenCaptured();
        this.ScreenCaptured = (Action) null;
      }
    }
    if (this.TargetRenderer.IsHooked(this.textureB.Target))
    {
      this.TargetRenderer.Resolve(this.textureB.Target, true);
      this.tbCaptured = true;
      this.WaitFor = (Func<bool>) null;
    }
    if ((this.WaitFor != null && !this.WaitFor() || this.TargetRenderer.IsHooked(this.textureB.Target) ? 0 : (!this.tbCaptured ? 1 : 0)) != 0)
      this.TargetRenderer.ScheduleHook(this.DrawOrder, this.textureB.Target);
    this.GraphicsDevice.Clear(Color.Black);
    this.GraphicsDevice.SetupViewport();
    this.mesh.Draw();
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private struct TileData
  {
    public float X;
    public float Y;
    public bool B;
    public bool Inverted;
    public bool Vertical;
  }
}
