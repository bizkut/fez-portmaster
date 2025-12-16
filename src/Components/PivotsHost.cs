// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PivotsHost
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

public class PivotsHost(Game game) : GameComponent(game)
{
  private readonly List<PivotsHost.PivotState> TrackedPivots = new List<PivotsHost.PivotState>();
  private SoundEffect LeftSound;
  private SoundEffect RightSound;

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanging += new Action(this.TryInitialize);
    this.TryInitialize();
    this.LeftSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/PivotLeft");
    this.RightSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Industrial/PivotRight");
  }

  private void TryInitialize()
  {
    this.TrackedPivots.Clear();
    foreach (ArtObjectInstance handleAo in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      if (handleAo.ArtObject.ActorType == ActorType.PivotHandle || handleAo.ArtObject.ActorType == ActorType.Bookcase)
        this.TrackedPivots.Add(new PivotsHost.PivotState(this, handleAo));
    }
    if (this.TrackedPivots.Count <= 0)
      return;
    this.LevelMaterializer.CullInstances();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Loading || this.EngineState.Paused || this.GameState.InMap || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning)
      return;
    float num1 = float.MaxValue;
    PivotsHost.PivotState pivotState = (PivotsHost.PivotState) null;
    foreach (PivotsHost.PivotState trackedPivot in this.TrackedPivots)
    {
      if (trackedPivot.Update(gameTime.ElapsedGameTime))
      {
        float num2 = trackedPivot.HandleAo.Position.Dot(this.CameraManager.Viewpoint.ForwardVector());
        if ((double) num2 < (double) num1)
        {
          pivotState = trackedPivot;
          num1 = num2;
        }
      }
    }
    pivotState?.Spin();
  }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  private class PivotState
  {
    private const float SpinTime = 1.5f;
    private readonly PivotsHost Host;
    private readonly TrileGroup Group;
    public readonly ArtObjectInstance HandleAo;
    private readonly ArtObjectInstance[] AttachedArtObjects;
    private readonly Vector3[] AttachedAoOrigins;
    private readonly Quaternion[] AttachedAoRotations;
    private readonly List<TrileInstance> TopLayer = new List<TrileInstance>();
    private readonly List<TrileInstance> AttachedTriles = new List<TrileInstance>();
    private Vector4[] OriginalStates;
    private Quaternion OriginalAoRotation;
    private SpinAction State;
    private TimeSpan SinceChanged;
    private int SpinSign;
    private bool HasShaken;

    public PivotState(PivotsHost host, ArtObjectInstance handleAo)
    {
      ServiceHelper.InjectServices((object) this);
      this.Host = host;
      this.HandleAo = handleAo;
      this.Group = this.LevelManager.Groups[handleAo.ActorSettings.AttachedGroup.Value];
      this.AttachedArtObjects = this.LevelManager.ArtObjects.Values.Where<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x =>
      {
        int? attachedGroup = x.ActorSettings.AttachedGroup;
        int id = this.Group.Id;
        return (attachedGroup.GetValueOrDefault() == id ? (attachedGroup.HasValue ? 1 : 0) : 0) != 0 && x != this.HandleAo;
      })).ToArray<ArtObjectInstance>();
      this.AttachedAoOrigins = ((IEnumerable<ArtObjectInstance>) this.AttachedArtObjects).Select<ArtObjectInstance, Vector3>((Func<ArtObjectInstance, Vector3>) (x => x.Position)).ToArray<Vector3>();
      this.AttachedAoRotations = ((IEnumerable<ArtObjectInstance>) this.AttachedArtObjects).Select<ArtObjectInstance, Quaternion>((Func<ArtObjectInstance, Quaternion>) (x => x.Rotation)).ToArray<Quaternion>();
      foreach (TrileInstance trile in this.Group.Triles)
        trile.ForceSeeThrough = true;
      float num = this.Group.Triles.Where<TrileInstance>((Func<TrileInstance, bool>) (x => !x.Trile.Immaterial)).Max<TrileInstance>((Func<TrileInstance, float>) (x => x.Position.Y));
      foreach (TrileInstance trile in this.Group.Triles)
      {
        if ((double) trile.Position.Y == (double) num)
          this.TopLayer.Add(trile);
      }
      if (this.LevelManager.Name == "WATER_TOWER" && this.LevelManager.LastLevelName == "LIGHTHOUSE")
      {
        if (this.GameState.SaveData.ThisLevel.PivotRotations.ContainsKey(handleAo.Id))
          this.GameState.SaveData.ThisLevel.PivotRotations[handleAo.Id] = 0;
      }
      else
      {
        int initialSpins;
        if (this.GameState.SaveData.ThisLevel.PivotRotations.TryGetValue(handleAo.Id, out initialSpins) && initialSpins != 0)
          this.ForceSpinTo(initialSpins);
      }
      if (!this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(this.HandleAo.Id))
        return;
      this.HandleAo.Enabled = false;
    }

    private void ForceSpinTo(int initialSpins)
    {
      int num = Math.Abs(initialSpins);
      for (int index1 = 0; index1 < num; ++index1)
      {
        this.OriginalAoRotation = this.HandleAo.Rotation;
        this.AttachedTriles.Clear();
        foreach (TrileInstance instance in this.TopLayer)
          this.AddSupportedTrilesOver(instance);
        this.OriginalStates = this.Group.Triles.Union<TrileInstance>((IEnumerable<TrileInstance>) this.AttachedTriles).Select<TrileInstance, Vector4>((Func<TrileInstance, Vector4>) (x => new Vector4(x.Position, x.Phi))).ToArray<Vector4>();
        float angle = 1.57079637f * (float) Math.Sign(initialSpins);
        Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
        Vector3 position = this.HandleAo.Position;
        for (int index2 = 0; index2 < this.AttachedArtObjects.Length; ++index2)
        {
          this.AttachedAoRotations[index2] = this.AttachedArtObjects[index2].Rotation;
          this.AttachedAoOrigins[index2] = this.AttachedArtObjects[index2].Position;
          this.AttachedArtObjects[index2].Rotation = this.AttachedAoRotations[index2] * fromAxisAngle;
          this.AttachedArtObjects[index2].Position = Vector3.Transform(this.AttachedAoOrigins[index2] - position, fromAxisAngle) + position;
        }
        for (int index3 = 0; index3 < this.OriginalStates.Length; ++index3)
        {
          TrileInstance instance = index3 < this.Group.Triles.Count ? this.Group.Triles[index3] : this.AttachedTriles[index3 - this.Group.Triles.Count];
          Vector4 originalState = this.OriginalStates[index3];
          instance.Position = Vector3.Transform(originalState.XYZ() + new Vector3(0.5f) - position, fromAxisAngle) + position - new Vector3(0.5f);
          instance.Phi = FezMath.WrapAngle(originalState.W + angle);
          this.LevelMaterializer.GetTrileMaterializer(instance.Trile).UpdateInstance(instance);
        }
        this.RotateTriles();
      }
      this.HandleAo.Rotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, 1.57079637f * (float) initialSpins);
    }

    public bool Update(TimeSpan elapsed)
    {
      this.SinceChanged += elapsed;
      switch (this.State)
      {
        case SpinAction.Idle:
          bool flag;
          if (this.HandleAo.ArtObject.ActorType == ActorType.Bookcase)
          {
            Vector3 vector3 = this.PlayerManager.Position - this.HandleAo.Position;
            flag = (double) vector3.Z < 2.75 && (double) vector3.Z > -0.25 && (double) vector3.Y < -3.0 && (double) vector3.Y > -4.0 && this.CameraManager.Viewpoint == Viewpoint.Left;
          }
          else
          {
            Vector3 vector = (this.PlayerManager.Position - (this.HandleAo.Position - new Vector3(0.0f, 1.5f, 0.0f))) * this.CameraManager.Viewpoint.ScreenSpaceMask();
            vector.X += vector.Z;
            Vector3 vector3 = vector.Abs();
            flag = (double) vector3.X > 0.75 && (double) vector3.X < 1.75 && (double) vector3.Y < 1.0;
          }
          if (this.HandleAo.Enabled & flag && this.PlayerManager.Grounded && this.PlayerManager.Action != ActionType.PushingPivot && this.InputManager.GrabThrow.IsDown() && this.PlayerManager.Action != ActionType.ReadingSign && this.PlayerManager.Action != ActionType.FreeFalling && this.PlayerManager.Action != ActionType.Dying)
          {
            this.SinceChanged = TimeSpan.Zero;
            return true;
          }
          break;
        case SpinAction.Spinning:
          double num = FezMath.Saturate(this.SinceChanged.TotalSeconds / 1.5);
          float angle = Easing.EaseIn(num < 0.800000011920929 ? num / 0.800000011920929 : 1.0 + Math.Sin((num - 0.800000011920929) / 0.20000000298023224 * 6.2831854820251465 * 2.0) * 0.0099999997764825821 * (1.0 - num) / 0.20000000298023224, EasingType.Quartic) * 1.57079637f * (float) this.SpinSign;
          Quaternion fromAxisAngle = Quaternion.CreateFromAxisAngle(Vector3.UnitY, angle);
          Vector3 position = this.HandleAo.Position;
          for (int index = 0; index < this.OriginalStates.Length; ++index)
          {
            TrileInstance instance = index < this.Group.Triles.Count ? this.Group.Triles[index] : this.AttachedTriles[index - this.Group.Triles.Count];
            Vector4 originalState = this.OriginalStates[index];
            instance.Position = Vector3.Transform(originalState.XYZ() + new Vector3(0.5f) - position, fromAxisAngle) + position - new Vector3(0.5f);
            instance.Phi = FezMath.WrapAngle(originalState.W + angle);
            this.LevelMaterializer.GetTrileMaterializer(instance.Trile).UpdateInstance(instance);
          }
          if (!this.HasShaken && num > 0.800000011920929)
          {
            ServiceHelper.AddComponent((IGameComponent) new CamShake(ServiceHelper.Game)
            {
              Distance = 0.25f,
              Duration = TimeSpan.FromSeconds(0.20000000298023224)
            });
            this.HasShaken = true;
          }
          this.HandleAo.Rotation = this.OriginalAoRotation * fromAxisAngle;
          for (int index = 0; index < this.AttachedArtObjects.Length; ++index)
          {
            this.AttachedArtObjects[index].Rotation = this.AttachedAoRotations[index] * fromAxisAngle;
            this.AttachedArtObjects[index].Position = Vector3.Transform(this.AttachedAoOrigins[index] - position, fromAxisAngle) + position;
          }
          if (this.SinceChanged.TotalSeconds >= 1.5)
          {
            this.RotateTriles();
            this.SinceChanged -= TimeSpan.FromSeconds(1.5);
            this.State = SpinAction.Idle;
            break;
          }
          break;
      }
      return false;
    }

    public void Spin()
    {
      this.PlayerManager.Action = ActionType.PushingPivot;
      Waiters.Wait(0.5, (Func<float, bool>) (_ => this.PlayerManager.Action != ActionType.PushingPivot), (Action) (() =>
      {
        if (this.PlayerManager.Action != ActionType.PushingPivot)
          return;
        this.SinceChanged = TimeSpan.Zero;
        this.OriginalAoRotation = this.HandleAo.Rotation;
        foreach (TrileInstance trile in this.Group.Triles)
        {
          if (trile.InstanceId == -1)
            this.LevelMaterializer.CullInstanceIn(trile);
        }
        Vector3 vector3 = (this.PlayerManager.Position - (this.HandleAo.Position - new Vector3(0.0f, 1.5f, 0.0f))) * this.CameraManager.Viewpoint.ScreenSpaceMask();
        vector3.X += vector3.Z;
        this.SpinSign = this.HandleAo.ArtObject.ActorType != ActorType.Bookcase ? (int) ((double) this.CameraManager.Viewpoint.RightVector().Sign().Dot(Vector3.One) * (double) Math.Sign(vector3.X)) : 1;
        if (this.SpinSign == 1)
          this.Host.RightSound.Emit();
        else
          this.Host.LeftSound.Emit();
        this.AttachedTriles.Clear();
        foreach (TrileInstance instance in this.TopLayer)
          this.AddSupportedTrilesOver(instance);
        this.OriginalStates = this.Group.Triles.Union<TrileInstance>((IEnumerable<TrileInstance>) this.AttachedTriles).Select<TrileInstance, Vector4>((Func<TrileInstance, Vector4>) (x => new Vector4(x.Position, x.Phi))).ToArray<Vector4>();
        for (int index = 0; index < this.AttachedArtObjects.Length; ++index)
        {
          this.AttachedAoRotations[index] = this.AttachedArtObjects[index].Rotation;
          this.AttachedAoOrigins[index] = this.AttachedArtObjects[index].Position;
        }
        int num;
        if (!this.GameState.SaveData.ThisLevel.PivotRotations.TryGetValue(this.HandleAo.Id, out num))
          this.GameState.SaveData.ThisLevel.PivotRotations.Add(this.HandleAo.Id, this.SpinSign);
        else
          this.GameState.SaveData.ThisLevel.PivotRotations[this.HandleAo.Id] = num + this.SpinSign;
        if (this.SpinSign == 1)
          this.PivotService.OnRotateRight(this.HandleAo.Id);
        else
          this.PivotService.OnRotateLeft(this.HandleAo.Id);
        this.HasShaken = false;
        this.State = SpinAction.Spinning;
        if (this.HandleAo.ArtObject.ActorType != ActorType.Bookcase)
          return;
        this.HandleAo.Enabled = false;
        this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(this.HandleAo.Id);
      }));
    }

    private void AddSupportedTrilesOver(TrileInstance instance)
    {
      TrileEmplacement id = new TrileEmplacement(instance.Emplacement.X, instance.Emplacement.Y + 1, instance.Emplacement.Z);
      TrileInstance instance1 = this.LevelManager.TrileInstanceAt(ref id);
      if (instance1 == null)
        return;
      this.AddSupportedTrile(instance1);
      if (!instance1.Overlaps)
        return;
      foreach (TrileInstance overlappedTrile in instance1.OverlappedTriles)
        this.AddSupportedTrile(overlappedTrile);
    }

    private void AddSupportedTrile(TrileInstance instance)
    {
      if (this.AttachedTriles.Contains(instance) || this.Group.Triles.Contains(instance) || instance.PhysicsState == null && !instance.Trile.ActorSettings.Type.IsPickable())
        return;
      this.AttachedTriles.Add(instance);
      this.AddSupportedTrilesOver(instance);
      TrileGroup trileGroup;
      if (!this.LevelManager.PickupGroups.TryGetValue(instance, out trileGroup))
        return;
      foreach (TrileInstance trile in trileGroup.Triles)
        this.AddSupportedTrile(trile);
    }

    private void RotateTriles()
    {
      foreach (TrileInstance instance in this.Group.Triles.Union<TrileInstance>((IEnumerable<TrileInstance>) this.AttachedTriles).ToArray<TrileInstance>())
        this.LevelManager.UpdateInstance(instance);
      if (!this.LevelManager.Groups.ContainsKey(this.Group.Id))
        throw new InvalidOperationException("Group was lost after pivot rotation!");
    }

    [ServiceDependency]
    public IPivotService PivotService { private get; set; }

    [ServiceDependency]
    public IGameLevelManager LevelManager { private get; set; }

    [ServiceDependency]
    public ILevelMaterializer LevelMaterializer { private get; set; }

    [ServiceDependency]
    public IInputManager InputManager { private get; set; }

    [ServiceDependency]
    public IDefaultCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public IPlayerManager PlayerManager { private get; set; }

    [ServiceDependency]
    public IGameStateManager GameState { private get; set; }
  }
}
