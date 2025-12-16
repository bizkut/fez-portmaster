// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.Sequencer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

#nullable disable
namespace FezEngine.Components;

public abstract class Sequencer(Game game) : GameComponent(game)
{
  private const float WarningTime = 0.45f;
  private const int WarningBlinks = 6;
  protected readonly Dictionary<TrileInstance, Sequencer.CrystalState> crystals = new Dictionary<TrileInstance, Sequencer.CrystalState>();
  private TimeSpan measureLength;
  private int step;

  public override void Initialize()
  {
    this.Enabled = false;
    this.LevelManager.LevelChanged += new Action(this.TryStartSequence);
    this.TryStartSequence();
  }

  private void TryStartSequence()
  {
    this.crystals.Clear();
    this.Enabled = this.LevelManager.SequenceSamplesPath != null && this.LevelManager.Song != null && this.LevelManager.BlinkingAlpha;
    if (!this.Enabled)
      return;
    foreach (TrileInstance key in this.LevelManager.Triles.Values.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Trile.ActorSettings.Type == ActorType.Crystal)))
      this.crystals.Add(key, new Sequencer.CrystalState());
    this.StartSequence();
  }

  protected void StartSequence()
  {
    this.LoadCrystalSamples();
    this.UpdateTempo();
  }

  protected void UpdateTempo()
  {
    this.measureLength = TimeSpan.FromMinutes(1.0 / ((double) this.LevelManager.Song.Tempo / 4.0) * 4.0);
  }

  protected void LoadCrystalSamples()
  {
    ContentManager forLevel = this.CMProvider.GetForLevel(this.LevelManager.Name);
    string path1 = Path.Combine("Sounds", this.LevelManager.SequenceSamplesPath ?? "");
    foreach (TrileInstance key in this.crystals.Keys.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.ActorSettings.SequenceSampleName != null)))
    {
      Sequencer.CrystalState crystal = this.crystals[key];
      InstanceActorSettings actorSettings = key.ActorSettings;
      try
      {
        crystal.Sample = forLevel.Load<SoundEffect>(Path.Combine(path1, actorSettings.SequenceSampleName));
        if (actorSettings.SequenceAlternateSampleName != null)
          crystal.AlternateSample = forLevel.Load<SoundEffect>(Path.Combine(path1, actorSettings.SequenceAlternateSampleName));
      }
      catch (Exception ex)
      {
        Logger.Log(nameof (Sequencer), LogSeverity.Warning, $"Could not find crystal sample : {(object) crystal.Sample} or {(object) crystal.AlternateSample}");
      }
      bool flag1 = actorSettings.Sequence[15];
      bool flag2 = false;
      for (int index = 0; index < 16 /*0x10*/; ++index)
      {
        if (!flag1 && actorSettings.Sequence[index])
        {
          crystal.Alternate[index] = flag2 && crystal.AlternateSample != null;
          flag2 = !flag2;
        }
        flag1 = actorSettings.Sequence[index];
      }
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (!this.Enabled || this.EngineState.Loading || this.EngineState.Paused || this.EngineState.InMap)
      return;
    double num1 = FezMath.Frac((double) this.SoundManager.PlayPosition.Ticks / (double) this.measureLength.Ticks);
    this.step = (int) Math.Floor(num1 * 16.0);
    double num2 = FezMath.Frac(num1 * 16.0);
    int index = (this.step + 1) % 16 /*0x10*/;
    bool flag = false;
    foreach (TrileInstance key in this.crystals.Keys)
    {
      if (key.ActorSettings.Sequence != null)
      {
        Sequencer.CrystalState crystal = this.crystals[key];
        bool enabled = key.Enabled;
        key.Enabled = key.ActorSettings.Sequence.Length > this.step && key.ActorSettings.Sequence[this.step];
        if (!enabled && key.Enabled)
        {
          this.LevelManager.RestoreTrile(key);
          key.Hidden = false;
          key.Enabled = true;
          if (key.InstanceId == -1)
            this.LevelMaterializer.CullInstanceIn(key);
          if (crystal.Sample != null)
          {
            Vector3 position = key.Position + new Vector3(0.5f);
            (crystal.Alternate[this.step] ? crystal.AlternateSample : crystal.Sample).EmitAt(position);
          }
          flag = true;
        }
        else if (enabled && !key.Enabled)
        {
          this.LevelMaterializer.UnregisterViewedInstance(key);
          this.LevelMaterializer.CullInstanceOut(key, true);
          this.LevelManager.ClearTrile(key, true);
          this.OnDisappear(key);
          flag = true;
        }
        else if (num2 > 0.44999998807907104 && key.Enabled && key.ActorSettings.Sequence.Length > index && !key.ActorSettings.Sequence[index])
        {
          if ((int) Math.Round((num2 - 0.44999998807907104) / 0.550000011920929 * 6.0) % 3 == 0)
          {
            if (!key.Hidden)
            {
              key.Hidden = true;
              this.LevelMaterializer.UnregisterViewedInstance(key);
              this.LevelMaterializer.CullInstanceOut(key, true);
              flag = true;
            }
          }
          else if (key.Hidden)
          {
            key.Hidden = false;
            if (key.InstanceId == -1)
            {
              this.LevelMaterializer.CullInstanceIn(key);
              flag = true;
            }
          }
        }
      }
    }
    if (!flag)
      return;
    this.LevelMaterializer.CommitBatchesIfNeeded();
  }

  protected virtual void OnDisappear(TrileInstance crystal)
  {
  }

  [ServiceDependency]
  public IEngineStateManager EngineState { protected get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraManager { protected get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { protected get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { protected get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  protected class CrystalState
  {
    public SoundEffect Sample { get; set; }

    public SoundEffect AlternateSample { get; set; }

    public bool[] Alternate { get; private set; }

    public CrystalState() => this.Alternate = new bool[16 /*0x10*/];
  }
}
