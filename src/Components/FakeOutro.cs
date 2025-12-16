// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FakeOutro
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezGame.Components;

internal class FakeOutro : DrawableGameComponent
{
  private Texture2D HappyLogo;
  private SpriteBatch SpriteBatch;
  private float logoAlpha;
  private float whiteAlpha;
  private float sinceStarted;

  public FakeOutro(Game game)
    : base(game)
  {
    this.DrawOrder = 100000;
  }

  protected override void LoadContent()
  {
    this.HappyLogo = this.CMProvider.Global.Load<Texture2D>("Other Textures/splash/FEZHAPPY_BW");
    this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
    this.SoundManager.GlobalVolumeFactor = 0.0f;
  }

  public override void Draw(GameTime gameTime)
  {
    this.sinceStarted += (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.logoAlpha = (double) this.sinceStarted > 0.5 ? 1f : 0.0f;
    this.whiteAlpha = 1f;
    this.TRM.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, this.whiteAlpha));
    Vector2 vector2 = (new Vector2((float) this.GraphicsDevice.Viewport.Width, (float) this.GraphicsDevice.Viewport.Height) / 2f).Round();
    this.SpriteBatch.BeginPoint();
    this.SpriteBatch.Draw(this.HappyLogo, vector2 - (new Vector2((float) this.HappyLogo.Width, (float) this.HappyLogo.Height) / 2f).Round(), new Color(1f, 1f, 1f, this.logoAlpha));
    this.SpriteBatch.End();
    if ((double) this.whiteAlpha != 0.0)
      return;
    ServiceHelper.RemoveComponent<FakeOutro>(this);
  }

  [ServiceDependency]
  public ITargetRenderingManager TRM { get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }
}
