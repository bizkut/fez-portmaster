// Decompiled with JetBrains decompiler
// Type: FezGame.Components.WatchersHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
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

internal class WatchersHost(Game game) : DrawableGameComponent(game)
{
  private const float WatchRange = 8f;
  private const float SpotDelay = 1f;
  private const float CrushSpeed = 15f;
  private const float WithdrawSpeed = 2f;
  private const float Acceleration = 0.025f;
  private const float CooldownTime = 0.5f;
  private const float CrushWaitTime = 1.5f;
  private Dictionary<TrileInstance, WatchersHost.WatcherState> watchers;
  private SoundEffect seeSound;
  private SoundEffect moveSound;
  private SoundEffect collideSound;
  private SoundEffect withdrawSound;
  private readonly List<Vector3> lastCrushDirections = new List<Vector3>();

  public override void Initialize()
  {
    base.Initialize();
    this.UpdateOrder = -2;
    this.DrawOrder = 6;
    this.LevelManager.LevelChanged += new Action(this.InitializeWatchers);
    this.InitializeWatchers();
    this.LightingPostProcess.DrawGeometryLights += new Action<GameTime>(this.PreDraw);
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.seeSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Zu/WatcherSee");
    this.moveSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Zu/WatcherMove");
    this.collideSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Zu/WatcherCollide");
    this.withdrawSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Zu/WatcherWithdraw");
  }

