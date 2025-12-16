// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.LeaderboardsMenuLevel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Services;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Globalization;

#nullable disable
namespace FezGame.Structure;

internal class LeaderboardsMenuLevel : MenuLevel
{
  private const int EntriesPerPage = 10;
  private LeaderboardsMenuLevel.LeaderboardView view;
  private LeaderboardsMenuLevel.CachedLeaderboard leaderboard;
  private bool moveToLast;
  private bool moveToTop;
  private Rectangle? leftArrowRect;
  private Rectangle? rightArrowRect;
  private readonly MenuBase menuBase;
  private bool viewChanged;

  public LeaderboardsMenuLevel(MenuBase menuBase) => this.menuBase = menuBase;

  public override void Initialize()
  {
    base.Initialize();
    this.IsDynamic = true;
    this.OnClose = (Action) (() =>
    {
      if (this.leaderboard == null)
        return;
      this.GameState.LiveConnectionChanged -= new Action(this.ChangeView);
      this.leaderboard = (LeaderboardsMenuLevel.CachedLeaderboard) null;
    });
    this.XButtonAction = (Action) (() =>
    {
      ++this.view;
      if (this.view > LeaderboardsMenuLevel.LeaderboardView.Overall)
        this.view = LeaderboardsMenuLevel.LeaderboardView.Friends;
      this.ChangeView();
    });
  }

  public override void Reset()
  {
    base.Reset();
    this.XButtonString = "{X} " + string.Format(StaticText.GetString("CurrentLeaderboardView"), (object) StaticText.GetString(this.view.ToString() + "LeaderboardView"));
  }

  public override void Update(TimeSpan elapsed)
  {
    base.Update(elapsed);
    if (this.leaderboard == null)
      this.InitLeaderboards();
    if (!this.leaderboard.InError)
    {
      if (this.InputManager.RotateRight == FezButtonState.Pressed)
        this.TryPageUp();
      if (this.InputManager.RotateLeft == FezButtonState.Pressed)
        this.TryPageDown();
    }
    else if (this.GameState.HasActivePlayer && this.GameState.ActiveGamer != null && !this.leaderboard.Reading && !this.leaderboard.ChangingPage)
    {
      this.leaderboard.ActiveGamer = this.GameState.ActiveGamer;
      this.ChangeView();
    }
    Point position = this.MouseState.Position;
    if (this.rightArrowRect.HasValue && this.rightArrowRect.Value.Contains(position))
    {
      this.menuBase.CursorSelectable = true;
      if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
        this.TryPageUp();
    }
    if (this.leftArrowRect.HasValue && this.leftArrowRect.Value.Contains(position))
    {
      this.menuBase.CursorSelectable = true;
      if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
        this.TryPageDown();
    }
    if (!this.leaderboard.InError && !this.leaderboard.Reading)
      return;
    this.XButtonString = (string) null;
  }

  private void TryPageUp()
  {
    if (this.leaderboard.Reading || !this.leaderboard.CanPageDown)
      return;
    this.Items.Clear();
    this.AddItem("LoadingLeaderboard", new Action(Util.NullAction));
    this.leaderboard.PageDown(new Action(this.Refresh));
  }

  private void TryPageDown()
  {
    if (this.leaderboard.Reading || !this.leaderboard.CanPageUp)
      return;
    this.Items.Clear();
    this.AddItem("LoadingLeaderboard", new Action(Util.NullAction));
    this.leaderboard.PageUp(new Action(this.Refresh));
  }

  private void CheckLive()
  {
    if (this.leaderboard != null)
      this.leaderboard.ActiveGamer = this.GameState.ActiveGamer;
    this.ChangeView();
  }

  private void InitLeaderboards()
  {
    this.leaderboard = new LeaderboardsMenuLevel.CachedLeaderboard(this.GameState.ActiveGamer, 10);
    this.OnScrollDown = (Action) (() =>
    {
      if (this.leaderboard.InError || !this.leaderboard.CanPageDown || this.leaderboard.Reading)
        return;
      this.moveToTop = true;
      this.TryPageUp();
    });
    this.OnScrollUp = (Action) (() =>
    {
      if (this.leaderboard.InError || !this.leaderboard.CanPageUp || this.leaderboard.Reading)
        return;
      this.moveToLast = true;
      this.TryPageDown();
    });
    this.ChangeView();
    this.GameState.LiveConnectionChanged += new Action(this.CheckLive);
  }

