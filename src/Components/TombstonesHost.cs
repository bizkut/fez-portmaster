// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TombstonesHost
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

public class TombstonesHost : DrawableGameComponent
{
  private readonly List<TombstonesHost.TombstoneState> TrackedStones = new List<TombstonesHost.TombstoneState>();
  private ArtObjectInstance SkullAo;
  private Vector4[] SkullAttachedTrilesOriginalStates;
  private TrileInstance[] SkullTopLayer;
  private TrileInstance[] SkullAttachedTriles;
  private Quaternion InterpolatedRotation;
  private Quaternion OriginalRotation;
  private bool SkullRotates;
  private bool StopSkullRotations;
  private SoundEffect GrabSound;
  private SoundEffect TurnLeft;
  private SoundEffect TurnRight;
  private SoundEffect sRumble;
  private SoundEmitter eRumble;
  private float lastAngle;

  public TombstonesHost(Game game)
    : base(game)
  {
    this.DrawOrder = 6;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.CameraManager.ViewpointChanged += new Action(this.OnViewpointChanged);
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void OnViewpointChanged()
  {
    foreach (TombstonesHost.TombstoneState trackedStone in this.TrackedStones)
      trackedStone.TrySpin();
  }

  private void TryInitialize()
  {
    this.TrackedStones.Clear();
    this.GrabSound = (SoundEffect) null;
    this.TurnLeft = (SoundEffect) null;
    this.TurnRight = (SoundEffect) null;
    this.sRumble = (SoundEffect) null;
    this.eRumble = (SoundEmitter) null;
    foreach (ArtObjectInstance ao in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      if (ao.ArtObject.ActorType == ActorType.Tombstone)
        this.TrackedStones.Add(new TombstonesHost.TombstoneState(this, ao));
    }
    this.SkullAo = this.LevelManager.ArtObjects.Values.SingleOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObjectName == "GIANT_SKULLAO"));
    this.Enabled = this.Visible = this.TrackedStones.Count > 0 && this.SkullAo != null;
    if (!this.Enabled)
      return;
    int key = this.SkullAo.ActorSettings.AttachedGroup.Value;
    this.SkullTopLayer = this.LevelManager.Groups[key].Triles.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.Faces[FaceOrientation.Back] == CollisionType.TopOnly)).ToArray<TrileInstance>();
    this.SkullAttachedTriles = this.LevelManager.Groups[key].Triles.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.Immaterial)).ToArray<TrileInstance>();
    this.SkullAttachedTrilesOriginalStates = ((IEnumerable<TrileInstance>) this.SkullAttachedTriles).Select<TrileInstance, Vector4>((Func<TrileInstance, Vector4>) (x => new Vector4(x.Position, x.Phi))).ToArray<Vector4>();
    this.GrabSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/GrabLever");
    this.TurnLeft = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Graveyard/TombRotateLeft");
    this.TurnRight = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Graveyard/TombRotateRight");
    this.sRumble = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/Rumble");
    this.eRumble = this.sRumble.Emit(true, true);
    int count = this.TrackedStones.Count<TombstonesHost.TombstoneState>((Func<TombstonesHost.TombstoneState, bool>) (x => x.LastViewpoint == this.TrackedStones[0].LastViewpoint));
    this.SkullRotates = count < 4;
    this.TombstoneService.UpdateAlignCount(count);
    this.OriginalRotation = this.SkullAo.Rotation * Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.57079637f);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Loading || this.EngineState.InMap || this.EngineState.Paused || !this.CameraManager.Viewpoint.IsOrthographic())
      return;
    float num1 = float.MaxValue;
    TombstonesHost.TombstoneState tombstoneState = (TombstonesHost.TombstoneState) null;
    foreach (TombstonesHost.TombstoneState trackedStone in this.TrackedStones)
    {
      if (trackedStone.Update(gameTime.ElapsedGameTime))
      {
        float num2 = trackedStone.ArtObject.Position.Dot(this.CameraManager.Viewpoint.ForwardVector());
        if ((double) num2 < (double) num1)
        {
          tombstoneState = trackedStone;
          num1 = num2;
        }
      }
    }
    tombstoneState?.GrabOnto();
    if (!this.SkullRotates)
      return;
    this.RotateSkull();
  }

  private void RotateSkull()
  {
    this.InterpolatedRotation = Quaternion.Slerp(this.InterpolatedRotation, this.StopSkullRotations ? this.OriginalRotation : this.CameraManager.Rotation, 0.05f);
    if (this.InterpolatedRotation == this.CameraManager.Rotation)
    {
      if (this.eRumble.Cue.State != SoundState.Paused)
        this.eRumble.Cue.Pause();
      if (!this.StopSkullRotations)
        return;
      this.SkullRotates = false;
      this.StopSkullRotations = false;
    }
    else
    {
      if (FezMath.AlmostEqual(this.InterpolatedRotation, this.CameraManager.Rotation) || FezMath.AlmostEqual(-this.InterpolatedRotation, this.CameraManager.Rotation))
        this.InterpolatedRotation = this.CameraManager.Rotation;
      this.SkullAo.Rotation = this.InterpolatedRotation * Quaternion.CreateFromAxisAngle(Vector3.UnitY, -1.57079637f);
      Vector3 axis;
      float angle1;
      TombstonesHost.ToAxisAngle(ref this.InterpolatedRotation, out axis, out angle1);
      float angle2 = this.lastAngle - angle1;
      if ((double) Math.Abs(angle2) > 0.10000000149011612)
      {
        this.lastAngle = angle1;
      }
      else
      {
        for (int index = 0; index < this.SkullAttachedTriles.Length; ++index)
        {
          TrileInstance skullAttachedTrile = this.SkullAttachedTriles[index];
          Vector4 trilesOriginalState = this.SkullAttachedTrilesOriginalStates[index];
          skullAttachedTrile.Position = Vector3.Transform(trilesOriginalState.XYZ() + new Vector3(0.5f) - this.SkullAo.Position, this.InterpolatedRotation) + this.SkullAo.Position - new Vector3(0.5f);
          skullAttachedTrile.Phi = FezMath.WrapAngle(trilesOriginalState.W + ((double) axis.Y > 0.0 ? -1f : 1f) * angle1);
          this.LevelMaterializer.GetTrileMaterializer(skullAttachedTrile.Trile).UpdateInstance(skullAttachedTrile);
        }
        if (((IEnumerable<TrileInstance>) this.SkullTopLayer).Contains<TrileInstance>(this.PlayerManager.Ground.First))
        {
          Vector3 position = this.PlayerManager.Position;
          this.PlayerManager.Position = Vector3.Transform(this.PlayerManager.Position - this.SkullAo.Position, Quaternion.CreateFromAxisAngle(axis, angle2)) + this.SkullAo.Position;
          IGameCameraManager cameraManager = this.CameraManager;
          cameraManager.Center = cameraManager.Center + (this.PlayerManager.Position - position);
        }
        if ((double) Math.Abs(axis.Y) > 0.5)
        {
          float max = angle2 * 5f;
          IGameCameraManager cameraManager = this.CameraManager;
          cameraManager.InterpolatedCenter = cameraManager.InterpolatedCenter + new Vector3(RandomHelper.Between(-(double) max, (double) max), RandomHelper.Between(-(double) max, (double) max), RandomHelper.Between(-(double) max, (double) max));
          if (this.eRumble.Cue.State == SoundState.Paused)
            this.eRumble.Cue.Resume();
          this.eRumble.VolumeFactor = FezMath.Saturate(Math.Abs(max) * 25f);
        }
        if (this.InterpolatedRotation == this.CameraManager.Rotation)
          this.RotateSkullTriles();
        this.lastAngle = angle1;
      }
    }
  }

  private void RotateSkullTriles()
  {
    foreach (TrileInstance skullAttachedTrile in this.SkullAttachedTriles)
      this.LevelManager.UpdateInstance(skullAttachedTrile);
  }

  private static void ToAxisAngle(ref Quaternion q, out Vector3 axis, out float angle)
  {
    angle = (float) Math.Acos((double) MathHelper.Clamp(q.W, -1f, 1f));
    float num1 = (float) Math.Sin((double) angle);
    float num2 = (float) (1.0 / ((double) num1 == 0.0 ? 1.0 : (double) num1));
    angle *= 2f;
    axis = new Vector3(-q.X * num2, -q.Y * num2, -q.Z * num2);
  }

  public override void Draw(GameTime gameTime)
  {
    int num = this.EngineState.Loading ? 1 : 0;
  }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { get; set; }

  [ServiceDependency]
  public ITombstoneService TombstoneService { private get; set; }

  private class TombstoneState
  {
    private const float SpinTime = 0.75f;
    private readonly TombstonesHost Host;
    private SpinAction State;
    private TimeSpan SinceChanged;
    private int SpinSign;
    private Vector3 OriginalPlayerPosition;
    private Quaternion OriginalAoRotation;
    internal Viewpoint LastViewpoint;
    public readonly ArtObjectInstance ArtObject;

    public TombstoneState(TombstonesHost host, ArtObjectInstance ao)
    {
      ServiceHelper.InjectServices((object) this);
      this.Host = host;
      this.ArtObject = ao;
      int num1;
      if (this.GameState.SaveData.ThisLevel.PivotRotations.TryGetValue(this.ArtObject.Id, out num1) && num1 != 0)
      {
        int num2 = Math.Abs(num1);
        for (int index = 0; index < num2; ++index)
        {
          this.OriginalAoRotation = this.ArtObject.Rotation;
          this.ArtObject.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.57079637f * (float) Math.Sign(num1));
        }
      }
      this.LastViewpoint = FezMath.OrientationFromDirection(Vector3.Transform(Vector3.Forward, this.ArtObject.Rotation).MaxClampXZ()).AsViewpoint();
    }

    public bool Update(TimeSpan elapsed)
    {
      this.SinceChanged += elapsed;
      switch (this.State)
      {
        case SpinAction.Idle:
          Vector3 vector = (this.PlayerManager.Position - (this.ArtObject.Position - new Vector3(0.0f, 1f, 0.0f))) * this.CameraManager.Viewpoint.ScreenSpaceMask();
          vector.X += vector.Z;
          Vector3 vector3 = vector.Abs();
          bool flag = (double) vector3.X < 0.89999997615814209 && (double) vector3.Y < 1.0;
          if (FezMath.AlmostEqual(Vector3.Transform(Vector3.UnitZ, this.ArtObject.Rotation).Abs(), this.CameraManager.Viewpoint.DepthMask()) & flag && this.PlayerManager.CarriedInstance == null && this.PlayerManager.Grounded && this.PlayerManager.Action != ActionType.GrabTombstone && this.InputManager.FpsToggle != FezButtonState.Pressed && this.InputManager.GrabThrow == FezButtonState.Pressed && this.PlayerManager.Action != ActionType.ReadingSign)
          {
            this.SinceChanged = TimeSpan.Zero;
            return true;
          }
          break;
        case SpinAction.Spinning:
          double num = FezMath.Saturate(this.SinceChanged.TotalSeconds / 0.75);
          Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.UnitY, Easing.EaseIn(num < 0.949999988079071 ? num / 0.949999988079071 : 1.0 + Math.Sin((num - 0.949999988079071) / 0.05000000074505806 * 6.2831854820251465 * 2.0) * 0.0099999997764825821 * (1.0 - num) / 0.05000000074505806, EasingType.Linear) * 1.57079637f * (float) this.SpinSign);
          this.ArtObject.Rotation = this.OriginalAoRotation * fromAxisAngle;
          this.PlayerManager.Position = Vector3.Transform(this.OriginalPlayerPosition - this.ArtObject.Position, fromAxisAngle) + this.ArtObject.Position;
          if (this.SinceChanged.TotalSeconds >= 0.75)
          {
            this.LastViewpoint = FezMath.OrientationFromDirection(Vector3.Transform(Vector3.Forward, this.ArtObject.Rotation).MaxClampXZ()).AsViewpoint();
            int count = this.Host.TrackedStones.Count<TombstonesHost.TombstoneState>((Func<TombstonesHost.TombstoneState, bool>) (x => x.LastViewpoint == this.LastViewpoint));
            this.TombstoneService.UpdateAlignCount(count);
            if (count > 1)
              this.TombstoneService.OnMoreThanOneAligned();
            this.Host.StopSkullRotations = count == 4;
            this.PlayerManager.Action = ActionType.GrabTombstone;
            this.PlayerManager.Position += 0.5f * Vector3.UnitY;
            this.PlayerManager.Velocity = Vector3.Down;
            this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
            this.SinceChanged -= TimeSpan.FromSeconds(0.75);
            this.State = SpinAction.Grabbed;
            break;
          }
          break;
        case SpinAction.Grabbed:
          if (this.PlayerManager.Action != ActionType.GrabTombstone)
          {
            this.State = SpinAction.Idle;
            break;
          }
          break;
      }
      return false;
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
        if (this.SpinSign == 1)
          this.Host.TurnRight.EmitAt(this.ArtObject.Position);
        else
          this.Host.TurnLeft.EmitAt(this.ArtObject.Position);
        int num;
        if (!this.GameState.SaveData.ThisLevel.PivotRotations.TryGetValue(this.ArtObject.Id, out num))
          this.GameState.SaveData.ThisLevel.PivotRotations.Add(this.ArtObject.Id, this.SpinSign);
        else
          this.GameState.SaveData.ThisLevel.PivotRotations[this.ArtObject.Id] = num + this.SpinSign;
        this.PlayerManager.Position = this.PlayerManager.Position * this.CameraManager.LastViewpoint.ScreenSpaceMask() + this.ArtObject.Position * this.CameraManager.LastViewpoint.DepthMask() + -this.CameraManager.LastViewpoint.ForwardVector();
        this.OriginalPlayerPosition = this.PlayerManager.Position;
        this.OriginalAoRotation = this.ArtObject.Rotation;
        this.SinceChanged = TimeSpan.Zero;
        this.State = SpinAction.Spinning;
        this.PlayerManager.Action = ActionType.PivotTombstone;
      }
    }

    [ServiceDependency]
    public IPhysicsManager PhysicsManager { private get; set; }

    [ServiceDependency]
    public IInputManager InputManager { private get; set; }

    [ServiceDependency]
    public IDefaultCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }

    [ServiceDependency]
    public ITombstoneService TombstoneService { private get; set; }
  }
}
