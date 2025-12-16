// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TrixelParticleSystems
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Threading;

#nullable disable
namespace FezGame.Components;

public class TrixelParticleSystems(Game game) : GameComponent(game), ITrixelParticleSystems
{
  private const int LimitBeforeMultithread = 2;
  private Worker<MtUpdateContext<List<TrixelParticleSystem>>> otherThread;
  private readonly List<TrixelParticleSystem> OtherDeadParticleSystems = new List<TrixelParticleSystem>();
  private readonly List<TrixelParticleSystem> ActiveParticleSystems = new List<TrixelParticleSystem>();
  private readonly List<TrixelParticleSystem> DeadParticleSystems = new List<TrixelParticleSystem>();

  public override void Initialize()
  {
    this.LevelManager.LevelChanged += (Action) (() =>
    {
      foreach (TrixelParticleSystem activeParticleSystem in this.ActiveParticleSystems)
        ServiceHelper.RemoveComponent<TrixelParticleSystem>(activeParticleSystem);
      this.ActiveParticleSystems.Clear();
    });
    this.otherThread = this.ThreadPool.Take<MtUpdateContext<List<TrixelParticleSystem>>>(new Action<MtUpdateContext<List<TrixelParticleSystem>>>(this.UpdateParticleSystems));
    this.otherThread.Priority = ThreadPriority.Normal;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || !this.CameraManager.Viewpoint.IsOrthographic() || !this.CameraManager.ActionRunning)
      return;
    TimeSpan elapsedGameTime = gameTime.ElapsedGameTime;
    int count = this.ActiveParticleSystems.Count;
    if (count >= 2)
    {
      this.otherThread.Start(new MtUpdateContext<List<TrixelParticleSystem>>()
      {
        Elapsed = elapsedGameTime,
        StartIndex = 0,
        EndIndex = count / 2,
        Result = this.OtherDeadParticleSystems
      });
      this.UpdateParticleSystems(new MtUpdateContext<List<TrixelParticleSystem>>()
      {
        Elapsed = elapsedGameTime,
        StartIndex = count / 2,
        EndIndex = count,
        Result = this.DeadParticleSystems
      });
      this.otherThread.Join();
    }
    else
      this.UpdateParticleSystems(new MtUpdateContext<List<TrixelParticleSystem>>()
      {
        Elapsed = elapsedGameTime,
        StartIndex = 0,
        EndIndex = count,
        Result = this.DeadParticleSystems
      });
    if (this.OtherDeadParticleSystems.Count > 0)
    {
      this.DeadParticleSystems.AddRange((IEnumerable<TrixelParticleSystem>) this.OtherDeadParticleSystems);
      this.OtherDeadParticleSystems.Clear();
    }
    if (this.DeadParticleSystems.Count <= 0)
      return;
    foreach (TrixelParticleSystem deadParticleSystem in this.DeadParticleSystems)
    {
      this.ActiveParticleSystems.Remove(deadParticleSystem);
      ServiceHelper.RemoveComponent<TrixelParticleSystem>(deadParticleSystem);
    }
    this.DeadParticleSystems.Clear();
  }

  private void UpdateParticleSystems(
    MtUpdateContext<List<TrixelParticleSystem>> context)
  {
    for (int startIndex = context.StartIndex; startIndex < context.EndIndex; ++startIndex)
    {
      if (startIndex < this.ActiveParticleSystems.Count)
      {
        this.ActiveParticleSystems[startIndex].Update(context.Elapsed);
        if (this.ActiveParticleSystems[startIndex].Dead)
          context.Result.Add(this.ActiveParticleSystems[startIndex]);
      }
    }
  }

  public void Add(TrixelParticleSystem system)
  {
    ServiceHelper.AddComponent((IGameComponent) system);
    this.ActiveParticleSystems.Add(system);
  }

  public void PropagateEnergy(Vector3 energySource, float energy)
  {
    foreach (TrixelParticleSystem activeParticleSystem in this.ActiveParticleSystems)
      activeParticleSystem.AddImpulse(energySource, energy);
  }

  protected override void Dispose(bool disposing)
  {
    this.ThreadPool.Return<MtUpdateContext<List<TrixelParticleSystem>>>(this.otherThread);
  }

  public void UnGroundAll()
  {
    foreach (TrixelParticleSystem activeParticleSystem in this.ActiveParticleSystems)
      activeParticleSystem.UnGround();
  }

  public int Count => this.ActiveParticleSystems.Count;

  public void ForceDraw()
  {
    foreach (TrixelParticleSystem activeParticleSystem in this.ActiveParticleSystems)
      activeParticleSystem.DoDraw();
  }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IThreadPool ThreadPool { private get; set; }
}
