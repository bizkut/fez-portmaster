// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Scripting.ActiveScript
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Components;
using FezEngine.Components.Scripting;
using FezEngine.Services;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components.Scripting;

internal class ActiveScript : IDisposable
{
  private readonly List<LongRunningAction> runningActions = new List<LongRunningAction>();
  private readonly Queue<RunnableAction> queuedActions = new Queue<RunnableAction>();
  public readonly ScriptTrigger InitiatingTrigger;
  public readonly Script Script;
  private TimeSpan RunningTime;

  public event Action Disposed = new Action(Util.NullAction);

  public bool IsDisposed { get; private set; }

  public ActiveScript(Script script, ScriptTrigger initiatingTrigger)
  {
    ServiceHelper.InjectServices((object) this);
    this.Script = script;
    this.InitiatingTrigger = initiatingTrigger;
  }

  public void EnqueueAction(RunnableAction runnableAction)
  {
    this.queuedActions.Enqueue(runnableAction);
  }

  public void Update(TimeSpan elapsed)
  {
    if (this.IsDisposed)
      return;
    this.RunningTime += elapsed;
    while (this.queuedActions.Count > 0 && (this.runningActions.Count == 0 || !this.queuedActions.Peek().Action.Blocking))
      this.StartAction(this.queuedActions.Dequeue());
    if (this.runningActions.Count != 0)
    {
      TimeSpan? timeout = this.Script.Timeout;
      if (!timeout.HasValue)
        return;
      TimeSpan runningTime = this.RunningTime;
      timeout = this.Script.Timeout;
      if ((timeout.HasValue ? (runningTime > timeout.GetValueOrDefault() ? 1 : 0) : 0) == 0)
        return;
    }
    this.Dispose();
  }

  private void StartAction(RunnableAction runnableAction)
  {
    LongRunningAction runningAction = runnableAction.Invocation() as LongRunningAction;
    runnableAction.Invocation = (Func<object>) null;
    if (runningAction == null)
      return;
    runningAction.Ended += (Action) (() =>
    {
      this.runningActions.Remove(runningAction);
      if (!runnableAction.Action.Killswitch)
        return;
      this.Dispose();
    });
    this.runningActions.Add(runningAction);
  }

  public void Dispose()
  {
    foreach (LongRunningAction component in this.runningActions.ToArray())
      ServiceHelper.RemoveComponent<LongRunningAction>(component);
    this.IsDisposed = true;
    this.Disposed();
  }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IScriptingManager Scripting { private get; set; }
}
