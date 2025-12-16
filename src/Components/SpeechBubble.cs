// Decompiled with JetBrains decompiler
// Type: FezGame.Components.SpeechBubble
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Components;
using FezEngine.Effects;
using FezEngine.Effects.Structures;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

#nullable disable
namespace FezGame.Components;

public class SpeechBubble : DrawableGameComponent, ISpeechBubbleManager
{
  private readonly Color TextColor = Color.White;
  private const int TextBorder = 4;
  private Vector2 scalableMiddleSize;
  private float sinceShown;
  private readonly Mesh textMesh;
  private readonly Mesh canvasMesh;
  private Group scalableMiddle;
  private Group scalableTop;
  private Group scalableBottom;
  private Group neGroup;
  private Group nwGroup;
  private Group seGroup;
  private Group swGroup;
  private Group tailGroup;
  private Group bGroup;
  private Group textGroup;
  private GlyphTextRenderer GTR;
  private SpriteBatch spriteBatch;
  private SpriteFont zuishFont;
  private RenderTarget2D text;
  private string originalString;
  private string textString;
  private float distanceFromCenterAtTextChange;
  private bool changingText;
  private bool show;
  private Vector3 origin;
  private Vector3 lastUsedOrigin;
  private RenderTarget2D bTexture;
  private Vector3 oldCamPos;

  public void ChangeText(string toText)
  {
    this.originalString = toText.ToUpper(CultureInfo.InvariantCulture);
    if (this.changingText)
      return;
    this.changingText = true;
    Waiters.Wait((Func<bool>) (() => (double) this.sinceShown == 0.0), (Action) (() =>
    {
      if (!this.changingText)
        return;
      this.changingText = false;
      this.UpdateBTexture();
      this.OnTextChanged(false);
      this.show = true;
    })).AutoPause = true;
  }

  public void Hide()
  {
    this.show = false;
    this.changingText = false;
  }

  public bool Hidden => !this.show && !this.changingText;

  public Vector3 Origin
  {
    set
    {
      this.origin = value;
      if (FezMath.AlmostEqual(this.lastUsedOrigin, this.origin, 1f / 16f) || (double) this.sinceShown < 1.0 || this.changingText)
        return;
      this.OnTextChanged(true);
    }
    private get => this.origin;
  }

  public SpeechFont Font { get; set; }

