// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.MenuItem`1
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Tools;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Globalization;

#nullable disable
namespace FezGame.Structure;

internal class MenuItem<T> : MenuItem
{
  private static readonly TimeSpan HoverGrowDuration = TimeSpan.FromSeconds(0.1);
  private string text;
  public Func<T> SliderValueGetter;
  public Action<T, int> SliderValueSetter;

  public string Text
  {
    get
    {
      string str = (this.text == null ? "" : StaticText.GetString(this.text)) + (this.SuffixText == null ? "" : this.SuffixText());
      return !this.UpperCase ? str : str.ToUpper(CultureInfo.InvariantCulture);
    }
    set => this.text = value;
  }

  public Func<string> SuffixText { get; set; }

  public MenuLevel Parent { get; set; }

  public bool Hovered { get; set; }

  public bool UpperCase { get; set; }

  public Action Selected { get; set; }

  public Vector2 Size { get; set; }

  public bool Selectable { get; set; }

  public bool IsSlider { get; set; }

  public bool LocalizeSliderValue { get; set; }

  public string LocalizationTagFormat { get; set; }

  public bool Centered { get; set; }

  public bool Disabled { get; set; }

  public bool IsGamerCard { get; set; }

  public TimeSpan SinceHovered { get; set; }

  public bool Hidden { get; set; }

  public Rectangle HoverArea { get; set; }

  public bool InError { get; set; }

  public MenuItem()
  {
    this.Selectable = true;
    this.Centered = true;
  }

  public void Slide(int direction)
  {
    if (!this.IsSlider)
      return;
    this.SliderValueSetter(this.SliderValueGetter(), Math.Sign(direction));
  }

  public void ClampTimer()
  {
    if (this.SinceHovered.Ticks < 0L)
      this.SinceHovered = TimeSpan.Zero;
    if (!(this.SinceHovered > MenuItem<T>.HoverGrowDuration))
      return;
    this.SinceHovered = MenuItem<T>.HoverGrowDuration;
  }

  public float ActivityRatio
  {
    get
    {
      TimeSpan timeSpan = this.SinceHovered;
      double ticks1 = (double) timeSpan.Ticks;
      timeSpan = MenuItem<T>.HoverGrowDuration;
      double ticks2 = (double) timeSpan.Ticks;
      return FezMath.Saturate((float) (ticks1 / ticks2));
    }
  }

  public void OnSelected() => this.Selected();

  public override string ToString()
  {
    string str = this.Text;
    if (this.IsSlider)
      str = string.Format(this.Text, this.LocalizeSliderValue ? (object) StaticText.GetString(string.Format(this.LocalizationTagFormat, (object) this.SliderValueGetter())) : (object) this.SliderValueGetter());
    if (this.UpperCase)
      str = str.ToUpperInvariant();
    return str;
  }
}
