// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.SaveManagementLevel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using EasyStorage;
using FezEngine.Components;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Services;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Globalization;

#nullable disable
namespace FezGame.Structure;

internal class SaveManagementLevel : MenuLevel
{
  private readonly MenuBase Menu;
  private readonly SaveSlotInfo[] Slots = new SaveSlotInfo[3];
  private SaveSlotInfo CopySourceSlot;
  private MenuLevel CopyDestLevel;
  private IGameStateManager GameState;
  private IFontManager FontManager;

  public SaveManagementLevel(MenuBase menu) => this.Menu = menu;

  public override void Initialize()
  {
    base.Initialize();
    this.FontManager = ServiceHelper.Get<IFontManager>();
    this.GameState = ServiceHelper.Get<IGameStateManager>();
    this.ReloadSlots();
    this.Title = "SaveManagementTitle";
    SpriteFont sf = this.FontManager.Small;
    MenuLevel changeLevel = new MenuLevel()
    {
      Title = "SaveChangeSlot",
      AButtonString = "ChangeWithGlyph"
    };
    changeLevel.OnPostDraw = (Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float>) ((b, f, tr, a) =>
    {
      this.IconPostDraw(changeLevel, b, sf, tr, a);
      this.DrawWarning(b, sf, tr, a, "SaveChangeWarning");
    });
    changeLevel.Parent = (MenuLevel) this;
    changeLevel.Initialize();
    MenuLevel copySrcLevel = new MenuLevel()
    {
      Title = "SaveCopySourceTitle",
      AButtonString = "ChooseWithGlyph"
    };
    copySrcLevel.OnPostDraw = (Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float>) ((b, f, tr, a) => this.IconPostDraw(copySrcLevel, b, sf, tr, a));
    copySrcLevel.Parent = (MenuLevel) this;
    copySrcLevel.Initialize();
    this.CopyDestLevel = new MenuLevel()
    {
      Title = "SaveCopyDestTitle",
      AButtonString = "ChooseWithGlyph"
    };
    this.CopyDestLevel.OnPostDraw = (Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float>) ((b, f, tr, a) =>
    {
      this.IconPostDraw(this.CopyDestLevel, b, sf, tr, a);
      this.DrawWarning(b, sf, tr, a, "SaveCopyWarning");
    });
    this.CopyDestLevel.Parent = copySrcLevel;
    this.CopyDestLevel.Initialize();
    MenuLevel clearLevel = new MenuLevel()
    {
      Title = "SaveClearTitle",
      AButtonString = "ChooseWithGlyph"
    };
    clearLevel.OnPostDraw = (Action<SpriteBatch, SpriteFont, GlyphTextRenderer, float>) ((b, f, tr, a) =>
    {
      this.IconPostDraw(clearLevel, b, sf, tr, a);
      this.DrawWarning(b, sf, tr, a, "SaveClearWarning");
    });
    clearLevel.Parent = (MenuLevel) this;
    clearLevel.Initialize();
    this.AddItem("SaveChangeSlot", (Action) (() =>
    {
      this.RefreshSlotsFor(changeLevel, SaveManagementLevel.SMOperation.Change, (Func<SaveSlotInfo, bool>) (s => s.Index != this.GameState.SaveSlot));
      this.Menu.ChangeMenuLevel(changeLevel);
    }));
    this.AddItem("SaveCopyTitle", (Action) (() =>
    {
      this.RefreshSlotsFor(copySrcLevel, SaveManagementLevel.SMOperation.CopySource, (Func<SaveSlotInfo, bool>) (s => !s.Empty));
      this.Menu.ChangeMenuLevel(copySrcLevel);
    }));
    this.AddItem("SaveClearTitle", (Action) (() =>
    {
      this.RefreshSlotsFor(clearLevel, SaveManagementLevel.SMOperation.Clear, (Func<SaveSlotInfo, bool>) (s => !s.Empty));
      this.Menu.ChangeMenuLevel(clearLevel);
    }));
  }

  private void DrawWarning(
    SpriteBatch batch,
    SpriteFont font,
    GlyphTextRenderer tr,
    float alpha,
    string locString)
  {
    float scale = this.FontManager.SmallFactor * batch.GraphicsDevice.GetViewScale();
    float num = (float) batch.GraphicsDevice.Viewport.Height / 2f;
    tr.DrawCenteredString(batch, font, StaticText.GetString(locString), new Color(1f, 1f, 1f, alpha), new Vector2(0.0f, num * 1.6f), scale);
  }

