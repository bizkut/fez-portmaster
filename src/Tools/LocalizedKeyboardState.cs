// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.LocalizedKeyboardState
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Microsoft.Xna.Framework.Input;
using System;
using System.Runtime.InteropServices;

#nullable disable
namespace FezGame.Tools;

public struct LocalizedKeyboardState
{
  internal const uint KLF_NOTELLSHELL = 128 /*0x80*/;
  public readonly KeyboardState Native;

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  internal static extern uint MapVirtualKeyEx(
    uint key,
    LocalizedKeyboardState.MAPVK mappingType,
    IntPtr keyboardLayout);

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  internal static extern IntPtr LoadKeyboardLayout(string keyboardLayoutID, uint flags);

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  internal static extern bool UnloadKeyboardLayout(IntPtr handle);

  [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  internal static extern IntPtr GetKeyboardLayout(IntPtr threadId);

  public static Keys USEnglishToLocal(Keys key)
  {
    return (Keys) LocalizedKeyboardState.MapVirtualKeyEx(LocalizedKeyboardState.MapVirtualKeyEx((uint) key, LocalizedKeyboardState.MAPVK.VK_TO_VSC, LocalizedKeyboardState.KeyboardLayout.US_English.Handle), LocalizedKeyboardState.MAPVK.VSC_TO_VK, LocalizedKeyboardState.KeyboardLayout.Active.Handle);
  }

  public class KeyboardLayout : IDisposable
  {
    public static LocalizedKeyboardState.KeyboardLayout US_English = new LocalizedKeyboardState.KeyboardLayout("00000409");
    public readonly IntPtr Handle;

    public KeyboardLayout(IntPtr handle) => this.Handle = handle;

    public KeyboardLayout(string keyboardLayoutID)
      : this(LocalizedKeyboardState.LoadKeyboardLayout(keyboardLayoutID, 128U /*0x80*/))
    {
    }

    public bool IsDisposed { get; private set; }

    public static LocalizedKeyboardState.KeyboardLayout Active
    {
      get
      {
        return new LocalizedKeyboardState.KeyboardLayout(LocalizedKeyboardState.GetKeyboardLayout(IntPtr.Zero));
      }
    }

    public void Dispose()
    {
      this.Dispose(true);
      GC.SuppressFinalize((object) this);
    }

    private void Dispose(bool disposing)
    {
      if (this.IsDisposed)
        return;
      LocalizedKeyboardState.UnloadKeyboardLayout(this.Handle);
      this.IsDisposed = true;
    }

    ~KeyboardLayout() => this.Dispose(false);
  }

  internal enum MAPVK : uint
  {
    VK_TO_VSC,
    VSC_TO_VK,
    VK_TO_CHAR,
  }
}
