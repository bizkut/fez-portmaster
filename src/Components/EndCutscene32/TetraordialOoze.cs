// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene32.TetraordialOoze
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

internal class TetraordialOoze : DrawableGameComponent
{
  private const float NoiseZoomDuration = 14f;
  private const int TetraCount = 2000;
  private readonly EndCutscene32Host Host;
  public Mesh NoiseMesh;
  private Mesh TetraMesh;
  private float Time;
  private TetraordialOoze.State ActiveState;

  public TetraordialOoze(Game game, EndCutscene32Host host)
    : base(game)
  {
    this.Host = host;
    this.DrawOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.Reset();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    if (this.TetraMesh != null)
      this.TetraMesh.Dispose();
    this.TetraMesh = (Mesh) null;
  }

  private void Reset()
  {
    this.CameraManager.Center = Vector3.Zero;
    this.CameraManager.Direction = Vector3.UnitZ;
    this.CameraManager.Radius = 10f;
    this.CameraManager.SnapInterpolation();
    Random random = RandomHelper.Random;
    this.TetraMesh = new Mesh();
    DrawActionScheduler.Schedule((Action) (() => this.TetraMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()));
    for (int index = 0; index < 2000; ++index)
    {
      float num1 = RandomHelper.Unit();
      Group group;
      switch (random.Next(0, 5))
      {
        case 0:
          group = TetraordialOoze.AddL(this.TetraMesh);
          break;
        case 1:
          group = TetraordialOoze.AddO(this.TetraMesh);
          break;
        case 2:
          group = TetraordialOoze.AddI(this.TetraMesh);
          break;
        case 3:
          group = TetraordialOoze.AddS(this.TetraMesh);
          break;
        default:
          group = TetraordialOoze.AddT(this.TetraMesh);
          break;
      }
      if (index == 0)
      {
        group.Position = Vector3.Zero;
        num1 = 0.0f;
      }
      else
      {
        float num2 = 5.714286f;
        Vector3 vector3_1 = new Vector3((float) (random.NextDouble() * (random.Next(0, 2) == 1 ? 1.0 : -1.0)), (float) (random.NextDouble() * (random.Next(0, 2) == 1 ? 1.0 : -1.0)), -1f);
        while ((double) vector3_1.LengthSquared() <= 1.0019999742507935)
          vector3_1 = new Vector3((float) (random.NextDouble() * (random.Next(0, 2) == 1 ? 1.0 : -1.0)), (float) (random.NextDouble() * (random.Next(0, 2) == 1 ? 1.0 : -1.0)), -1f);
        float x = vector3_1.Length();
        Vector3 vector3_2 = vector3_1 / x;
        group.Position = vector3_2 * (float) Math.Pow((double) x, 2.0) * new Vector3(num2 / 2f, num2 / 2f / this.CameraManager.AspectRatio, 1f);
      }
      group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, RandomHelper.Between(0.0, 6.2831854820251465));
      group.Scale = new Vector3(MathHelper.Lerp(0.0005f, 0.0075f, 1f - num1));
      group.Material = new Material()
      {
        Diffuse = new Vector3(1f - num1),
        Opacity = 0.0f
      };
    }
  }

  private static Group AddL(Mesh m)
  {
    Vector3 vector3 = new Vector3(4f, 1f, 0.0f);
    Group group = m.AddGroup();
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionColor>(new VertexPositionColor[6]
    {
      new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(4f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(4f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(1f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(1f, 2f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(0.0f, 2f, 0.0f) - vector3 / 2f, Color.White)
    }, new int[7]{ 0, 1, 2, 3, 4, 5, 0 }, PrimitiveType.LineStrip);
    return group;
  }

  private static Group AddO(Mesh m)
  {
    Vector3 vector3 = new Vector3(2f, 2f, 0.0f);
    Group group = m.AddGroup();
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionColor>(new VertexPositionColor[4]
    {
      new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(2f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(2f, 2f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(0.0f, 2f, 0.0f) - vector3 / 2f, Color.White)
    }, new int[5]{ 0, 1, 2, 3, 0 }, PrimitiveType.LineStrip);
    return group;
  }

  private static Group AddI(Mesh m)
  {
    Vector3 vector3 = new Vector3(4f, 1f, 0.0f);
    Group group = m.AddGroup();
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionColor>(new VertexPositionColor[4]
    {
      new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(4f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(4f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(0.0f, 1f, 0.0f) - vector3 / 2f, Color.White)
    }, new int[5]{ 0, 1, 2, 3, 0 }, PrimitiveType.LineStrip);
    return group;
  }

  private static Group AddS(Mesh m)
  {
    Vector3 vector3 = new Vector3(3f, 2f, 0.0f);
    Group group = m.AddGroup();
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionColor>(new VertexPositionColor[8]
    {
      new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(2f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(2f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(3f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(3f, 2f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(1f, 2f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(1f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(0.0f, 1f, 0.0f) - vector3 / 2f, Color.White)
    }, new int[9]{ 0, 1, 2, 3, 4, 5, 6, 7, 0 }, PrimitiveType.LineStrip);
    return group;
  }

  private static Group AddT(Mesh m)
  {
    Vector3 vector3 = new Vector3(3f, 2f, 0.0f);
    Group group = m.AddGroup();
    group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionColor>(new VertexPositionColor[8]
    {
      new VertexPositionColor(new Vector3(0.0f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(3f, 0.0f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(3f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(2f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(2f, 2f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(1f, 2f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(1f, 1f, 0.0f) - vector3 / 2f, Color.White),
      new VertexPositionColor(new Vector3(0.0f, 1f, 0.0f) - vector3 / 2f, Color.White)
    }, new int[9]{ 0, 1, 2, 3, 4, 5, 6, 7, 0 }, PrimitiveType.LineStrip);
    return group;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading)
      return;
    this.Time += (float) gameTime.ElapsedGameTime.TotalSeconds;
    if (this.ActiveState != TetraordialOoze.State.Zoom)
      return;
    if ((double) this.Time == 0.0)
    {
      this.CameraManager.Center = Vector3.Zero;
      this.CameraManager.Direction = Vector3.UnitZ;
      this.CameraManager.Radius = 10f;
      this.CameraManager.SnapInterpolation();
    }
    if ((double) this.Time == 0.0)
    {
      this.NoiseMesh.Scale = new Vector3(1.87495f);
      this.TetraMesh.Scale = Vector3.One;
    }
    for (int index = 0; index < this.TetraMesh.Groups.Count / 10; ++index)
      this.SwapTetraminos();
    if (this.TetraMesh.Groups.Count < 10 && RandomHelper.Probability(0.10000000149011612))
      this.SwapTetraminos();
    float amount = FezMath.Saturate(this.Time / 14f);
    if ((double) amount != 1.0)
    {
      this.NoiseMesh.Scale *= MathHelper.Lerp(1.0025f, 1.01625f, amount);
      this.GameState.SkyRender = true;
      this.CameraManager.Radius /= MathHelper.Lerp(1.0025f, 1.01625f, amount);
      this.CameraManager.SnapInterpolation();
      this.GameState.SkyRender = false;
    }
    float num = MathHelper.Lerp(0.0f, 1f, Easing.EaseIn((double) FezMath.Saturate(amount * 4f), EasingType.Linear));
    foreach (Group group in this.TetraMesh.Groups)
      group.Material.Opacity = num;
    this.NoiseMesh.Material.Opacity = 1f - FezMath.Saturate(amount * 1.5f);
    if ((double) amount != 1.0)
      return;
    this.ChangeState();
  }

  private void SwapTetraminos()
  {
    int num1 = RandomHelper.Random.Next(0, this.TetraMesh.Groups.Count);
    int num2 = RandomHelper.Random.Next(0, this.TetraMesh.Groups.Count);
    Group group1 = this.TetraMesh.Groups[num1];
    Group group2 = this.TetraMesh.Groups[num2];
    if (this.CameraManager.Frustum.Contains(Vector3.Transform(group1.Position, (Matrix) group1.WorldMatrix)) == ContainmentType.Disjoint)
      this.TetraMesh.RemoveGroupAt(num1);
    else if (this.CameraManager.Frustum.Contains(Vector3.Transform(group2.Position, (Matrix) group2.WorldMatrix)) == ContainmentType.Disjoint)
    {
      this.TetraMesh.RemoveGroupAt(num2);
    }
    else
    {
      Vector3 position = group1.Position;
      Material material = group1.Material;
      Vector3 scale = group1.Scale;
      group1.Position = group2.Position;
      group1.Scale = group2.Scale;
      group1.Material = group2.Material;
      group2.Position = position;
      group2.Scale = scale;
      group2.Material = material;
    }
  }

  private void ChangeState()
  {
    if (this.ActiveState == TetraordialOoze.State.Zoom)
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
    if (this.ActiveState != TetraordialOoze.State.Zoom)
      return;
    if ((double) this.NoiseMesh.Material.Opacity > 0.0)
      this.NoiseMesh.Draw();
    if (FezMath.AlmostEqual(this.TetraMesh.FirstGroup.Material.Opacity, 0.0f))
      return;
    this.TetraMesh.Draw();
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  private enum State
  {
    Zoom,
  }
}
