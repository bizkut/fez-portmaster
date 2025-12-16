// Decompiled with JetBrains decompiler
// Type: FezGame.Components.EndCutscene64.ZoomOut
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

#nullable disable
namespace FezGame.Components.EndCutscene64;

internal class ZoomOut : DrawableGameComponent
{
  private readonly EndCutscene64Host Host;
  private float StepTime;
  private ZoomOut.State ActiveState;
  private float OldSfxVol;
  private Vector3 OriginalCenter;

  public ZoomOut(Game game, EndCutscene64Host host)
    : base(game)
  {
    this.Host = host;
    this.DrawOrder = 1000;
    this.UpdateOrder = 1000;
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.ActualAmbient = new Color(0.25f, 0.25f, 0.25f);
    this.LevelManager.ActualDiffuse = Color.White;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused)
      return;
    float totalSeconds = (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.StepTime += totalSeconds;
    if (this.ActiveState == ZoomOut.State.Wait)
    {
      if ((double) this.StepTime > 5.0)
      {
        this.OldSfxVol = this.SoundManager.SoundEffectVolume;
        this.CameraManager.Constrained = true;
        this.OriginalCenter = this.CameraManager.Center;
        this.ChangeState();
      }
    }
    else if (this.ActiveState == ZoomOut.State.Zooming)
    {
      this.CameraManager.Radius *= MathHelper.Lerp(1f, 1.05f, Easing.EaseIn((double) FezMath.Saturate(this.StepTime / 35f), EasingType.Quadratic));
      this.CameraManager.Center = Vector3.Lerp(this.OriginalCenter, this.LevelManager.Size / 2f, Easing.EaseInOut((double) FezMath.Saturate(this.StepTime / 15f), EasingType.Sine));
      this.SoundManager.SoundEffectVolume = (float) (1.0 - (double) FezMath.Saturate(this.StepTime / 33f) * 0.89999997615814209);
      if ((double) this.StepTime > 33.0)
        this.ChangeState();
    }
    if ((double) totalSeconds == 0.0 || !Keyboard.GetState().IsKeyDown(Keys.R))
      return;
    this.ActiveState = ZoomOut.State.Zooming;
    this.ChangeState();
  }

  private void ChangeState()
  {
    if (this.ActiveState == ZoomOut.State.Zooming)
    {
      this.SoundManager.KillSounds();
      this.SoundManager.SoundEffectVolume = this.OldSfxVol;
      this.Host.Cycle();
    }
    else
    {
      this.StepTime = 0.0f;
      ++this.ActiveState;
      this.Update(new GameTime());
    }
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.ActiveState == ZoomOut.State.Wait || this.GameState.Loading || (double) this.StepTime <= 25.0)
      return;
    this.TargetRenderer.DrawFullscreen(new Color(0.270588249f, 0.9764706f, 1f, Easing.EaseInOut((double) FezMath.Saturate((float) (((double) this.StepTime - 25.0) / 7.0)), EasingType.Sine)));
  }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  private enum State
  {
    Wait,
    Zooming,
  }
}
