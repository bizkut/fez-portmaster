// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.ITimeService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Components.Scripting;
using FezEngine.Structure.Scripting;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Static = true)]
public interface ITimeService : IScriptingBase
{
  [Description("The hour of day (0-23)")]
  int Hour { get; }

  [Description("Changes the hour of day (0-23), gradually or immediately")]
  LongRunningAction SetHour(int hour, bool immediate);

  [Description("Sets the speed of time passage (0 = paused)")]
  void SetTimeFactor(int factor);

  [Description("Increments the time factor (specifying how much time before it doubles up)")]
  LongRunningAction IncrementTimeFactor(float secondsUntilDouble);
}
