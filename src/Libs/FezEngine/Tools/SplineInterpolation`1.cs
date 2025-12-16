// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.SplineInterpolation`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Tools;

public abstract class SplineInterpolation<T>
{
  public static EasingType EaseInType = EasingType.None;
  public static EasingType EaseOutType = EasingType.Quadratic;
  public static bool LongScreenshot;
  private TimeSpan totalElapsed;
  private TimeSpan duration;
  private static readonly GameTime EmptyGameTime = new GameTime();

  protected SplineInterpolation(TimeSpan duration, params T[] points)
  {
    this.Paused = true;
    this.duration = duration;
    this.Points = points;
  }

  public void Start()
  {
    this.Paused = false;
    this.Update(SplineInterpolation<T>.EmptyGameTime);
  }

  public void Update(GameTime gameTime)
  {
    if (this.Reached || this.Paused)
      return;
    this.totalElapsed += gameTime.ElapsedGameTime;
    int max = this.Points.Length - 1;
    if (this.totalElapsed >= this.duration)
    {
      this.TotalStep = 1f;
      this.Current = this.Points[max];
      this.totalElapsed = this.duration;
      this.Reached = true;
      this.Paused = true;
    }
    else
    {
      float num1 = SplineInterpolation<T>.EaseInType != EasingType.None ? (SplineInterpolation<T>.EaseOutType != EasingType.None ? (SplineInterpolation<T>.EaseInType != SplineInterpolation<T>.EaseOutType ? Easing.EaseInOut((double) this.totalElapsed.Ticks / (double) this.duration.Ticks, SplineInterpolation<T>.EaseInType, SplineInterpolation<T>.EaseOutType) : Easing.EaseInOut((double) this.totalElapsed.Ticks / (double) this.duration.Ticks, SplineInterpolation<T>.EaseInType)) : Easing.EaseIn((double) this.totalElapsed.Ticks / (double) this.duration.Ticks, SplineInterpolation<T>.EaseInType)) : Easing.EaseOut((double) this.totalElapsed.Ticks / (double) this.duration.Ticks, SplineInterpolation<T>.EaseOutType);
      int index1 = (int) MathHelper.Clamp((float) ((double) max * (double) num1 - 1.0), 0.0f, (float) max);
      int index2 = (int) MathHelper.Clamp((float) max * num1, 0.0f, (float) max);
      int index3 = (int) MathHelper.Clamp((float) ((double) max * (double) num1 + 1.0), 0.0f, (float) max);
      int index4 = (int) MathHelper.Clamp((float) ((double) max * (double) num1 + 2.0), 0.0f, (float) max);
      double num2 = (double) index2 / (double) max;
      double num3 = (double) index3 / (double) max - num2;
      float t = (float) FezMath.Saturate(((double) num1 - num2) / (num3 == 0.0 ? 1.0 : num3));
      this.TotalStep = num1;
      this.Interpolate(this.Points[index1], this.Points[index2], this.Points[index3], this.Points[index4], t);
    }
  }

  public T[] Points { get; private set; }

  public T Current { get; protected set; }

  public bool Reached { get; private set; }

  public bool Paused { get; private set; }

  public float TotalStep { get; private set; }

  protected abstract void Interpolate(T p0, T p1, T p2, T p3, float t);
}
