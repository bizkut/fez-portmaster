// Decompiled with JetBrains decompiler
// Type: Common.SoundInfo
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

using System;
using System.Runtime.InteropServices;
using System.Text;

#nullable disable
namespace Common;

public static class SoundInfo
{
  [DllImport("winmm.dll")]
  private static extern uint mciSendString(
    string command,
    StringBuilder returnValue,
    int returnLength,
    IntPtr winHandle);

  public static int GetSoundLength(string fileName)
  {
    StringBuilder returnValue = new StringBuilder(32 /*0x20*/);
    int num1 = (int) SoundInfo.mciSendString($"open \"{fileName}\" type waveaudio alias wave", (StringBuilder) null, 0, IntPtr.Zero);
    int num2 = (int) SoundInfo.mciSendString("status wave length", returnValue, returnValue.Capacity, IntPtr.Zero);
    int num3 = (int) SoundInfo.mciSendString("close wave", (StringBuilder) null, 0, IntPtr.Zero);
    int result;
    int.TryParse(returnValue.ToString(), out result);
    return result;
  }
}
