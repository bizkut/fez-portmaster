// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.PatternTester
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure.Input;

public static class PatternTester
{
  public static bool Test(IList<CodeInput> input, CodeInput[] pattern)
  {
    int count = input.Count;
    bool flag = false;
    for (int index = 0; index < pattern.Length && index < count && input[count - index - 1] == pattern[pattern.Length - index - 1]; ++index)
    {
      if (index == pattern.Length - 1)
      {
        flag = true;
        input.Clear();
        break;
      }
    }
    return flag;
  }

  public static bool Test(IList<VibrationMotor> input, VibrationMotor[] pattern)
  {
    int count = input.Count;
    bool flag = false;
    int num = 0;
    for (int index = 0; index + num < pattern.Length && index < count; ++index)
    {
      while (pattern[pattern.Length - index - 1 - num] == VibrationMotor.None)
      {
        ++num;
        if (index + num >= pattern.Length)
          break;
      }
      if (input[count - index - 1] == pattern[pattern.Length - index - 1 - num])
      {
        if (index == pattern.Length - 1 - num)
        {
          flag = true;
          input.Clear();
          break;
        }
      }
      else
        break;
    }
    return flag;
  }
}
