// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.ControlsMenuLevel
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Components;
using FezGame.Services;
using FezGame.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable
namespace FezGame.Structure;

internal class ControlsMenuLevel : MenuLevel
{
  private readonly MenuBase menuBase;
  private SoundEffect sSliderValueIncrease;
  private SoundEffect sSliderValueDecrease;
  private int chosen;
  private MenuItem keyGrabFor;
  private HashSet<Keys> lastPressed = new HashSet<Keys>();
  private HashSet<Keys> thisPressed = new HashSet<Keys>();
  private readonly HashSet<Keys> keysDown = new HashSet<Keys>();
  private GamePadButtons lastButton = new GamePadButtons((Buttons) 0);
  private GamePadButtons thisButton = new GamePadButtons((Buttons) 0);
  private bool forGamepad;
  private bool noArrows;
  private bool mappedButton;
  private MenuItem gamepadFPItem;
  private MenuItem keyboardFPItem;
  private int keyboardStart;
  private int selectorStart;
  private Rectangle? leftSliderRect;
  private Rectangle? rightSliderRect;
  private static readonly MappedAction[] KeyboardActionOrder = new MappedAction[14]
  {
    MappedAction.Jump,
    MappedAction.GrabThrow,
    MappedAction.CancelTalk,
    MappedAction.Up,
    MappedAction.LookUp,
    MappedAction.OpenMap,
    MappedAction.OpenInventory,
    MappedAction.Pause,
    MappedAction.RotateLeft,
    MappedAction.RotateRight,
    MappedAction.ClampLook,
    MappedAction.MapZoomIn,
    MappedAction.MapZoomOut,
    MappedAction.FpViewToggle
  };
  private static readonly MappedAction[] GamepadActionOrder = new MappedAction[12]
  {
    MappedAction.Jump,
    MappedAction.GrabThrow,
    MappedAction.CancelTalk,
    MappedAction.OpenInventory,
    MappedAction.RotateLeft,
    MappedAction.RotateRight,
    MappedAction.MapZoomIn,
    MappedAction.MapZoomOut,
    MappedAction.OpenMap,
    MappedAction.Pause,
    MappedAction.ClampLook,
    MappedAction.FpViewToggle
  };
  private static readonly string[] GamepadButtonOrder = new string[12]
  {
    "{A}",
    "{X}",
    "{B}",
    "{Y}",
    "{LT}",
    "{RT}",
    "{RB}",
    "{LB}",
    "{BACK}",
    "{START}",
    "{RS}",
    "{LS}"
  };

  private void ToggleVibration()
  {
    SettingsManager.Settings.Vibration = !SettingsManager.Settings.Vibration;
  }

