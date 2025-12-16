// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.MenuLevel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Services;
using FezGame.Components;
using FezGame.Tools;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Structure;

internal class MenuLevel
{
  private string titleString;
  public readonly List<MenuItem> Items = new List<MenuItem>();
  public MenuLevel Parent;
  public bool IsDynamic;
  public bool Oversized;
  public Action OnScrollUp;
  public Action OnScrollDown;
  public Action OnClose;
  public Action OnReset;
  private bool initialized;
  public Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float> OnPostDraw;
  private string xButtonString;
  private string aButtonString;
  private string bButtonString;
  public Action XButtonAction;
  public Action AButtonAction;
  public bool AButtonStarts;

  public string Title
  {
    get => this.titleString != null ? StaticText.GetString(this.titleString) : (string) null;
    set => this.titleString = value;
  }

  public MenuItem SelectedItem
  {
    get
    {
      return this.SelectedIndex != -1 && this.Items.Count > this.SelectedIndex ? this.Items[this.SelectedIndex] : (MenuItem) null;
    }
  }

  public int SelectedIndex { get; set; }

  public bool ForceCancel { get; set; }

  public bool TrapInput { get; set; }

  public virtual void Initialize() => this.initialized = true;

  public virtual void Dispose()
  {
  }

  public bool MoveDown()
  {
    MenuItem selectedItem = this.SelectedItem;
    if (this.SelectedItem != null)
      this.SelectedItem.Hovered = false;
    if (this.Items.Count == 0 || this.Items.All<MenuItem>((Func<MenuItem, bool>) (x => !x.Selectable || this.SelectedItem.Hidden)))
    {
      this.SelectedIndex = -1;
    }
    else
    {
      do
      {
        ++this.SelectedIndex;
        if (this.SelectedIndex == this.Items.Count)
        {
          if (this.OnScrollDown != null)
          {
            this.OnScrollDown();
            if (this.SelectedIndex == this.Items.Count)
            {
              do
              {
                --this.SelectedIndex;
              }
              while ((!this.SelectedItem.Selectable || this.SelectedItem.Hidden) && !this.Items.All<MenuItem>((Func<MenuItem, bool>) (x => !x.Selectable || x.Hidden)));
              if (this.SelectedItem == null)
              {
                this.SelectedIndex = 0;
                break;
              }
              break;
            }
            if (this.SelectedItem == null)
              this.SelectedIndex = 0;
          }
          else
            this.SelectedIndex = 0;
        }
      }
      while ((!this.SelectedItem.Selectable || this.SelectedItem.Hidden) && !this.Items.All<MenuItem>((Func<MenuItem, bool>) (x => !x.Selectable || x.Hidden)));
      if (selectedItem != this.SelectedItem)
        this.SelectedItem.SinceHovered = TimeSpan.Zero;
      this.SelectedItem.Hovered = true;
    }
    return selectedItem != this.SelectedItem;
  }

  public bool MoveUp()
  {
    MenuItem selectedItem = this.SelectedItem;
    if (this.SelectedItem != null)
      this.SelectedItem.Hovered = false;
    if (this.Items.Count == 0 || this.Items.All<MenuItem>((Func<MenuItem, bool>) (x => !x.Selectable || this.SelectedItem.Hidden)))
    {
      this.SelectedIndex = -1;
    }
    else
    {
      do
      {
        --this.SelectedIndex;
        if (this.SelectedIndex == -1)
        {
          if (this.OnScrollUp != null)
          {
            this.OnScrollUp();
            if (this.SelectedIndex == -1)
              ++this.SelectedIndex;
          }
          else
            this.SelectedIndex = this.Items.Count - 1;
        }
      }
      while ((!this.SelectedItem.Selectable || this.SelectedItem.Hidden) && !this.Items.All<MenuItem>((Func<MenuItem, bool>) (x => !x.Selectable || x.Hidden)));
      if (selectedItem != this.SelectedItem)
        this.SelectedItem.SinceHovered = TimeSpan.Zero;
      this.SelectedItem.Hovered = true;
    }
    return selectedItem != this.SelectedItem;
  }