  public override void Dispose()
  {
    if (this.leaderboard != null)
      this.GameState.LiveConnectionChanged -= new Action(this.CheckLive);
    this.OnScrollDown = (Action) null;
    this.OnScrollUp = (Action) null;
    this.OnClose = (Action) null;
  }

  private void ChangeView()
  {
    lock (this)
    {
      this.Items.Clear();
      this.AddItem("LoadingLeaderboard", new Action(Util.NullAction));
    }
    this.viewChanged = true;
    this.leaderboard.ChangeView(this.view, new Action(this.Refresh));
  }

  public override void PostDraw(
    SpriteBatch batch,
    SpriteFont font,
    GlyphTextRenderer tr,
    float alpha)
  {
    if (this.leaderboard == null)
      this.InitLeaderboards();
    float viewScale = batch.GraphicsDevice.GetViewScale();
    Viewport viewport;
    if (!this.leaderboard.InError)
    {
      GlyphTextRenderer glyphTextRenderer = tr;
      SpriteBatch batch1 = batch;
      SpriteFont font1 = font;
      string text = string.Format(StaticText.GetString("LeaderboardEntriesCount").ToUpper(CultureInfo.InvariantCulture), (object) this.leaderboard.TotalEntries);
      viewport = batch.GraphicsDevice.Viewport;
      Vector2 position = new Vector2(125f, (float) (viewport.Height / 2 + 260)) * viewScale;
      Color color = new Color(1f, 1f, 1f, alpha);
      double scale = (Culture.IsCJK ? 0.20000000298023224 : 1.5) * (double) viewScale;
      glyphTextRenderer.DrawString(batch1, font1, text, position, color, (float) scale);
    }
    float num1 = this.leaderboard.InError || this.leaderboard.Reading ? 0.0f : (this.leaderboard.CanPageUp ? 1f : 0.1f);
    float num2 = this.leaderboard.InError || this.leaderboard.Reading ? 0.0f : (this.leaderboard.CanPageDown ? 1f : 0.1f);
    float num3 = Culture.IsCJK ? -15f : 0.0f;
    if (this.Items.Count > 1)
    {
      viewport = ServiceHelper.Game.GraphicsDevice.Viewport;
      int num4 = viewport.Width / 2 - (int) viewScale * 20;
      viewport = ServiceHelper.Game.GraphicsDevice.Viewport;
      int y = viewport.Height / 2;
      this.leftArrowRect = new Rectangle?(new Rectangle((int) ((double) num4 - (double) num4 * 5.0 / 7.0 + (double) num3 - (double) viewScale * 10.0), y, (int) (40.0 * (double) viewScale), (int) (25.0 * (double) viewScale)));
      this.rightArrowRect = new Rectangle?(new Rectangle((int) ((double) num4 + (double) num4 * 5.0 / 7.0 + (double) num3), y, (int) (40.0 * (double) viewScale), (int) (25.0 * (double) viewScale)));
      GlyphTextRenderer glyphTextRenderer1 = tr;
      SpriteBatch batch2 = batch;
      SpriteFont font2 = font;
      Rectangle rectangle = this.leftArrowRect.Value;
      double x1 = (double) rectangle.Left + 15.0 * (double) viewScale;
      rectangle = this.leftArrowRect.Value;
      double top1 = (double) rectangle.Top;
      Vector2 position1 = new Vector2((float) x1, (float) top1);
      Color color1 = new Color(1f, 1f, 1f, num1 * alpha);
      double scale1 = (Culture.IsCJK ? 0.20000000298023224 : 1.0) * (double) viewScale;
      glyphTextRenderer1.DrawString(batch2, font2, "{LA}", position1, color1, (float) scale1);
      GlyphTextRenderer glyphTextRenderer2 = tr;
      SpriteBatch batch3 = batch;
      SpriteFont font3 = font;
      rectangle = this.rightArrowRect.Value;
      double x2 = (double) rectangle.Left + 15.0 * (double) viewScale;
      rectangle = this.rightArrowRect.Value;
      double top2 = (double) rectangle.Top;
      Vector2 position2 = new Vector2((float) x2, (float) top2);
      Color color2 = new Color(1f, 1f, 1f, num2 * alpha);
      double scale2 = (Culture.IsCJK ? 0.20000000298023224 : 1.0) * (double) viewScale;
      glyphTextRenderer2.DrawString(batch3, font3, "{RA}", position2, color2, (float) scale2);
    }
    else
      this.leftArrowRect = this.rightArrowRect = new Rectangle?();
    if (!this.leaderboard.CanPageUp)
      this.leftArrowRect = new Rectangle?();
    if (this.leaderboard.CanPageDown)
      return;
    this.rightArrowRect = new Rectangle?();
  }