  private void ReloadSlots()
  {
    this.IsDynamic = false;
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
          saveSlotInfo3.SaveData = saveData;
          this.IsDynamic = true;
        }
      }
    }
  }

  private void RefreshSlotsFor(
    MenuLevel level,
    SaveManagementLevel.SMOperation operation,
    Func<SaveSlotInfo, bool> condition)
  {
    level.Items.Clear();
    level.IsDynamic = this.IsDynamic;
    if (this.IsDynamic)
    {
      for (int index = 0; index < 3; ++index)
        level.AddItem((string) null).Selectable = false;
    }
    foreach (SaveSlotInfo slot in this.Slots)
    {
      SaveSlotInfo s = slot;
      MenuItem menuItem;
      if (slot.Empty)
        (menuItem = level.AddItem((string) null, (Action) (() => this.ChooseSaveSlot(s, operation)))).SuffixText = (Func<string>) (() => StaticText.GetString("NewSlot"));
      else
        (menuItem = level.AddItem("SaveSlotPrefix", (Action) (() => this.ChooseSaveSlot(s, operation)))).SuffixText = (Func<string>) (() => string.Format((IFormatProvider) CultureInfo.InvariantCulture, " {0} ({1:P1} - {2:dd\\.hh\\:mm})", new object[3]
        {
          (object) (s.Index + 1),
          (object) s.Percentage,
          (object) s.PlayTime
        }));
      menuItem.Disabled = !condition(slot);
      menuItem.Selectable = condition(slot);
    }
    for (int index = this.IsDynamic ? 3 : 0; index < level.Items.Count; ++index)
    {
      if (level.Items[index].Selectable)
      {
        level.SelectedIndex = index;
        break;
      }
    }
  }

  private void ChooseSaveSlot(SaveSlotInfo slot, SaveManagementLevel.SMOperation operation)
  {
    switch (operation)
    {
      case SaveManagementLevel.SMOperation.Change:
        this.GameState.SaveSlot = slot.Index;
        this.GameState.LoadSaveFile((Action) (() =>
        {
          this.GameState.Save();
          this.GameState.SaveImmediately();
          this.GameState.Restart();
        }));
        SpeedRun.Dispose();
        break;
      case SaveManagementLevel.SMOperation.CopySource:
        this.CopySourceSlot = slot;
        this.RefreshSlotsFor(this.CopyDestLevel, SaveManagementLevel.SMOperation.CopyDestination, (Func<SaveSlotInfo, bool>) (s => this.CopySourceSlot != s));
        this.Menu.ChangeMenuLevel(this.CopyDestLevel);
        break;
      case SaveManagementLevel.SMOperation.CopyDestination:
        new PCSaveDevice("FEZ").Save("SaveSlot" + (object) slot.Index, (SaveAction) (writer => SaveFileOperations.Write(new CrcWriter(writer), this.CopySourceSlot.SaveData)));
        this.ReloadSlots();
        this.Menu.ChangeMenuLevel((MenuLevel) this);
        break;
      case SaveManagementLevel.SMOperation.Clear:
        new PCSaveDevice("FEZ").Delete("SaveSlot" + (object) slot.Index);
        if (this.GameState.SaveSlot == slot.Index)
        {
          this.GameState.LoadSaveFile((Action) (() =>
          {
            this.GameState.Save();
            this.GameState.SaveImmediately();
            this.GameState.Restart();
          }));
          break;
        }
        this.ReloadSlots();
        this.Menu.ChangeMenuLevel((MenuLevel) this);
        break;
    }
  }

  private void IconPostDraw(
    MenuLevel level,
    SpriteBatch batch,
    SpriteFont font,
    GlyphTextRenderer tr,
    float alpha)
  {
    if (level.SelectedIndex <= 2 || this.Slots[level.SelectedIndex - 3].Empty)
      return;
    float viewScale = batch.GraphicsDevice.GetViewScale();
    int num = (int) (192.0 * (double) viewScale);
    SpriteBatch spriteBatch = batch;
    Texture2D previewTexture = this.Slots[level.SelectedIndex - 3].PreviewTexture;
    Viewport viewport = batch.GraphicsDevice.Viewport;
    int x = viewport.Width / 2;
    viewport = batch.GraphicsDevice.Viewport;
    int y = viewport.Height / 2 - (int) (128.0 * (double) viewScale);
    int width = num;
    int height = num;
    Rectangle destinationRectangle = new Rectangle(x, y, width, height);
    Rectangle? sourceRectangle = new Rectangle?();
    Color white = Color.White;
    Vector2 origin = new Vector2((float) (this.Slots[level.SelectedIndex - 3].PreviewTexture.Width / 2), (float) (this.Slots[level.SelectedIndex - 3].PreviewTexture.Height / 2));
    spriteBatch.Draw(previewTexture, destinationRectangle, sourceRectangle, white, 0.0f, origin, SpriteEffects.None, 0.0f);
  }

  private enum SMOperation
  {
    Change,
    CopySource,
    CopyDestination,
    Clear,
  }
}
