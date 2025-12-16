// Decompiled with JetBrains decompiler
// Type: FezGame.Components.RumblerHost
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
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class RumblerHost(Game game) : GameComponent(game)
{
  private static readonly TimeSpan SignalDuration = TimeSpan.FromSeconds(0.25);
  private static readonly TimeSpan SilenceDuration = TimeSpan.FromSeconds(0.4);
  private ArtObjectInstance ArtObject;
  private int CurrentIndex;
  private VibrationMotor CurrentSignal;
  private TimeSpan SinceChanged;
  private SoundEmitter eForkRumble;
  private readonly List<VibrationMotor> Input = new List<VibrationMotor>();
  private SoundEffect ActivateSound;

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
    this.ArtObject = this.LevelManager.ArtObjects.Values.FirstOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.Rumbler && x.ActorSettings.VibrationPattern != null));
    if (this.ArtObject != null)
    {
      this.Enabled = true;
      if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(this.ArtObject.Id))
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
        this.LevelManager.ArtObjects.Remove(this.ArtObject.Id);
        this.ArtObject.Dispose();
        this.LevelMaterializer.RegisterSatellites();
        Vector3 position1 = this.ArtObject.Position;
        if (!this.GameState.SaveData.ThisLevel.DestroyedTriles.Contains(new TrileEmplacement(position1 - Vector3.One / 2f)))
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
            if (toAdd.InstanceId == -1)
              this.LevelMaterializer.CullInstanceIn(toAdd);
          }
        }
        this.Enabled = false;
      }
    }
    if (this.Enabled)
    {
      this.eForkRumble = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/ForkRumble").Emit(true, 0.0f, 0.0f);
      this.ActivateSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/ForkActivate");
      this.ArtObject.ActorSettings.VibrationPattern = Util.JoinArrays<VibrationMotor>(this.ArtObject.ActorSettings.VibrationPattern, new VibrationMotor[3]);
    }
    else
    {
      this.ActivateSound = (SoundEffect) null;
      this.eForkRumble = (SoundEmitter) null;
    }
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
    this.RumblerService.OnActivate(this.ArtObject.Id);
    this.Enabled = false;
    Waiters.Wait((Func<bool>) (() => this.CameraManager.ViewTransitionReached), new Action(this.Solve));
  }

  private void Solve()
  {
    foreach (Volume volume in (IEnumerable<Volume>) this.LevelManager.Volumes.Values)
    {
      if (volume.ActorSettings != null && volume.ActorSettings.IsPointOfInterest && volume.BoundingBox.Contains(this.ArtObject.Bounds) != ContainmentType.Disjoint && volume.Enabled)
      {
        volume.Enabled = false;
        this.GameState.SaveData.ThisLevel.InactiveVolumes.Add(volume.Id);
      }
    }
    this.SoundManager.MusicVolumeFactor = 1f;
    this.eForkRumble.Cue.Stop();
    this.ActivateSound.EmitAt(this.ArtObject.Position);
    ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, this.ArtObject, this.ArtObject.Position)
    {
      FlashOnSpawn = true
    });
    this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(this.ArtObject.Id);
    this.LevelService.ResolvePuzzle();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading || this.GameState.InMap || !this.CameraManager.Viewpoint.IsOrthographic())
      return;
    this.SinceChanged += gameTime.ElapsedGameTime;
    if (this.SinceChanged >= this.CurrentDuration)
    {
      ++this.CurrentIndex;
      if (this.CurrentIndex >= this.ArtObject.ActorSettings.VibrationPattern.Length || this.CurrentIndex < 0)
        this.CurrentIndex = 0;
      this.SinceChanged -= this.CurrentDuration;
      this.CurrentSignal = this.ArtObject.ActorSettings.VibrationPattern.Length != 0 ? this.ArtObject.ActorSettings.VibrationPattern[this.CurrentIndex] : VibrationMotor.None;
    }
    float amount = FezMath.Saturate(1f - ((((this.PlayerManager.Center - this.ArtObject.Position) * this.CameraManager.Viewpoint.ScreenSpaceMask()).Abs() - new Vector3(0.5f, 2f, 0.5f)) / 3f).Saturate().Length());
    if (this.CurrentSignal == VibrationMotor.None)
    {
      this.eForkRumble.VolumeFactor *= Math.Max((float) (1.0 - Math.Pow(this.SinceChanged.TotalSeconds / this.CurrentDuration.TotalSeconds, 4.0)), 0.75f);
      this.SoundManager.MusicVolumeFactor = (float) (1.0 - (double) amount * 0.60000002384185791 - (double) this.eForkRumble.VolumeFactor * 0.20000000298023224);
    }
    else
    {
      this.eForkRumble.VolumeFactor = amount;
      this.eForkRumble.Pan = this.CurrentSignal == VibrationMotor.RightHigh ? 1f : -1f;
      this.SoundManager.MusicVolumeFactor = (float) (1.0 - (double) amount * 0.800000011920929);
      if ((double) amount != 1.0)
        amount *= 0.5f;
      if (this.CurrentSignal == VibrationMotor.LeftLow)
        amount *= 0.5f;
      if ((double) amount > 0.0)
        this.InputManager.ActiveGamepad.Vibrate(this.CurrentSignal, (double) amount, this.CurrentDuration - this.SinceChanged, EasingType.None);
      else
        this.Input.Clear();
    }
  }

  private TimeSpan CurrentDuration
  {
    get
    {
      return this.CurrentSignal != VibrationMotor.None ? RumblerHost.SignalDuration : RumblerHost.SilenceDuration;
    }
  }

  [ServiceDependency]
  public ILevelService LevelService { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IInputManager InputManager { get; set; }

  [ServiceDependency]
  public ICodePatternService RumblerService { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }
}
