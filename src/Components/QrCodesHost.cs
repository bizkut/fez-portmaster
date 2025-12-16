// Decompiled with JetBrains decompiler
// Type: FezGame.Components.QrCodesHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class QrCodesHost(Game game) : GameComponent(game)
{
  private ArtObjectInstance ArtObject;
  private readonly List<VibrationMotor> Input = new List<VibrationMotor>();

  public override void Initialize()
  {
    base.Initialize();
    this.CameraManager.ViewpointChanged += new Action(this.CheckForPattern);
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Enabled = false;
    this.ArtObject = this.LevelManager.ArtObjects.Values.FirstOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.QrCode && x.ActorSettings.VibrationPattern != null));
    if (this.ArtObject != null)
    {
      this.Enabled = true;
      if (this.GameService.IsSewerQrResolved)
      {
        int? attachedGroup = this.ArtObject.ActorSettings.AttachedGroup;
        if (attachedGroup.HasValue)
        {
          attachedGroup = this.ArtObject.ActorSettings.AttachedGroup;
          int key1 = attachedGroup.Value;
          IDictionary<int, TrileGroup> groups = this.LevelManager.Groups;
          attachedGroup = this.ArtObject.ActorSettings.AttachedGroup;
          int key2 = attachedGroup.Value;
          foreach (TrileInstance instance in groups[key2].Triles.ToArray())
            this.LevelManager.ClearTrile(instance);
          this.LevelManager.Groups.Remove(key1);
        }
        this.LevelManager.Volumes[1].Enabled = false;
        this.LevelManager.ArtObjects.Remove(this.ArtObject.Id);
        this.ArtObject.Dispose();
        this.LevelMaterializer.RegisterSatellites();
        Vector3 position1 = this.ArtObject.Position;
        if (this.GameState.SaveData.ThisLevel.ScriptingState == "NOT_COLLECTED")
        {
          Trile trile = this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>();
          if (trile != null)
          {
            Vector3 position2 = position1 - Vector3.One / 2f;
            this.LevelManager.ClearTrile(new TrileEmplacement(position2));
            IGameLevelManager levelManager = this.LevelManager;
            TrileInstance instance = new TrileInstance(position2, trile.Id);
            instance.OriginalEmplacement = new TrileEmplacement(position2);
            TrileInstance toAdd = instance;
            levelManager.RestoreTrile(instance);
            this.GomezService.CollectedAnti += (Action) (() => this.GameState.SaveData.ThisLevel.ScriptingState = (string) null);
            if (toAdd.InstanceId == -1)
              this.LevelMaterializer.CullInstanceIn(toAdd);
          }
        }
        this.Enabled = false;
      }
    }
    if (!this.Enabled)
      return;
    this.ArtObject.ActorSettings.VibrationPattern = Util.JoinArrays<VibrationMotor>(this.ArtObject.ActorSettings.VibrationPattern, new VibrationMotor[3]);
  }

  private void CheckForPattern()
  {
    if (!this.Enabled || this.GameState.Loading)
      return;
    this.Input.Add(this.CameraManager.Viewpoint.GetDistance(this.CameraManager.LastViewpoint) == 1 ? VibrationMotor.LeftLow : VibrationMotor.RightHigh);
    if (this.Input.Count > 16 /*0x10*/)
      this.Input.RemoveAt(0);
    if (!PatternTester.Test((IList<VibrationMotor>) this.Input, this.ArtObject.ActorSettings.VibrationPattern))
      return;
    this.Input.Clear();
    this.Enabled = false;
    Waiters.Wait((Func<bool>) (() => this.CameraManager.ViewTransitionReached), new Action(this.Solve));
  }

  private void Solve()
  {
    this.GameState.SaveData.ThisLevel.ScriptingState = "NOT_COLLECTED";
    ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, this.ArtObject, this.ArtObject.Position)
    {
      FlashOnSpawn = true
    });
    this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(this.ArtObject.Id);
    this.GameService.ResolveSewerQR();
    this.LevelService.ResolvePuzzle();
    this.GomezService.CollectedAnti += (Action) (() => this.GameState.SaveData.ThisLevel.ScriptingState = (string) null);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading || this.GameState.InMap)
      return;
    this.CameraManager.Viewpoint.IsOrthographic();
  }

  [ServiceDependency]
  public IGomezService GomezService { get; set; }

  [ServiceDependency]
  public IGameService GameService { get; set; }

  [ServiceDependency]
  public ILevelService LevelService { get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }
}
