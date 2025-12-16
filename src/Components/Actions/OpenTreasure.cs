// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.OpenTreasure
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
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
namespace FezGame.Components.Actions;

internal class OpenTreasure : PlayerAction
{
  private static readonly TimeSpan OpeningDuration = TimeSpan.FromSeconds(4.0);
  private const float StarPulsateSpeed = 5f;
  private const float StarRotateSpeed = 1f;
  private const float ItemRotateSpeed = 2f;
  private readonly List<Mesh> FloatingMaps = new List<Mesh>();
  private readonly List<ArtObjectInstance> LevelChests = new List<ArtObjectInstance>();
  private float SinceCreated;
  private TimeSpan sinceActive;
  private ArtObjectInstance chestAO;
  private Trile treasureTrile;
  private TrileInstance treasureInstance;
  private ArtObject treasureAo;
  private ArtObjectInstance treasureAoInstance;
  private Vector3 treasureOrigin;
  private Vector3 aoOrigin;
  private Quaternion aoInitialRotation;
  private bool restored;
  private bool reculled;
  private TimeSpan sinceCollect;
  private float lastZoom;
  private bool treasureIsAo;
  private bool treasureIsMap;
  private bool treasureIsMail;
  private ActorType treasureActorType;
  private bool WasConstrained;
  private Vector2? OldPan;
  private Vector3 OldCenter;
  private float OldPixPerTrix;
  private TrileInstance oldGround;
  private float oldGroundHeight;
  private float oldDepth;
  private SoundEffect treasureGetSound;
  private SoundEffect assembleSound;
  private readonly Mesh lightBox;
  private readonly Mesh fadedStar;
  private readonly Mesh solidStar;
  private readonly Mesh flare;
  private readonly Mesh map;
  private readonly Mesh mail;
  private bool hasHooked;
  private float lastSin;
  private Group[] mystery2Groups = new Group[3];
  private Texture2D mystery2Xbox;
  private Texture2D mystery2Sony;

