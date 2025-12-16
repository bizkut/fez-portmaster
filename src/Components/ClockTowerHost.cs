// Decompiled with JetBrains decompiler
// Type: FezGame.Components.ClockTowerHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

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

internal class ClockTowerHost(Game game) : GameComponent(game)
{
  private ArtObjectInstance RedAo;
  private ArtObjectInstance BlueAo;
  private ArtObjectInstance GreenAo;
  private ArtObjectInstance WhiteAo;
  private Quaternion RedOriginalRotation;
  private Quaternion BlueOriginalRotation;
  private Quaternion GreenOriginalRotation;
  private Quaternion WhiteOriginalRotation;
  private Vector3 RedOriginalPosition;
  private Vector3 BlueOriginalPosition;
  private Vector3 GreenOriginalPosition;
  private Vector3 WhiteOriginalPosition;
  private TrileGroup RedGroup;
  private TrileGroup BlueGroup;
  private TrileGroup GreenGroup;
  private TrileGroup WhiteGroup;
  private TrileInstance RedTopMost;
  private TrileInstance BlueTopMost;
  private TrileInstance GreenTopMost;
  private TrileInstance WhiteTopMost;
  private TrileInstance RedSecret;
  private TrileInstance BlueSecret;
  private TrileInstance GreenSecret;
  private TrileInstance WhiteSecret;
  private SoundEffect sTickTock;
  private SoundEmitter eTickTock;
  private float lastRedAngle;

  public override void Initialize()
  {
    base.Initialize();
    this.TryInitialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
  }

