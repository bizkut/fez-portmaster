// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.MenuItem
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Structure;

internal interface MenuItem
{
  string Text { get; set; }

  Func<string> SuffixText { get; set; }

  MenuLevel Parent { get; set; }

  bool Hovered { get; set; }

  Action Selected { get; set; }

  Vector2 Size { get; set; }

  bool Selectable { get; set; }

  bool IsSlider { get; set; }

  bool UpperCase { get; set; }

  bool Centered { get; set; }

  bool Disabled { get; set; }

  bool InError { get; set; }

  bool IsGamerCard { get; set; }

  bool Hidden { get; set; }

  TimeSpan SinceHovered { get; set; }

  Rectangle HoverArea { get; set; }

  bool LocalizeSliderValue { get; set; }

  string LocalizationTagFormat { get; set; }

  float ActivityRatio { get; }

  void OnSelected();

  void ClampTimer();

  void Slide(int direction);
}
