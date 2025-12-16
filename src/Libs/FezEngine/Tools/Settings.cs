// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.Settings
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class Settings
{
  public bool UseCurrentMode { get; set; }

  public ScreenMode ScreenMode { get; set; }

  [Serialization(Optional = true)]
  public ScaleMode ScaleMode { get; set; }

  public int Width { get; set; }

  public int Height { get; set; }

  [Serialization(Optional = true)]
  public bool HighDPI { get; set; }

  public Language Language { get; set; }

  public float SoundVolume { get; set; }

  public float MusicVolume { get; set; }

  [Serialization(Optional = true)]
  public bool Vibration { get; set; }

  [Serialization(Optional = true)]
  public bool PauseOnLostFocus { get; set; }

  [Serialization(Optional = true)]
  public bool Singlethreaded { get; set; }

  [Serialization(Optional = true)]
  public float Brightness { get; set; }

  public Dictionary<MappedAction, Keys> KeyboardMapping { get; set; }

  public Dictionary<MappedAction, Buttons> ControllerMapping { get; set; }

  [Serialization(Optional = true)]
  public Dictionary<MappedAction, Keys> UiKeyboardMapping { get; set; }

  [Serialization(Optional = true)]
  public int DeadZone { get; set; }

  [Serialization(Optional = true)]
  public bool DisableController { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool InvertMouse
  {
    get => false;
    set => this.InvertLook = value;
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool InvertLook
  {
    get => false;
    set => this.InvertLookX = this.InvertLookY = value;
  }

  [Serialization(Optional = true)]
  public bool InvertLookX { get; set; }

  [Serialization(Optional = true)]
  public bool InvertLookY { get; set; }

  [Serialization(Optional = true)]
  public bool VSync { get; set; }

  [Serialization(Optional = true)]
  public bool HardwareInstancing { get; set; }

  [Serialization(Optional = true)]
  public int MultiSampleCount { get; set; }

  [Serialization(Optional = true)]
  public bool MultiSampleOption { get; set; }

  [Serialization(Optional = true)]
  public bool Lighting { get; set; }

  public Settings()
  {
    this.KeyboardMapping = new Dictionary<MappedAction, Keys>();
    this.ControllerMapping = new Dictionary<MappedAction, Buttons>();
    this.UiKeyboardMapping = new Dictionary<MappedAction, Keys>();
    this.RevertToDefaults();
  }

  public void RevertToDefaults()
  {
    DisplayMode currentDisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
    this.ScreenMode = ScreenMode.Fullscreen;
    this.ScaleMode = ScaleMode.FullAspect;
    this.Width = currentDisplayMode.Width;
    this.Height = currentDisplayMode.Height;
    this.HighDPI = false;
    this.Language = Culture.Language;
    this.Brightness = 0.5f;
    this.SoundVolume = 1f;
    this.MusicVolume = 1f;
    this.Vibration = true;
    this.PauseOnLostFocus = true;
    this.Singlethreaded = false;
    this.InvertMouse = false;
    this.DeadZone = 40;
    this.DisableController = false;
    this.VSync = true;
    this.HardwareInstancing = true;
    this.MultiSampleCount = 0;
    this.MultiSampleOption = false;
    this.Lighting = true;
    this.ResetMapping();
  }

  public void ResetMapping(bool forKeyboard = true, bool forGamepad = true)
  {
    if (forKeyboard)
    {
      this.KeyboardMapping[MappedAction.Jump] = Keyboard.GetKeyFromScancodeEXT(Keys.Space);
      this.KeyboardMapping[MappedAction.GrabThrow] = Keyboard.GetKeyFromScancodeEXT(Keys.LeftControl);
      this.KeyboardMapping[MappedAction.CancelTalk] = Keyboard.GetKeyFromScancodeEXT(Keys.LeftShift);
      this.KeyboardMapping[MappedAction.Up] = Keyboard.GetKeyFromScancodeEXT(Keys.Up);
      this.KeyboardMapping[MappedAction.Down] = Keyboard.GetKeyFromScancodeEXT(Keys.Down);
      this.KeyboardMapping[MappedAction.Left] = Keyboard.GetKeyFromScancodeEXT(Keys.Left);
      this.KeyboardMapping[MappedAction.Right] = Keyboard.GetKeyFromScancodeEXT(Keys.Right);
      this.KeyboardMapping[MappedAction.LookUp] = Keyboard.GetKeyFromScancodeEXT(Keys.I);
      this.KeyboardMapping[MappedAction.LookDown] = Keyboard.GetKeyFromScancodeEXT(Keys.K);
      this.KeyboardMapping[MappedAction.LookRight] = Keyboard.GetKeyFromScancodeEXT(Keys.L);
      this.KeyboardMapping[MappedAction.LookLeft] = Keyboard.GetKeyFromScancodeEXT(Keys.J);
      this.KeyboardMapping[MappedAction.OpenMap] = Keyboard.GetKeyFromScancodeEXT(Keys.Escape);
      this.KeyboardMapping[MappedAction.OpenInventory] = Keyboard.GetKeyFromScancodeEXT(Keys.Tab);
      this.KeyboardMapping[MappedAction.MapZoomIn] = Keyboard.GetKeyFromScancodeEXT(Keys.W);
      this.KeyboardMapping[MappedAction.MapZoomOut] = Keyboard.GetKeyFromScancodeEXT(Keys.S);
      this.KeyboardMapping[MappedAction.Pause] = Keyboard.GetKeyFromScancodeEXT(Keys.Enter);
      this.KeyboardMapping[MappedAction.RotateLeft] = Keyboard.GetKeyFromScancodeEXT(Keys.A);
      this.KeyboardMapping[MappedAction.RotateRight] = Keyboard.GetKeyFromScancodeEXT(Keys.D);
      this.KeyboardMapping[MappedAction.FpViewToggle] = Keyboard.GetKeyFromScancodeEXT(Keys.RightAlt);
      this.KeyboardMapping[MappedAction.ClampLook] = Keyboard.GetKeyFromScancodeEXT(Keys.RightShift);
    }
    if (forGamepad)
    {
      this.ControllerMapping[MappedAction.Jump] = Buttons.A;
      this.ControllerMapping[MappedAction.GrabThrow] = Buttons.X;
      this.ControllerMapping[MappedAction.CancelTalk] = Buttons.B;
      this.ControllerMapping[MappedAction.OpenMap] = Buttons.Back;
      this.ControllerMapping[MappedAction.OpenInventory] = Buttons.Y;
      this.ControllerMapping[MappedAction.MapZoomIn] = Buttons.RightShoulder;
      this.ControllerMapping[MappedAction.MapZoomOut] = Buttons.LeftShoulder;
      this.ControllerMapping[MappedAction.Pause] = Buttons.Start;
      this.ControllerMapping[MappedAction.RotateLeft] = Buttons.LeftTrigger;
      this.ControllerMapping[MappedAction.RotateRight] = Buttons.RightTrigger;
      this.ControllerMapping[MappedAction.FpViewToggle] = Buttons.LeftStick;
      this.ControllerMapping[MappedAction.ClampLook] = Buttons.RightStick;
    }
    this.UiKeyboardMapping[MappedAction.Up] = Keyboard.GetKeyFromScancodeEXT(Keys.Up);
    this.UiKeyboardMapping[MappedAction.Down] = Keyboard.GetKeyFromScancodeEXT(Keys.Down);
    this.UiKeyboardMapping[MappedAction.Left] = Keyboard.GetKeyFromScancodeEXT(Keys.Left);
    this.UiKeyboardMapping[MappedAction.Right] = Keyboard.GetKeyFromScancodeEXT(Keys.Right);
    this.UiKeyboardMapping[MappedAction.Jump] = Keyboard.GetKeyFromScancodeEXT(Keys.Enter);
    this.UiKeyboardMapping[MappedAction.CancelTalk] = Keyboard.GetKeyFromScancodeEXT(Keys.Escape);
    this.UiKeyboardMapping[MappedAction.Pause] = Keyboard.GetKeyFromScancodeEXT(Keys.Enter);
    this.UiKeyboardMapping[MappedAction.OpenMap] = Keyboard.GetKeyFromScancodeEXT(Keys.Escape);
    this.UiKeyboardMapping[MappedAction.GrabThrow] = Keyboard.GetKeyFromScancodeEXT(Keys.LeftControl);
    this.UiKeyboardMapping[MappedAction.LookUp] = Keyboard.GetKeyFromScancodeEXT(Keys.I);
    this.UiKeyboardMapping[MappedAction.LookDown] = Keyboard.GetKeyFromScancodeEXT(Keys.K);
    this.UiKeyboardMapping[MappedAction.LookRight] = Keyboard.GetKeyFromScancodeEXT(Keys.L);
    this.UiKeyboardMapping[MappedAction.LookLeft] = Keyboard.GetKeyFromScancodeEXT(Keys.J);
    this.UiKeyboardMapping[MappedAction.OpenInventory] = Keyboard.GetKeyFromScancodeEXT(Keys.Tab);
    this.UiKeyboardMapping[MappedAction.MapZoomIn] = Keyboard.GetKeyFromScancodeEXT(Keys.W);
    this.UiKeyboardMapping[MappedAction.MapZoomOut] = Keyboard.GetKeyFromScancodeEXT(Keys.S);
    this.UiKeyboardMapping[MappedAction.RotateLeft] = Keyboard.GetKeyFromScancodeEXT(Keys.A);
    this.UiKeyboardMapping[MappedAction.RotateRight] = Keyboard.GetKeyFromScancodeEXT(Keys.D);
    this.UiKeyboardMapping[MappedAction.FpViewToggle] = Keyboard.GetKeyFromScancodeEXT(Keys.RightAlt);
    this.UiKeyboardMapping[MappedAction.ClampLook] = Keyboard.GetKeyFromScancodeEXT(Keys.RightShift);
  }
}