  private void TryInitialize()
  {
    this.Enabled = this.LevelManager.Name == "CLOCK";
    this.RedAo = this.BlueAo = this.GreenAo = this.WhiteAo = (ArtObjectInstance) null;
    this.sTickTock = (SoundEffect) null;
    this.eTickTock = (SoundEmitter) null;
    if (!this.Enabled)
      return;
    foreach (ArtObjectInstance artObjectInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      if (artObjectInstance.ArtObjectName == "CLOCKHAND_RAO")
        this.RedAo = artObjectInstance;
      if (artObjectInstance.ArtObjectName == "CLOCKHAND_GAO")
        this.GreenAo = artObjectInstance;
      if (artObjectInstance.ArtObjectName == "CLOCKHAND_BAO")
        this.BlueAo = artObjectInstance;
      if (artObjectInstance.ArtObjectName == "CLOCKHAND_WAO")
        this.WhiteAo = artObjectInstance;
    }
    this.RedOriginalRotation = this.RedAo.Rotation;
    this.BlueOriginalRotation = this.BlueAo.Rotation;
    this.GreenOriginalRotation = this.GreenAo.Rotation;
    this.WhiteOriginalRotation = this.WhiteAo.Rotation;
    this.RedOriginalPosition = this.RedAo.Position + 1.125f * Vector3.UnitX;
    this.GreenOriginalPosition = this.GreenAo.Position - 1.125f * Vector3.UnitX;
    this.BlueOriginalPosition = this.BlueAo.Position + 1.125f * Vector3.UnitZ;
    this.WhiteOriginalPosition = this.WhiteAo.Position - 1.125f * Vector3.UnitZ;
    this.RedGroup = this.LevelManager.Groups[23];
    this.BlueGroup = this.LevelManager.Groups[24];
    this.GreenGroup = this.LevelManager.Groups[25];
    this.WhiteGroup = this.LevelManager.Groups[26];
    this.RedTopMost = this.RedGroup.Triles.First<TrileInstance>((Func<TrileInstance, bool>) (x => x.Emplacement.Y == 58));
    this.BlueTopMost = this.BlueGroup.Triles.First<TrileInstance>((Func<TrileInstance, bool>) (x => x.Emplacement.Y == 58));
    this.GreenTopMost = this.GreenGroup.Triles.First<TrileInstance>((Func<TrileInstance, bool>) (x => x.Emplacement.Y == 58));
    this.WhiteTopMost = this.WhiteGroup.Triles.First<TrileInstance>((Func<TrileInstance, bool>) (x => x.Emplacement.Y == 58));
    if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(this.RedAo.Id))
    {
      this.RedAo.Enabled = false;
      this.LevelManager.RemoveArtObject(this.RedAo);
    }
    if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(this.GreenAo.Id))
    {
      this.GreenAo.Enabled = false;
      this.LevelManager.RemoveArtObject(this.GreenAo);
    }
    if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(this.BlueAo.Id))
    {
      this.BlueAo.Enabled = false;
      this.LevelManager.RemoveArtObject(this.BlueAo);
    }
    if (this.GameState.SaveData.ThisLevel.InactiveArtObjects.Contains(this.WhiteAo.Id))
    {
      this.WhiteAo.Enabled = false;
      this.LevelManager.RemoveArtObject(this.WhiteAo);
    }
    TimeSpan timeSpan = TimeSpan.FromTicks((DateTime.UtcNow - DateTime.FromFileTimeUtc(this.GameState.SaveData.CreationTime)).Ticks);
    if (this.RedAo.Enabled)
    {
      float angle = FezMath.WrapAngle((float) ((double) FezMath.Round(timeSpan.TotalSeconds) / 60.0 * 6.2831854820251465));
      this.lastRedAngle = angle;
      this.RedAo.Rotation = Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, angle) * this.RedOriginalRotation;
    }
    if (!this.WhiteAo.Enabled && !this.BlueAo.Enabled && !this.GreenAo.Enabled && !this.RedAo.Enabled)
      return;
    this.sTickTock = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/TickTockLoop");
    Waiters.Wait(FezMath.Frac(timeSpan.TotalSeconds), (Action) (() => this.eTickTock = this.sTickTock.EmitAt(new Vector3(41.5f, 61.5f, 35.5f), true)));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InMap || !this.CameraManager.ActionRunning || !this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.Loading || this.PlayerManager.Action == ActionType.FindingTreasure || this.GameState.FarawaySettings.InTransition)
      return;
    TimeSpan timeSpan = TimeSpan.FromTicks(TimeSpan.FromTicks((DateTime.UtcNow - DateTime.FromFileTimeUtc(this.GameState.SaveData.CreationTime)).Ticks).Ticks % 6048000000000L);
    if (this.RedAo.Enabled)
    {
      float num = FezMath.CurveAngle(this.lastRedAngle, FezMath.WrapAngle((float) ((double) FezMath.Round(timeSpan.TotalSeconds) / 60.0 * 6.2831854820251465)), 0.1f);
      this.lastRedAngle = num;
      this.RedAo.Rotation = Quaternion.CreateFromAxisAngle(-Vector3.UnitZ, num) * this.RedOriginalRotation;
      this.RedAo.Position = this.RedOriginalPosition + new Vector3(-(float) Math.Cos((double) num), (float) Math.Sin((double) num), 0.0f) * 1.45f;
      this.RedSecret = this.TestSecretFor((double) num >= 1.1957963705062866 && (double) num <= 1.6957963705062866, this.RedAo, this.RedSecret, this.RedTopMost);
    }
    if (this.BlueAo.Enabled)
    {
      float num = FezMath.WrapAngle((float) (timeSpan.TotalMinutes / 60.0 * 6.2831854820251465));
      this.BlueAo.Rotation = Quaternion.CreateFromAxisAngle(-Vector3.UnitX, num) * this.BlueOriginalRotation;
      this.BlueAo.Position = this.BlueOriginalPosition + new Vector3(0.0f, (float) Math.Sin((double) num), (float) Math.Cos((double) num)) * 1.45f;
      this.BlueSecret = this.TestSecretFor(FezMath.AlmostEqual(num, 1.57079637f, 0.125f), this.BlueAo, this.BlueSecret, this.BlueTopMost);
    }
    if (this.GreenAo.Enabled)
    {
      float num = FezMath.WrapAngle((float) (timeSpan.TotalHours / 24.0 * 6.2831854820251465));
      this.GreenAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitZ, num) * this.GreenOriginalRotation;
      this.GreenAo.Position = this.GreenOriginalPosition + new Vector3((float) Math.Cos((double) num), (float) Math.Sin((double) num), 0.0f) * 1.45f;
      this.GreenSecret = this.TestSecretFor(FezMath.AlmostEqual(num, 1.57079637f, 0.125f), this.GreenAo, this.GreenSecret, this.GreenTopMost);
    }
    if (!this.WhiteAo.Enabled)
      return;
    float num1 = FezMath.WrapAngle((float) (timeSpan.TotalDays / 7.0 * 6.2831854820251465));
    this.WhiteAo.Rotation = Quaternion.CreateFromAxisAngle(Vector3.UnitX, num1) * this.WhiteOriginalRotation;
    this.WhiteAo.Position = this.WhiteOriginalPosition + new Vector3(0.0f, (float) Math.Sin((double) num1), -(float) Math.Cos((double) num1)) * 1.45f;
    this.WhiteSecret = this.TestSecretFor(FezMath.AlmostEqual(num1, 1.57079637f, 0.125f), this.WhiteAo, this.WhiteSecret, this.WhiteTopMost);
  }

  private TrileInstance TestSecretFor(
    bool condition,
    ArtObjectInstance ao,
    TrileInstance secretTrile,
    TrileInstance topMost)
  {
    Vector3 position = topMost.Position + Vector3.Up * 1.5f;
    if (condition)
    {
      if (secretTrile != null && secretTrile.Collected)
      {
        ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, ao));
        this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(ao.Id);
        ao.Enabled = false;
        this.TestAllSolved();
        this.LevelService.ResolvePuzzle();
        return (TrileInstance) null;
      }
      if (secretTrile == null)
      {
        secretTrile = new TrileInstance(position, this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>().Id);
        ServiceHelper.AddComponent((IGameComponent) new GlitchyRespawner(this.Game, secretTrile)
        {
          DontCullIn = true
        });
      }
      secretTrile.Position = position;
      if (!secretTrile.Hidden)
        this.LevelManager.UpdateInstance(secretTrile);
    }
    else if (secretTrile != null)
    {
      if (secretTrile.Collected)
      {
        ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, ao));
        this.GameState.SaveData.ThisLevel.InactiveArtObjects.Add(ao.Id);
        ao.Enabled = false;
        this.TestAllSolved();
        this.LevelService.ResolvePuzzle();
        return (TrileInstance) null;
      }
      ServiceHelper.AddComponent((IGameComponent) new GlitchyDespawner(this.Game, secretTrile));
      TrileInstance rs = secretTrile;
      Vector3 p = position;
      Waiters.Interpolate(2.5, (Action<float>) (_ => rs.Position = p));
      return (TrileInstance) null;
    }
    return secretTrile;
  }

  private void TestAllSolved()
  {
    if (this.WhiteAo.Enabled || this.BlueAo.Enabled || this.GreenAo.Enabled || this.RedAo.Enabled)
      return;
    this.eTickTock.FadeOutAndDie(1f);
    this.eTickTock = (SoundEmitter) null;
    Volume volume = this.LevelManager.Volumes[6];
    if (!volume.Enabled)
      return;
    volume.Enabled = false;
    this.GameState.SaveData.ThisLevel.InactiveVolumes.Add(volume.Id);
  }

  [ServiceDependency]
  public ILevelService LevelService { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