  private void InitializeWatchers()
  {
    this.watchers = new Dictionary<TrileInstance, WatchersHost.WatcherState>();
    if (this.LevelManager.TrileSet == null)
      return;
    foreach (TrileInstance trileInstance in this.LevelManager.TrileSet.Triles.Values.Where<Trile>((Func<Trile, bool>) (t => t.ActorSettings.Type == ActorType.Watcher)).SelectMany<Trile, TrileInstance>((Func<Trile, IEnumerable<TrileInstance>>) (t => (IEnumerable<TrileInstance>) t.Instances)))
    {
      trileInstance.Trile.Size = new Vector3(31f / 32f);
      trileInstance.PhysicsState = new InstancePhysicsState(trileInstance);
      this.watchers.Add(trileInstance, new WatchersHost.WatcherState()
      {
        Eyes = new Mesh(),
        OriginalCenter = trileInstance.Center,
        CrashAttenuation = 1f
      });
      this.watchers[trileInstance].Eyes.AddColoredBox(Vector3.One / 16f, Vector3.Zero, new Color((int) byte.MaxValue, (int) sbyte.MaxValue, 0), false);
      this.watchers[trileInstance].Eyes.AddColoredBox(Vector3.One / 16f, Vector3.Zero, new Color((int) byte.MaxValue, (int) sbyte.MaxValue, 0), false);
      this.watchers[trileInstance].Eyes.AddColoredBox(Vector3.One / 16f, Vector3.Zero, new Color((int) byte.MaxValue, (int) sbyte.MaxValue, 0), false);
      this.watchers[trileInstance].Eyes.AddColoredBox(Vector3.One / 16f, Vector3.Zero, new Color((int) byte.MaxValue, (int) sbyte.MaxValue, 0), false);
    }
    DrawActionScheduler.Schedule((Action) (() =>
    {
      foreach (WatchersHost.WatcherState watcherState in this.watchers.Values)
      {
        Mesh eyes = watcherState.Eyes;
        eyes.Effect = (BaseEffect) new DefaultEffect.VertexColored()
        {
          Fullbright = true
        };
      }
    }));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.CameraManager.Viewpoint == Viewpoint.Perspective || this.GameState.InMap || this.GameState.Paused || this.GameState.Loading || this.watchers.Count == 0)
      return;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.RightVector();
    Vector3 vector3_2 = vector3_1.Abs();
    Vector3 vector3_3 = this.CameraManager.Viewpoint.ForwardVector();
    foreach (TrileInstance key in this.watchers.Keys)
    {
      WatchersHost.WatcherState watcher = this.watchers[key];
      Vector3 vector3_4 = key.PhysicsState.Center + vector3_2 * -5f / 16f + Vector3.UnitY * -2f / 16f - 0.5f * vector3_3;
      watcher.Eyes.Groups[0].Position = vector3_4 + watcher.EyeOffset;
      watcher.Eyes.Groups[1].Position = vector3_4 + vector3_2 * 9f / 16f + watcher.EyeOffset;
      watcher.Eyes.Groups[0].Enabled = true;
      watcher.Eyes.Groups[1].Enabled = true;
    }
    if (!this.CameraManager.ActionRunning || !this.CameraManager.ViewTransitionReached)
      return;
    Vector3 center1 = this.PlayerManager.Center;
    BoundingBox box = FezMath.Enclose(center1 - this.PlayerManager.Size / 2f, center1 + this.PlayerManager.Size / 2f);
    Vector3 vector3_5 = vector3_1 * 8f;
    Vector3 vector3_6 = vector3_3 * this.LevelManager.Size;
    Vector3 vector3_7 = Vector3.Up * 8f;
    this.lastCrushDirections.Clear();
    bool flag1 = false;
    foreach (TrileInstance key in this.watchers.Keys)
    {
      WatchersHost.WatcherState watcher = this.watchers[key];
      Vector3 vector1 = (center1 - key.Position).Sign() * vector3_2;
      Vector3 vector3_8 = (center1 - key.Position).Sign() * Vector3.UnitY;
      BoundingBox boundingBox1 = (double) Vector3.Dot(vector1, vector3_1) > 0.0 ? FezMath.Enclose(key.Position + Vector3.UnitY * 0.05f - vector3_6, key.Position + vector3_5 + vector3_6 + new Vector3(0.9f)) : FezMath.Enclose(key.Position + Vector3.UnitY * 0.05f - vector3_6 - vector3_5, key.Position + vector3_6 + new Vector3(0.9f));
      BoundingBox boundingBox2 = FezMath.Enclose(key.Position + Vector3.UnitY * 0.05f - vector3_7 - vector3_6, key.Position + vector3_7 + new Vector3(0.9f) + vector3_6);
      switch (watcher.Action)
      {
        case WatchersHost.WatcherAction.Idle:
          bool flag2 = boundingBox1.Intersects(box);
          bool flag3 = boundingBox2.Intersects(box);
          watcher.EyeOffset = !flag2 ? (!flag3 ? Vector3.Lerp(watcher.EyeOffset, Vector3.Zero, 0.1f) : Vector3.Lerp(watcher.EyeOffset, vector3_8 * 1f / 16f, 0.25f)) : Vector3.Lerp(watcher.EyeOffset, vector1 * 1f / 16f, 0.25f);
          watcher.CrushDirection = flag2 ? vector1 : (flag3 ? vector3_8 : Vector3.Zero);
          watcher.Eyes.Material.Opacity = 1f;
          WatchersHost.WatcherState watcherState;
          if (this.LevelManager.NearestTrile(key.Position + new Vector3(0.5f)).Deep == key && flag2 | flag3 && !FezMath.In<ActionType>(this.PlayerManager.Action, ActionType.GrabCornerLedge, ActionType.Suffering, ActionType.Dying, (IEqualityComparer<ActionType>) ActionTypeComparer.Default) && (watcherState = this.HasPair(key)) != null)
          {
            watcher.Action = WatchersHost.WatcherAction.Spotted;
            watcherState.StartTime = watcher.StartTime = gameTime.TotalGameTime;
            if (!watcher.SkipNextSound)
            {
              this.seeSound.EmitAt(key.Center);
              watcherState.SkipNextSound = true;
              break;
            }
            break;
          }
          break;
        case WatchersHost.WatcherAction.Spotted:
          watcher.EyeOffset = Vector3.Lerp(watcher.EyeOffset, watcher.CrushDirection * 1f / 16f, 0.25f);
          if ((gameTime.TotalGameTime - watcher.StartTime).TotalSeconds > 1.0)
          {
            watcher.Action = WatchersHost.WatcherAction.Crushing;
            watcher.StartTime = gameTime.TotalGameTime;
            key.PhysicsState.Velocity = watcher.OriginalCenter - key.Center;
            this.PhysicsManager.Update((ISimplePhysicsEntity) key.PhysicsState, true, false);
            key.PhysicsState.UpdateInstance();
            this.LevelManager.UpdateInstance(key);
            watcher.MoveEmitter = watcher.SkipNextSound ? (SoundEmitter) null : this.moveSound.EmitAt(key.Center);
            break;
          }
          Vector3 vector3_9 = watcher.CrushDirection * RandomHelper.Unit() * 0.5f / 16f;
          key.PhysicsState.Sticky = true;
          key.PhysicsState.Velocity = watcher.OriginalCenter + vector3_9 - key.Center;
          this.PhysicsManager.Update((ISimplePhysicsEntity) key.PhysicsState, true, false);
          key.PhysicsState.UpdateInstance();
          this.LevelManager.UpdateInstance(key);
          break;
        case WatchersHost.WatcherAction.Crushing:
          if (key.PhysicsState.Sticky)
          {
            key.PhysicsState.Sticky = false;
            key.PhysicsState.Velocity = Vector3.Zero;
          }
          watcher.EyeOffset = watcher.CrushDirection * 1f / 16f;
          Vector3 vector3_10 = watcher.CrushDirection * (float) gameTime.ElapsedGameTime.TotalSeconds * 15f;
          Vector3 vector3_11 = Vector3.Lerp(key.PhysicsState.Velocity, vector3_10, 0.025f);
          key.PhysicsState.Velocity = vector3_11 * watcher.CrashAttenuation;
          if (this.CameraManager.Viewpoint.VisibleAxis() != FezMath.OrientationFromDirection(watcher.CrushDirection).AsAxis())
            this.PhysicsManager.Update((ISimplePhysicsEntity) key.PhysicsState, false, false);
          Vector3 vector3_12 = vector3_11 * watcher.CrashAttenuation - key.PhysicsState.Velocity;
          if (watcher.MoveEmitter != null)
            watcher.MoveEmitter.Position = key.Center;
          key.PhysicsState.UpdateInstance();
          this.LevelManager.UpdateInstance(key);
          this.PlayerManager.ForceOverlapsDetermination();
          bool flag4 = this.PlayerManager.HeldInstance == key || this.PlayerManager.WallCollision.FarHigh.Destination == key || this.PlayerManager.WallCollision.NearLow.Destination == key || this.PlayerManager.Ground.NearLow == key || this.PlayerManager.Ground.FarHigh == key;
          if (!flag4)
          {
            foreach (PointCollision pointCollision in this.PlayerManager.CornerCollision)
            {
              if (pointCollision.Instances.Deep == key)
              {
                flag4 = true;
                break;
              }
            }
          }
          if (flag1 & flag4 && this.lastCrushDirections.Contains(-watcher.CrushDirection))
          {
            this.PlayerManager.Position = key.Center + Vector3.One / 2f * watcher.CrushDirection + -this.CameraManager.Viewpoint.SideMask() * watcher.CrushDirection.Abs() * 1.5f / 16f;
            this.PlayerManager.Velocity = Vector3.Zero;
            this.PlayerManager.Action = (double) watcher.CrushDirection.Y == 0.0 ? ActionType.CrushHorizontal : ActionType.CrushVertical;
            watcher.CrashAttenuation = this.PlayerManager.Action == ActionType.CrushVertical ? 0.5f : 0.75f;
          }
          flag1 |= flag4;
          if (flag4 && this.PlayerManager.Action != ActionType.CrushHorizontal && this.PlayerManager.Action != ActionType.CrushVertical)
          {
            this.lastCrushDirections.Add(watcher.CrushDirection);
            if ((double) watcher.CrushDirection.Y == 0.0)
              this.PlayerManager.Position += key.PhysicsState.Velocity;
          }
          if ((double) vector3_12.LengthSquared() > 4.9999998736893758E-05 || (double) Math.Abs(Vector3.Dot(key.Center - watcher.OriginalCenter, watcher.CrushDirection.Abs())) >= 8.0)
          {
            if (watcher.MoveEmitter != null && !watcher.MoveEmitter.Dead)
              watcher.MoveEmitter.Cue.Stop();
            watcher.MoveEmitter = (SoundEmitter) null;
            if (!watcher.SkipNextSound)
              this.collideSound.EmitAt(key.Center);
            watcher.Action = WatchersHost.WatcherAction.Wait;
            key.PhysicsState.Velocity = Vector3.Zero;
            watcher.StartTime = TimeSpan.Zero;
            watcher.CrashAttenuation = 1f;
            break;
          }
          break;
        case WatchersHost.WatcherAction.Wait:
          watcher.StartTime += gameTime.ElapsedGameTime;
          if (watcher.StartTime.TotalSeconds > 1.5)
          {
            watcher.Action = WatchersHost.WatcherAction.Withdrawing;
            watcher.StartTime = gameTime.TotalGameTime;
            watcher.WithdrawEmitter = watcher.SkipNextSound ? (SoundEmitter) null : this.withdrawSound.EmitAt(key.Center, true);
            break;
          }
          break;
        case WatchersHost.WatcherAction.Withdrawing:
          watcher.EyeOffset = Vector3.Lerp(watcher.EyeOffset, -watcher.CrushDirection * 0.5f / 16f, 0.05f);
          Vector3 vector3_13 = -watcher.CrushDirection * (float) gameTime.ElapsedGameTime.TotalSeconds * 2f;
          key.PhysicsState.Velocity = Vector3.Lerp(key.PhysicsState.Velocity, vector3_13, 0.025f);
          if (watcher.WithdrawEmitter != null)
            watcher.WithdrawEmitter.VolumeFactor = 0.0f;
          bool flag5 = false;
          if (this.CameraManager.Viewpoint.DepthMask() == FezMath.OrientationFromDirection(watcher.CrushDirection).AsAxis().GetMask())
            flag5 = true;
          if (watcher.WithdrawEmitter != null)
            watcher.WithdrawEmitter.VolumeFactor = 1f;
          Vector3 center2 = key.PhysicsState.Center;
          Vector3 velocity = key.PhysicsState.Velocity;
          this.PhysicsManager.Update((ISimplePhysicsEntity) key.PhysicsState, true, false);
          key.PhysicsState.Center = center2 + velocity;
          if (watcher.WithdrawEmitter != null)
            watcher.WithdrawEmitter.Position = key.Center;
          if ((flag5 ? ((double) Math.Abs(Vector3.Dot(key.Center - watcher.OriginalCenter, vector3_1 + Vector3.Up)) <= 1.0 / 32.0 ? 1 : 0) : ((double) Vector3.Dot(key.Center - watcher.OriginalCenter, watcher.CrushDirection) <= 1.0 / 1000.0 ? 1 : 0)) != 0)
          {
            if (watcher.WithdrawEmitter != null)
            {
              watcher.WithdrawEmitter.FadeOutAndDie(0.1f);
              watcher.WithdrawEmitter = (SoundEmitter) null;
            }
            watcher.SkipNextSound = false;
            watcher.Action = WatchersHost.WatcherAction.Cooldown;
            watcher.CrushDirection = Vector3.Zero;
            watcher.StartTime = TimeSpan.Zero;
          }
          key.PhysicsState.UpdateInstance();
          this.LevelManager.UpdateInstance(key);
          break;
        case WatchersHost.WatcherAction.Cooldown:
          key.PhysicsState.Velocity = watcher.OriginalCenter - key.Center;
          this.PhysicsManager.Update((ISimplePhysicsEntity) key.PhysicsState, true, false);
          key.PhysicsState.UpdateInstance();
          this.LevelManager.UpdateInstance(key);
          watcher.EyeOffset = Vector3.Lerp(watcher.EyeOffset, Vector3.Zero, 0.05f);
          watcher.Eyes.Material.Opacity = 0.5f;
          watcher.StartTime += gameTime.ElapsedGameTime;
          if (watcher.StartTime.TotalSeconds > 0.5)
          {
            key.PhysicsState.Velocity = Vector3.Zero;
            watcher.Action = WatchersHost.WatcherAction.Idle;
            break;
          }
          break;
      }
      Vector3 vector3_14 = key.PhysicsState.Center + vector3_2 * -5f / 16f + Vector3.UnitY * -2f / 16f - 0.5f * vector3_3;
      watcher.Eyes.Groups[0].Position = vector3_14 + watcher.EyeOffset;
      watcher.Eyes.Groups[1].Position = vector3_14 + vector3_2 * 9f / 16f + watcher.EyeOffset;
      watcher.Eyes.Groups[2].Position = watcher.Eyes.Groups[0].Position;
      watcher.Eyes.Groups[3].Position = watcher.Eyes.Groups[1].Position;
      watcher.Eyes.Groups[0].Enabled = false;
      watcher.Eyes.Groups[1].Enabled = false;
    }
  }

  private WatchersHost.WatcherState HasPair(TrileInstance watcher)
  {
    WatchersHost.WatcherState watcher1 = this.watchers[watcher];
    Vector3 b = this.CameraManager.Viewpoint.ScreenSpaceMask();
    foreach (TrileInstance key in this.watchers.Keys)
    {
      if (key != watcher)
      {
        WatchersHost.WatcherState watcher2 = this.watchers[key];
        if (watcher1.CrushDirection == -watcher2.CrushDirection && watcher2.Action != WatchersHost.WatcherAction.Cooldown && watcher2.Action != WatchersHost.WatcherAction.Withdrawing && watcher2.Action != WatchersHost.WatcherAction.Crushing && (double) Math.Abs((watcher1.OriginalCenter - watcher2.OriginalCenter).Dot(b)) > 2.0 && (double) Math.Abs((watcher.Center - key.Center).Dot(b)) > 2.0 && this.LevelManager.NearestTrile(key.Position + new Vector3(0.5f)).Deep == key)
          return watcher2;
      }
    }
    return (WatchersHost.WatcherState) null;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.watchers.Count == 0)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    foreach (WatchersHost.WatcherState watcherState in this.watchers.Values)
      watcherState.Eyes.Draw();
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  private void PreDraw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.watchers.Count == 0)
      return;
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Level));
    foreach (WatchersHost.WatcherState watcherState in this.watchers.Values)
    {
      (watcherState.Eyes.Effect as DefaultEffect).Pass = LightingEffectPass.Pre;
      watcherState.Eyes.Draw();
      (watcherState.Eyes.Effect as DefaultEffect).Pass = LightingEffectPass.Main;
    }
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.None));
  }

  [ServiceDependency]
  public ILightingPostProcess LightingPostProcess { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IPhysicsManager PhysicsManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private class WatcherState
  {
    public WatchersHost.WatcherAction Action { get; set; }

    public TimeSpan StartTime { get; set; }

    public Vector3 OriginalCenter { get; set; }

    public Vector3 CrushDirection { get; set; }

    public Mesh Eyes { get; set; }

    public SoundEmitter MoveEmitter { get; set; }

    public SoundEmitter WithdrawEmitter { get; set; }

    public Vector3 EyeOffset { get; set; }

    public float CrashAttenuation { get; set; }

    public bool SkipNextSound { get; set; }
  }

  private enum WatcherAction
  {
    Idle,
    Spotted,
    Crushing,
    Wait,
    Withdrawing,
    Cooldown,
  }
}
