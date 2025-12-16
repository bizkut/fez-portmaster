// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.FarawayPlaceHost
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects;
using FezEngine.Readers;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#nullable disable
namespace FezEngine.Components;

public class FarawayPlaceHost(Game game) : DrawableGameComponent(game)
{
  private const float Visibility = 0.125f;
  private Mesh ThisLevelMesh;
  private Mesh LastLevelMesh;
  private Mesh NextLevelMesh;
  private Mesh FarawayWaterMesh;
  private Mesh LastWaterMesh;
  private float OriginalFakeRadius;
  private float DestinationFakeRadius;
  private float FakeRadius;
  private bool IsFake;
  private FarawayPlaceHost.PlaceFader Fader;
  private Vector3 waterRightVector;
  private Texture2D HorizontalGradientTex;
  private static readonly object FarawayWaterMutex = new object();
  private static readonly object FarawayPlaceMutex = new object();
  private bool hasntSnapped;

  public override void Initialize()
  {
    base.Initialize();
    Material material = new Material();
    this.LastLevelMesh = new Mesh()
    {
      DepthWrites = false,
      Material = material,
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.PointClamp
    };
    this.ThisLevelMesh = new Mesh()
    {
      DepthWrites = false,
      Material = material,
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.PointClamp
    };
    this.NextLevelMesh = new Mesh()
    {
      DepthWrites = false,
      Material = material,
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.PointClamp
    };
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.LastLevelMesh.Effect = (BaseEffect) new FarawayEffect();
      this.ThisLevelMesh.Effect = (BaseEffect) new FarawayEffect();
      this.NextLevelMesh.Effect = (BaseEffect) new FarawayEffect();
    }));
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    ServiceHelper.AddComponent((IGameComponent) (this.Fader = new FarawayPlaceHost.PlaceFader(this.Game)));
    this.Fader.Visible = false;
  }

  private void TryInitialize()
  {
    this.ThisLevelMesh.ClearGroups();
    this.NextLevelMesh.ClearGroups();
    lock (FarawayPlaceHost.FarawayWaterMutex)
      this.FarawayWaterMesh = (Mesh) null;
    (this.NextLevelMesh.Effect as FarawayEffect).CleanUp();
    foreach (Script script1 in this.LevelManager.Scripts.Values.Where<Script>((Func<Script, bool>) (x => x.Actions.Any<ScriptAction>((Func<ScriptAction, bool>) (y => y.Operation == "ChangeToFarAwayLevel")))))
    {
      Volume volume;
      if (this.LevelManager.Volumes.TryGetValue(script1.Triggers.FirstOrDefault<ScriptTrigger>((Func<ScriptTrigger, bool>) (x => x.Object.Type == "Volume" && x.Event == "Enter")).Object.Identifier.Value, out volume))
      {
        FaceOrientation faceOrientation = volume.Orientations.FirstOrDefault<FaceOrientation>();
        ScriptAction scriptAction1 = script1.Actions.FirstOrDefault<ScriptAction>((Func<ScriptAction, bool>) (x => x.Operation == "ChangeToFarAwayLevel"));
        string destLevelName = scriptAction1.Arguments[0];
        int pixPerTrix = 0;
        FaceOrientation destFace;
        Vector2 destOffset;
        float waterLevel;
        float destWl;
        float destLevelHeight;
        bool destinationHasWater;
        string dlSong;
        using (MemoryContentManager memoryContentManager = new MemoryContentManager((IServiceProvider) this.Game.Services, this.Game.Content.RootDirectory))
        {
          string str = destLevelName;
          if (!MemoryContentManager.AssetExists("Levels\\" + destLevelName.Replace('/', '\\')))
            str = this.LevelManager.FullPath.Substring(0, this.LevelManager.FullPath.LastIndexOf("/") + 1) + destLevelName.Substring(destLevelName.LastIndexOf("/") + 1);
          LevelReader.MinimalRead = true;
          Level level;
          try
          {
            level = memoryContentManager.Load<Level>("Levels/" + str);
          }
          catch (Exception ex)
          {
            Logger.Log(nameof (FarawayPlaceHost), LogSeverity.Warning, "Couldn't load faraway place destination level : " + destLevelName);
            continue;
          }
          LevelReader.MinimalRead = false;
          dlSong = level.SongName;
          int key;
          try
          {
            key = int.Parse(scriptAction1.Arguments[1]);
          }
          catch (Exception ex)
          {
            key = -1;
          }
          Volume volume1 = key == -1 || !level.Volumes.ContainsKey(key) ? level.Volumes[level.Scripts.Values.First<Script>((Func<Script, bool>) (s => s.Actions.Any<ScriptAction>((Func<ScriptAction, bool>) (a => a.Object.Type == "Level" && a.Operation.Contains("Level") && a.Arguments[0] == this.LevelManager.Name)))).Triggers.First<ScriptTrigger>((Func<ScriptTrigger, bool>) (t => t.Object.Type == "Volume" && t.Event == "Enter")).Object.Identifier.Value] : level.Volumes[key];
          destFace = volume1.Orientations.FirstOrDefault<FaceOrientation>();
          Vector3 vector3 = (level.Size / 2f - (volume1.From + volume1.To) / 2f) * (destFace.AsViewpoint().RightVector() + Vector3.Up);
          destOffset = new Vector2(vector3.X + vector3.Z, vector3.Y);
          destinationHasWater = level.WaterType != 0;
          destWl = level.WaterHeight - (volume1.From + volume1.To).Y / 2f + this.EngineState.WaterLevelOffset;
          waterLevel = this.LevelManager.WaterHeight - (volume.From + volume.To).Y / 2f - destWl / 4f;
          destLevelHeight = level.Size.Y;
          Script script2 = level.Scripts.Values.FirstOrDefault<Script>((Func<Script, bool>) (s => s.Triggers.Any<ScriptTrigger>((Func<ScriptTrigger, bool>) (t => t.Event == "Start" && t.Object.Type == "Level")) && s.Actions.Any<ScriptAction>((Func<ScriptAction, bool>) (a => a.Object.Type == "Camera" && a.Operation == "SetPixelsPerTrixel"))));
          if (script2 != null)
          {
            ScriptAction scriptAction2 = script2.Actions.First<ScriptAction>((Func<ScriptAction, bool>) (a => a.Object.Type == "Camera" && a.Operation == "SetPixelsPerTrixel"));
            try
            {
              pixPerTrix = int.Parse(scriptAction2.Arguments[0]);
            }
            catch (Exception ex)
            {
            }
          }
          destWl = level.WaterHeight;
        }
        DrawActionScheduler.Schedule((Action) (() =>
        {
          string assetName = $"Other Textures/faraway_thumbs/{destLevelName} ({(object) destFace.AsViewpoint()})";
          Texture2D texture = this.CMProvider.CurrentLevel.Load<Texture2D>(assetName);
          texture.Name = assetName;
          if (this.ThisLevelMesh.Groups.Any<Group>((Func<Group, bool>) (x => x.Texture == texture)))
            return;
          if (pixPerTrix == 0)
            pixPerTrix = (int) this.CameraManager.PixelsPerTrixel;
          Group group1 = this.ThisLevelMesh.AddFace(new Vector3((float) texture.Width, (float) texture.Height, (float) texture.Width) / 16f / 2f, Vector3.Zero, faceOrientation, true);
          Group group2 = this.NextLevelMesh.AddFace(new Vector3((float) texture.Width, (float) texture.Height, (float) texture.Width) / 16f / 2f, Vector3.Zero, faceOrientation, true);
          FarawayPlaceData farawayPlaceData = new FarawayPlaceData()
          {
            OriginalCenter = (volume.From + volume.To) / 2f,
            Viewpoint = faceOrientation.AsViewpoint(),
            Volume = volume,
            DestinationOffset = destOffset.X * faceOrientation.AsViewpoint().RightVector() + Vector3.Up * destOffset.Y,
            WaterLevelOffset = new float?(waterLevel),
            DestinationLevelName = destLevelName,
            DestinationWaterLevel = destWl,
            DestinationLevelSize = destLevelHeight
          };
          if (this.LevelManager.WaterType == LiquidType.None & destinationHasWater)
          {
            if (this.HorizontalGradientTex == null || this.HorizontalGradientTex.IsDisposed)
              this.HorizontalGradientTex = this.CMProvider.Global.Load<Texture2D>("Other Textures/WaterHorizGradient");
            DefaultEffect.Textured textured = new DefaultEffect.Textured()
            {
              AlphaIsEmissive = false
            };
            lock (FarawayPlaceHost.FarawayWaterMutex)
            {
              ref FarawayPlaceData local = ref farawayPlaceData;
              Mesh mesh1 = new Mesh();
              mesh1.Effect = (BaseEffect) textured;
              Mesh mesh2 = mesh1;
              local.WaterBodyMesh = mesh1;
              this.FarawayWaterMesh = mesh2;
              this.FarawayWaterMesh.AddFace(Vector3.One, new Vector3(-0.5f, -1f, -0.5f) + faceOrientation.AsVector().Abs() * 0.5f, faceOrientation, false).Material = new Material();
              this.FarawayWaterMesh.AddFace(Vector3.One, new Vector3(-0.5f, -1f, -0.5f) + faceOrientation.AsVector().Abs() * 0.5f, faceOrientation, false).Material = new Material();
            }
          }
          group2.CustomData = group1.CustomData = (object) farawayPlaceData;
          group2.Position = group1.Position = farawayPlaceData.OriginalCenter;
          group2.Texture = group1.Texture = (Texture) texture;
          group2.Material = new Material()
          {
            Opacity = 0.125f
          };
          group1.Material = new Material()
          {
            Opacity = 0.125f
          };
          if (volume.ActorSettings == null)
            volume.ActorSettings = new VolumeActorSettings();
          volume.ActorSettings.DestinationSong = dlSong;
          switch (pixPerTrix)
          {
            case 1:
              volume.ActorSettings.DestinationRadius = 80f;
              break;
            case 2:
              volume.ActorSettings.DestinationRadius = 40f;
              break;
            case 3:
              volume.ActorSettings.DestinationRadius = 26.666666f;
              break;
            case 4:
              volume.ActorSettings.DestinationRadius = 20f;
              break;
            case 5:
              volume.ActorSettings.DestinationRadius = 16f;
              break;
          }
          volume.ActorSettings.DestinationPixelsPerTrixel = (float) pixPerTrix;
          volume.ActorSettings.DestinationOffset = destOffset;
        }));
      }
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Paused || this.EngineState.InMap || !this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.ProjectionTransition)
      return;
    if (this.EngineState.FarawaySettings.InTransition && (double) this.EngineState.FarawaySettings.OriginFadeOutStep == 1.0 && !this.IsFake)
    {
      for (int index = this.NextLevelMesh.Groups.Count - 1; index >= 0; --index)
      {
        try
        {
          if (index < this.NextLevelMesh.Groups.Count)
          {
            Group group = this.NextLevelMesh.Groups[index];
            FarawayPlaceData customData = (FarawayPlaceData) group.CustomData;
            string levelName = this.LevelManager.Name.Substring(0, this.LevelManager.Name.LastIndexOf("/") + 1) + customData.DestinationLevelName;
            if (this.CameraManager.Viewpoint != customData.Viewpoint)
            {
              this.NextLevelMesh.RemoveGroupAt(index);
              (this.NextLevelMesh.Effect as FarawayEffect).CleanUp();
            }
            else
              this.CMProvider.GetForLevel(levelName).Load<Texture2D>(group.Texture.Name);
          }
        }
        catch (Exception ex)
        {
        }
      }
      this.hasntSnapped = true;
      Mesh nextLevelMesh = this.NextLevelMesh;
      this.NextLevelMesh = this.LastLevelMesh;
      this.LastLevelMesh = nextLevelMesh;
      this.LastWaterMesh = this.FarawayWaterMesh;
      this.ThisLevelMesh.ClearGroups();
      this.OriginalFakeRadius = (float) this.GraphicsDevice.Viewport.Width / (this.CameraManager.PixelsPerTrixel * 16f);
      this.DestinationFakeRadius = this.EngineState.FarawaySettings.DestinationRadius / 4f;
      this.EngineState.FarawaySettings.InterpolatedFakeRadius = this.CameraManager.Radius;
      this.LastLevelMesh.Effect.ForcedViewMatrix = new Matrix?(this.CameraManager.View);
      (this.LastLevelMesh.Effect as FarawayEffect).ActualOpacity = 1f;
      if (this.LastWaterMesh != null && this.LastWaterMesh.Groups.Count > 0)
      {
        this.LastWaterMesh.Effect.ForcedViewMatrix = new Matrix?(this.CameraManager.View);
        try
        {
          this.LastWaterMesh.Groups[0].Material.Opacity = this.LastWaterMesh.Groups[1].Material.Opacity = 1f;
        }
        catch (Exception ex)
        {
        }
        lock (FarawayPlaceHost.FarawayWaterMutex)
          this.FarawayWaterMesh = (Mesh) null;
      }
      this.IsFake = true;
      this.Fader.PlacesMesh = this.LastLevelMesh;
      this.Fader.FarawayWaterMesh = this.LastWaterMesh;
      this.LastLevelMesh.AlwaysOnTop = true;
      if (this.LastWaterMesh != null)
        this.LastWaterMesh.AlwaysOnTop = true;
      this.EngineState.FarawaySettings.LoadingAllowed = true;
    }
    if (!this.EngineState.FarawaySettings.InTransition && this.IsFake)
    {
      this.IsFake = false;
      this.Fader.Visible = false;
      lock (FarawayPlaceHost.FarawayPlaceMutex)
        this.Fader.PlacesMesh = (Mesh) null;
      lock (FarawayPlaceHost.FarawayWaterMutex)
        this.Fader.FarawayWaterMesh = (Mesh) null;
    }
    if (this.EngineState.FarawaySettings.InTransition)
    {
      float transitionStep = this.EngineState.FarawaySettings.TransitionStep;
      this.Fader.Visible = true;
      float viewScale = this.GraphicsDevice.GetViewScale();
      if (this.IsFake)
      {
        this.FakeRadius = MathHelper.Lerp(this.OriginalFakeRadius, this.DestinationFakeRadius, transitionStep);
        this.EngineState.FarawaySettings.InterpolatedFakeRadius = MathHelper.Lerp(this.EngineState.FarawaySettings.InterpolatedFakeRadius, this.FakeRadius, MathHelper.Clamp((float) gameTime.ElapsedGameTime.TotalSeconds * this.CameraManager.InterpolationSpeed, 0.0f, 1f));
        this.LastLevelMesh.Effect.ForcedProjectionMatrix = new Matrix?(Matrix.CreateOrthographic(this.EngineState.FarawaySettings.InterpolatedFakeRadius / viewScale, this.EngineState.FarawaySettings.InterpolatedFakeRadius / this.CameraManager.AspectRatio / viewScale, this.CameraManager.NearPlane, this.CameraManager.FarPlane));
        if (this.LastWaterMesh != null)
          this.LastWaterMesh.Effect.ForcedProjectionMatrix = this.LastLevelMesh.Effect.ForcedProjectionMatrix;
        this.EngineState.SkipRendering = true;
        this.CameraManager.Radius = this.FakeRadius * 4f;
        (this.ThisLevelMesh.Effect as FarawayEffect).ActualOpacity = (float) (((double) transitionStep - 0.5) * 2.0);
        (this.LastLevelMesh.Effect as FarawayEffect).ActualOpacity = 1f - this.EngineState.FarawaySettings.DestinationCrossfadeStep;
        try
        {
          if (this.FarawayWaterMesh != null)
          {
            lock (FarawayPlaceHost.FarawayWaterMutex)
            {
              if ((double) transitionStep > 0.5)
              {
                this.FarawayWaterMesh.Groups[0].Material.Opacity = (float) (((double) transitionStep - 0.5) * 2.0);
                this.FarawayWaterMesh.Groups[1].Material.Opacity = (float) (((double) transitionStep - 0.5) * 2.0);
              }
              else
                this.FarawayWaterMesh.Groups[0].Material.Opacity = this.FarawayWaterMesh.Groups[1].Material.Opacity = 0.0f;
            }
          }
          else if (this.LastWaterMesh != null)
          {
            lock (FarawayPlaceHost.FarawayWaterMutex)
            {
              this.LastWaterMesh.Groups[0].Material.Opacity = 1f - this.EngineState.FarawaySettings.DestinationCrossfadeStep;
              this.LastWaterMesh.Groups[1].Material.Opacity = 1f - this.EngineState.FarawaySettings.DestinationCrossfadeStep;
              Material material1 = this.LastWaterMesh.Groups[0].Material;
              Color color = this.FogManager.Color;
              Vector3 vector3_1 = color.ToVector3();
              Vector3 waterBodyColor = this.EngineState.WaterBodyColor;
              color = this.LevelManager.ActualDiffuse;
              Vector3 vector3_2 = color.ToVector3();
              Vector3 vector3_3 = waterBodyColor * vector3_2;
              double amount1 = (double) Easing.EaseIn((double) transitionStep, EasingType.Sine) * 0.875 + 0.125;
              Vector3 vector3_4 = Vector3.Lerp(vector3_1, vector3_3, (float) amount1);
              material1.Diffuse = vector3_4;
              Material material2 = this.LastWaterMesh.Groups[1].Material;
              color = this.FogManager.Color;
              Vector3 vector3_5 = color.ToVector3();
              Vector3 waterFoamColor = this.EngineState.WaterFoamColor;
              color = this.LevelManager.ActualDiffuse;
              Vector3 vector3_6 = color.ToVector3();
              Vector3 vector3_7 = waterFoamColor * vector3_6;
              double amount2 = (double) Easing.EaseIn((double) transitionStep, EasingType.Sine) * 0.875 + 0.125;
              Vector3 vector3_8 = Vector3.Lerp(vector3_5, vector3_7, (float) amount2);
              material2.Diffuse = vector3_8;
            }
          }
        }
        catch (Exception ex)
        {
        }
        if ((double) this.EngineState.FarawaySettings.DestinationCrossfadeStep == 0.0 && !this.hasntSnapped)
        {
          this.hasntSnapped = false;
          this.CameraManager.SnapInterpolation();
        }
        this.EngineState.SkipRendering = false;
        foreach (Group group in this.LastLevelMesh.Groups)
          group.Material.Opacity = (float) ((double) Easing.EaseIn((double) transitionStep, EasingType.Sine) * 0.875 + 0.125);
      }
      else
      {
        foreach (Group group in this.ThisLevelMesh.Groups)
          group.Material.Opacity = (float) ((double) Easing.EaseIn((double) transitionStep, EasingType.Sine) * 0.875 + 0.125);
      }
    }
    if (this.EngineState.Loading)
      return;
    this.LastLevelMesh.Material.Diffuse = this.FogManager.Color.ToVector3();
  }

  private static double GetCustomOffset(double pixelsPerTrixel)
  {
    return 0.0 + 12.0 * Math.Pow(pixelsPerTrixel, -1.0) - 2.0;
  }

  private void PositionFarawayPlaces()
  {
    if (!this.CameraManager.Viewpoint.IsOrthographic() || this.CameraManager.ProjectionTransition)
      return;
    float num1 = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
    for (int index = 0; index < this.ThisLevelMesh.Groups.Count; ++index)
    {
      Group group1 = this.ThisLevelMesh.Groups[index];
      Group group2 = this.NextLevelMesh.Groups[index];
      FarawayPlaceData customData = (FarawayPlaceData) this.ThisLevelMesh.Groups[index].CustomData;
      Vector2 vector2 = customData.Volume.ActorSettings == null ? Vector2.Zero : customData.Volume.ActorSettings.FarawayPlaneOffset;
      int num2 = customData.Volume.ActorSettings == null ? 0 : (customData.Volume.ActorSettings.WaterLocked ? 1 : 0);
      float pixelsPerTrixel = this.CameraManager.PixelsPerTrixel;
      if (this.EngineState.FarawaySettings.InTransition && FezMath.AlmostEqual(this.EngineState.FarawaySettings.DestinationCrossfadeStep, 1f))
        pixelsPerTrixel = MathHelper.Lerp(this.CameraManager.PixelsPerTrixel, this.EngineState.FarawaySettings.DestinationPixelsPerTrixel, (float) (((double) this.EngineState.FarawaySettings.TransitionStep - 0.875) / 0.125));
      float num3 = (float) ((double) (-4 * (this.LevelManager.Descending ? -1 : 1)) / (double) pixelsPerTrixel - 15.0 / 32.0 + 1.0);
      Vector3 vector3_1 = this.CameraManager.InterpolatedCenter - customData.OriginalCenter + num3 * Vector3.UnitY;
      float num4 = (float) (FarawayPlaceHost.GetCustomOffset((double) pixelsPerTrixel) * (this.LevelManager.Descending ? -1.0 : 1.0) + 15.0 / 32.0);
      float num5 = 0.0f;
      if (num2 != 0 && customData.WaterLevelOffset.HasValue)
      {
        vector3_1 *= FezMath.XZMask;
        vector2 *= Vector2.UnitX;
        num5 = (float) ((double) customData.WaterLevelOffset.Value - (double) num4 / 4.0 - 0.5 + 0.125);
        customData.Volume.ActorSettings.WaterOffset = num5;
      }
      group1.Position = group2.Position = (customData.OriginalCenter + (customData.DestinationOffset + num4 * Vector3.UnitY) / 4f) * customData.Viewpoint.ScreenSpaceMask() + customData.Viewpoint.DepthMask() * this.CameraManager.InterpolatedCenter + customData.Viewpoint.ForwardVector() * 30f * num1 + customData.Viewpoint.RightVector() * vector2.X + Vector3.Up * vector2.Y + vector3_1 * customData.Viewpoint.ScreenSpaceMask() / 2f + num5 * Vector3.UnitY;
      if (customData.WaterBodyMesh != null && customData.WaterBodyMesh.Groups.Count > 0)
      {
        this.waterRightVector = customData.Viewpoint.RightVector();
        customData.WaterBodyMesh.Position = group1.Position * (customData.Viewpoint.DepthMask() + Vector3.UnitY) + this.CameraManager.InterpolatedCenter * customData.Viewpoint.SideMask() + ((float) ((double) customData.DestinationWaterLevel - (double) customData.DestinationLevelSize / 2.0 - 0.5) + this.EngineState.WaterLevelOffset) * Vector3.UnitY / 4f;
        customData.WaterBodyMesh.Groups[0].Scale = new Vector3(this.CameraManager.Radius);
        Material material1 = customData.WaterBodyMesh.Groups[0].Material;
        Vector3 waterBodyColor = this.EngineState.WaterBodyColor;
        Color color = this.LevelManager.ActualDiffuse;
        Vector3 vector3_2 = color.ToVector3();
        Vector3 vector3_3 = waterBodyColor * vector3_2;
        color = this.FogManager.Color;
        Vector3 vector3_4 = color.ToVector3();
        Vector3 vector3_5 = Vector3.Lerp(vector3_3, vector3_4, 0.875f);
        material1.Diffuse = vector3_5;
        customData.WaterBodyMesh.Groups[1].Scale = new Vector3(this.CameraManager.Radius, 1f / 16f, this.CameraManager.Radius);
        Material material2 = customData.WaterBodyMesh.Groups[1].Material;
        Vector3 waterFoamColor = this.EngineState.WaterFoamColor;
        color = this.LevelManager.ActualDiffuse;
        Vector3 vector3_6 = color.ToVector3();
        Vector3 vector3_7 = waterFoamColor * vector3_6;
        color = this.FogManager.Color;
        Vector3 vector3_8 = color.ToVector3();
        Vector3 vector3_9 = Vector3.Lerp(vector3_7, vector3_8, 0.875f);
        material2.Diffuse = vector3_9;
      }
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.EngineState.Loading || this.EngineState.InMap)
      return;
    this.PositionFarawayPlaces();
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.SkyLayer3));
    this.ThisLevelMesh.Draw();
    lock (FarawayPlaceHost.FarawayWaterMutex)
    {
      if (this.FarawayWaterMesh != null)
      {
        this.FarawayWaterMesh.Draw();
        Vector3 position = this.FarawayWaterMesh.Position;
        Vector3 scale = this.FarawayWaterMesh.Scale;
        this.FarawayWaterMesh.Blending = new BlendingMode?(BlendingMode.Alphablending);
        this.FarawayWaterMesh.SamplerState = SamplerState.LinearClamp;
        this.FarawayWaterMesh.Texture = (Dirtyable<Texture>) (Texture) this.HorizontalGradientTex;
        this.FarawayWaterMesh.Position -= Math.Abs(this.FarawayWaterMesh.Groups[0].Scale.X) * this.waterRightVector;
        this.FarawayWaterMesh.Draw();
        this.FarawayWaterMesh.Scale = this.waterRightVector.Abs() * -1f + Vector3.One - this.waterRightVector.Abs();
        this.FarawayWaterMesh.Culling = CullMode.CullClockwiseFace;
        this.FarawayWaterMesh.Position += Math.Abs(this.FarawayWaterMesh.Groups[0].Scale.X) * this.waterRightVector * 2f;
        this.FarawayWaterMesh.Draw();
        this.FarawayWaterMesh.Culling = CullMode.CullCounterClockwiseFace;
        this.FarawayWaterMesh.Position = position;
        this.FarawayWaterMesh.Scale = scale;
        this.FarawayWaterMesh.Texture = (Dirtyable<Texture>) null;
      }
    }
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
  }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IFogManager FogManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  private class PlaceFader : DrawableGameComponent
  {
    private CombineEffect CombineEffect;
    private RenderTargetHandle rth;

    public Mesh PlacesMesh { private get; set; }

    public Mesh FarawayWaterMesh { private get; set; }

    public PlaceFader(Game game)
      : base(game)
    {
      this.DrawOrder = 1001;
    }

    protected override void LoadContent()
    {
      DrawActionScheduler.Schedule((Action) (() => this.CombineEffect = new CombineEffect()
      {
        RedGamma = 1f
      }));
    }

    public override void Update(GameTime gameTime)
    {
      if (this.GameState.StereoMode != (this.DrawOrder == 1002))
      {
        this.DrawOrder = this.GameState.StereoMode ? 1002 : 1001;
        this.OnDrawOrderChanged((object) this, EventArgs.Empty);
      }
      if (this.Visible)
      {
        if (this.GameState.StereoMode && this.rth == null)
        {
          this.rth = this.TargetRenderer.TakeTarget();
          this.TargetRenderer.ScheduleHook(this.DrawOrder, this.rth.Target);
        }
        else if (this.rth != null && !this.GameState.StereoMode)
        {
          this.TargetRenderer.ReturnTarget(this.rth);
          this.rth = (RenderTargetHandle) null;
        }
      }
      if (this.Visible || this.rth == null)
        return;
      this.TargetRenderer.ReturnTarget(this.rth);
      this.rth = (RenderTargetHandle) null;
    }

    protected override void Dispose(bool disposing)
    {
      base.Dispose(disposing);
      if (this.rth == null)
        return;
      this.TargetRenderer.ReturnTarget(this.rth);
      this.rth = (RenderTargetHandle) null;
    }

    public override void Draw(GameTime gameTime)
    {
      GraphicsDevice graphicsDevice = this.GraphicsDevice;
      graphicsDevice.PrepareStencilRead(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.Water);
      lock (FarawayPlaceHost.FarawayPlaceMutex)
      {
        if (this.PlacesMesh != null)
          this.PlacesMesh.Draw();
      }
      graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
      lock (FarawayPlaceHost.FarawayWaterMutex)
      {
        if (this.FarawayWaterMesh != null)
          this.FarawayWaterMesh.Draw();
      }
      if (!this.GameState.StereoMode || this.rth == null)
        return;
      this.TargetRenderer.Resolve(this.rth.Target, true);
      this.GraphicsDevice.Clear(Color.Black);
      this.GraphicsDevice.SetupViewport();
      this.CombineEffect.RightTexture = this.CombineEffect.LeftTexture = (Texture2D) this.rth.Target;
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Opaque);
      this.TargetRenderer.DrawFullscreen((BaseEffect) this.CombineEffect);
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    }

    [ServiceDependency]
    public ITargetRenderingManager TargetRenderer { get; set; }

    [ServiceDependency]
    public IEngineStateManager GameState { get; set; }
  }
}