  public OpenTreasure(Game game)
    : base(game)
  {
    this.DrawOrder = 50;
    this.lightBox = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.LinearClamp,
      DepthWrites = false,
      Scale = new Vector3(1.6f, 1.5f, 1.2f),
      TextureMatrix = (Dirtyable<Matrix>) new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, -1f, 0.0f, 0.0f, 0.0f, 1f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)
    };
    this.lightBox.AddFace(Vector3.One, -Vector3.UnitZ / 2f + Vector3.UnitY / 2f, FaceOrientation.Back, true);
    this.lightBox.AddFace(Vector3.One, Vector3.UnitZ / 2f + Vector3.UnitY / 2f, FaceOrientation.Front, true);
    this.lightBox.AddFace(Vector3.One, Vector3.Left / 2f + Vector3.UnitY / 2f, FaceOrientation.Left, true);
    this.lightBox.AddFace(Vector3.One, Vector3.Right / 2f + Vector3.UnitY / 2f, FaceOrientation.Right, true);
    this.flare = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Additive),
      SamplerState = SamplerState.LinearClamp,
      DepthWrites = false
    };
    this.flare.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Front, true, true);
    this.fadedStar = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      DepthWrites = false
    };
    this.solidStar = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      DepthWrites = false
    };
    Color color = new Color(1f, 1f, 0.3f, 0.0f);
    for (int index = 0; index < 8; ++index)
    {
      float num1 = (float) ((double) index * 6.2831854820251465 / 8.0);
      float num2 = (float) (((double) index + 0.5) * 6.2831854820251465 / 8.0);
      this.fadedStar.AddColoredTriangle(Vector3.Zero, new Vector3((float) Math.Sin((double) num1), (float) Math.Cos((double) num1), 0.0f), new Vector3((float) Math.Sin((double) num2), (float) Math.Cos((double) num2), 0.0f), new Color(1f, 1f, 1f, 0.7f), color, color);
      this.solidStar.AddColoredTriangle(Vector3.Zero, new Vector3((float) Math.Sin((double) num1), (float) Math.Cos((double) num1), 0.0f), new Vector3((float) Math.Sin((double) num2), (float) Math.Cos((double) num2), 0.0f), new Color(1f, 1f, 1f, 0.7f), new Color(1f, 1f, 1f, 0.7f), new Color(1f, 1f, 1f, 0.7f));
    }
    this.map = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.PointClamp
    };
    OpenTreasure.BuildMap(this.map);
    this.mail = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending),
      SamplerState = SamplerState.PointClamp,
      Scale = new Vector3(1.5f)
    };
    Group group1 = this.mail.AddFace(Vector3.One, Vector3.Zero, FaceOrientation.Back, true);
    group1.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, -1.57079637f);
    Group group2 = this.mail.CloneGroup(group1);
    group2.CullMode = new CullMode?(CullMode.CullClockwiseFace);
    group2.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(-1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 1f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.map.Effect = (BaseEffect) new DefaultEffect.LitTextured();
      Mesh mail = this.mail;
      mail.Effect = (BaseEffect) new DefaultEffect.LitTextured()
      {
        Emissive = 0.5f
      };
    }));
  }

  private static void BuildMap(Mesh mesh)
  {
    Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.Up, -1.57079637f);
    Vector3 zero = Vector3.Zero;
    Group group1 = mesh.AddGroup();
    group1.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionNormalTexture>(new FezVertexPositionNormalTexture[4]
    {
      new FezVertexPositionNormalTexture(new Vector3(-1f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(1f, 0.0f)),
      new FezVertexPositionNormalTexture(new Vector3(-1f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(1f, 1f)),
      new FezVertexPositionNormalTexture(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.625f, 0.0f)),
      new FezVertexPositionNormalTexture(new Vector3(0.0f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.625f, 1f))
    }, new int[6]{ 0, 1, 2, 2, 1, 3 }, PrimitiveType.TriangleList);
    group1.Scale = new Vector3(0.375f, 1f, 1f) * 1.5f;
    group1.Position = zero + MenuCubeFace.Maps.GetRight() * 0.125f * 1.5f;
    group1.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 0.3926991f) * fromAxisAngle;
    Group group2 = mesh.CloneGroup(group1);
    group2.CullMode = new CullMode?(CullMode.CullClockwiseFace);
    group2.InvertNormals<FezVertexPositionNormalTexture>();
    group2.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(-1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 1f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
    Group group3 = mesh.AddGroup();
    group3.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionNormalTexture>(new FezVertexPositionNormalTexture[4]
    {
      new FezVertexPositionNormalTexture(new Vector3(-0.5f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.625f, 0.0f)),
      new FezVertexPositionNormalTexture(new Vector3(-0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.625f, 1f)),
      new FezVertexPositionNormalTexture(new Vector3(0.5f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.375f, 0.0f)),
      new FezVertexPositionNormalTexture(new Vector3(0.5f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.375f, 1f))
    }, new int[6]{ 0, 1, 2, 2, 1, 3 }, PrimitiveType.TriangleList);
    group3.Scale = new Vector3(0.25f, 1f, 1f) * 1.5f;
    group3.Position = zero;
    group3.Rotation = fromAxisAngle;
    Group group4 = mesh.CloneGroup(group3);
    group4.CullMode = new CullMode?(CullMode.CullClockwiseFace);
    group4.InvertNormals<FezVertexPositionNormalTexture>();
    group4.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(-1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 1f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
    Group group5 = mesh.AddGroup();
    group5.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<FezVertexPositionNormalTexture>(new FezVertexPositionNormalTexture[4]
    {
      new FezVertexPositionNormalTexture(new Vector3(0.0f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.375f, 0.0f)),
      new FezVertexPositionNormalTexture(new Vector3(0.0f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.375f, 1f)),
      new FezVertexPositionNormalTexture(new Vector3(1f, 0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.0f, 0.0f)),
      new FezVertexPositionNormalTexture(new Vector3(1f, -0.5f, 0.0f), new Vector3(0.0f, 0.0f, -1f), new Vector2(0.0f, 1f))
    }, new int[6]{ 0, 1, 2, 2, 1, 3 }, PrimitiveType.TriangleList);
    group5.Scale = new Vector3(0.375f, 1f, 1f) * 1.5f;
    group5.Position = zero - MenuCubeFace.Maps.GetRight() * 0.125f * 1.5f;
    group5.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 0.3926991f) * fromAxisAngle;
    Group group6 = mesh.CloneGroup(group5);
    group6.CullMode = new CullMode?(CullMode.CullClockwiseFace);
    group6.InvertNormals<FezVertexPositionNormalTexture>();
    group6.TextureMatrix = (Dirtyable<Matrix?>) new Matrix?(new Matrix(-1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 1f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
  }

  public override void Initialize()
  {
    this.SoundManager.SongChanged += new Action(this.RefreshSounds);
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      foreach (int inactiveArtObject in this.GameState.SaveData.ThisLevel.InactiveArtObjects)
      {
        ArtObjectInstance artObjectInstance;
        if (inactiveArtObject >= 0 && this.LevelManager.ArtObjects.TryGetValue(inactiveArtObject, out artObjectInstance) && artObjectInstance.ArtObject.ActorType == ActorType.TreasureChest)
        {
          Vector3 vector3 = -FezMath.AlmostClamp(Vector3.Transform(Vector3.UnitZ, artObjectInstance.Rotation));
          artObjectInstance.Position += 1.375f * vector3 - 0.25f * Vector3.UnitY;
          artObjectInstance.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, -2.3561945f);
          artObjectInstance.ActorSettings.Inactive = true;
        }
      }
      this.LevelChests.Clear();
      this.LevelChests.AddRange(this.LevelManager.ArtObjects.Values.Where<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.TreasureChest)));
      this.FloatingMaps.Clear();
      foreach (ArtObjectInstance artObjectInstance in this.LevelManager.ArtObjects.Values.Where<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.TreasureMap)).ToArray<ArtObjectInstance>())
      {
        if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(artObjectInstance.Id))
        {
          this.LevelManager.ArtObjects.Remove(artObjectInstance.Id);
          artObjectInstance.Dispose();
        }
        else
        {
          Mesh m = new Mesh()
          {
            Blending = new BlendingMode?(BlendingMode.Alphablending),
            SamplerState = SamplerState.PointClamp
          };
          this.FloatingMaps.Add(m);
          OpenTreasure.BuildMap(m);
          artObjectInstance.Position += Vector3.UnitY * 0.5f;
          m.Position = artObjectInstance.Position;
          string mapName = artObjectInstance.ActorSettings.TreasureMapName;
          DrawActionScheduler.Schedule((Action) (() =>
          {
            m.Effect = (BaseEffect) new DefaultEffect.LitTextured();
            Texture2D texture2D3 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{mapName}_1");
            Texture2D texture2D4 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{mapName}_2");
            for (int index = 0; index < m.Groups.Count; ++index)
              m.Groups[index].Texture = index % 2 == 0 ? (Texture) texture2D3 : (Texture) texture2D4;
          }));
          m.CustomData = (object) new OpenTreasure.TreasureActorSettings()
          {
            ArtObject = artObjectInstance.ArtObject,
            AoInstance = artObjectInstance
          };
          this.LevelManager.ArtObjects.Remove(artObjectInstance.Id);
          artObjectInstance.Dispose();
        }
      }
      this.RefreshSounds();
    });
    base.Initialize();
  }

  private void RefreshSounds()
  {
    TrackedSong currentlyPlayingSong = this.SoundManager.CurrentlyPlayingSong;
    this.assembleSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Collects/SplitUpCube/Assemble_" + (object) (AssembleChords) (currentlyPlayingSong == null ? 0 : (int) currentlyPlayingSong.AssembleChord));
  }

  protected override void LoadContent()
  {
    this.treasureGetSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Collects/OpenTreasure");
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.lightBox.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.flare.Effect = (BaseEffect) new DefaultEffect.Textured();
      this.fadedStar.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.solidStar.Effect = (BaseEffect) new DefaultEffect.VertexColored();
      this.lightBox.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Background Planes/gradient_down");
      this.flare.Texture = (Dirtyable<Texture>) (Texture) this.CMProvider.Global.Load<Texture2D>("Background Planes/flare");
    }));
  }

  protected override void TestConditions()
  {
    if (this.PlayerManager.Action == ActionType.OpeningTreasure || this.PlayerManager.Action == ActionType.FindingTreasure || this.PlayerManager.Action == ActionType.ReadingSign || this.PlayerManager.Action == ActionType.FreeFalling || this.PlayerManager.Action == ActionType.Dying || this.PlayerManager.Action == ActionType.LesserWarp || this.PlayerManager.Action == ActionType.GateWarp || this.GameState.InFpsMode)
      return;
    TrileInstance surface = this.PlayerManager.AxisCollision[VerticalDirection.Up].Surface;
    if (surface != null && !surface.Hidden && surface.Trile.ActorSettings.Type.IsTreasure())
    {
      this.treasureIsMap = this.treasureIsAo = false;
      this.treasureIsAo = false;
      this.chestAO = (ArtObjectInstance) null;
      this.treasureInstance = surface;
      this.treasureTrile = surface.Trile;
      this.treasureActorType = this.treasureTrile.ActorSettings.Type;
      this.PlayerManager.Action = ActionType.FindingTreasure;
      this.sinceCollect = TimeSpan.Zero;
      this.oldDepth = this.PlayerManager.Position.Dot(this.CameraManager.Viewpoint.DepthMask());
      this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + this.treasureInstance.Position * this.CameraManager.Viewpoint.DepthMask() + 2f * -this.CameraManager.Viewpoint.ForwardVector();
    }
    else
    {
      foreach (Mesh floatingMap in this.FloatingMaps)
      {
        Vector3 vector3_1 = new Vector3(0.75f);
        Vector3 a = (floatingMap.Position - this.PlayerManager.Center).Abs();
        if ((double) a.Dot(this.CameraManager.Viewpoint.SideMask()) < (double) vector3_1.X && (double) a.Y < (double) vector3_1.Y)
        {
          Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
          NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.PlayerManager.Position);
          if (nearestTriles.Deep != null)
          {
            Vector3 vector3_2 = nearestTriles.Deep.Center - nearestTriles.Deep.TransformedSize * b / 2f;
            if ((double) (floatingMap.Position - b - vector3_2).Dot(b) > 0.0)
              continue;
          }
          this.treasureIsMap = true;
          this.treasureIsAo = false;
          this.chestAO = (ArtObjectInstance) null;
          this.treasureAo = ((OpenTreasure.TreasureActorSettings) floatingMap.CustomData).ArtObject;
          this.treasureAoInstance = ((OpenTreasure.TreasureActorSettings) floatingMap.CustomData).AoInstance;
          this.treasureActorType = this.treasureAo.ActorType;
          this.PlayerManager.Action = ActionType.FindingTreasure;
          this.sinceCollect = TimeSpan.Zero;
          this.oldDepth = this.PlayerManager.Position.Dot(this.CameraManager.Viewpoint.DepthMask());
          this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + floatingMap.Position * this.CameraManager.Viewpoint.DepthMask() + 2f * -this.CameraManager.Viewpoint.ForwardVector();
          return;
        }
      }
      if (!this.PlayerManager.Grounded || this.PlayerManager.Background)
        return;
      this.chestAO = (ArtObjectInstance) null;
      foreach (ArtObjectInstance levelChest in this.LevelChests)
      {
        if (levelChest.Visible && !levelChest.ActorSettings.Inactive)
        {
          Vector3 vector3_3 = levelChest.ArtObject.Size / 2f;
          Vector3 a = (levelChest.Position - this.PlayerManager.Center).Abs();
          if ((double) a.Dot(this.CameraManager.Viewpoint.SideMask()) < (double) vector3_3.X && (double) a.Y < (double) vector3_3.Y)
          {
            Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
            NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.PlayerManager.Position);
            if (nearestTriles.Deep != null)
            {
              Vector3 vector3_4 = nearestTriles.Deep.Center - nearestTriles.Deep.TransformedSize * b / 2f;
              if ((double) (levelChest.Position - b - vector3_4).Dot(b) > 0.0)
                continue;
            }
            if (FezMath.OrientationFromDirection(FezMath.AlmostClamp(Vector3.Transform(Vector3.UnitZ, levelChest.Rotation))).AsViewpoint() == this.CameraManager.Viewpoint && levelChest.ActorSettings.ContainedTrile != ActorType.None)
            {
              this.chestAO = levelChest;
              this.sinceCollect = TimeSpan.Zero;
              break;
            }
          }
        }
      }
      if (this.chestAO == null || this.InputManager.GrabThrow != FezButtonState.Pressed)
        return;
      this.GomezService.OnOpenTreasure();
      Volume volume;
      if ((volume = this.PlayerManager.CurrentVolumes.FirstOrDefault<Volume>((Func<Volume, bool>) (x => x.ActorSettings != null && x.ActorSettings.IsPointOfInterest && (double) Vector3.DistanceSquared(x.BoundingBox.GetCenter(), this.chestAO.Position) < 2.0))) != null)
      {
        volume.Enabled = false;
        this.GameState.SaveData.ThisLevel.InactiveVolumes.Add(volume.Id);
      }
      this.PlayerManager.Action = ActionType.OpeningTreasure;
    }
  }

  protected override void Begin()
  {
    bool flag = this.PlayerManager.Action == ActionType.OpeningTreasure;
    this.sinceActive = flag ? TimeSpan.FromSeconds(-1.0) : TimeSpan.Zero;
    if (flag)
    {
      this.oldDepth = this.PlayerManager.Position.Dot(this.CameraManager.Viewpoint.DepthMask());
      this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + this.chestAO.Position * this.CameraManager.Viewpoint.DepthMask() + this.CameraManager.Viewpoint.ForwardVector() * -1.5f;
      this.PlayerManager.LookingDirection = HorizontalDirection.Right;
      this.aoOrigin = this.chestAO.Position;
      this.chestAO.ActorSettings.Inactive = true;
    }
    else
    {
      if (this.PlayerManager.ForcedTreasure != null)
      {
        this.treasureIsMap = false;
        this.treasureInstance = this.PlayerManager.ForcedTreasure;
        this.treasureIsAo = false;
        this.treasureIsMail = false;
        this.chestAO = (ArtObjectInstance) null;
        this.treasureTrile = this.treasureInstance.Trile;
        this.treasureActorType = this.treasureTrile.ActorSettings.Type;
        this.PlayerManager.Action = ActionType.FindingTreasure;
        this.sinceCollect = TimeSpan.Zero;
        this.oldDepth = this.PlayerManager.Position.Dot(this.CameraManager.Viewpoint.DepthMask());
        this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + this.treasureInstance.Position * this.CameraManager.Viewpoint.DepthMask() + 2f * -this.CameraManager.Viewpoint.ForwardVector();
      }
      this.aoOrigin = this.treasureIsMap ? this.treasureAoInstance.Position : this.treasureInstance.Position;
      this.sinceActive = TimeSpan.FromSeconds(OpenTreasure.OpeningDuration.TotalSeconds * 0.60000002384185791);
      this.assembleSound.Emit();
    }
    if (!flag)
      this.PlayerManager.Velocity = new Vector3(0.0f, 0.05f, 0.0f);
    this.WasConstrained = this.CameraManager.Constrained;
    if (this.WasConstrained)
    {
      this.OldCenter = this.CameraManager.Center;
      this.OldPixPerTrix = this.CameraManager.PixelsPerTrixel;
    }
    this.OldPan = this.CameraManager.PanningConstraints;
    this.CameraManager.Constrained = true;
    this.CameraManager.PanningConstraints = new Vector2?();
    this.CameraManager.Center = this.aoOrigin;
    if (!flag)
    {
      this.lastZoom = this.CameraManager.PixelsPerTrixel;
      this.CameraManager.PixelsPerTrixel = 4f;
    }
    if (flag)
      this.aoInitialRotation = this.lightBox.Rotation = this.solidStar.Rotation = this.fadedStar.Rotation = this.flare.Rotation = this.chestAO.Rotation;
    else
      this.solidStar.Rotation = this.fadedStar.Rotation = this.flare.Rotation = Quaternion.Inverse(this.CameraManager.Rotation);
    this.lightBox.Position = this.aoOrigin - Vector3.UnitY / 2f;
    this.reculled = this.restored = false;
    if (flag)
    {
      this.treasureAo = (ArtObject) null;
      this.treasureTrile = (Trile) null;
      this.treasureIsMap = this.treasureIsAo = false;
      this.treasureActorType = this.chestAO.ActorSettings.ContainedTrile;
      if (this.chestAO.ActorSettings.ContainedTrile == ActorType.TreasureMap)
        this.treasureIsMap = true;
      else if (this.chestAO.ActorSettings.ContainedTrile == ActorType.Mail)
        this.treasureIsMail = true;
      else if (this.chestAO.ActorSettings.ContainedTrile.SupportsArtObjects())
      {
        this.treasureAo = this.CMProvider.Global.Load<ArtObject>("Art Objects/" + this.chestAO.ActorSettings.ContainedTrile.GetArtObjectName());
        this.treasureIsAo = true;
      }
      else
        this.treasureTrile = this.LevelManager.ActorTriles(this.chestAO.ActorSettings.ContainedTrile).LastOrDefault<Trile>();
      Waiters.Wait(1.0, (Action) (() => this.treasureGetSound.Emit()));
    }
    if (!flag && this.treasureIsMap)
    {
      this.treasureOrigin = this.aoOrigin - new Vector3(0.0f, 0.125f, 0.0f);
      string treasureMapName = this.treasureAoInstance.ActorSettings.TreasureMapName;
      Texture2D texture2D1 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{treasureMapName}_1");
      Texture2D texture2D2 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{treasureMapName}_2");
      for (int index = 0; index < this.map.Groups.Count; ++index)
        this.map.Groups[index].Texture = index % 2 == 0 ? (Texture) texture2D1 : (Texture) texture2D2;
      if (treasureMapName == "MAP_MYSTERY")
      {
        this.mystery2Groups[0] = this.map.Groups[1];
        this.mystery2Groups[1] = this.map.Groups[3];
        this.mystery2Groups[2] = this.map.Groups[5];
        this.mystery2Xbox = texture2D2;
        this.mystery2Sony = this.CMProvider.Global.Load<Texture2D>("Other Textures/maps/MAP_MYSTERY_2_SONY");
        GamepadState.OnLayoutChanged += new EventHandler(this.UpdateControllerTexture);
        this.UpdateControllerTexture((object) null, (EventArgs) null);
      }
      Mesh mesh = this.FloatingMaps.First<Mesh>((Func<Mesh, bool>) (x => ((OpenTreasure.TreasureActorSettings) x.CustomData).AoInstance == this.treasureAoInstance));
      this.map.Position = mesh.Position - this.CameraManager.Viewpoint.ForwardVector() * 0.5f;
      this.map.Rotation = mesh.Rotation;
      this.FloatingMaps.Remove(mesh);
    }
    this.oldGround = (TrileInstance) null;
    if (this.PlayerManager.Grounded)
    {
      MultipleHits<TrileInstance> ground = this.PlayerManager.Ground;
      this.oldGround = ground.First;
      ground = this.PlayerManager.Ground;
      this.oldGroundHeight = ground.First.Center.Y;
    }
    this.SoundManager.FadeVolume(1f, 0.125f, 2f);
  }

  protected override void End()
  {
    if (this.oldGround != null)
      this.PlayerManager.Position += (this.oldGround.Center.Y - this.oldGroundHeight) * Vector3.UnitY;
    this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + this.oldDepth * this.CameraManager.Viewpoint.DepthMask();
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.hasHooked)
    {
      this.LightingPostProcess.DrawOnTopLights += new Action(this.DoDraw);
      this.hasHooked = true;
    }
    if (!this.GameState.Paused && !this.GameState.InMenuCube && !this.GameState.InMap && !this.GameState.InFpsMode && this.CameraManager.Viewpoint.IsOrthographic() && this.CameraManager.ActionRunning && !this.GameState.Loading && this.FloatingMaps.Count > 0)
    {
      this.SinceCreated += (float) gameTime.ElapsedGameTime.TotalSeconds;
      Quaternion fromRotationMatrix = Quaternion.CreateFromRotationMatrix(Matrix.CreateLookAt(Vector3.One, Vector3.Zero, Vector3.Up));
      float num1 = (float) Math.Sin((double) this.SinceCreated * 3.1415927410125732) * 0.1f;
      float num2 = num1 - this.lastSin;
      this.lastSin = num1;
      foreach (Mesh floatingMap in this.FloatingMaps)
      {
        floatingMap.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, FezMath.WrapAngle((float) (-(double) this.SinceCreated * 2.0))) * fromRotationMatrix;
        floatingMap.Position += num2 * Vector3.UnitY;
      }
    }
    base.Update(gameTime);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    this.sinceActive += elapsed;
    float totalSeconds = (float) this.sinceActive.TotalSeconds;
    float num1 = FezMath.Saturate((float) this.sinceActive.Ticks / (float) OpenTreasure.OpeningDuration.Ticks);
    float num2 = MathHelper.Lerp(num1, Easing.EaseInOut((double) num1, EasingType.Sine, EasingType.Linear), 0.5f) * (float) Math.Pow(0.5, 0.5);
    if ((double) num1 > Math.Pow(0.5, 0.5))
      num2 = (float) Math.Pow((double) num1, 2.0);
    bool flag = this.PlayerManager.Action == ActionType.OpeningTreasure;
    if (flag)
    {
      this.chestAO.Position = this.aoOrigin + num2 * 1.375f * this.CameraManager.Viewpoint.ForwardVector() + (float) (Math.Sin((double) num2 * 1.5707963705062866 * 3.0) * 4.0 / 16.0) * Vector3.UnitY;
      this.chestAO.Rotation = this.aoInitialRotation * Quaternion.CreateFromAxisAngle(Vector3.UnitX, (float) (-(double) num2 * 1.5707963705062866 * 3.0 / 2.0));
      float num3 = num2 * 6.28318548f + this.CameraManager.Viewpoint.ToPhi();
      this.CameraManager.Direction = new Vector3((float) Math.Sin((double) num3), 0.0f, (float) Math.Cos((double) num3));
      this.CameraManager.Center = this.aoOrigin + Vector3.UnitY * 1.5f * num1;
    }
    if (!flag)
    {
      IPlayerManager playerManager = this.PlayerManager;
      playerManager.Velocity = playerManager.Velocity * 0.95f;
    }
    this.lightBox.Material.Diffuse = new Vector3(FezMath.Saturate(num2 * 1.5f) * 0.75f);
    this.lightBox.Material.Diffuse *= new Vector3(1f, 1f, 0.5f);
    this.lightBox.Scale = new Vector3(1.6f, FezMath.Saturate(num2 * 1.5f) * 1.5f, 1.2f);
    if ((double) num2 > 0.5)
    {
      if (!this.restored && this.chestAO != null)
      {
        this.restored = true;
        if (this.treasureIsAo)
        {
          this.treasureOrigin = this.aoOrigin - new Vector3(0.0f, 0.125f, 0.0f);
          int key = IdentifierPool.FirstAvailable<ArtObjectInstance>(this.LevelManager.ArtObjects);
          this.treasureAoInstance = new ArtObjectInstance(this.treasureAo)
          {
            Id = key
          };
          this.LevelManager.ArtObjects.Add(key, this.treasureAoInstance);
          this.treasureAoInstance.Initialize();
        }
        else if (this.treasureIsMail)
        {
          this.treasureOrigin = this.aoOrigin - new Vector3(0.0f, 0.125f, 0.0f);
          string treasureMapName = this.chestAO.ActorSettings.TreasureMapName;
          Texture2D texture2D1 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/mail/{treasureMapName}_1");
          Texture2D texture2D2 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/mail/{treasureMapName}_2");
          this.mail.Groups[0].Texture = (Texture) texture2D1;
          this.mail.Groups[1].Texture = (Texture) texture2D2;
        }
        else if (this.treasureIsMap)
        {
          this.treasureOrigin = this.aoOrigin - new Vector3(0.0f, 0.125f, 0.0f);
          string treasureMapName = this.chestAO.ActorSettings.TreasureMapName;
          Texture2D texture2D3 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{treasureMapName}_1");
          Texture2D texture2D4 = this.CMProvider.Global.Load<Texture2D>($"Other Textures/maps/{treasureMapName}_2");
          for (int index = 0; index < this.map.Groups.Count; ++index)
            this.map.Groups[index].Texture = index % 2 == 0 ? (Texture) texture2D3 : (Texture) texture2D4;
          if (treasureMapName == "MAP_MYSTERY")
          {
            this.mystery2Groups[0] = this.map.Groups[1];
            this.mystery2Groups[1] = this.map.Groups[3];
            this.mystery2Groups[2] = this.map.Groups[5];
            this.mystery2Xbox = texture2D4;
            this.mystery2Sony = this.CMProvider.Global.Load<Texture2D>("Other Textures/maps/MAP_MYSTERY_2_SONY");
            GamepadState.OnLayoutChanged += new EventHandler(this.UpdateControllerTexture);
            this.UpdateControllerTexture((object) null, (EventArgs) null);
          }
        }
        else
        {
          this.treasureOrigin = this.aoOrigin - this.treasureTrile.Size / 2f - new Vector3(0.0f, 0.125f, 0.0f);
          this.LevelManager.ClearTrile(new TrileEmplacement(this.treasureOrigin));
          this.LevelManager.RestoreTrile(this.treasureInstance = new TrileInstance(this.treasureOrigin, this.treasureTrile.Id));
          this.LevelMaterializer.CullInstanceIn(this.treasureInstance, true);
        }
      }
      float num4 = 2f;
      if (!flag)
        num4 = !this.treasureIsMap ? (float) (((double) Easing.EaseIn((1.0 - (double) num2) * 2.0, EasingType.Quadratic) * 2.0 + 0.5) * 2.0) : (float) (((double) Easing.EaseIn((1.0 - (double) num2) * 2.0, EasingType.Quadratic) * 4.0 + 0.5) * 2.0);
      else if (this.treasureIsAo)
        this.treasureAoInstance.Position = this.treasureOrigin + Vector3.UnitY * (num2 - 0.5f) * 4f;
      else if (this.treasureIsMap)
        this.map.Position = this.treasureOrigin + Vector3.UnitY * (num2 - 0.5f) * 4f;
      else if (this.treasureIsMail)
        this.mail.Position = this.treasureOrigin + Vector3.UnitY * (num2 - 0.5f) * 4f;
      else
        this.treasureInstance.Position = this.treasureOrigin + Vector3.UnitY * (num2 - 0.5f) * 4f;
      if (this.treasureIsAo)
      {
        Quaternion quaternion = Quaternion.CreateFromAxisAngle(Vector3.Right, -(float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
        this.treasureAoInstance.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, FezMath.WrapAngle(-totalSeconds * num4)) * quaternion;
      }
      else if (this.treasureIsMap)
      {
        if (flag)
        {
          Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0)));
          this.map.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, FezMath.WrapAngle(-totalSeconds * num4)) * fromAxisAngle;
        }
        else
          this.map.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (float) ((double) num4 * 1.0 / 30.0)) * this.map.Rotation;
      }
      else if (this.treasureIsMail)
      {
        Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.Right, 0.3926991f);
        this.mail.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, FezMath.WrapAngle(-totalSeconds * num4)) * fromAxisAngle;
      }
      else
      {
        this.treasureInstance.Phi = FezMath.WrapAngle(-totalSeconds * num4);
        this.LevelManager.UpdateInstance(this.treasureInstance);
      }
      this.flare.Position = this.solidStar.Position = this.fadedStar.Position = (this.treasureIsMail ? this.mail.Position : (this.treasureIsMap ? this.map.Position : (this.treasureIsAo ? this.treasureAoInstance.Position : this.treasureInstance.Center))) + Vector3.Normalize(-this.CameraManager.Direction) * 0.5f;
      if (this.treasureIsAo)
        this.treasureAoInstance.Position += Vector3.Transform(-this.treasureActorType.GetArtifactOffset().XYX() * new Vector3(1f, -1f, 1f) / 16f, this.treasureAoInstance.Rotation);
    }
    float num5 = FezMath.Saturate((float) (((double) num2 - 0.949999988079071) * 19.999996185302734));
    this.fadedStar.Material.Opacity = this.treasureIsMail || this.treasureIsMap || this.treasureIsAo || this.treasureTrile.ActorSettings.Type.IsCubeShard() ? num5 : 0.0f;
    this.flare.Material.Diffuse = new Vector3(num5 / 3f);
    this.flare.Scale = this.solidStar.Scale = this.fadedStar.Scale = MathHelper.Lerp(this.CameraManager.Radius * 0.6f, this.CameraManager.Radius * 0.5f, (float) (Math.Sin(this.sinceActive.TotalSeconds * 5.0) / 2.0 + 0.5)) * Vector3.One * num5;
    this.solidStar.Rotation = this.fadedStar.Rotation = this.CameraManager.Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitZ, totalSeconds * 1f);
    if (this.LevelManager.WaterType == LiquidType.Sewer)
    {
      this.lightBox.Material.Diffuse *= new Vector3(0.68235296f, 0.768627465f, 0.2509804f);
      this.flare.Material.Diffuse = new Vector3(0.68235296f, 0.768627465f, 0.2509804f) * new Vector3(num5 / 3f);
      this.flare.Scale *= 0.75f;
      this.solidStar.Enabled = true;
      this.solidStar.Material.Opacity = this.fadedStar.Material.Opacity * 0.75f;
      this.solidStar.Scale = this.CameraManager.Radius * Vector3.One;
      this.fadedStar.Material.Opacity = 0.0f;
      this.solidStar.Material.Diffuse = (new Vector3(0.68235296f, 0.768627465f, 0.2509804f) + new Vector3(0.843137264f, 0.9098039f, 0.5803922f)) / 2f;
    }
    else if (this.LevelManager.WaterType == LiquidType.Lava)
    {
      this.lightBox.Material.Diffuse *= new Vector3(0.996078432f, 0.003921569f, 0.0f);
      this.flare.Material.Diffuse = new Vector3(0.996078432f, 0.003921569f, 0.0f) * num5 / 2.5f;
      this.flare.Scale *= 0.75f;
      this.solidStar.Enabled = true;
      this.solidStar.Material.Opacity = this.fadedStar.Material.Opacity * 0.75f;
      this.solidStar.Scale = this.CameraManager.Radius * Vector3.One;
      this.fadedStar.Material.Opacity = 0.0f;
      this.solidStar.Material.Diffuse = new Vector3(0.996078432f, 0.003921569f, 0.0f);
    }
    else
      this.solidStar.Enabled = false;
    if ((double) num2 == 1.0)
    {
      this.sinceCollect += elapsed;
      if (!this.reculled && FezMath.AlmostEqual(this.CameraManager.View.Forward, FezMath.AlmostClamp(this.CameraManager.View.Forward)))
      {
        int num6 = flag ? 1 : 0;
        this.LevelMaterializer.CullInstances();
        this.reculled = true;
      }
      if (this.sinceCollect.TotalSeconds > 3.0 || this.InputManager.Jump == FezButtonState.Pressed || this.InputManager.GrabThrow == FezButtonState.Pressed)
      {
        this.SoundManager.FadeVolume(0.125f, 1f, 2f);
        if (flag)
        {
          this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(this.chestAO.Id);
          ++this.GameState.SaveData.ThisLevel.FilledConditions.ChestCount;
          this.ArtObjectService.OnTreasureOpened(this.chestAO.Id);
        }
        else if (this.PlayerManager.ForcedTreasure == null)
        {
          if (this.treasureIsMap)
          {
            this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(this.treasureAoInstance.Id);
            ++this.GameState.SaveData.ThisLevel.FilledConditions.OtherCollectibleCount;
          }
          else
          {
            this.GameState.SaveData.ThisLevel.DestroyedTriles.Add(this.treasureInstance.OriginalEmplacement);
            if (!this.treasureInstance.Foreign)
            {
              if (this.treasureInstance.Trile.ActorSettings.Type == ActorType.CubeShard)
                ++this.GameState.SaveData.ThisLevel.FilledConditions.CubeShardCount;
              else
                ++this.GameState.SaveData.ThisLevel.FilledConditions.OtherCollectibleCount;
            }
          }
        }
        if (!flag)
          this.CameraManager.PixelsPerTrixel = this.lastZoom;
        this.CameraManager.Constrained = this.WasConstrained;
        if (this.WasConstrained)
        {
          this.CameraManager.Center = this.OldCenter;
          this.CameraManager.PixelsPerTrixel = this.OldPixPerTrix;
        }
        this.CameraManager.PanningConstraints = this.OldPan;
        this.PlayerManager.Action = ActionType.Idle;
        switch (this.treasureActorType)
        {
          case ActorType.CubeShard:
            ++this.GameState.SaveData.CubeShards;
            if (this.GameState.SaveData.CubeShards > 32 /*0x20*/)
              this.GameState.SaveData.CubeShards = 32 /*0x20*/;
            if (this.PlayerManager.ForcedTreasure != null)
              this.GameState.SaveData.CollectedParts = 0;
            this.GameState.SaveData.ScoreDirty = true;
            this.GomezService.OnCollectedShard();
            this.CheckCubes();
            SpeedRun.AddCube(false);
            break;
          case ActorType.SkeletonKey:
            ++this.GameState.SaveData.Keys;
            if (!this.GameState.SaveData.OneTimeTutorials.ContainsKey("DOT_KEY_A"))
            {
              this.GameState.SaveData.OneTimeTutorials.Add("DOT_KEY_A", true);
              this.DotService.Say("DOT_KEY_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_KEY_B", true, true));
              break;
            }
            break;
          case ActorType.NumberCube:
            this.GameState.SaveData.Artifacts.Add(this.treasureActorType);
            this.DotService.Say("DOT_ANCIENT_ARTIFACT", true, false).Ended = (Action) (() => this.DotService.Say("DOT_NUMBERS_A", true, true));
            break;
          case ActorType.LetterCube:
            this.GameState.SaveData.Artifacts.Add(this.treasureActorType);
            this.DotService.Say("DOT_ANCIENT_ARTIFACT", true, false).Ended = (Action) (() => this.DotService.Say("DOT_ALPHABET_A", true, true));
            break;
          case ActorType.TriSkull:
            this.GameState.SaveData.Artifacts.Add(this.treasureActorType);
            this.DotService.Say("DOT_ANCIENT_ARTIFACT", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TRISKULL_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TRISKULL_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TRISKULL_C", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TRISKULL_D", true, true)))));
            break;
          case ActorType.Tome:
            this.GameState.SaveData.Artifacts.Add(this.treasureActorType);
            this.DotService.Say("DOT_ANCIENT_ARTIFACT", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TOME_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TOME_B", true, true)));
            break;
          case ActorType.SecretCube:
            ++this.GameState.SaveData.SecretCubes;
            if (this.GameState.SaveData.SecretCubes > 32 /*0x20*/)
              this.GameState.SaveData.SecretCubes = 32 /*0x20*/;
            this.GameState.SaveData.ScoreDirty = true;
            this.treasureInstance.Collected = true;
            if (this.treasureInstance.GlobalSpawn)
              this.GomezService.OnCollectedGlobalAnti();
            else
              this.GomezService.OnCollectedAnti();
            SpeedRun.AddCube(true);
            if (this.GameState.SaveData.SecretCubes == 1)
            {
              this.DotService.Say("DOT_ANTI_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_ANTI_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_ANTI_C", true, false).Ended = (Action) (() => this.DotService.Say("DOT_ANTI_D", true, true).Ended = new Action(this.CheckCubes))));
              break;
            }
            this.CheckCubes();
            break;
          case ActorType.TreasureMap:
            if (!flag)
              this.GameState.SaveData.Maps.Add(this.treasureAoInstance.ActorSettings.TreasureMapName);
            else
              this.GameState.SaveData.Maps.Add(this.chestAO.ActorSettings.TreasureMapName);
            if (this.GameState.SaveData.Maps.Count == 1)
            {
              this.DotService.Say("DOT_TREASURE_MAP_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TREASURE_MAP_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TREASURE_MAP_C", true, false).Ended = (Action) (() => this.DotService.Say("DOT_TREASURE_MAP_D", true, true))));
              break;
            }
            break;
          case ActorType.PieceOfHeart:
            ++this.GameState.SaveData.PiecesOfHeart;
            if (this.GameState.SaveData.PiecesOfHeart > 3)
              this.GameState.SaveData.PiecesOfHeart = 3;
            this.GameState.SaveData.ScoreDirty = true;
            this.GomezService.OnCollectedPieceOfHeart();
            this.DotService.Say("DOT_HEART_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_HEART_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_HEART_C", true, true)));
            break;
        }
        if (this.treasureActorType != ActorType.PieceOfHeart)
          this.GameState.OnHudElementChanged();
        this.chestAO = (ArtObjectInstance) null;
        if (this.PlayerManager.ForcedTreasure != null)
        {
          this.PlayerManager.ForcedTreasure.Phi = 0.0f;
          this.PlayerManager.ForcedTreasure.Collected = true;
          this.LevelManager.UpdateInstance(this.PlayerManager.ForcedTreasure);
          this.LevelManager.ClearTrile(this.PlayerManager.ForcedTreasure);
          this.PlayerManager.ForcedTreasure = (TrileInstance) null;
          this.treasureInstance = (TrileInstance) null;
        }
        else if (this.treasureIsAo)
        {
          this.treasureAoInstance.SoftDispose();
          this.LevelManager.ArtObjects.Remove(this.treasureAoInstance.Id);
          this.treasureAoInstance = (ArtObjectInstance) null;
        }
        else if (!this.treasureIsMap && !this.treasureIsMail)
        {
          this.treasureInstance.Phi = 0.0f;
          this.treasureInstance.Collected = true;
          this.LevelManager.UpdateInstance(this.treasureInstance);
          this.LevelManager.ClearTrile(this.treasureInstance);
          this.treasureInstance = (TrileInstance) null;
        }
        this.GameState.Save();
      }
    }
    this.PlayerManager.Animation.Timing.Update(elapsed, 0.9f);
    return false;
  }

  private void CheckCubes()
  {
    switch (this.GameState.SaveData.CubeShards + this.GameState.SaveData.SecretCubes)
    {
      case 4:
        this.DotService.Say("DOT_CUBES_FOUR_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_FOUR_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_FOUR_C", true, true)));
        break;
      case 8:
        this.DotService.Say("DOT_CUBES_EIGHT_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_EIGHT_B", true, true));
        break;
      case 16 /*0x10*/:
        this.DotService.Say("DOT_CUBES_SIXTEEN_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_SIXTEEN_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_SIXTEEN_C", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_SIXTEEN_D", true, true))));
        break;
      case 32 /*0x20*/:
        this.DotService.Say("DOT_CUBES_THIRTYTWO_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_THIRTYTWO_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_THIRTYTWO_C", true, true)));
        break;
      case 64 /*0x40*/:
        this.DotService.Say("DOT_CUBES_SIXTYFOUR_A", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_SIXTYFOUR_B", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_SIXTYFOUR_C", true, false).Ended = (Action) (() => this.DotService.Say("DOT_CUBES_SIXTYFOUR_D", true, true))));
        break;
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading)
      return;
    foreach (Mesh floatingMap in this.FloatingMaps)
      floatingMap.Draw();
    this.GraphicsDevice.GetRasterCombiner();
    this.DoDraw(false);
  }

  private void DoDraw()
  {
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    foreach (Mesh floatingMap in this.FloatingMaps)
    {
      (floatingMap.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      floatingMap.Draw();
      (floatingMap.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    }
    this.DoDraw(true);
  }

  private void DoDraw(bool lightPrePass)
  {
    if (!this.IsActionAllowed(this.PlayerManager.Action) || this.LevelManager.BlinkingAlpha & lightPrePass)
      return;
    bool flag = this.PlayerManager.Action == ActionType.OpeningTreasure;
    if (lightPrePass)
    {
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
      (this.flare.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      (this.lightBox.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      (this.map.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      (this.mail.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      this.flare.Blending = new BlendingMode?(BlendingMode.Alphablending);
      this.lightBox.Blending = new BlendingMode?(BlendingMode.Alphablending);
      this.flare.AlwaysOnTop = true;
    }
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Trails));
    if (flag)
      this.lightBox.Draw();
    if (this.treasureIsMap && (this.restored || !flag))
      this.map.Draw();
    if (this.treasureIsMail && this.restored)
      this.mail.Draw();
    if (!lightPrePass)
    {
      this.fadedStar.Draw();
      this.solidStar.Draw();
    }
    if (this.LevelManager.WaterType != LiquidType.Sewer && !this.LevelManager.BlinkingAlpha)
    {
      this.GraphicsDevice.GetDssCombiner().StencilEnable = false;
      this.flare.Draw();
      this.GraphicsDevice.GetDssCombiner().StencilEnable = true;
    }
    this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    if (!lightPrePass)
      return;
    (this.flare.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    (this.lightBox.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    (this.map.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    (this.mail.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    this.flare.Blending = new BlendingMode?(BlendingMode.Additive);
    this.lightBox.Blending = new BlendingMode?(BlendingMode.Additive);
    this.flare.AlwaysOnTop = false;
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.OpeningTreasure || type == ActionType.FindingTreasure;
  }

  private void UpdateControllerTexture(object sender, EventArgs e)
  {
    if (GamepadState.Layout == GamepadState.GamepadLayout.Xbox360)
    {
      foreach (Group mystery2Group in this.mystery2Groups)
        mystery2Group.Texture = (Texture) this.mystery2Xbox;
    }
    else
    {
      foreach (Group mystery2Group in this.mystery2Groups)
        mystery2Group.Texture = (Texture) this.mystery2Sony;
    }
  }

  [ServiceDependency]
  public IArtObjectService ArtObjectService { private get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { private get; set; }

  [ServiceDependency]
  public IDotService DotService { private get; set; }

  private struct TreasureActorSettings
  {
    public ArtObjectInstance AoInstance;
    public ArtObject ArtObject;
  }
}
