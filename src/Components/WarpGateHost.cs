// Decompiled with JetBrains decompiler
// Type: FezGame.Components.WarpGateHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class WarpGateHost : DrawableGameComponent
{
  private readonly Dictionary<WarpDestinations, WarpPanel> panels = new Dictionary<WarpDestinations, WarpPanel>((IEqualityComparer<WarpDestinations>) WarpDestinationsComparer.Default);
  private ArtObjectInstance warpGateAo;
  private string CurrentLevelName;
  private Vector3 InterpolatedCenter;

  public static WarpGateHost Instance { get; private set; }

  public WarpGateHost(Game game)
    : base(game)
  {
    this.DrawOrder = 10;
    WarpGateHost.Instance = this;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.Visible = this.Enabled = false;
  }

  private void TryInitialize()
  {
    this.warpGateAo = this.LevelManager.ArtObjects.Values.FirstOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.WarpGate));
    this.Visible = this.Enabled = this.warpGateAo != null;
    if (!this.Enabled)
    {
      foreach (KeyValuePair<WarpDestinations, WarpPanel> panel in this.panels)
      {
        panel.Value.PanelMask.Dispose();
        panel.Value.Layers.Dispose();
      }
      this.panels.Clear();
    }
    else
    {
      if (this.panels.Count == 0)
      {
        WarpPanel naturePanel = new WarpPanel()
        {
          Face = FaceOrientation.Front,
          Destination = "NATURE_HUB",
          PanelMask = new Mesh() { DepthWrites = false },
          Layers = new Mesh()
          {
            AlwaysOnTop = true,
            DepthWrites = false,
            SamplerState = SamplerState.PointClamp
          }
        };
        this.panels.Add(WarpDestinations.First, naturePanel);
        DrawActionScheduler.Schedule((Action) (() =>
        {
          Mesh panelMask = naturePanel.PanelMask;
          panelMask.Effect = (BaseEffect) new DefaultEffect.Textured();
          panelMask.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/fullwhite");
          panelMask.AddFace(new Vector3(3.875f), Vector3.Backward * (25f / 16f), FaceOrientation.Front, true);
          Mesh layers = naturePanel.Layers;
          layers.Effect = (BaseEffect) new DefaultEffect.Textured();
          Texture2D texture2D = this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/nature/background");
          layers.AddFace(new Vector3(10f, 4f, 10f), Vector3.Forward * 5f, FaceOrientation.Front, true).Texture = (Texture) texture2D;
          layers.AddFace(new Vector3(10f, 4f, 10f), Vector3.Right * 5f, FaceOrientation.Left, true).Texture = (Texture) texture2D;
          layers.AddFace(new Vector3(10f, 4f, 10f), Vector3.Left * 5f, FaceOrientation.Right, true).Texture = (Texture) texture2D;
          Group group1 = layers.AddFace(new Vector3(32f, 32f, 1f), new Vector3(0.0f, 2f, -8f), FaceOrientation.Front, true);
          group1.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/WATERFRONT/WATERFRONT_C");
          group1.Material = new Material()
          {
            Opacity = 0.3f,
            Diffuse = Vector3.Lerp(Vector3.One, new Vector3(0.1215686f, 0.96f, 1f), 0.7f)
          };
          Group group2 = layers.AddFace(new Vector3(32f, 32f, 1f), new Vector3(0.0f, 2f, -8f), FaceOrientation.Front, true);
          group2.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/WATERFRONT/WATERFRONT_B");
          group2.Material = new Material()
          {
            Opacity = 0.5f,
            Diffuse = Vector3.Lerp(Vector3.One, new Vector3(0.1215686f, 0.96f, 1f), 0.5f)
          };
          Group group3 = layers.AddFace(new Vector3(32f, 32f, 1f), new Vector3(0.0f, 2f, -8f), FaceOrientation.Front, true);
          group3.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/WATERFRONT/WATERFRONT_A");
          group3.Material = new Material()
          {
            Opacity = 1f,
            Diffuse = Vector3.Lerp(Vector3.One, new Vector3(0.1215686f, 0.96f, 1f), 0.4f)
          };
        }));
        WarpPanel graveyardPanel = new WarpPanel()
        {
          Face = FaceOrientation.Right,
          Destination = "GRAVEYARD_GATE",
          PanelMask = new Mesh() { DepthWrites = false },
          Layers = new Mesh()
          {
            AlwaysOnTop = true,
            DepthWrites = false,
            SamplerState = SamplerState.PointClamp
          }
        };
        this.panels.Add(WarpDestinations.Graveyard, graveyardPanel);
        DrawActionScheduler.Schedule((Action) (() =>
        {
          Mesh panelMask = graveyardPanel.PanelMask;
          panelMask.Effect = (BaseEffect) new DefaultEffect.Textured();
          panelMask.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/fullwhite");
          panelMask.AddFace(new Vector3(3.875f), Vector3.Right * (25f / 16f), FaceOrientation.Right, true);
          Mesh layers = graveyardPanel.Layers;
          layers.Effect = (BaseEffect) new DefaultEffect.Textured();
          Texture2D texture2D = this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/graveyard/back");
          Group group4 = layers.AddFace(Vector3.One * 16f, Vector3.Zero, FaceOrientation.Right, true);
          group4.Texture = (Texture) texture2D;
          group4.SamplerState = SamplerState.PointWrap;
          layers.AddFace(new Vector3(1f, 16f, 32f), new Vector3(-8f, 4f, 0.0f), FaceOrientation.Right, true).Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/GRAVE/GRAVE_CLOUD_C");
          layers.AddFace(new Vector3(1f, 16f, 32f), new Vector3(-8f, 4f, 0.0f), FaceOrientation.Right, true).Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/GRAVE/GRAVE_CLOUD_B");
          layers.AddFace(new Vector3(1f, 16f, 32f), new Vector3(-8f, 4f, 0.0f), FaceOrientation.Right, true).Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/GRAVE/GRAVE_CLOUD_A");
          Group group5 = layers.AddFace(Vector3.One * 16f, Vector3.Zero, FaceOrientation.Right, true);
          group5.SamplerState = SamplerState.PointWrap;
          group5.Blending = new BlendingMode?(BlendingMode.Additive);
          group5.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/graveyard/rainoverlay");
        }));
        WarpPanel industrialPanel = new WarpPanel()
        {
          Face = FaceOrientation.Left,
          Destination = "INDUSTRIAL_HUB",
          PanelMask = new Mesh() { DepthWrites = false },
          Layers = new Mesh()
          {
            AlwaysOnTop = true,
            DepthWrites = false,
            SamplerState = SamplerState.PointClamp
          }
        };
        this.panels.Add(WarpDestinations.Mechanical, industrialPanel);
        DrawActionScheduler.Schedule((Action) (() =>
        {
          Mesh panelMask = industrialPanel.PanelMask;
          panelMask.Effect = (BaseEffect) new DefaultEffect.Textured();
          panelMask.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/fullwhite");
          panelMask.AddFace(new Vector3(3.875f), Vector3.Left * (25f / 16f), FaceOrientation.Left, true);
          Mesh layers = industrialPanel.Layers;
          layers.Effect = (BaseEffect) new DefaultEffect.Textured();
          Texture2D texture2D = this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/industrial/background");
          layers.AddFace(new Vector3(10f, 4f, 10f), Vector3.Right * 5f, FaceOrientation.Left, true).Texture = (Texture) texture2D;
          layers.AddFace(new Vector3(10f, 4f, 10f), Vector3.Backward * 5f, FaceOrientation.Back, true).Texture = (Texture) texture2D;
          layers.AddFace(new Vector3(10f, 4f, 10f), Vector3.Forward * 5f, FaceOrientation.Front, true).Texture = (Texture) texture2D;
          Group group6 = layers.AddFace(new Vector3(1f, 8f, 8f), new Vector3(8f, 0.0f, 0.0f), FaceOrientation.Left, true);
          group6.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/industrial/INDUST_CLOUD_B");
          group6.Material = new Material()
          {
            Opacity = 0.5f
          };
          Group group7 = layers.AddFace(new Vector3(1f, 8f, 8f), new Vector3(8f, 0.0f, 0.0f), FaceOrientation.Left, true);
          group7.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/industrial/INDUST_CLOUD_F");
          group7.Material = new Material()
          {
            Opacity = 0.325f
          };
        }));
        WarpPanel sewerPanel = new WarpPanel()
        {
          Face = FaceOrientation.Back,
          Destination = "SEWER_HUB",
          PanelMask = new Mesh() { DepthWrites = false },
          Layers = new Mesh()
          {
            AlwaysOnTop = true,
            DepthWrites = false,
            SamplerState = SamplerState.PointClamp
          }
        };
        this.panels.Add(WarpDestinations.Sewers, sewerPanel);
        DrawActionScheduler.Schedule((Action) (() =>
        {
          Mesh panelMask = sewerPanel.PanelMask;
          panelMask.Effect = (BaseEffect) new DefaultEffect.Textured();
          panelMask.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/fullwhite");
          panelMask.AddFace(new Vector3(3.875f), Vector3.Forward * (25f / 16f), FaceOrientation.Back, true);
          Mesh layers = sewerPanel.Layers;
          layers.Effect = (BaseEffect) new DefaultEffect.Textured();
          Texture2D texture2D = this.CMProvider.Global.Load<Texture2D>("Skies/SEWER/BRICK_BACKGROUND");
          Group group8 = layers.AddFace(Vector3.One * 16f, Vector3.Backward * 8f, FaceOrientation.Back, true);
          group8.Texture = (Texture) texture2D;
          group8.SamplerState = SamplerState.PointWrap;
          Group group9 = layers.AddFace(Vector3.One * 16f, Vector3.Right * 8f, FaceOrientation.Left, true);
          group9.Texture = (Texture) texture2D;
          group9.SamplerState = SamplerState.PointWrap;
          Group group10 = layers.AddFace(Vector3.One * 16f, Vector3.Left * 8f, FaceOrientation.Right, true);
          group10.Texture = (Texture) texture2D;
          group10.SamplerState = SamplerState.PointWrap;
          Group group11 = layers.AddFace(new Vector3(128f, 8f, 1f), new Vector3(0.0f, 4f, -8f), FaceOrientation.Back, true);
          group11.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/sewer/sewage");
          group11.SamplerState = SamplerState.PointWrap;
        }));
        WarpPanel zuPanel = new WarpPanel()
        {
          Face = FaceOrientation.Front,
          Destination = "ZU_CITY_RUINS",
          PanelMask = new Mesh() { DepthWrites = false },
          Layers = new Mesh()
          {
            AlwaysOnTop = true,
            DepthWrites = false,
            SamplerState = SamplerState.PointClamp
          }
        };
        this.panels.Add(WarpDestinations.Zu, zuPanel);
        DrawActionScheduler.Schedule((Action) (() =>
        {
          Mesh panelMask = zuPanel.PanelMask;
          panelMask.Effect = (BaseEffect) new DefaultEffect.Textured();
          panelMask.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/fullwhite");
          panelMask.AddFace(new Vector3(3.875f), Vector3.Backward * (25f / 16f), FaceOrientation.Front, true);
          Mesh layers = zuPanel.Layers;
          layers.Effect = (BaseEffect) new DefaultEffect.Textured();
          Texture2D texture2D = this.CMProvider.Global.Load<Texture2D>("Other Textures/warp/zu/back");
          Group group12 = layers.AddFace(new Vector3(16f, 32f, 16f), Vector3.Zero, FaceOrientation.Front, true);
          group12.Texture = (Texture) texture2D;
          group12.SamplerState = SamplerState.PointWrap;
          Group group13 = layers.AddFace(new Vector3(32f, 32f, 1f), new Vector3(0.0f, 0.0f, -8f), FaceOrientation.Front, true);
          group13.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/ABOVE/ABOVE_C");
          group13.Material = new Material()
          {
            Opacity = 0.4f,
            Diffuse = Vector3.Lerp(new Vector3(0.6862745f, 1f, 0.97647f), new Vector3(0.129411772f, 0.4f, 1f), 0.7f)
          };
          Group group14 = layers.AddFace(new Vector3(32f, 32f, 1f), new Vector3(0.0f, 0.0f, -8f), FaceOrientation.Front, true);
          group14.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/ABOVE/ABOVE_B");
          group14.Material = new Material()
          {
            Opacity = 0.6f,
            Diffuse = Vector3.Lerp(new Vector3(0.6862745f, 1f, 0.97647f), new Vector3(0.129411772f, 0.4f, 1f), 0.6f)
          };
          Group group15 = layers.AddFace(new Vector3(32f, 32f, 1f), new Vector3(0.0f, 0.0f, -8f), FaceOrientation.Front, true);
          group15.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Skies/ABOVE/ABOVE_A");
          group15.Material = new Material()
          {
            Opacity = 1f,
            Diffuse = Vector3.Lerp(new Vector3(0.6862745f, 1f, 0.97647f), new Vector3(0.129411772f, 0.4f, 1f), 0.5f)
          };
        }));
      }
      if (Fez.LongScreenshot)
      {
        this.GameState.SaveData.UnlockedWarpDestinations.Add("SEWER_HUB");
        this.GameState.SaveData.UnlockedWarpDestinations.Add("GRAVEYARD_GATE");
        this.GameState.SaveData.UnlockedWarpDestinations.Add("INDUSTRIAL_HUB");
        this.GameState.SaveData.UnlockedWarpDestinations.Add("ZU_CITY_RUINS");
      }
      string str = this.LevelManager.Name.Replace('\\', '/');
      this.CurrentLevelName = str.Substring(str.LastIndexOf('/') + 1);
      if (!this.GameState.SaveData.UnlockedWarpDestinations.Contains(this.CurrentLevelName))
      {
        this.GameState.SaveData.UnlockedWarpDestinations.Add(this.CurrentLevelName);
      }
      else
      {
        Volume volume;
        if (this.GameState.SaveData.UnlockedWarpDestinations.Count > 1 && (volume = this.LevelManager.Volumes.Values.FirstOrDefault<Volume>((Func<Volume, bool>) (x => x.ActorSettings != null && x.ActorSettings.IsPointOfInterest && (double) Vector3.DistanceSquared(x.BoundingBox.GetCenter(), this.warpGateAo.Position) < 4.0))) != null)
        {
          volume.ActorSettings.DotDialogue.Clear();
          volume.ActorSettings.DotDialogue.AddRange((IEnumerable<DotDialogueLine>) new DotDialogueLine[3]
          {
            new DotDialogueLine()
            {
              ResourceText = "DOT_WARP_A",
              Grouped = true
            },
            new DotDialogueLine()
            {
              ResourceText = "DOT_WARP_B",
              Grouped = true
            },
            new DotDialogueLine()
            {
              ResourceText = "DOT_WARP_UP",
              Grouped = true
            }
          });
          bool flag;
          if (this.GameState.SaveData.OneTimeTutorials.TryGetValue("DOT_WARP_A", out flag) & flag)
            volume.ActorSettings.PreventHey = true;
        }
      }
      Vector3 zero = Vector3.Zero;
      if (this.warpGateAo.ArtObject.Name == "GATE_INDUSTRIALAO")
        zero -= Vector3.UnitY;
      foreach (WarpPanel warpPanel in this.panels.Values)
      {
        warpPanel.PanelMask.Position = this.warpGateAo.Position + zero;
        warpPanel.Layers.Position = this.warpGateAo.Position + zero;
        warpPanel.Enabled = warpPanel.Destination != this.CurrentLevelName && this.GameState.SaveData.UnlockedWarpDestinations.Contains(warpPanel.Destination);
        if (warpPanel.Destination == "ZU_CITY_RUINS")
        {
          switch (this.CurrentLevelName)
          {
            case "NATURE_HUB":
              warpPanel.Face = FaceOrientation.Front;
              warpPanel.PanelMask.Rotation = Quaternion.Identity;
              warpPanel.Layers.Rotation = Quaternion.Identity;
              continue;
            case "GRAVEYARD_GATE":
              warpPanel.Face = FaceOrientation.Right;
              warpPanel.PanelMask.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f);
              warpPanel.Layers.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f);
              continue;
            case "INDUSTRIAL_HUB":
              warpPanel.Face = FaceOrientation.Left;
              warpPanel.PanelMask.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 4.712389f);
              warpPanel.Layers.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 4.712389f);
              continue;
            case "SEWER_HUB":
              warpPanel.Face = FaceOrientation.Back;
              warpPanel.PanelMask.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f);
              warpPanel.Layers.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f);
              continue;
            default:
              continue;
          }
        }
      }
    }
  }

  protected override void LoadContent()
  {
    this.LightingPostProcess.DrawOnTopLights += new Action(this.DrawLights);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.warpGateAo.ActorSettings.Inactive || this.GameState.InMap || this.GameState.Paused)
      return;
    this.UpdateParallax(gameTime.ElapsedGameTime);
    this.UpdateDoors();
  }

  private void UpdateDoors()
  {
    if (!this.CameraManager.Viewpoint.IsOrthographic() || this.InputManager.Up != FezButtonState.Pressed || !this.PlayerManager.Grounded || this.PlayerManager.Action == ActionType.GateWarp || this.PlayerManager.Action == ActionType.WakingUp || this.PlayerManager.Action == ActionType.LesserWarp || !this.SpeechBubble.Hidden)
      return;
    Vector3 vector3_1 = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask();
    Vector3 vector3_2 = this.warpGateAo.Position * this.CameraManager.Viewpoint.ScreenSpaceMask();
    if ((double) this.warpGateAo.ArtObject.Size.Y == 8.0)
      vector3_2 -= new Vector3(0.0f, 1f, 0.0f);
    Vector3 a = vector3_2 - vector3_1;
    if ((double) Math.Abs(a.Dot(this.CameraManager.Viewpoint.SideMask())) > 1.5 || (double) Math.Abs(a.Y) > 2.0)
      return;
    foreach (WarpPanel warpPanel in this.panels.Values)
    {
      if (warpPanel.Enabled && warpPanel.Face == this.CameraManager.Viewpoint.VisibleOrientation())
      {
        this.PlayerManager.Action = ActionType.GateWarp;
        this.PlayerManager.WarpPanel = warpPanel;
        this.PlayerManager.OriginWarpViewpoint = this.panels.Values.First<WarpPanel>((Func<WarpPanel, bool>) (x => x.Destination == this.CurrentLevelName)).Face.AsViewpoint();
        break;
      }
    }
  }

  private void UpdateParallax(TimeSpan elapsed)
  {
    if (!this.CameraManager.Viewpoint.IsOrthographic())
      return;
    this.InterpolatedCenter = Vector3.Lerp(this.InterpolatedCenter, this.CameraManager.Center, MathHelper.Clamp((float) elapsed.TotalSeconds * this.CameraManager.InterpolationSpeed, 0.0f, 1f));
    Vector3 forward = this.CameraManager.View.Forward;
    forward.Z *= -1f;
    Vector3 right = this.CameraManager.View.Right;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    Vector3 vector3_2 = (this.InterpolatedCenter - this.warpGateAo.Position) / 2.5f;
    Vector3 a = (this.CameraManager.InterpolatedCenter - this.warpGateAo.Position) * vector3_1;
    foreach (WarpDestinations key in this.panels.Keys)
    {
      WarpPanel panel = this.panels[key];
      if (panel.Enabled && (double) panel.Face.AsVector().Dot(forward) > 0.0)
      {
        panel.Timer += elapsed;
        Vector3 vector3_3 = vector3_2 * vector3_1;
        switch (key)
        {
          case WarpDestinations.First:
            panel.Layers.Groups[3].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds * 0.25 % 16.0 - 8.0) * right;
            panel.Layers.Groups[4].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds * 0.5 % 16.0 - 8.0) * right;
            panel.Layers.Groups[5].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds % 16.0 - 8.0) * right;
            continue;
          case WarpDestinations.Mechanical:
            panel.Layers.Groups[3].Position = vector3_3 + new Vector3(0.0f, 2f, 0.0f) + (float) (panel.Timer.TotalSeconds % 16.0 - 8.0) * right;
            panel.Layers.Groups[4].Position = vector3_3 - new Vector3(0.0f, 2f, 0.0f) + (float) ((panel.Timer.TotalSeconds + 8.0) % 16.0 - 8.0) * right;
            continue;
          case WarpDestinations.Graveyard:
            float m31_1 = (float) (((double) a.X + (double) a.Z) / 16.0);
            float m32_1 = a.Y / 16f;
            panel.Layers.Groups[0].TextureMatrix.Set(new Matrix?(new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, m31_1, m32_1, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
            panel.Layers.Groups[4].TextureMatrix.Set(new Matrix?(new Matrix(2f, 0.0f, 0.0f, 0.0f, 0.0f, 2f, 0.0f, 0.0f, m31_1 / 2f, (float) ((double) m32_1 / 2.0 - panel.Timer.TotalSeconds * 5.0), 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
            panel.Layers.Groups[1].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds * 0.5 % 16.0 - 8.0) * right;
            panel.Layers.Groups[2].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds % 16.0 - 8.0) * right;
            panel.Layers.Groups[3].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds * 2.0 % 16.0 - 8.0) * right;
            continue;
          case WarpDestinations.Sewers:
            Matrix matrix = new Matrix(4f, 0.0f, 0.0f, 0.0f, 0.0f, 4f, 0.0f, 0.0f, (float) (((double) a.X + (double) a.Z) / 8.0), a.Y / 8f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f);
            panel.Layers.Groups[0].TextureMatrix.Set(new Matrix?(matrix));
            panel.Layers.Groups[1].TextureMatrix.Set(new Matrix?(matrix));
            panel.Layers.Groups[2].TextureMatrix.Set(new Matrix?(matrix));
            panel.Layers.Groups[3].Position = vector3_3 + new Vector3(0.0f, -8f, 0.0f);
            continue;
          case WarpDestinations.Zu:
            float m31_2 = (float) (-(double) a.Dot(right) / 16.0);
            float m32_2 = a.Y / 32f;
            panel.Layers.Groups[0].TextureMatrix.Set(new Matrix?(new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, m31_2, m32_2, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
            panel.Layers.Groups[1].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds * 0.25 % 16.0 - 8.0) * right;
            panel.Layers.Groups[2].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds * 0.5 % 16.0 - 8.0) * right;
            panel.Layers.Groups[3].Position = vector3_3 - Vector3.UnitY * 1.5f + (float) (panel.Timer.TotalSeconds % 16.0 - 8.0) * right;
            continue;
          default:
            continue;
        }
      }
    }
  }

  private void DrawLights()
  {
    if (!this.Visible || this.GameState.Loading || this.warpGateAo.ActorSettings.Inactive || Fez.LongScreenshot)
      return;
    foreach (WarpDestinations key in this.panels.Keys)
    {
      WarpPanel panel = this.panels[key];
      if (panel.Enabled)
      {
        (panel.PanelMask.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
        panel.PanelMask.Draw();
        (panel.PanelMask.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
      }
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.warpGateAo.ActorSettings.Inactive || this.GameState.StereoMode)
      return;
    this.DoDraw();
  }

  public void DoDraw()
  {
    foreach (WarpDestinations key in this.panels.Keys)
    {
      WarpPanel panel = this.panels[key];
      if (panel.Enabled)
      {
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
        this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.WarpGate));
        panel.PanelMask.Draw();
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
        this.GraphicsDevice.PrepareStencilRead(CompareFunction.Equal, FezEngine.Structure.StencilMask.WarpGate);
        panel.Layers.Draw();
        this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
        this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
        panel.PanelMask.Draw();
      }
    }
    this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
  }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
