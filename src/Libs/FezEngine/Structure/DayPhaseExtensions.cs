// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.DayPhaseExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Structure;

public static class DayPhaseExtensions
{
  public static float StartTime(this DayPhase phase)
  {
    switch (phase)
    {
      case DayPhase.Night:
        return 0.8333333f;
      case DayPhase.Dawn:
        return 0.0833333358f;
      case DayPhase.Day:
        return 0.208333328f;
      case DayPhase.Dusk:
        return 0.75f;
      default:
        throw new InvalidOperationException();
    }
  }

  public static float EndTime(this DayPhase phase)
  {
    switch (phase)
    {
      case DayPhase.Night:
        return 0.166666672f;
      case DayPhase.Dawn:
        return 0.25f;
      case DayPhase.Day:
        return 0.8333333f;
      case DayPhase.Dusk:
        return 0.9166667f;
      default:
        throw new InvalidOperationException();
    }
  }

  public static float MusicStartTime(this DayPhase phase)
  {
    switch (phase)
    {
      case DayPhase.Night:
        return 0.875f;
      case DayPhase.Dawn:
        return 0.0833333358f;
      case DayPhase.Day:
        return 0.208333328f;
      case DayPhase.Dusk:
        return 0.7916667f;
      default:
        throw new InvalidOperationException();
    }
  }

  public static float MusicEndTime(this DayPhase phase)
  {
    switch (phase)
    {
      case DayPhase.Night:
        return 0.0833333358f;
      case DayPhase.Dawn:
        return 0.208333328f;
      case DayPhase.Day:
        return 0.7916667f;
      case DayPhase.Dusk:
        return 0.875f;
      default:
        throw new InvalidOperationException();
    }
  }

  public static float Duration(this DayPhase phase)
  {
    float num1 = phase.EndTime();
    float num2 = phase.StartTime();
    if ((double) num1 < (double) num2)
      ++num1;
    return num1 - num2;
  }
}
