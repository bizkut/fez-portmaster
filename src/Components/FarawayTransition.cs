// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FarawayTransition
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

public class FarawayTransition : DrawableGameComponent
{
  private const float TotalDuration = 6f;
  private const float FadeOutDuration = 0.75f;
  private const float CrossfadeInStart = 4.5f;
  private const float CrossfadeInDuration = 0.75f;
  private TimeSpan SinceStarted;
  private Volume StartVolume;
  private float OriginalRadius;
  private float DestinationRadius;
  private RenderTargetHandle SkyRt;
  private RenderTargetHandle LevelRt;
  private FarawayTransition.LevelFader Fader;
  private string NextLevel;

  public FarawayTransition(Game game)
    : base(game)
  {
    this.DrawOrder = 1;
    ITargetRenderingManager renderingManager = ServiceHelper.Get<ITargetRenderingManager>();
    this.SkyRt = renderingManager.TakeTarget();
    renderingManager.ScheduleHook(this.DrawOrder, this.SkyRt.Target);
  }

  public override void Initialize()
  {
    base.Initialize();
    this.PlayerManager.Hidden = true;
    this.PlayerManager.CanControl = false;
    this.GameState.FarawaySettings.InTransition = true;
    this.StartVolume = this.LevelManager.Volumes[this.PlayerManager.DoorVolume.Value];
    if (this.StartVolume.ActorSettings == null)
      this.StartVolume.ActorSettings = new VolumeActorSettings();
    float viewScale = this.GraphicsDevice.GetViewScale();
    float num = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
    this.OriginalRadius = this.CameraManager.Radius;
    this.GameState.FarawaySettings.DestinationRadius = this.DestinationRadius = (double) this.StartVolume.ActorSettings.DestinationRadius == 0.0 ? this.CameraManager.Radius : this.StartVolume.ActorSettings.DestinationRadius * viewScale * num;
    this.GameState.FarawaySettings.DestinationPixelsPerTrixel = this.StartVolume.ActorSettings.DestinationPixelsPerTrixel;
    this.GameState.FarawaySettings.SkyRt = this.SkyRt.Target;
    this.NextLevel = this.PlayerManager.NextLevel;
    this.LevelRt = this.TargetRenderer.TakeTarget();
    ServiceHelper.AddComponent((IGameComponent) (this.Fader = new FarawayTransition.LevelFader(this.Game, this.SkyRt, this.LevelRt)));
    this.TargetRenderer.ScheduleHook(this.Fader.DrawOrder, this.LevelRt.Target);
    if (this.StartVolume.ActorSettings.WaterLocked)
      LiquidHost.Instance.StartTransition();
    if (this.LevelManager.Rainy && RainHost.Instance != null)
      RainHost.Instance.StartTransition();
    if (this.StartVolume.ActorSettings.DestinationSong != this.LevelManager.SongName)
      this.SoundManager.PlayNewSong((string) null, 10f);
    foreach (SoundEmitter emitter in this.SoundManager.Emitters)
      emitter.FadeOutAndDie(1f, false);
    this.CMProvider.Global.Load<SoundEffect>("Sounds/Intro/ZoomToFarawayPlace").Emit().Persistent = true;
    this.DotManager.ForceDrawOrder(1001);
    this.DotManager.Burrow();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap)
      return;
    this.SinceStarted += gameTime.ElapsedGameTime;
    this.GameState.FarawaySettings.TransitionStep = FezMath.Saturate((float) this.SinceStarted.TotalSeconds / 6f);
    float transitionStep = this.GameState.FarawaySettings.TransitionStep;
    this.GameState.FarawaySettings.OriginFadeOutStep = FezMath.Saturate((float) this.SinceStarted.TotalSeconds / 0.75f);
    this.GameState.FarawaySettings.DestinationCrossfadeStep = FezMath.Saturate((float) ((this.SinceStarted.TotalSeconds - 4.5) / 0.75));
    if ((double) this.GameState.FarawaySettings.DestinationCrossfadeStep > 0.0 && !this.PlayerManager.CanControl && this.PlayerManager.Action != ActionType.ExitDoor)
    {
      this.LevelMaterializer.PrepareFullCull();
      this.PlayerManager.Action = ActionType.ExitDoor;
    }
    if ((double) this.GameState.FarawaySettings.DestinationCrossfadeStep > 0.5 && !this.PlayerManager.CanControl)
      this.PlayerManager.CanControl = true;
    if ((double) this.GameState.FarawaySettings.DestinationCrossfadeStep == 1.0 && this.StartVolume.ActorSettings.WaterLocked && LiquidHost.Instance.InTransition)
      LiquidHost.Instance.EndTransition();
    if ((double) this.GameState.FarawaySettings.OriginFadeOutStep != 1.0)
      this.CameraManager.Radius = MathHelper.Lerp(this.OriginalRadius, this.DestinationRadius / 4f, transitionStep);
    if (this.GameState.FarawaySettings.LoadingAllowed)
    {
      this.GameState.Loading = true;
      Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoLoad));
      worker.Finished += (Action) (() => this.ThreadPool.Return<bool>(worker));
      worker.Start(false);
      this.GameState.FarawaySettings.LoadingAllowed = false;
    }
    if ((double) transitionStep != 1.0)
      return;
    this.CameraManager.PixelsPerTrixel = this.GameState.FarawaySettings.DestinationPixelsPerTrixel;
    this.DotManager.RevertDrawOrder();
    this.Enabled = false;
    Waiters.Wait(0.25, (Action) (() => ServiceHelper.RemoveComponent<FarawayTransition>(this)));
  }

  private void DoLoad(bool dummy)
  {
    this.LevelManager.ChangeLevel(this.NextLevel);
    this.PlayerManager.ForceOverlapsDetermination();
    TrileInstance surface = this.PlayerManager.AxisCollision[VerticalDirection.Up].Surface;
    if (surface != null && surface.Trile.ActorSettings.Type == ActorType.UnlockedDoor && FezMath.OrientationFromPhi(surface.Trile.ActorSettings.Face.ToPhi() + surface.Phi) == this.CameraManager.Viewpoint.VisibleOrientation())
    {
      ++this.GameState.SaveData.ThisLevel.FilledConditions.UnlockedDoorCount;
      TrileEmplacement id = surface.Emplacement + Vector3.UnitY;
      TrileInstance instance = this.LevelManager.TrileInstanceAt(ref id);
      if (instance.Trile.ActorSettings.Type == ActorType.UnlockedDoor)
        ++this.GameState.SaveData.ThisLevel.FilledConditions.UnlockedDoorCount;
      this.LevelManager.ClearTrile(surface);
      this.LevelManager.ClearTrile(instance);
      this.GameState.SaveData.ThisLevel.InactiveTriles.Add(surface.Emplacement);
      surface.ActorSettings.Inactive = true;
    }
    this.PlayerManager.Hidden = false;
    this.CameraManager.Constrained = false;
    this.GameState.ScheduleLoadEnd = true;
  }

  public override void Draw(GameTime gameTime)
  {
    this.TargetRenderer.Resolve(this.SkyRt.Target, true);
    this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, ColorEx.TransparentBlack, 1f, 0);
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.GameState.FarawaySettings.Reset();
    this.TargetRenderer.ReturnTarget(this.SkyRt);
    this.TargetRenderer.ReturnTarget(this.LevelRt);
    this.SkyRt = this.LevelRt = (RenderTargetHandle) null;
    ServiceHelper.RemoveComponent<FarawayTransition.LevelFader>(this.Fader);
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { private get; set; }

  [ServiceDependency]
  public IDotManager DotManager { private get; set; }

  private class LevelFader : DrawableGameComponent
  {
    private readonly RenderTargetHandle SkyRt;
    private readonly RenderTargetHandle LevelRt;

    public LevelFader(Game game, RenderTargetHandle SkyRt, RenderTargetHandle LevelRt)
      : base(game)
    {
      this.DrawOrder = 1000;
      this.SkyRt = SkyRt;
      this.LevelRt = LevelRt;
    }

    public override void Draw(GameTime gameTime)
    {
      float alpha = FezMath.Saturate((double) this.GameState.FarawaySettings.OriginFadeOutStep < 1.0 ? 1f - this.GameState.FarawaySettings.OriginFadeOutStep : this.GameState.FarawaySettings.DestinationCrossfadeStep);
      this.TargetRenderer.Resolve(this.LevelRt.Target, true);
      this.GraphicsDevice.SetupViewport();
      this.GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer | ClearOptions.Stencil, Color.Black, 1f, 0);
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Opaque);
      this.TargetRenderer.DrawFullscreen((Texture) this.SkyRt.Target);
      this.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
      this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
      this.TargetRenderer.DrawFullscreen((Texture) this.LevelRt.Target, new Color(1f, 1f, 1f, alpha));
      this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    }

    [ServiceDependency]
    public ITargetRenderingManager TargetRenderer { get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { get; set; }
  }
}