  public ControlsMenuLevel(MenuBase menuBase)
  {
    this.InputManager = ServiceHelper.Get<IInputManager>();
    this.GameState = ServiceHelper.Get<IGameStateManager>();
    this.KeyboardManager = ServiceHelper.Get<IKeyboardStateManager>();
    this.MouseState = ServiceHelper.Get<IMouseStateManager>();
    this.menuBase = menuBase;
    this.IsDynamic = true;
    Dictionary<MappedAction, Keys> kmap = SettingsManager.Settings.KeyboardMapping;
    MenuItem gjmi = this.AddItem("ControlsJump");
    gjmi.Selected = (Action) (() => this.ChangeButton(gjmi));
    gjmi.SuffixText = (Func<string>) (() => " : {A}");
    MenuItem gami = this.AddItem("ControlsAction");
    gami.Selected = (Action) (() => this.ChangeButton(gami));
    gami.SuffixText = (Func<string>) (() => " : {X}");
    MenuItem gtmi = this.AddItem("ControlsTalk");
    gtmi.Selected = (Action) (() => this.ChangeButton(gtmi));
    gtmi.SuffixText = (Func<string>) (() => " : {B}");
    MenuItem gimi = this.AddItem("ControlsInventory");
    gimi.Selected = (Action) (() => this.ChangeButton(gimi));
    gimi.SuffixText = (Func<string>) (() => " : {Y}");
    MenuItem grlmi = this.AddItem("ControlsRotateLeft");
    grlmi.Selected = (Action) (() => this.ChangeButton(grlmi));
    grlmi.SuffixText = (Func<string>) (() => " : {LT}");
    MenuItem grrmi = this.AddItem("ControlsRotateRight");
    grrmi.Selected = (Action) (() => this.ChangeButton(grrmi));
    grrmi.SuffixText = (Func<string>) (() => " : {RT}");
    MenuItem gmzimi = this.AddItem("ControlsMapZoomIn");
    gmzimi.Selected = (Action) (() => this.ChangeButton(gmzimi));
    gmzimi.SuffixText = (Func<string>) (() => " : {RB}");
    MenuItem gmzomi = this.AddItem("ControlsZoomOut");
    gmzomi.Selected = (Action) (() => this.ChangeButton(gmzomi));
    gmzomi.SuffixText = (Func<string>) (() => " : {LB}");
    MenuItem gmami = this.AddItem("Map_Title");
    gmami.Selected = (Action) (() => this.ChangeButton(gmami));
    gmami.SuffixText = (Func<string>) (() => " : {BACK}");
    MenuItem gpmi = this.AddItem("ControlsPause");
    gpmi.Selected = (Action) (() => this.ChangeButton(gpmi));
    gpmi.SuffixText = (Func<string>) (() => " : {START}");
    MenuItem gclmi = this.AddItem("ControlsClampLook");
    gclmi.Selected = (Action) (() => this.ChangeButton(gclmi));
    gclmi.SuffixText = (Func<string>) (() => " : {RS}");
    this.gamepadFPItem = this.AddItem((string) null);
    this.gamepadFPItem.Selected = (Action) (() => this.ChangeButton(this.gamepadFPItem));
    this.gamepadFPItem.Selectable = false;
    this.AddItem((string) null, MenuBase.SliderAction).Selectable = false;
    this.AddItem<string>("Vibration", (Action) (() => { }), false, (Func<string>) (() => !SettingsManager.Settings.Vibration ? StaticText.GetString("Off") : StaticText.GetString("On")), (Action<string, int>) ((_, __) => this.ToggleVibration()));
    this.AddItem<string>("DeadZone", (Action) (() => { }), false, (Func<string>) (() => SettingsManager.Settings.DeadZone.ToString() + "%"), (Action<string, int>) ((_, diff) =>
    {
      SettingsManager.Settings.DeadZone += 10 * diff;
      if (SettingsManager.Settings.DeadZone < 0)
      {
        SettingsManager.Settings.DeadZone = 0;
      }
      else
      {
        if (SettingsManager.Settings.DeadZone <= 90)
          return;
        SettingsManager.Settings.DeadZone = 90;
      }
    }));
    this.AddItem("ResetToDefault", (Action) (() => this.ResetToDefault(false, true)));
    this.keyboardStart = this.Items.Count;
    MenuItem jmi = this.AddItem("ControlsJump");
    jmi.Selected = (Action) (() => this.ChangeKey(jmi));
    jmi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.Jump]));
    MenuItem ami = this.AddItem("ControlsAction");
    ami.Selected = (Action) (() => this.ChangeKey(ami));
    ami.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.GrabThrow]));
    MenuItem tmi = this.AddItem("ControlsTalk");
    tmi.Selected = (Action) (() => this.ChangeKey(tmi));
    tmi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.CancelTalk]));
    this.AddItem<ControlsMenuLevel.ArrowKeyMapping>("ControlsMove", MenuBase.SliderAction, false, (Func<ControlsMenuLevel.ArrowKeyMapping>) (() => this.UpToAKM(kmap[MappedAction.Up])), (Action<ControlsMenuLevel.ArrowKeyMapping, int>) ((lastValue, change) =>
    {
      ControlsMenuLevel.ArrowKeyMapping akm = this.UpToAKM(kmap[MappedAction.Up]) + change;
      if (akm == (ControlsMenuLevel.ArrowKeyMapping.ZQSD | ControlsMenuLevel.ArrowKeyMapping.Arrows))
        akm = ControlsMenuLevel.ArrowKeyMapping.WASD;
      if (akm < ControlsMenuLevel.ArrowKeyMapping.WASD)
        akm = ControlsMenuLevel.ArrowKeyMapping.Arrows;
      kmap[MappedAction.Up] = this.AKMToKey(akm, 0);
      kmap[MappedAction.Left] = this.AKMToKey(akm, 1);
      kmap[MappedAction.Down] = this.AKMToKey(akm, 2);
      kmap[MappedAction.Right] = this.AKMToKey(akm, 3);
      this.KeyboardManager.UpdateMapping();
      this.ValidateKeyCollision();
    })).SuffixText = (Func<string>) (() => " : " + this.Localize((object) this.UpToAKM(kmap[MappedAction.Up])));
    this.AddItem<ControlsMenuLevel.ArrowKeyMapping>("ControlsLook", MenuBase.SliderAction, false, (Func<ControlsMenuLevel.ArrowKeyMapping>) (() => this.UpToAKM(kmap[MappedAction.LookUp])), (Action<ControlsMenuLevel.ArrowKeyMapping, int>) ((lastValue, change) =>
    {
      ControlsMenuLevel.ArrowKeyMapping akm = this.UpToAKM(kmap[MappedAction.LookUp]) + change;
      if (akm == (ControlsMenuLevel.ArrowKeyMapping.ZQSD | ControlsMenuLevel.ArrowKeyMapping.Arrows))
        akm = ControlsMenuLevel.ArrowKeyMapping.WASD;
      if (akm < ControlsMenuLevel.ArrowKeyMapping.WASD)
        akm = ControlsMenuLevel.ArrowKeyMapping.Arrows;
      kmap[MappedAction.LookUp] = this.AKMToKey(akm, 0);
      kmap[MappedAction.LookLeft] = this.AKMToKey(akm, 1);
      kmap[MappedAction.LookDown] = this.AKMToKey(akm, 2);
      kmap[MappedAction.LookRight] = this.AKMToKey(akm, 3);
      this.KeyboardManager.UpdateMapping();
      this.ValidateKeyCollision();
    })).SuffixText = (Func<string>) (() => " : " + this.Localize((object) this.UpToAKM(kmap[MappedAction.LookUp])));
    MenuItem mami = this.AddItem("Map_Title");
    mami.Selected = (Action) (() => this.ChangeKey(mami));
    mami.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.OpenMap]));
    MenuItem imi = this.AddItem("ControlsInventory");
    imi.Selected = (Action) (() => this.ChangeKey(imi));
    imi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.OpenInventory]));
    MenuItem pmi = this.AddItem("ControlsPause");
    pmi.Selected = (Action) (() => this.ChangeKey(pmi));
    pmi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.Pause]));
    MenuItem rlmi = this.AddItem("ControlsRotateLeft");
    rlmi.Selected = (Action) (() => this.ChangeKey(rlmi));
    rlmi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.RotateLeft]));
    MenuItem rrmi = this.AddItem("ControlsRotateRight");
    rrmi.Selected = (Action) (() => this.ChangeKey(rrmi));
    rrmi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.RotateRight]));
    MenuItem clmi = this.AddItem("ControlsClampLook");
    clmi.Selected = (Action) (() => this.ChangeKey(clmi));
    clmi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.ClampLook]));
    MenuItem mzimi = this.AddItem("ControlsMapZoomIn");
    mzimi.Selected = (Action) (() => this.ChangeKey(mzimi));
    mzimi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.MapZoomIn]));
    MenuItem mzomi = this.AddItem("ControlsZoomOut");
    mzomi.Selected = (Action) (() => this.ChangeKey(mzomi));
    mzomi.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kmap[MappedAction.MapZoomOut]));
    this.keyboardFPItem = this.AddItem((string) null);
    this.keyboardFPItem.Selected = (Action) (() => this.ChangeKey(this.keyboardFPItem));
    this.keyboardFPItem.Selectable = false;
    this.AddItem((string) null, MenuBase.SliderAction).Selectable = false;
    this.AddItem("ResetToDefault", (Action) (() => this.ResetToDefault(true, false)));
    this.selectorStart = this.Items.Count;
    this.AddItem("Controller", MenuBase.SliderAction, true).UpperCase = true;
    this.AddItem("Keyboard", MenuBase.SliderAction, true).UpperCase = true;
  }

  public override void Reset()
  {
    base.Reset();
    this.Items[this.selectorStart].Hidden = true;
    this.Items[this.selectorStart].Selectable = false;
    this.Items[this.selectorStart + 1].Hidden = true;
    this.Items[this.selectorStart + 1].Selectable = false;
    this.chosen = this.Items.Count - 1;
    this.FakeSlideRight(true);
    this.noArrows = !GamepadState.AnyConnected;
    if (!this.GameState.SaveData.HasFPView)
      return;
    this.gamepadFPItem.Text = "ControlsToggleFpView";
    this.keyboardFPItem.Text = "ControlsToggleFpView";
    this.gamepadFPItem.SuffixText = (Func<string>) (() => " : {LS}");
    this.keyboardFPItem.SuffixText = (Func<string>) (() => " : " + this.Localize((object) SettingsManager.Settings.KeyboardMapping[MappedAction.FpViewToggle]));
    this.gamepadFPItem.Selectable = true;
    this.keyboardFPItem.Selectable = true;
  }

  private void ResetToDefault(bool forKeyboard, bool forGamepad)
  {
    SettingsManager.Settings.ResetMapping(forKeyboard, forGamepad);
    if (forGamepad)
    {
      SettingsManager.Settings.Vibration = false;
      this.ToggleVibration();
      SettingsManager.Settings.DeadZone = 40;
    }
    if (!forKeyboard)
      return;
    this.ValidateKeyCollision();
  }

  private void ChangeButton(MenuItem mi)
  {
    if (this.TrapInput)
      return;
    if (mi != this.gamepadFPItem || this.GameState.SaveData.HasFPView)
      mi.SuffixText = (Func<string>) (() => " : " + StaticText.GetString("ChangeGamepadMapping"));
    this.keyGrabFor = mi;
    this.TrapInput = true;
    this.forGamepad = true;
  }

  private void ChangeKey(MenuItem mi)
  {
    if (this.TrapInput)
      return;
    if (mi != this.keyboardFPItem || this.GameState.SaveData.HasFPView)
      mi.SuffixText = (Func<string>) (() => " : " + StaticText.GetString("ChangeMapping"));
    this.keyGrabFor = mi;
    this.TrapInput = true;
    this.forGamepad = false;
    this.lastPressed.Clear();
    this.keysDown.Clear();
    foreach (Keys pressedKey in Keyboard.GetState().GetPressedKeys())
      this.lastPressed.Add(pressedKey);
  }

  private string Localize(object input)
  {
    string str = input.ToString();
    if (((IEnumerable<char>) str.ToCharArray()).All<char>(new Func<char, bool>(char.IsUpper)))
      return str;
    if (str.StartsWith("D") && str.Length == 2 && char.IsNumber(str[1]))
      return str[1].ToString();
    string input1 = str.Replace("Oem", string.Empty);
    string text;
    return StaticText.TryGetString("Keyboard" + input1, out text) ? text : Regex.Replace(input1, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
  }

  private ControlsMenuLevel.ArrowKeyMapping UpToAKM(Keys key)
  {
    switch (key)
    {
      case Keys.E:
        return ControlsMenuLevel.ArrowKeyMapping.ESDF;
      case Keys.I:
        return ControlsMenuLevel.ArrowKeyMapping.IJKL;
      case Keys.W:
        return ControlsMenuLevel.ArrowKeyMapping.WASD;
      case Keys.Z:
        return ControlsMenuLevel.ArrowKeyMapping.ZQSD;
      default:
        return ControlsMenuLevel.ArrowKeyMapping.Arrows;
    }
  }

  private Keys AKMToKey(ControlsMenuLevel.ArrowKeyMapping akm, int i)
  {
    Keys key;
    switch (akm)
    {
      case ControlsMenuLevel.ArrowKeyMapping.WASD:
        switch (i)
        {
          case 0:
            key = Keys.W;
            break;
          case 1:
            key = Keys.A;
            break;
          case 2:
            key = Keys.S;
            break;
          default:
            key = Keys.D;
            break;
        }
        break;
      case ControlsMenuLevel.ArrowKeyMapping.ZQSD:
        switch (i)
        {
          case 0:
            key = Keys.Z;
            break;
          case 1:
            key = Keys.Q;
            break;
          case 2:
            key = Keys.S;
            break;
          default:
            key = Keys.D;
            break;
        }
        break;
      case ControlsMenuLevel.ArrowKeyMapping.IJKL:
        switch (i)
        {
          case 0:
            key = Keys.I;
            break;
          case 1:
            key = Keys.J;
            break;
          case 2:
            key = Keys.K;
            break;
          default:
            key = Keys.L;
            break;
        }
        break;
      case ControlsMenuLevel.ArrowKeyMapping.ESDF:
        switch (i)
        {
          case 0:
            key = Keys.E;
            break;
          case 1:
            key = Keys.S;
            break;
          case 2:
            key = Keys.D;
            break;
          default:
            key = Keys.F;
            break;
        }
        break;
      default:
        switch (i)
        {
          case 0:
            key = Keys.Up;
            break;
          case 1:
            key = Keys.Left;
            break;
          case 2:
            key = Keys.Down;
            break;
          default:
            key = Keys.Right;
            break;
        }
        break;
    }
    return key;
  }

  public override void Update(TimeSpan elapsed)
  {
    base.Update(elapsed);
    this.lastButton = this.thisButton;
    if (GamepadState.AnyConnected)
    {
      this.noArrows = false;
      this.thisButton = GamePad.GetState(this.InputManager.ActiveGamepad.PlayerIndex).Buttons;
    }
    else
    {
      this.noArrows = true;
      if (this.SelectedIndex == this.selectorStart)
        this.FakeSlideRight(true);
    }
    if (this.mappedButton)
    {
      this.TrapInput = false;
      this.mappedButton = false;
    }
    if (this.TrapInput)
    {
      if (this.forGamepad)
      {
        if (this.KeyboardManager.GetKeyState(Keys.Escape) == FezButtonState.Pressed)
        {
          if (this.keyGrabFor != this.gamepadFPItem && this.GameState.SaveData.HasFPView)
            this.keyGrabFor.SuffixText = (Func<string>) (() => " : " + ControlsMenuLevel.GamepadButtonOrder[this.Items.IndexOf(this.keyGrabFor)]);
          this.TrapInput = false;
          this.forGamepad = false;
        }
        else if (this.thisButton != this.lastButton)
        {
          int num = this.thisButton.GetHashCode() & 12645360;
          if (num != 0)
          {
            int j = this.Items.IndexOf(this.keyGrabFor);
            MappedAction key = ControlsMenuLevel.GamepadActionOrder[j];
            Dictionary<MappedAction, Buttons> controllerMapping = SettingsManager.Settings.ControllerMapping;
            Buttons buttons = controllerMapping[key];
            if (this.thisButton.Start == ButtonState.Pressed)
              controllerMapping[key] = Buttons.Start;
            else if (this.thisButton.Back == ButtonState.Pressed)
              controllerMapping[key] = Buttons.Back;
            else if (this.thisButton.LeftStick == ButtonState.Pressed)
              controllerMapping[key] = Buttons.LeftStick;
            else if (this.thisButton.RightStick == ButtonState.Pressed)
              controllerMapping[key] = Buttons.RightStick;
            else if (this.thisButton.LeftShoulder == ButtonState.Pressed)
              controllerMapping[key] = Buttons.LeftShoulder;
            else if (this.thisButton.RightShoulder == ButtonState.Pressed)
              controllerMapping[key] = Buttons.RightShoulder;
            else if (this.thisButton.A == ButtonState.Pressed)
              controllerMapping[key] = Buttons.A;
            else if (this.thisButton.B == ButtonState.Pressed)
              controllerMapping[key] = Buttons.B;
            else if (this.thisButton.X == ButtonState.Pressed)
              controllerMapping[key] = Buttons.X;
            else if (this.thisButton.Y == ButtonState.Pressed)
              controllerMapping[key] = Buttons.Y;
            else if ((num & 4194304 /*0x400000*/) == 4194304 /*0x400000*/)
            {
              controllerMapping[key] = Buttons.RightTrigger;
            }
            else
            {
              if ((num & 8388608 /*0x800000*/) != 8388608 /*0x800000*/)
                throw new InvalidOperationException("How did you get here...?");
              controllerMapping[key] = Buttons.LeftTrigger;
            }
            if (this.keyGrabFor != this.gamepadFPItem || this.GameState.SaveData.HasFPView)
              this.keyGrabFor.SuffixText = (Func<string>) (() => " : " + ControlsMenuLevel.GamepadButtonOrder[j]);
            MappedAction? nullable = new MappedAction?();
            foreach (KeyValuePair<MappedAction, Buttons> keyValuePair in controllerMapping)
            {
              if (keyValuePair.Value == controllerMapping[key] && keyValuePair.Key != key)
              {
                nullable = new MappedAction?(keyValuePair.Key);
                break;
              }
            }
            if (nullable.HasValue)
            {
              controllerMapping[nullable.Value] = buttons;
              for (int k = 0; k < ControlsMenuLevel.GamepadActionOrder.Length; ++k)
              {
                if (ControlsMenuLevel.GamepadActionOrder[k] == nullable.Value)
                {
                  if (ControlsMenuLevel.GamepadActionOrder[k] != MappedAction.FpViewToggle || this.GameState.SaveData.HasFPView)
                  {
                    this.Items[k].SuffixText = (Func<string>) (() => " : " + ControlsMenuLevel.GamepadButtonOrder[k]);
                    break;
                  }
                  break;
                }
              }
            }
            this.mappedButton = true;
            this.forGamepad = false;
          }
        }
      }
      else
      {
        this.thisPressed.Clear();
        foreach (Keys pressedKey in Keyboard.GetState().GetPressedKeys())
          this.thisPressed.Add(pressedKey);
        foreach (Keys keys1 in this.keysDown)
        {
          if (!this.thisPressed.Contains(keys1))
          {
            Keys keys2 = keys1;
            int num = this.Items.IndexOf(this.keyGrabFor);
            MappedAction mappedAction = ControlsMenuLevel.KeyboardActionOrder[num - this.keyboardStart];
            Dictionary<MappedAction, Keys> kMap = SettingsManager.Settings.KeyboardMapping;
            kMap[mappedAction] = keys2;
            if (this.keyGrabFor != this.keyboardFPItem || this.GameState.SaveData.HasFPView)
              this.keyGrabFor.SuffixText = (Func<string>) (() => " : " + this.Localize((object) kMap[mappedAction]));
            this.KeyboardManager.UpdateMapping();
            this.ValidateKeyCollision();
            this.TrapInput = false;
            break;
          }
        }
        foreach (Keys keys in this.thisPressed)
        {
          if (!this.lastPressed.Contains(keys))
            this.keysDown.Add(keys);
        }
        HashSet<Keys> thisPressed = this.thisPressed;
        this.thisPressed = this.lastPressed;
        this.lastPressed = thisPressed;
      }
    }
    if (this.SelectedIndex < this.Items.Count - 2)
      return;
    Point position = this.MouseState.Position;
    if (this.leftSliderRect.HasValue && this.leftSliderRect.Value.Contains(position))
    {
      this.menuBase.CursorSelectable = true;
      if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
        this.FakeSlideLeft();
    }
    else if (this.rightSliderRect.HasValue && this.rightSliderRect.Value.Contains(position))
    {
      this.menuBase.CursorSelectable = true;
      if (this.MouseState.LeftButton.State == MouseButtonStates.Pressed)
        this.FakeSlideRight();
    }
    if (this.InputManager.Right == FezButtonState.Pressed)
    {
      this.FakeSlideRight();
    }
    else
    {
      if (this.InputManager.Left != FezButtonState.Pressed)
        return;
      this.FakeSlideLeft();
    }
  }

  private void ValidateKeyCollision()
  {
    Dictionary<MappedAction, Keys> keyboardMapping = SettingsManager.Settings.KeyboardMapping;
    for (int index = 0; index < ControlsMenuLevel.KeyboardActionOrder.Length; ++index)
      this.Items[index + this.keyboardStart].InError = false;
    for (int index1 = 0; index1 < ControlsMenuLevel.KeyboardActionOrder.Length; ++index1)
    {
      for (int index2 = 0; index2 < ControlsMenuLevel.KeyboardActionOrder.Length; ++index2)
      {
        if (index1 != index2)
        {
          MenuItem menuItem1 = this.Items[index1 + this.keyboardStart];
          MenuItem menuItem2 = this.Items[index2 + this.keyboardStart];
          if (keyboardMapping[ControlsMenuLevel.KeyboardActionOrder[index1]] == keyboardMapping[ControlsMenuLevel.KeyboardActionOrder[index2]])
          {
            menuItem1.InError = true;
            menuItem2.InError = true;
          }
        }
      }
    }
  }

  private void FakeSlideRight(bool silent = false)
  {
    this.Items[this.chosen].Hidden = true;
    this.Items[this.chosen].Selectable = false;
    int chosen = this.chosen;
    ++this.chosen;
    if (this.chosen == this.Items.Count)
      this.chosen = this.selectorStart;
    if (!GamepadState.AnyConnected && this.chosen == this.selectorStart)
      ++this.chosen;
    int num = this.chosen - this.selectorStart;
    for (int index = 0; index < this.keyboardStart; ++index)
      this.Items[index].Hidden = num != 0;
    for (int keyboardStart = this.keyboardStart; keyboardStart < this.selectorStart; ++keyboardStart)
      this.Items[keyboardStart].Hidden = num != 1;
    this.Items[this.chosen].Hidden = false;
    this.Items[this.chosen].Selectable = true;
    this.SelectedIndex = this.chosen;
    if (silent || chosen == this.chosen)
      return;
    this.sSliderValueIncrease.Emit();
  }

  private void FakeSlideLeft(bool silent = false)
  {
    this.Items[this.chosen].Hidden = true;
    this.Items[this.chosen].Selectable = false;
    int chosen = this.chosen;
    --this.chosen;
    if (!GamepadState.AnyConnected && this.chosen == this.selectorStart)
      ++this.chosen;
    if (this.chosen == this.selectorStart - 1)
      this.chosen = this.Items.Count - 1;
    int num = this.chosen - this.selectorStart;
    for (int index = 0; index < this.keyboardStart; ++index)
      this.Items[index].Hidden = num != 0;
    for (int keyboardStart = this.keyboardStart; keyboardStart < this.selectorStart; ++keyboardStart)
      this.Items[keyboardStart].Hidden = num != 1;
    this.Items[this.chosen].Hidden = false;
    this.Items[this.chosen].Selectable = true;
    this.SelectedIndex = this.chosen;
    if (silent || chosen == this.chosen)
      return;
    this.sSliderValueDecrease.Emit();
  }

  public override void Initialize()
  {
    ContentManager contentManager = this.CMProvider.Get(CM.Menu);
    this.sSliderValueDecrease = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/SliderValueDecrease");
    this.sSliderValueIncrease = contentManager.Load<SoundEffect>("Sounds/Ui/Menu/SliderValueIncrease");
    base.Initialize();
  }

  public override string AButtonString
  {
    get
    {
      if (this.SelectedItem.IsSlider || this.SelectedIndex == this.chosen)
        return (string) null;
      return this.SelectedItem.SuffixText == null ? StaticText.GetString("MenuApplyWithGlyph") : StaticText.GetString("ChangeWithGlyph");
    }
  }

  public override void PostDraw(
    SpriteBatch batch,
    SpriteFont font,
    GlyphTextRenderer tr,
    float alpha)
  {
    float viewScale = batch.GraphicsDevice.GetViewScale();
    int num1 = batch.GraphicsDevice.Viewport.Height / 2;
    float num2 = this.Items[this.chosen].Size.X + 70f;
    if (Culture.IsCJK)
      num2 *= 0.5f;
    if (this.SelectedIndex >= this.Items.Count - 2)
    {
      float num3 = 25f;
      float num4;
      if (!Culture.IsCJK)
      {
        num4 = num3 * viewScale;
      }
      else
      {
        num2 = num2 * 0.4f + 25f;
        num4 = 5f * viewScale;
        if (Culture.Language == Language.Chinese)
          num4 = (float) (10.0 + 25.0 * (double) viewScale);
      }
      int num5 = ServiceHelper.Game.GraphicsDevice.Viewport.Width / 2;
      Vector2 offset = new Vector2((float) (-(double) num2 - 40.0 * ((double) viewScale - 1.0)), (float) num1 + 180f * viewScale + num4);
      int num6 = ServiceHelper.Game.GraphicsDevice.PresentationParameters.BackBufferWidth / 2 - num5;
      int num7 = (ServiceHelper.Game.GraphicsDevice.PresentationParameters.BackBufferHeight - ServiceHelper.Game.GraphicsDevice.Viewport.Height) / 2;
      if (!this.noArrows)
      {
        tr.DrawCenteredString(batch, this.FontManager.Big, "{LA}", new Color(1f, 1f, 1f, alpha), offset, (Culture.IsCJK ? 0.2f : 1f) * viewScale);
        this.leftSliderRect = new Rectangle?(new Rectangle((int) ((double) num6 + (double) offset.X + (double) num5 - 25.0 * (double) viewScale), (int) ((double) num7 + (double) offset.Y), (int) (40.0 * (double) viewScale), (int) (25.0 * (double) viewScale)));
      }
      else
        this.leftSliderRect = new Rectangle?();
      offset = new Vector2(num2 + (float) (40.0 * ((double) viewScale - 1.0)), (float) num1 + 180f * viewScale + num4);
      if (!this.noArrows)
      {
        tr.DrawCenteredString(batch, this.FontManager.Big, "{RA}", new Color(1f, 1f, 1f, alpha), offset, (Culture.IsCJK ? 0.2f : 1f) * viewScale);
        this.rightSliderRect = new Rectangle?(new Rectangle((int) ((double) num6 + (double) offset.X + (double) num5 - 30.0 * (double) viewScale), (int) ((double) num7 + (double) offset.Y), (int) (40.0 * (double) viewScale), (int) (25.0 * (double) viewScale)));
      }
      else
        this.rightSliderRect = new Rectangle?();
    }
    else
      this.leftSliderRect = this.rightSliderRect = new Rectangle?();
  }

  private void DrawLeftAligned(
    GlyphTextRenderer tr,
    SpriteBatch batch,
    SpriteFont font,
    string text,
    float alpha,
    Vector2 offset,
    float size)
  {
    float num = font.MeasureString(text).X * size;
    tr.DrawShadowedText(batch, font, text, offset - num * Vector2.UnitX, new Color(1f, 1f, 1f, alpha), size);
  }

  public IFontManager FontManager { private get; set; }

  public IInputManager InputManager { private get; set; }

  public IKeyboardStateManager KeyboardManager { private get; set; }

  public IMouseStateManager MouseState { private get; set; }

  public IGameStateManager GameState { private get; set; }

  private enum ArrowKeyMapping
  {
    WASD,
    ZQSD,
    IJKL,
    ESDF,
    Arrows,
  }
}
