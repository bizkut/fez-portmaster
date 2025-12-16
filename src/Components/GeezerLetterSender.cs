// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GeezerLetterSender
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

internal class GeezerLetterSender : DrawableGameComponent
{
  private int? NpcId;
  private NpcInstance Npc;
  private bool Walking;
  private float SinceStarted;
  private BackgroundPlane Plane;
  private float SinceGotThere;
  private Vector3 OldPosition;
  private Vector3 OldDestinationOffset;
  private bool hooked;
  private bool geezerReset;
  private SoundEffect sLetterInsert;

  public GeezerLetterSender(Game game, int npcId)
    : base(game)
  {
    this.NpcId = new int?(npcId);
    this.DrawOrder = 99;
  }

  public GeezerLetterSender(Game game)
    : base(game)
  {
    this.NpcId = new int?();
    this.DrawOrder = 99;
  }

  public override void Initialize()
  {
    base.Initialize();
    DrawActionScheduler.Schedule((Action) (() =>
    {
      IGameLevelManager levelManager = this.LevelManager;
      BackgroundPlane backgroundPlane1 = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/CAPSULE"));
      backgroundPlane1.Rotation = this.CameraManager.Rotation;
      backgroundPlane1.Loop = false;
      BackgroundPlane backgroundPlane2 = backgroundPlane1;
      this.Plane = backgroundPlane1;
      BackgroundPlane plane = backgroundPlane2;
      levelManager.AddPlane(plane);
      if (this.NpcId.HasValue)
      {
        this.Npc = this.LevelManager.NonPlayerCharacters[this.NpcId.Value];
        this.OldPosition = this.Npc.Position;
        this.Npc.Position = new Vector3(487f / 16f, 49f, 10f);
        this.OldDestinationOffset = this.Npc.DestinationOffset;
        this.Npc.DestinationOffset = new Vector3(-63f / 16f, 0.0f, 0.0f);
        this.Npc.State.Scripted = true;
        this.Npc.State.LookingDirection = HorizontalDirection.Right;
        this.Npc.State.WalkStep = 0.0f;
        this.Npc.State.CurrentAction = NpcAction.Idle;
        this.Npc.State.UpdateAction();
        this.Npc.State.SyncTextureMatrix();
        this.Npc.Group.Position = this.LevelManager.NonPlayerCharacters[this.NpcId.Value].Position;
        this.CameraManager.Constrained = true;
        this.CameraManager.Center = new Vector3(32.5f, 50.5f, 16.5f);
        this.CameraManager.SnapInterpolation();
        this.Plane.Position = this.Npc.Group.Position + new Vector3((float) (this.Npc.State.LookingDirection.Sign() * 4) / 16f, 0.375f, 0.0f);
        this.sLetterInsert = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/LetterTubeInsert");
      }
      else
      {
        this.Plane.Position = new Vector3(20.5f, 20.75f, 23.5f);
        this.Enabled = false;
        this.GomezService.ReadMail += new Action(this.Destroy);
      }
    }));
    this.LevelManager.LevelChanged += new Action(this.TryDestroy);
  }

  private void TryDestroy() => ServiceHelper.RemoveComponent<GeezerLetterSender>(this);

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.GomezService.ReadMail -= new Action(this.Destroy);
    this.LevelManager.LevelChanged -= new Action(this.TryDestroy);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.TimePaused || this.GameState.Loading || this.geezerReset)
      return;
    this.CameraManager.Constrained = false;
    double sinceStarted = (double) this.SinceStarted;
    TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
    double totalSeconds1 = elapsedGameTime.TotalSeconds;
    this.SinceStarted = (float) (sinceStarted + totalSeconds1);
    if ((double) this.SinceStarted > 3.0 && !this.Walking)
    {
      this.Walking = true;
      this.Npc.State.LookingDirection = HorizontalDirection.Left;
      this.Npc.State.CurrentAction = NpcAction.Walk;
      this.Npc.State.UpdateAction();
      this.Npc.State.SyncTextureMatrix();
    }
    if (this.Npc.State.CurrentAction == NpcAction.Walk && (double) this.Npc.State.WalkStep == 1.0)
    {
      this.Npc.State.CurrentAction = NpcAction.Idle;
      this.Npc.State.UpdateAction();
      this.Npc.State.SyncTextureMatrix();
    }
    if ((double) this.Npc.State.WalkStep == 1.0)
    {
      double sinceGotThere = (double) this.SinceGotThere;
      elapsedGameTime = gameTime.ElapsedGameTime;
      double totalSeconds2 = elapsedGameTime.TotalSeconds;
      this.SinceGotThere = (float) (sinceGotThere + totalSeconds2);
      if ((double) this.SinceGotThere < 0.5)
      {
        if (this.sLetterInsert != null)
        {
          this.sLetterInsert.Emit();
          this.sLetterInsert = (SoundEffect) null;
        }
        this.Plane.Position = this.Npc.Group.Position + new Vector3((float) (this.Npc.State.LookingDirection.Sign() * 4) / 16f, 0.375f, 0.0f) + new Vector3(-Easing.EaseIn((double) this.SinceGotThere / 0.5, EasingType.Quadratic), 0.375f, 0.0f);
      }
      if ((double) this.SinceGotThere > 12.5 && (double) this.SinceGotThere < 14.5)
        this.Plane.Position = new Vector3(20.5f, 20.75f + Easing.EaseOut((double) FezMath.Saturate((float) ((13.25 - (double) this.SinceGotThere) * 2.0)), EasingType.Cubic), 23.5f);
      else if ((double) this.SinceGotThere > 14.5 && this.NpcId.HasValue)
      {
        this.Npc.Position = this.OldPosition;
        this.Npc.DestinationOffset = this.OldDestinationOffset;
        this.Npc.State.CurrentAction = NpcAction.Walk;
        this.Npc.State.UpdateAction();
        this.Npc.State.WalkStep = 0.25f;
        this.Npc.State.Scripted = false;
        this.geezerReset = true;
      }
      if (this.hooked)
        return;
      this.GomezService.ReadMail += new Action(this.Destroy);
      this.hooked = true;
    }
    else
      this.Plane.Position = this.Npc.Group.Position + new Vector3((float) (this.Npc.State.LookingDirection.Sign() * 4) / 16f, 0.375f, 0.0f);
  }

  private void Destroy()
  {
    this.LevelManager.RemovePlane(this.Plane);
    ServiceHelper.RemoveComponent<GeezerLetterSender>(this);
  }

  [ServiceDependency]
  public IVolumeService VolumeService { get; set; }

  [ServiceDependency]
  public IGomezService GomezService { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }
}
