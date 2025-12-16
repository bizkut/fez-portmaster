// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.HowToPlayMenuLevel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezGame.Structure;

internal class HowToPlayMenuLevel : MenuLevel
{
  private Texture2D HowToPlayImage;
  private Texture2D HowToPlayImageSony;

  public override void Initialize()
  {
    this.HowToPlayImage = this.CMProvider.Get(CM.Menu).Load<Texture2D>("Other Textures/how_to_play/howtoplay");
    this.HowToPlayImageSony = this.CMProvider.Get(CM.Menu).Load<Texture2D>("Other Textures/how_to_play/howtoplay_SONY");
    base.Initialize();
  }

  public override void PostDraw(
    SpriteBatch batch,
    SpriteFont font,
    GlyphTextRenderer tr,
    float alpha)
  {
    float scale = 4f * batch.GraphicsDevice.GetViewScale();
    Viewport viewport = batch.GraphicsDevice.Viewport;
    double width = (double) viewport.Width;
    viewport = batch.GraphicsDevice.Viewport;
    double height = (double) viewport.Height;
    Vector2 vector2_1 = new Vector2((float) width, (float) height) / 2f;
    Vector2 vector2_2 = new Vector2((float) this.HowToPlayImage.Width, (float) this.HowToPlayImage.Height) * scale;
    batch.End();
    batch.BeginPoint();
    Vector2 vector2_3 = vector2_2 / 2f;
    Vector2 position = vector2_1 - vector2_3;
    if (GamepadState.Layout != GamepadState.GamepadLayout.Xbox360)
      batch.Draw(this.HowToPlayImageSony, position, new Rectangle?(), new Color(1f, 1f, 1f, alpha), 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
    else
      batch.Draw(this.HowToPlayImage, position, new Rectangle?(), new Color(1f, 1f, 1f, alpha), 0.0f, Vector2.Zero, scale, SpriteEffects.None, 0.0f);
  }

  public ITargetRenderingManager TargetRenderer { private get; set; }
}
