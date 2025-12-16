// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SplitUpCubeHost
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

internal class SplitUpCubeHost : DrawableGameComponent
{
  private SoundEffect[] CollectSounds;
  private SplitCollectorEffect SplitCollectorEffect;
  private float WireOpacityFactor;
  private bool WirecubeVisible;
  private bool SolidCubesVisible;
  private Mesh WireframeCube;
  private Mesh SolidCubes;
  private readonly List<SplitUpCubeHost.SwooshingCube> TrackedCollects = new List<SplitUpCubeHost.SwooshingCube>();
  private SplitUpCubeHost.TrailsRenderer trailsRenderer;
  private float SinceNoTrails;
  private float SinceCollect;
  private bool AssembleScheduled;
  private readonly List<TrileInstance> TrackedBits = new List<TrileInstance>();
  private Mesh ChimeOutline;
  private float UntilNextShine;
  private TrileInstance ShineOn;
  private SoundEffect sBitChime;
  public const int ShineRate = 7;
  private readonly Vector3[] CubeOffsets = new Vector3[8]
  {
    new Vector3(0.25f, -0.25f, 0.25f),
    new Vector3(-0.25f, -0.25f, 0.25f),
    new Vector3(-0.25f, -0.25f, -0.25f),
    new Vector3(0.25f, -0.25f, -0.25f),
    new Vector3(0.25f, 0.25f, 0.25f),
    new Vector3(-0.25f, 0.25f, 0.25f),
    new Vector3(-0.25f, 0.25f, -0.25f),
    new Vector3(0.25f, 0.25f, -0.25f)
  };
  private TimeSpan timeAcc;

  public SplitUpCubeHost(Game game)
    : base(game)
  {
    this.DrawOrder = 75;
  }

  public override void Initialize()
  {
    base.Initialize();
    ServiceHelper.AddComponent((IGameComponent) (this.trailsRenderer = new SplitUpCubeHost.TrailsRenderer(this.Game, this)));
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.trailsRenderer.Visible = this.Enabled = this.Visible = false;
    this.SoundManager.SongChanged += new Action(this.RefreshSounds);
    this.LightingPostProcess.DrawGeometryLights += new Action<GameTime>(this.DrawLights);
  }

