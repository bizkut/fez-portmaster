// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.TimeManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public class TimeManager : ITimeManager
{
  private const float TransitionDivider = 3f;
  public static DateTime InitialTime = DateTime.Today.AddHours(12.0);

  public TimeManager()
  {
    this.TimeFactor = this.DefaultTimeFactor;
    this.CurrentTime = TimeManager.InitialTime;
    this.Tick += (Action) (() =>
    {
      this.DawnContribution = TimeManager.Ease(this.DayFraction, DayPhase.Dawn.StartTime(), DayPhase.Dawn.Duration());
      this.DuskContribution = TimeManager.Ease(this.DayFraction, DayPhase.Dusk.StartTime(), DayPhase.Dusk.Duration());
      this.NightContribution = TimeManager.Ease(this.DayFraction, DayPhase.Night.StartTime(), DayPhase.Night.Duration());
      this.NightContribution = Math.Max(this.NightContribution, TimeManager.Ease(this.DayFraction, DayPhase.Night.StartTime() - 1f, DayPhase.Night.Duration()));
    });
  }

  private static float Ease(float value, float start, float duration)
  {
    float num1 = value - start;
    float num2 = duration / 3f;
    if ((double) num1 < (double) num2)
      return FezMath.Saturate(num1 / num2);
    if ((double) num1 > 2.0 * (double) num2)
      return 1f - FezMath.Saturate((num1 - 2f * num2) / num2);
    return (double) num1 < 0.0 || (double) num1 > (double) duration ? 0.0f : 1f;
  }

  public void OnTick() => this.Tick();

  public event Action Tick = new Action(Util.NullAction);

  public DateTime CurrentTime { get; set; }

  public float DefaultTimeFactor => 260f;

  public bool IsDayPhase(DayPhase phase)
  {
    float dayFraction = this.DayFraction;
    float num1 = phase.StartTime();
    float num2 = phase.EndTime();
    return (double) num1 < (double) num2 ? (double) dayFraction >= (double) num1 && (double) dayFraction <= (double) num2 : (double) dayFraction >= (double) num1 || (double) dayFraction <= (double) num2;
  }

  public bool IsDayPhaseForMusic(DayPhase phase)
  {
    float dayFraction = this.DayFraction;
    float num1 = phase.MusicStartTime();
    float num2 = phase.MusicEndTime();
    return (double) num1 < (double) num2 ? (double) dayFraction >= (double) num1 && (double) dayFraction <= (double) num2 : (double) dayFraction >= (double) num1 || (double) dayFraction <= (double) num2;
  }

  public float DayPhaseFraction(DayPhase phase)
  {
    float num = this.DayFraction - phase.StartTime();
    if ((double) num < 1.0)
      ++num;
    return num / phase.Duration();
  }

  public float DayFraction => (float) this.CurrentTime.TimeOfDay.TotalDays;

  public float TimeFactor { get; set; }

  public float NightContribution { get; private set; }

  public float DawnContribution { get; private set; }

  public float DuskContribution { get; private set; }

  public float CurrentAmbientFactor { get; set; }

  public Color CurrentFogColor { get; set; }
}
