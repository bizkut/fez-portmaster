// Decompiled with JetBrains decompiler
// Type: FezGame.Components.ValvesBoltsTimeswitchesHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class ValvesBoltsTimeswitchesHost : DrawableGameComponent
{
  private readonly List<ValvesBoltsTimeswitchesHost.ValveState> TrackedValves = new List<ValvesBoltsTimeswitchesHost.ValveState>();
  private SoundEffect GrabSound;
  private SoundEffect ValveUnscrew;
  private SoundEffect ValveScrew;
  private SoundEffect BoltScrew;
  private SoundEffect BoltUnscrew;
  private SoundEffect TimeSwitchWind;
  private SoundEffect TimeswitchWindBackSound;
  private SoundEffect TimeswitchEndWindBackSound;

  public ValvesBoltsTimeswitchesHost(Game game)
    : base(game)
  {
    this.DrawOrder = 6;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.GrabSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/MiscActors/GrabLever");
    this.ValveUnscrew = this.CMProvider.Global.Load<SoundEffect>("Sounds/Sewer/ValveUnscrew");
    this.ValveScrew = this.CMProvider.Global.Load<SoundEffect>("Sounds/Sewer/ValveScrew");
    this.BoltUnscrew = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/BoltUnscrew");
    this.BoltScrew = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/BoltScrew");
    this.TimeswitchWindBackSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Nature/TimeswitchWindBack");
    this.TimeswitchEndWindBackSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Nature/TimeswitchEndWindBack");
    this.TimeSwitchWind = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/TimeswitchWindUp");
    this.CameraManager.PreViewpointChanged += new Action(this.OnViewpointChanged);
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void OnViewpointChanged()
  {
    foreach (ValvesBoltsTimeswitchesHost.ValveState trackedValve in this.TrackedValves)
      trackedValve.TrySpin();
  }

  private void TryInitialize()
  {
    this.TrackedValves.Clear();
    foreach (ArtObjectInstance ao in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      if (ao.ArtObject.ActorType == ActorType.Valve || ao.ArtObject.ActorType == ActorType.BoltHandle || ao.ArtObject.ActorType == ActorType.Timeswitch)
        this.TrackedValves.Add(new ValvesBoltsTimeswitchesHost.ValveState(this, ao));
    }
    this.Enabled = this.Visible = this.TrackedValves.Count > 0;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Loading || this.EngineState.InMap || this.EngineState.Paused || !this.CameraManager.Viewpoint.IsOrthographic())
      return;
    float num1 = float.MaxValue;
    ValvesBoltsTimeswitchesHost.ValveState valveState = (ValvesBoltsTimeswitchesHost.ValveState) null;
    foreach (ValvesBoltsTimeswitchesHost.ValveState trackedValve in this.TrackedValves)
    {
      if (trackedValve.ArtObject.ActorSettings.ShouldMoveToEnd)
        trackedValve.MoveToEnd();
      if (trackedValve.ArtObject.ActorSettings.ShouldMoveToHeight.HasValue)
        trackedValve.MoveToHeight();
      if (trackedValve.Update(gameTime.ElapsedGameTime))
      {
        float num2 = trackedValve.ArtObject.Position.Dot(this.CameraManager.Viewpoint.ForwardVector());
        if ((double) num2 < (double) num1)
        {
          valveState = trackedValve;
          num1 = num2;
        }
      }
    }
    valveState?.GrabOnto();
  }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  private class ValveState
  {
    private const float SpinTime = 0.75f;
    private readonly ValvesBoltsTimeswitchesHost Host;
    public readonly ArtObjectInstance TimeswitchScrewAo;
    private SpinAction State;
    private TimeSpan SinceChanged;
    private int SpinSign;
    private Vector3 OriginalPlayerPosition;
    private Vector3 OriginalAoPosition;
    private Quaternion OriginalAoRotation;
    private Quaternion OriginalScrewRotation;
    private Vector3[] OriginalGroupTrilePositions;
    private float ScrewHeight;
    private float RewindSpeed;
    private bool MovingToHeight;
    private readonly SoundEmitter eTimeswitchWindBack;
    private readonly bool IsBolt;
    private readonly bool IsTimeswitch;
    private readonly TrileGroup AttachedGroup;
    private readonly Vector3 CenterOffset;
    public readonly ArtObjectInstance ArtObject;

    public ValveState(ValvesBoltsTimeswitchesHost host, ArtObjectInstance ao)
    {
      ServiceHelper.InjectServices((object) this);
      this.Host = host;
      this.ArtObject = ao;
      this.IsBolt = this.ArtObject.ArtObject.ActorType == ActorType.BoltHandle;
      this.IsTimeswitch = this.ArtObject.ArtObject.ActorType == ActorType.Timeswitch;
      BoundingBox boundingBox = new BoundingBox(this.ArtObject.Position - this.ArtObject.ArtObject.Size / 2f, this.ArtObject.Position + this.ArtObject.ArtObject.Size / 2f);
      if (this.ArtObject.ActorSettings.AttachedGroup.HasValue)
        this.AttachedGroup = this.LevelManager.Groups[this.ArtObject.ActorSettings.AttachedGroup.Value];
      if (this.IsTimeswitch)
      {
        this.eTimeswitchWindBack = this.Host.TimeswitchWindBackSound.EmitAt(ao.Position, true, true);
        foreach (ArtObjectInstance artObjectInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
        {
          if (artObjectInstance != ao && artObjectInstance.ArtObject.ActorType == ActorType.TimeswitchMovingPart)
          {
            BoundingBox box = new BoundingBox(artObjectInstance.Position - artObjectInstance.ArtObject.Size / 2f, artObjectInstance.Position + artObjectInstance.ArtObject.Size / 2f);
            if (boundingBox.Intersects(box))
            {
              this.TimeswitchScrewAo = artObjectInstance;
              break;
            }
          }
        }
      }
      int num1;
      if (!this.IsBolt && !this.IsTimeswitch && this.GameState.SaveData.ThisLevel.PivotRotations.TryGetValue(this.ArtObject.Id, out num1) && num1 != 0)
      {
        int num2 = Math.Abs(num1);
        int num3 = Math.Sign(num1);
        for (int index = 0; index < num2; ++index)
          this.ArtObject.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.57079637f * (float) num3);
      }
      if (this.IsBolt)
      {
        foreach (TrileInstance trile in this.AttachedGroup.Triles)
          trile.PhysicsState = new InstancePhysicsState(trile);
      }
      foreach (Volume volume in (IEnumerable<Volume>) this.LevelManager.Volumes.Values)
      {
        Vector3 vector3 = (volume.To - volume.From).Abs();
        if ((double) vector3.X == 3.0 && (double) vector3.Z == 3.0 && (double) vector3.Y == 1.0 && boundingBox.Contains(volume.BoundingBox) == ContainmentType.Contains)
        {
          this.CenterOffset = (volume.From + volume.To) / 2f - this.ArtObject.Position;
          break;
        }
      }
    }

    public bool Update(TimeSpan elapsed)
    {
      if (this.MovingToHeight)
        return false;
      this.SinceChanged += elapsed;
      Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
      switch (this.State)
      {
        case SpinAction.Idle:
          Vector3 vector = (this.PlayerManager.Position - (this.ArtObject.Position - new Vector3(0.0f, 1f, 0.0f) + this.CenterOffset)) * vector3_1;
          vector.X += vector.Z;
          Vector3 vector3_2 = vector.Abs();
          bool flag = this.IsBolt || this.IsTimeswitch ? (double) vector3_2.X > 0.75 && (double) vector3_2.X < 1.75 && (double) vector3_2.Y < 1.0 : (double) vector3_2.X < 1.0 && (double) vector3_2.Y < 1.0;
          if (this.LevelManager.Flat)
            flag = (double) vector3_2.X < 1.5 && (double) vector3_2.Y < 1.0;
          if (flag && this.PlayerManager.CarriedInstance == null && this.PlayerManager.Grounded && this.PlayerManager.Action != ActionType.GrabTombstone && this.InputManager.FpsToggle != FezButtonState.Pressed && this.InputManager.GrabThrow == FezButtonState.Pressed && this.PlayerManager.Action != ActionType.ReadingSign && this.PlayerManager.Action != ActionType.Dying && this.PlayerManager.Action != ActionType.FreeFalling)
          {
            Vector3 vector3_3 = this.CameraManager.Viewpoint.ForwardVector();
            Vector3 vector3_4 = this.CameraManager.Viewpoint.DepthMask();
            Vector3 vector3_5 = (this.ArtObject.Position + this.CenterOffset) * vector3_4;
            this.PlayerManager.Position = this.PlayerManager.Position * vector3_1 + vector3_4 * vector3_5 - vector3_3 * 1.5f;
            this.SinceChanged = TimeSpan.Zero;
            return true;
          }
          if (this.IsTimeswitch && (double) this.ScrewHeight >= 0.0 && (double) this.ScrewHeight <= 2.0)
          {
            float num1 = (double) this.ArtObject.ActorSettings.TimeswitchWindBackSpeed == 0.0 ? 4f : this.ArtObject.ActorSettings.TimeswitchWindBackSpeed;
            float num2 = (float) (elapsed.TotalSeconds / ((double) num1 - 0.25) * 2.0);
            this.RewindSpeed = this.SinceChanged.TotalSeconds < 0.5 ? MathHelper.Lerp(0.0f, num2, (float) this.SinceChanged.TotalSeconds * 2f) : num2;
            double screwHeight1 = (double) this.ScrewHeight;
            this.ScrewHeight = MathHelper.Clamp(this.ScrewHeight - this.RewindSpeed, 0.0f, 2f);
            double screwHeight2 = (double) this.ScrewHeight;
            float num3 = (float) (screwHeight1 - screwHeight2);
            if ((double) this.ScrewHeight == 0.0 && (double) num3 != 0.0)
            {
              this.Host.TimeswitchEndWindBackSound.EmitAt(this.ArtObject.Position);
              this.TimeswitchService.OnHitBase(this.ArtObject.Id);
              if (this.eTimeswitchWindBack != null && !this.eTimeswitchWindBack.Dead && this.eTimeswitchWindBack.Cue.State == SoundState.Playing)
                this.eTimeswitchWindBack.Cue.Pause();
            }
            else if ((double) num3 != 0.0)
            {
              if (this.eTimeswitchWindBack != null && !this.eTimeswitchWindBack.Dead && this.eTimeswitchWindBack.Cue.State == SoundState.Paused)
                this.eTimeswitchWindBack.Cue.Resume();
              this.eTimeswitchWindBack.VolumeFactor = FezMath.Saturate(num3 * 20f * this.ArtObject.ActorSettings.TimeswitchWindBackSpeed);
            }
            else
            {
              this.eTimeswitchWindBack.VolumeFactor = 0.0f;
              if (this.eTimeswitchWindBack != null && !this.eTimeswitchWindBack.Dead && this.eTimeswitchWindBack.Cue.State == SoundState.Playing)
                this.eTimeswitchWindBack.Cue.Pause();
            }
            this.TimeswitchScrewAo.Position -= Vector3.UnitY * num3;
            this.TimeswitchScrewAo.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((double) num3 * 1.5707963705062866 * -4.0));
            break;
          }
          break;
        case SpinAction.Spinning:
          float num4 = (float) FezMath.Saturate(this.SinceChanged.TotalSeconds / 0.75);
          Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.UnitY, num4 * 1.57079637f * (float) this.SpinSign);
          this.ArtObject.Rotation = this.OriginalAoRotation * fromAxisAngle;
          this.PlayerManager.Position = Vector3.Transform(this.OriginalPlayerPosition - this.ArtObject.Position, fromAxisAngle) + this.ArtObject.Position;
          if (this.IsBolt)
          {
            Vector3 vector3_6 = num4 * (this.SpinSign == 1 ? 1f : -1f) * Vector3.Up;
            this.ArtObject.Position = this.OriginalAoPosition + vector3_6;
            int num5 = 0;
            foreach (TrileInstance trile in this.AttachedGroup.Triles)
            {
              trile.Position = this.OriginalGroupTrilePositions[num5++] + vector3_6;
              this.LevelManager.UpdateInstance(trile);
            }
            this.PlayerManager.Position += vector3_6;
          }
          if (this.IsTimeswitch)
          {
            float num6 = num4;
            if (this.SpinSign == -1 && (double) this.ScrewHeight <= 0.5)
              num6 = Math.Min(this.ScrewHeight, num4 / 2f) * 2f;
            else if (this.SpinSign == 1 && (double) this.ScrewHeight >= 1.5)
              num6 = Math.Min(2f - this.ScrewHeight, num4 / 2f) * 2f;
            this.TimeswitchScrewAo.Position = this.OriginalAoPosition + num6 * (this.SpinSign == 1 ? 1f : -1f) * Vector3.Up / 2f;
            this.TimeswitchScrewAo.Rotation = this.OriginalScrewRotation * Quaternion.CreateFromAxisAngle(Vector3.UnitY, (float) ((double) num6 * 1.5707963705062866 * (double) this.SpinSign * 2.0));
          }
          if (this.SinceChanged.TotalSeconds >= 0.75)
          {
            this.PlayerManager.Position += 0.5f * Vector3.UnitY;
            IPlayerManager playerManager = this.PlayerManager;
            playerManager.Velocity = playerManager.Velocity - Vector3.UnitY;
            this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
            this.CameraManager.Viewpoint.ForwardVector();
            Vector3 vector3_7 = (this.ArtObject.Position + this.CenterOffset) * this.CameraManager.Viewpoint.DepthMask();
            this.ScrewHeight = MathHelper.Clamp(this.ScrewHeight + (float) this.SpinSign / 2f, 0.0f, 2f);
            if ((double) this.ScrewHeight == 0.0 && this.SpinSign == -1)
              this.TimeswitchService.OnHitBase(this.ArtObject.Id);
            this.PlayerManager.Action = ActionType.GrabTombstone;
            this.SinceChanged -= TimeSpan.FromSeconds(0.75);
            this.State = SpinAction.Grabbed;
            break;
          }
          break;
        case SpinAction.Grabbed:
          this.RewindSpeed = 0.0f;
          if (this.PlayerManager.Action != ActionType.GrabTombstone)
            this.State = SpinAction.Idle;
          if (this.IsTimeswitch)
          {
            this.SinceChanged = TimeSpan.Zero;
            this.eTimeswitchWindBack.VolumeFactor = 0.0f;
            if (this.eTimeswitchWindBack != null && !this.eTimeswitchWindBack.Dead && this.eTimeswitchWindBack.Cue.State == SoundState.Playing)
            {
              this.eTimeswitchWindBack.Cue.Pause();
              break;
            }
            break;
          }
          break;
      }
      return false;
    }

    public void MoveToHeight()
    {
      if (this.MovingToHeight)
        return;
      this.MovingToHeight = true;
      float? nullable1 = this.ArtObject.ActorSettings.ShouldMoveToHeight;
      float y = nullable1.Value;
      ArtObjectActorSettings actorSettings = this.ArtObject.ActorSettings;
      nullable1 = new float?();
      float? nullable2 = nullable1;
      actorSettings.ShouldMoveToHeight = nullable2;
      Vector3 vector3 = this.ArtObject.Position + this.CenterOffset;
      Vector3 movement = (new Vector3(0.0f, y, 0.0f) - vector3) * Vector3.UnitY + Vector3.UnitY / 2f;
      Vector3 origin = vector3;
      Vector3 destination = vector3 + movement;
      float lastHeight = origin.Y;
      if (this.PlayerManager.Action == ActionType.PivotTombstone || this.PlayerManager.Grounded && this.AttachedGroup.Triles.Contains(this.PlayerManager.Ground.First))
        this.MovingToHeight = false;
      else if ((double) Math.Abs(movement.Y) < 1.0)
        this.MovingToHeight = false;
      else
        Waiters.Interpolate((double) Math.Abs(movement.Y / 2f), (Action<float>) (step =>
        {
          float amount = Easing.EaseInOut((double) step, EasingType.Sine);
          this.ArtObject.Position = Vector3.Lerp(origin, destination, amount);
          this.ArtObject.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, amount * 1.57079637f * (float) (int) Math.Round((double) movement.Y / 2.0));
          foreach (TrileInstance trile in this.AttachedGroup.Triles)
          {
            trile.Position += Vector3.UnitY * (this.ArtObject.Position.Y - lastHeight);
            trile.PhysicsState.Velocity = Vector3.UnitY * (this.ArtObject.Position.Y - lastHeight);
            this.LevelManager.UpdateInstance(trile);
          }
          lastHeight = this.ArtObject.Position.Y;
        }), (Action) (() =>
        {
          this.ArtObject.Position = destination;
          this.ArtObject.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.57079637f * (float) (int) Math.Round((double) movement.Y / 2.0));
          foreach (TrileInstance trile in this.AttachedGroup.Triles)
            trile.PhysicsState.Velocity = Vector3.Zero;
          this.MovingToHeight = false;
        })).AutoPause = true;
    }

    public void MoveToEnd()
    {
      this.ArtObject.ActorSettings.ShouldMoveToEnd = false;
      Vector3 vector3_1 = new Vector3(2f, 100f, 2f);
      Vector3 vector3_2 = this.ArtObject.Position + this.CenterOffset;
      BoundingBox box = new BoundingBox(vector3_2 - vector3_1, vector3_2 + vector3_1);
      foreach (ArtObjectInstance artObjectInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
      {
        if (artObjectInstance.ArtObject.ActorType == ActorType.BoltNutTop)
        {
          Vector3 vector3_3 = artObjectInstance.Position + Vector3.Up * 3.5f;
          Vector3 vector3_4 = artObjectInstance.ArtObject.Size / 2f + Vector3.Up / 32f;
          if (new BoundingBox(vector3_3 - vector3_4, vector3_3 + vector3_4).Intersects(box))
          {
            Vector3 vector3_5 = artObjectInstance.Position - vector3_2 + Vector3.UnitY / 2f;
            this.ArtObject.Position += vector3_5;
            using (List<TrileInstance>.Enumerator enumerator = this.AttachedGroup.Triles.GetEnumerator())
            {
              while (enumerator.MoveNext())
              {
                TrileInstance current = enumerator.Current;
                current.Position += vector3_5;
                this.LevelManager.UpdateInstance(current);
              }
              break;
            }
          }
        }
      }
    }

    public void GrabOnto()
    {
      this.PlayerManager.Action = ActionType.GrabTombstone;
      Waiters.Wait(0.4, (Func<float, bool>) (_ => this.PlayerManager.Action != ActionType.GrabTombstone), (Action) (() =>
      {
        if (this.PlayerManager.Action != ActionType.GrabTombstone)
          return;
        this.Host.GrabSound.EmitAt(this.ArtObject.Position);
        this.State = SpinAction.Grabbed;
      }));
    }

    public void TrySpin()
    {
      if (this.State != SpinAction.Grabbed)
        return;
      if (this.PlayerManager.Action != ActionType.GrabTombstone)
      {
        this.State = SpinAction.Idle;
      }
      else
      {
        if (!this.PlayerManager.Animation.Timing.Ended || this.CameraManager.Viewpoint == Viewpoint.Perspective || this.CameraManager.LastViewpoint == this.CameraManager.Viewpoint)
          return;
        this.SpinSign = this.CameraManager.LastViewpoint.GetDistance(this.CameraManager.Viewpoint);
        if (this.IsBolt)
        {
          Vector3 vector3_1 = new Vector3(2f);
          Vector3 vector3_2 = this.ArtObject.Position + this.CenterOffset;
          BoundingBox box = new BoundingBox(vector3_2 - vector3_1, vector3_2 + vector3_1);
          foreach (ArtObjectInstance artObjectInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
          {
            if (artObjectInstance.ArtObject.ActorType == ActorType.BoltNutBottom && this.SpinSign == -1)
            {
              Vector3 vector3_3 = artObjectInstance.ArtObject.Size / 2f + Vector3.Up / 32f;
              if (new BoundingBox(artObjectInstance.Position - vector3_3, artObjectInstance.Position + vector3_3).Intersects(box))
              {
                this.CameraManager.CancelViewTransition();
                return;
              }
            }
            else if (artObjectInstance.ArtObject.ActorType == ActorType.BoltNutTop && this.SpinSign == 1)
            {
              Vector3 vector3_4 = artObjectInstance.Position + Vector3.Up * 3.5f;
              Vector3 vector3_5 = artObjectInstance.ArtObject.Size / 2f + Vector3.Up / 32f;
              if (new BoundingBox(vector3_4 - vector3_5, vector3_4 + vector3_5).Intersects(box))
              {
                this.CameraManager.CancelViewTransition();
                return;
              }
            }
          }
        }
        if (this.IsTimeswitch && this.SpinSign == -1)
        {
          this.CameraManager.CancelViewTransition();
        }
        else
        {
          if (this.IsBolt)
          {
            if (this.SpinSign == 1)
              this.Host.BoltScrew.EmitAt(this.ArtObject.Position);
            else
              this.Host.BoltUnscrew.EmitAt(this.ArtObject.Position);
          }
          else if (this.IsTimeswitch)
            this.Host.TimeSwitchWind.EmitAt(this.ArtObject.Position);
          else if (this.SpinSign == 1)
            this.Host.ValveScrew.EmitAt(this.ArtObject.Position);
          else
            this.Host.ValveUnscrew.EmitAt(this.ArtObject.Position);
          int num;
          if (!this.GameState.SaveData.ThisLevel.PivotRotations.TryGetValue(this.ArtObject.Id, out num))
            this.GameState.SaveData.ThisLevel.PivotRotations.Add(this.ArtObject.Id, this.SpinSign);
          else
            this.GameState.SaveData.ThisLevel.PivotRotations[this.ArtObject.Id] = num + this.SpinSign;
          int lastViewpoint = (int) this.CameraManager.LastViewpoint;
          Vector3 vector3_6 = ((Viewpoint) lastViewpoint).ScreenSpaceMask();
          Vector3 vector3_7 = ((Viewpoint) lastViewpoint).ForwardVector();
          Vector3 vector3_8 = ((Viewpoint) lastViewpoint).DepthMask();
          Vector3 vector3_9 = (this.ArtObject.Position + this.CenterOffset) * vector3_8;
          this.OriginalPlayerPosition = this.PlayerManager.Position = this.PlayerManager.Position * vector3_6 + vector3_8 * vector3_9 - vector3_7 * 2f;
          this.OriginalAoRotation = this.ArtObject.Rotation;
          this.OriginalAoPosition = this.IsTimeswitch ? this.TimeswitchScrewAo.Position : this.ArtObject.Position;
          if (this.IsTimeswitch)
            this.OriginalScrewRotation = this.TimeswitchScrewAo.Rotation;
          if (this.AttachedGroup != null)
            this.OriginalGroupTrilePositions = this.AttachedGroup.Triles.Select<TrileInstance, Vector3>((Func<TrileInstance, Vector3>) (x => x.Position)).ToArray<Vector3>();
          this.SinceChanged = TimeSpan.Zero;
          this.State = SpinAction.Spinning;
          this.PlayerManager.Action = ActionType.PivotTombstone;
          if (this.IsTimeswitch)
          {
            if (this.SpinSign != 1 || (double) this.ScrewHeight > 0.0)
              return;
            this.TimeswitchService.OnScrewedOut(this.ArtObject.Id);
          }
          else if (this.SpinSign == -1)
            this.ValveService.OnUnscrew(this.ArtObject.Id);
          else
            this.ValveService.OnScrew(this.ArtObject.Id);
        }
      }
    }

    [ServiceDependency]
    public ISoundManager SoundManager { private get; set; }

    [ServiceDependency]
    public IPhysicsManager PhysicsManager { private get; set; }

    [ServiceDependency]
    public ILevelManager LevelManager { private get; set; }

    [ServiceDependency]
    public IInputManager InputManager { private get; set; }

    [ServiceDependency]
    public IGameCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }

    [ServiceDependency]
    public IValveService ValveService { private get; set; }

    [ServiceDependency]
    public ITimeswitchService TimeswitchService { private get; set; }

    [ServiceDependency]
    public IDebuggingBag DebuggingBag { private get; set; }
  }
}
