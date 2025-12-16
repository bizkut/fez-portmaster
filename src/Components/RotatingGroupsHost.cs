// Decompiled with JetBrains decompiler
// Type: FezGame.Components.RotatingGroupsHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
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

internal class RotatingGroupsHost(Game game) : GameComponent(game), IRotatingGroupService, IScriptingBase
{
  private readonly List<RotatingGroupsHost.RotatingGroupState> RotatingGroups = new List<RotatingGroupsHost.RotatingGroupState>();

  public override void Initialize()
  {
    base.Initialize();
    this.TryInitialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
  }

  private void TryInitialize()
  {
    this.RotatingGroups.Clear();
    this.RotatingGroups.AddRange(this.LevelManager.Groups.Values.Where<TrileGroup>((Func<TrileGroup, bool>) (x => x.ActorType == ActorType.RotatingGroup)).Select<TrileGroup, RotatingGroupsHost.RotatingGroupState>((Func<TrileGroup, RotatingGroupsHost.RotatingGroupState>) (x => new RotatingGroupsHost.RotatingGroupState(x))));
    this.Enabled = this.RotatingGroups.Count > 0;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || this.GameState.InMenuCube || this.GameState.InFpsMode)
      return;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    foreach (RotatingGroupsHost.RotatingGroupState rotatingGroup in this.RotatingGroups)
      rotatingGroup.Update(totalSeconds);
  }

  public void ResetEvents()
  {
  }

  public void Rotate(int id, bool clockwise, int turns)
  {
    if (!this.Enabled)
      return;
    foreach (RotatingGroupsHost.RotatingGroupState rotatingGroup in this.RotatingGroups)
    {
      if (rotatingGroup.Group.Id == id)
      {
        RotatingGroupsHost.RotatingGroupState cached = rotatingGroup;
        Waiters.Wait((Func<bool>) (() => cached.Action == SpinAction.Idle), (Action) (() =>
        {
          cached.Rotate(clockwise, turns);
          cached.SinceChanged = 0.0f;
        }));
      }
    }
  }

  public void SetEnabled(int id, bool enabled)
  {
    if (!this.Enabled)
      return;
    foreach (RotatingGroupsHost.RotatingGroupState rotatingGroup in this.RotatingGroups)
    {
      if (rotatingGroup.Group.Id == id)
      {
        rotatingGroup.Enabled = enabled;
        rotatingGroup.SinceChanged = 0.0f;
      }
    }
  }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  private class RotatingGroupState
  {
    private const float SpinTime = 0.75f;
    public readonly TrileGroup Group;
    private readonly Vector3 Center;
    private readonly ArtObjectInstance[] AttachedArtObjects;
    private readonly Vector3[] AttachedAoOrigins;
    private readonly Quaternion[] AttachedAoRotations;
    private readonly TrileMaterializer[] CachedMaterializers;
    private readonly List<TrileInstance> TopLayer = new List<TrileInstance>();
    private readonly HashSet<Point> RecullAtPoints = new HashSet<Point>();
    public bool Enabled;
    public float SinceChanged;
    private SoundEffect sSpin;
    private Vector3 OriginalForward;
    private Vector4[] OriginalStates;
    private Vector3 OriginalPlayerPosition;
    private int SpinSign;
    private int Turns;
    private bool HeldOnto;
    private bool GroundedOn;

    public SpinAction Action { get; private set; }

    public RotatingGroupState(TrileGroup group)
    {
      ServiceHelper.InjectServices((object) this);
      this.Group = group;
      this.AttachedArtObjects = this.LevelManager.ArtObjects.Values.Where<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x =>
      {
        int? attachedGroup = x.ActorSettings.AttachedGroup;
        int id = this.Group.Id;
        return attachedGroup.GetValueOrDefault() == id && attachedGroup.HasValue;
      })).ToArray<ArtObjectInstance>();
      this.AttachedAoOrigins = ((IEnumerable<ArtObjectInstance>) this.AttachedArtObjects).Select<ArtObjectInstance, Vector3>((Func<ArtObjectInstance, Vector3>) (x => x.Position)).ToArray<Vector3>();
      this.AttachedAoRotations = ((IEnumerable<ArtObjectInstance>) this.AttachedArtObjects).Select<ArtObjectInstance, Quaternion>((Func<ArtObjectInstance, Quaternion>) (x => x.Rotation)).ToArray<Quaternion>();
      this.CachedMaterializers = group.Triles.Select<TrileInstance, TrileMaterializer>((Func<TrileInstance, TrileMaterializer>) (x => this.LevelMaterializer.GetTrileMaterializer(x.Trile))).ToArray<TrileMaterializer>();
      foreach (TrileInstance trile in this.Group.Triles)
      {
        trile.ForceSeeThrough = true;
        trile.Unsafe = true;
      }
      float num = this.Group.Triles.Where<TrileInstance>((Func<TrileInstance, bool>) (x => !x.Trile.Immaterial)).Max<TrileInstance>((Func<TrileInstance, float>) (x => x.Position.Y));
      foreach (TrileInstance trile in this.Group.Triles)
      {
        if ((double) trile.Position.Y == (double) num)
          this.TopLayer.Add(trile);
      }
      if (this.Group.SpinCenter != Vector3.Zero)
      {
        this.Center = this.Group.SpinCenter;
      }
      else
      {
        foreach (TrileInstance trile in this.Group.Triles)
          this.Center += trile.Position + FezMath.HalfVector;
        this.Center /= (float) this.Group.Triles.Count;
      }
      this.Enabled = !this.Group.SpinNeedsTriggering;
      this.SinceChanged = -this.Group.SpinOffset;
      if ((double) this.SinceChanged != 0.0)
        this.SinceChanged -= 0.375f;
      if (string.IsNullOrEmpty(group.AssociatedSound))
        return;
      try
      {
        this.sSpin = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/" + group.AssociatedSound);
      }
      catch (Exception ex)
      {
        Logger.Log("RotatingGroups", LogSeverity.Warning, $"Could not find associated sound '{group.AssociatedSound}'");
      }
    }

    public void Update(float elapsedSeconds)
    {
      if (!this.Enabled && this.Action == SpinAction.Idle)
        return;
      this.SinceChanged += elapsedSeconds;
      float spinFrequency = this.Group.SpinFrequency;
      switch (this.Action)
      {
        case SpinAction.Idle:
          if ((double) this.SinceChanged < (double) spinFrequency)
            break;
          this.SinceChanged -= spinFrequency;
          this.Rotate(this.Group.SpinClockwise, this.Group.Spin180Degrees ? 2 : 1);
          break;
        case SpinAction.Spinning:
          float num = Easing.EaseInOut((double) FezMath.Saturate(FezMath.Saturate(this.SinceChanged / (0.75f * (float) this.Turns)) / 0.75f), EasingType.Quartic, EasingType.Quadratic);
          float angle = num * 1.57079637f * (float) this.SpinSign * (float) this.Turns;
          Matrix fromAxisAngle1 = Matrix.CreateFromAxisAngle(Vector3.UnitY, angle);
          Quaternion fromAxisAngle2 = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
          if (!this.PlayerManager.IsOnRotato && (double) num < 0.5)
          {
            this.HeldOnto |= this.Group.Triles.Contains(this.PlayerManager.HeldInstance);
            this.GroundedOn = ((this.GroundedOn ? 1 : 0) | (!this.PlayerManager.Grounded ? 0 : (this.TopLayer.Contains(this.PlayerManager.Ground.First) ? 1 : 0))) != 0;
            if (this.GroundedOn || this.HeldOnto)
            {
              this.OriginalPlayerPosition = this.PlayerManager.Position;
              this.PlayerManager.IsOnRotato = true;
            }
          }
          if ((this.GroundedOn || this.HeldOnto) && (double) num > 0.10000000149011612 && !this.CameraManager.ForceTransition)
          {
            if (this.HeldOnto && this.Group.FallOnRotate)
            {
              this.PlayerManager.Action = ActionType.Idle;
              this.PlayerManager.HeldInstance = (TrileInstance) null;
              this.PlayerManager.IsOnRotato = false;
              this.HeldOnto = false;
            }
            else
            {
              this.CameraManager.ForceTransition = true;
              this.CameraManager.ChangeViewpoint(this.CameraManager.Viewpoint.GetRotatedView(this.SpinSign * this.Turns), -1f);
              this.CameraManager.Direction = -this.CameraManager.LastViewpoint.ForwardVector();
              this.CameraManager.RebuildView();
            }
          }
          Vector3 vector3_1 = new Vector3(this.Center.X - 0.5f, this.Center.Y - 0.5f, this.Center.Z - 0.5f);
          for (int index = 0; index < this.OriginalStates.Length; ++index)
          {
            TrileInstance trile = this.Group.Triles[index];
            Vector4 originalState = this.OriginalStates[index];
            Vector3 vector3_2 = Vector3.Transform(new Vector3(originalState.X, originalState.Y, originalState.Z), fromAxisAngle1);
            trile.Position = new Vector3(vector3_2.X + vector3_1.X, vector3_2.Y + vector3_1.Y, vector3_2.Z + vector3_1.Z);
            trile.SetPhiLight(originalState.W + angle);
            this.CachedMaterializers[index].UpdateInstance(trile);
          }
          for (int index = 0; index < this.AttachedArtObjects.Length; ++index)
          {
            this.AttachedArtObjects[index].Rotation = this.AttachedAoRotations[index] * fromAxisAngle2;
            this.AttachedArtObjects[index].Position = Vector3.Transform(this.AttachedAoOrigins[index] - this.Center, fromAxisAngle1) + this.Center;
          }
          if (this.GroundedOn || this.HeldOnto)
          {
            Vector3 position = this.PlayerManager.Position;
            Vector3 vector3_3 = Vector3.Transform(this.OriginalPlayerPosition - this.Center, fromAxisAngle1) + this.Center;
            if (!this.HeldOnto || !this.Group.FallOnRotate)
            {
              this.CameraManager.Center += vector3_3 - position;
              this.CameraManager.Direction = Vector3.Transform(-this.OriginalForward, fromAxisAngle1);
            }
            this.PlayerManager.Position += vector3_3 - position;
          }
          if ((double) this.SinceChanged < 0.75 * (double) this.Turns)
            break;
          if (this.GroundedOn || this.HeldOnto)
          {
            this.PlayerManager.IsOnRotato = false;
            this.RotateTriles();
            this.CameraManager.ForceTransition = false;
            this.PlayerManager.ForceOverlapsDetermination();
          }
          else
            this.RotateTriles();
          this.SinceChanged -= 0.75f;
          this.Action = SpinAction.Idle;
          break;
      }
    }

    public void Rotate(bool clockwise, int turns)
    {
      this.SpinSign = clockwise ? 1 : -1;
      this.Turns = turns;
      foreach (TrileInstance trile in this.Group.Triles)
      {
        this.LevelMaterializer.UnregisterViewedInstance(trile);
        if (trile.InstanceId == -1)
          this.LevelMaterializer.CullInstanceInNoRegister(trile);
        trile.SkipCulling = true;
      }
      this.LevelMaterializer.CommitBatchesIfNeeded();
      this.RecordStates();
      for (int index = 0; index < this.AttachedArtObjects.Length; ++index)
      {
        this.AttachedAoRotations[index] = this.AttachedArtObjects[index].Rotation;
        this.AttachedAoOrigins[index] = this.AttachedArtObjects[index].Position;
      }
      this.HeldOnto = this.Group.Triles.Contains(this.PlayerManager.HeldInstance);
      this.GroundedOn = this.PlayerManager.Grounded && this.TopLayer.Contains(this.PlayerManager.Ground.First);
      if (this.GroundedOn || this.HeldOnto)
        this.PlayerManager.IsOnRotato = true;
      this.OriginalForward = this.CameraManager.Viewpoint.ForwardVector();
      this.OriginalPlayerPosition = this.PlayerManager.Position;
      this.Action = SpinAction.Spinning;
      if (this.sSpin == null)
        return;
      this.sSpin.EmitAt(this.Center, false, RandomHelper.Centered(0.10000000149011612), false).FadeDistance = 50f;
    }

    private void RecordStates()
    {
      this.OriginalStates = this.Group.Triles.Select<TrileInstance, Vector4>((Func<TrileInstance, Vector4>) (x => new Vector4(x.Position + FezMath.HalfVector - this.Center, x.Phi))).ToArray<Vector4>();
    }

    private void RotateTriles()
    {
      float angle = 1.57079637f * (float) this.SpinSign * (float) this.Turns;
      Matrix fromAxisAngle = Matrix.CreateFromAxisAngle(Vector3.UnitY, angle);
      this.RecullAtPoints.Clear();
      bool flag = (double) this.CameraManager.Viewpoint.SideMask().X != 0.0;
      Vector3 vector3 = new Vector3(this.Center.X - 0.5f, this.Center.Y - 0.5f, this.Center.Z - 0.5f);
      for (int index = 0; index < this.OriginalStates.Length; ++index)
      {
        TrileInstance trile = this.Group.Triles[index];
        Vector4 originalState = this.OriginalStates[index];
        trile.Position = Vector3.Transform(originalState.XYZ(), fromAxisAngle) + vector3;
        trile.Phi = FezMath.WrapAngle(originalState.W + angle);
        this.LevelManager.UpdateInstance(trile);
        trile.SkipCulling = false;
        this.RecullAtPoints.Add(new Point(flag ? trile.Emplacement.X : trile.Emplacement.Z, trile.Emplacement.Y));
      }
      foreach (Point recullAtPoint in this.RecullAtPoints)
        this.LevelManager.RecullAt(recullAtPoint, true);
      this.LevelMaterializer.CommitBatchesIfNeeded();
    }

    [ServiceDependency]
    public IGameLevelManager LevelManager { private get; set; }

    [ServiceDependency]
    public ILevelMaterializer LevelMaterializer { private get; set; }

    [ServiceDependency]
    public IDefaultCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public IContentManagerProvider CMProvider { private get; set; }
  }
}
