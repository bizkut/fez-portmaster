// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.ThreadExecutionState
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using System;
using System.Runtime.InteropServices;

#nullable disable
namespace FezGame.Tools;

internal static class ThreadExecutionState
{
  private static bool screenSaverWasEnabled;

  [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
  private static extern ThreadExecutionState.EXECUTION_STATE SetThreadExecutionState(
    ThreadExecutionState.EXECUTION_STATE esFlags);

  [DllImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool SystemParametersInfo(
    ThreadExecutionState.SPI uiAction,
    uint uiParam,
    ref uint pvParam,
    ThreadExecutionState.SPIF fWinIni);

  [DllImport("user32.dll", SetLastError = true)]
  [return: MarshalAs(UnmanagedType.Bool)]
  private static extern bool SystemParametersInfo(
    ThreadExecutionState.SPI uiAction,
    uint uiParam,
    uint pvParam,
    ThreadExecutionState.SPIF fWinIni);

  public static void SetUp()
  {
    int num = (int) ThreadExecutionState.SetThreadExecutionState(ThreadExecutionState.EXECUTION_STATE.ES_CONTINUOUS | ThreadExecutionState.EXECUTION_STATE.ES_DISPLAY_REQUIRED);
    uint pvParam = 0;
    if (ThreadExecutionState.SystemParametersInfo(ThreadExecutionState.SPI.SPI_GETSCREENSAVEACTIVE, 0U, ref pvParam, ThreadExecutionState.SPIF.None))
      ThreadExecutionState.screenSaverWasEnabled = pvParam == 1U;
    if (!ThreadExecutionState.screenSaverWasEnabled)
      return;
    ThreadExecutionState.SystemParametersInfo(ThreadExecutionState.SPI.SPI_SETSCREENSAVEACTIVE, 0U, 0U, ThreadExecutionState.SPIF.None);
  }

  public static void TearDown()
  {
    if (!ThreadExecutionState.screenSaverWasEnabled)
      return;
    ThreadExecutionState.SystemParametersInfo(ThreadExecutionState.SPI.SPI_SETSCREENSAVEACTIVE, 1U, 0U, ThreadExecutionState.SPIF.None);
  }

  [Flags]
  public enum EXECUTION_STATE : uint
  {
    ES_AWAYMODE_REQUIRED = 64, // 0x00000040
    ES_CONTINUOUS = 2147483648, // 0x80000000
    ES_DISPLAY_REQUIRED = 2,
    ES_SYSTEM_REQUIRED = 1,
  }

  public enum SPI : uint
  {
    SPI_GETSCREENSAVEACTIVE = 16, // 0x00000010
    SPI_SETSCREENSAVEACTIVE = 17, // 0x00000011
  }

  public enum SPIF : uint
  {
    None = 0,
    SPIF_UPDATEINIFILE = 1,
    SPIF_SENDCHANGE = 2,
    SPIF_SENDWININICHANGE = 2,
  }
}
