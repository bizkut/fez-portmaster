// Decompiled with JetBrains decompiler
// Type: FezGame.Components.FakeIntro
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezGame.Components;

internal class FakeIntro : DrawableGameComponent
{
  private Texture2D PolyLogo;
  private SpriteBatch SpriteBatch;
  private float logoAlpha;
  private float whiteAlpha;
  private float sinceStarted;

  public FakeIntro(Game game)
    : base(game)
  {
    this.DrawOrder = 1000;
  }

  protected override void LoadContent()
  {
    this.PolyLogo = this.CMProvider.Global.Load<Texture2D>("Other Textures/splash/Polytron Logo");
    this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
  }

  public override void Draw(GameTime gameTime)
  {
    this.sinceStarted += (float) gameTime.ElapsedGameTime.TotalSeconds;
    this.logoAlpha = (float) (((double) this.sinceStarted - 1.0) / 1.0);
    this.whiteAlpha = 1f;
    if ((double) this.sinceStarted > 3.0)
      this.logoAlpha = FezMath.Saturate((float) (1.0 - ((double) this.sinceStarted - 3.0) / 0.5));
    if ((double) this.sinceStarted > 3.5)
      this.whiteAlpha = FezMath.Saturate((float) (1.0 - ((double) this.sinceStarted - 4.0) / 0.5));
    this.TRM.DrawFullscreen(new Color(1f, 1f, 1f, this.whiteAlpha));
    Vector2 vector2 = (new Vector2((float) this.GraphicsDevice.Viewport.Width, (float) this.GraphicsDevice.Viewport.Height) / 2f).Round();
    this.SpriteBatch.BeginPoint();
    this.SpriteBatch.Draw(this.PolyLogo, vector2 - (new Vector2((float) this.PolyLogo.Width, (float) this.PolyLogo.Height) / 2f).Round(), new Color(1f, 1f, 1f, this.logoAlpha));
    this.SpriteBatch.End();
    if ((double) this.whiteAlpha != 0.0)
      return;
    ServiceHelper.RemoveComponent<FakeIntro>(this);
  }

  [ServiceDependency]
  public ITargetRenderingManager TRM { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }
}
