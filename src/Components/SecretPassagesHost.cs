// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SecretPassagesHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using FezGame.Components.Actions;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class SecretPassagesHost(Game game) : GameComponent(game)
{
  private ArtObjectInstance DoorAo;
  private Volume AssociatedVolume;
  private TrileGroup AttachedGroup;
  private bool Accessible;
  private BackgroundPlane GlowPlane;
  private Viewpoint ExpectedViewpoint;
  private bool MoveUp;
  private TimeSpan SinceStarted;
  private Vector3 AoOrigin;
  private Vector3 PlaneOrigin;
  private SoundEffect sRumble;
  private SoundEffect sLightUp;
  private SoundEffect sFadeOut;
  private SoundEmitter eRumble;
  private bool loop;

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanging += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.DoorAo = (ArtObjectInstance) null;
    this.AttachedGroup = (TrileGroup) null;
    this.AssociatedVolume = (Volume) null;
    this.Enabled = false;
    this.sRumble = (SoundEffect) null;
    this.sLightUp = (SoundEffect) null;
    this.sFadeOut = (SoundEffect) null;
    if (this.eRumble != null && !this.eRumble.Dead)
      this.eRumble.Cue.Stop();
    this.eRumble = (SoundEmitter) null;
    foreach (ArtObjectInstance artObjectInstance in (IEnumerable<ArtObjectInstance>) this.LevelManager.ArtObjects.Values)
    {
      if (artObjectInstance.ArtObject.ActorType == ActorType.SecretPassage)
      {
        this.DoorAo = artObjectInstance;
        this.Enabled = true;
        break;
      }
    }
    if (this.GlowPlane != null)
    {
      this.GlowPlane.Dispose();
      this.GlowPlane = (BackgroundPlane) null;
    }
    if (!this.Enabled)
      return;
    this.AttachedGroup = this.LevelManager.Groups[this.DoorAo.ActorSettings.AttachedGroup.Value];
    this.AssociatedVolume = this.LevelManager.Volumes.Values.FirstOrDefault<Volume>((Func<Volume, bool>) (x => x.ActorSettings != null && x.ActorSettings.IsSecretPassage));
    string key = (string) null;
    foreach (Script script in (IEnumerable<Script>) this.LevelManager.Scripts.Values)
    {
      foreach (ScriptAction action in script.Actions)
      {
        if (action.Object.Type == "Level" && action.Operation.Contains("Level"))
        {
          foreach (ScriptTrigger trigger in script.Triggers)
          {
            if (trigger.Object.Type == "Volume" && trigger.Event == "Enter" && trigger.Object.Identifier.HasValue)
              key = action.Arguments[0];
          }
        }
      }
    }
    this.Accessible = this.GameState.SaveData.World.ContainsKey(key);
    this.sRumble = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/MiscActors/Rumble");
    this.sLightUp = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/DoorBitLightUp");
    this.sFadeOut = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/Zu/DoorBitFadeOut");
    if (!this.Accessible)
    {
      this.Enabled = false;
    }
    else
    {
      this.ExpectedViewpoint = this.AssociatedVolume.Orientations.First<FaceOrientation>().AsViewpoint();
      if (!this.LevelManager.WentThroughSecretPassage)
        return;
      this.MoveUp = true;
      this.SinceStarted = TimeSpan.Zero;
      this.AoOrigin = this.DoorAo.Position;
      this.PlaneOrigin = this.DoorAo.Position + this.AssociatedVolume.Orientations.First<FaceOrientation>().AsVector() / (65f / 32f);
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    if (this.GlowPlane == null)
    {
      if (!this.Accessible)
      {
        this.Enabled = false;
      }
      else
      {
        this.GlowPlane = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/glow/secret_passage"))
        {
          Fullbright = true,
          Opacity = this.MoveUp ? 1f : 0.0f,
          Position = this.DoorAo.Position + this.AssociatedVolume.Orientations.First<FaceOrientation>().AsVector() / (65f / 32f),
          Rotation = FezMath.QuaternionFromPhi(this.AssociatedVolume.Orientations.First<FaceOrientation>().ToPhi()),
          AttachedGroup = new int?(this.AttachedGroup.Id)
        };
        this.LevelManager.AddPlane(this.GlowPlane);
      }
    }
    if (this.MoveUp)
    {
      if (this.eRumble == null)
      {
        this.eRumble = this.sRumble.EmitAt(this.DoorAo.Position, true, 0.0f, 0.625f);
        Waiters.Wait(1.25, (Action) (() => this.eRumble.FadeOutAndDie(0.25f))).AutoPause = true;
      }
      this.DoMoveUp(gameTime.ElapsedGameTime);
    }
    else
    {
      if (!this.DoorAo.Visible || this.DoorAo.ActorSettings.Inactive || this.CameraManager.Viewpoint != this.ExpectedViewpoint || this.PlayerManager.Background)
        return;
      Vector3 vector3 = (this.DoorAo.Position - this.PlayerManager.Position).Abs() * this.CameraManager.Viewpoint.ScreenSpaceMask();
      bool b = (double) vector3.X + (double) vector3.Z < 0.75 && (double) vector3.Y < 2.0 && (double) (this.DoorAo.Position - this.PlayerManager.Position).Dot(this.CameraManager.Viewpoint.ForwardVector()) >= 0.0;
      float opacity = this.GlowPlane.Opacity;
      this.GlowPlane.Opacity = MathHelper.Lerp(this.GlowPlane.Opacity, (float) b.AsNumeric(), 0.05f);
      if ((double) this.GlowPlane.Opacity > (double) opacity && (double) opacity > 0.10000000149011612 && this.loop)
      {
        this.sLightUp.EmitAt(this.DoorAo.Position);
        this.loop = false;
      }
      else if ((double) this.GlowPlane.Opacity < (double) opacity && !this.loop)
      {
        this.sFadeOut.EmitAt(this.DoorAo.Position);
        this.loop = true;
      }
      if (!b || !this.PlayerManager.Grounded || this.InputManager.ExactUp != FezButtonState.Pressed)
        return;
      this.Open();
    }
  }

  private void DoMoveUp(TimeSpan elapsed)
  {
    this.SinceStarted += elapsed;
    float amount = Easing.EaseInOut((double) FezMath.Saturate((float) this.SinceStarted.TotalSeconds / 2f), EasingType.Quadratic);
    int num = Math.Sign(this.AttachedGroup.Path.Segments[0].Destination.Y);
    this.GlowPlane.Position = Vector3.Lerp(this.PlaneOrigin + Vector3.UnitY * 2f * (float) num, this.PlaneOrigin, amount);
    this.DoorAo.Position = Vector3.Lerp(this.AoOrigin + Vector3.UnitY * 2f * (float) num, this.AoOrigin, amount);
    if ((double) amount != 1.0)
      return;
    this.MoveUp = false;
  }

  private void Open()
  {
    this.GroupService.RunPathOnce(this.AttachedGroup.Id, false);
    this.PlayerManager.CanControl = false;
    this.Enabled = false;
    this.PlayerManager.Action = ActionType.WalkingTo;
    this.WalkToService.Destination = new Func<Vector3>(this.GetDestination);
    this.WalkToService.NextAction = ActionType.Idle;
    this.eRumble = this.sRumble.EmitAt(this.DoorAo.Position, true);
    Waiters.Interpolate(0.5, (Action<float>) (step => this.GlowPlane.Opacity = Easing.EaseOut(1.0 - (double) step, EasingType.Quadratic))).AutoPause = true;
    Waiters.Wait(1.5, (Action) (() => this.eRumble.FadeOutAndDie(0.5f))).AutoPause = true;
    Waiters.Wait(2.0, (Action) (() =>
    {
      this.PlayerManager.CanControl = true;
      this.PlayerManager.Action = ActionType.OpeningDoor;
      this.PlayerManager.Action = ActionType.Idle;
    })).AutoPause = true;
  }

  private Vector3 GetDestination()
  {
    Viewpoint viewpoint = this.CameraManager.Viewpoint;
    return this.PlayerManager.Position * (Vector3.UnitY + viewpoint.DepthMask()) + this.DoorAo.Position * viewpoint.SideMask();
  }

  [ServiceDependency]
  public IWalkToService WalkToService { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGroupService GroupService { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }
}
