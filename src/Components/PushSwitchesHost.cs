// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PushSwitchesHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class PushSwitchesHost : GameComponent
{
  private readonly Dictionary<int, PushSwitchesHost.SwitchState> trackedSwitches = new Dictionary<int, PushSwitchesHost.SwitchState>();
  private SoundEffect chick;
  private SoundEffect poum;
  private SoundEffect release;

  public PushSwitchesHost(Game game)
    : base(game)
  {
    this.UpdateOrder = -2;
  }

  public override void Initialize()
  {
    this.LevelManager.LevelChanging += new Action(this.TrackSwitches);
    this.TrackSwitches();
    this.chick = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/SwitchHalfPress");
    this.poum = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/SwitchPress");
    this.release = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/SwitchHalfRelease");
  }

  private void TrackSwitches()
  {
    this.trackedSwitches.Clear();
    foreach (TrileGroup group in this.LevelManager.Groups.Values.Where<TrileGroup>((Func<TrileGroup, bool>) (x => x.ActorType.IsPushSwitch())))
      this.trackedSwitches.Add(group.Id, new PushSwitchesHost.SwitchState(group, this.chick, this.poum, this.release));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || this.GameState.Loading)
      return;
    foreach (PushSwitchesHost.SwitchState switchState in this.trackedSwitches.Values)
      switchState.Update(gameTime.ElapsedGameTime);
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private class SwitchState
  {
    private static readonly TimeSpan HalfPushDuration = TimeSpan.FromSeconds(0.15000000596046448);
    private static readonly TimeSpan FullPushDuration = TimeSpan.FromSeconds(0.44999998807907104);
    private static readonly TimeSpan ComeBackDuration = TimeSpan.FromSeconds(0.75);
    private const float HalfPushHeight = 0.1875f;
    private const float FullPushHeight = 0.8125f;
    private readonly SoundEffect ClickSound;
    private readonly SoundEffect ThudSound;
    private readonly SoundEffect ReleaseSound;
    private readonly TrileGroup Group;
    private readonly float OriginalHeight;
    private readonly PushSwitchesHost.SwitchState.SwitchPermanence Permanence;
    private PushSwitchesHost.SwitchState.SwitchAction action;
    private TimeSpan sinceActionStarted;
    private float lastStep;
    private TrileInstance[] Hits = new TrileInstance[5];

    public SwitchState(
      TrileGroup group,
      SoundEffect clickSound,
      SoundEffect thudSound,
      SoundEffect releaseSound)
    {
      foreach (TrileInstance instance in group.Triles.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.PhysicsState == null)))
        instance.PhysicsState = new InstancePhysicsState(instance)
        {
          Sticky = true
        };
      this.ClickSound = clickSound;
      this.ThudSound = thudSound;
      this.ReleaseSound = releaseSound;
      this.Permanence = group.ActorType == ActorType.PushSwitchPermanent ? PushSwitchesHost.SwitchState.SwitchPermanence.Permanent : (group.ActorType == ActorType.PushSwitchSticky ? PushSwitchesHost.SwitchState.SwitchPermanence.Sticky : PushSwitchesHost.SwitchState.SwitchPermanence.Volatile);
      this.Group = group;
      this.OriginalHeight = group.Triles[0].Position.Y;
      ServiceHelper.InjectServices((object) this);
      if (this.Permanence != PushSwitchesHost.SwitchState.SwitchPermanence.Permanent || !this.GameState.SaveData.ThisLevel.InactiveGroups.Contains(this.Group.Id))
        return;
      this.action = PushSwitchesHost.SwitchState.SwitchAction.HeldDown;
    }

    public void Update(TimeSpan elapsed)
    {
      bool flag1 = false;
      bool flag2 = false;
      if (this.PlayerManager.Grounded && (this.Group.Triles.Contains(this.PlayerManager.Ground.NearLow) || this.Group.Triles.Contains(this.PlayerManager.Ground.FarHigh)))
      {
        flag1 = true;
        flag2 = this.PlayerManager.CarriedInstance != null;
      }
      foreach (TrileInstance trile in this.Group.Triles)
      {
        Vector3 transformedSize = trile.TransformedSize;
        Vector3 center = trile.Center;
        Vector3 vector3 = new Vector3(0.0f, 0.5f, 0.0f);
        Array.Clear((Array) this.Hits, 0, 5);
        int index = 0;
        TrileInstance trileInstance1 = this.LevelManager.ActualInstanceAt(center + transformedSize * new Vector3(0.0f, 0.5f, 0.0f) + vector3);
        if (trileInstance1 != null && Array.IndexOf<TrileInstance>(this.Hits, trileInstance1) == -1)
          this.Hits[index++] = trileInstance1;
        TrileInstance trileInstance2 = this.LevelManager.ActualInstanceAt(center + transformedSize * new Vector3(0.5f, 0.5f, 0.0f) + vector3);
        if (trileInstance2 != null && Array.IndexOf<TrileInstance>(this.Hits, trileInstance2) == -1)
          this.Hits[index++] = trileInstance2;
        TrileInstance trileInstance3 = this.LevelManager.ActualInstanceAt(center + transformedSize * new Vector3(-0.5f, 0.5f, 0.0f) + vector3);
        if (trileInstance3 != null && Array.IndexOf<TrileInstance>(this.Hits, trileInstance3) == -1)
          this.Hits[index++] = trileInstance3;
        TrileInstance trileInstance4 = this.LevelManager.ActualInstanceAt(center + transformedSize * new Vector3(0.0f, 0.5f, 0.5f) + vector3);
        if (trileInstance4 != null && Array.IndexOf<TrileInstance>(this.Hits, trileInstance4) == -1)
          this.Hits[index++] = trileInstance4;
        TrileInstance trileInstance5 = this.LevelManager.ActualInstanceAt(center + transformedSize * new Vector3(0.0f, 0.5f, -0.5f) + vector3);
        if (trileInstance5 != null && Array.IndexOf<TrileInstance>(this.Hits, trileInstance5) == -1)
          this.Hits[index] = trileInstance5;
        if (index != 0 || this.Hits[0] != null)
        {
          foreach (TrileInstance hit in this.Hits)
          {
            if (hit != null && hit.PhysicsState != null && (hit.PhysicsState.Ground.NearLow == trile || hit.PhysicsState.Ground.FarHigh == trile))
            {
              if (hit.Trile.ActorSettings.Type.IsHeavy() | flag1)
                flag2 = true;
              flag1 = true;
            }
          }
        }
      }
      float num = 0.0f;
      PushSwitchesHost.SwitchState.SwitchAction switchAction = this.action;
      switch (this.action)
      {
        case PushSwitchesHost.SwitchState.SwitchAction.Up:
          num = 0.0f;
          if (flag1)
          {
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.HalfPush;
            break;
          }
          break;
        case PushSwitchesHost.SwitchState.SwitchAction.HalfPush:
          num = (float) ((double) this.sinceActionStarted.Ticks / (double) PushSwitchesHost.SwitchState.HalfPushDuration.Ticks * (3.0 / 16.0));
          if (!flag1)
          {
            this.ReleaseSound.EmitAt(this.Group.Triles.First<TrileInstance>().Center);
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.ComingBack;
          }
          if (this.sinceActionStarted.Ticks >= PushSwitchesHost.SwitchState.HalfPushDuration.Ticks)
          {
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.HeldAtHalf;
            this.ClickSound.EmitAt(this.Group.Triles.First<TrileInstance>().Center);
            break;
          }
          break;
        case PushSwitchesHost.SwitchState.SwitchAction.HeldAtHalf:
          num = 3f / 16f;
          if (!flag1)
          {
            this.ReleaseSound.EmitAt(this.Group.Triles.First<TrileInstance>().Center);
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.ComingBack;
          }
          if (flag1 & flag2)
          {
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.FullPush;
            break;
          }
          break;
        case PushSwitchesHost.SwitchState.SwitchAction.FullPush:
          num = (float) (3.0 / 16.0 + (double) Easing.EaseIn((double) this.sinceActionStarted.Ticks / (double) PushSwitchesHost.SwitchState.FullPushDuration.Ticks, EasingType.Quadratic) * 0.625);
          if (!flag1 && this.Permanence == PushSwitchesHost.SwitchState.SwitchPermanence.Volatile)
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.ComingBack;
          if (this.sinceActionStarted.Ticks >= PushSwitchesHost.SwitchState.FullPushDuration.Ticks)
          {
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.HeldDown;
            this.SwitchService.OnPush(this.Group.Id);
            this.ThudSound.EmitAt(this.Group.Triles.First<TrileInstance>().Center);
            if (this.Permanence == PushSwitchesHost.SwitchState.SwitchPermanence.Permanent)
            {
              this.GameState.SaveData.ThisLevel.InactiveGroups.Add(this.Group.Id);
              break;
            }
            break;
          }
          break;
        case PushSwitchesHost.SwitchState.SwitchAction.HeldDown:
          num = 13f / 16f;
          if (!flag1 && this.Permanence == PushSwitchesHost.SwitchState.SwitchPermanence.Volatile)
          {
            this.SwitchService.OnLift(this.Group.Id);
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.ComingBack;
          }
          if (flag1 && !flag2 && this.Permanence == PushSwitchesHost.SwitchState.SwitchPermanence.Volatile)
          {
            this.SwitchService.OnLift(this.Group.Id);
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.BackToHalf;
            break;
          }
          break;
        case PushSwitchesHost.SwitchState.SwitchAction.ComingBack:
          num = this.lastStep - (float) this.sinceActionStarted.Ticks / ((float) PushSwitchesHost.SwitchState.ComeBackDuration.Ticks * this.lastStep) * this.lastStep;
          if ((double) this.sinceActionStarted.Ticks >= (double) PushSwitchesHost.SwitchState.ComeBackDuration.Ticks * (double) this.lastStep)
          {
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.Up;
            break;
          }
          break;
        case PushSwitchesHost.SwitchState.SwitchAction.BackToHalf:
          num = this.lastStep - (float) this.sinceActionStarted.Ticks / ((float) PushSwitchesHost.SwitchState.ComeBackDuration.Ticks * this.lastStep) * this.lastStep;
          if ((double) this.sinceActionStarted.Ticks >= (double) PushSwitchesHost.SwitchState.ComeBackDuration.Ticks * (double) this.lastStep)
          {
            switchAction = PushSwitchesHost.SwitchState.SwitchAction.Up;
            break;
          }
          break;
      }
      if (switchAction != this.action)
      {
        this.action = switchAction;
        if (switchAction == PushSwitchesHost.SwitchState.SwitchAction.ComingBack || switchAction == PushSwitchesHost.SwitchState.SwitchAction.BackToHalf)
          this.lastStep = num;
        this.sinceActionStarted = TimeSpan.Zero;
      }
      this.sinceActionStarted += elapsed;
      float y1 = this.Group.Triles[0].Position.Y;
      foreach (TrileInstance trile in this.Group.Triles)
        trile.Position = new Vector3(trile.Position.X, this.OriginalHeight - num, trile.Position.Z);
      float y2 = this.Group.Triles[0].Position.Y - y1;
      foreach (TrileInstance trile in this.Group.Triles)
      {
        trile.PhysicsState.Velocity = new Vector3(0.0f, y2, 0.0f);
        if ((double) y2 != 0.0)
          this.LevelManager.UpdateInstance(trile);
      }
    }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }

    [ServiceDependency]
    public ILevelManager LevelManager { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public ICollisionManager CollisionManager { private get; set; }

    [ServiceDependency]
    public ISwitchService SwitchService { private get; set; }

    private enum SwitchPermanence
    {
      Volatile,
      Sticky,
      Permanent,
    }

    private enum SwitchAction
    {
      Up,
      HalfPush,
      HeldAtHalf,
      FullPush,
      HeldDown,
      ComingBack,
      BackToHalf,
    }
  }
}