  public SpeechBubble(Game game)
    : base(game)
  {
    this.textMesh = new Mesh()
    {
      AlwaysOnTop = true,
      SamplerState = SamplerState.PointClamp,
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    this.canvasMesh = new Mesh()
    {
      AlwaysOnTop = true,
      SamplerState = SamplerState.PointClamp,
      Blending = new BlendingMode?(BlendingMode.Alphablending)
    };
    this.DrawOrder = 150;
    this.Font = SpeechFont.Pixel;
    this.show = false;
  }

  public override void Initialize()
  {
    this.scalableTop = this.canvasMesh.AddFace(new Vector3(1f, 0.5f, 0.0f), Vector3.Zero, FaceOrientation.Front, false);
    this.scalableBottom = this.canvasMesh.CloneGroup(this.scalableTop);
    this.scalableMiddle = this.canvasMesh.AddFace(new Vector3(1f, 1f, 0.0f), Vector3.Zero, FaceOrientation.Front, false);
    this.neGroup = this.canvasMesh.AddFace(new Vector3(0.5f, 0.5f, 0.0f), Vector3.Zero, FaceOrientation.Front, false);
    this.nwGroup = this.canvasMesh.CloneGroup(this.neGroup);
    this.seGroup = this.canvasMesh.CloneGroup(this.neGroup);
    this.swGroup = this.canvasMesh.CloneGroup(this.neGroup);
    this.tailGroup = this.canvasMesh.AddFace(new Vector3(5f / 16f, 0.25f, 0.0f), Vector3.Zero, FaceOrientation.Front, false, true);
    this.textGroup = this.textMesh.AddFace(new Vector3(1f, 1f, 0.0f), Vector3.Zero, FaceOrientation.Front, false);
    this.bGroup = this.canvasMesh.AddFace(new Vector3(1f, 1f, 0.0f), Vector3.Zero, FaceOrientation.Front, false);
    this.swGroup.Position = Vector3.Zero;
    this.scalableBottom.Position = new Vector3(0.5f, 0.0f, 0.0f);
    this.scalableMiddle.Position = new Vector3(0.0f, 0.5f, 0.0f);
    this.tailGroup.Position = new Vector3(0.5f, -0.25f, 0.0f);
    this.GTR = new GlyphTextRenderer(this.Game);
    this.LevelManager.LevelChanged += new Action(this.Hide);
    base.Initialize();
  }

  protected override void LoadContent()
  {
    this.textMesh.Effect = this.canvasMesh.Effect = (BaseEffect) new DefaultEffect.Textured();
    this.tailGroup.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/speech_bubble/SpeechBubbleTail");
    this.neGroup.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/speech_bubble/SpeechBubbleNE");
    this.nwGroup.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/speech_bubble/SpeechBubbleNW");
    this.seGroup.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/speech_bubble/SpeechBubbleSE");
    this.swGroup.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/speech_bubble/SpeechBubbleSW");
    this.scalableBottom.Texture = (Texture) this.CMProvider.Global.Load<Texture2D>("Other Textures/FullBlack");
    this.scalableMiddle.Texture = this.scalableTop.Texture = this.scalableBottom.Texture;
    this.zuishFont = this.CMProvider.Global.Load<SpriteFont>("Fonts/Zuish");
    ++this.zuishFont.LineSpacing;
    this.spriteBatch = new SpriteBatch(this.GraphicsDevice);
    GamepadState.OnLayoutChanged += new EventHandler(this.OnLayoutChanged);
  }

  protected override void UnloadContent()
  {
    GamepadState.OnLayoutChanged -= new EventHandler(this.OnLayoutChanged);
    if (this.text == null)
      return;
    this.text.Unhook();
    this.text.Dispose();
  }

  private void OnTextChanged(bool update)
  {
    float num1 = 2f;
    string textString = this.textString;
    this.textString = this.originalString;
    SpriteFont spriteFont = this.Font == SpeechFont.Pixel ? this.FontManager.Big : this.zuishFont;
    if (this.Font == SpeechFont.Zuish)
      this.textString = this.textString.Replace(" ", "  ");
    float scale = !Culture.IsCJK || this.Font != SpeechFont.Pixel ? 1f : this.FontManager.SmallFactor;
    bool flag1 = this.GraphicsDevice.DisplayMode.Width < 1280 /*0x0500*/ && this.Font == SpeechFont.Pixel;
    float num2 = 0.0f;
    if (this.Font != SpeechFont.Zuish)
    {
      float num3 = update ? 0.9f : 0.85f;
      float num4 = (float) this.GraphicsDevice.Viewport.Width / (1280f * this.GraphicsDevice.GetViewScale());
      num2 = (this.Origin - this.CameraManager.InterpolatedCenter).Dot(this.CameraManager.Viewpoint.RightVector());
      float val1 = flag1 ? Math.Max((float) (-(double) num2 * 16.0 * (double) this.CameraManager.PixelsPerTrixel + 640.0 * (double) num3), 50f) * 0.6666667f : Math.Max((float) (-(double) num2 * 16.0 * (double) this.CameraManager.PixelsPerTrixel + 1280.0 * (double) num4 / 2.0 * (double) num3), 50f) / (this.CameraManager.PixelsPerTrixel / 2f);
      if (this.GameState.InMap)
        val1 = 500f;
      float num5 = Math.Max(val1, 70f);
      List<GlyphTextRenderer.FilledInGlyph> glyphLocations;
      string text = this.GTR.FillInGlyphs(this.textString, out glyphLocations);
      if (Culture.IsCJK)
        scale /= 2f;
      SpriteFont font = spriteFont;
      double maxTextSize = (double) num5 / (double) scale;
      StringBuilder stringBuilder = new StringBuilder(WordWrap.Split(text, font, (float) maxTextSize));
      if (Culture.IsCJK)
        scale *= 2f;
      bool flag2 = true;
      int index1 = 0;
      for (int index2 = 0; index2 < stringBuilder.Length; ++index2)
      {
        if (flag2 && stringBuilder[index2] == '^')
        {
          for (int index3 = index2; index3 < index2 + glyphLocations[index1].Length; ++index3)
          {
            if (stringBuilder[index3] == '\r' || stringBuilder[index3] == '\n')
            {
              stringBuilder.Remove(index3, 1);
              --index3;
            }
          }
          stringBuilder.Remove(index2, glyphLocations[index1].Length);
          stringBuilder.Insert(index2, glyphLocations[index1].OriginalGlyph);
          ++index1;
        }
        else
          flag2 = stringBuilder[index2] == ' ' || stringBuilder[index2] == '\r' || stringBuilder[index2] == '\n';
      }
      this.textString = stringBuilder.ToString();
      if (!update)
        this.distanceFromCenterAtTextChange = num2;
    }
    if (update && (textString == this.textString || (double) Math.Abs(this.distanceFromCenterAtTextChange - num2) < 1.5))
    {
      this.textString = textString;
    }
    else
    {
      if (Culture.IsCJK && this.Font == SpeechFont.Pixel)
      {
        if ((double) this.GraphicsDevice.GetViewScale() < 1.5)
        {
          spriteFont = this.FontManager.Small;
        }
        else
        {
          spriteFont = this.FontManager.Big;
          scale /= 2f;
        }
        scale *= num1;
      }
      bool multilineGlyphs;
      Vector2 vector2_1 = this.GTR.MeasureWithGlyphs(spriteFont, this.textString, scale, out multilineGlyphs);
      if (!Culture.IsCJK & multilineGlyphs)
      {
        spriteFont.LineSpacing += 8;
        int num6 = multilineGlyphs ? 1 : 0;
        vector2_1 = this.GTR.MeasureWithGlyphs(spriteFont, this.textString, scale, out multilineGlyphs);
        multilineGlyphs = num6 != 0;
      }
      float num7 = 1f;
      if (Culture.IsCJK && this.Font == SpeechFont.Pixel)
        num7 = num1;
      this.scalableMiddleSize = vector2_1 + Vector2.One * 4f * 2f * num7 + Vector2.UnitX * 4f * 2f * num7;
      if (this.Font == SpeechFont.Zuish)
        this.scalableMiddleSize += Vector2.UnitY * 2f;
      int x1 = (int) this.scalableMiddleSize.X;
      int y = (int) this.scalableMiddleSize.Y;
      if (Culture.IsCJK && this.Font == SpeechFont.Pixel)
      {
        scale *= 2f;
        x1 *= 2;
        y *= 2;
      }
      Vector2 scalableMiddleSize = this.scalableMiddleSize;
      if (this.text != null)
      {
        this.text.Unhook();
        this.text.Dispose();
      }
      this.text = new RenderTarget2D(this.GraphicsDevice, x1, y, false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, this.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
      this.GraphicsDevice.SetRenderTarget(this.text);
      this.GraphicsDevice.PrepareDraw();
      this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
      Vector2 vector2_2 = Culture.IsCJK ? new Vector2(4f * num1) : Vector2.Zero;
      if (Culture.IsCJK)
        this.spriteBatch.BeginLinear();
      else
        this.spriteBatch.BeginPoint();
      if (this.Font == SpeechFont.Pixel)
        this.GTR.DrawString(this.spriteBatch, spriteFont, this.textString, (scalableMiddleSize / 2f - vector2_1 / 2f + vector2_2).Round(), this.TextColor, scale);
      else
        this.spriteBatch.DrawString(spriteFont, this.textString, scalableMiddleSize / 2f - vector2_1 / 2f, this.TextColor, 0.0f, Vector2.Zero, this.scalableMiddleSize / scalableMiddleSize, SpriteEffects.None, 0.0f);
      this.spriteBatch.End();
      this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
      if (this.Font == SpeechFont.Zuish)
      {
        float x2 = this.scalableMiddleSize.X;
        this.scalableMiddleSize.X = this.scalableMiddleSize.Y;
        this.scalableMiddleSize.Y = x2;
      }
      if (Culture.IsCJK && this.Font == SpeechFont.Pixel)
        this.scalableMiddleSize /= num1;
      this.scalableMiddleSize /= 16f;
      this.scalableMiddleSize -= Vector2.One;
      this.textMesh.SamplerState = !Culture.IsCJK || this.Font != SpeechFont.Pixel ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
      this.textGroup.Texture = (Texture) this.text;
      this.oldCamPos = this.CameraManager.InterpolatedCenter;
      this.lastUsedOrigin = this.Origin;
      if (!(!Culture.IsCJK & multilineGlyphs))
        return;
      spriteFont.LineSpacing -= 8;
    }
  }

  private void UpdateBTexture()
  {
    SpriteFont small = this.FontManager.Small;
    Vector2 vector2 = small.MeasureString(this.GTR.FillInGlyphs(" {B} ")) * FezMath.Saturate(this.FontManager.SmallFactor);
    if (this.bTexture != null)
      this.bTexture.Dispose();
    this.bTexture = new RenderTarget2D(this.GraphicsDevice, (int) vector2.X, (int) vector2.Y, false, this.GraphicsDevice.PresentationParameters.BackBufferFormat, this.GraphicsDevice.PresentationParameters.DepthStencilFormat, 0, RenderTargetUsage.PreserveContents);
    this.GraphicsDevice.SetRenderTarget(this.bTexture);
    this.GraphicsDevice.PrepareDraw();
    this.GraphicsDevice.Clear(ClearOptions.Target, ColorEx.TransparentWhite, 1f, 0);
    this.spriteBatch.BeginPoint();
    this.GTR.DrawString(this.spriteBatch, small, " {B} ", new Vector2(0.0f, 0.0f), Color.White, FezMath.Saturate(this.FontManager.SmallFactor));
    this.spriteBatch.End();
    this.GraphicsDevice.SetRenderTarget((RenderTarget2D) null);
    this.bGroup.Texture = (Texture) this.bTexture;
    float num = Culture.IsCJK ? 25f : 24f;
    this.bGroup.Scale = new Vector3(vector2.X / num, vector2.Y / num, 1f);
    if (this.bGroup.Material != null)
      return;
    this.bGroup.Material = new Material();
  }

  private void OnLayoutChanged(object sender, EventArgs e)
  {
    if (!string.IsNullOrEmpty(this.originalString))
      this.OnTextChanged(false);
    this.UpdateBTexture();
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.show || this.changingText || FezMath.AlmostEqual(this.CameraManager.InterpolatedCenter, this.oldCamPos, 1f / 16f))
      return;
    this.OnTextChanged(true);
  }

  public override void Draw(GameTime gameTime)
  {
    bool flag1 = this.show;
    if (this.show && this.changingText)
      flag1 = false;
    if (!flag1 && (double) this.sinceShown > 1.0)
      this.sinceShown = 1f;
    this.sinceShown += (float) (gameTime.ElapsedGameTime.TotalSeconds * (flag1 ? 1.0 : -2.0) * 5.0);
    if ((double) this.sinceShown < 0.0)
      this.sinceShown = 0.0f;
    if ((double) this.sinceShown == 0.0 && !flag1)
    {
      if (this.changingText || this.Font != SpeechFont.Zuish)
        return;
      this.Font = SpeechFont.Pixel;
    }
    else
    {
      this.scalableBottom.Scale = new Vector3(this.scalableMiddleSize.X, 1f, 1f);
      this.seGroup.Position = new Vector3(this.scalableMiddleSize.X + 0.5f, 0.0f, 0.0f);
      this.scalableMiddle.Scale = new Vector3(this.scalableMiddleSize.X + 1f, this.scalableMiddleSize.Y, 1f);
      this.nwGroup.Position = new Vector3(0.0f, this.scalableMiddleSize.Y + 0.5f, 0.0f);
      this.scalableTop.Position = new Vector3(0.5f, this.nwGroup.Position.Y, 0.0f);
      this.scalableTop.Scale = this.scalableBottom.Scale;
      this.neGroup.Position = new Vector3(this.seGroup.Position.X, this.nwGroup.Position.Y, 0.0f);
      bool flag2 = this.GameState.InMap && this.Font == SpeechFont.Pixel;
      if (!((double) this.CameraManager.PixelsPerTrixel == 3.0 | flag2))
        this.seGroup.Scale = new Vector3(1f, 1f, 1f);
      float x = (float) (3.0 * (double) this.bGroup.Scale.X / 4.0);
      float y = 0.0f;
      if (Culture.IsCJK)
      {
        x /= 2f;
        y = 0.25f;
      }
      this.bGroup.Position = this.seGroup.Position + new Vector3(0.5f, -0.5f, 0.0f) - new Vector3(x, y, 0.0f);
      float viewScale = this.GraphicsDevice.GetViewScale();
      float num1 = (float) this.GraphicsDevice.Viewport.Width / (1280f * viewScale);
      float num2 = flag2 ? (float) (0.5 * (double) this.CameraManager.Radius / 26.666666030883789) / num1 / viewScale : 0.5f;
      this.canvasMesh.Scale = new Vector3(num2);
      this.tailGroup.Scale = new Vector3(this.Font == SpeechFont.Zuish ? -1f : 1f, 1f, 1f);
      this.canvasMesh.Rotation = Quaternion.Normalize(this.CameraManager.Rotation);
      this.canvasMesh.Position = this.Origin + this.canvasMesh.WorldMatrix.Left * 2f + Vector3.UnitY * 0.65f;
      this.canvasMesh.Position = (this.canvasMesh.Position * 16f * this.CameraManager.PixelsPerTrixel).Round() / 16f / this.CameraManager.PixelsPerTrixel;
      this.textMesh.Scale = this.Font != SpeechFont.Zuish ? new Vector3(this.scalableMiddleSize.X + 1f, this.scalableMiddleSize.Y + 1f, 1f) * num2 : new Vector3(this.scalableMiddleSize.Y + 1f, this.scalableMiddleSize.X + 1f, 1f) * num2;
      this.textMesh.Rotation = this.canvasMesh.Rotation;
      if (this.Font == SpeechFont.Zuish)
        this.textMesh.Rotation *= Quaternion.CreateFromYawPitchRoll(0.0f, 0.0f, -1.57079637f);
      this.textMesh.Position = this.canvasMesh.Position;
      if (this.Font == SpeechFont.Zuish)
        this.textMesh.Position += (this.scalableMiddleSize.Y + 1f) * Vector3.UnitY / 2f;
      this.canvasMesh.Material.Opacity = FezMath.Saturate(this.sinceShown);
      this.textMesh.Material.Opacity = FezMath.Saturate(this.sinceShown);
      this.bGroup.Material.Opacity = !flag1 ? Math.Min(this.bGroup.Material.Opacity, FezMath.Saturate(this.sinceShown)) : FezMath.Saturate(this.sinceShown - (float) ((0.075000002980232239 * (double) this.textString.StripPunctuation().Length + 2.0) * 2.0));
      this.canvasMesh.Draw();
      this.textMesh.Draw();
    }
  }

  public void ForceDrawOrder(int drawOrder)
  {
    this.DrawOrder = drawOrder;
    this.OnDrawOrderChanged((object) this, EventArgs.Empty);
  }

  public void RevertDrawOrder()
  {
    this.DrawOrder = 150;
    this.OnDrawOrderChanged((object) this, EventArgs.Empty);
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IFontManager FontManager { private get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }
}
