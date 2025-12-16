// Decompiled with JetBrains decompiler
// Type: FezGame.Components.BlackHolesHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class BlackHolesHost : DrawableGameComponent, IBlackHoleManager
{
  private static readonly string[] AlwaysEnabledLevels = new string[3]
  {
    "NUZU_ABANDONED_B",
    "STARGATE_RUINS",
    "WALL_INTERIOR_HOLE"
  };
  private Mesh HolesBodyMesh;
  private Mesh HolesFringeMesh;
  private Matrix[] InstanceData;
  private readonly List<BlackHolesHost.HoleState> holes = new List<BlackHolesHost.HoleState>();
  private Texture2D starsTexture;
  private Texture2D ripsTexture;
  private Matrix textureMatrix;
  private BlackHolesHost.HoleState currentHole;
  private SoundEffect buzz;
  private SoundEffect[] glitches;
  private readonly Random random = new Random();
  public static BlackHolesHost Instance;

  public BlackHolesHost(Game game)
    : base(game)
  {
    this.DrawOrder = 50;
    BlackHolesHost.Instance = this;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.starsTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/black_hole/Stars");
    this.buzz = this.CMProvider.Global.Load<SoundEffect>("Sounds/Zu/BlackHoleBuzz");
    this.glitches = new SoundEffect[3]
    {
      this.CMProvider.Global.Load<SoundEffect>("Sounds/Intro/FezLogoGlitch1"),
      this.CMProvider.Global.Load<SoundEffect>("Sounds/Intro/FezLogoGlitch2"),
      this.CMProvider.Global.Load<SoundEffect>("Sounds/Intro/FezLogoGlitch3")
    };
    this.ripsTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/black_hole/Rips");
    this.HolesFringeMesh = new Mesh()
    {
      Effect = (BaseEffect) new InstancedBlackHoleEffect(false),
      SamplerState = SamplerState.PointClamp,
      DepthWrites = false,
      Texture = (Dirtyable<Texture>) (Texture) this.ripsTexture,
      SkipGroupCheck = true
    };
    this.HolesBodyMesh = new Mesh()
    {
      Effect = (BaseEffect) new InstancedBlackHoleEffect(true),
      DepthWrites = false,
      Material = {
        Diffuse = Vector3.Zero
      },
      SkipGroupCheck = true
    };
    BaseEffect.InstancingModeChanged += new Action(this.RefreshEffects);
    this.CameraManager.ProjectionChanged += new Action(this.ScaleStarBackground);
    this.ScaleStarBackground();
  }

  private void RefreshEffects()
  {
    this.HolesFringeMesh.Effect = (BaseEffect) new InstancedBlackHoleEffect(false);
    this.HolesBodyMesh.Effect = (BaseEffect) new InstancedBlackHoleEffect(true);
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    BaseEffect.InstancingModeChanged -= new Action(this.RefreshEffects);
  }

  private void ScaleStarBackground()
  {
    if (this.CameraManager.ProjectionTransition || !this.CameraManager.Viewpoint.IsOrthographic())
      return;
    Viewport viewport = this.GraphicsDevice.Viewport;
    float num = (float) ((double) viewport.Width / (double) this.CameraManager.Radius / 16.0) * this.GraphicsDevice.GetViewScale();
    float m11 = (float) viewport.Width / (float) this.starsTexture.Width / num;
    float m22 = (float) viewport.Height / (float) this.starsTexture.Height / num;
    this.textureMatrix = new Matrix(m11, 0.0f, 0.0f, 0.0f, 0.0f, m22, 0.0f, 0.0f, (float) (-(double) m11 / 2.0), (float) (-(double) m22 / 2.0), 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
  }

  public override void Initialize()
  {
    base.Initialize();
    this.Visible = this.Enabled = false;
    this.CameraManager.ViewChanged += new Action(this.CullHolesByBounds);
    this.LevelManager.LevelChanged += new Action(this.TryCreateHoles);
  }

  private void CullHolesByBounds()
  {
    if (!this.Enabled || this.GameState.Loading)
      return;
    bool inTransition = this.GameState.FarawaySettings.InTransition;
    bool flag = this.CameraManager.ProjectionTransition || !this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.ProjectionTransitionNewlyReached;
    foreach (BlackHolesHost.HoleState hole in this.holes)
    {
      hole.Visible = flag || !inTransition && this.CameraManager.Frustum.Contains(hole.Volume.BoundingBox) != 0;
      if (hole.Emitter != null && !hole.Emitter.Dead)
        hole.Emitter.FactorizeVolume = hole.Visible;
    }
  }

  private void TryCreateHoles()
  {
    this.Visible = this.Enabled = false;
    this.HolesBodyMesh.ClearGroups();
    this.HolesFringeMesh.ClearGroups();
    this.holes.Clear();
    this.CreateHoles();
    this.Visible = this.Enabled = this.holes.Count > 0;
    if (!this.Enabled)
      return;
    this.ScaleStarBackground();
  }

  private void CreateHoles()
  {
    bool flag = ((IEnumerable<string>) BlackHolesHost.AlwaysEnabledLevels).Contains<string>(this.LevelManager.Name) || !this.GameState.SaveData.ThisLevel.FirstVisit && !this.GameState.IsTrialMode && RandomHelper.Probability((double) Math.Min(this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes, 32 /*0x20*/) / 64.0);
    foreach (Volume volume in this.LevelManager.Volumes.Values.Where<Volume>((Func<Volume, bool>) (x => x.ActorSettings != null && x.ActorSettings.IsBlackHole)))
    {
      BlackHolesHost.HoleState holeState = new BlackHolesHost.HoleState(volume, this);
      holeState.Build();
      holeState.Enabled = flag;
      if (holeState.Enabled)
      {
        holeState.Emitter = this.buzz.EmitAt(holeState.Center, true, 0.0f, 0.0f);
        holeState.Visible = true;
      }
      this.holes.Add(holeState);
    }
    this.InstanceData = new Matrix[this.holes.Count];
    for (int index = 0; index < this.InstanceData.Length; ++index)
      this.InstanceData[index] = new Matrix(0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 1f, 0.0f, 0.0f);
    this.HolesBodyMesh.Collapse<VertexPositionInstance>();
    this.HolesFringeMesh.Collapse<VertexPositionTextureInstance>();
  }

  public void DisableAll()
  {
    foreach (BlackHolesHost.HoleState hole in this.holes)
    {
      if (hole.Emitter != null && !hole.Emitter.Dead)
        hole.Emitter.FadeOutAndDie(0.1f);
      hole.Emitter = (SoundEmitter) null;
      hole.Enabled = false;
    }
  }

  public void EnableAll()
  {
    foreach (BlackHolesHost.HoleState hole in this.holes)
    {
      if (hole.Emitter == null || hole.Emitter.Dead)
        hole.Emitter = this.buzz.EmitAt(hole.Center, true);
      hole.Enabled = true;
      hole.SinceEnabled = 0.0f;
    }
  }

  public void Randomize()
  {
    bool flag = RandomHelper.Probability((double) Math.Min(this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes, 32 /*0x20*/) / 64.0);
    foreach (BlackHolesHost.HoleState hole in this.holes)
    {
      hole.Enabled = flag;
      if (hole.Enabled)
      {
        if (hole.Emitter == null || hole.Emitter.Dead)
          hole.Emitter = this.buzz.EmitAt(hole.Center, true);
      }
      else
      {
        if (hole.Emitter != null && !hole.Emitter.Dead)
          hole.Emitter.FadeOutAndDie(0.1f);
        hole.Emitter = (SoundEmitter) null;
      }
      hole.SinceEnabled = 0.0f;
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMenuCube || this.GameState.InMap || !this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.ProjectionTransition)
      return;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
    if (this.PlayerManager.Action != ActionType.SuckedIn && this.currentHole != null)
    {
      this.currentHole.Sucking = false;
      this.currentHole = (BlackHolesHost.HoleState) null;
    }
    if (this.PlayerManager.Action == ActionType.SuckedIn && this.currentHole == null)
    {
      foreach (BlackHolesHost.HoleState hole in this.holes)
      {
        if (hole.Volume.ActorSettings.IsBlackHole && hole.Volume.ActorSettings.Sucking)
        {
          this.PlayerManager.Velocity = Vector3.Zero;
          hole.Volume.ActorSettings.Sucking = false;
          hole.Sucking = true;
          this.currentHole = hole;
          break;
        }
      }
    }
    if (this.currentHole != null)
    {
      Vector3 vector3_2 = -this.CameraManager.Viewpoint.ForwardVector();
      this.PlayerManager.Position = Vector3.Lerp(this.PlayerManager.Position, this.currentHole.Center + -0.25f * Vector3.UnitY + 0.375f * vector3_1 + (this.currentHole.Size + new Vector3(0.6f)) * vector3_2, 0.025f);
    }
    foreach (BlackHolesHost.HoleState hole in this.holes)
    {
      if (hole.Visible && hole.Enabled)
      {
        if (!this.GameState.SaveData.OneTimeTutorials.ContainsKey("DOT_BLACKHOLE_A") && !this.GameState.FarawaySettings.InTransition && !this.PlayerManager.InDoorTransition && this.PlayerManager.Action.AllowsLookingDirectionChange() && this.SpeechBubble.Hidden && !this.PlayerManager.Action.DisallowsRespawn() && this.CameraManager.ViewTransitionReached && this.PlayerManager.CarriedInstance == null)
        {
          this.DotSpeak();
          this.GameState.SaveData.OneTimeTutorials.Add("DOT_BLACKHOLE_A", true);
        }
        BlackHolesHost.HoleState holeState = hole;
        double sinceEnabled = (double) holeState.SinceEnabled;
        TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
        double num1 = elapsedGameTime.TotalSeconds * 0.5;
        holeState.SinceEnabled = (float) (sinceEnabled + num1);
        double num2;
        if (!hole.Sucking)
        {
          elapsedGameTime = gameTime.ElapsedGameTime;
          num2 = elapsedGameTime.TotalSeconds * 0.5;
        }
        else
          num2 = 0.2;
        double num3 = this.random.NextDouble();
        bool flag1 = num2 > num3;
        if (flag1 && (double) hole.SinceEnabled > 1.0)
        {
          this.glitches[this.random.Next(this.glitches.Length)].EmitAt(hole.Center, false, (float) (this.random.NextDouble() - 0.5) * 0.1f);
          bool flag2 = 0.5 > this.random.NextDouble();
          bool flag3 = 0.5 > this.random.NextDouble();
          hole.SetTextureTransform(new Vector2(flag2 ? 1f : 0.0f, flag3 ? 1f : 0.0f), new Vector2(flag2 ? -1f : 1f, flag3 ? -1f : 1f));
        }
        float num4 = (float) this.random.NextDouble() * 16f;
        float num5 = (float) (3.0 / 32.0 * (flag1 ? (double) num4 : 1.0) * 2.0);
        for (int index = 0; index < 3; ++index)
        {
          hole.Offsets[index].X = (float) (this.random.NextDouble() - 0.5) * num5;
          hole.Offsets[index].Y = (float) (this.random.NextDouble() - 0.5) * num5;
          hole.Offsets[index].Z = (float) (this.random.NextDouble() - 0.5) * num5;
        }
      }
    }
  }

  private void DotSpeak()
  {
    this.DotService.Say("DOT_BLACKHOLE_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_BLACKHOLE_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_BLACKHOLE_C", true, false).Ended = (Action) (() => this.DotService.Say("DOT_BLACKHOLE_D", true, false).Ended = (Action) (() => this.DotService.Say("DOT_BLACKHOLE_F", true, true)))));
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.StereoMode)
      return;
    this.DoDraw();
  }

  public void DoDraw()
  {
    if (this.GameState.Loading)
      return;
    bool flag = false;
    foreach (BlackHolesHost.HoleState hole in this.holes)
    {
      flag = ((flag ? 1 : 0) | (!hole.Visible ? 0 : (hole.Enabled ? 1 : 0))) != 0;
      if (flag)
        break;
    }
    if (!flag)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    Vector3 vector3_1 = this.TimeManager.CurrentFogColor.ToVector3();
    Vector3 vector3_2 = Vector3.Lerp(new Vector3((float) (((double) vector3_1.X + (double) vector3_1.Y + (double) vector3_1.Z) / 3.0)), vector3_1, 0.5f);
    for (int i = 0; i < 3; ++i)
    {
      graphicsDevice.SetColorWriteChannels((ColorWriteChannels) ((i == 0 ? 1 : (i == 1 ? 2 : 4)) | 8));
      Vector3 vector3_3 = new Vector3(i == 0 ? 1f : 0.0f, i == 1 ? 1f : 0.0f, i == 2 ? 1f : 0.0f);
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.BlackHoles));
      graphicsDevice.SetBlendingMode(BlendingMode.Multiply);
      foreach (BlackHolesHost.HoleState hole in this.holes)
      {
        if (hole.Visible && hole.Enabled)
        {
          hole.RandomVisibility = (double) FezMath.Saturate(hole.SinceEnabled) > this.random.NextDouble() ? 1f : 0.0f;
          if (i == 0)
            hole.Emitter.VolumeFactor = hole.RandomVisibility;
          hole.SetDiffuse(new Vector3(1f - hole.RandomVisibility));
          hole.SetPositionForPass(i);
        }
      }
      this.DrawBatch(this.HolesBodyMesh);
      if ((double) this.TimeManager.NightContribution == 0.0)
      {
        graphicsDevice.GetDssCombiner().StencilPass = StencilOperation.Keep;
        foreach (BlackHolesHost.HoleState hole in this.holes)
        {
          if (hole.Visible && hole.Enabled)
          {
            hole.SetDiffuse(new Vector3(1f - hole.RandomVisibility));
            hole.SetPositionForPass(i);
          }
        }
        this.DrawBatch(this.HolesFringeMesh);
      }
      else
      {
        graphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.BlackHoles);
        graphicsDevice.SetBlendingMode(BlendingMode.Additive);
        foreach (BlackHolesHost.HoleState hole in this.holes)
        {
          if (hole.Visible && hole.Enabled)
          {
            hole.SetDiffuse((new Vector3(0.75f) - vector3_2).Saturate() * vector3_3 * new Vector3(hole.RandomVisibility));
            hole.SetPositionForPass(i);
          }
        }
        this.DrawBatch(this.HolesFringeMesh);
      }
    }
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    graphicsDevice.SetBlendingMode(BlendingMode.Additive);
    graphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.BlackHoles);
    graphicsDevice.SamplerStates[0] = SamplerState.PointWrap;
    this.TargetRenderingManager.DrawFullscreen((Texture) this.starsTexture, this.textureMatrix);
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
  }

  private void DrawBatch(Mesh mesh)
  {
    (mesh.Effect as InstancedBlackHoleEffect).SetInstanceData(this.InstanceData, 0, this.InstanceData.Length);
    mesh.Draw();
  }

  [ServiceDependency]
  public IDotService DotService { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderingManager { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  private class HoleState
  {
    private readonly BlackHolesHost Host;
    private int instanceIndex;

    public Volume Volume { get; private set; }

    public Vector3[] Offsets { get; private set; }

    public Vector3 Center { get; private set; }

    public SoundEmitter Emitter { get; set; }

    public bool Sucking { get; set; }

    public Vector3 Size { get; private set; }

    public bool Visible { get; set; }

    public void SetTextureTransform(Vector2 offset, Vector2 scale)
    {
      this.Host.InstanceData[this.instanceIndex].M31 = offset.X;
      this.Host.InstanceData[this.instanceIndex].M32 = offset.Y;
      this.Host.InstanceData[this.instanceIndex].M41 = scale.X;
      this.Host.InstanceData[this.instanceIndex].M42 = scale.Y;
    }

    public void SetDiffuse(Vector3 diffuse)
    {
      this.Host.InstanceData[this.instanceIndex].M21 = diffuse.X;
      this.Host.InstanceData[this.instanceIndex].M22 = diffuse.Y;
      this.Host.InstanceData[this.instanceIndex].M23 = diffuse.Z;
    }

    public void SetPositionForPass(int i)
    {
      this.Host.InstanceData[this.instanceIndex].M11 = this.Offsets[i].X;
      this.Host.InstanceData[this.instanceIndex].M12 = this.Offsets[i].Y;
      this.Host.InstanceData[this.instanceIndex].M13 = this.Offsets[i].Z;
    }

    public bool Enabled
    {
      get => this.Volume.Enabled;
      set => this.Volume.Enabled = value;
    }

    public float SinceEnabled { get; set; }

    public float RandomVisibility { get; set; }

    private HoleState(BlackHolesHost host)
    {
      this.Host = host;
      this.Offsets = new Vector3[3];
    }

    public HoleState(Volume volume, BlackHolesHost host)
      : this(host)
    {
      this.Volume = volume;
      this.Size = volume.To - volume.From;
    }

    public void Build()
    {
      this.Center = (this.Volume.From + this.Volume.To) / 2f;
      this.BuildMesh();
    }

    private void BuildMesh()
    {
      Vector3 a = (this.Volume.To - this.Volume.From) / 2f;
      float num1 = a.Y * 2f;
      Mesh mesh1 = new Mesh();
      Mesh mesh2 = new Mesh();
      FaceOrientation[] faceOrientationArray = new FaceOrientation[4]
      {
        FaceOrientation.Front,
        FaceOrientation.Right,
        FaceOrientation.Back,
        FaceOrientation.Left
      };
      foreach (FaceOrientation faceOrientation in faceOrientationArray)
      {
        Vector3 vector3_1 = (faceOrientation.GetTangent().IsSide() ? faceOrientation.GetTangent() : faceOrientation.GetBitangent()).AsVector();
        Vector3 origin = this.Center + faceOrientation.AsVector() * a;
        float num2 = Math.Abs(a.Dot(vector3_1)) * 2f;
        Vector3 vector3_2 = origin + (a - new Vector3(0.5f)) * (-vector3_1 - Vector3.UnitY);
        Vector3 vector3_3 = origin + (a - new Vector3(0.5f)) * (-vector3_1 + Vector3.UnitY);
        for (int index = 0; (double) index < (double) num2; ++index)
        {
          Vector3 p = vector3_2 + (float) index * vector3_1;
          if (!mesh1.Groups.Any<Group>((Func<Group, bool>) (g => FezMath.AlmostEqual(g.Position, p))))
            mesh1.AddFace(Vector3.One * 2f, Vector3.Zero, faceOrientation, true).Position = p;
          p = vector3_3 + (float) index * vector3_1;
          if (!mesh1.Groups.Any<Group>((Func<Group, bool>) (g => FezMath.AlmostEqual(g.Position, p))))
            mesh1.AddFace(Vector3.One * 2f, Vector3.Zero, faceOrientation, true).Position = p;
        }
        Vector3 vector3_4 = origin + (a - new Vector3(0.5f)) * (-vector3_1 - Vector3.UnitY);
        Vector3 vector3_5 = origin + (a - new Vector3(0.5f)) * (vector3_1 - Vector3.UnitY);
        for (int index = 0; (double) index < (double) num1; ++index)
        {
          Vector3 p = vector3_4 + (float) index * Vector3.UnitY;
          if (!mesh1.Groups.Any<Group>((Func<Group, bool>) (g => FezMath.AlmostEqual(g.Position, p))))
            mesh1.AddFace(Vector3.One * 2f, Vector3.Zero, faceOrientation, true).Position = p;
          p = vector3_5 + (float) index * Vector3.UnitY;
          if (!mesh1.Groups.Any<Group>((Func<Group, bool>) (g => FezMath.AlmostEqual(g.Position, p))))
            mesh1.AddFace(Vector3.One * 2f, Vector3.Zero, faceOrientation, true).Position = p;
        }
        mesh2.AddFace(num2 * vector3_1.Abs() + num1 * Vector3.UnitY, origin, faceOrientation, Color.White, true);
      }
      foreach (Group group in mesh1.Groups)
        group.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(0.5f, 0.0f, 0.0f, 0.0f, 0.0f, 0.5f, 0.0f, 0.0f, 0.5 > this.Host.random.NextDouble() ? 0.5f : 0.0f, 0.5 > this.Host.random.NextDouble() ? 0.5f : 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f));
      mesh2.Collapse<VertexPositionNormalColor>();
      IndexedUserPrimitives<VertexPositionNormalColor> geometry1 = mesh2.FirstGroup.Geometry as IndexedUserPrimitives<VertexPositionNormalColor>;
      mesh1.CollapseWithNormalTexture<FezVertexPositionNormalTexture>();
      IndexedUserPrimitives<FezVertexPositionNormalTexture> geometry2 = mesh1.FirstGroup.Geometry as IndexedUserPrimitives<FezVertexPositionNormalTexture>;
      this.instanceIndex = this.Host.HolesBodyMesh.Groups.Count;
      this.Host.HolesBodyMesh.AddGroup().Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionInstance>(((IEnumerable<VertexPositionNormalColor>) geometry1.Vertices).Select<VertexPositionNormalColor, VertexPositionInstance>((Func<VertexPositionNormalColor, VertexPositionInstance>) (x => new VertexPositionInstance(x.Position)
      {
        InstanceIndex = (float) this.instanceIndex
      })).ToArray<VertexPositionInstance>(), geometry1.Indices, PrimitiveType.TriangleList);
      this.Host.HolesFringeMesh.AddGroup().Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionTextureInstance>(((IEnumerable<FezVertexPositionNormalTexture>) geometry2.Vertices).Select<FezVertexPositionNormalTexture, VertexPositionTextureInstance>((Func<FezVertexPositionNormalTexture, VertexPositionTextureInstance>) (x => new VertexPositionTextureInstance(x.Position, x.TextureCoordinate)
      {
        InstanceIndex = (float) this.instanceIndex
      })).ToArray<VertexPositionTextureInstance>(), geometry2.Indices, PrimitiveType.TriangleList);
    }
  }
}
