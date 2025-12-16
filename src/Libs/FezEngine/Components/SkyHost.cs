// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.SkyHost
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Components;

public class SkyHost : DrawableGameComponent
{
  private const int Clouds = 64 /*0x40*/;
  private const int BaseDistance = 32 /*0x20*/;
  private const int ParallaxDistance = 48 /*0x30*/;
  private const int HeightSpread = 96 /*0x60*/;
  private const float MovementSpeed = 0.025f;
  private const float PerspectiveScaling = 4f;
  private readonly List<Mesh> cloudMeshes = new List<Mesh>();
  private readonly Dictionary<Layer, List<SkyHost.CloudState>> cloudStates = new Dictionary<Layer, List<SkyHost.CloudState>>((IEqualityComparer<Layer>) LayerComparer.Default);
  private readonly Mesh stars;
  private Matrix backgroundMatrix = Matrix.Identity;
  private Texture2D skyBackground;
  private AnimatedTexture shootingStar;
  public Mesh BgLayers;
  private SoundEffect sShootingStar;
  public static SkyHost Instance;
  private Color[] fogColors;
  private Color[] cloudColors;
  private static readonly float[] starSideOffsets = new float[4];
  private float flickerIn;
  private float flickerCount;
  private string lastSkyName;
  public bool flickering;
  private Vector3 lastCamPos;
  private TimeSpan sinceReached;
  private IWaiter waiter;
  private int sideToSwap;
  private float lastCamSide;
  private float sideOffset;
  private float startStep;
  private float startStep2;
  private RenderTargetHandle RtHandle;
  private float RadiusAtFirstDraw;

  public SkyHost(Game game)
    : base(game)
  {
    this.DrawOrder = 0;
    this.UpdateOrder = 11;
    SkyHost.Instance = this;
    this.stars = new Mesh()
    {
      Culling = CullMode.CullClockwiseFace,
      AlwaysOnTop = true,
      DepthWrites = false,
      SamplerState = SamplerState.PointWrap
    };
    ServiceHelper.AddComponent((IGameComponent) new CloudShadowsHost(game, this));
  }

  public override void Initialize()
  {
    base.Initialize();
    int index = 0;
    foreach (FaceOrientation faceOrientation in Util.GetValues<FaceOrientation>())
    {
      if (faceOrientation.IsSide())
      {
        this.stars.AddFace(Vector3.One, faceOrientation.AsVector() / 2f, faceOrientation, true);
        SkyHost.starSideOffsets[index] = (float) index++ / 4f;
      }
    }
    this.CameraManager.ViewChanged += new Action(this.OnViewChanged);
    this.CameraManager.ViewpointChanged += new Action(this.OnViewpointChanged);
    this.TimeManager.Tick += new Action(this.UpdateTimeOfDay);
  }

  protected override void LoadContent()
  {
    DrawActionScheduler.Schedule((Action) (() => this.stars.Effect = (BaseEffect) new StarsEffect()));
    this.shootingStar = this.CMProvider.Global.Load<AnimatedTexture>("Background Planes/shootingstar");
    this.sShootingStar = this.CMProvider.Global.Load<SoundEffect>("Sounds/Nature/ShootingStar");
    this.LevelManager.SkyChanged += new Action(this.InitializeSky);
  }

