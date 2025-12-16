// Decompiled with JetBrains decompiler
// Type: FezGame.Components.NowLoadingHexahedron
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Tools;
using FezGame.Components.Actions;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

internal class NowLoadingHexahedron : DrawableGameComponent
{
  private static TimeSpan SincePhaseStarted;
  private static TimeSpan SinceWavesStarted;
  private static TimeSpan SinceTurning;
  private readonly string ToLevel;
  private readonly Vector3 Center;
  private static NowLoadingHexahedron.Phases Phase;
  private Mesh Outline;
  private Mesh WireCube;
  private Mesh SolidCube;
  private Mesh Flare;
  private Mesh Rays;
  private int NextOutline = 1;
  private float OutlineIn;
  private float WhiteFillStep;
  private float Phi;
  private SoundEffect WarpSound;
  private NowLoadingHexahedron.Darkener TheDarkening;

  public NowLoadingHexahedron(Game game, Vector3 center, string toLevel)
    : base(game)
  {
    this.ToLevel = toLevel;
    this.Center = center;
    this.UpdateOrder = 10;
    this.DrawOrder = 901;
  }

  public override void Initialize()
  {
    base.Initialize();
    NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.VectorOutline;
    TimeSpan zero;
    NowLoadingHexahedron.SinceTurning = zero = TimeSpan.Zero;
    NowLoadingHexahedron.SinceWavesStarted = zero;
    NowLoadingHexahedron.SincePhaseStarted = zero;
    this.PlayerManager.CanControl = false;
    this.WarpSound = this.CMProvider.GetForLevel(this.GameState.IsTrialMode ? "trial/ELDERS" : "ELDERS").Load<SoundEffect>("Sounds/Zu/HexaWarpIn");
    this.Dot.Hidden = false;
    this.Dot.Behaviour = DotHost.BehaviourType.ClampToTarget;
    this.Dot.Target = this.Center;
    this.Dot.ScalePulsing = 0.0f;
    this.Dot.Opacity = 0.0f;
    Waiters.Wait((Func<bool>) (() => this.PlayerManager.Grounded), (Action) (() =>
    {
      this.WalkTo.Destination = (Func<Vector3>) (() => this.PlayerManager.Position * Vector3.UnitY + this.Center * FezMath.XZMask);
      this.WalkTo.NextAction = ActionType.Idle;
      this.PlayerManager.Action = ActionType.WalkingTo;
    }));
    this.TimeManager.TimeFactor = this.TimeManager.DefaultTimeFactor;
    this.Outline = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true
    };
    this.Outline.AddWireframePolygon(Color.White, new Vector3(0.0f, 0.8660254f, 0.0f), new Vector3(0.7071068f, 0.2886752f, 0.0f), new Vector3(0.7071068f, -0.2886752f, 0.0f), new Vector3(0.0f, -0.8660254f, 0.0f), new Vector3(-0.7071068f, -0.2886752f, 0.0f), new Vector3(-0.7071068f, 0.2886752f, 0.0f), new Vector3(0.0f, 0.8660254f, 0.0f));
    this.Outline.Scale = new Vector3(4f);
    this.Outline.BakeTransform<FezVertexPositionColor>();
    Group firstGroup = this.Outline.FirstGroup;
    firstGroup.Material = new Material();
    firstGroup.Enabled = false;
    for (int index = 0; index < 1024 /*0x0400*/; ++index)
      this.Outline.CloneGroup(firstGroup);
    firstGroup.Enabled = true;
    this.WireCube = new Mesh()
    {
      DepthWrites = false,
      AlwaysOnTop = true,
      Material = {
        Opacity = 0.0f
      }
    };
    this.WireCube.AddWireframeBox(Vector3.One * 4f, Vector3.Zero, Color.White, true);
    this.WireCube.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
    this.WireCube.BakeTransform<FezVertexPositionColor>();
    this.SolidCube = new Mesh()
    {
      AlwaysOnTop = true,
      Material = {
        Opacity = 0.0f
      }
    };
    this.SolidCube.AddFlatShadedBox(Vector3.One * 4f, Vector3.Zero, Color.White, true);
    this.SolidCube.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
    this.SolidCube.BakeTransform<VertexPositionNormalColor>();
    this.Flare = new Mesh()
    {
      DepthWrites = false,
      Material = {
        Opacity = 0.0f
      },
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.LinearClamp
    };
    this.Flare.AddFace(Vector3.One * 4f, Vector3.Zero, FaceOrientation.Front, true);
    this.Rays = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.AnisotropicClamp,
      DepthWrites = false,
      AlwaysOnTop = true
    };
    for (int index = 0; index < 128 /*0x80*/; ++index)
    {
      float x = 0.75f;
      float num = 0.0075f;
      Group group = this.Rays.AddGroup();
      group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionTexture>(new FezVertexPositionTexture[6]
      {
        new FezVertexPositionTexture(new Vector3(0.0f, (float) ((double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(0.0f, 0.0f)),
        new FezVertexPositionTexture(new Vector3(x, num / 2f, 0.0f), new Vector2(1f, 0.0f)),
        new FezVertexPositionTexture(new Vector3(x, (float) ((double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(1f, 0.45f)),
        new FezVertexPositionTexture(new Vector3(x, (float) (-(double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(1f, 0.55f)),
        new FezVertexPositionTexture(new Vector3(x, (float) (-(double) num / 2.0), 0.0f), new Vector2(1f, 1f)),
        new FezVertexPositionTexture(new Vector3(0.0f, (float) (-(double) num / 2.0 * 0.10000000149011612), 0.0f), new Vector2(0.0f, 1f))
      }, new int[12]{ 0, 1, 2, 0, 2, 5, 5, 2, 3, 5, 3, 4 }, PrimitiveType.TriangleList);
      group.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Forward, RandomHelper.Between(0.0, 6.2831854820251465));
      group.Material = new Material()
      {
        Diffuse = new Vector3(0.0f)
      };
    }
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh outline = this.Outline;
      outline.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        Fullbright = true,
        AlphaIsEmissive = false
      };
      Mesh wireCube = this.WireCube;
      wireCube.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        Fullbright = true,
        AlphaIsEmissive = false
      };
      Mesh solidCube = this.SolidCube;
      solidCube.Effect = (BaseEffect) new DefaultEffect.LitVertexColored()
      {
        Fullbright = false,
        AlphaIsEmissive = false
      };
      Mesh flare = this.Flare;
      flare.Effect = (BaseEffect) new DefaultEffect.Textured()
      {
        Fullbright = true,
        AlphaIsEmissive = false
      };
      this.Flare.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/flare_alpha");
      this.Rays.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.Rays.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/smooth_ray");
    }));
    ServiceHelper.AddComponent((IGameComponent) (this.TheDarkening = new NowLoadingHexahedron.Darkener(this.Game)));
    this.LevelManager.LevelChanged += new Action(this.Kill);
    this.WarpSound.Emit().Persistent = true;
  }

  private void Kill()
  {
    if (NowLoadingHexahedron.Phase == NowLoadingHexahedron.Phases.Load)
      return;
    ServiceHelper.RemoveComponent<NowLoadingHexahedron>(this);
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.Outline.Dispose();
    this.WireCube.Dispose();
    this.SolidCube.Dispose();
    this.Rays.Dispose();
    this.Flare.Dispose();
    ServiceHelper.RemoveComponent<NowLoadingHexahedron.Darkener>(this.TheDarkening);
    this.LevelManager.LevelChanged -= new Action(this.Kill);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    TimeSpan timeSpan = TimeSpan.Zero;
    if (!this.GameState.Paused && this.CameraManager.ActionRunning && this.CameraManager.Viewpoint.IsOrthographic())
      timeSpan = gameTime.ElapsedGameTime;
    float totalSeconds1 = (float) NowLoadingHexahedron.SincePhaseStarted.TotalSeconds;
    float totalSeconds2 = (float) timeSpan.TotalSeconds;
    this.Rays.Position = this.Flare.Position = this.SolidCube.Position = this.WireCube.Position = this.Outline.Position = this.Center;
    this.Flare.Rotation = this.Outline.Rotation = this.CameraManager.Rotation;
    this.SolidCube.Rotation = this.WireCube.Rotation = this.CameraManager.Rotation * Quaternion.CreateFromAxisAngle(Vector3.Up, this.Phi);
    NowLoadingHexahedron.SincePhaseStarted += timeSpan;
    if (NowLoadingHexahedron.Phase < NowLoadingHexahedron.Phases.TurnTurnTurn)
    {
      NowLoadingHexahedron.SinceWavesStarted += timeSpan;
      float num = (float) (1.0 + Math.Pow(Math.Max(NowLoadingHexahedron.SinceWavesStarted.TotalSeconds - 2.0, 0.0) / 5.0, 4.0));
      if (NowLoadingHexahedron.SinceWavesStarted.TotalSeconds > 2.0 && NowLoadingHexahedron.SinceWavesStarted.TotalSeconds < 12.0)
      {
        this.OutlineIn -= totalSeconds2;
        if ((double) this.OutlineIn <= 0.0)
        {
          this.Outline.Groups[this.NextOutline].Enabled = true;
          this.Outline.Groups[this.NextOutline].Scale = new Vector3(5f);
          if (++this.NextOutline >= this.Outline.Groups.Count)
            this.NextOutline = 1;
          if ((double) num > 15.0)
            this.OutlineIn = 0.0f;
          else
            this.OutlineIn += 2f / (float) Math.Pow((double) num * 1.25, 2.0);
        }
      }
      foreach (Group group in this.Outline.Groups)
      {
        if (group.Enabled && group.Id != 0)
        {
          group.Scale -= new Vector3(num * totalSeconds2);
          group.Material.Opacity = Easing.EaseOut((double) FezMath.Saturate((float) (1.0 - ((double) group.Scale.X - 1.0) / 4.0)), EasingType.Sine);
          if ((double) group.Scale.X <= 1.0)
            group.Enabled = false;
        }
      }
    }
    if (NowLoadingHexahedron.Phase >= NowLoadingHexahedron.Phases.TurnTurnTurn)
    {
      NowLoadingHexahedron.SinceTurning += timeSpan;
      float x = MathHelper.Lerp(0.0f, 13f, Easing.EaseIn((double) Easing.EaseOut(NowLoadingHexahedron.SinceTurning.TotalSeconds / 9.75, EasingType.Sine), EasingType.Quintic));
      this.Dot.RotationSpeed = -x;
      this.Phi += totalSeconds2 * (float) Math.Pow((double) x, 1.125);
    }
    switch (NowLoadingHexahedron.Phase)
    {
      case NowLoadingHexahedron.Phases.VectorOutline:
        float num1 = FezMath.Saturate(Easing.Ease((double) totalSeconds1 / 3.0, -0.75f, EasingType.Sine));
        this.Outline.FirstGroup.Material.Opacity = num1;
        this.Outline.FirstGroup.Scale = new Vector3((float) (4.0 - (double) num1 * 3.0));
        if ((double) totalSeconds1 < 3.0)
          break;
        NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.WireframeWaves;
        this.PlayerManager.Action = ActionType.LookingUp;
        NowLoadingHexahedron.SincePhaseStarted = TimeSpan.Zero;
        ServiceHelper.AddComponent((IGameComponent) new CamShake(ServiceHelper.Game)
        {
          Duration = TimeSpan.FromSeconds(10.0),
          Distance = 0.25f
        });
        break;
      case NowLoadingHexahedron.Phases.WireframeWaves:
        this.Speech.Hide();
        float num2 = Easing.EaseIn((double) FezMath.Saturate(totalSeconds1 / 5f), EasingType.Quadratic);
        this.WireCube.Material.Opacity = num2;
        this.Outline.FirstGroup.Material.Opacity = 1f - num2;
        if ((double) num2 != 1.0)
          break;
        NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.FillCube;
        NowLoadingHexahedron.SincePhaseStarted = TimeSpan.Zero;
        break;
      case NowLoadingHexahedron.Phases.FillCube:
        float num3 = FezMath.Saturate(totalSeconds1 / 4f);
        this.WireCube.Material.Opacity = 1f - num3;
        this.SolidCube.Material.Opacity = num3;
        (this.SolidCube.Effect as DefaultEffect).Emissive = num3 / 2f;
        this.Flare.Material.Opacity = num3 / 2f;
        this.Flare.Scale = new Vector3(4f * num3);
        if ((double) num3 != 1.0)
          break;
        NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.FillDot;
        NowLoadingHexahedron.SincePhaseStarted = TimeSpan.Zero;
        break;
      case NowLoadingHexahedron.Phases.FillDot:
        float linearStep = FezMath.Saturate(totalSeconds1 / 2.75f);
        this.Dot.Hidden = false;
        this.Dot.ScaleFactor = 50f * Easing.EaseOut((double) linearStep, EasingType.Sine);
        this.Dot.InnerScale = 1f;
        this.Dot.Opacity = (float) (0.5 + 0.25 * (double) linearStep);
        this.Dot.RotationSpeed = 0.0f;
        this.GameState.SkyOpacity = 1f - linearStep;
        if ((double) linearStep != 1.0)
          break;
        NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.TurnTurnTurn;
        NowLoadingHexahedron.SincePhaseStarted = TimeSpan.Zero;
        break;
      case NowLoadingHexahedron.Phases.TurnTurnTurn:
        if ((double) FezMath.Saturate(totalSeconds1 / 7.5f) != 1.0)
          break;
        NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.Rays;
        NowLoadingHexahedron.SincePhaseStarted = TimeSpan.Zero;
        break;
      case NowLoadingHexahedron.Phases.Rays:
        float num4 = Easing.EaseIn((double) FezMath.Saturate(totalSeconds1 / 1.75f), EasingType.Quadratic);
        (this.SolidCube.Effect as DefaultEffect).Emissive = (float) (0.5 + (double) num4 / 2.0);
        this.Flare.Material.Opacity = (float) (0.75 + (double) num4 * 0.25);
        this.Flare.Scale = new Vector3((float) (4.0 + 5.0 * (double) num4));
        float num5 = Easing.EaseIn((double) FezMath.Saturate(totalSeconds1 / 1.75f), EasingType.Cubic);
        foreach (Group group in this.Rays.Groups)
        {
          float num6 = (float) group.Id / (float) this.Rays.Groups.Count;
          group.Material.Diffuse = new Vector3(FezMath.Saturate((float) ((double) num5 / (double) num6 * 4.0)) * 0.25f);
          float val1 = (float) Math.Pow((double) num5 / (double) num6 * 4.0, 2.0);
          group.Scale = new Vector3(Math.Min(val1, 50f), Math.Min(val1, 100f), 1f);
        }
        if ((double) num5 != 1.0)
          break;
        NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.FadeToWhite;
        NowLoadingHexahedron.SincePhaseStarted = TimeSpan.Zero;
        break;
      case NowLoadingHexahedron.Phases.FadeToWhite:
        float num7 = FezMath.Saturate(totalSeconds1 / 1f);
        this.WhiteFillStep = num7;
        if ((double) num7 != 1.0)
          break;
        NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.Load;
        NowLoadingHexahedron.SincePhaseStarted = TimeSpan.Zero;
        break;
      case NowLoadingHexahedron.Phases.Load:
        this.GameState.SkyOpacity = 1f;
        this.GameState.SkipLoadScreen = true;
        this.GameState.Loading = true;
        Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoLoad));
        worker.Finished += (Action) (() => this.ThreadPool.Return<bool>(worker));
        worker.Start(false);
        break;
      case NowLoadingHexahedron.Phases.FadeOut:
        this.WhiteFillStep = 1f - Easing.EaseOut((double) FezMath.Saturate(totalSeconds1 / 0.75f), EasingType.Quintic);
        if ((double) totalSeconds1 <= 0.75)
          break;
        ServiceHelper.RemoveComponent<NowLoadingHexahedron>(this);
        break;
    }
  }

  private void DoLoad(bool dummy)
  {
    HorizontalDirection lookingDirection = this.PlayerManager.LookingDirection;
    this.Dot.Reset();
    this.LevelManager.ChangeLevel(this.ToLevel);
    this.PlayerManager.LookingDirection = lookingDirection;
    NowLoadingHexahedron.Phase = NowLoadingHexahedron.Phases.FadeOut;
    this.PlayerManager.CheckpointGround = (TrileInstance) null;
    this.PlayerManager.RespawnAtCheckpoint();
    this.CameraManager.Center = this.PlayerManager.Position + Vector3.Up * this.PlayerManager.Size.Y / 2f + Vector3.UnitY;
    this.CameraManager.SnapInterpolation();
    this.LevelMaterializer.CullInstances();
    this.GameState.ScheduleLoadEnd = true;
    this.GameState.SkipLoadScreen = false;
    this.TimeManager.TimeFactor = this.TimeManager.DefaultTimeFactor;
  }

  public override void Draw(GameTime gameTime)
  {
    if (NowLoadingHexahedron.Phase != NowLoadingHexahedron.Phases.FadeOut)
    {
      this.Outline.Draw();
      this.WireCube.Draw();
      this.Rays.Draw();
      this.SolidCube.Draw();
      this.Flare.Draw();
    }
    if ((double) this.WhiteFillStep <= 0.0)
      return;
    this.TargetRenderer.DrawFullscreen(new Color(1f, 1f, 1f, this.WhiteFillStep));
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager Speech { get; set; }

  [ServiceDependency]
  public IDotManager Dot { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency(Optional = true)]
  public IWalkToService WalkTo { protected get; set; }

  private enum Phases
  {
    VectorOutline,
    WireframeWaves,
    FillCube,
    FillDot,
    TurnTurnTurn,
    Rays,
    FadeToWhite,
    Load,
    FadeOut,
  }

  public class Darkener : DrawableGameComponent
  {
    public Darkener(Game game)
      : base(game)
    {
      this.DrawOrder = 899;
    }

    public override void Draw(GameTime gameTime)
    {
      if (NowLoadingHexahedron.Phase == NowLoadingHexahedron.Phases.FadeOut)
        return;
      this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, FezMath.Saturate(Easing.EaseIn(NowLoadingHexahedron.SinceWavesStarted.TotalSeconds / 12.0, EasingType.Cubic)) * 0.375f * this.GameState.SkyOpacity));
    }

    [ServiceDependency]
    public ITargetRenderingManager TargetRenderer { get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { get; set; }
  }
}
