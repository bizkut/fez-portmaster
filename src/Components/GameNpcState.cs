// Decompiled with JetBrains decompiler
// Type: FezGame.Components.GameNpcState
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Components.Actions;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

public class GameNpcState : NpcState
{
  private bool SaidFirstLine;
  private int SequentialLineIndex;
  public bool ForceVisible;
  public bool IsNightForOwl;
  private readonly float originalSpeed;
  private bool waitingToSpeak;
  private SoundEffect takeoffSound;
  private SoundEffect hideSound;
  private SoundEffect comeOutSound;
  private SoundEffect burrowSound;

  public GameNpcState(Game game, NpcInstance npc)
    : base(game, npc)
  {
    this.originalSpeed = this.Npc.WalkSpeed;
  }

  public override void Initialize()
  {
    base.Initialize();
    if (this.CanTakeOff)
      this.takeoffSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Wildlife/BirdTakeoff");
    if (this.CanHide)
    {
      this.hideSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Wildlife/CritterHide");
      this.comeOutSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Wildlife/CritterComeOut");
    }
    if (!this.CanBurrow)
      return;
    this.burrowSound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Wildlife/RabbitBurrow");
  }

  public override void Update(GameTime gameTime)
  {
    if (this.isDisposed)
      return;
    if (this.EngineState.Paused || this.GameState.InMap)
    {
      if (this.Emitter == null || this.Emitter.Dead)
        return;
      this.Emitter.FadeOutAndDie(0.1f);
    }
    else
    {
      if (this.EngineState.Loading || this.EngineState.SkipRendering)
        return;
      this.Npc.Visible = this.CameraManager.Frustum.Contains(new BoundingBox(this.Position - this.Group.Scale, this.Position + this.Group.Scale)) != 0;
      if (this.Npc.Visible)
        this.UpdateRotation();
      if (!this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning)
        return;
      base.Update(gameTime);
      if (this.Npc.ActorType == ActorType.LightningGhost)
      {
        if (this.CurrentAction == NpcAction.Talk)
          this.Group.Material.Opacity = Math.Min(this.Group.Material.Opacity + 0.01f, 0.5f);
        else if (this.Npc.Talking)
        {
          this.Group.Material.Opacity = Math.Max(this.Group.Material.Opacity - 0.01f, 0.0f);
          if ((double) this.Group.Material.Opacity == 0.0)
          {
            this.Npc.Talking = false;
            this.Group.Material.Opacity = 1f;
          }
        }
      }
      if (this.Npc.ActorType != ActorType.Owl)
        return;
      bool flag = this.TimeManager.IsDayPhase(DayPhase.Night) || this.ForceVisible;
      if (this.ForceVisible)
        this.OwlInvisible = false;
      if (flag)
      {
        if (!this.IsNightForOwl && !this.GameState.SaveData.ThisLevel.InactiveNPCs.Contains(this.Npc.Id))
        {
          this.CurrentAction = NpcAction.Fly;
          this.UpdateAction();
          this.FlyingBack = true;
          this.OwlInvisible = false;
          float num = this.Npc.Position.Y - this.CameraManager.Center.Y;
          this.Position = this.Npc.Position + ((float) ((double) Math.Min(this.CameraManager.Radius, 60f * this.GraphicsDeviceService.GraphicsDevice.GetViewScale()) / (double) this.CameraManager.AspectRatio / 2.0) - num) * (-Vector3.Transform(Vector3.Right, this.Group.Rotation) * 1.50295115f + Vector3.Up);
        }
      }
      else if (this.IsNightForOwl)
      {
        this.Hide();
        this.UpdateAction();
        this.MayComeBack = true;
      }
      this.IsNightForOwl = flag;
    }
  }

