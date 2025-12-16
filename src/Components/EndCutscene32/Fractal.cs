// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene32.Fractal
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components.EndCutscene32;

internal class Fractal : DrawableGameComponent
{
  private const float RotateRevealDuration = 5f;
  private const float ZoomDuration = 18f;
  private const float AxisZoomDuration = 6f;
  private const int FractalDepth = 15;
  private const float FractalCycleMaxScreenSize = 20f;
  private readonly EndCutscene32Host Host;
  private readonly List<Mesh> FractalMeshes = new List<Mesh>();
  private float Time;
  private Fractal.State ActiveState;
  private Mesh OuterShellMesh;
  private Mesh AxisMesh;

  public Fractal(Game game, EndCutscene32Host host)
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

  private void Reset()
  {
    this.FractalMeshes.Clear();
    this.OuterShellMesh = new Mesh();
    Fractal.AddShell(this.OuterShellMesh, true, true);
    Fractal.AddShell(this.OuterShellMesh, false, true);
    Mesh mesh1 = this.OuterShellMesh;
    bool positive = true;
    for (int index = 0; index < 15; ++index)
    {
      Mesh mesh2 = new Mesh()
      {
        Material = {
          Diffuse = Color.Cyan.ToVector3()
        }
      };
      Fractal.AddShell(mesh2, positive, false);
      mesh2.Culling = positive ? CullMode.CullCounterClockwiseFace : CullMode.CullClockwiseFace;
      mesh2.CustomData = (object) positive;
      mesh2.Parent = mesh1;
      this.FractalMeshes.Add(mesh2);
      positive = !positive;
      mesh1 = mesh2;
    }
    this.AxisMesh = new Mesh();
    this.AxisMesh.AddColoredBox(new Vector3(1f, 1f, 10000f) / 200f, Vector3.Zero, Color.Blue, false);
    this.AxisMesh.AddColoredBox(new Vector3(1f, 10000f, 1f) / 200f, Vector3.Zero, Color.Green, false);
    this.AxisMesh.AddColoredBox(new Vector3(10000f, 1f, 1f) / 200f, Vector3.Zero, Color.Red, false);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.OuterShellMesh.Effect = (BaseEffect) new DefaultEffect.LitVertexColored();
      for (int index = 0; index < 15; ++index)
        this.FractalMeshes[index].Effect = (BaseEffect) new DefaultEffect.LitVertexColored();
      Mesh axisMesh = this.AxisMesh;
      axisMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        Fullbright = true
      };
    }));
    this.OuterShellMesh.Scale = Vector3.One * 2f;
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    foreach (Mesh fractalMesh in this.FractalMeshes)
      fractalMesh.Dispose();
    this.FractalMeshes.Clear();
    if (this.OuterShellMesh != null)
      this.OuterShellMesh.Dispose();
    this.OuterShellMesh = (Mesh) null;
    if (this.AxisMesh != null)
      this.AxisMesh.Dispose();
    this.AxisMesh = (Mesh) null;
  }

  private static void AddShell(Mesh mesh, bool positive, bool full)
  {
    Vector3 vector3 = new Vector3(0.5f);
    Group group = mesh.AddGroup();
    if (positive)
    {
      if (full)
        group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalColor>(new VertexPositionNormalColor[12]
        {
          new VertexPositionNormalColor(new Vector3(1f, -1f, -1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, -1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, 1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, -1f, 1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, -1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, -1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, -1f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, 1f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, 1f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, -1f) * vector3, Vector3.UnitY, Color.White)
        }, new int[18]
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
          10
        }, PrimitiveType.TriangleList);
      else
        group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalColor>(new VertexPositionNormalColor[24]
        {
          new VertexPositionNormalColor(new Vector3(1f, -1f, 0.0f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 0.0f, 0.0f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 0.0f, 1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, -1f, 1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, -1f, -1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, -1f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, 0.0f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, -1f, 0.0f) * vector3, Vector3.UnitX, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, -1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 0.0f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(0.0f, 0.0f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(0.0f, -1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(0.0f, -1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(0.0f, 1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, -1f, 1f) * vector3, Vector3.UnitZ, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, 0.0f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, 1f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(0.0f, 1f, 1f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(0.0f, 1f, 0.0f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, -1f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(-1f, 1f, 0.0f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, 0.0f) * vector3, Vector3.UnitY, Color.White),
          new VertexPositionNormalColor(new Vector3(1f, 1f, -1f) * vector3, Vector3.UnitY, Color.White)
        }, new int[36]
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
          18,
          17,
          16 /*0x10*/,
          19,
          18,
          20,
          22,
          21,
          20,
          23,
          22
        }, PrimitiveType.TriangleList);
    }
    else
      group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalColor>(new VertexPositionNormalColor[24]
      {
        new VertexPositionNormalColor(new Vector3(-1f, 0.0f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, 1f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, 1f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, 0.0f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, -1f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, 1f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(1f, 1f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(1f, -1f, -1f) * vector3, -Vector3.UnitZ, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, 0.0f, 0.0f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, 1f, 0.0f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, 1f, -1f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, 0.0f, -1f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, -1f, 1f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, 1f, 1f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, 1f, 0.0f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, -1f, 0.0f) * vector3, -Vector3.UnitX, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, -1f, 0.0f) * vector3, -Vector3.UnitY, Color.White),
        new VertexPositionNormalColor(new Vector3(-1f, -1f, 1f) * vector3, -Vector3.UnitY, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, -1f, 1f) * vector3, -Vector3.UnitY, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, -1f, 0.0f) * vector3, -Vector3.UnitY, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, -1f, -1f) * vector3, -Vector3.UnitY, Color.White),
        new VertexPositionNormalColor(new Vector3(0.0f, -1f, 1f) * vector3, -Vector3.UnitY, Color.White),
        new VertexPositionNormalColor(new Vector3(1f, -1f, 1f) * vector3, -Vector3.UnitY, Color.White),
        new VertexPositionNormalColor(new Vector3(1f, -1f, -1f) * vector3, -Vector3.UnitY, Color.White)
      }, new int[36]
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
        21,
        22,
        20,
        22,
        23
      }, PrimitiveType.TriangleList);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading)
      return;
    this.Time += (float) gameTime.ElapsedGameTime.TotalSeconds;
    float num1 = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    switch (this.ActiveState)
    {
      case Fractal.State.RotateReveal:
        double num2;
        float amount1 = Easing.EaseInOut(num2 = (double) FezMath.Saturate(this.Time / 5f), EasingType.Quadratic);
        if (num2 == 0.0)
        {
          this.CameraManager.Center = Vector3.Zero;
          this.CameraManager.Direction = Vector3.UnitZ;
          this.CameraManager.Radius = 10f * this.GraphicsDevice.GetViewScale() * num1;
          this.CameraManager.SnapInterpolation();
          this.LevelManager.ActualAmbient = new Color(0.25f, 0.25f, 0.25f);
          this.LevelManager.ActualDiffuse = Color.White;
          this.OuterShellMesh.Enabled = true;
          this.OuterShellMesh.Scale = Vector3.One * 2f;
          Mesh mesh = this.OuterShellMesh;
          foreach (Mesh fractalMesh in this.FractalMeshes)
          {
            fractalMesh.Parent = mesh;
            fractalMesh.Enabled = true;
            mesh = fractalMesh;
          }
        }
        this.OuterShellMesh.Scale *= 1.0025f;
        this.OuterShellMesh.Rotation = Quaternion.Slerp(Quaternion.Identity, Quaternion.CreateFromAxisAngle(Vector3.UnitY, 3.14159274f), amount1) * Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.One, Vector3.Zero, Vector3.Up));
        double hue1;
        double saturation1;
        double num3;
        Util.ColorToHSV(new Color(this.FractalMeshes[0].Material.Diffuse), out hue1, out saturation1, out num3);
        bool flag1 = true;
        for (int index = 0; index < this.FractalMeshes.Count; ++index)
        {
          Mesh fractalMesh = this.FractalMeshes[index];
          fractalMesh.Scale = fractalMesh.Parent.Scale / 2f;
          fractalMesh.Culling = flag1 ? CullMode.CullClockwiseFace : CullMode.CullCounterClockwiseFace;
          fractalMesh.Rotation = fractalMesh.Parent.Rotation;
          fractalMesh.Position = fractalMesh.Parent.Position - Vector3.Transform(fractalMesh.Parent.Scale / 2f, fractalMesh.Parent.Rotation) * (flag1 ? 1f : -1f) + Vector3.Transform(fractalMesh.Scale / 2f, fractalMesh.Rotation) * (flag1 ? 1f : -1f);
          fractalMesh.Material.Diffuse = Util.ColorFromHSV(hue1, saturation1, num3).ToVector3();
          hue1 += 15.0;
          flag1 = !flag1;
        }
        if ((double) this.Time <= 5.0)
          break;
        this.ChangeState();
        break;
      case Fractal.State.RainbowZoom:
        float amount2 = FezMath.Saturate(this.Time / 18f);
        if ((double) amount2 == 0.0)
        {
          this.OuterShellMesh.Enabled = true;
          this.OuterShellMesh.Scale = Vector3.One * 4.188076f;
          this.OuterShellMesh.Rotation = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(-Vector3.One, Vector3.Zero, Vector3.Up));
          Mesh mesh = this.OuterShellMesh;
          foreach (Mesh fractalMesh in this.FractalMeshes)
          {
            fractalMesh.Parent = mesh;
            fractalMesh.Rotation = this.OuterShellMesh.Rotation;
            fractalMesh.Enabled = true;
            mesh = fractalMesh;
          }
        }
        float num4 = MathHelper.Lerp(1.0025f, 1.075f, amount2);
        if ((double) this.OuterShellMesh.Scale.X > 20.0)
        {
          this.FractalMeshes[0].Scale *= num4;
          this.OuterShellMesh.Enabled = false;
        }
        else
          this.OuterShellMesh.Scale *= num4;
        double hue2;
        double saturation2;
        double num5;
        Util.ColorToHSV(new Color(this.FractalMeshes[0].Material.Diffuse), out hue2, out saturation2, out num5);
        this.OuterShellMesh.Rotation = this.OuterShellMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, Easing.EaseIn((double) FezMath.Saturate(FezMath.Saturate(amount2 - 0.1f) / 0.7f), EasingType.Quadratic) * 0.02f) * this.OuterShellMesh.Rotation;
        this.AxisMesh.Rotation = this.OuterShellMesh.Rotation;
        for (int index = 0; index < this.FractalMeshes.Count; ++index)
        {
          Mesh fractalMesh = this.FractalMeshes[index];
          if (fractalMesh.Parent.Enabled)
            fractalMesh.Scale = fractalMesh.Parent.Scale / 2f;
          bool customData = (bool) fractalMesh.CustomData;
          fractalMesh.Position = fractalMesh.Parent.Position - Vector3.Transform(fractalMesh.Parent.Scale / 2f, fractalMesh.Parent.Rotation) * (customData ? 1f : -1f) + Vector3.Transform(fractalMesh.Scale / 2f, fractalMesh.Rotation) * (customData ? 1f : -1f);
          fractalMesh.Material.Diffuse = Util.ColorFromHSV(hue2, saturation2, num5).ToVector3();
          fractalMesh.Rotation = this.OuterShellMesh.Rotation;
          if ((double) fractalMesh.Scale.X > 20.0)
          {
            this.FractalMeshes.RemoveAt(0);
            this.FractalMeshes[0].Parent = this.OuterShellMesh;
            fractalMesh.Parent = this.FractalMeshes[this.FractalMeshes.Count - 1];
            this.FractalMeshes.Add(fractalMesh);
            --index;
          }
          hue2 += 15.0;
        }
        if ((double) this.Time <= 18.0)
          break;
        this.ChangeState();
        break;
      case Fractal.State.AxisReveal:
        if ((double) this.Time == 0.0)
        {
          Mesh mesh = this.OuterShellMesh;
          foreach (Mesh fractalMesh in this.FractalMeshes)
          {
            fractalMesh.Parent = mesh;
            fractalMesh.Rotation = this.OuterShellMesh.Rotation;
            mesh = fractalMesh;
          }
        }
        foreach (Mesh fractalMesh in this.FractalMeshes)
        {
          if (fractalMesh.Enabled)
          {
            fractalMesh.Scale *= 1.075f;
            break;
          }
        }
        this.OuterShellMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, 0.02f) * this.OuterShellMesh.Rotation;
        this.AxisMesh.Rotation = this.OuterShellMesh.Rotation;
        bool flag2 = false;
        for (int index = 0; index < this.FractalMeshes.Count; ++index)
        {
          Mesh fractalMesh = this.FractalMeshes[index];
          if (fractalMesh.Enabled)
          {
            if (fractalMesh.Parent.Enabled)
              fractalMesh.Scale = fractalMesh.Parent.Scale / 2f;
            bool customData = (bool) fractalMesh.CustomData;
            fractalMesh.Position = fractalMesh.Parent.Position - Vector3.Transform(fractalMesh.Parent.Scale / 2f, fractalMesh.Parent.Rotation) * (customData ? 1f : -1f) + Vector3.Transform(fractalMesh.Scale / 2f, fractalMesh.Rotation) * (customData ? 1f : -1f);
            fractalMesh.Rotation = this.OuterShellMesh.Rotation;
            if ((double) fractalMesh.Scale.X > 20.0)
            {
              this.FractalMeshes[index].Enabled = false;
              if (index < this.FractalMeshes.Count - 1)
                this.FractalMeshes[index + 1].Parent = this.OuterShellMesh;
            }
            flag2 |= this.FractalMeshes[index].Enabled;
          }
        }
        if (flag2)
          break;
        this.ChangeState();
        break;
      case Fractal.State.AxisZoom:
        float linearStep = FezMath.Saturate(this.Time / 6f);
        this.AxisMesh.Scale = Vector3.Lerp(Vector3.One, new Vector3(3f), Easing.EaseIn((double) linearStep, EasingType.Cubic));
        this.AxisMesh.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, (float) (0.02500000037252903 * (1.0 - (double) linearStep))) * this.AxisMesh.Rotation;
        this.AxisMesh.Position = Vector3.Lerp(Vector3.Zero, Vector3.Transform(Vector3.Forward * 10f, this.AxisMesh.Rotation), Easing.EaseIn((double) linearStep, EasingType.Cubic));
        if ((double) this.Time <= 6.0)
          break;
        this.ChangeState();
        break;
    }
  }

  private void ChangeState()
  {
    if (this.ActiveState == Fractal.State.AxisZoom)
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
    switch (this.ActiveState)
    {
      case Fractal.State.RotateReveal:
      case Fractal.State.RainbowZoom:
        this.OuterShellMesh.Draw();
        using (List<Mesh>.Enumerator enumerator = this.FractalMeshes.GetEnumerator())
        {
          while (enumerator.MoveNext())
            enumerator.Current.Draw();
          break;
        }
      case Fractal.State.AxisReveal:
        this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.CutsceneWipe));
        foreach (Mesh fractalMesh in this.FractalMeshes)
          fractalMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.CutsceneWipe);
        this.AxisMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
        break;
      case Fractal.State.AxisZoom:
        this.GraphicsDevice.PrepareStencilReadWrite(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.CutsceneWipe);
        this.AxisMesh.Draw();
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
        break;
    }
  }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  private enum State
  {
    RotateReveal,
    RainbowZoom,
    AxisReveal,
    AxisZoom,
  }
}
