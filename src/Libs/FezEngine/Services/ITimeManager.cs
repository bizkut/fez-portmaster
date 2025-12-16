// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.ITimeManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public interface ITimeManager
{
  void OnTick();

  event Action Tick;

  DateTime CurrentTime { get; set; }

  float TimeFactor { get; set; }

  float DayFraction { get; }

  float NightContribution { get; }

  float DawnContribution { get; }

  float DuskContribution { get; }

  float CurrentAmbientFactor { get; set; }

  Color CurrentFogColor { get; set; }

  float DefaultTimeFactor { get; }

  bool IsDayPhase(DayPhase phase);

  bool IsDayPhaseForMusic(DayPhase phase);

  float DayPhaseFraction(DayPhase phase);
}