  protected override void TryFlee()
  {
    if (this.PlayerManager.Action == ActionType.IdleSleep)
      return;
    float num1 = (this.Position - this.PlayerManager.Position).Abs().Dot(this.CameraManager.Viewpoint.ScreenSpaceMask());
    this.Npc.WalkSpeed = this.originalSpeed * (float) (1.0 + (1.0 - (double) FezMath.Saturate(num1 / 3f)));
    switch (this.CurrentAction)
    {
      case NpcAction.Turn:
        break;
      case NpcAction.Burrow:
        break;
      case NpcAction.ComeOut:
        break;
      case NpcAction.TakeOff:
        break;
      case NpcAction.Fly:
        break;
      default:
        Vector3 vector3 = Vector3.UnitY + this.CameraManager.Viewpoint.SideMask() * 3f + this.CameraManager.Viewpoint.DepthMask() * float.MaxValue;
        if (new BoundingBox(this.Position - vector3, this.Position + vector3).Contains(this.PlayerManager.Position) == 0)
        {
          if (this.CurrentAction != NpcAction.Hide)
            break;
          this.CurrentAction = NpcAction.ComeOut;
          this.UpdateAction();
          break;
        }
        float num2 = (this.PlayerManager.Position - this.Position).Dot(this.CameraManager.Viewpoint.RightVector()) * (float) Math.Sign(this.Npc.DestinationOffset.Dot(this.CameraManager.Viewpoint.RightVector()));
        NpcAction currentAction = this.CurrentAction;
        if ((double) num1 < 1.0)
        {
          this.Hide();
        }
        else
        {
          if (this.CurrentAction == NpcAction.Hide)
          {
            this.CurrentAction = NpcAction.ComeOut;
            this.comeOutSound.EmitAt(this.Position);
            this.UpdateAction();
            break;
          }
          HorizontalDirection horizontalDirection = FezMath.DirectionFromMovement(-num2);
          if (this.LookingDirection != horizontalDirection)
          {
            if (this.CanTurn)
            {
              this.CurrentAction = NpcAction.Turn;
            }
            else
            {
              this.LookingDirection = horizontalDirection;
              this.CurrentAction = this.CanWalk ? NpcAction.Walk : NpcAction.Idle;
            }
          }
          else
            this.CurrentAction = this.CanWalk ? NpcAction.Walk : NpcAction.Idle;
        }
        if (currentAction == this.CurrentAction)
          break;
        this.UpdateAction();
        break;
    }
  }

  private void Hide()
  {
    if (this.CanBurrow)
    {
      this.CurrentAction = NpcAction.Burrow;
      this.burrowSound.EmitAt(this.Position);
    }
    else if (this.CanHide)
    {
      if (this.CurrentAction != NpcAction.Hide)
        this.hideSound.EmitAt(this.Position);
      this.CurrentAction = NpcAction.Hide;
    }
    else if (this.CanTakeOff)
    {
      this.CurrentAction = NpcAction.TakeOff;
      this.takeoffSound.EmitAt(this.Position);
    }
    else
      this.CurrentAction = this.CanIdle ? NpcAction.Idle : NpcAction.Walk;
  }

  protected override float AnimationSpeed
  {
    get
    {
      return this.CurrentAction != NpcAction.Walk && this.CurrentAction != NpcAction.Turn ? 1f : this.Npc.WalkSpeed / ((double) this.originalSpeed == 0.0 ? 1f : this.originalSpeed);
    }
  }

