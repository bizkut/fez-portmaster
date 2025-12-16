// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.TimeService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

public class TimeService : ITimeService, IScriptingBase
{
  public LongRunningAction SetHour(int hour, bool immediate)
  {
    DateTime dateTime = new DateTime(1985, 12, 23, hour, 0, 0);
    if (immediate)
    {
      this.TimeManager.CurrentTime = dateTime;
      return (LongRunningAction) null;
    }
    long ticks = this.TimeManager.CurrentTime.Ticks;
    long destinationTicks = dateTime.Ticks;
    while (ticks - destinationTicks > 432000000000L)
      destinationTicks += 864000000000L;
    while (destinationTicks - ticks > 432000000000L)
      ticks += 864000000000L;
    int direction = Math.Sign(destinationTicks - ticks);
    destinationTicks -= (long) direction * 36000000000L / 2L;
    return new LongRunningAction((Func<float, float, bool>) ((elapsedSeconds, totalSeconds) =>
    {
      int num = direction != Math.Sign(destinationTicks - this.TimeManager.CurrentTime.Ticks) ? 1 : 0;
      if (num != 0)
        this.TimeManager.TimeFactor = MathHelper.Lerp(this.TimeManager.TimeFactor, this.TimeManager.DefaultTimeFactor, elapsedSeconds);
      else if ((double) totalSeconds < 1.0)
        this.TimeManager.TimeFactor = (float) ((double) this.TimeManager.DefaultTimeFactor * (double) Easing.EaseIn((double) FezMath.Saturate(totalSeconds), EasingType.Quadratic) * 100.0) * (float) direction;
      return num != 0 && FezMath.AlmostEqual(this.TimeManager.TimeFactor, 360f);
    }), (Action) (() => this.TimeManager.TimeFactor = this.TimeManager.DefaultTimeFactor));
  }

  public void SetTimeFactor(int factor) => this.TimeManager.TimeFactor = (float) factor;

  public LongRunningAction IncrementTimeFactor(float secondsUntilDouble)
  {
    return new LongRunningAction((Func<float, float, bool>) ((elapsedSeconds, _) =>
    {
      this.TimeManager.TimeFactor = FezMath.DoubleIter(this.TimeManager.TimeFactor, elapsedSeconds, secondsUntilDouble);
      return false;
    }));
  }

  public int Hour => this.TimeManager.CurrentTime.Hour;

  public void ResetEvents()
  {
  }

  [ServiceDependency]
  public ITimeManager TimeManager { private get; set; }
}