  public virtual void Update(TimeSpan elapsed)
  {
    foreach (MenuItem menuItem in this.Items)
    {
      if (menuItem.Hovered)
        menuItem.SinceHovered += elapsed;
      else
        menuItem.SinceHovered -= elapsed;
      menuItem.ClampTimer();
    }
  }

  public virtual void PostDraw(
    SpriteBatch batch,
    SpriteFont font,
    GlyphTextRenderer tr,
    float alpha)
  {
    if (this.OnPostDraw == null)
      return;
    this.OnPostDraw(batch, font, tr, alpha);
  }

  public void Select()
  {
    if (this.SelectedItem == null || !this.SelectedItem.Selectable)
      return;
    this.SelectedItem.OnSelected();
  }

  public MenuItem AddItem(string text) => this.AddItem(text, -1);

  public MenuItem AddItem(string text, Action onSelect) => this.AddItem(text, onSelect, false, -1);

  public MenuItem AddItem(string text, Action onSelect, bool defaultItem)
  {
    return (MenuItem) this.AddItem<float>(text, onSelect, defaultItem, new Func<float>(Util.NullFunc<float>), new Action<float, int>(Util.NullAction<float, int>), -1);
  }

  public MenuItem<T> AddItem<T>(
    string text,
    Action onSelect,
    bool defaultItem,
    Func<T> sliderValueGetter,
    Action<T, int> sliderValueSetter)
  {
    return this.AddItem<T>(text, onSelect, defaultItem, sliderValueGetter, sliderValueSetter, -1);
  }

  public MenuItem AddItem(string text, int at)
  {
    return this.AddItem(text, MenuBase.SliderAction, false, at);
  }

  public MenuItem AddItem(string text, Action onSelect, int at)
  {
    return this.AddItem(text, onSelect, false, at);
  }

  public MenuItem AddItem(string text, Action onSelect, bool defaultItem, int at)
  {
    return (MenuItem) this.AddItem<float>(text, onSelect, defaultItem, new Func<float>(Util.NullFunc<float>), new Action<float, int>(Util.NullAction<float, int>), at);
  }

  public MenuItem<T> AddItem<T>(
    string text,
    Action onSelect,
    bool defaultItem,
    Func<T> sliderValueGetter,
    Action<T, int> sliderValueSetter,
    int at)
  {
    MenuItem<T> menuItem1 = new MenuItem<T>()
    {
      Parent = this,
      Text = text,
      Selected = onSelect,
      IsSlider = sliderValueGetter != new Func<T>(Util.NullFunc<T>),
      SliderValueGetter = sliderValueGetter,
      SliderValueSetter = sliderValueSetter
    };
    if (!this.initialized && this.Items.Count == 0 | defaultItem)
    {
      foreach (MenuItem menuItem2 in this.Items)
        menuItem2.Hovered = false;
      menuItem1.Hovered = true;
      this.SelectedIndex = this.Items.Count;
    }
    if (onSelect == new Action(Util.NullAction))
      menuItem1.Hovered = menuItem1.Selectable = false;
    if (at == -1)
      this.Items.Add((MenuItem) menuItem1);
    else
      this.Items.Insert(at, (MenuItem) menuItem1);
    return menuItem1;
  }

  public virtual void Reset()
  {
    if (this.OnReset == null)
      return;
    this.OnReset();
  }

  public string XButtonString
  {
    get => this.xButtonString;
    set => this.xButtonString = value;
  }

  public virtual string AButtonString
  {
    get => this.aButtonString != null ? StaticText.GetString(this.aButtonString) : (string) null;
    set => this.aButtonString = value;
  }

  public string BButtonString
  {
    get => this.bButtonString != null ? StaticText.GetString(this.bButtonString) : (string) null;
    set => this.bButtonString = value;
  }

  public IContentManagerProvider CMProvider { protected get; set; }
}