  protected override void TryTalk()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Idle:
      case ActionType.Walking:
      case ActionType.Running:
      case ActionType.Sliding:
        if (this.PlayerManager.Background || !this.SpeechManager.Hidden || this.Npc.ActorType == ActorType.Owl && (this.OwlInvisible || this.CurrentAction == NpcAction.TakeOff || this.CurrentAction == NpcAction.Fly))
          break;
        if (this.Npc.CustomSpeechLine == null)
        {
          if (this.InputManager.CancelTalk != FezButtonState.Pressed)
            break;
          Vector3 vector3_1 = Vector3.UnitY + (this.CameraManager.Viewpoint.SideMask() + this.CameraManager.Viewpoint.DepthMask()) * 1.5f;
          BoundingBox boundingBox = new BoundingBox(this.Position - vector3_1, this.Position + vector3_1);
          Vector3 mask = this.CameraManager.Viewpoint.VisibleAxis().GetMask();
          Vector3 vector3_2 = this.CameraManager.Viewpoint.ForwardVector();
          Ray ray = new Ray()
          {
            Position = this.PlayerManager.Center * (Vector3.One - mask) - vector3_2 * this.LevelManager.Size,
            Direction = vector3_2
          };
          float? nullable = boundingBox.Intersects(ray);
          if (!nullable.HasValue || this.TestObstruction(ray.Position, nullable.Value))
            break;
        }
        this.Talk();
        break;
    }
  }

  private bool TestObstruction(Vector3 hitStart, float hitDistance)
  {
    Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
    Vector3 vector3 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    foreach (TrileInstance trileInstance in (IEnumerable<TrileInstance>) this.LevelManager.Triles.Values)
    {
      if (trileInstance.InstanceId != -1 && (double) ((trileInstance.Center - hitStart) * vector3).LengthSquared() < 0.5)
      {
        Trile trile = trileInstance.Trile;
        if (trileInstance.Enabled && !trile.Immaterial && !trile.SeeThrough)
          return (double) (trileInstance.Position + Vector3.One / 2f + b * -0.5f - hitStart).Dot(b) <= (double) hitDistance + 0.25;
      }
    }
    return false;
  }

  private void Talk()
  {
    bool wasTalking = this.CurrentAction == NpcAction.Talk;
    if (this.Npc.CustomSpeechLine != null)
    {
      this.CurrentLine = this.Npc.CustomSpeechLine;
    }
    else
    {
      SpeechLine currentLine = this.CurrentLine;
      if (this.Npc.Speech.Count <= 1 || this.Npc.SayFirstSpeechLineOnce && !this.SaidFirstLine)
      {
        this.CurrentLine = this.Npc.Speech.FirstOrDefault<SpeechLine>();
      }
      else
      {
        do
        {
          if (this.Npc.RandomizeSpeech)
          {
            this.CurrentLine = RandomHelper.InList<SpeechLine>(this.Npc.Speech);
          }
          else
          {
            this.CurrentLine = this.Npc.Speech[this.SequentialLineIndex];
            ++this.SequentialLineIndex;
            if (this.SequentialLineIndex == this.Npc.Speech.Count)
              this.SequentialLineIndex = 0;
          }
        }
        while (currentLine == this.CurrentLine || this.Npc.SayFirstSpeechLineOnce && this.SaidFirstLine && this.CurrentLine == this.Npc.Speech[0]);
      }
      this.SaidFirstLine = true;
    }
    IPlayerManager playerManager = this.PlayerManager;
    playerManager.Velocity = playerManager.Velocity * Vector3.UnitY;
    Vector3 a = this.PlayerManager.Position - this.Position;
    Vector3 b = this.CameraManager.Viewpoint.SideMask();
    Vector3 vector3_1 = this.CameraManager.Viewpoint.DepthMask();
    float num1 = a.Dot(b);
    if ((double) Math.Abs(num1) < 1.0 && !wasTalking)
    {
      Vector3 vector3_2 = this.Position * b + this.PlayerManager.Center * (Vector3.UnitY + vector3_1);
      Vector3 velocity = this.PlayerManager.Velocity;
      MultipleHits<TrileInstance> ground = this.PlayerManager.Ground;
      Vector3 potentialPosition = vector3_2 + (float) Math.Sign(num1) * 1.125f * this.CameraManager.Viewpoint.SideMask();
      bool flag = true;
      if (!this.CollisionManager.CollidePoint(potentialPosition, Vector3.Down).Collided)
      {
        a *= -1f;
        float num2 = a.Dot(b);
        potentialPosition = vector3_2 + (float) Math.Sign(num2) * 1.125f * this.CameraManager.Viewpoint.SideMask();
        if (!this.CollisionManager.CollidePoint(potentialPosition, Vector3.Down).Collided)
        {
          this.PlayerManager.Action = ActionType.ReadingSign;
          flag = false;
        }
      }
      if (flag)
      {
        this.WalkTo.Destination = (Func<Vector3>) (() => potentialPosition);
        this.WalkTo.NextAction = ActionType.ReadingSign;
        this.PlayerManager.Action = ActionType.WalkingTo;
        this.waitingToSpeak = true;
      }
    }
    else
      this.PlayerManager.Action = ActionType.ReadingSign;
    this.LookingDirection = FezMath.DirectionFromMovement(a.Dot(this.CameraManager.Viewpoint.RightVector()));
    this.PlayerManager.LookingDirection = FezMath.DirectionFromMovement(-a.Dot(this.CameraManager.Viewpoint.RightVector()));
    Action onValid = (Action) (() =>
    {
      this.waitingToSpeak = false;
      this.SpeechManager.Origin = this.Position + Vector3.UnitY * 0.5f;
      string s;
      if (this.LevelManager.SongName == "Majesty")
      {
        this.SpeechManager.Font = SpeechFont.Zuish;
        string stringRaw = GameText.GetStringRaw(this.CurrentLine.Text);
        this.SpeechManager.Origin = this.Position + Vector3.UnitY * 0.5f + this.CameraManager.Viewpoint.RightVector();
        this.SpeechManager.ChangeText(s = stringRaw);
      }
      else
        this.SpeechManager.ChangeText(s = GameText.GetString(this.CurrentLine.Text));
      this.CurrentAction = NpcAction.Talk;
      this.Npc.Talking = true;
      if (!wasTalking && this.Npc.ActorType == ActorType.LightningGhost)
        this.Group.Material.Opacity = 0.0f;
      this.talkWaiter = Waiters.Wait(0.10000000149011612 + 0.075000002980232239 * (double) s.StripPunctuation().Length * (Culture.IsCJK ? 2.0 : 1.0), (Action) (() =>
      {
        if (this.talkEmitter == null)
          return;
        this.talkEmitter.FadeOutAndPause(0.1f);
      }));
      this.talkWaiter.AutoPause = true;
      this.UpdateAction(wasTalking);
      this.SyncTextureMatrix();
    });
    if (this.PlayerManager.Action == ActionType.WalkingTo)
    {
      if (this.CanIdle)
      {
        this.CurrentAction = NpcAction.Idle;
        this.UpdateAction();
      }
      else
      {
        this.CurrentAction = NpcAction.Talk;
        this.UpdateAction();
      }
      Waiters.Wait((Func<bool>) (() => this.PlayerManager.Action != ActionType.WalkingTo), onValid).AutoPause = true;
    }
    else
      onValid();
  }

  protected override bool TryStopTalking()
  {
    bool flag = this.SpeechManager.Hidden && !this.waitingToSpeak || !this.PlayerManager.Grounded;
    if (flag)
    {
      this.waitingToSpeak = false;
      if (this.Npc.CustomSpeechLine == null && !this.Npc.RandomizeSpeech && !this.Npc.SayFirstSpeechLineOnce && this.SequentialLineIndex != 0)
      {
        this.Talk();
        return false;
      }
      if (this.talkWaiter != null && this.talkWaiter.Alive)
        this.talkWaiter.Cancel();
      if (this.talkEmitter != null && !this.talkEmitter.Dead)
        this.talkEmitter.FadeOutAndPause(0.1f);
      if (!this.SpeechManager.Hidden)
        this.SpeechManager.Hide();
      this.Npc.CustomSpeechLine = (SpeechLine) null;
      if (this.Npc.ActorType == ActorType.Owl)
      {
        this.LookingDirection = FezMath.DirectionFromMovement((-(this.PlayerManager.Position - this.Position) * this.Npc.DestinationOffset.Sign()).Dot(this.CameraManager.Viewpoint.SideMask()));
        this.CurrentAction = NpcAction.TakeOff;
        this.UpdateAction();
        ++this.GameState.SaveData.CollectedOwls;
        this.OwlService.OnOwlCollected();
        this.GameState.SaveData.ThisLevel.InactiveNPCs.Add(this.Npc.Id);
        this.LevelService.ResolvePuzzle();
      }
    }
    return flag;
  }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public IGraphicsDeviceService GraphicsDeviceService { private get; set; }

  [ServiceDependency]
  public ILevelService LevelService { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IOwlService OwlService { get; set; }

  [ServiceDependency(Optional = true)]
  public IWalkToService WalkTo { protected get; set; }
}
