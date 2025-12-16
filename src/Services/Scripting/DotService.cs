// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.DotService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Structure;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Linq;

#nullable disable
namespace FezGame.Services.Scripting;

internal class DotService : IDotService, IScriptingBase
{
  private SoundEffect sTextNext;
  private SoundEffect sDotTalk;
  private SoundEmitter eDotTalk;

  public void ResetEvents()
  {
  }

  public LongRunningAction Say(string line, bool nearGomez, bool hideAfter)
  {
    if (this.sTextNext == null)
      this.sTextNext = this.CMProvider.Global.Load<SoundEffect>("Sounds/Ui/TextNext");
    if (this.sDotTalk == null)
      this.sDotTalk = this.CMProvider.Global.Load<SoundEffect>("Sounds/Dot/Talk");
    bool first = false;
    bool wasPreventing = false;
    IWaiter w = (IWaiter) null;
    return new LongRunningAction((Func<float, float, bool>) ((_, __) =>
    {
      if (!first && this.LevelManager.Name == "VILLAGEVILLE_3D" && !line.StartsWith("DOT_INTRO") && this.GameService.GetLevelState != "INTRO_COMPLETE")
        return false;
      if (!first)
      {
        switch (this.PlayerManager.Action)
        {
          case ActionType.Idle:
          case ActionType.Walking:
          case ActionType.Running:
          case ActionType.Sliding:
          case ActionType.Teetering:
            if (Intro.Instance != null)
              return false;
            this.Dot.ComeOut();
            wasPreventing = this.Dot.PreventPoI;
            this.Dot.PreventPoI = true;
            if (nearGomez || this.Dot.Hidden || this.Dot.Burrowing || this.Dot.Behaviour == DotHost.BehaviourType.RoamInVolume)
              this.Dot.Behaviour = DotHost.BehaviourType.ReadyToTalk;
            string str = GameText.GetString(line);
            this.SpeechBubble.Font = SpeechFont.Pixel;
            this.SpeechBubble.ChangeText(str);
            this.PlayerManager.CanControl = false;
            this.SpeechBubble.Origin = this.Dot.Position;
            first = true;
            if (this.eDotTalk == null || this.eDotTalk.Dead)
            {
              this.eDotTalk = this.sDotTalk.EmitAt(this.Dot.Position, true);
            }
            else
            {
              this.eDotTalk.Cue.Resume();
              this.eDotTalk.VolumeFactor = 1f;
              this.eDotTalk.Position = this.Dot.Position;
            }
            w = Waiters.Wait(0.075000002980232239 * (double) str.StripPunctuation().Length * (Culture.IsCJK ? 2.0 : 1.0), (Action) (() =>
            {
              if (this.eDotTalk == null)
                return;
              this.eDotTalk.FadeOutAndPause(0.1f);
            }));
            w.AutoPause = true;
            break;
          default:
            return false;
        }
      }
      this.SpeechBubble.Origin = this.Dot.Position;
      if (this.eDotTalk != null && !this.eDotTalk.Dead)
        this.eDotTalk.Position = this.Dot.Position;
      if (this.InputManager.CancelTalk != FezButtonState.Pressed)
        return false;
      if (w.Alive)
        w.Cancel();
      if (this.eDotTalk != null && !this.eDotTalk.Dead && this.eDotTalk.Cue.State == SoundState.Playing)
        this.eDotTalk.Cue.Pause();
      this.sTextNext.Emit();
      this.SpeechBubble.Hide();
      return true;
    }), (Action) (() =>
    {
      if (hideAfter)
      {
        this.Dot.Burrow();
        if (this.LevelManager.Name == "VILLAGEVILLE_3D")
          this.CameraManager.Constrained = false;
      }
      this.Dot.PreventPoI = wasPreventing;
      this.PlayerManager.CanControl = true;
    }));
  }

  public LongRunningAction ComeBackAndHide(bool withCamera)
  {
    this.Dot.MoveWithCamera(this.PlayerManager.Position + new Vector3(0.0f, 1f, 0.0f), true);
    return new LongRunningAction((Func<float, float, bool>) ((_, __) => this.Dot.Burrowing), (Action) (() =>
    {
      this.CameraManager.Constrained = false;
      this.Dot.PreventPoI = false;
    }));
  }

  public LongRunningAction SpiralAround(bool withCamera, bool hideDot)
  {
    this.Dot.PreventPoI = true;
    Vector3 vector3_1 = this.LevelManager.Triles.Values.Aggregate<TrileInstance, Vector3>(Vector3.Zero, (Func<Vector3, TrileInstance, Vector3>) ((a, b) => a + b.Position)) / (float) this.LevelManager.Triles.Values.Count;
    Vector3 vector3_2 = this.LevelManager.Size + 4f * Vector3.UnitY;
    if (this.LevelManager.Name == "SEWER_HUB")
      vector3_2 -= Vector3.UnitY * 20f;
    if (this.LevelManager.Name.EndsWith("BIG_TOWER"))
      vector3_2 -= Vector3.UnitY * 13.5f;
    IDotManager dot = this.Dot;
    Volume volume = new Volume();
    volume.From = Vector3.Zero;
    volume.To = vector3_2;
    Vector3 center = vector3_1;
    int num = hideDot ? 1 : 0;
    dot.SpiralAround(volume, center, num != 0);
    return new LongRunningAction((Func<float, float, bool>) ((_, __) => this.Dot.Behaviour == DotHost.BehaviourType.ReadyToTalk), (Action) (() =>
    {
      this.CameraManager.Constrained = false;
      this.Dot.PreventPoI = false;
    }));
  }

  [ServiceDependency]
  public IGameService GameService { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IDotManager Dot { private get; set; }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }
}
