// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.PlayerAction
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
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components.Actions;

public abstract class PlayerAction(Game game) : DrawableGameComponent(game)
{
  private ActionType lastAction;
  private bool wasActive;
  private bool? overridden;

  public void Reset()
  {
    this.wasActive = false;
    this.lastAction = ActionType.None;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading || this.GameState.InCutscene || this.GameState.InMap || this.GameState.InFpsMode || this.GameState.InMenuCube || !this.ViewTransitionIndependent && !this.CameraManager.ActionRunning)
      return;
    if (!this.PlayerManager.CanControl)
    {
      this.InputManager.SaveState();
      this.InputManager.Reset();
    }
    this.TestConditions();
    bool isActive = this.IsActionAllowed(this.PlayerManager.Action);
    this.SyncAnimation(isActive);
    if (isActive)
    {
      if (!this.wasActive)
        this.Begin();
      TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
      if (this.Act(elapsedGameTime))
        this.PlayerManager.Animation.Timing.Update(elapsedGameTime, (float) ((1.0 + (double) Math.Abs(this.CollisionManager.GravityFactor)) / 2.0));
    }
    else if (this.wasActive)
      this.End();
    this.SyncAnimation(isActive);
    if (!this.PlayerManager.CanControl)
      this.InputManager.RecoverState();
    this.wasActive = isActive;
  }

  public bool IsUpdateOverridden
  {
    get
    {
      return this.overridden.HasValue ? this.overridden.Value : (this.overridden = new bool?(new Action<GameTime>(((GameComponent) this).Update).Method.DeclaringType != typeof (PlayerAction))).Value;
    }
  }

  public void LightUpdate(GameTime gameTime, bool actionNotRunning)
  {
    if (!this.ViewTransitionIndependent & actionNotRunning)
      return;
    this.TestConditions();
    bool isActive = this.IsActionAllowed(this.PlayerManager.Action);
    this.SyncAnimation(isActive);
    if (isActive)
    {
      if (!this.wasActive)
        this.Begin();
      TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
      if (this.Act(elapsedGameTime))
        this.PlayerManager.Animation.Timing.Update(elapsedGameTime, (float) ((1.0 + (double) Math.Abs(this.CollisionManager.GravityFactor)) / 2.0));
    }
    else if (this.wasActive)
      this.End();
    this.SyncAnimation(isActive);
    this.wasActive = isActive;
  }

  protected void SyncAnimation(bool isActive)
  {
    if (isActive && this.lastAction != this.PlayerManager.Action)
    {
      AnimatedTexture animation1 = this.PlayerManager.Animation;
      AnimatedTexture animation2 = this.PlayerManager.GetAnimation(this.PlayerManager.Action);
      this.PlayerManager.Animation = animation2;
      animation2.Timing.StartFrame = this.PlayerManager.Action.GetStartFrame();
      int endFrame = this.PlayerManager.Action.GetEndFrame();
      animation2.Timing.EndFrame = endFrame != -1 ? endFrame : animation2.Timing.InitialEndFrame;
      animation2.Timing.Loop = this.PlayerManager.Action.IsAnimationLooping();
      if ((animation2 != animation1 || !animation2.Timing.Loop) && (this.lastAction != ActionType.Pushing && this.lastAction != ActionType.DropHeavyTrile && this.lastAction != ActionType.DropTrile || this.PlayerManager.Action != ActionType.Grabbing))
        animation2.Timing.Restart();
      if (this.PlayerManager.Action == ActionType.GrabCornerLedge && this.lastAction == ActionType.LowerToCornerLedge)
        animation2.Timing.Step = animation2.Timing.EndStep - 1f / 1000f;
      else if (this.PlayerManager.Action == ActionType.ThrowingHeavy && this.PlayerManager.LastAction == ActionType.CarryHeavyJump)
        animation2.Timing.Step = animation2.Timing.EndStep * (3f / (float) animation2.Timing.EndFrame);
      else if ((this.PlayerManager.Action == ActionType.GrabLedgeFront || this.PlayerManager.Action == ActionType.GrabLedgeBack) && this.lastAction.IsOnLedge())
        animation2.Timing.Step = animation2.Timing.EndStep - 1f / 1000f;
      else if (FezMath.In<ActionType>(this.lastAction, ActionType.ToCornerBack, ActionType.ToCornerFront, ActionType.GrabLedgeFront, ActionType.GrabLedgeBack, (IEqualityComparer<ActionType>) ActionTypeComparer.Default) && this.PlayerManager.Action == ActionType.GrabCornerLedge)
        animation2.Timing.Step = animation2.Timing.EndStep - 1f / 1000f;
      else if (this.PlayerManager.Action == ActionType.GrabTombstone && this.PlayerManager.LastAction == ActionType.PivotTombstone)
        animation2.Timing.Step = animation2.Timing.EndStep - 1f / 1000f;
      else if (this.PlayerManager.Action == ActionType.TurnToBell && this.PlayerManager.LastAction == ActionType.HitBell)
        animation2.Timing.Step = animation2.Timing.EndStep - 1f / 1000f;
    }
    this.lastAction = this.PlayerManager.Action;
  }

  protected virtual void TestConditions()
  {
  }

  protected virtual void Begin()
  {
  }

  protected virtual void End()
  {
  }

  protected virtual bool Act(TimeSpan elapsed) => false;

  protected virtual bool ViewTransitionIndependent => false;

  protected abstract bool IsActionAllowed(ActionType type);

  [ServiceDependency]
  public ISoundManager SoundManager { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }

  [ServiceDependency]
  public IGomezService GomezService { protected get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { protected get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { protected get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { protected get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { protected get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { protected get; set; }

  [ServiceDependency]
  public IGamepadsManager GamepadsManager { protected get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { protected get; set; }

  [ServiceDependency]
  public IInputManager InputManager { protected get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { protected get; set; }

  [ServiceDependency]
  public IPhysicsManager PhysicsManager { protected get; set; }

  [ServiceDependency(Optional = true)]
  public IWalkToService WalkTo { protected get; set; }
}
