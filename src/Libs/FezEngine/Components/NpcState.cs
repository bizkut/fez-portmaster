// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.NpcState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Linq;

#nullable disable
namespace FezEngine.Components;

public class NpcState : GameComponent
{
  private const int MinSecondsActionToggle = 2;
  private const int MaxSecondsActionToggle = 5;
  public readonly NpcInstance Npc;
  public NpcAction CurrentAction;
  public HorizontalDirection LookingDirection;
  protected SpeechLine CurrentLine;
  protected readonly bool CanIdle2;
  protected readonly bool CanIdle3;
  protected readonly bool CanTurn;
  protected readonly bool CanBurrow;
  protected readonly bool CanHide;
  protected readonly bool CanTakeOff;
  protected bool CanIdle;
  protected bool CanWalk;
  protected bool CanTalk;
  public float WalkStep;
  public bool Scripted;
  protected Group Group;
  protected AnimationTiming CurrentTiming;
  protected AnimatedTexture CurrentAnimation;
  protected SoundEmitter Emitter;
  protected SoundEmitter talkEmitter;
  protected IWaiter talkWaiter;
  private float WalkedDistance;
  private TimeSpan TimeUntilActionChange;
  private TimeSpan TimeSinceActionChange;
  protected SoundEffect flySound;
  protected bool FlyingBack;
  protected bool MayComeBack;
  protected bool OwlInvisible;
  protected Vector2 flySpeed;
  protected bool InBackground;
  private bool initialized;
  protected bool isDisposed;
  private Quaternion oldRotation;
  private HorizontalDirection oldDirection;
  private int lastFrame;
  private Vector3? flyRight;

  public NpcState(Game game, NpcInstance npc)
    : base(game)
  {
    this.Npc = npc;
    this.Npc.State = this;
    foreach (NpcAction key in Util.GetValues<NpcAction>())
    {
      switch (key)
      {
        case NpcAction.None:
        case NpcAction.Idle:
        case NpcAction.Walk:
        case NpcAction.Talk:
          continue;
        default:
          if (!this.Npc.Actions.ContainsKey(key))
          {
            if (MemoryContentManager.AssetExists($"Character Animations\\{this.Npc.Name}\\{(object) key}"))
            {
              this.Npc.Actions.Add(key, new NpcActionContent()
              {
                AnimationName = key.ToString()
              });
              continue;
            }
            continue;
          }
          continue;
      }
    }
    if (MemoryContentManager.AssetExists($"Character Animations\\{this.Npc.Name}\\Metadata"))
      npc.FillMetadata(ServiceHelper.Get<IContentManagerProvider>().CurrentLevel.Load<NpcMetadata>($"Character Animations/{npc.Name}/Metadata"));
    this.CanIdle = this.Npc.Actions.ContainsKey(NpcAction.Idle);
    this.CanIdle2 = this.Npc.Actions.ContainsKey(NpcAction.Idle2);
    this.CanIdle3 = this.Npc.Actions.ContainsKey(NpcAction.Idle3);
    this.CanTalk = this.Npc.Actions.ContainsKey(NpcAction.Talk);
    this.CanWalk = this.Npc.Actions.ContainsKey(NpcAction.Walk);
    this.CanTurn = this.Npc.Actions.ContainsKey(NpcAction.Turn);
    this.CanBurrow = this.Npc.Actions.ContainsKey(NpcAction.Burrow);
    this.CanHide = this.Npc.Actions.ContainsKey(NpcAction.Hide);
    this.CanTakeOff = this.Npc.Actions.ContainsKey(NpcAction.TakeOff);
  }

  public override void Initialize()
  {
    if (this.initialized)
      return;
    this.LoadContent();
    this.Npc.Group = this.Group = this.LevelMaterializer.NpcMesh.AddFace(Vector3.One, Vector3.UnitY / 2f, FaceOrientation.Front, true, true);
    this.Group.Material = new Material();
    this.WalkStep = RandomHelper.Between(0.0, 1.0);
    this.LookingDirection = HorizontalDirection.Right;
    this.UpdatePath();
    this.OwlInvisible = !this.TimeManager.IsDayPhase(DayPhase.Night);
    this.Npc.Enabled = this.Npc.ActorType != ActorType.Owl || !this.OwlInvisible;
    this.Walk(TimeSpan.Zero);
    this.ToggleAction();
    this.CameraManager.ViewpointChanged += new Action(this.UpdateRotation);
    this.CameraManager.ViewpointChanged += new Action(this.UpdatePath);
    this.UpdateRotation();
    this.UpdateScale();
    DrawActionScheduler.Schedule(new Action(this.SyncTextureMatrix));
    this.initialized = true;
  }

