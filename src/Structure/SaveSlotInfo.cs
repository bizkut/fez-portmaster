// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.SaveSlotInfo
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezGame.Structure;

internal class SaveSlotInfo
{
  public TimeSpan PlayTime;
  public float Percentage;
  public int Index;
  public bool Empty;
  public Texture2D PreviewTexture;
  public SaveData SaveData;
}
