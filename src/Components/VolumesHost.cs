// Decompiled with JetBrains decompiler
// Type: FezGame.Components.VolumesHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Components.Scripting;
using FezGame.Services;
using FezGame.Services.Scripting;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class VolumesHost(Game game) : GameComponent(game)
{
  private Volume[] levelVolumes;
  private readonly List<CodeInput> Input = new List<CodeInput>();
  private TimeSpan SinceInput;
  private bool deferredScripts;
  private bool checkForContainment;
  private bool pendingCheck;

  public override void Initialize()
  {
    this.RegisterVolumes();
    this.TestVolumes(false);
    this.LevelManager.LevelChanged += new Action(this.DisableTriggeredVolumes);
    this.LevelManager.LevelChanged += new Action(this.RegisterVolumes);
    this.LevelManager.LevelChanged += (Action) (() => this.TestVolumes(true));
  }

  private void DisableTriggeredVolumes()
  {
    foreach (Volume volume in (IEnumerable<Volume>) this.LevelManager.Volumes.Values)
    {
      if (volume.ActorSettings != null && volume.ActorSettings.NeedsTrigger)
        volume.Enabled = false;
    }
    foreach (int inactiveVolume in this.GameState.SaveData.ThisLevel.InactiveVolumes)
    {
      Volume volume;
      if ((!(this.LevelManager.Name == "ZU_CITY_RUINS") || inactiveVolume != 2) && (!(this.LevelManager.Name == "TELESCOPE") || inactiveVolume != 4 || this.GameState.SaveData.ThisLevel.DestroyedTriles.Contains(new TrileEmplacement(18, 36, 20))) && this.LevelManager.Volumes.TryGetValue(inactiveVolume, out volume))
        volume.Enabled = !volume.Enabled;
    }
    this.checkForContainment = this.LevelManager.Name == "RITUAL";
    this.Input.Clear();
    this.pendingCheck = false;
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.InMap || this.GameState.Paused || this.GameState.Loading)
      return;
    this.pendingCheck |= this.GrabInput();
    if (this.LevelManager.IsInvalidatingScreen)
      return;
    if (this.LevelManager.Volumes.Count != this.levelVolumes.Length || this.VolumeService.RegisterNeeded)
      this.RegisterVolumes();
    if (this.levelVolumes.Length == 0)
      return;
    this.TestVolumes(false);
    this.SinceInput += gameTime.ElapsedGameTime;
  }

  private void HeightCheck()
  {
    SoundService.ImmediateEffect = true;
    foreach (Volume levelVolume in this.levelVolumes)
    {
      bool flag = (double) this.CameraManager.Center.Y > ((double) levelVolume.From.Y + (double) levelVolume.To.Y) / 2.0;
      if (flag)
        this.VolumeService.OnGoHigher(levelVolume.Id);
      else
        this.VolumeService.OnGoLower(levelVolume.Id);
      levelVolume.PlayerIsHigher = new bool?(flag);
    }
  }

  private void RegisterVolumes()
  {
    this.VolumeService.RegisterNeeded = false;
    this.levelVolumes = this.LevelManager.Volumes.Values.ToArray<Volume>();
  }

  private void TestVolumes(bool force)
  {
    if (!force && this.GameState.Loading)
      return;
    if (!force && this.deferredScripts)
    {
      foreach (Volume currentVolume in this.PlayerManager.CurrentVolumes)
        this.VolumeService.OnEnter(currentVolume.Id);
      this.HeightCheck();
      this.deferredScripts = false;
    }
    else
      SoundService.ImmediateEffect = false;
    if (force)
      this.deferredScripts = true;
    Vector3 mask = this.CameraManager.Viewpoint.VisibleAxis().GetMask();
    Vector3 vector3 = this.CameraManager.Viewpoint.ForwardVector();
    if (this.PlayerManager.Background)
      vector3 *= -1f;
    Ray ray = new Ray()
    {
      Position = this.PlayerManager.Center * (Vector3.One - mask) - vector3 * this.LevelManager.Size,
      Direction = vector3
    };
    if (this.PlayerManager.Action == ActionType.PullUpBack || this.PlayerManager.Action == ActionType.PullUpFront || this.PlayerManager.Action == ActionType.PullUpCornerLedge)
      ray.Position += new Vector3(0.0f, 0.5f, 0.0f);
    foreach (Volume levelVolume in this.levelVolumes)
    {
      if (levelVolume.Enabled)
      {
        if (!this.GameState.FarawaySettings.InTransition)
        {
          bool flag = (double) this.CameraManager.Center.Y > ((double) levelVolume.From.Y + (double) levelVolume.To.Y) / 2.0;
          if (!levelVolume.PlayerIsHigher.HasValue || flag != levelVolume.PlayerIsHigher.Value)
          {
            if (flag)
              this.VolumeService.OnGoHigher(levelVolume.Id);
            else
              this.VolumeService.OnGoLower(levelVolume.Id);
            levelVolume.PlayerIsHigher = new bool?(flag);
          }
        }
        if (this.checkForContainment && (levelVolume.Id == 1 || levelVolume.Id == 2))
        {
          if (levelVolume.BoundingBox.Contains(this.PlayerManager.Position) != ContainmentType.Disjoint)
            this.PlayerIsInside(levelVolume, force);
        }
        else
        {
          float? nullable = levelVolume.BoundingBox.Intersects(ray);
          if (levelVolume.ActorSettings != null && levelVolume.ActorSettings.IsBlackHole)
          {
            if (!nullable.HasValue)
              nullable = levelVolume.BoundingBox.Intersects(new Ray(ray.Position + new Vector3(0.0f, 0.3f, 0.0f), ray.Direction));
            if (!nullable.HasValue)
              nullable = levelVolume.BoundingBox.Intersects(new Ray(ray.Position - new Vector3(0.0f, 0.3f, 0.0f), ray.Direction));
          }
          if (nullable.HasValue)
          {
            bool flag = false;
            bool isBlackHole = levelVolume.ActorSettings != null && levelVolume.ActorSettings.IsBlackHole;
            if (this.PlayerManager.CarriedInstance != null)
              this.PlayerManager.CarriedInstance.PhysicsState.UpdatingPhysics = true;
            NearestTriles nearestTriles = this.LevelManager.NearestTrile(ray.Position, this.PlayerManager.Background ? QueryOptions.Background : QueryOptions.None);
            if (this.LevelManager.Name != "PIVOT_TWO" && nearestTriles.Surface != null)
              flag |= this.TestObstruction(nearestTriles.Surface, nullable.Value, ray.Position, isBlackHole);
            if (nearestTriles.Deep != null)
              flag |= this.TestObstruction(nearestTriles.Deep, nullable.Value, ray.Position, isBlackHole);
            if (this.PlayerManager.CarriedInstance != null)
              this.PlayerManager.CarriedInstance.PhysicsState.UpdatingPhysics = false;
            if (!flag && (levelVolume.ActorSettings != null && levelVolume.ActorSettings.IsBlackHole || levelVolume.Orientations.Contains(this.CameraManager.VisibleOrientation)))
              this.PlayerIsInside(levelVolume, force);
          }
        }
      }
    }
    for (int index = this.PlayerManager.CurrentVolumes.Count - 1; index >= 0; --index)
    {
      Volume currentVolume = this.PlayerManager.CurrentVolumes[index];
      if (!currentVolume.PlayerInside)
      {
        if (!force)
          this.VolumeService.OnExit(currentVolume.Id);
        this.PlayerManager.CurrentVolumes.RemoveAt(index);
      }
      currentVolume.PlayerInside = false;
    }
    if (this.PlayerManager.CurrentVolumes.Count <= 0 || this.GameState.FarawaySettings.InTransition || this.GameState.DotLoading)
      return;
    if (this.PlayerManager.Action == ActionType.LesserWarp || this.PlayerManager.Action == ActionType.GateWarp)
      this.Input.Clear();
    if (!this.pendingCheck)
      return;
    foreach (Volume currentVolume in this.PlayerManager.CurrentVolumes)
    {
      if (currentVolume.ActorSettings != null && currentVolume.ActorSettings.CodePattern != null && currentVolume.ActorSettings.CodePattern.Length != 0)
        this.TestCodePattern(currentVolume);
    }
  }

  private bool GrabInput()
  {
    CodeInput codeInput = CodeInput.None;
    if (this.InputManager.Jump == FezButtonState.Pressed)
      codeInput = CodeInput.Jump;
    else if (this.InputManager.RotateRight == FezButtonState.Pressed)
      codeInput = CodeInput.SpinRight;
    else if (this.InputManager.RotateLeft == FezButtonState.Pressed)
      codeInput = CodeInput.SpinLeft;
    else if (this.InputManager.Left == FezButtonState.Pressed)
      codeInput = CodeInput.Left;
    else if (this.InputManager.Right == FezButtonState.Pressed)
      codeInput = CodeInput.Right;
    else if (this.InputManager.Up == FezButtonState.Pressed)
      codeInput = CodeInput.Up;
    else if (this.InputManager.Down == FezButtonState.Pressed)
      codeInput = CodeInput.Down;
    if (codeInput == CodeInput.None)
      return false;
    this.Input.Add(codeInput);
    if (this.Input.Count > 16 /*0x10*/)
      this.Input.RemoveAt(0);
    return true;
  }

  private void TestCodePattern(Volume volume)
  {
    if (PatternTester.Test((IList<CodeInput>) this.Input, volume.ActorSettings.CodePattern))
    {
      this.Input.Clear();
      Waiters.Wait((Func<bool>) (() => this.CameraManager.ViewTransitionReached), (Action) (() =>
      {
        ScriptingHost.ScriptExecuted = false;
        this.VolumeService.OnCodeAccepted(volume.Id);
        if (!ScriptingHost.ScriptExecuted)
          return;
        this.GameState.SaveData.AnyCodeDeciphered = true;
        this.LevelService.ResolvePuzzle();
      }));
    }
    this.SinceInput = TimeSpan.Zero;
  }

  private bool TestObstruction(
    TrileInstance trile,
    float hitDistance,
    Vector3 hitStart,
    bool isBlackHole)
  {
    Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
    if (this.PlayerManager.Background)
      b *= -1f;
    return trile != null && trile.Enabled && !trile.Trile.Immaterial && trile.Trile.ActorSettings.Type != ActorType.Hole | isBlackHole && (double) (trile.Emplacement.AsVector + Vector3.One / 2f + b * -0.5f - hitStart).Dot(b) <= (double) hitDistance + 0.25;
  }

  private void PlayerIsInside(Volume volume, bool force)
  {
    volume.PlayerInside = true;
    if (this.PlayerManager.CurrentVolumes.Contains(volume))
      return;
    this.PlayerManager.CurrentVolumes.Add(volume);
    if (force)
      return;
    this.VolumeService.OnEnter(volume.Id);
  }

  [ServiceDependency]
  public ILevelService LevelService { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IVolumeService VolumeService { private get; set; }
}