  public void SyncTextureMatrix()
  {
    int width = this.CurrentAnimation.Texture.Width;
    int height = this.CurrentAnimation.Texture.Height;
    Rectangle offset = this.CurrentAnimation.Offsets[this.CurrentTiming.Frame];
    this.Group.TextureMatrix.Set(new Matrix?(new Matrix((float) offset.Width / (float) width, 0.0f, 0.0f, 0.0f, 0.0f, (float) offset.Height / (float) height, 0.0f, 0.0f, (float) offset.X / (float) width, (float) offset.Y / (float) height, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f)));
  }

  protected void UpdatePath()
  {
    float walkedDistance = this.WalkedDistance;
    this.WalkedDistance = Math.Abs(this.Npc.DestinationOffset.Dot(this.CameraManager.Viewpoint.SideMask()));
    if ((double) this.WalkedDistance != 0.0)
      return;
    this.WalkedDistance = walkedDistance;
  }

  protected void LoadContent()
  {
    foreach (NpcAction key in this.Npc.Actions.Keys)
    {
      NpcActionContent action = this.Npc.Actions[key];
      action.Animation = this.LoadAnimation(action.AnimationName);
      action.Animation.Timing.Loop = key.Loops();
      if (action.SoundName != null)
        action.Sound = this.LoadSound(action.SoundName);
      else if (this.Npc.Metadata.SoundActions.Contains(key))
        action.Sound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/" + this.Npc.Metadata.SoundPath);
    }
    foreach (NpcActionContent npcActionContent in this.Npc.Speech.Where<SpeechLine>((Func<SpeechLine, bool>) (x => x.OverrideContent != null)).Select<SpeechLine, NpcActionContent>((Func<SpeechLine, NpcActionContent>) (x => x.OverrideContent)))
    {
      if (npcActionContent.AnimationName != null)
        npcActionContent.Animation = this.LoadAnimation(npcActionContent.AnimationName);
      if (npcActionContent.SoundName != null)
        npcActionContent.Sound = this.LoadSound(npcActionContent.SoundName);
    }
    if (!this.CanTakeOff)
      return;
    this.flySound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Wildlife/BirdFly");
  }

  private AnimatedTexture LoadAnimation(string name)
  {
    return this.CMProvider.CurrentLevel.Load<AnimatedTexture>($"Character Animations/{this.Npc.Name}/{name}");
  }

  private SoundEffect LoadSound(string name)
  {
    return this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Npc/" + name);
  }

  protected override void Dispose(bool disposing)
  {
    this.CameraManager.ViewpointChanged -= new Action(this.UpdateRotation);
    this.CameraManager.ViewpointChanged -= new Action(this.UpdatePath);
    this.LevelMaterializer.NpcMesh.RemoveGroup(this.Group);
    if (this.Emitter != null && !this.Emitter.Dead)
      this.Emitter.FadeOutAndDie(0.0f);
    if (this.talkEmitter != null)
    {
      if (!this.talkEmitter.Dead)
        this.talkEmitter.FadeOutAndDie(0.0f);
      this.talkEmitter = (SoundEmitter) null;
    }
    this.isDisposed = true;
  }

  protected void UpdateRotation()
  {
    Quaternion rotation = this.CameraManager.Rotation;
    if (this.oldRotation == rotation && this.oldDirection == this.LookingDirection)
      return;
    this.oldRotation = rotation;
    this.oldDirection = this.LookingDirection;
    if (this.LookingDirection == HorizontalDirection.Left)
      rotation *= FezMath.QuaternionFromPhi(3.14159274f);
    this.Group.Rotation = rotation;
    if (!this.CameraManager.Viewpoint.IsOrthographic())
      return;
    if (this.LevelManager.IsInvalidatingScreen)
      this.LevelManager.ScreenInvalidated += new Action(this.UpdateBackgroundState);
    else
      this.UpdateBackgroundState();
  }