  private void Refresh()
  {
    lock (this)
    {
      int selectedIndex = this.SelectedIndex;
      this.Items.Clear();
      bool flag1 = false;
      if (this.leaderboard.InError)
      {
        this.AddItem("LeaderboardsNeedLIVE", new Action(Util.NullAction));
      }
      else
      {
        this.SelectedIndex = 0;
        bool flag2 = false;
        int num = 0;
        foreach (LeaderboardsMenuLevel.LeaderboardEntry entry in this.leaderboard.Entries)
        {
          LeaderboardsMenuLevel.LeaderboardEntry e = entry;
          MenuItem menuItem = this.AddItem((string) null, (Action) (() => { }), !flag2);
          string personaName = e.PersonaName;
          menuItem.SuffixText = (Func<string>) (() => $"{(object) e.m_nGlobalRank}. {personaName} : {(object) Math.Round((double) e.m_nScore / 32.0 * 100.0, 1)} %");
          menuItem.Hovered = !flag2;
          flag2 = true;
          ++num;
        }
      }
      if (!flag1)
        this.SelectedIndex = Math.Min(selectedIndex, this.Items.Count - 1);
      if (this.moveToLast)
      {
        this.SelectedIndex = this.Items.Count - 1;
        this.moveToLast = false;
      }
      if (this.moveToTop)
      {
        this.SelectedIndex = 0;
        this.moveToTop = false;
      }
      if (this.Items.Count > 0 && this.SelectedIndex == -1)
        this.SelectedIndex = 0;
      if (!this.leaderboard.InError)
      {
        if (this.Items.Count == 0)
        {
          this.AddItem(this.view == LeaderboardsMenuLevel.LeaderboardView.MyScore ? "NotRankedInLeaderboard" : "NoEntriesLeaderboard", new Action(Util.NullAction));
          this.SelectedIndex = -1;
        }
        else
        {
          while (this.Items.Count < 10)
            this.AddItem((string) null, new Action(Util.NullAction));
        }
      }
      this.XButtonString = "{X} " + string.Format(StaticText.GetString("CurrentLeaderboardView"), (object) StaticText.GetString(this.view.ToString() + "LeaderboardView"));
      this.viewChanged = false;
    }
  }

  public IInputManager InputManager { private get; set; }

  public IMouseStateManager MouseState { private get; set; }

  public IGameStateManager GameState { private get; set; }

  public SpriteFont Font { private get; set; }

  private enum LeaderboardView
  {
    Friends,
    MyScore,
    Overall,
  }

  private struct LeaderboardEntry
  {
    public string PersonaName;
    public int m_nGlobalRank;
    public int m_nScore;
  }

  private class CachedLeaderboard
  {
    private int pageStart;
    private readonly int EntriesPerPage;

    public CachedLeaderboard(MockUser gamer, int entriesPerPage)
    {
      this.ActiveGamer = gamer;
      this.EntriesPerPage = entriesPerPage;
    }

    public bool InError => false;

    public bool Reading => false;

    public bool CanPageDown => this.pageStart < 90;

    public bool CanPageUp => this.pageStart > 0;

    public bool ChangingPage => false;

    public MockUser ActiveGamer { get; set; }

    public int TotalEntries => 95;

    public IEnumerable<LeaderboardsMenuLevel.LeaderboardEntry> Entries
    {
      get
      {
        for (int i = 0; i < (this.CanPageDown ? this.EntriesPerPage : 5); ++i)
          yield return new LeaderboardsMenuLevel.LeaderboardEntry()
          {
            PersonaName = "Player",
            m_nGlobalRank = this.pageStart + i + 1,
            m_nScore = (int) Math.Round((double) (this.pageStart + i) / (double) this.TotalEntries * 67.0)
          };
      }
    }

    public void ChangeView(LeaderboardsMenuLevel.LeaderboardView view, Action onFinish)
    {
      onFinish();
    }

    public void PageDown(Action onFinish)
    {
      this.pageStart += 10;
      onFinish();
    }

    public void PageUp(Action onFinish)
    {
      this.pageStart -= 10;
      onFinish();
    }
  }
}
