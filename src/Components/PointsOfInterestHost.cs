// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PointsOfInterestHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class PointsOfInterestHost(Game game) : GameComponent(game)
{
  private Volume[] PointsList;
  private bool InGroup;
  private SoundEffect sDotTalk;
  private SoundEmitter eDotTalk;
  private IWaiter talkWaiter;

  public override void Initialize()
  {
    base.Initialize();
    this.sDotTalk = this.CMProvider.Global.Load<SoundEffect>("Sounds/Dot/Talk");
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Dot.Reset();
    this.PointsList = this.LevelManager.Volumes.Values.Where<Volume>((Func<Volume, bool>) (x => x.ActorSettings != null && x.Enabled && x.ActorSettings.IsPointOfInterest)).ToArray<Volume>();
    this.SyncTutorials();
  }

  private void SyncTutorials()
  {
    foreach (Volume points in this.PointsList)
    {
      foreach (DotDialogueLine dotDialogueLine in points.ActorSettings.DotDialogue)
      {
        bool flag;
        if (this.GameState.SaveData.OneTimeTutorials.TryGetValue(dotDialogueLine.ResourceText, out flag) & flag)
        {
          points.ActorSettings.PreventHey = true;
          break;
        }
      }
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.InMap || this.PointsList.Length == 0 || this.Dot.PreventPoI || this.GameState.Paused || this.GameState.InMenuCube || this.Dot.Owner != null && this.Dot.Owner != this || this.GameState.FarawaySettings.InTransition || this.PlayerManager.Action.IsEnteringDoor())
      return;
    Vector3 vector3 = this.CameraManager.Viewpoint.DepthMask();
    BoundingBox boundingBox1 = new BoundingBox(this.PlayerManager.Center - new Vector3(6f), this.PlayerManager.Center + new Vector3(6f));
    Volume volume = (Volume) null;
    foreach (Volume points in this.PointsList)
    {
      if (points.Enabled)
      {
        BoundingBox boundingBox2 = points.BoundingBox;
        boundingBox2.Min -= vector3 * 1000f;
        boundingBox2.Max += vector3 * 1000f;
        if (boundingBox1.Contains(boundingBox2) != ContainmentType.Disjoint)
        {
          this.Dot.ComeOut();
          this.Dot.Behaviour = DotHost.BehaviourType.RoamInVolume;
          this.Dot.RoamingVolume = volume = points;
        }
        if (this.SpeechBubble.Hidden)
        {
          if (this.talkWaiter != null && this.talkWaiter.Alive)
            this.talkWaiter.Cancel();
          if (this.eDotTalk != null && !this.eDotTalk.Dead)
            this.eDotTalk.FadeOutAndPause(0.1f);
        }
        if (!this.SpeechBubble.Hidden && this.PlayerManager.CurrentVolumes.Contains(points) && points.ActorSettings.DotDialogue.Count > 0 && (this.PlayerManager.Action == ActionType.Suffering || this.PlayerManager.Action == ActionType.SuckedIn || this.PlayerManager.Action == ActionType.LesserWarp || this.PlayerManager.Action == ActionType.GateWarp))
        {
          this.SpeechBubble.Hide();
          this.Dot.Behaviour = DotHost.BehaviourType.ReadyToTalk;
          this.InGroup = false;
        }
        if (!this.GameState.InFpsMode && this.PlayerManager.CurrentVolumes.Contains(points) && points.ActorSettings.DotDialogue.Count > 0)
        {
          if (this.SpeechBubble.Hidden && (this.InputManager.CancelTalk == FezButtonState.Pressed || this.InGroup))
          {
            switch (this.PlayerManager.Action)
            {
              case ActionType.Idle:
              case ActionType.Walking:
              case ActionType.Running:
              case ActionType.Sliding:
              case ActionType.Landing:
              case ActionType.IdlePlay:
              case ActionType.IdleSleep:
              case ActionType.IdleLookAround:
              case ActionType.IdleYawn:
                points.ActorSettings.NextLine = (points.ActorSettings.NextLine + 1) % points.ActorSettings.DotDialogue.Count;
                int index = (points.ActorSettings.NextLine + 1) % points.ActorSettings.DotDialogue.Count;
                this.InGroup = points.ActorSettings.DotDialogue[points.ActorSettings.NextLine].Grouped && points.ActorSettings.DotDialogue[index].Grouped && index != 0;
                points.ActorSettings.PreventHey = true;
                string resourceText = points.ActorSettings.DotDialogue[points.ActorSettings.NextLine].ResourceText;
                bool flag;
                if (this.GameState.SaveData.OneTimeTutorials.TryGetValue(resourceText, out flag) && !flag)
                {
                  this.GameState.SaveData.OneTimeTutorials[resourceText] = true;
                  this.SyncTutorials();
                }
                if (this.talkWaiter != null && this.talkWaiter.Alive)
                  this.talkWaiter.Cancel();
                string str = GameText.GetString(resourceText);
                this.SpeechBubble.ChangeText(str);
                this.PlayerManager.Action = ActionType.ReadingSign;
                if (this.eDotTalk == null || this.eDotTalk.Dead)
                {
                  this.eDotTalk = this.sDotTalk.EmitAt(this.Dot.Position, true);
                }
                else
                {
                  this.eDotTalk.Position = this.Dot.Position;
                  Waiters.Wait(0.10000000149011612, (Action) (() =>
                  {
                    this.eDotTalk.Cue.Resume();
                    this.eDotTalk.VolumeFactor = 1f;
                  })).AutoPause = true;
                }
                this.talkWaiter = Waiters.Wait(0.10000000149011612 + 0.075000002980232239 * (double) str.StripPunctuation().Length * (Culture.IsCJK ? 2.0 : 1.0), (Action) (() =>
                {
                  if (this.eDotTalk == null)
                    return;
                  this.eDotTalk.FadeOutAndPause(0.1f);
                }));
                this.talkWaiter.AutoPause = true;
                break;
            }
          }
          if (this.SpeechBubble.Hidden && !points.ActorSettings.PreventHey)
          {
            if (this.PlayerManager.Grounded)
            {
              switch (this.PlayerManager.Action)
              {
                case ActionType.Idle:
                case ActionType.Walking:
                case ActionType.Running:
                case ActionType.Sliding:
                case ActionType.Landing:
                case ActionType.IdlePlay:
                case ActionType.IdleSleep:
                case ActionType.IdleLookAround:
                case ActionType.IdleYawn:
                  this.Dot.Behaviour = DotHost.BehaviourType.ThoughtBubble;
                  this.Dot.FaceButton = DotFaceButton.B;
                  if (this.Dot.Owner != this)
                    this.Dot.Hey();
                  this.Dot.Owner = (object) this;
                  break;
                default:
                  this.Dot.Behaviour = DotHost.BehaviourType.ReadyToTalk;
                  break;
              }
            }
          }
          else
            this.Dot.Behaviour = DotHost.BehaviourType.ReadyToTalk;
          if (!this.SpeechBubble.Hidden)
            this.SpeechBubble.Origin = this.Dot.Position;
          if (this.Dot.Behaviour == DotHost.BehaviourType.ReadyToTalk || this.Dot.Behaviour == DotHost.BehaviourType.ThoughtBubble)
            break;
        }
      }
    }
    if (this.Dot.Behaviour != DotHost.BehaviourType.ThoughtBubble && this.Dot.Owner == this && this.SpeechBubble.Hidden)
      this.Dot.Owner = (object) null;
    if (volume != null || this.Dot.RoamingVolume == null)
      return;
    this.Dot.Burrow();
  }

  [ServiceDependency]
  public IDotManager Dot { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
