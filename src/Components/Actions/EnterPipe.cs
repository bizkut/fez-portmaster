// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.EnterPipe
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components.Actions;

internal class EnterPipe : PlayerAction
{
  private const float SuckedSeconds = 0.75f;
  private const float FadeSeconds = 1.25f;
  private SoundEffect EnterSound;
  private SoundEffect ExitSound;
  private Volume PipeVolume;
  private EnterPipe.States State;
  private bool Descending;
  private TimeSpan SinceChanged;
  private string NextLevel;
  private float Depth;

  public EnterPipe(Game game)
    : base(game)
  {
    this.DrawOrder = 101;
  }

  protected override void LoadContent()
  {
    base.LoadContent();
    this.EnterSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Sewer/PipeDown");
    this.ExitSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Sewer/PipeUp");
  }

  protected override void TestConditions()
  {
    if (this.PlayerManager.Action == ActionType.WalkingTo || this.IsActionAllowed(this.PlayerManager.Action))
      return;
    int? pipeVolume;
    if (this.PlayerManager.Grounded)
    {
      pipeVolume = this.PlayerManager.PipeVolume;
      if (pipeVolume.HasValue && this.InputManager.Down == FezButtonState.Pressed)
      {
        IDictionary<int, Volume> volumes = this.LevelManager.Volumes;
        pipeVolume = this.PlayerManager.PipeVolume;
        int key = pipeVolume.Value;
        this.PipeVolume = volumes[key];
        this.PlayerManager.Action = ActionType.EnteringPipe;
        this.Descending = true;
      }
    }
    if (this.PlayerManager.Grounded)
      return;
    pipeVolume = this.PlayerManager.PipeVolume;
    if (!pipeVolume.HasValue || !this.InputManager.Up.IsDown() || !this.PlayerManager.Ceiling.AnyCollided())
      return;
    IDictionary<int, Volume> volumes1 = this.LevelManager.Volumes;
    pipeVolume = this.PlayerManager.PipeVolume;
    int key1 = pipeVolume.Value;
    this.PipeVolume = volumes1[key1];
    this.PlayerManager.Action = ActionType.EnteringPipe;
    this.Descending = false;
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    Vector3 vector3_2 = (this.PipeVolume.From + this.PipeVolume.To) / 2f;
    this.PlayerManager.Position = this.PlayerManager.Position * vector3_1 + vector3_2 * (Vector3.One - vector3_1);
  }

  protected override void Begin()
  {
    this.NextLevel = this.PlayerManager.NextLevel;
    this.State = EnterPipe.States.Sucked;
    this.SinceChanged = TimeSpan.Zero;
    this.PlayerManager.Velocity = Vector3.Zero;
    this.EnterSound.EmitAt(this.PlayerManager.Position);
  }

  protected override void End() => this.State = EnterPipe.States.None;