  protected override void LoadContent()
  {
    this.SolidCubes = new Mesh()
    {
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    this.ChimeOutline = new Mesh() { DepthWrites = false };
    this.ChimeOutline.AddWireframePolygon(Color.Yellow, new Vector3(0.0f, 0.7071068f, 0.0f), new Vector3(0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, -0.7071068f, 0.0f), new Vector3(-0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, 0.7071068f, 0.0f));
    DrawActionScheduler.Schedule((Action) (() =>
    {
      Mesh chimeOutline = this.ChimeOutline;
      chimeOutline.Effect = (BaseEffect) new DefaultEffect.VertexColored()
      {
        Fullbright = true,
        AlphaIsEmissive = false
      };
      Mesh solidCubes = this.SolidCubes;
      solidCubes.Effect = (BaseEffect) new DefaultEffect.LitTextured()
      {
        Specular = true,
        Emissive = 0.5f,
        AlphaIsEmissive = true
      };
    }));
    this.ChimeOutline.AddWireframePolygon(new Color(Color.Yellow.ToVector3() * 0.333333343f), new Vector3(0.0f, 0.7071068f, 0.0f), new Vector3(0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, -0.7071068f, 0.0f), new Vector3(-0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, 0.7071068f, 0.0f));
    this.ChimeOutline.AddWireframePolygon(new Color(Color.Yellow.ToVector3() * 0.111111112f), new Vector3(0.0f, 0.7071068f, 0.0f), new Vector3(0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, -0.7071068f, 0.0f), new Vector3(-0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, 0.7071068f, 0.0f));
    this.ChimeOutline.AddWireframePolygon(new Color(Color.Yellow.ToVector3() * 0.0370370373f), new Vector3(0.0f, 0.7071068f, 0.0f), new Vector3(0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, -0.7071068f, 0.0f), new Vector3(-0.7071068f, 0.0f, 0.0f), new Vector3(0.0f, 0.7071068f, 0.0f));
    this.sBitChime = this.CMProvider.Global.Load<SoundEffect>("Sounds/Collects/BitChime");
  }

  private void RefreshSounds()
  {
    this.CollectSounds = new SoundEffect[8];
    TrackedSong currentlyPlayingSong = this.SoundManager.CurrentlyPlayingSong;
    ShardNotes[] shardNotesArray;
    if (currentlyPlayingSong != null)
      shardNotesArray = currentlyPlayingSong.Notes;
    else
      shardNotesArray = new ShardNotes[8]
      {
        ShardNotes.C2,
        ShardNotes.D2,
        ShardNotes.E2,
        ShardNotes.F2,
        ShardNotes.G2,
        ShardNotes.A2,
        ShardNotes.B2,
        ShardNotes.C3
      };
    for (int index = 0; index < shardNotesArray.Length; ++index)
      this.CollectSounds[index] = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Collects/SplitUpCube/" + (object) shardNotesArray[index]);
  }

  private void TryInitialize()
  {
    if (this.WireframeCube == null)
    {
      this.WireframeCube = new Mesh()
      {
        Material = {
          Diffuse = Vector3.One,
          Opacity = 1f
        },
        Blending = new BlendingMode?(BlendingMode.Alphablending)
      };
      DrawActionScheduler.Schedule((Action) (() => this.WireframeCube.Effect = (BaseEffect) (this.SplitCollectorEffect = new SplitCollectorEffect())));
      for (int index = 0; index < 7; ++index)
      {
        Mesh wireframeCube1 = this.WireframeCube;
        Vector3 one = Vector3.One;
        Vector3 zero = Vector3.Zero;
        double r1 = index == 0 ? 1.0 : (index == 1 ? 0.0 : 0.5);
        double g1;
        switch (index)
        {
          case 2:
            g1 = 1.0;
            break;
          case 3:
            g1 = 0.0;
            break;
          default:
            g1 = 0.5;
            break;
        }
        double b1;
        switch (index)
        {
          case 4:
            b1 = 1.0;
            break;
          case 5:
            b1 = 0.0;
            break;
          default:
            b1 = 0.5;
            break;
        }
        double alpha1 = index == 6 ? 1.0 : 0.0;
        Color color1 = new Color((float) r1, (float) g1, (float) b1, (float) alpha1);
        wireframeCube1.AddWireframeBox(one, zero, color1, true);
        foreach (Vector3 cubeOffset in this.CubeOffsets)
        {
          Mesh wireframeCube2 = this.WireframeCube;
          Vector3 size = Vector3.One / 2f;
          Vector3 origin = cubeOffset;
          double r2;
          switch (index)
          {
            case 0:
              r2 = 1.0;
              break;
            case 1:
              r2 = 0.0;
              break;
            default:
              r2 = 0.5;
              break;
          }
          double g2;
          switch (index)
          {
            case 2:
              g2 = 1.0;
              break;
            case 3:
              g2 = 0.0;
              break;
            default:
              g2 = 0.5;
              break;
          }
          double b2;
          switch (index)
          {
            case 4:
              b2 = 1.0;
              break;
            case 5:
              b2 = 0.0;
              break;
            default:
              b2 = 0.5;
              break;
          }
          double alpha2 = index == 6 ? 0.625 : 0.375;
          Color color2 = new Color((float) r2, (float) g2, (float) b2, (float) alpha2);
          wireframeCube2.AddWireframeBox(size, origin, color2, true);
        }
      }
      this.WireframeCube.CollapseToBuffer<FezVertexPositionColor>();
    }
    this.SolidCubesVisible = true;
    if (this.TrackedCollects.Count > 0)
    {
      this.GameState.SaveData.CollectedParts += this.TrackedCollects.Count;
      Waiters.Wait(0.5, (Action) (() => Waiters.Wait((Func<bool>) (() => this.PlayerManager.CanControl && this.PlayerManager.Grounded), (Action) (() =>
      {
        this.GomezService.OnCollectedSplitUpCube();
        this.GameState.OnHudElementChanged();
        this.GameState.Save();
        this.TryAssembleCube();
      }))));
    }
    foreach (SplitUpCubeHost.SwooshingCube trackedCollect in this.TrackedCollects)
      trackedCollect.Dispose();
    this.TrackedCollects.Clear();
    if (this.LevelManager.TrileSet == null)
    {
      this.trailsRenderer.Visible = this.Enabled = this.Visible = false;
    }
    else
    {
      Trile goldenCubeTrile = this.LevelManager.ActorTriles(ActorType.GoldenCube).FirstOrDefault<Trile>();
      IEnumerable<TrileInstance> source = this.LevelManager.Triles.Values.Union<TrileInstance>(this.LevelManager.Triles.SelectMany<KeyValuePair<TrileEmplacement, TrileInstance>, TrileInstance>((Func<KeyValuePair<TrileEmplacement, TrileInstance>, IEnumerable<TrileInstance>>) (x => (IEnumerable<TrileInstance>) x.Value.OverlappedTriles ?? Enumerable.Empty<TrileInstance>())));
      this.trailsRenderer.Visible = this.Enabled = this.Visible = goldenCubeTrile != null && (source.Count<TrileInstance>((Func<TrileInstance, bool>) (x => x.TrileId == goldenCubeTrile.Id)) != 0 || this.AssembleScheduled || this.GameState.SaveData.CollectedParts == 8);
      if (!this.Enabled)
        return;
      this.RefreshSounds();
      this.TrackedBits.Clear();
      this.TrackedBits.AddRange(source.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.TrileId == goldenCubeTrile.Id)));
      this.SolidCubes.ClearGroups();
      ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry = goldenCubeTrile.Geometry;
      this.SolidCubes.Position = Vector3.Zero;
      this.SolidCubes.Rotation = Quaternion.Identity;
      foreach (Vector3 cubeOffset in this.CubeOffsets)
      {
        Group group = this.SolidCubes.AddGroup();
        group.Geometry = (IIndexedPrimitiveCollection) new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(((IEnumerable<VertexPositionNormalTextureInstance>) geometry.Vertices).ToArray<VertexPositionNormalTextureInstance>(), geometry.Indices, geometry.PrimitiveType);
        group.Position = cubeOffset;
        group.BakeTransform<VertexPositionNormalTextureInstance>();
        group.Enabled = false;
      }
      DrawActionScheduler.Schedule((Action) (() => this.SolidCubes.Texture = this.LevelMaterializer.TrilesMesh.Texture));
      this.SolidCubes.Rotation = this.WireframeCube.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Right, (float) Math.Asin(Math.Sqrt(2.0) / Math.Sqrt(3.0))) * Quaternion.CreateFromAxisAngle(Vector3.Up, 0.7853982f);
      this.WireOpacityFactor = 1f;
      this.SinceNoTrails = 3f;
      this.ShineOn = (TrileInstance) null;
      this.UntilNextShine = 7f;
      if (this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.BlinkingAlpha)
      {
        if (this.SolidCubes.Effect is DefaultEffect.LitTextured)
          DrawActionScheduler.Schedule((Action) (() =>
          {
            Mesh solidCubes = this.SolidCubes;
            solidCubes.Effect = (BaseEffect) new DefaultEffect.Textured()
            {
              AlphaIsEmissive = true,
              IgnoreCache = true
            };
          }));
      }
      else if (this.SolidCubes.Effect is DefaultEffect.Textured)
        DrawActionScheduler.Schedule((Action) (() =>
        {
          Mesh solidCubes = this.SolidCubes;
          solidCubes.Effect = (BaseEffect) new DefaultEffect.LitTextured()
          {
            Specular = true,
            Emissive = 0.5f,
            AlphaIsEmissive = true,
            IgnoreCache = true
          };
        }));
      this.TryAssembleCube();
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.TimePaused || !this.CameraManager.Viewpoint.IsOrthographic() || this.AssembleScheduled)
      return;
    this.ShineOnYouCrazyDiamonds((float) gameTime.ElapsedGameTime.TotalSeconds);
    if (this.GameState.SaveData.CollectedParts + this.TrackedCollects.Count != 8 && this.PlayerManager.Action != ActionType.GateWarp && this.PlayerManager.Action != ActionType.LesserWarp && !this.PlayerManager.Action.IsSwimming())
    {
      TrileInstance collect = this.PlayerManager.AxisCollision[VerticalDirection.Up].Surface ?? this.PlayerManager.AxisCollision[VerticalDirection.Down].Surface;
      if (collect != null && collect.Trile.ActorSettings.Type == ActorType.GoldenCube && !this.TrackedCollects.Any<SplitUpCubeHost.SwooshingCube>((Func<SplitUpCubeHost.SwooshingCube, bool>) (x => x.Instance == collect)) && !this.LevelManager.Triles.Values.Any<TrileInstance>((Func<TrileInstance, bool>) (x => x.Overlaps && x.OverlappedTriles.Contains(collect) && x.Position == collect.Position)))
      {
        int index = this.GameState.SaveData.CollectedParts + this.TrackedCollects.Count;
        this.CollectSounds[index].Emit();
        this.TrackedCollects.Add(new SplitUpCubeHost.SwooshingCube(collect, this.SolidCubes, this.CubeOffsets[index], this.SolidCubes.Rotation));
        this.GameState.SaveData.ThisLevel.DestroyedTriles.Add(collect.OriginalEmplacement);
        ++this.GameState.SaveData.ThisLevel.FilledConditions.SplitUpCount;
        this.LevelManager.ClearTrile(collect);
        this.TrackedBits.Remove(collect);
        if ((double) this.SinceNoTrails == 3.0)
          this.SinceCollect = 0.0f;
      }
    }
    for (int index = this.TrackedCollects.Count - 1; index >= 0; --index)
    {
      if (this.TrackedCollects[index].Spline.Reached)
      {
        ++this.GameState.SaveData.CollectedParts;
        this.GomezService.OnCollectedSplitUpCube();
        this.TrackedCollects.RemoveAt(index);
        this.GameState.OnHudElementChanged();
        this.GameState.Save();
        this.TryAssembleCube();
      }
      this.SinceNoTrails = 0.0f;
    }
    double sinceNoTrails = (double) this.SinceNoTrails;
    TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
    double totalSeconds1 = elapsedGameTime.TotalSeconds;
    this.SinceNoTrails = Math.Min(3f, (float) (sinceNoTrails + totalSeconds1));
    double sinceCollect = (double) this.SinceCollect;
    elapsedGameTime = gameTime.ElapsedGameTime;
    double totalSeconds2 = elapsedGameTime.TotalSeconds;
    this.SinceCollect = Math.Min(1f, (float) (sinceCollect + totalSeconds2));
    for (int index = 0; index < 8; ++index)
      this.SolidCubes.Groups[index].Enabled = this.GameState.SaveData.CollectedParts > index;
    this.WirecubeVisible = (double) this.SinceNoTrails < 3.0;
    this.WireOpacityFactor = (1f - FezMath.Saturate((float) (((double) this.SinceNoTrails - 1.0) / 1.0))) * this.SinceCollect;
    if (this.GameState.SaveData.CollectedParts + this.TrackedCollects.Count == 0)
      this.WireOpacityFactor = 0.0f;
    this.SolidCubes.Material.Opacity = this.WireOpacityFactor;
    this.WirecubeVisible = (double) this.WireOpacityFactor != 0.0;
    this.SolidCubesVisible = this.WirecubeVisible;
  }

  private void ShineOnYouCrazyDiamonds(float elapsedTime)
  {
    this.UntilNextShine -= elapsedTime;
    if ((double) this.UntilNextShine <= 0.0 && this.TrackedBits.Count > 0 && this.PlayerManager.CanControl && this.CameraManager.ViewTransitionReached)
    {
      this.UntilNextShine = 7f;
      this.ChimeOutline.Scale = new Vector3(0.1f);
      this.ChimeOutline.Groups[0].Scale = Vector3.One;
      this.ChimeOutline.Groups[1].Scale = Vector3.One;
      this.ChimeOutline.Groups[2].Scale = Vector3.One;
      this.ChimeOutline.Groups[3].Scale = Vector3.One;
      this.ShineOn = RandomHelper.InList<TrileInstance>(this.TrackedBits);
      this.sBitChime.EmitAt(this.ShineOn.Center);
    }
    if (this.ShineOn == null)
      return;
    this.ChimeOutline.Position = this.ShineOn.Center;
    this.ChimeOutline.Rotation = this.CameraManager.Rotation;
    this.ChimeOutline.Scale = new Vector3((float) ((double) Easing.EaseInOut((double) FezMath.Saturate(7f - this.UntilNextShine), EasingType.Quadratic) * 10.0 + (double) Easing.EaseIn(7.0 - (double) this.UntilNextShine, EasingType.Quadratic) * 7.0)) * 0.75f;
    this.ChimeOutline.Groups[0].Scale /= 1.002f;
    this.ChimeOutline.Groups[1].Scale /= 1.006f;
    this.ChimeOutline.Groups[2].Scale /= 1.012f;
    this.ChimeOutline.Groups[3].Scale /= 1.018f;
    this.ChimeOutline.Material.Diffuse = new Vector3((float) ((double) Easing.EaseIn((double) FezMath.Saturate((float) (1.0 - (double) this.ChimeOutline.Scale.X / 40.0)), EasingType.Quadratic) * (1.0 - (double) this.TimeManager.NightContribution * 0.64999997615814209) * (1.0 - (double) this.TimeManager.DawnContribution * 0.699999988079071) * (1.0 - (double) this.TimeManager.DuskContribution * 0.699999988079071)));
    this.ChimeOutline.Blending = new BlendingMode?(BlendingMode.Additive);
    if ((double) this.ChimeOutline.Scale.X <= 40.0)
      return;
    this.ShineOn = (TrileInstance) null;
  }

  private void TryAssembleCube()
  {
    if (this.AssembleScheduled || this.GameState.SaveData.CollectedParts != 8)
      return;
    this.AssembleScheduled = true;
    Waiters.Wait((Func<bool>) (() => !this.GameState.Loading && this.PlayerManager.Action.AllowsLookingDirectionChange() && this.SpeechBubble.Hidden && !this.GameState.ForceTimePaused && this.PlayerManager.CanControl && !this.PlayerManager.Action.DisallowsRespawn() && this.CameraManager.ViewTransitionReached && !this.PlayerManager.InDoorTransition && this.PlayerManager.CarriedInstance == null), (Action) (() => Waiters.Wait(0.0, (Action) (() =>
    {
      Vector3 vector3_1 = this.CameraManager.Viewpoint.DepthMask();
      Vector3 vector3_2 = this.CameraManager.Viewpoint.ForwardVector();
      TrileInstance trileInstance = new TrileInstance((this.PlayerManager.Position + Vector3.UnitY * (float) (Math.Sin(this.timeAcc.TotalSeconds * 3.1415927410125732) * 0.10000000149011612 + 2.0) - FezMath.HalfVector) * (Vector3.One - vector3_1) - vector3_2 * (this.LevelManager.Size / 2f - vector3_1 * 2f) + vector3_1 * this.LevelManager.Size / 2f, this.LevelManager.TrileSet.Triles.Values.Last<Trile>((Func<Trile, bool>) (x => x.ActorSettings.Type == ActorType.CubeShard)).Id);
      this.LevelManager.RestoreTrile(trileInstance);
      this.LevelMaterializer.CullInstanceIn(trileInstance);
      this.PlayerManager.ForcedTreasure = trileInstance;
      this.PlayerManager.Action = ActionType.FindingTreasure;
      this.AssembleScheduled = false;
    }))));
  }

  private void ClearDepth(Mesh mesh)
  {
    bool depthWrites = mesh.DepthWrites;
    ColorWriteChannels colorWriteChannels = this.GraphicsDevice.GetBlendCombiner().ColorWriteChannels;
    mesh.DepthWrites = true;
    this.GraphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
    mesh.AlwaysOnTop = true;
    mesh.Position += this.CameraManager.InverseView.Forward * 2f;
    mesh.Draw();
    mesh.AlwaysOnTop = false;
    mesh.Position -= this.CameraManager.InverseView.Forward * 2f;
    this.GraphicsDevice.SetColorWriteChannels(colorWriteChannels);
    mesh.DepthWrites = depthWrites;
  }

  private void DrawLights(GameTime gameTime)
  {
    if (this.GameState.Loading || !this.Visible || this.PlayerManager.Action == ActionType.FindingTreasure)
      return;
    if (this.ShineOn != null)
      this.ChimeOutline.Draw();
    if ((double) this.SolidCubes.Material.Opacity < 0.25)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    foreach (SplitUpCubeHost.SwooshingCube trackedCollect in this.TrackedCollects)
      this.ClearDepth(trackedCollect.Cube);
    if (this.SolidCubesVisible)
    {
      this.ClearDepth(this.SolidCubes);
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
      (this.SolidCubes.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      this.SolidCubes.Draw();
      (this.SolidCubes.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    }
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    foreach (SplitUpCubeHost.SwooshingCube trackedCollect in this.TrackedCollects)
    {
      (trackedCollect.Cube.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      trackedCollect.Cube.Draw();
      (trackedCollect.Cube.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    }
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.PlayerManager.Action == ActionType.FindingTreasure)
      return;
    if (this.SolidCubesVisible || this.WirecubeVisible)
    {
      this.SolidCubes.Position = this.WireframeCube.Position = GomezHost.Instance.PlayerMesh.Position + Vector3.UnitY * (float) (Math.Sin(this.timeAcc.TotalSeconds * 3.1415927410125732) * 0.10000000149011612 + 2.0);
      Mesh solidCubes = this.SolidCubes;
      Mesh wireframeCube = this.WireframeCube;
      Vector3 up = Vector3.Up;
      double totalSeconds = gameTime.ElapsedGameTime.TotalSeconds;
      Quaternion quaternion1;
      Quaternion quaternion2 = quaternion1 = Quaternion.CreateFromAxisAngle(up, (float) totalSeconds) * this.WireframeCube.Rotation;
      wireframeCube.Rotation = quaternion1;
      Quaternion quaternion3 = quaternion2;
      solidCubes.Rotation = quaternion3;
      this.SolidCubes.Position += this.PlayerManager.SplitUpCubeCollectorOffset;
      this.WireframeCube.Position += this.PlayerManager.SplitUpCubeCollectorOffset;
      this.timeAcc += gameTime.ElapsedGameTime;
    }
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    if (this.WirecubeVisible)
      this.ClearDepth(this.WireframeCube);
    foreach (SplitUpCubeHost.SwooshingCube trackedCollect in this.TrackedCollects)
    {
      trackedCollect.Update(gameTime);
      this.ClearDepth(trackedCollect.Cube);
    }
    if (this.SolidCubesVisible)
    {
      this.ClearDepth(this.SolidCubes);
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
      this.SolidCubes.Draw();
    }
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    foreach (SplitUpCubeHost.SwooshingCube trackedCollect in this.TrackedCollects)
      trackedCollect.Cube.Draw();
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    if (this.ShineOn != null)
      this.ChimeOutline.Draw();
    if (!this.SolidCubes.Effect.IgnoreCache)
      return;
    this.SolidCubes.Effect.IgnoreCache = false;
  }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { get; set; }

  [ServiceDependency]
  public IGomezService GomezService { get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { get; set; }

  private class TrailsRenderer : DrawableGameComponent
  {
    private readonly SplitUpCubeHost Host;

    public TrailsRenderer(Game game, SplitUpCubeHost host)
      : base(game)
    {
      this.Host = host;
      this.DrawOrder = 101;
    }

    public override void Draw(GameTime gameTime)
    {
      if (this.Host.GameState.Loading || this.Host.PlayerManager.Action == ActionType.FindingTreasure)
        return;
      GraphicsDevice graphicsDevice = this.GraphicsDevice;
      if (this.Host.WirecubeVisible)
      {
        graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Wirecube));
        this.Host.WireframeCube.DepthWrites = false;
        switch (this.Host.LevelManager.WaterType)
        {
          case LiquidType.Lava:
            this.Host.WireframeCube.Material.Diffuse = new Vector3((float) byte.MaxValue, 0.0f, 0.0f) / (float) byte.MaxValue;
            break;
          case LiquidType.Sewer:
            this.Host.WireframeCube.Material.Diffuse = new Vector3(215f, 232f, 148f) / (float) byte.MaxValue;
            break;
          default:
            this.Host.WireframeCube.Material.Diffuse = Vector3.One;
            break;
        }
        this.Host.SplitCollectorEffect.Offset = (float) (1.0 / 16.0 / (double) this.Host.CameraManager.PixelsPerTrixel * (double) Math.Abs((float) Math.Sin(this.Host.timeAcc.TotalSeconds)) * 8.0);
        this.Host.SplitCollectorEffect.VaryingOpacity = (float) (0.05000000074505806 + (double) Math.Abs((float) Math.Cos(this.Host.timeAcc.TotalSeconds * 3.0)) * 0.20000000298023224);
        this.Host.WireframeCube.Material.Opacity = this.Host.WireOpacityFactor;
        this.Host.WireframeCube.Draw();
      }
      foreach (SplitUpCubeHost.SwooshingCube trackedCollect in this.Host.TrackedCollects)
      {
        graphicsDevice.PrepareStencilReadWrite(CompareFunction.NotEqual, FezEngine.Structure.StencilMask.Trails);
        trackedCollect.Trail.Draw();
        graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
        graphicsDevice.GetDssCombiner().DepthBufferWriteEnable = false;
        graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
        trackedCollect.Trail.Draw();
        graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
        graphicsDevice.GetDssCombiner().DepthBufferWriteEnable = true;
      }
      graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
    }
  }

  private class SwooshingCube
  {
    private readonly Vector3 RedTrail = new Vector3(254f, 1f, 0.0f) / (float) byte.MaxValue;
    private readonly Vector3 StandardTrail = new Vector3((float) byte.MaxValue, 230f, 96f) / (float) byte.MaxValue;
    private readonly Vector3 SewerTrail = new Vector3(215f, 232f, 148f) / (float) byte.MaxValue;
    private readonly Vector3 CMYTrail = new Vector3(1f, 1f, 0.0f);
    private const float TrailRadius = 0.5f;
    public readonly Mesh Trail;
    public readonly Mesh Cube;
    public readonly TrileInstance Instance;
    public readonly Vector3SplineInterpolation Spline;
    private readonly IndexedUserPrimitives<FezVertexPositionColor> TrailGeometry;
    private readonly Mesh DestinationMesh;
    private readonly Vector3 sideDirection;
    private readonly Vector3 color;
    private readonly Vector3 positionOffset;
    private Vector3 lastPoint;
    private float lastStep;
    private Quaternion rotation;
    private FezVertexPositionColor[] TrailVertices;
    private int[] TrailIndices;

    public SwooshingCube(
      TrileInstance instance,
      Mesh destinationMesh,
      Vector3 Offset,
      Quaternion Rotation)
    {
      this.CameraManager = ServiceHelper.Get<IGameCameraManager>();
      this.LevelManager = ServiceHelper.Get<ILevelManager>();
      this.LevelMaterializer = ServiceHelper.Get<ILevelMaterializer>();
      this.rotation = Rotation;
      this.positionOffset = Offset;
      this.color = this.StandardTrail;
      switch (this.LevelManager.WaterType)
      {
        case LiquidType.Lava:
          this.color = this.RedTrail;
          break;
        case LiquidType.Sewer:
          this.color = this.SewerTrail;
          break;
      }
      if (this.LevelManager.BlinkingAlpha)
        this.color = this.CMYTrail;
      this.Trail = new Mesh()
      {
        Effect = (BaseEffect) new DefaultEffect.VertexColored(),
        Culling = CullMode.None,
        Blending = new BlendingMode?(BlendingMode.Additive),
        AlwaysOnTop = true
      };
      this.Cube = new Mesh()
      {
        Texture = this.LevelMaterializer.TrilesMesh.Texture
      };
      if (this.LevelManager.WaterType == LiquidType.Sewer || this.LevelManager.WaterType == LiquidType.Lava || this.LevelManager.BlinkingAlpha)
      {
        Mesh cube = this.Cube;
        DefaultEffect.Textured textured = new DefaultEffect.Textured();
        textured.AlphaIsEmissive = true;
        cube.Effect = (BaseEffect) textured;
      }
      else
      {
        Mesh cube = this.Cube;
        DefaultEffect.LitTextured litTextured = new DefaultEffect.LitTextured();
        litTextured.Specular = true;
        litTextured.Emissive = 0.5f;
        litTextured.AlphaIsEmissive = true;
        cube.Effect = (BaseEffect) litTextured;
      }
      ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Vector4> geometry = instance.Trile.Geometry;
      IndexedUserPrimitives<VertexPositionNormalTextureInstance> indexedUserPrimitives = new IndexedUserPrimitives<VertexPositionNormalTextureInstance>(geometry.Vertices, geometry.Indices, geometry.PrimitiveType);
      this.Cube.AddGroup().Geometry = (IIndexedPrimitiveCollection) indexedUserPrimitives;
      this.Trail.AddGroup().Geometry = (IIndexedPrimitiveCollection) (this.TrailGeometry = new IndexedUserPrimitives<FezVertexPositionColor>(this.TrailVertices = new FezVertexPositionColor[0], this.TrailIndices = new int[0], PrimitiveType.TriangleList));
      this.Instance = instance;
      this.lastPoint = instance.Center;
      this.DestinationMesh = destinationMesh;
      this.sideDirection = (RandomHelper.Probability(0.5) ? -1f : 1f) * this.CameraManager.Viewpoint.RightVector();
      this.Spline = new Vector3SplineInterpolation(TimeSpan.FromSeconds(3.0), new Vector3[10]);
      this.Spline.Start();
      this.AddSegment();
    }

    public void Dispose()
    {
      this.Cube.Dispose();
      this.Trail.Dispose();
    }

    private void AddSegment()
    {
      bool flag = this.TrailVertices.Length == 0;
      Array.Resize<FezVertexPositionColor>(ref this.TrailVertices, this.TrailVertices.Length + (flag ? 12 : 6));
      Array.Resize<int>(ref this.TrailIndices, this.TrailIndices.Length + 36);
      this.TrailGeometry.Vertices = this.TrailVertices;
      this.TrailGeometry.Indices = this.TrailIndices;
      int num = this.TrailVertices.Length - 12;
      for (int index1 = 0; index1 < 6; ++index1)
      {
        int index2 = index1 * 6 + this.TrailIndices.Length - 36;
        this.TrailIndices[index2] = index1 + num;
        this.TrailIndices[index2 + 1] = (index1 + 6) % 12 + num;
        this.TrailIndices[index2 + 2] = (index1 + 1) % 12 + num;
        this.TrailIndices[index2 + 3] = (index1 + 1) % 12 + num;
        this.TrailIndices[index2 + 4] = (index1 + 6) % 12 + num;
        this.TrailIndices[index2 + 5] = (index1 + 7) % 12 + num;
      }
      if (!flag)
        return;
      for (int index = 0; index < 6; ++index)
      {
        this.TrailVertices[index].Position = this.lastPoint;
        this.TrailVertices[index].Color = Color.Black;
      }
    }

    private void AlignLastSegment()
    {
      Vector3 current = this.Spline.Current;
      Vector3 vector3_1 = Vector3.Normalize(current - this.lastPoint);
      Vector3 vector2 = Vector3.Normalize(Vector3.Cross(FezMath.Slerp(Vector3.Up, Vector3.Forward, Math.Abs(vector3_1.Dot(Vector3.Up))), vector3_1));
      Vector3 vector3_2 = Vector3.Normalize(Vector3.Cross(vector3_1, vector2));
      Quaternion rotation = Quaternion.Inverse(Quaternion.CreateFromRotationMatrix(new Matrix(vector2.X, vector3_2.X, vector3_1.X, 0.0f, vector2.Y, vector3_2.Y, vector3_1.Y, 0.0f, vector2.Z, vector3_2.Z, vector3_1.Z, 0.0f, 0.0f, 0.0f, 0.0f, 1f)));
      int num1 = this.TrailVertices.Length - 6;
      for (int index = 0; index < 6; ++index)
      {
        float num2 = (float) ((double) index / 6.0 * 6.2831854820251465);
        this.TrailVertices[num1 + index].Position = Vector3.Transform(new Vector3((float) (Math.Sin((double) num2) * 0.5 / 2.0), (float) (Math.Cos((double) num2) * 0.5 / 2.0), 0.0f), rotation) + current;
      }
    }

    private void ColorSegments()
    {
      int num1 = this.TrailVertices.Length / 6;
      for (int index1 = 0; index1 < num1; ++index1)
      {
        for (int index2 = 0; index2 < 6; ++index2)
        {
          float num2 = Easing.EaseIn((double) Math.Max((float) (index1 - (num1 - 9)) / 10f, 0.0f), EasingType.Sine) * (float) Math.Pow(1.0 - (double) this.Spline.TotalStep, 0.5);
          this.TrailVertices[index1 * 6 + index2].Color = new Color(new Vector3(num2) * this.color);
        }
      }
    }

    public void Update(GameTime gameTime)
    {
      for (int index = 0; index < this.Spline.Points.Length; ++index)
      {
        float amount = Easing.EaseOut((double) index / (double) (this.Spline.Points.Length - 1), EasingType.Sine);
        this.Spline.Points[index] = Vector3.Lerp(this.Instance.Center, this.DestinationMesh.Position, amount);
        Vector3 vector3 = Vector3.Zero + this.sideDirection * 3.5f * (0.7f - (float) Math.Sin((double) amount * 6.2831854820251465 + 0.78539818525314331)) + Vector3.Up * 2f * (0.7f - (float) Math.Cos((double) amount * 6.2831854820251465 + 0.78539818525314331));
        if (index != 0 && index != this.Spline.Points.Length - 1)
          this.Spline.Points[index] += vector3;
      }
      this.Spline.Update(gameTime);
      if ((double) this.Spline.TotalStep - (double) this.lastStep > 0.025)
      {
        this.lastPoint = this.Spline.Current;
        this.lastStep = this.Spline.TotalStep;
        this.AddSegment();
      }
      this.AlignLastSegment();
      this.ColorSegments();
      this.rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, (float) gameTime.ElapsedGameTime.TotalSeconds) * this.rotation;
      this.Cube.Position = this.Spline.Current + Vector3.Transform(this.positionOffset, this.rotation) * this.Spline.TotalStep;
      this.Cube.Rotation = Quaternion.Slerp(Quaternion.Identity, this.rotation, this.Spline.TotalStep);
    }

    [ServiceDependency]
    public IGameCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public ILevelManager LevelManager { private get; set; }

    [ServiceDependency]
    public ILevelMaterializer LevelMaterializer { private get; set; }
  }
}
