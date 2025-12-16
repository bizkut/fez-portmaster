// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.AnimationTiming
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Tools;

public class AnimationTiming
{
  public readonly float[] FrameTimings;
  public readonly int InitialFirstFrame;
  public readonly int InitialEndFrame;
  private readonly float stepPerFrame;
  private float startStep;
  private float endStep;
  private int startFrame;
  private int endFrame;

  public AnimationTiming(float[] frameTimings)
    : this(0, frameTimings.Length - 1, false, frameTimings)
  {
  }

  public AnimationTiming(int startFrame, float[] frameTimings)
    : this(startFrame, frameTimings.Length - 1, false, frameTimings)
  {
  }

  public AnimationTiming(int startFrame, int endFrame, float[] frameTimings)
    : this(startFrame, endFrame, false, frameTimings)
  {
  }

  public AnimationTiming(int startFrame, int endFrame, bool loop, float[] frameTimings)
  {
    this.Loop = loop;
    this.FrameTimings = ((IEnumerable<float>) frameTimings).Select<float, float>((Func<float, float>) (x => (double) x != 0.0 ? x : 0.1f)).ToArray<float>();
    this.stepPerFrame = 1f / (float) frameTimings.Length;
    this.InitialFirstFrame = this.StartFrame = startFrame;
    this.InitialEndFrame = this.EndFrame = endFrame;
  }

  public void Restart()
  {
    this.Step = this.startStep;
    this.Paused = false;
  }

  public void Update(TimeSpan elapsed) => this.Update(elapsed, 1f);

  public void Update(TimeSpan elapsed, float timeFactor)
  {
    if (this.Paused || this.Ended)
      return;
    int index = (int) Math.Floor((double) this.Step * (double) this.FrameTimings.Length);
    this.Step += (float) elapsed.TotalSeconds * timeFactor / this.FrameTimings[index] * this.stepPerFrame;
    while ((double) this.Step >= (double) this.endStep)
    {
      if (this.Loop)
        this.Step -= this.endStep - this.startStep;
      else
        this.Step = this.endStep - 1f / 1000f;
    }
    while ((double) this.Step < (double) this.startStep)
    {
      if (this.Loop)
        this.Step += (float) ((double) this.endStep - (double) this.startStep - 1.0 / 1000.0);
      else
        this.Step = this.startStep;
    }
  }

  public bool Loop { get; set; }

  public bool Paused { get; set; }

  public float Step { get; set; }

  public float NormalizedStep => (this.Step - this.startStep) / this.endStep;

  public void RandomizeStep()
  {
    this.Step = RandomHelper.Between((double) this.startStep, (double) this.endStep);
  }

  public int StartFrame
  {
    get => this.startFrame;
    set
    {
      this.startFrame = value;
      this.startStep = (float) this.startFrame / (float) this.FrameTimings.Length;
    }
  }

  public int EndFrame
  {
    get => this.endFrame;
    set
    {
      this.endFrame = value;
      this.endStep = (float) (this.endFrame + 1) / (float) this.FrameTimings.Length;
    }
  }

  public float StartStep => this.startStep;

  public float EndStep => this.endStep;

  public bool Ended => !this.Loop && FezMath.AlmostEqual(this.Step, this.endStep);

  public int Frame
  {
    get => (int) Math.Floor((double) this.Step * (double) this.FrameTimings.Length);
    set => this.Step = (float) value / (float) this.FrameTimings.Length;
  }

  public float NextFrameContribution => FezMath.Frac(this.Step * (float) this.FrameTimings.Length);

  public AnimationTiming Clone()
  {
    return new AnimationTiming(this.StartFrame, this.EndFrame, this.Loop, this.FrameTimings);
  }
}
