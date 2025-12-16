// Decompiled with JetBrains decompiler
// Type: FezGame.Components.HeadsUpDisplay
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Components;

public class HeadsUpDisplay(Game game) : DrawableGameComponent(game)
{
  private static readonly TimeSpan HudVisibleTime = TimeSpan.FromSeconds(3.0);
  private static readonly TimeSpan HudFadeInTime = TimeSpan.FromSeconds(0.25);
  private static readonly TimeSpan HudFadeOutTime = TimeSpan.FromSeconds(1.0);
  private SpriteBatch spriteBatch;
  private Texture2D keyIcon;
  private Texture2D cubeIcon;
  private Texture2D antiIcon;
  private Texture2D[] smallCubes;
  private TimeSpan sinceHudUpdate;
  private GlyphTextRenderer tr;
  private string cubeShardsText;
  private string antiText;
  private string keysText;
  private TimeSpan sinceSave;

  public override void Initialize()
  {
    this.DrawOrder = 10000;
    this.GameState.HudElementChanged += new Action(this.RefreshHud);
    this.RefreshHud();
    base.Initialize();
  }

  private void RefreshHud()
  {
    long ticks1 = this.sinceHudUpdate.Ticks;
    TimeSpan timeSpan1 = HeadsUpDisplay.HudFadeInTime;
    long ticks2 = timeSpan1.Ticks;
    timeSpan1 = HeadsUpDisplay.HudVisibleTime;
    long ticks3 = timeSpan1.Ticks;
    long num1 = ticks2 + ticks3;
    timeSpan1 = HeadsUpDisplay.HudFadeOutTime;
    long ticks4 = timeSpan1.Ticks;
    long num2 = num1 + ticks4;
    if (ticks1 > num2)
    {
      this.sinceHudUpdate = TimeSpan.Zero;
    }
    else
    {
      long ticks5 = this.sinceHudUpdate.Ticks;
      TimeSpan timeSpan2 = HeadsUpDisplay.HudFadeInTime;
      long ticks6 = timeSpan2.Ticks;
      timeSpan2 = HeadsUpDisplay.HudVisibleTime;
      long num3 = timeSpan2.Ticks * 4L / 5L;
      long num4 = ticks6 + num3;
      if (ticks5 > num4)
        this.sinceHudUpdate = TimeSpan.FromTicks(HeadsUpDisplay.HudFadeInTime.Ticks + HeadsUpDisplay.HudVisibleTime.Ticks * 4L / 5L);
    }
    this.cubeShardsText = this.GameState.SaveData.CubeShards.ToString();
    this.antiText = this.GameState.SaveData.SecretCubes.ToString();
    this.keysText = this.GameState.SaveData.Keys.ToString();
  }

  protected override void LoadContent()
  {
    this.tr = new GlyphTextRenderer(this.Game);
    DrawActionScheduler.Schedule((Action) (() =>
    {
      this.keyIcon = this.CMProvider.Global.Load<Texture2D>("Other Textures/hud/KEY_CUBE");
      this.cubeIcon = this.CMProvider.Global.Load<Texture2D>("Other Textures/hud/NORMAL_CUBE");
      this.antiIcon = this.CMProvider.Global.Load<Texture2D>("Other Textures/hud/ANTI_CUBE");
      this.smallCubes = new Texture2D[8];
      for (int index = 0; index < 8; ++index)
        this.smallCubes[index] = this.CMProvider.Global.Load<Texture2D>("Other Textures/smallcubes/sc_" + (object) (index + 1));
      this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
    }));
  }

