// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.SaveSlotSelectionLevel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using EasyStorage;
using FezEngine.Components;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;

#nullable disable
namespace FezGame.Structure;

internal class SaveSlotSelectionLevel : MenuLevel
{
  private readonly SaveSlotInfo[] Slots = new SaveSlotInfo[3];
  public Func<bool> RecoverMainMenu;
  public Action RunStart;
  private Texture2D GottaGomezFast;
  private IGameStateManager GameState;

  public override void Initialize()
  {
    base.Initialize();
    this.AButtonString = "ChooseWithGlyph";
    this.BButtonString = "ExitWithGlyph";
    this.GameState = ServiceHelper.Get<IGameStateManager>();
    this.Title = "SaveSlotTitle";
    for (int index1 = 0; index1 < 3; ++index1)
    {
      SaveSlotInfo[] slots = this.Slots;
      int index2 = index1;
      SaveSlotInfo saveSlotInfo1 = new SaveSlotInfo();
      saveSlotInfo1.Index = index1;
      SaveSlotInfo saveSlotInfo2 = saveSlotInfo1;
      slots[index2] = saveSlotInfo1;
      SaveSlotInfo saveSlotInfo3 = saveSlotInfo2;
      PCSaveDevice pcSaveDevice = new PCSaveDevice("FEZ");
      string fileName = "SaveSlot" + (object) index1;
      if (!pcSaveDevice.FileExists(fileName))
      {
        saveSlotInfo3.Empty = true;
      }
      else
      {
        SaveData saveData = (SaveData) null;
        if (!pcSaveDevice.Load(fileName, (LoadAction) (stream => saveData = SaveFileOperations.Read(new CrcReader(stream)))) || saveData == null)
        {
          saveSlotInfo3.Empty = true;
        }
        else
        {
          saveSlotInfo3.Percentage = (float) (((double) (saveData.CubeShards + saveData.SecretCubes + saveData.PiecesOfHeart) + (double) saveData.CollectedParts / 8.0) / 32.0);
          saveSlotInfo3.PlayTime = new TimeSpan(saveData.PlayTime);
          string str = saveData.Level;
          if (str.Contains("GOMEZ_HOUSE"))
            str = "GOMEZ_HOUSE";
          if (str.Contains("VILLAGEVILLE") || str == "ELDERS")
            str = "VILLAGEVILLE_3D";
          if (str == "PYRAMID" || str == "HEX_REBUILD")
            str = "STARGATE";
          try
          {
            saveSlotInfo3.PreviewTexture = this.CMProvider.Global.Load<Texture2D>("Other Textures/map_screens/" + str);
          }
          catch
          {
            Logger.Log("Content", $"Room {str} does not have a map image!");
          }
          this.IsDynamic = true;
        }
      }
    }
    if (this.RunStart != null && Fez.SpeedRunMode)
    {
      this.IsDynamic = true;
      this.AddItem((string) null).Selectable = false;
      this.GottaGomezFast = this.CMProvider.Global.Load<Texture2D>("Other Textures/GottaGomezFast");
    }
    if (this.IsDynamic)
    {
      for (int index = 0; index < 5; ++index)
        this.AddItem((string) null).Selectable = false;
      this.SelectedIndex = this.Items.Count;
      if (this.SelectedIndex == 6)
        this.AddItem((string) null, (Action) (() => this.BeginSpeedRun())).SuffixText = (Func<string>) (() => "SPEEDRUN");
      this.OnPostDraw = this.OnPostDraw + new Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float>(this.IconPostDraw);
    }
    foreach (SaveSlotInfo slot1 in this.Slots)
    {
      SaveSlotInfo slot = slot1;
      if (slot.Empty)
        this.AddItem((string) null, (Action) (() => this.ChooseSaveSlot(slot))).SuffixText = (Func<string>) (() => StaticText.GetString("NewSlot"));
      else
        this.AddItem("SaveSlotPrefix", (Action) (() => this.ChooseSaveSlot(slot))).SuffixText = (Func<string>) (() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, " {0} ({1:P1} - {2:dd\\.hh\\:mm})", new object[3]
        {
          (object) (slot.Index + 1),
          (object) slot.Percentage,
          (object) slot.PlayTime
        }));
    }
  }

  private void ChooseSaveSlot(SaveSlotInfo slot)
  {
    this.GameState.SaveSlot = slot.Index;
    this.GameState.LoadSaveFile((Action) (() =>
    {
      this.GameState.Save();
      this.GameState.SaveImmediately();
      if (this.RecoverMainMenu == null || !this.RecoverMainMenu())
        return;
      this.RecoverMainMenu = (Func<bool>) null;
    }));
  }

  private void BeginSpeedRun()
  {
    this.GameState.SaveSlot = 4;
    this.GameState.LoadSaveFile((Action) (() =>
    {
      this.GameState.Save();
      this.GameState.SaveImmediately();
      if (this.RecoverMainMenu == null || !this.RecoverMainMenu())
        return;
      this.RecoverMainMenu = (Func<bool>) null;
    }));
    this.RunStart();
    SpeedRun.Begin(this.CMProvider.Global.Load<Texture2D>("Other Textures/SpeedRun"));
  }

  private void IconPostDraw(SpriteBatch batch, SpriteFont font, GlyphTextRenderer tr, float alpha)
  {
    Texture2D texture = (Texture2D) null;
    int num1 = 4;
    int index = this.SelectedIndex - 5;
    if (Fez.SpeedRunMode)
    {
      num1 += 2;
      index -= 2;
    }
    if (this.SelectedIndex > num1 && !this.Slots[index].Empty)
      texture = this.Slots[index].PreviewTexture;
    else if (this.SelectedIndex == num1)
      texture = this.GottaGomezFast;
    if (texture == null)
      return;
    float viewScale = batch.GraphicsDevice.GetViewScale();
    int num2 = (int) (256.0 * (double) viewScale);
    batch.Draw(texture, new Rectangle(batch.GraphicsDevice.Viewport.Width / 2, batch.GraphicsDevice.Viewport.Height / 2 - (int) (96.0 * (double) viewScale), num2, num2), new Rectangle?(), Color.White, 0.0f, new Vector2((float) (texture.Width / 2), (float) (texture.Height / 2)), SpriteEffects.None, 0.0f);
  }
}
