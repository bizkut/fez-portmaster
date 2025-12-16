// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.SpaceDivider
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Tools;

internal class SpaceDivider
{
  public static List<SpaceDivider.DividedCell> Split(int count)
  {
    List<SpaceDivider.DividedCell> list = new List<SpaceDivider.DividedCell>()
    {
      new SpaceDivider.DividedCell(0, 0, 16 /*0x10*/, 16 /*0x10*/)
    };
    for (int index = 0; index < count - 1; ++index)
    {
      int num1 = 4;
      int num2 = 0;
      SpaceDivider.DividedCell dividedCell;
      do
      {
        dividedCell = RandomHelper.InList<SpaceDivider.DividedCell>(list);
        if (num2++ <= 100)
          num1 = (double) dividedCell.Bottom + (double) dividedCell.Height / 2.0 < 6.0 ? 2 : 4;
        else
          break;
      }
      while (dividedCell.Width <= num1 && dividedCell.Height <= num1);
      if (num2 <= 100)
      {
        list.Remove(dividedCell);
        if (RandomHelper.Probability(0.5) && dividedCell.Height != num1 || dividedCell.Width == num1)
        {
          int height = (int) Math.Round((double) dividedCell.Height / 2.0);
          list.Add(new SpaceDivider.DividedCell(dividedCell.Left, dividedCell.Bottom + height, dividedCell.Width, dividedCell.Height - height));
          list.Add(new SpaceDivider.DividedCell(dividedCell.Left, dividedCell.Bottom, dividedCell.Width, height));
        }
        else
        {
          int width = (int) Math.Round((double) dividedCell.Width / 2.0);
          list.Add(new SpaceDivider.DividedCell(dividedCell.Left, dividedCell.Bottom, width, dividedCell.Height));
          list.Add(new SpaceDivider.DividedCell(dividedCell.Left + width, dividedCell.Bottom, dividedCell.Width - width, dividedCell.Height));
        }
      }
      else
        break;
    }
    return list;
  }

  public struct DividedCell(int left, int bottom, int width, int height)
  {
    public int Left = left;
    public int Bottom = bottom;
    public int Width = width;
    public int Height = height;

    public int Top => this.Bottom + this.Height;

    public int Right => this.Left + this.Width;

    public Vector2 Center
    {
      get
      {
        return new Vector2((float) this.Left + (float) this.Width / 2f, (float) this.Bottom + (float) this.Height / 2f);
      }
    }
  }
}
