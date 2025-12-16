// Decompiled with JetBrains decompiler
// Type: FezGame.Components.LogoRenderer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezGame.Components;

internal class LogoRenderer(Game game) : DrawableGameComponent(game)
{
  private Texture2D TrapdoorLogo;
  private SpriteBatch SpriteBatch;

  public override void Initialize()
  {
    base.Initialize();
    this.TrapdoorLogo = this.CMProvider.Global.Load<Texture2D>("Other Textures/splash/trapdoor");
    this.SpriteBatch = new SpriteBatch(this.GraphicsDevice);
  }

  public override void Draw(GameTime gameTime)
  {
    base.Draw(gameTime);
    Viewport viewport = this.GraphicsDevice.Viewport;
    double width = (double) viewport.Width;
    viewport = this.GraphicsDevice.Viewport;
    double height = (double) viewport.Height;
    Vector2 vector2 = (new Vector2((float) width, (float) height) / 2f).Round();
    this.GraphicsDevice.Clear(Color.White);
    this.SpriteBatch.BeginPoint();
    this.SpriteBatch.Draw(this.TrapdoorLogo, vector2 - (new Vector2((float) this.TrapdoorLogo.Width, (float) this.TrapdoorLogo.Height) / 2f).Round(), new Color(1f, 1f, 1f, 1f));
    this.SpriteBatch.End();
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