  private void InitializeSky()
  {
    if (this.LevelManager.Sky == null)
      return;
    ContentManager cm = this.LevelManager.Name == null ? this.CMProvider.Global : this.CMProvider.GetForLevel(this.LevelManager.Name);
    string skyPath = $"Skies/{this.LevelManager.Sky.Name}/";
    if (this.LevelManager.Sky.Name == this.lastSkyName && !this.EngineState.InEditor)
    {
      foreach (string cloud in this.LevelManager.Sky.Clouds)
        cm.Load<Texture2D>(skyPath + cloud);
      foreach (SkyLayer layer in this.LevelManager.Sky.Layers)
        cm.Load<Texture2D>(skyPath + layer.Name);
      this.skyBackground = cm.Load<Texture2D>(skyPath + this.LevelManager.Sky.Background);
      if (this.LevelManager.Sky.Stars == null)
        return;
      cm.Load<Texture2D>(skyPath + this.LevelManager.Sky.Stars);
    }
    else
    {
      this.lastSkyName = this.LevelManager.Sky.Name;
      if (this.LevelManager.Sky.Stars != null)
        DrawActionScheduler.Schedule((Action) (() => this.stars.Texture = (Dirtyable<Texture>) (Texture) cm.Load<Texture2D>(skyPath + this.LevelManager.Sky.Stars)));
      else
        this.stars.Texture.Set((Texture) null);
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.skyBackground = cm.Load<Texture2D>(skyPath + this.LevelManager.Sky.Background);
        this.fogColors = new Color[this.skyBackground.Width];
        this.skyBackground.GetData<Color>(0, new Rectangle?(new Rectangle(0, this.skyBackground.Height / 2, this.skyBackground.Width, 1)), this.fogColors, 0, this.skyBackground.Width);
      }));
      if (this.LevelManager.Sky.CloudTint != null)
        DrawActionScheduler.Schedule((Action) (() =>
        {
          Texture2D texture2D = cm.Load<Texture2D>(skyPath + this.LevelManager.Sky.CloudTint);
          this.cloudColors = new Color[texture2D.Width];
          texture2D.GetData<Color>(0, new Rectangle?(new Rectangle(0, texture2D.Height / 2, texture2D.Width, 1)), this.cloudColors, 0, texture2D.Width);
        }));
      else
        this.cloudColors = new Color[1]{ Color.White };
      this.cloudStates.Clear();
      foreach (Mesh cloudMesh in this.cloudMeshes)
        cloudMesh.Dispose();
      this.cloudMeshes.Clear();
      if (this.BgLayers != null)
      {
        this.BgLayers.ClearGroups();
      }
      else
      {
        this.BgLayers = new Mesh()
        {
          AlwaysOnTop = true,
          DepthWrites = false
        };
        DrawActionScheduler.Schedule((Action) (() => this.BgLayers.Effect = (BaseEffect) new DefaultEffect.Textured()));
      }
      int num1 = 0;
      foreach (SkyLayer layer in this.LevelManager.Sky.Layers)
      {
        int num2 = 0;
        foreach (FaceOrientation faceOrientation in Util.GetValues<FaceOrientation>())
        {
          if (faceOrientation.IsSide())
          {
            Group group = this.BgLayers.AddFace(Vector3.One, -faceOrientation.AsVector() / 2f, faceOrientation, true);
            group.AlwaysOnTop = new bool?(layer.InFront);
            group.Material = new Material()
            {
              Opacity = layer.Opacity
            };
            group.CustomData = (object) new SkyHost.BgLayerState()
            {
              Layer = num1,
              Side = num2++,
              OriginalOpacity = layer.Opacity
            };
          }
        }
        ++num1;
      }
      DrawActionScheduler.Schedule((Action) (() =>
      {
        int num3 = 0;
        foreach (SkyLayer layer in this.LevelManager.Sky.Layers)
        {
          Texture2D texture2D1 = cm.Load<Texture2D>(skyPath + layer.Name);
          Texture2D texture2D2 = (Texture2D) null;
          if (layer.Name == "OBS_SKY_A")
            texture2D2 = cm.Load<Texture2D>(skyPath + "OBS_SKY_C");
          foreach (FaceOrientation orientation in Util.GetValues<FaceOrientation>())
          {
            if (orientation.IsSide())
              this.BgLayers.Groups[num3++].Texture = texture2D2 == null || orientation == FaceOrientation.Left ? (Texture) texture2D1 : (Texture) texture2D2;
          }
        }
      }));
      foreach (Layer key in Util.GetValues<Layer>())
        this.cloudStates.Add(key, new List<SkyHost.CloudState>());
      foreach (string cloud in this.LevelManager.Sky.Clouds)
        this.cloudMeshes.Add(new Mesh()
        {
          AlwaysOnTop = true,
          DepthWrites = false,
          Culling = CullMode.None,
          SamplerState = SamplerState.PointClamp
        });
      DrawActionScheduler.Schedule((Action) (() =>
      {
        int index = 0;
        foreach (string cloud in this.LevelManager.Sky.Clouds)
        {
          this.cloudMeshes[index].Effect = (BaseEffect) new CloudsEffect();
          this.cloudMeshes[index].Texture = (Dirtyable<Texture>) (Texture) cm.Load<Texture2D>(skyPath + cloud);
          ++index;
        }
      }));
      double num4;
      int num5 = (int) Math.Sqrt(num4 = 64.0 * (double) this.LevelManager.Sky.Density);
      double num6 = (double) num5;
      float num7 = (float) (num4 / num6);
      float num8 = RandomHelper.Between(0.0, 6.2831854820251465);
      float num9 = RandomHelper.Between(0.0, 192.0);
      if (this.cloudMeshes.Count > 0)
      {
        for (int index1 = 0; index1 < num5; ++index1)
        {
          for (int index2 = 0; (double) index2 < (double) num7; ++index2)
          {
            Group group = RandomHelper.InList<Mesh>(this.cloudMeshes).AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, true);
            float num10 = RandomHelper.Between(0.0, 1.0 / (double) num5 * 6.2831854820251465);
            float num11 = RandomHelper.Between(0.0, 1.0 / (double) num7 * 192.0);
            this.cloudStates[RandomHelper.EnumField<Layer>()].Add(new SkyHost.CloudState()
            {
              Group = group,
              Phi = (float) (((double) index1 / (double) num5 * 6.2831854820251465 + (double) num8 + (double) num10) % 6.2831854820251465),
              LocalHeightOffset = (float) (((double) index2 / (double) num7 * 96.0 * 2.0 + (double) num9 + (double) num11) % 192.0 - 96.0)
            });
            group.Material = new Material();
          }
        }
      }
      this.flickerIn = RandomHelper.Between(2.0, 10.0);
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.ResizeLayers();
        this.ResizeStars();
        this.OnViewpointChanged();
      }));
      this.OnViewChanged();
    }
  }

  private Color CurrentFogColor
  {
    get
    {
      float num = this.TimeManager.DayFraction * (float) this.fogColors.Length;
      if ((double) num == (double) this.fogColors.Length)
        num = 0.0f;
      this.TimeManager.CurrentFogColor = Color.Lerp(this.fogColors[Math.Max((int) Math.Floor((double) num), 0)], this.fogColors[Math.Min((int) Math.Ceiling((double) num), this.fogColors.Length - 1)], FezMath.Frac(num));
      this.TimeManager.CurrentAmbientFactor = Math.Max(this.TimeManager.CurrentFogColor.ToVector3().Dot(new Vector3(0.333333343f)), 0.1f);
      for (int index = 0; index < 4; ++index)
        GamePad.SetLightBarEXT((PlayerIndex) index, this.TimeManager.CurrentFogColor);
      return this.TimeManager.CurrentFogColor;
    }
  }

  private Color CurrentCloudTint
  {
    get
    {
      float num1 = this.TimeManager.DayFraction * (float) this.cloudColors.Length;
      if ((double) num1 == (double) this.cloudColors.Length)
        num1 = 0.0f;
      Color cloudColor1 = this.cloudColors[Math.Max((int) Math.Floor((double) num1), 0)];
      Color cloudColor2 = this.cloudColors[Math.Min((int) Math.Ceiling((double) num1), this.cloudColors.Length - 1)];
      float num2 = FezMath.Frac(num1);
      Color color = cloudColor2;
      double amount = (double) num2;
      return Color.Lerp(cloudColor1, color, (float) amount);
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Paused || this.EngineState.InMap || this.EngineState.Loading && !this.EngineState.FarawaySettings.InTransition || this.LevelManager.Sky == null || this.LevelManager.Name == null)
      return;
    this.ForceUpdate(gameTime);
  }

  private void ForceUpdate(GameTime gameTime)
  {
    float elapsedTime = (float) gameTime.ElapsedGameTime.TotalSeconds * (this.TimeManager.TimeFactor / 360f);
    Vector3 a = this.LevelManager.Size == Vector3.Zero ? new Vector3(16f) : this.LevelManager.Size;
    Vector3 vector3 = a / 2f;
    Vector3 forward = this.CameraManager.View.Forward;
    if ((double) forward.Z != 0.0)
      forward.Z *= -1f;
    double currentAmbientFactor = (double) this.TimeManager.CurrentAmbientFactor;
    double num = (double) Math.Abs(a.Dot(this.CameraManager.Viewpoint.RightVector())) / 32.0;
    if (this.CameraManager.ActionRunning)
      this.sinceReached += gameTime.ElapsedGameTime;
    if (this.CameraManager.Viewpoint.IsOrthographic() && this.LevelManager.Sky.HorizontalScrolling)
    {
      foreach (Group group in this.BgLayers.Groups)
        (group.CustomData as SkyHost.BgLayerState).WindOffset += (float) ((double) elapsedTime * (double) this.LevelManager.Sky.WindSpeed * 0.02500000037252903);
    }
    this.ShootStars();
    this.ResizeStars();
    this.DoFlicker(elapsedTime);
  }

  private void ShootStars()
  {
    if (this.LevelManager.Sky == null || !this.CameraManager.Viewpoint.IsOrthographic() || this.LevelManager.Rainy || (double) this.TimeManager.TimeFactor <= 0.0 || this.LevelManager.Sky.Stars == null || (double) this.TimeManager.NightContribution != 1.0 || !RandomHelper.Probability(5E-05) || this.LevelManager.Name == "TELESCOPE" || this.LevelManager.Sky.Name == "TREE")
      return;
    Vector3 position = this.CameraManager.Center + this.LevelManager.Size / 2f * this.CameraManager.Viewpoint.ForwardVector() + new Vector3(RandomHelper.Centered((double) this.CameraManager.Radius / 2.0 - (double) this.shootingStar.FrameWidth / 32.0)) * this.CameraManager.Viewpoint.SideMask() + RandomHelper.Between(-(double) this.CameraManager.Radius / (double) this.CameraManager.AspectRatio / 6.0, (double) this.CameraManager.Radius / (double) this.CameraManager.AspectRatio / 2.0 - (double) this.shootingStar.FrameHeight / 32.0) * Vector3.UnitY;
    BackgroundPlane plane = new BackgroundPlane(this.LevelMaterializer.AnimatedPlanesMesh, this.shootingStar)
    {
      Position = position,
      Rotation = this.CameraManager.Rotation,
      Doublesided = true,
      Loop = false,
      Fullbright = true,
      Opacity = this.TimeManager.NightContribution,
      Timing = {
        Step = 0.0f
      }
    };
    this.sShootingStar.EmitAt(position);
    this.LevelManager.AddPlane(plane);
  }

  private void OnViewpointChanged()
  {
    foreach (Layer key in this.cloudStates.Keys)
    {
      foreach (Group group in this.cloudStates[key].Select<SkyHost.CloudState, Group>((Func<SkyHost.CloudState, Group>) (x => x.Group)))
      {
        if (group.Mesh.Texture.Value != null)
          group.Scale = new Vector3((float) group.Mesh.TextureMap.Width / 16f, (float) group.Mesh.TextureMap.Height / 16f, 1f) * (this.CameraManager.Viewpoint.IsOrthographic() ? Vector3.One : new Vector3((float) (4.0 + (double) key.DistanceFactor() * 2.0)));
        if (!this.CameraManager.Viewpoint.IsOrthographic())
          group.Enabled = true;
      }
    }
    if (this.CameraManager.Viewpoint != Viewpoint.Perspective)
      return;
    float num = this.LevelManager.Sky.Name == "OBS_SKY" ? 1f : 1.5f;
    foreach (Group group in this.BgLayers.Groups)
    {
      Matrix matrix = group.TextureMatrix.Value ?? Matrix.Identity;
      matrix.M31 += (float) (-(double) matrix.M11 / 2.0 + (double) matrix.M11 / (2.0 * (double) num));
      matrix.M32 += (float) ((double) matrix.M22 / 2.0 - (double) matrix.M22 / (2.0 * (double) num));
      matrix.M11 /= num;
      matrix.M22 /= num;
      group.TextureMatrix.Set(new Matrix?(matrix));
    }
  }

  public void RotateLayer(int layerId, Quaternion rotation)
  {
    this.BgLayers.Groups[layerId].Rotation = rotation;
  }

  private void DoFlicker(float elapsedTime)
  {
    if (this.LevelManager.Sky == null || this.LevelManager.Sky.Name != "INDUS_CITY")
      return;
    this.flickerIn -= elapsedTime;
    if ((double) this.flickerIn > 0.0)
      return;
    if ((double) this.flickerCount == -1.0)
    {
      this.flickerCount = (float) RandomHelper.Random.Next(2, 6);
      this.flickering = false;
    }
    this.flickerIn = RandomHelper.Between(0.05000000074505806, 0.25);
    for (int index = 0; index < 16 /*0x10*/; ++index)
      this.BgLayers.Groups[index].Material.Opacity = !this.flickering ? 0.0f : (this.BgLayers.Groups[index].CustomData as SkyHost.BgLayerState).OriginalOpacity;
    if (!this.flickering)
      this.SoundManager.MuteAmbience("Ambience ^ rain", 0.0f);
    else
      this.SoundManager.UnmuteAmbience("Ambience ^ rain", 0.0f);
    this.flickering = !this.flickering;
    --this.flickerCount;
    if ((double) this.flickerCount != 0.0)
      return;
    this.SoundManager.UnmuteAmbience("Ambience ^ rain", 0.0f);
    this.flickerCount = -1f;
    this.flickering = false;
    this.flickerIn = RandomHelper.Between(2.0, 4.0);
    for (int index = 0; index < 16 /*0x10*/; ++index)
      this.BgLayers.Groups[index].Material.Opacity = (this.BgLayers.Groups[index].CustomData as SkyHost.BgLayerState).OriginalOpacity;
  }

  private void OnViewChanged()
  {
    if (this.EngineState.LoopRender || this.EngineState.SkyRender)
      return;
    this.UpdateLayerAndCloudParallax();
  }

  private void UpdateLayerAndCloudParallax()
  {
    if (this.EngineState.Paused || this.EngineState.InMap || this.EngineState.Loading && !this.EngineState.FarawaySettings.InTransition || this.LevelManager.Sky == null)
      return;
    Vector3 vector3_1 = this.CameraManager.Position - this.CameraManager.ViewOffset;
    if (this.BgLayers != null && this.BgLayers.Groups.Count != 0)
      this.BgLayers.Position = vector3_1;
    Vector3 b = this.CameraManager.InterpolatedCenter - this.CameraManager.ViewOffset;
    if (!this.CameraManager.ActionRunning)
      this.sinceReached = TimeSpan.Zero;
    if (this.CameraManager.Viewpoint.IsOrthographic())
    {
      Vector3 vector3_2 = this.CameraManager.Viewpoint.RightVector();
      float num1 = vector3_2.Dot(b) - vector3_2.Dot(this.lastCamPos);
      Quaternion rotation = this.CameraManager.Rotation;
      float num2 = Math.Abs((this.LevelManager.Size + Vector3.One * 32f).Dot(vector3_2));
      bool flag = this.CameraManager.ActionRunning && this.sinceReached.TotalSeconds > 1.0;
      foreach (Layer key in this.cloudStates.Keys)
      {
        float num3 = MathHelper.Lerp(1f, key.ParallaxFactor(), this.LevelManager.Sky.CloudsParallax);
        foreach (SkyHost.CloudState cloudState in this.cloudStates[key])
        {
          while ((double) cloudState.GetHeight(0.2f) - (double) b.Y > 19.200000762939453)
            cloudState.GlobalHeightOffset -= 38.4f;
          while ((double) cloudState.GetHeight(0.2f) - (double) b.Y < -19.200000762939453)
            cloudState.GlobalHeightOffset += 38.4f;
          if (flag)
          {
            cloudState.GlobalHeightOffset += num3 * (b.Y - this.lastCamPos.Y);
            cloudState.Phi -= num3 * 2.25f * num1 / num2;
          }
          if (cloudState.Group.Enabled)
            cloudState.Group.Rotation = rotation;
        }
      }
    }
    if (!this.CameraManager.ActionRunning)
      return;
    this.lastCamPos = b;
  }

  private void MoveAndRotateClouds(GameTime gameTime)
  {
    float num1 = (float) gameTime.ElapsedGameTime.TotalSeconds * (this.TimeManager.TimeFactor / 360f);
    Vector3 a = this.LevelManager.Size == Vector3.Zero ? new Vector3(16f) : this.LevelManager.Size;
    Vector3 vector3_1 = a / 2f;
    Vector3 forward = this.CameraManager.View.Forward;
    if ((double) forward.Z != 0.0)
      forward.Z *= -1f;
    float currentAmbientFactor = this.TimeManager.CurrentAmbientFactor;
    float num2 = Math.Abs(a.Dot(this.CameraManager.Viewpoint.RightVector())) / 32f;
    if (this.CameraManager.ActionRunning)
      this.sinceReached += gameTime.ElapsedGameTime;
    bool flag1 = this.CameraManager.Viewpoint.IsOrthographic();
    float num3 = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    foreach (Layer key in this.cloudStates.Keys)
    {
      float num4 = (float) ((double) num1 * (double) this.LevelManager.Sky.WindSpeed * 0.02500000037252903) * key.SpeedFactor() / num2;
      Vector3 vector3_2 = !flag1 ? vector3_1 + Vector3.One * (float) (32.0 + 48.0 * (double) key.DistanceFactor()) : vector3_1 + Vector3.One * 32f / 2.5f * num3;
      foreach (SkyHost.CloudState cloudState in this.cloudStates[key])
      {
        if (flag1)
          cloudState.Phi -= num4;
        else
          cloudState.GlobalHeightOffset = this.CameraManager.Center.Y;
        float spreadFactor = !flag1 ? 1f : (this.CameraManager.ProjectionTransition ? MathHelper.Lerp(1f, 0.2f, this.CameraManager.ViewTransitionStep) : 0.2f);
        Vector3 vector3_3 = new Vector3((float) Math.Sin((double) cloudState.Phi) * vector3_2.X + vector3_1.X, cloudState.GetHeight(spreadFactor), (float) Math.Cos((double) cloudState.Phi) * vector3_2.Z + vector3_1.Z);
        Quaternion fromYawPitchRoll = Quaternion.CreateFromYawPitchRoll(cloudState.Phi, 3.14159274f, 3.14159274f);
        if (this.CameraManager.ProjectionTransition)
        {
          if (flag1)
          {
            int width = cloudState.Group.Mesh.TextureMap.Width;
            int height = cloudState.Group.Mesh.TextureMap.Height;
            cloudState.Group.Scale = new Vector3((float) width, (float) height, (float) width) / 16f * (2f - this.CameraManager.ViewTransitionStep);
            cloudState.Group.Rotation = Quaternion.Slerp(fromYawPitchRoll, this.CameraManager.Rotation, this.CameraManager.ViewTransitionStep);
            cloudState.ActualVisibility = (double) (vector3_3 - vector3_1).Dot(forward) <= 0.0;
          }
          else
          {
            cloudState.Group.Rotation = Quaternion.Slerp(this.CameraManager.Rotation, fromYawPitchRoll, this.CameraManager.ViewTransitionStep);
            cloudState.ActualVisibility = true;
          }
        }
        else if (flag1)
        {
          bool flag2 = (double) (vector3_3 - vector3_1).Dot(forward) <= 0.0;
          if (!cloudState.ActualVisibility & flag2)
          {
            cloudState.Group.Rotation = this.CameraManager.Rotation;
            cloudState.VisibilityFactor = 0.0f;
          }
          cloudState.ActualVisibility = flag2;
        }
        else
          cloudState.Group.Rotation = fromYawPitchRoll;
        if (!flag1 || cloudState.Group.Enabled)
          cloudState.Group.Position = vector3_3;
        cloudState.VisibilityFactor = FezMath.Saturate(cloudState.VisibilityFactor + (float) (gameTime.ElapsedGameTime.TotalSeconds * 5.0 * (cloudState.ActualVisibility ? 1.0 : -1.0)));
        cloudState.Group.Material.Opacity = key.Opacity() * currentAmbientFactor * Easing.EaseIn((double) cloudState.VisibilityFactor, EasingType.Quadratic);
        cloudState.Group.Enabled = (double) cloudState.Group.Material.Opacity > 1.0 / 510.0;
      }
    }
  }

  private void UpdateTimeOfDay()
  {
    if (this.LevelManager.Sky == null)
      return;
    this.backgroundMatrix.M11 = 0.0001f;
    this.backgroundMatrix.M31 = this.TimeManager.DayFraction;
    if (this.fogColors != null)
    {
      this.FogManager.Color = this.CurrentFogColor;
      foreach (Group group in this.BgLayers.Groups)
      {
        Material material = group.Material;
        Color color = this.CurrentCloudTint;
        Vector3 vector3_1 = color.ToVector3();
        color = this.FogManager.Color;
        Vector3 vector3_2 = color.ToVector3();
        double fogTint = (double) this.LevelManager.Sky.Layers[((SkyHost.BgLayerState) group.CustomData).Layer].FogTint;
        Vector3 vector3_3 = Vector3.Lerp(vector3_1, vector3_2, (float) fogTint);
        material.Diffuse = vector3_3;
      }
    }
    this.stars.Material.Opacity = this.LevelManager.Rainy || this.LevelManager.Sky.Name == "PYRAMID_SKY" || this.LevelManager.Sky.Name == "ABOVE" ? 1f : (this.LevelManager.Sky.Name == "OBS_SKY" ? MathHelper.Lerp(this.TimeManager.NightContribution, 1f, 0.25f) : this.TimeManager.NightContribution);
  }

  private void ResizeStars()
  {
    if (this.LevelManager.Sky == null || this.stars.TextureMap == null || !this.LevelManager.Rainy && !(this.LevelManager.Sky.Name == "ABOVE") && !(this.LevelManager.Sky.Name == "PYRAMID_SKY") && !(this.LevelManager.Sky.Name == "OBS_SKY") && (double) this.TimeManager.NightContribution == 0.0)
      return;
    if (this.EngineState.FarawaySettings.InTransition)
    {
      float viewScale = this.GraphicsDevice.GetViewScale();
      if (!this.stars.Effect.ForcedProjectionMatrix.HasValue)
      {
        this.sideToSwap = (int) this.CameraManager.Viewpoint.VisibleOrientation().GetOpposite();
        if (this.sideToSwap > 1)
          --this.sideToSwap;
        if (this.sideToSwap == 4)
          --this.sideToSwap;
      }
      float num1 = (float) this.GraphicsDevice.Viewport.Width / (this.CameraManager.PixelsPerTrixel * 16f);
      float num2;
      if ((double) this.EngineState.FarawaySettings.OriginFadeOutStep == 1.0)
      {
        float amount = Easing.EaseInOut(((double) this.EngineState.FarawaySettings.TransitionStep - (double) this.startStep) / (1.0 - (double) this.startStep), EasingType.Sine);
        num2 = num1 = MathHelper.Lerp(num1, this.EngineState.FarawaySettings.DestinationRadius, amount);
      }
      else
        num2 = num1;
      this.stars.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(num1 / viewScale, num1 / this.CameraManager.AspectRatio / viewScale, this.CameraManager.NearPlane, this.CameraManager.FarPlane));
      int opposite = (int) this.CameraManager.Viewpoint.VisibleOrientation().GetOpposite();
      if (opposite > 1)
        --opposite;
      if (opposite == 4)
        --opposite;
      if (opposite != this.sideToSwap)
      {
        float starSideOffset = SkyHost.starSideOffsets[opposite];
        SkyHost.starSideOffsets[opposite] = SkyHost.starSideOffsets[this.sideToSwap];
        SkyHost.starSideOffsets[this.sideToSwap] = starSideOffset;
        this.sideToSwap = opposite;
      }
      this.stars.Scale = new Vector3(1f, 5f, 1f) * num2 * 2f;
    }
    else
    {
      if (this.waiter == null && this.stars.Effect.ForcedProjectionMatrix.HasValue)
        this.waiter = Waiters.Wait(1.0, (Action) (() =>
        {
          this.stars.Effect.ForcedProjectionMatrix = new Matrix?();
          this.waiter = (IWaiter) null;
        }));
      float num = this.CameraManager.Viewpoint.IsOrthographic() ? 1f - this.CameraManager.ViewTransitionStep : this.CameraManager.ViewTransitionStep;
      this.stars.Scale = new Vector3(1f, 5f, 1f) * (float) ((double) this.CameraManager.Radius * 2.0 + (double) Easing.EaseOut(this.CameraManager.ProjectionTransition ? (double) num : 1.0, EasingType.Quintic) * 40.0);
    }
    int num3 = 0;
    foreach (Group group in this.stars.Groups)
    {
      float m11 = this.stars.Scale.X / ((float) this.stars.TextureMap.Width / 16f);
      float m22 = this.stars.Scale.Y / ((float) this.stars.TextureMap.Height / 16f);
      float starSideOffset = SkyHost.starSideOffsets[num3++];
      group.TextureMatrix.Set(new Matrix?(new Matrix(m11, 0.0f, 0.0f, 0.0f, 0.0f, m22, 0.0f, 0.0f, starSideOffset - m11 / 2f, starSideOffset - m22 / 2f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
    }
  }

  private void ResizeLayers()
  {
    if (this.BgLayers == null || this.BgLayers.Groups.Count == 0 || this.EngineState.SkyRender || this.LevelManager.Sky == null)
      return;
    float num1 = 0.0f;
    float viewScale = this.GraphicsDevice.GetViewScale();
    if (this.EngineState.FarawaySettings.InTransition)
    {
      float num2 = (float) this.GraphicsDevice.Viewport.Width / (this.CameraManager.PixelsPerTrixel * 16f);
      if ((double) this.EngineState.FarawaySettings.OriginFadeOutStep == 1.0)
      {
        float num3 = this.startStep;
        if ((double) num3 == 0.0)
          num3 = 0.1275f;
        float amount = Easing.EaseInOut(((double) this.EngineState.FarawaySettings.TransitionStep - (double) num3) / (1.0 - (double) num3), EasingType.Sine);
        num1 = num2 = MathHelper.Lerp(num2, this.EngineState.FarawaySettings.DestinationRadius, amount);
      }
      this.BgLayers.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(num2 / viewScale, num2 / this.CameraManager.AspectRatio / viewScale, this.CameraManager.NearPlane, this.CameraManager.FarPlane));
    }
    else if (this.BgLayers.Effect.ForcedProjectionMatrix.HasValue)
      this.BgLayers.Effect.ForcedProjectionMatrix = new Matrix?();
    Vector3 vector3 = new Vector3(this.CameraManager.InterpolatedCenter.X, this.CameraManager.Position.Y, this.CameraManager.InterpolatedCenter.Z);
    if (this.EngineState.FarawaySettings.InTransition)
      vector3 = this.BgLayers.Position = this.CameraManager.Position;
    float num4;
    float num5;
    if (this.EngineState.FarawaySettings.InTransition && (double) this.EngineState.FarawaySettings.OriginFadeOutStep == 1.0)
    {
      float num6 = this.CameraManager.PixelsPerTrixel;
      if (this.EngineState.FarawaySettings.InTransition && FezMath.AlmostEqual(this.EngineState.FarawaySettings.DestinationCrossfadeStep, 1f))
        num6 = MathHelper.Lerp(this.CameraManager.PixelsPerTrixel, this.EngineState.FarawaySettings.DestinationPixelsPerTrixel, (float) (((double) this.EngineState.FarawaySettings.TransitionStep - 0.875) / 0.125));
      float num7 = (float) ((double) (-4 * (this.LevelManager.Descending ? -1 : 1)) / (double) num6 - 15.0 / 32.0 + 1.0);
      num4 = -this.EngineState.FarawaySettings.DestinationOffset.X;
      num5 = -this.EngineState.FarawaySettings.DestinationOffset.Y + num7;
      if (!this.EngineState.Loading)
      {
        if ((double) this.startStep2 == 0.0)
          this.startStep2 = this.EngineState.FarawaySettings.TransitionStep;
        num4 = MathHelper.Lerp(num4, (vector3 - this.LevelManager.Size / 2f).Dot(this.CameraManager.InverseView.Right), Easing.EaseInOut(((double) this.EngineState.FarawaySettings.TransitionStep - (double) this.startStep2) / (1.0 - (double) this.startStep2), EasingType.Sine));
        num5 = MathHelper.Lerp(num5, vector3.Y - this.LevelManager.Size.Y / 2f - this.CameraManager.ViewOffset.Y, Easing.EaseInOut(((double) this.EngineState.FarawaySettings.TransitionStep - (double) this.startStep2) / (1.0 - (double) this.startStep2), EasingType.Sine));
      }
    }
    else
    {
      num4 = (vector3 - this.LevelManager.Size / 2f).Dot(this.CameraManager.InverseView.Right);
      num5 = vector3.Y - this.LevelManager.Size.Y / 2f - this.CameraManager.ViewOffset.Y;
    }
    if (this.LevelManager.Sky.NoPerFaceLayerXOffset)
      this.sideOffset = num4;
    else if (this.CameraManager.ActionRunning && this.CameraManager.Viewpoint.IsOrthographic())
    {
      if (this.sinceReached.TotalSeconds > 0.45)
        this.sideOffset -= this.lastCamSide - num4;
      this.lastCamSide = num4;
    }
    float num8 = this.CameraManager.Viewpoint.IsOrthographic() ? 1f - this.CameraManager.ViewTransitionStep : this.CameraManager.ViewTransitionStep;
    this.BgLayers.Scale = !this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.ProjectionTransition ? new Vector3(1f, 5f, 1f) * (float) ((double) this.CameraManager.Radius * 2.0 + (double) Easing.EaseOut(this.CameraManager.ProjectionTransition ? (double) num8 : 1.0, EasingType.Quintic) * 40.0) : (!this.EngineState.FarawaySettings.InTransition || (double) this.EngineState.FarawaySettings.OriginFadeOutStep != 1.0 ? new Vector3(1f, 5f, 1f) * this.CameraManager.Radius * 2f / viewScale : new Vector3(1f, 5f, 1f) * num1 * 2f);
    foreach (Group group in this.BgLayers.Groups)
    {
      group.Enabled = false;
      SkyHost.BgLayerState customData = (SkyHost.BgLayerState) group.CustomData;
      float num9 = (float) customData.Layer / (this.LevelManager.Sky.Layers.Count == 1 ? 1f : (float) (this.LevelManager.Sky.Layers.Count - 1));
      int num10 = 1;
      float num11 = this.BgLayers.Scale.X / ((float) group.TextureMap.Width / 16f) / (float) num10;
      float m22 = this.BgLayers.Scale.Y / ((float) group.TextureMap.Height / 16f) / (float) num10;
      if (this.CameraManager.ProjectionTransition)
        group.Scale = Vector3.One + FezMath.XZMask * num9 * 0.125f * num8;
      Vector2 vector2_1 = new Vector2(this.sideOffset / ((float) group.TextureMap.Width / 16f), num5 / ((float) group.TextureMap.Height / 16f));
      if (this.EngineState.FarawaySettings.InTransition && (double) this.EngineState.FarawaySettings.OriginFadeOutStep != 1.0)
      {
        customData.OriginalTC = vector2_1;
        this.startStep = 0.0f;
        this.startStep2 = 0.0f;
      }
      else if (this.EngineState.FarawaySettings.InTransition && (double) this.EngineState.FarawaySettings.OriginFadeOutStep == 1.0)
      {
        if (vector2_1 != customData.OriginalTC && (double) this.startStep == 0.0)
          this.startStep = this.EngineState.FarawaySettings.TransitionStep;
        if ((double) this.startStep != 0.0)
          vector2_1 = Vector2.Lerp(customData.OriginalTC, vector2_1, Easing.EaseInOut(((double) this.EngineState.FarawaySettings.TransitionStep - (double) this.startStep) / (1.0 - (double) this.startStep), EasingType.Sine));
      }
      Vector2 vector2_2;
      vector2_2.X = (float) ((this.LevelManager.Sky.NoPerFaceLayerXOffset ? 0.0 : (double) customData.Side / 4.0) + (double) this.LevelManager.Sky.LayerBaseXOffset + (double) vector2_1.X * (double) this.LevelManager.Sky.HorizontalDistance + (double) vector2_1.X * (double) this.LevelManager.Sky.InterLayerHorizontalDistance * (double) num9 + -(double) customData.WindOffset * (double) this.LevelManager.Sky.WindDistance + -(double) customData.WindOffset * (double) this.LevelManager.Sky.WindParallax * (double) num9);
      if (!this.LevelManager.Sky.VerticalTiling)
        num9 -= 0.5f;
      vector2_2.Y = (float) ((double) this.LevelManager.Sky.LayerBaseHeight + (double) num9 * (double) this.LevelManager.Sky.LayerBaseSpacing + -(double) vector2_1.Y * (double) this.LevelManager.Sky.VerticalDistance + -(double) num9 * (double) this.LevelManager.Sky.InterLayerVerticalDistance * (double) vector2_1.Y);
      group.TextureMatrix.Set(new Matrix?(new Matrix(-num11, 0.0f, 0.0f, 0.0f, 0.0f, m22, 0.0f, 0.0f, (float) (-(double) vector2_2.X + (double) num11 / 2.0), vector2_2.Y - m22 / 2f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
    }
  }

  public void DrawBackground()
  {
    this.GraphicsDevice.SamplerStates[0] = SamplerStates.LinearUWrapVClamp;
    this.TargetRenderer.DrawFullscreen((Texture) this.skyBackground, this.backgroundMatrix, new Color(this.EngineState.SkyOpacity, this.EngineState.SkyOpacity, this.EngineState.SkyOpacity, 1f));
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.EngineState.Loading && !this.EngineState.FarawaySettings.InTransition || this.BgLayers == null || (double) this.EngineState.SkyOpacity == 0.0 || this.LevelManager.Name == null || this.LevelManager.Sky == null || this.EngineState.InMap)
      return;
    this.ForceDraw(gameTime);
  }

  public void ForceDraw(GameTime gameTime)
  {
    if ((double) this.stars.Material.Opacity > 0.0)
      this.stars.Position = this.CameraManager.Position - this.CameraManager.ViewOffset;
    this.UpdateLayerAndCloudParallax();
    this.MoveAndRotateClouds(gameTime);
    this.ResizeLayers();
    RenderTarget2D renderTarget = (RenderTarget2D) null;
    bool flag1 = false;
    if ((double) this.EngineState.FarawaySettings.OriginFadeOutStep == 1.0 && this.RtHandle == null)
    {
      this.RtHandle = this.TargetRenderer.TakeTarget();
      renderTarget = this.GraphicsDevice.GetRenderTargets()[0].RenderTarget as RenderTarget2D;
      this.GraphicsDevice.SetRenderTarget(this.RtHandle.Target);
      this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1f, 0);
      flag1 = true;
      this.RadiusAtFirstDraw = this.CameraManager.Radius;
    }
    else if (this.RtHandle != null && !this.EngineState.FarawaySettings.InTransition)
    {
      this.TargetRenderer.ReturnTarget(this.RtHandle);
      this.RtHandle = (RenderTargetHandle) null;
    }
    this.EngineState.SkyRender = true;
    Vector3 viewOffset = this.CameraManager.ViewOffset;
    this.CameraManager.ViewOffset -= viewOffset;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Sky));
    graphicsDevice.SetBlendingMode(BlendingMode.Maximum);
    bool flag2 = this.LevelManager.Name == "INDUSTRIAL_CITY";
    if ((double) this.EngineState.FarawaySettings.OriginFadeOutStep < 1.0 | flag1 || (double) this.EngineState.FarawaySettings.DestinationCrossfadeStep > 0.0)
    {
      bool flag3 = (double) this.EngineState.FarawaySettings.DestinationCrossfadeStep > 0.0;
      if (!flag2 || this.flickering)
      {
        foreach (Mesh cloudMesh in this.cloudMeshes)
        {
          if (flag3)
            cloudMesh.Material.Opacity = this.EngineState.FarawaySettings.DestinationCrossfadeStep;
          float opacity = cloudMesh.Material.Opacity;
          if ((double) this.EngineState.SkyOpacity != 1.0)
            cloudMesh.Material.Opacity = opacity * this.EngineState.SkyOpacity;
          cloudMesh.Draw();
          cloudMesh.Material.Opacity = opacity;
        }
      }
    }
    if (this.RtHandle != null)
    {
      if (flag1)
      {
        this.GraphicsDevice.SetRenderTarget(renderTarget);
        this.GraphicsDevice.Clear(Color.Black);
      }
      float num1 = ((double) this.EngineState.FarawaySettings.InterpolatedFakeRadius == 0.0 ? this.RadiusAtFirstDraw : this.EngineState.FarawaySettings.InterpolatedFakeRadius) / this.RadiusAtFirstDraw;
      Matrix matrix1 = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, -0.5f, -0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
      Matrix matrix2 = new Matrix(num1, 0.0f, 0.0f, 0.0f, 0.0f, num1, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
      Matrix matrix3 = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.5f, 0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
      float num2 = num1 * (1f - this.EngineState.FarawaySettings.DestinationCrossfadeStep);
      this.TargetRenderer.DrawFullscreen((Texture) this.RtHandle.Target, matrix1 * matrix2 * matrix3, new Color(num2, num2, num2));
    }
    if (!flag2 || this.flickering)
    {
      graphicsDevice.SetBlendingMode(BlendingMode.Screen);
      this.DrawBackground();
    }
    if (this.stars.TextureMap != null && (double) this.stars.Material.Opacity > 0.0)
    {
      graphicsDevice.SetBlendingMode(BlendingMode.StarsOverClouds);
      float opacity = this.stars.Material.Opacity;
      this.stars.Material.Opacity = opacity * this.EngineState.SkyOpacity;
      this.stars.Draw();
      this.stars.Material.Opacity = opacity;
    }
    this.GraphicsDevice.SamplerStates[0] = this.LevelManager.Sky.VerticalTiling ? SamplerState.PointWrap : SamplerStates.PointUWrapVClamp;
    graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    if (this.LevelManager.Rainy && !flag2 || this.LevelManager.Sky.Name == "WATERFRONT")
    {
      int count = this.BgLayers.Groups.Count;
      int num = count / 3;
      for (int index1 = 0; index1 < 3; ++index1)
      {
        for (int index2 = index1 * num; index2 < count && index2 < (index1 + 1) * num; ++index2)
          this.BgLayers.Groups[index2].Enabled = ((int) this.BgLayers.Groups[index2].AlwaysOnTop ?? 0) == 0;
        graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?((FezEngine.Structure.StencilMask) (5 + index1)));
        this.BgLayers.Draw();
        for (int index3 = index1 * num; index3 < count && index3 < (index1 + 1) * num; ++index3)
          this.BgLayers.Groups[index3].Enabled = false;
      }
    }
    else
    {
      if (this.LevelManager.Name != null && (this.LevelManager.BlinkingAlpha || this.LevelManager.WaterType == LiquidType.Sewer && this.EngineState.StereoMode))
      {
        graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.SkyLayer1));
        graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
        this.LevelMaterializer.StaticPlanesMesh.AlwaysOnTop = true;
        this.LevelMaterializer.StaticPlanesMesh.DepthWrites = false;
        foreach (BackgroundPlane levelPlane in this.LevelMaterializer.LevelPlanes)
          levelPlane.Group.Enabled = levelPlane.Id < 0;
        this.LevelMaterializer.StaticPlanesMesh.Draw();
        this.LevelMaterializer.StaticPlanesMesh.AlwaysOnTop = false;
        this.LevelMaterializer.StaticPlanesMesh.DepthWrites = true;
        foreach (BackgroundPlane levelPlane in this.LevelMaterializer.LevelPlanes)
          levelPlane.Group.Enabled = true;
        graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
        graphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.SkyLayer1);
        graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
        this.GraphicsDevice.SamplerStates[0] = this.LevelManager.Sky.VerticalTiling ? SamplerState.PointWrap : SamplerStates.PointUWrapVClamp;
      }
      foreach (Group group in this.BgLayers.Groups)
        group.Enabled = ((int) group.AlwaysOnTop ?? 0) == 0;
      this.BgLayers.Draw();
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    }
    this.CameraManager.ViewOffset += viewOffset;
    this.EngineState.SkyRender = false;
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  [ServiceDependency]
  public IFogManager FogManager { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  private class CloudState
  {
    public float Phi;
    public float LocalHeightOffset;
    public float GlobalHeightOffset;
    public Group Group;
    public float VisibilityFactor;
    public bool ActualVisibility;

    public float GetHeight(float spreadFactor)
    {
      return this.LocalHeightOffset * spreadFactor + this.GlobalHeightOffset;
    }
  }

  private class BgLayerState
  {
    public int Layer;
    public int Side;
    public float WindOffset;
    public Vector2 OriginalTC;
    public float OriginalOpacity;
  }
}
