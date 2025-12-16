// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.KeyboardStateManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public class KeyboardStateManager : IKeyboardStateManager
{
  private readonly Dictionary<Keys, FezButtonState> keyStates = new Dictionary<Keys, FezButtonState>((IEqualityComparer<Keys>) KeysEqualityComparer.Default);
  private readonly List<Keys> registeredKeys = new List<Keys>();
  private Dictionary<MappedAction, Keys> lastMapping;
  private bool enterDown;
  private readonly List<Keys> activeKeys = new List<Keys>();

  public bool IgnoreMapping { get; set; }

  public KeyboardStateManager() => this.UpdateMapping();

  public FezButtonState GetKeyState(Keys key)
  {
    FezButtonState keyState;
    if (!this.keyStates.TryGetValue(key, out keyState))
      keyState = FezButtonState.Up;
    return keyState;
  }

  public void RegisterKey(Keys key)
  {
    lock (this)
    {
      if (this.registeredKeys.Contains(key))
        return;
      this.registeredKeys.Add(key);
    }
  }

  public void UpdateMapping()
  {
    Dictionary<MappedAction, Keys> keyboardMapping = SettingsManager.Settings.KeyboardMapping;
    if (this.lastMapping != null)
    {
      foreach (Keys keys in this.lastMapping.Values)
        this.registeredKeys.Remove(keys);
    }
    foreach (Keys key in keyboardMapping.Values)
      this.RegisterKey(key);
    this.RegisterKey(Keys.Down);
    this.RegisterKey(Keys.Up);
    this.RegisterKey(Keys.Right);
    this.RegisterKey(Keys.Left);
    this.RegisterKey(Keys.Enter);
    this.RegisterKey(Keys.Escape);
    this.lastMapping = keyboardMapping;
  }

  private FezButtonState GetUIMapping(MappedAction action)
  {
    return this.GetKeyState(this.IgnoreMapping ? SettingsManager.Settings.UiKeyboardMapping[action] : this.lastMapping[action]);
  }

  public FezButtonState Up => this.GetUIMapping(MappedAction.Up);

  public FezButtonState Down => this.GetUIMapping(MappedAction.Down);

  public FezButtonState Left => this.GetUIMapping(MappedAction.Left);

  public FezButtonState Right => this.GetUIMapping(MappedAction.Right);

  public FezButtonState Jump => this.GetUIMapping(MappedAction.Jump);

  public FezButtonState CancelTalk => this.GetUIMapping(MappedAction.CancelTalk);

  public FezButtonState Pause => this.GetUIMapping(MappedAction.Pause);

  public FezButtonState OpenMap => this.GetUIMapping(MappedAction.OpenMap);

  public FezButtonState GrabThrow => this.GetKeyState(this.lastMapping[MappedAction.GrabThrow]);

  public FezButtonState LookUp => this.GetKeyState(this.lastMapping[MappedAction.LookUp]);

  public FezButtonState LookDown => this.GetKeyState(this.lastMapping[MappedAction.LookDown]);

  public FezButtonState LookRight => this.GetKeyState(this.lastMapping[MappedAction.LookRight]);

  public FezButtonState LookLeft => this.GetKeyState(this.lastMapping[MappedAction.LookLeft]);

  public FezButtonState MapZoomIn => this.GetKeyState(this.lastMapping[MappedAction.MapZoomIn]);

  public FezButtonState MapZoomOut => this.GetKeyState(this.lastMapping[MappedAction.MapZoomOut]);

  public FezButtonState OpenInventory
  {
    get => this.GetKeyState(this.lastMapping[MappedAction.OpenInventory]);
  }

  public FezButtonState RotateLeft => this.GetKeyState(this.lastMapping[MappedAction.RotateLeft]);

  public FezButtonState RotateRight => this.GetKeyState(this.lastMapping[MappedAction.RotateRight]);

  public FezButtonState FpViewToggle
  {
    get => this.GetKeyState(this.lastMapping[MappedAction.FpViewToggle]);
  }

  public FezButtonState ClampLook => this.GetKeyState(this.lastMapping[MappedAction.ClampLook]);

  public void Update(KeyboardState state, GameTime time)
  {
    KeyboardState state1 = Keyboard.GetState();
    this.activeKeys.Clear();
    lock (this)
      this.activeKeys.AddRange((IEnumerable<Keys>) this.registeredKeys);
    foreach (Keys activeKey in this.activeKeys)
    {
      bool pressed = state1.IsKeyDown(activeKey);
      FezButtonState state2;
      if (this.keyStates.TryGetValue(activeKey, out state2))
      {
        if (pressed || state2 != FezButtonState.Up)
        {
          FezButtonState fezButtonState = state2.NextState(pressed);
          if (fezButtonState != state2)
          {
            this.keyStates.Remove(activeKey);
            this.keyStates.Add(activeKey, fezButtonState);
          }
        }
      }
      else
        this.keyStates.Add(activeKey, state2.NextState(pressed));
    }
    bool flag = state.IsKeyDown(Keys.Enter);
    if (state.IsKeyDown(Keys.LeftAlt) & flag)
    {
      if (this.keyStates.ContainsKey(Keys.Enter))
        this.keyStates[Keys.Enter] = FezButtonState.Up;
      if (!this.enterDown)
      {
        SettingsManager.DeviceManager.ToggleFullScreen();
        SettingsManager.Settings.ScreenMode = SettingsManager.DeviceManager.IsFullScreen ? ScreenMode.Fullscreen : ScreenMode.Windowed;
      }
    }
    this.enterDown = flag;
  }
}
