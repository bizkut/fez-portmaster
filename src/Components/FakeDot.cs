// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FakeDot
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Components.Scripting;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

public class FakeDot : DrawableGameComponent
{
  private List<Vector4> Vertices = new List<Vector4>();
  private int[] FaceVertexIndices;
  private Mesh DotMesh;
  private Mesh RaysMesh;
  private Mesh FlareMesh;
  private IndexedUserPrimitives<FezVertexPositionColor> DotWireGeometry;
  private IndexedUserPrimitives<FezVertexPositionColor> DotFacesGeometry;
  private float Theta;
  private float EightShapeStep;

  public float Opacity { get; set; }

  public FakeDot(Game game)
    : base(game)
  {
    this.DrawOrder = 100000000;
    this.Visible = this.Enabled = false;
  }

  public override void Initialize()
  {
    base.Initialize();
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
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/smooth_ray"),
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.FlareMesh = new Mesh()
    {
      Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/rainbow_flare"),
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh dotMesh = this.DotMesh;
      dotMesh.Effect = (BaseEffect) new DotEffect()
      {
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up))
      };
      Mesh raysMesh = this.RaysMesh;
      raysMesh.Effect = (BaseEffect) new DefaultEffect.Textured()
      {
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up))
      };
      Mesh flareMesh = this.FlareMesh;
      flareMesh.Effect = (BaseEffect) new DefaultEffect.Textured()
      {
        ForcedViewMatrix = new Matrix?(Matrix.CreateLookAt(new Vector3(0.0f, 0.0f, 10f), Vector3.Zero, Vector3.Up))
      };
    }));
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
  }

  public override void Update(GameTime gameTime)
  {
    this.Theta += (float) (gameTime.ElapsedGameTime.TotalSeconds * 1.0);
    float num1 = (float) Math.Cos((double) this.Theta);
    float m14 = (float) Math.Sin((double) this.Theta);
    Matrix matrix = new Matrix(num1, 0.0f, 0.0f, m14, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, -m14, 0.0f, 0.0f, num1);
    for (int index = 0; index < this.Vertices.Count; ++index)
    {
      Vector4 vector4 = Vector4.Transform(this.Vertices[index], matrix);
      float num2 = (float) ((((double) vector4.W + 1.0) / 3.0 * 1.0 + 0.5) * 0.3333333432674408);
      this.DotWireGeometry.Vertices[index].Position = new Vector3(vector4.X, vector4.Y, vector4.Z) * num2;
    }
    for (int index = 0; index < this.FaceVertexIndices.Length; ++index)
      this.DotFacesGeometry.Vertices[index].Position = this.DotWireGeometry.Vertices[this.FaceVertexIndices[index]].Position;
    float num3 = (float) (Math.Sin(gameTime.TotalGameTime.TotalSeconds / 3.0) * 0.5 + 1.0);
    this.EightShapeStep += (float) gameTime.ElapsedGameTime.TotalSeconds * num3;
    Vector3 vector3 = new Vector3((float) (4.0 + Math.Sin((double) this.EightShapeStep * 4.0 / 3.0) * 1.25));
    this.DotMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
    this.DotMesh.Position = Vector3.Zero;
    this.DotMesh.Scale = vector3;
    this.UpdateRays((float) gameTime.ElapsedGameTime.TotalSeconds);
  }

  private void UpdateRays(float elapsedSeconds)
  {
    if (RandomHelper.Probability(0.03))
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
    for (int index = this.RaysMesh.Groups.Count - 1; index >= 0; --index)
    {
      Group group = this.RaysMesh.Groups[index];
      DotHost.RayState customData = group.CustomData as DotHost.RayState;
      customData.Age += elapsedSeconds * 0.15f;
      float num = Easing.EaseOut(Math.Sin((double) customData.Age * 6.2831854820251465 - 1.5707963705062866) * 0.5 + 0.5, EasingType.Quadratic);
      group.Material.Diffuse = new Vector3(num * 0.0375f) + customData.Tint.ToVector3() * 0.075f * num;
      float speed = customData.Speed;
      group.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.Forward, (float) ((double) elapsedSeconds * (double) speed * 0.30000001192092896));
      group.Scale = new Vector3((float) ((double) num * 0.75 + 0.25), (float) ((double) num * 0.5 + 0.5), 1f);
      if ((double) customData.Age > 1.0)
        this.RaysMesh.RemoveGroupAt(index);
    }
    this.FlareMesh.Position = this.RaysMesh.Position = this.DotMesh.Position;
    this.FlareMesh.Rotation = this.RaysMesh.Rotation = Quaternion.Identity;
    this.RaysMesh.Scale = this.DotMesh.Scale * 0.5f;
    this.FlareMesh.Scale = new Vector3(MathHelper.Lerp(this.DotMesh.Scale.X * 0.875f, (float) Math.Pow((double) this.DotMesh.Scale.X * 1.5, 1.5), 1f));
    this.FlareMesh.Material.Diffuse = new Vector3(0.25f * FezMath.Saturate(this.Opacity * 2f));
  }

  public override void Draw(GameTime gameTime)
  {
    float aspectRatio = this.GraphicsDevice.Viewport.AspectRatio;
    BaseEffect effect1 = this.RaysMesh.Effect;
    BaseEffect effect2 = this.FlareMesh.Effect;
    BaseEffect effect3 = this.DotMesh.Effect;
    Matrix? nullable1 = new Matrix?(Matrix.CreateOrthographic(14f * aspectRatio, 14f, 0.1f, 100f));
    Matrix? nullable2 = nullable1;
    effect3.ForcedProjectionMatrix = nullable2;
    Matrix? nullable3;
    Matrix? nullable4 = nullable3 = nullable1;
    effect2.ForcedProjectionMatrix = nullable3;
    Matrix? nullable5 = nullable4;
    effect1.ForcedProjectionMatrix = nullable5;
    this.FlareMesh.Draw();
    this.RaysMesh.Draw();
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Dot));
    (this.DotMesh.Effect as DotEffect).UpdateHueOffset(gameTime.ElapsedGameTime);
    this.DotMesh.Blending = new BlendingMode?(BlendingMode.Alphablending);
    this.DotMesh.Material.Diffuse = new Vector3(0.0f);
    this.DotMesh.Material.Opacity = (double) this.Opacity > 0.5 ? this.Opacity * 0.25f : 0.0f;
    this.DotMesh.Draw();
    this.DotMesh.Groups[0].Enabled = true;
    this.DotMesh.Groups[1].Enabled = false;
    this.DotMesh.Blending = new BlendingMode?(BlendingMode.Additive);
    float num = (float) Math.Pow(Math.Sin(gameTime.TotalGameTime.TotalSeconds * 2.0) * 0.5 + 0.5, 3.0);
    this.DotMesh.Material.Opacity = 1f;
    this.DotMesh.Material.Diffuse = new Vector3(num * 0.5f * this.Opacity);
    this.DotMesh.Draw();
    this.DotMesh.Groups[0].Enabled = false;
    this.DotMesh.Groups[1].Enabled = true;
    this.DotMesh.Material.Diffuse = new Vector3(this.Opacity);
    this.DotMesh.Draw();
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  internal IScriptingManager Scripting { get; set; }
}
