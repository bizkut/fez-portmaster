// Decompiled with JetBrains decompiler
// Type: FezGame.Components.IdleRestarter
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

internal class IdleRestarter(Game game) : GameComponent(game)
{
  private const float Timeout = 1f;
  private float counter;

  public override void Update(GameTime gameTime)
  {
    base.Update(gameTime);
    if (this.InputManager.AnyButtonPressed() || this.InputManager.Down != FezButtonState.Up || this.InputManager.Up != FezButtonState.Up || this.InputManager.Left != FezButtonState.Up || this.InputManager.Right != FezButtonState.Up)
    {
      this.counter = 0.0f;
    }
    else
    {
      if (Intro.Instance != null || this.GameState.InCutscene || !this.PlayerManager.CanControl && this.SpeechBubble.Hidden && !this.GameState.InMenuCube && !this.GameState.InMap)
        return;
      this.counter += (float) gameTime.ElapsedGameTime.TotalMinutes;
      if ((double) this.counter < 1.0)
        return;
      this.GameState.Restart();
      this.counter = 0.0f;
    }
  }

  [ServiceDependency]
  public ISpeechBubbleManager SpeechBubble { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }
}
