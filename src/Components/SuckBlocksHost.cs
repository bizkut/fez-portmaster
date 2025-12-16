// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SuckBlocksHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class SuckBlocksHost(Game game) : GameComponent(game)
{
  private readonly List<SuckBlocksHost.SuckBlockState> TrackedSuckBlocks = new List<SuckBlocksHost.SuckBlockState>();
  private readonly List<Volume> HostingVolumes = new List<Volume>();
  private SoundEmitter eCratePush;
  private SoundEmitter eSuck;
  private SoundEffect sDenied;
  private SoundEffect sSuck;
  private SoundEffect[] sAccept;
  private List<BackgroundPlane> highlightPlanes;
  private readonly Ray[] cornerRays = new Ray[4];

  public override void Initialize()
  {
    this.LevelManager.LevelChanged += new Action(this.InitSuckBlocks);
    if (this.LevelManager.Name != null)
      this.InitSuckBlocks();
    this.sAccept = new SoundEffect[4]
    {
      this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/AcceptSuckBlock1"),
      this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/AcceptSuckBlock2"),
      this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/AcceptSuckBlock3"),
      this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/AcceptSuckBlock4")
    };
    this.sDenied = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/Denied");
    this.sSuck = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/SuckBlockSuck");
  }

  private void InitSuckBlocks()
  {
    this.HostingVolumes.Clear();
    this.TrackedSuckBlocks.Clear();
    this.highlightPlanes = (List<BackgroundPlane>) null;
    this.eCratePush = this.eSuck = (SoundEmitter) null;
    foreach (TrileGroup group in (IEnumerable<TrileGroup>) this.LevelManager.Groups.Values)
    {
      if (group.ActorType == ActorType.SuckBlock)
      {
        TrileInstance instance = group.Triles.First<TrileInstance>();
        int? hostVolume = instance.ActorSettings.HostVolume;
        if (hostVolume.HasValue)
        {
          this.TrackedSuckBlocks.Add(new SuckBlocksHost.SuckBlockState(instance, group));
          SuckBlocksHost.EnableTrile(instance);
          List<Volume> hostingVolumes = this.HostingVolumes;
          IDictionary<int, Volume> volumes = this.LevelManager.Volumes;
          hostVolume = instance.ActorSettings.HostVolume;
          int key = hostVolume.Value;
          Volume volume = volumes[key];
          hostingVolumes.Add(volume);
        }
      }
    }
    if (this.TrackedSuckBlocks.Count <= 0)
      return;
    this.eCratePush = this.CMProvider.Global.Load<SoundEffect>("Sounds/Gomez/PushPickup").EmitAt(Vector3.Zero, true, true);
    this.highlightPlanes = new List<BackgroundPlane>();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || !this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.Loading || this.TrackedSuckBlocks.Count == 0)
      return;
    FaceOrientation visibleOrientation = this.CameraManager.VisibleOrientation;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ForwardVector();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.DepthMask();
    Vector3 vector3_3 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    Vector3 vector3_4 = vector3_3 / 2f;
    bool flag1 = false;
    foreach (SuckBlocksHost.SuckBlockState trackedSuckBlock in this.TrackedSuckBlocks)
    {
      TrileInstance instance = trackedSuckBlock.Instance;
      if (this.PlayerManager.HeldInstance != instance)
      {
        int num1 = instance.ActorSettings.HostVolume.Value;
        Vector3 vector3_5 = instance.Center * (Vector3.One - vector3_2) + this.CameraManager.Position * vector3_2;
        Ray[] cornerRays1 = this.cornerRays;
        Ray ray1 = new Ray();
        ray1.Position = vector3_5 + vector3_4 * new Vector3(1f, 0.499f, 1f);
        ray1.Direction = vector3_1;
        Ray ray2 = ray1;
        cornerRays1[0] = ray2;
        Ray[] cornerRays2 = this.cornerRays;
        ray1 = new Ray();
        ray1.Position = vector3_5 + vector3_4 * new Vector3(1f, -1f, 1f);
        ray1.Direction = vector3_1;
        Ray ray3 = ray1;
        cornerRays2[1] = ray3;
        Ray[] cornerRays3 = this.cornerRays;
        ray1 = new Ray();
        ray1.Position = vector3_5 + vector3_4 * new Vector3(-1f, 0.499f, -1f);
        ray1.Direction = vector3_1;
        Ray ray4 = ray1;
        cornerRays3[2] = ray4;
        Ray[] cornerRays4 = this.cornerRays;
        ray1 = new Ray();
        ray1.Position = vector3_5 + vector3_4 * new Vector3(-1f, -1f, -1f);
        ray1.Direction = vector3_1;
        Ray ray5 = ray1;
        cornerRays4[3] = ray5;
        trackedSuckBlock.Update(gameTime.ElapsedGameTime);
        this.eCratePush.Position = instance.Center;
        bool flag2 = false;
        foreach (Volume hostingVolume in this.HostingVolumes)
        {
          if (hostingVolume.Orientations.Contains(visibleOrientation))
          {
            bool flag3 = false;
            foreach (Ray cornerRay in this.cornerRays)
              flag3 |= hostingVolume.BoundingBox.Intersects(cornerRay).HasValue;
            if (flag3)
            {
              flag2 = true;
              if (trackedSuckBlock.Action == SuckBlocksHost.SuckBlockAction.Sucking && (this.eSuck == null || this.eSuck.Dead))
                this.eSuck = this.sSuck.EmitAt(instance.Center, true);
              flag1 = ((flag1 ? 1 : 0) | (trackedSuckBlock.Action == SuckBlocksHost.SuckBlockAction.Sucking ? 1 : (trackedSuckBlock.Action == SuckBlocksHost.SuckBlockAction.Processing ? 1 : 0))) != 0;
              Vector3 vector3_6 = (hostingVolume.BoundingBox.Min + hostingVolume.BoundingBox.Max) / 2f;
              Vector3 vector3_7 = (vector3_6 - instance.Center) * vector3_3;
              float num2 = vector3_7.Length();
              if ((double) num2 < 0.0099999997764825821)
              {
                if (trackedSuckBlock.Action == SuckBlocksHost.SuckBlockAction.Sucking)
                {
                  trackedSuckBlock.Action = SuckBlocksHost.SuckBlockAction.Processing;
                  this.PlayerManager.CanRotate = false;
                  this.eCratePush.VolumeFactor = 0.5f;
                  this.eCratePush.Cue.Pitch = -0.4f;
                }
                if (trackedSuckBlock.Action == SuckBlocksHost.SuckBlockAction.Processing)
                {
                  Vector3 vector3_8 = (hostingVolume.BoundingBox.Max - hostingVolume.BoundingBox.Min) / 2f;
                  Vector3 vector3_9 = hostingVolume.BoundingBox.Min * vector3_3 + vector3_6 * vector3_2 + vector3_8 * vector3_1 - vector3_1 * 0.5f - vector3_2 * 0.5f;
                  Vector3 vector3_10 = vector3_9 - vector3_1;
                  instance.Position = Vector3.Lerp(vector3_10, vector3_9, (float) trackedSuckBlock.SinceActionChanged.Ticks / (float) SuckBlocksHost.SuckBlockState.ProcessingTime.Ticks);
                  this.LevelManager.UpdateInstance(instance);
                  if (trackedSuckBlock.SinceActionChanged > SuckBlocksHost.SuckBlockState.ProcessingTime)
                  {
                    this.PlayerManager.CanRotate = true;
                    if (hostingVolume.Id == num1)
                    {
                      SuckBlocksHost.DisableTrile(instance);
                      trackedSuckBlock.Action = SuckBlocksHost.SuckBlockAction.Accepted;
                      if (this.eCratePush.Cue.State != SoundState.Paused)
                        this.eCratePush.Cue.Pause();
                      this.SuckBlockService.OnSuck(trackedSuckBlock.Group.Id);
                      this.sAccept[4 - this.TrackedSuckBlocks.Count].Emit();
                      BackgroundPlane plane = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/suck_blocks/four_highlight_" + instance.Trile.CubemapPath.Substring(instance.Trile.CubemapPath.Length - 1).ToLower(CultureInfo.InvariantCulture)))
                      {
                        Position = instance.Center + visibleOrientation.AsVector() * (17f / 32f),
                        Rotation = this.CameraManager.Rotation,
                        Doublesided = true,
                        Fullbright = true,
                        Opacity = 0.0f
                      };
                      List<BackgroundPlane> localPlanes = this.highlightPlanes;
                      this.highlightPlanes.Add(plane);
                      this.LevelManager.AddPlane(plane);
                      Waiters.Interpolate(1.0, (Action<float>) (s => plane.Opacity = s));
                      if (this.TrackedSuckBlocks.Count == 1)
                      {
                        Waiters.Wait(2.0, (Action) (() => Waiters.Interpolate(1.0, (Action<float>) (s =>
                        {
                          foreach (BackgroundPlane backgroundPlane in localPlanes)
                            backgroundPlane.Opacity = 1f - s;
                        }), (Action) (() => this.eSuck = (SoundEmitter) null))));
                        this.eSuck.FadeOutAndDie(1f);
                      }
                    }
                    else
                      trackedSuckBlock.Action = SuckBlocksHost.SuckBlockAction.Rejected;
                  }
                }
                if (trackedSuckBlock.Action == SuckBlocksHost.SuckBlockAction.Rejected && instance.PhysicsState.Velocity.XZ() == Vector2.Zero)
                {
                  int num3 = RandomHelper.Probability(0.5) ? -1 : 1;
                  Vector3 vector3_11 = new Vector3((float) num3, 0.75f, (float) num3) * vector3_3;
                  ServiceHelper.AddComponent((IGameComponent) new CamShake(this.Game)
                  {
                    Distance = 0.1f,
                    Duration = TimeSpan.FromSeconds(0.25)
                  });
                  this.sDenied.Emit();
                  if (this.eCratePush.Cue.State != SoundState.Paused)
                    this.eCratePush.Cue.Pause();
                  instance.PhysicsState.Velocity += 6f * vector3_11 * (float) gameTime.ElapsedGameTime.TotalSeconds;
                }
              }
              else if (trackedSuckBlock.Action != SuckBlocksHost.SuckBlockAction.Rejected)
              {
                if (instance.PhysicsState.Grounded && this.eCratePush.Cue.State != SoundState.Playing)
                {
                  this.eCratePush.Cue.Pitch = 0.0f;
                  this.eCratePush.Cue.Resume();
                }
                else if (!instance.PhysicsState.Grounded && this.eCratePush.Cue.State != SoundState.Paused)
                  this.eCratePush.Cue.Pause();
                if (this.eCratePush.Cue.State == SoundState.Playing)
                  this.eCratePush.VolumeFactor = FezMath.Saturate(Math.Abs(instance.PhysicsState.Velocity.Dot(FezMath.XZMask) / 0.1f));
                trackedSuckBlock.Action = SuckBlocksHost.SuckBlockAction.Sucking;
                instance.PhysicsState.Velocity += 0.25f * (vector3_7 / num2) * (float) gameTime.ElapsedGameTime.TotalSeconds;
              }
            }
          }
        }
        if (!flag2)
          trackedSuckBlock.Action = SuckBlocksHost.SuckBlockAction.Idle;
      }
    }
    if (!flag1 && this.eSuck != null && !this.eSuck.Dead)
    {
      this.eSuck.FadeOutAndDie(0.1f);
      this.eSuck = (SoundEmitter) null;
    }
    for (int index = 0; index < this.TrackedSuckBlocks.Count; ++index)
    {
      if (this.TrackedSuckBlocks[index].Action == SuckBlocksHost.SuckBlockAction.Accepted)
      {
        this.TrackedSuckBlocks.RemoveAt(index);
        --index;
      }
    }
  }

  private static void DisableTrile(TrileInstance instance)
  {
    Trile trile = instance.Trile;
    trile.ActorSettings.Type = ActorType.None;
    trile.Faces[FaceOrientation.Left] = trile.Faces[FaceOrientation.Right] = trile.Faces[FaceOrientation.Back] = trile.Faces[FaceOrientation.Front] = CollisionType.None;
  }

  private static void EnableTrile(TrileInstance instance)
  {
    Trile trile = instance.Trile;
    trile.ActorSettings.Type = ActorType.SinkPickup;
    trile.Faces[FaceOrientation.Left] = trile.Faces[FaceOrientation.Right] = trile.Faces[FaceOrientation.Back] = trile.Faces[FaceOrientation.Front] = CollisionType.AllSides;
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ISuckBlockService SuckBlockService { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private class SuckBlockState
  {
    public static readonly TimeSpan ProcessingTime = TimeSpan.FromSeconds(0.5);
    public readonly TrileInstance Instance;
    public readonly TrileGroup Group;
    private SuckBlocksHost.SuckBlockAction action;

    public TimeSpan SinceActionChanged { get; private set; }

    public SuckBlockState(TrileInstance instance, TrileGroup group)
    {
      this.Instance = instance;
      this.Group = group;
    }

    public SuckBlocksHost.SuckBlockAction Action
    {
      get => this.action;
      set
      {
        if (this.action != value)
          this.SinceActionChanged = TimeSpan.Zero;
        this.action = value;
      }
    }

    public void Update(TimeSpan elapsed) => this.SinceActionChanged += elapsed;
  }

  private enum SuckBlockAction
  {
    Idle,
    Processing,
    Sucking,
    Rejected,
    Accepted,
  }
}
