// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SpinBlocksHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
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

internal class SpinBlocksHost(Game game) : GameComponent(game)
{
  private readonly List<SpinBlocksHost.SpinBlockState> TrackedBlocks = new List<SpinBlocksHost.SpinBlockState>();
  private SoundEffect smallSound;
  private SoundEffect largeSound;
  private SoundEffect rotatoSound;

  public override void Initialize()
  {
    base.Initialize();
    this.smallSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/SmallSpinblock");
    this.largeSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/LargeSpinblock");
    this.rotatoSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/RotatoSpinblock");
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.TrackedBlocks.Clear();
    if (this.LevelManager.TrileSet == null)
      return;
    TrileInstance[] array = this.LevelManager.TrileSet.Triles.Values.Where<Trile>((Func<Trile, bool>) (x => x.Geometry != null && x.Geometry.Empty)).SelectMany<Trile, TrileInstance>((Func<Trile, IEnumerable<TrileInstance>>) (x => (IEnumerable<TrileInstance>) x.Instances)).ToArray<TrileInstance>();
    foreach (ArtObjectInstance aoInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      if (aoInstance.ArtObject.ActorType == ActorType.SpinBlock)
      {
        Vector3 vector3_1 = aoInstance.ActorSettings.OffCenter ? aoInstance.ActorSettings.RotationCenter : aoInstance.Position;
        BoundingBox box = new BoundingBox((vector3_1 - aoInstance.ArtObject.Size / 2f).Floor(), (vector3_1 + aoInstance.ArtObject.Size / 2f).Floor());
        List<TrileInstance> triles = new List<TrileInstance>();
        foreach (TrileInstance trileInstance in array)
        {
          Vector3 center = trileInstance.Center;
          Vector3 vector3_2 = trileInstance.TransformedSize / 2f;
          if (new BoundingBox(center - vector3_2, center + vector3_2).Intersects(box))
          {
            triles.Add(trileInstance);
            trileInstance.ForceTopMaybe = true;
          }
        }
        if (triles.Count > 0)
        {
          SoundEffect soundEffect = aoInstance.ActorSettings.SpinView == Viewpoint.Up || aoInstance.ActorSettings.SpinView == Viewpoint.Down ? this.rotatoSound : (triles.Count < 4 ? this.smallSound : this.largeSound);
          this.TrackedBlocks.Add(new SpinBlocksHost.SpinBlockState(triles, aoInstance, soundEffect));
        }
      }
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || this.GameState.InMenuCube || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning && !this.PlayerManager.IsOnRotato)
      return;
    foreach (SpinBlocksHost.SpinBlockState trackedBlock in this.TrackedBlocks)
      trackedBlock.Update(gameTime.ElapsedGameTime);
  }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  private class SpinBlockState
  {
    private const float WarnTime = 0.1f;
    private const float SpinTime = 0.5f;
    private const float Charge = 0.1f;
    private readonly List<TrileInstance> Triles;
    private readonly ArtObjectInstance ArtObject;
    private readonly Vector3 OriginalPosition;
    private readonly Vector3 RotationOffset;
    private readonly bool IsRotato;
    private SpinBlocksHost.SpinState State;
    private TimeSpan SinceChanged;
    private Quaternion OriginalRotation;
    private Quaternion SpinAccumulatedRotation = Quaternion.Identity;
    private Vector3 OriginalPlayerPosition;
    private readonly SoundEffect SoundEffect;
    private SoundEmitter Emitter;
    private bool hasRotated;

    public SpinBlockState(
      List<TrileInstance> triles,
      ArtObjectInstance aoInstance,
      SoundEffect soundEffect)
    {
      ServiceHelper.InjectServices((object) this);
      this.Triles = triles;
      this.ArtObject = aoInstance;
      this.OriginalPosition = this.ArtObject.Position;
      if (this.ArtObject.ActorSettings.OffCenter)
        this.RotationOffset = this.ArtObject.ActorSettings.RotationCenter - this.ArtObject.Position;
      if (this.ArtObject.ActorSettings.SpinView == Viewpoint.None)
        this.ArtObject.ActorSettings.SpinView = Viewpoint.Front;
      foreach (TrileInstance trile in this.Triles)
        trile.Unsafe = true;
      this.SoundEffect = soundEffect;
      this.SinceChanged -= TimeSpan.FromSeconds((double) this.ArtObject.ActorSettings.SpinOffset);
      this.IsRotato = this.ArtObject.ActorSettings.SpinView == Viewpoint.Up || this.ArtObject.ActorSettings.SpinView == Viewpoint.Down;
    }

    public void Update(TimeSpan elapsed)
    {
      if (this.ArtObject.ActorSettings.Inactive && this.State == SpinBlocksHost.SpinState.Idle)
        return;
      this.SinceChanged += elapsed;
      switch (this.State)
      {
        case SpinBlocksHost.SpinState.Idle:
          if (this.SinceChanged.TotalSeconds < (double) this.ArtObject.ActorSettings.SpinEvery - 0.5 - 0.10000000149011612)
            break;
          this.OriginalRotation = this.ArtObject.Rotation;
          this.SinceChanged -= TimeSpan.FromSeconds((double) this.ArtObject.ActorSettings.SpinEvery - 0.5 - 0.10000000149011612);
          this.State = SpinBlocksHost.SpinState.Warning;
          Vector3 right = this.CameraManager.InverseView.Right;
          Vector3 interpolatedCenter = this.CameraManager.InterpolatedCenter;
          float num1 = new Vector2()
          {
            X = (this.ArtObject.Position - interpolatedCenter).Dot(right),
            Y = (interpolatedCenter.Y - this.ArtObject.Position.Y)
          }.Length();
          if (((double) num1 > 10.0 ? 0.60000002384185791 / (((double) num1 - 10.0) / 5.0 + 1.0) : 1.0 - (double) Easing.EaseIn((double) num1 / 10.0, EasingType.Quadratic) * 0.40000000596046448) <= 0.05000000074505806)
            break;
          this.Emitter = this.SoundEffect.EmitAt(this.ArtObject.Position, RandomHelper.Centered(0.079999998211860657));
          if (!this.IsRotato)
            break;
          this.Emitter.PauseViewTransitions = false;
          break;
        case SpinBlocksHost.SpinState.Warning:
          float num2 = (float) Math.Sin(FezMath.Saturate(this.SinceChanged.TotalSeconds / 0.10000000149011612) * 0.78539818525314331);
          Quaternion fromAxisAngle1 = Quaternion.CreateFromAxisAngle(this.ArtObject.ActorSettings.SpinView.ForwardVector(), (float) (-1.5707963705062866 * (double) num2 * 0.10000000149011612));
          this.ArtObject.Rotation = fromAxisAngle1 * this.OriginalRotation;
          this.ArtObject.Position = this.OriginalPosition + Vector3.Transform(-this.RotationOffset, this.SpinAccumulatedRotation * fromAxisAngle1) + this.RotationOffset;
          if (this.SinceChanged.TotalSeconds < 0.10000000149011612)
            break;
          this.SinceChanged -= TimeSpan.FromSeconds(0.10000000149011612);
          this.State = SpinBlocksHost.SpinState.Spinning;
          break;
        case SpinBlocksHost.SpinState.Spinning:
          double num3 = FezMath.Saturate(this.SinceChanged.TotalSeconds / 0.5);
          float num4 = this.IsRotato ? Easing.EaseInOut(FezMath.Saturate(num3 / 0.75), EasingType.Quartic, EasingType.Quadratic) : Easing.EaseIn(num3 < 0.75 ? num3 / 0.75 : 1.0 + Math.Sin((num3 - 0.75) / 0.25 * 6.2831854820251465) * 0.014999999664723873, EasingType.Quintic);
          bool flag1 = this.PlayerManager.Grounded && this.Triles.Contains(this.PlayerManager.Ground.First);
          if (flag1)
          {
            if (!this.IsRotato)
            {
              IPlayerManager playerManager = this.PlayerManager;
              playerManager.Velocity = playerManager.Velocity + this.ArtObject.ActorSettings.SpinView.RightVector() * num4 * 0.1f;
              if ((double) num4 > 0.25)
                this.PlayerManager.Position -= 0.0100000007f * Vector3.UnitY;
            }
            else if ((double) num4 > 0.0 && !this.hasRotated)
            {
              this.PlayerManager.IsOnRotato = true;
              this.Rotate();
              this.OriginalPlayerPosition = this.PlayerManager.Position;
              this.CameraManager.ChangeViewpoint(this.CameraManager.Viewpoint.GetRotatedView(this.ArtObject.ActorSettings.SpinView == Viewpoint.Up ? 1 : -1), 0.5f);
              this.hasRotated = true;
            }
          }
          if (!this.IsRotato)
          {
            foreach (TrileInstance trile in this.Triles)
              trile.Enabled = (double) num4 <= 0.25;
          }
          bool flag2 = this.PlayerManager.Action.IsOnLedge() && this.Triles.Contains(this.PlayerManager.HeldInstance);
          if (flag2)
          {
            if (!this.IsRotato)
            {
              IPlayerManager playerManager = this.PlayerManager;
              playerManager.Velocity = playerManager.Velocity + this.ArtObject.ActorSettings.SpinView.RightVector() * num4 * 0.1f;
              if ((double) num4 > 0.25)
              {
                this.PlayerManager.Action = ActionType.Falling;
                this.PlayerManager.HeldInstance = (TrileInstance) null;
              }
            }
            else if ((double) num4 > 0.0 && (double) num4 < 0.5 && !this.hasRotated)
            {
              this.PlayerManager.IsOnRotato = true;
              this.Rotate();
              this.OriginalPlayerPosition = this.PlayerManager.Position;
              this.CameraManager.ChangeViewpoint(this.CameraManager.Viewpoint.GetRotatedView(this.ArtObject.ActorSettings.SpinView == Viewpoint.Up ? 1 : -1), 0.5f);
              this.hasRotated = true;
            }
          }
          this.TrixelParticleSystems.PropagateEnergy(this.ArtObject.Position - this.ArtObject.ActorSettings.SpinView.RightVector(), num4 * 0.1f);
          Quaternion fromAxisAngle2 = Quaternion.CreateFromAxisAngle(this.ArtObject.ActorSettings.SpinView.ForwardVector(), (float) (1.5707963705062866 * (double) num4 * 1.1000000238418579 - 0.15707963705062866));
          this.ArtObject.Rotation = fromAxisAngle2 * this.OriginalRotation;
          this.ArtObject.Position = this.OriginalPosition + Vector3.Transform(-this.RotationOffset, this.SpinAccumulatedRotation * fromAxisAngle2) + this.RotationOffset;
          if (this.IsRotato && flag1 | flag2)
          {
            Vector3 vector3 = this.ArtObject.ActorSettings.RotationCenter;
            if (!this.ArtObject.ActorSettings.OffCenter)
              vector3 = this.ArtObject.Position;
            this.PlayerManager.Position = Vector3.Transform(this.OriginalPlayerPosition - vector3, fromAxisAngle2) + vector3;
          }
          if (this.SinceChanged.TotalSeconds < 0.5)
            break;
          foreach (TrileInstance trile in this.Triles)
            trile.Enabled = true;
          if ((!this.IsRotato || !this.hasRotated) && (double) this.Triles.Count != (double) this.ArtObject.ArtObject.Size.X * (double) this.ArtObject.ArtObject.Size.Y * (double) this.ArtObject.ArtObject.Size.Z)
            this.Rotate();
          this.SpinAccumulatedRotation *= fromAxisAngle2;
          this.State = SpinBlocksHost.SpinState.Idle;
          this.hasRotated = false;
          this.SinceChanged -= TimeSpan.FromSeconds(0.5);
          if (!this.IsRotato || !(flag1 | flag2))
            break;
          this.PlayerManager.IsOnRotato = false;
          break;
      }
    }

    private void Rotate()
    {
      Vector3 vector3 = this.ArtObject.ActorSettings.RotationCenter;
      if (!this.ArtObject.ActorSettings.OffCenter)
        vector3 = this.ArtObject.Position;
      Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(this.ArtObject.ActorSettings.SpinView.ForwardVector(), 1.57079637f);
      TrileInstance[] array = this.Triles.ToArray();
      foreach (TrileInstance instance in array)
      {
        Vector3 a = Vector3.Transform(instance.Position + FezMath.HalfVector - vector3, fromAxisAngle) + vector3 - FezMath.HalfVector;
        if (!FezMath.AlmostEqual(a, instance.Position))
        {
          this.LevelManager.ClearTrile(instance, true);
          instance.Position = a;
        }
      }
      foreach (TrileInstance instance in array)
        this.LevelManager.UpdateInstance(instance);
    }

    [ServiceDependency]
    public ILevelManager LevelManager { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public IDefaultCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public ISoundManager SoundManager { private get; set; }

    [ServiceDependency]
    public ITrixelParticleSystems TrixelParticleSystems { private get; set; }
  }

  private enum SpinState
  {
    Idle,
    Warning,
    Spinning,
  }
}