  protected override bool Act(TimeSpan elapsed)
  {
    switch (this.State)
    {
      case EnterPipe.States.Sucked:
        this.PlayerManager.Position += (float) elapsed.TotalSeconds * Vector3.UnitY * (this.Descending ? -1f : 1f) * 0.75f;
        this.SinceChanged += elapsed;
        if (this.SinceChanged.TotalSeconds > 0.75)
        {
          this.State = EnterPipe.States.FadeOut;
          this.SinceChanged = TimeSpan.Zero;
          break;
        }
        break;
      case EnterPipe.States.FadeOut:
        this.PlayerManager.Position += (float) elapsed.TotalSeconds * Vector3.UnitY * (this.Descending ? -1f : 1f) * 0.75f;
        this.SinceChanged += elapsed;
        if (this.SinceChanged.TotalSeconds > 1.25)
        {
          this.State = EnterPipe.States.LevelChange;
          this.SinceChanged = TimeSpan.Zero;
          this.GameState.Loading = true;
          Worker<bool> worker = this.ThreadPool.Take<bool>(new Action<bool>(this.DoLoad));
          worker.Finished += (Action) (() => this.ThreadPool.Return<bool>(worker));
          worker.Start(false);
          break;
        }
        break;
      case EnterPipe.States.FadeIn:
        if (this.SinceChanged == TimeSpan.Zero)
          this.ExitSound.EmitAt(this.PlayerManager.Position);
        Vector3 vector3 = this.CameraManager.Viewpoint.ScreenSpaceMask();
        this.PlayerManager.Position = this.PlayerManager.Position * vector3 + this.Depth * (Vector3.One - vector3);
        this.PlayerManager.Position += (float) elapsed.TotalSeconds * Vector3.UnitY * (this.Descending ? -1.1f : 1f) * 0.75f;
        this.SinceChanged += elapsed;
        if (this.SinceChanged.TotalSeconds > 1.25)
        {
          this.State = EnterPipe.States.SpitOut;
          this.SinceChanged = TimeSpan.Zero;
          break;
        }
        break;
      case EnterPipe.States.SpitOut:
        this.PlayerManager.Position += (float) elapsed.TotalSeconds * Vector3.UnitY * (this.Descending ? -1.1f : 1f) * 0.875f;
        this.SinceChanged += elapsed;
        bool flag = true;
        foreach (PointCollision pointCollision in this.PlayerManager.CornerCollision)
          flag &= pointCollision.Instances.Deep == null;
        if (!this.Descending & flag || this.SinceChanged.TotalSeconds > 0.75)
        {
          this.State = EnterPipe.States.None;
          this.SinceChanged = TimeSpan.Zero;
          if (!this.Descending)
          {
            this.PlayerManager.Position += 0.5f * Vector3.UnitY;
            IPlayerManager playerManager = this.PlayerManager;
            playerManager.Velocity = playerManager.Velocity - Vector3.UnitY;
            this.PhysicsManager.Update((IComplexPhysicsEntity) this.PlayerManager);
          }
          this.PlayerManager.Action = ActionType.Idle;
          break;
        }
        break;
    }
    return false;
  }

  private void DoLoad(bool dummy)
  {
    this.LevelManager.ChangeLevel(this.NextLevel);
    this.GameState.ScheduleLoadEnd = true;
    this.State = EnterPipe.States.FadeIn;
    Volume volume = this.LevelManager.Volumes.Values.FirstOrDefault<Volume>((Func<Volume, bool>) (v =>
    {
      int id = v.Id;
      int? identifier = this.LevelManager.Scripts.Values.First<Script>((Func<Script, bool>) (s => s.Actions.Any<ScriptAction>((Func<ScriptAction, bool>) (a => a.Operation == "AllowPipeChangeLevel")))).Triggers[0].Object.Identifier;
      int valueOrDefault = identifier.GetValueOrDefault();
      return id == valueOrDefault && identifier.HasValue;
    }));
    if (volume == null)
      throw new InvalidOperationException("Missing pipe volume in destination level!");
    Vector3 a1 = (volume.From + volume.To) / 2f;
    this.PlayerManager.Action = ActionType.EnteringPipe;
    this.PlayerManager.Position = a1 + Vector3.UnitY * 1.25f * (this.Descending ? 1f : -1f);
    this.PlayerManager.Velocity = Vector3.Zero;
    this.PlayerManager.RecordRespawnInformation();
    this.Depth = a1.Dot(this.CameraManager.Viewpoint.DepthMask());
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.State != EnterPipe.States.FadeOut && this.State != EnterPipe.States.FadeIn && this.State != EnterPipe.States.LevelChange)
      return;
    double linearStep = this.SinceChanged.TotalSeconds / 1.25;
    if (this.State == EnterPipe.States.FadeIn)
      linearStep = 1.0 - linearStep;
    float alpha = FezMath.Saturate(Easing.EaseIn(linearStep, EasingType.Cubic));
    this.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, alpha));
  }

  protected override bool IsActionAllowed(ActionType type)
  {
    return type == ActionType.EnteringPipe || this.State != 0;
  }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { private get; set; }

  private enum States
  {
    None,
    Sucked,
    FadeOut,
    LevelChange,
    FadeIn,
    SpitOut,
  }
}