  private void UpdateBackgroundState()
  {
    this.InBackground = false;
    Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
    NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.Position, QueryOptions.Simple);
    TrileInstance trileInstance = nearestTriles.Surface ?? nearestTriles.Deep;
    if (trileInstance == null)
      return;
    Vector3 a = trileInstance.Center + trileInstance.TransformedSize / 2f * -b;
    this.InBackground = (double) this.Position.Dot(b) > (double) a.Dot(b);
  }

  public override void Update(GameTime gameTime)
  {
    TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
    this.Npc.Enabled = this.Npc.ActorType != ActorType.Owl || !this.OwlInvisible;
    if (!this.Scripted && this.CurrentAction.AllowsRandomChange())
    {
      this.TimeSinceActionChange += elapsedGameTime;
      if (this.TimeSinceActionChange > this.TimeUntilActionChange)
        this.ToggleAction();
    }
    else if (!this.CurrentAction.Loops() && this.CurrentTiming.Ended && this.CurrentAction != NpcAction.Hide)
      this.ToggleAction();
    if (this.CurrentAction != NpcAction.Talk)
    {
      if (this.CanTalk && (this.Npc.Speech.Count > 0 || this.Npc.CustomSpeechLine != null))
        this.TryTalk();
      if (this.CurrentAction == NpcAction.Walk)
        this.Walk(elapsedGameTime);
      if (this.CurrentAction == NpcAction.Fly || this.CurrentAction == NpcAction.TakeOff)
        this.Fly(elapsedGameTime);
    }
    else if (this.TryStopTalking() && this.CurrentAction != NpcAction.TakeOff)
      this.ToggleAction();
    if (this.Npc.AvoidsGomez && this.Npc.Visible)
      this.TryFlee();
    if (this.CurrentTiming == null)
      return;
    this.CurrentTiming.Update(elapsedGameTime, this.AnimationSpeed);
    this.SyncTextureMatrix();
  }

  private void Fly(TimeSpan elapsed)
  {
    if (!this.flyRight.HasValue)
      this.flyRight = new Vector3?(Vector3.Transform(Vector3.Right, this.Group.Rotation));
    Vector3 vector = Vector3.Transform(Vector3.Right, this.Group.Rotation);
    if ((!this.FlyingBack || this.Npc.ActorType != ActorType.Owl) && FezMath.AlmostEqual(vector.Abs().Dot(this.flyRight.Value.Abs()), 0.0f, 0.1f))
    {
      Vector3? flyRight = this.flyRight;
      Vector3 vector3 = Vector3.Transform(Vector3.Right, this.Group.Rotation);
      this.flyRight = flyRight.HasValue ? new Vector3?(flyRight.GetValueOrDefault() + vector3) : new Vector3?();
    }
    this.flySpeed = Vector2.Lerp(this.flySpeed, new Vector2(4f, 3f), 0.0333333351f);
    Vector2 vector2 = this.flySpeed * ((1f - FezMath.Frac(this.CurrentTiming.Step + 0.75f)) * new Vector2(0.4f, 0.6f) + new Vector2(0.6f, 0.4f));
    this.Position += (float) elapsed.TotalSeconds * (vector2.X * this.flyRight.Value + Vector3.Up * vector2.Y * (this.FlyingBack ? -1f : 1f));
    this.LookingDirection = (double) this.flyRight.Value.Dot(this.CameraManager.InverseView.Right) > 0.0 ? HorizontalDirection.Right : HorizontalDirection.Left;
    if (this.CameraManager.Viewpoint.IsOrthographic() && this.CameraManager.ViewTransitionReached)
    {
      Vector3 b = this.CameraManager.Viewpoint.ForwardVector();
      if (this.InBackground)
        b *= -1f;
      NearestTriles nearestTriles = this.LevelManager.NearestTrile(this.Position, QueryOptions.Simple);
      TrileInstance trileInstance = nearestTriles.Surface ?? nearestTriles.Deep;
      if (trileInstance != null)
      {
        Vector3 a = trileInstance.Center + trileInstance.TransformedSize / 2f * -b;
        Vector3 vector3 = this.CameraManager.Viewpoint.DepthMask();
        if ((double) this.Position.Dot(b) > (double) a.Dot(b))
          this.Position = this.Position * this.CameraManager.Viewpoint.ScreenSpaceMask() + a * vector3 - b;
      }
    }
    if (this.CurrentTiming.Frame == 0 && this.lastFrame != 0)
      this.flySound.EmitAt(this.Position);
    this.lastFrame = this.CurrentTiming.Frame;
    if (this.FlyingBack)
    {
      if ((double) this.Position.Y > (double) this.Npc.Position.Y)
        return;
      this.Position = this.Npc.Position;
      this.CurrentAction = NpcAction.Land;
      this.flySpeed = Vector2.Zero;
      this.UpdateAction();
      this.FlyingBack = false;
      this.flyRight = new Vector3?();
    }
    else
    {
      if (this.CameraManager.Frustum.Contains(new BoundingBox(this.Position - Vector3.One, this.Position + Vector3.One)) != ContainmentType.Disjoint)
        return;
      if (this.MayComeBack)
      {
        this.OwlInvisible = true;
        this.MayComeBack = false;
        this.CurrentAction = NpcAction.Idle;
        this.flySpeed = Vector2.Zero;
        this.flyRight = new Vector3?();
        this.UpdateAction();
      }
      else
        ServiceHelper.RemoveComponent<NpcState>(this);
    }
  }

  protected virtual void TryTalk()
  {
  }

  protected virtual bool TryStopTalking() => false;

  protected virtual void TryFlee()
  {
  }

  protected virtual float AnimationSpeed => 1f;

  private void Walk(TimeSpan elapsed)
  {
    this.WalkStep += (float) ((double) ((float) this.LookingDirection.Sign() * this.CameraManager.InverseView.Right).Dot(this.Npc.DestinationOffset.Sign()) / ((double) this.WalkedDistance == 0.0 ? 1.0 : (double) this.WalkedDistance) * elapsed.TotalSeconds) * this.Npc.WalkSpeed;
    if (!this.Scripted && ((double) this.WalkStep > 1.0 || (double) this.WalkStep < 0.0))
    {
      this.WalkStep = FezMath.Saturate(this.WalkStep);
      this.ToggleAction();
    }
    else
    {
      this.WalkStep = FezMath.Saturate(this.WalkStep);
      this.Position = Vector3.Lerp(this.Npc.Position, this.Npc.Position + this.Npc.DestinationOffset, this.WalkStep);
    }
  }

  private void ToggleAction()
  {
    NpcAction currentAction = this.CurrentAction;
    if (this.initialized)
      this.RandomizeAction();
    else
      this.CurrentAction = this.CanIdle ? NpcAction.Idle : NpcAction.Walk;
    this.TimeUntilActionChange = new TimeSpan(0, 0, RandomHelper.Random.Next(2, 5));
    this.TimeSinceActionChange = TimeSpan.Zero;
    if (this.initialized && this.CurrentAction == currentAction)
      return;
    this.UpdateAction();
  }

  private void RandomizeAction()
  {
    switch (this.CurrentAction)
    {
      case NpcAction.Turn:
        this.Turn();
        break;
      case NpcAction.Burrow:
        ServiceHelper.RemoveComponent<NpcState>(this);
        break;
      case NpcAction.TakeOff:
        this.CurrentAction = NpcAction.Fly;
        this.UpdateAction();
        break;
      case NpcAction.Land:
        this.CurrentAction = NpcAction.Idle;
        this.UpdateAction();
        break;
      default:
        if ((RandomHelper.Probability(0.5) || !this.CanWalk) && this.CanIdle)
        {
          if (this.CanWalk || RandomHelper.Probability(0.5))
          {
            this.ChooseIdle();
            break;
          }
          if (this.CanTurn)
          {
            this.CurrentAction = NpcAction.Turn;
            break;
          }
          this.Turn();
          break;
        }
        if (!this.CanWalk)
          throw new InvalidOperationException("This NPC can't walk or idle!");
        if ((double) this.WalkStep == 1.0 || (double) this.WalkStep == 0.0)
        {
          if (this.CanIdle && RandomHelper.Probability(0.5))
          {
            this.ChooseIdle();
            break;
          }
          if (this.CanTurn)
          {
            this.CurrentAction = NpcAction.Turn;
            break;
          }
          this.Turn();
          break;
        }
        if (this.CanTurn && RandomHelper.Probability(0.5))
        {
          this.CurrentAction = NpcAction.Turn;
          break;
        }
        this.CurrentAction = NpcAction.Walk;
        break;
    }
  }

  private void Turn()
  {
    this.LookingDirection = this.LookingDirection.GetOpposite();
    this.UpdateRotation();
    this.CurrentAction = this.CanWalk ? NpcAction.Walk : NpcAction.Idle;
  }

  private void ChooseIdle()
  {
    if (this.CurrentAction.IsSpecialIdle())
    {
      this.CurrentAction = NpcAction.Idle;
    }
    else
    {
      float num1 = RandomHelper.Unit();
      float num2 = (float) (1 + this.CanIdle2.AsNumeric() + this.CanIdle3.AsNumeric());
      if ((double) num1 < 1.0 / (double) num2)
        this.CurrentAction = NpcAction.Idle;
      else if ((double) num2 > 1.0 && (double) num1 < 2.0 / (double) num2)
      {
        this.CurrentAction = this.CanIdle2 ? NpcAction.Idle2 : NpcAction.Idle3;
      }
      else
      {
        if ((double) num2 <= 2.0 || (double) num1 >= 3.0 / (double) num2)
          return;
        this.CurrentAction = NpcAction.Idle3;
      }
    }
  }

  public void UpdateAction(bool resumeTiming = false)
  {
    if (this.Emitter != null)
    {
      if (this.Emitter.Cue != null && this.Emitter.Cue.IsLooped)
        this.Emitter.Cue.Stop(true);
      this.Emitter = (SoundEmitter) null;
    }
    AnimatedTexture animation = this.Npc.Actions[this.CurrentAction].Animation;
    SoundEffect sound = this.Npc.Actions[this.CurrentAction].Sound;
    if (this.CurrentAction == NpcAction.Talk && this.CurrentLine != null && this.CurrentLine.OverrideContent != null)
    {
      if (this.CurrentLine.OverrideContent.Animation != null)
        animation = this.CurrentLine.OverrideContent.Animation;
      if (this.CurrentLine.OverrideContent.Sound != null)
        sound = this.CurrentLine.OverrideContent.Sound;
    }
    if (!resumeTiming)
      this.CurrentTiming = animation.Timing.Clone();
    this.CurrentAnimation = animation;
    if (!this.initialized)
      DrawActionScheduler.Schedule((Action) (() => this.Group.Texture = (Texture) animation.Texture));
    else
      this.Group.Texture = (Texture) animation.Texture;
    this.UpdateScale();
    if (sound == null || this.Npc.ActorType == ActorType.Owl && this.OwlInvisible || !this.initialized)
      return;
    if (this.CurrentAction == NpcAction.Talk)
    {
      if (this.talkEmitter == null || this.talkEmitter.Dead)
      {
        this.talkEmitter = sound.EmitAt(this.Position, true, RandomHelper.Centered(0.05));
      }
      else
      {
        this.talkEmitter.Position = this.Position;
        Waiters.Wait(0.10000000149011612, (Action) (() =>
        {
          this.talkEmitter.Cue.Resume();
          this.talkEmitter.VolumeFactor = 1f;
        })).AutoPause = true;
      }
    }
    else
      this.Emitter = sound.EmitAt(this.Position, true, RandomHelper.Centered(0.05));
  }

  private void UpdateScale()
  {
    if (this.CurrentAnimation == null)
      return;
    this.Group.Scale = new Vector3((float) this.CurrentAnimation.Offsets[0].Width / 16f, (float) this.CurrentAnimation.Offsets[0].Height / 16f, 1f);
  }

  protected Vector3 Position
  {
    get => this.Group.Position;
    set => this.Group.Position = value;
  }

  [ServiceDependency]
  public ITimeManager TimeManager { protected get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { protected get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }
}
