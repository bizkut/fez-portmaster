// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GameWideCodes
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class GameWideCodes(Game game) : GameComponent(game)
{
  private static readonly CodeInput[] AchievementCode = new CodeInput[8]
  {
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinLeft,
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinLeft,
    CodeInput.SpinLeft,
    CodeInput.SpinLeft
  };
  private static readonly CodeInput[] JetpackCode = new CodeInput[5]
  {
    CodeInput.Up,
    CodeInput.Up,
    CodeInput.Up,
    CodeInput.Up,
    CodeInput.Jump
  };
  private static readonly CodeInput[] MapCode = new CodeInput[8]
  {
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinLeft,
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinRight,
    CodeInput.SpinLeft
  };
  private readonly List<CodeInput> Input = new List<CodeInput>();
  private TimeSpan SinceInput;
  private TrileInstance waitingForTrile;
  private bool isMapQr;
  private bool isAchievementCode;

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      this.waitingForTrile = (TrileInstance) null;
      this.Input.Clear();
      this.isMapQr = this.isAchievementCode = false;
    });
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.CameraManager.Viewpoint.IsOrthographic() || this.GameState.InMap || this.GameState.Paused || this.GameState.Loading || this.GameState.InCutscene || this.GameState.IsTrialMode || this.GameState.FarawaySettings.InTransition || this.GameState.DotLoading || this.PlayerManager.Action == ActionType.OpeningTreasure)
      return;
    this.TestInput();
    this.SinceInput += gameTime.ElapsedGameTime;
  }

  private void TestInput()
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
      return;
    this.Input.Add(codeInput);
    if (this.Input.Count > 16 /*0x10*/)
      this.Input.RemoveAt(0);
    if (!this.isAchievementCode && !this.GameState.SaveData.AchievementCheatCodeDone && !this.GameState.SaveData.FezHidden && PatternTester.Test((IList<CodeInput>) this.Input, GameWideCodes.AchievementCode) && this.LevelManager.Name != "ELDERS")
    {
      this.Input.Clear();
      this.isAchievementCode = true;
      this.LevelService.ResolvePuzzleSoundOnly();
      Waiters.Wait((Func<bool>) (() => this.CameraManager.ViewTransitionReached && this.PlayerManager.Grounded && !this.PlayerManager.Background), (Action) (() =>
      {
        Vector3 position = this.PlayerManager.Center + new Vector3(0.0f, 2f, 0.0f);
        Trile trile = this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>();
        if (trile == null)
          return;
        Vector3 vector3 = position - Vector3.One / 2f;
        NearestTriles nearestTriles = this.LevelManager.NearestTrile(position);
        TrileInstance trileInstance = nearestTriles.Surface ?? nearestTriles.Deep;
        if (trileInstance != null)
          vector3 = this.CameraManager.Viewpoint.ScreenSpaceMask() * vector3 + trileInstance.Center * this.CameraManager.Viewpoint.DepthMask() - this.CameraManager.Viewpoint.ForwardVector() * 2f;
        ServiceHelper.AddComponent((IGameComponent) new GlitchyRespawner(this.Game, this.waitingForTrile = new TrileInstance(Vector3.Clamp(vector3, Vector3.Zero, this.LevelManager.Size - Vector3.One), trile.Id)));
        this.waitingForTrile.GlobalSpawn = true;
        this.GomezService.CollectedGlobalAnti += new Action(this.GotTrile);
      }));
    }
    if (!this.isMapQr && !this.GameState.SaveData.MapCheatCodeDone && this.GameState.SaveData.Maps.Contains("MAP_MYSTERY") && this.LevelManager.Name != "WATERTOWER_SECRET" && PatternTester.Test((IList<CodeInput>) this.Input, GameWideCodes.MapCode))
    {
      this.Input.Clear();
      this.GameState.SaveData.AnyCodeDeciphered = true;
      this.isMapQr = true;
      if (this.GameState.SaveData.World.ContainsKey("WATERTOWER_SECRET"))
        this.GameState.SaveData.World["WATERTOWER_SECRET"].FilledConditions.SecretCount = 1;
      this.LevelService.ResolvePuzzleSoundOnly();
      Waiters.Wait((Func<bool>) (() => this.CameraManager.ViewTransitionReached && this.PlayerManager.Grounded && !this.PlayerManager.Background), (Action) (() =>
      {
        Vector3 position = this.PlayerManager.Center + new Vector3(0.0f, 2f, 0.0f);
        Trile trile = this.LevelManager.ActorTriles(ActorType.SecretCube).FirstOrDefault<Trile>();
        if (trile == null)
          return;
        Vector3 vector3 = position - Vector3.One / 2f;
        NearestTriles nearestTriles = this.LevelManager.NearestTrile(position);
        TrileInstance trileInstance = nearestTriles.Surface ?? nearestTriles.Deep;
        if (trileInstance != null)
          vector3 = this.CameraManager.Viewpoint.ScreenSpaceMask() * vector3 + trileInstance.Center * this.CameraManager.Viewpoint.DepthMask() - this.CameraManager.Viewpoint.ForwardVector() * 2f;
        ServiceHelper.AddComponent((IGameComponent) new GlitchyRespawner(this.Game, this.waitingForTrile = new TrileInstance(Vector3.Clamp(vector3, Vector3.Zero, this.LevelManager.Size - Vector3.One), trile.Id)));
        this.waitingForTrile.GlobalSpawn = true;
        this.GomezService.CollectedGlobalAnti += new Action(this.GotTrile);
      }));
    }
    if (this.GameState.SaveData.HasNewGamePlus && PatternTester.Test((IList<CodeInput>) this.Input, GameWideCodes.JetpackCode))
    {
      this.Input.Clear();
      this.GameState.JetpackMode = true;
    }
    this.SinceInput = TimeSpan.Zero;
  }

  private void GotTrile()
  {
    if (this.waitingForTrile == null || !this.waitingForTrile.Collected)
      return;
    this.waitingForTrile = (TrileInstance) null;
    this.GomezService.CollectedGlobalAnti -= new Action(this.GotTrile);
    if (this.isMapQr)
      this.GameState.SaveData.MapCheatCodeDone = true;
    else if (this.isAchievementCode)
      this.GameState.SaveData.AchievementCheatCodeDone = true;
    this.isAchievementCode = this.isMapQr = false;
  }

  [ServiceDependency]
  public IGomezService GomezService { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public ILevelService LevelService { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }
}