  public override void Update(GameTime gameTime)
  {
    this.sinceHudUpdate += gameTime.ElapsedGameTime;
    if (this.GameState.InMenuCube)
      this.RefreshHud();
    if (this.GameState.Saving)
    {
      if (this.sinceSave.Ticks < HeadsUpDisplay.HudFadeInTime.Ticks)
        this.sinceSave += gameTime.ElapsedGameTime;
    }
    else if (this.sinceSave.Ticks > 0L)
      this.sinceSave -= gameTime.ElapsedGameTime;
    base.Update(gameTime);
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.InCutscene || this.GameState.HideHUD && !this.GameState.Paused)
      return;
    this.GraphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    this.spriteBatch.BeginPoint();
    if (!Fez.LongScreenshot)
    {
      this.DrawHud();
      this.DrawDebugging();
    }
    this.spriteBatch.End();
  }

  private void DrawHud()
  {
    Vector2 position = new Vector2(50f, 58f);
    float linearStep1 = FezMath.Saturate((float) this.sinceHudUpdate.Ticks / (float) HeadsUpDisplay.HudFadeInTime.Ticks);
    float linearStep2 = FezMath.Saturate((float) (this.sinceHudUpdate.Ticks - HeadsUpDisplay.HudVisibleTime.Ticks) / (float) HeadsUpDisplay.HudFadeOutTime.Ticks);
    if ((double) linearStep2 == 1.0)
      return;
    Color color = new Color(1f, 1f, 1f, Easing.EaseOut((double) linearStep1, EasingType.Quadratic) - Easing.EaseOut((double) linearStep2, EasingType.Quadratic));
    SpriteFont font = Culture.IsCJK ? this.Fonts.Big : this.Fonts.Small;
    float num1 = Culture.IsCJK ? this.Fonts.BigFactor * 0.625f : this.Fonts.SmallFactor;
    int scale1 = FezMath.Round((double) this.GraphicsDevice.GetViewScale());
    float scale2 = num1 * (float) scale1;
    Vector2 vector2_1 = new Vector2(60f, 32f);
    if (this.GameState.SaveData.CollectedParts > 0)
    {
      int num2 = Math.Min(this.GameState.SaveData.CollectedParts, 8);
      this.spriteBatch.Draw(this.smallCubes[num2 - 1], position, new Rectangle?(), color, 0.0f, Vector2.Zero, (float) scale1, SpriteEffects.None, 0.0f);
      string text = num2.ToString();
      Vector2 vector2_2 = this.Fonts.Small.MeasureString(text) * scale2;
      this.tr.DrawShadowedText(this.spriteBatch, font, text, position + vector2_1 * (float) scale1 - vector2_2 * Vector2.UnitY / 2f + this.Fonts.TopSpacing * this.Fonts.SmallFactor * Vector2.UnitY, color, scale2, Color.Black, 1f, 1f);
      position.Y += (float) (51 * scale1);
    }
    this.spriteBatch.Draw(this.cubeIcon, position, new Rectangle?(), color, 0.0f, Vector2.Zero, (float) scale1, SpriteEffects.None, 0.0f);
    Vector2 vector2_3 = this.Fonts.Small.MeasureString(this.cubeShardsText) * scale2;
    this.tr.DrawShadowedText(this.spriteBatch, font, this.cubeShardsText, position + vector2_1 * (float) scale1 - vector2_3 * Vector2.UnitY / 2f + this.Fonts.TopSpacing * this.Fonts.SmallFactor * Vector2.UnitY, color, scale2, Color.Black, 1f, 1f);
    position.Y += (float) (51 * scale1);
    if (this.GameState.SaveData.SecretCubes > 0)
    {
      this.spriteBatch.Draw(this.antiIcon, position, new Rectangle?(), color, 0.0f, Vector2.Zero, (float) scale1, SpriteEffects.None, 0.0f);
      Vector2 vector2_4 = this.Fonts.Small.MeasureString(this.antiText) * scale2;
      this.tr.DrawShadowedText(this.spriteBatch, font, this.antiText, position + vector2_1 * (float) scale1 - vector2_4 * Vector2.UnitY / 2f + this.Fonts.TopSpacing * this.Fonts.SmallFactor * Vector2.UnitY, color, scale2, Color.Black, 1f, 1f);
      position.Y += (float) (51 * scale1);
    }
    this.spriteBatch.Draw(this.keyIcon, position, new Rectangle?(), color, 0.0f, Vector2.Zero, (float) scale1, SpriteEffects.None, 0.0f);
    Vector2 vector2_5 = this.Fonts.Small.MeasureString(this.keysText) * scale2;
    this.tr.DrawShadowedText(this.spriteBatch, font, this.keysText, position + vector2_1 * (float) scale1 - vector2_5 * Vector2.UnitY / 2f + this.Fonts.TopSpacing * this.Fonts.SmallFactor * Vector2.UnitY, color, scale2, Color.Black, 1f, 1f);
  }

  private void DrawDebugging()
  {
  }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency(Optional = true)]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IFontManager Fonts { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }
}
