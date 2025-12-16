// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Actions.Suffer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Components.Actions;

public class Suffer(Game game) : PlayerAction(game)
{
  private static readonly TimeSpan HurtTime = TimeSpan.FromSeconds(1.0);
  private const float RepelStrength = 0.0625f;
  private TimeSpan sinceHurt;
  private bool causedByHurtActor;
  private bool doneFor;
  private ScreenFade fade;

  protected override void TestConditions()
  {
    switch (this.PlayerManager.Action)
    {
      case ActionType.Dying:
        break;
      case ActionType.Suffering:
        break;
      case ActionType.SuckedIn:
        break;
      default:
        bool flag = false;
        foreach (PointCollision pointCollision in this.PlayerManager.CornerCollision)
        {
          flag = ((flag ? 1 : 0) | (pointCollision.Instances.Surface == null ? 0 : (pointCollision.Instances.Surface.Trile.ActorSettings.Type == ActorType.Hurt ? 1 : 0))) != 0;
          if (flag)
            break;
        }
        if (!flag)
          break;
        this.PlayerManager.Action = ActionType.Suffering;
        this.causedByHurtActor = true;
        this.doneFor = (double) this.PlayerManager.RespawnPosition.Y < (double) this.LevelManager.WaterHeight - 0.25;
        this.fade = (ScreenFade) null;
        break;
    }
  }

  protected override void Begin()
  {
    base.Begin();
    if (!this.PlayerManager.CanControl)
      return;
    if (this.PlayerManager.HeldInstance != null)
    {
      this.PlayerManager.HeldInstance = (TrileInstance) null;
      this.PlayerManager.Action = ActionType.Idle;
      this.PlayerManager.Action = ActionType.Suffering;
    }
    this.PlayerManager.CarriedInstance = (TrileInstance) null;
    if (!this.causedByHurtActor)
      this.PlayerManager.Velocity = Vector3.Zero;
    else
      this.PlayerManager.Velocity = 1f / 16f * (this.CameraManager.Viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.GetOpposite().Sign() + Vector3.UnitY);
  }

  protected override bool Act(TimeSpan elapsed)
  {
    if (!this.PlayerManager.CanControl)
      return true;
    if (this.fade == null && this.sinceHurt.TotalSeconds > (this.doneFor ? 1.25 : 1.0))
    {
      this.sinceHurt = TimeSpan.Zero;
      this.causedByHurtActor = false;
      if (this.doneFor)
      {
        this.fade = new ScreenFade(ServiceHelper.Game)
        {
          FromColor = ColorEx.TransparentBlack,
          ToColor = Color.Black,
          Duration = 1f
        };
        ServiceHelper.AddComponent((IGameComponent) this.fade);
        this.fade.Faded += new Action(this.Respawn);
      }
      else
        this.PlayerManager.Action = ActionType.Idle;
    }
    else
    {
      this.sinceHurt += elapsed;
      this.PlayerManager.BlinkSpeed = Easing.EaseIn(this.sinceHurt.TotalSeconds / 1.25, EasingType.Cubic) * 1.5f;
    }
    return true;
  }

  private void Respawn()
  {
    ServiceHelper.AddComponent((IGameComponent) new ScreenFade(ServiceHelper.Game)
    {
      FromColor = Color.Black,
      ToColor = ColorEx.TransparentBlack,
      Duration = 1.5f
    });
    this.GameState.LoadSaveFile((Action) (() =>
    {
      this.GameState.Loading = true;
      this.LevelManager.ChangeLevel(this.LevelManager.Name);
      this.GameState.ScheduleLoadEnd = true;
      this.PlayerManager.RespawnAtCheckpoint();
      this.LevelMaterializer.ForceCull();
    }));
  }

  protected override bool IsActionAllowed(ActionType type) => type == ActionType.Suffering;
}
