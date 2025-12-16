// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Reboot
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

public class Reboot : DrawableGameComponent
{
  private const float WaitTime = 3f;
  private const float TimeUntilLogo = 1f;
  private const float TimeUntilBootup = 4f;
  private Texture2D BootTexture;
  private Texture2D LaserCheckTexture;
  private RebootPOSTEffect effect;
  private TimeSpan SinceCreated;
  private SoundEffect RebootSound;
  private bool hasPlayedSound;
  private readonly string ToLevel = "GOMEZ_INTERIOR_3D";

  public Reboot(Game game, string toLevel)
    : base(game)
  {
    if (toLevel != null)
      this.ToLevel = toLevel;
    this.DrawOrder = 1005;
  }

  protected override void LoadContent()
  {
    ContentManager contentManager = this.CMProvider.Get(CM.Reboot);
    this.BootTexture = contentManager.Load<Texture2D>("Other Textures/reboot/boot");
    this.LaserCheckTexture = contentManager.Load<Texture2D>("Other Textures/reboot/lasercheck");
    this.RebootSound = contentManager.Load<SoundEffect>("Sounds/Intro/Reboot");
    this.effect = new RebootPOSTEffect();
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    this.effect.Dispose();
    this.CMProvider.Dispose(CM.Reboot);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.SinceCreated.TotalSeconds <= 7.0)
      return;
    if (this.GameState.InCutscene && Intro.Instance != null)
      ServiceHelper.RemoveComponent<Intro>(Intro.Instance);
    Intro component = new Intro(this.Game);
    component.Fake = true;
    component.FakeLevel = this.ToLevel;
    component.Glitch = true;
    this.TimeManager.TimeFactor = this.TimeManager.DefaultTimeFactor;
    this.TimeManager.CurrentTime = DateTime.Now;
    ServiceHelper.AddComponent((IGameComponent) component);
    Waiters.Wait(0.10000000149011612, (Action) (() => ServiceHelper.RemoveComponent<Reboot>(this)));
    this.Enabled = false;
  }

  public override void Draw(GameTime gameTime)
  {
    this.SinceCreated += gameTime.ElapsedGameTime;
    this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    if (this.SinceCreated.TotalSeconds > 3.0)
    {
      if (!this.hasPlayedSound)
      {
        this.RebootSound.Emit();
        this.hasPlayedSound = true;
      }
      double num1 = this.SinceCreated.TotalSeconds - 3.0;
      float viewScale = this.GraphicsDevice.GetViewScale();
      float width1 = (float) this.GraphicsDevice.Viewport.Width;
      float height1 = (float) this.GraphicsDevice.Viewport.Height;
      float width2 = (float) this.BootTexture.Width;
      float height2 = (float) this.BootTexture.Height;
      this.TargetRenderer.DrawFullscreen((Texture) this.BootTexture, new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, -0.5f, -0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f) * new Matrix((float) ((double) width1 / (double) width2 / 2.0) / viewScale, 0.0f, 0.0f, 0.0f, 0.0f, (float) ((double) height1 / (double) height2 / 2.0) / viewScale, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f) * new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.56f, 0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
      float num2 = (float) (num1 / 4.0);
      this.effect.PseudoWorld = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, (float) (-((double) num2 < 0.20000000298023224 ? 0.2199999988079071 : ((double) num2 < 0.40000000596046448 ? 0.29300001263618469 : ((double) num2 < 0.60000002384185791 ? 0.36100000143051147 : ((double) num2 < 0.699999988079071 ? 0.527999997138977 : ((double) num2 < 0.800000011920929 ? 0.699999988079071 : 1.0))))) * 2.0), 0.0f, 1f);
      this.TargetRenderer.DrawFullscreen((BaseEffect) this.effect, Color.Black);
      if (num1 <= 1.0)
        return;
      float width3 = (float) this.LaserCheckTexture.Width;
      float height3 = (float) this.LaserCheckTexture.Height;
      this.TargetRenderer.DrawFullscreen((Texture) this.LaserCheckTexture, new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, -0.5f, -0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f) * new Matrix((float) ((double) width1 / (double) width3 / 2.0) / viewScale, 0.0f, 0.0f, 0.0f, 0.0f, (float) ((double) height1 / (double) height3 / 2.0) / viewScale, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f) * new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, -1.15f, 1f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f));
    }
    else
      this.TargetRenderer.DrawFullscreen(Color.Black);
  }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
