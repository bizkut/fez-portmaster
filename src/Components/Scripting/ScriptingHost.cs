// Decompiled with JetBrains decompiler
// Type: FezGame.Components.Scripting.ScriptingHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure.Scripting;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components.Scripting;

internal class ScriptingHost : GameComponent, IScriptingManager
{
  private Script[] levelScripts = new Script[0];
  private readonly Dictionary<string, IScriptingBase> services = new Dictionary<string, IScriptingBase>();
  private readonly List<ActiveScript> activeScripts = new List<ActiveScript>();
  public static bool ScriptExecuted;
  private string LastLevel;
  public static ScriptingHost Instance;

  public event Action CutsceneSkipped;

  public void OnCutsceneSkipped()
  {
    if (this.CutsceneSkipped == null)
      return;
    this.CutsceneSkipped();
  }

  public ActiveScript EvaluatedScript { get; private set; }

  public ScriptingHost(Game game)
    : base(game)
  {
    ScriptingHost.Instance = this;
  }

  public override void Initialize()
  {
    base.Initialize();
    foreach (KeyValuePair<string, EntityTypeDescriptor> type in (IEnumerable<KeyValuePair<string, EntityTypeDescriptor>>) EntityTypes.Types)
      this.services.Add(type.Key, ServiceHelper.Get(type.Value.Interface) as IScriptingBase);
    this.LevelManager.LevelChanged += new Action(this.PrepareScripts);
    this.PrepareScripts();
  }

  private void PrepareScripts()
  {
    this.levelScripts = this.LevelManager.Scripts.Values.ToArray<Script>();
    foreach (ActiveScript activeScript in this.activeScripts)
    {
      activeScript.Dispose();
      if (activeScript.Script.OneTime)
      {
        activeScript.Script.Disabled = true;
        LevelSaveData levelSaveData;
        if (!activeScript.Script.LevelWideOneTime && this.GameState.SaveData.World.TryGetValue(this.LastLevel, out levelSaveData))
          levelSaveData.InactiveEvents.Add(activeScript.Script.Id);
      }
    }
    this.activeScripts.Clear();
    foreach (IScriptingBase scriptingBase in this.services.Values)
      scriptingBase.ResetEvents();
    if (this.LevelManager.Name != null)
    {
      foreach (int inactiveEvent in this.GameState.SaveData.ThisLevel.InactiveEvents)
      {
        Script script;
        if (this.LevelManager.Scripts.TryGetValue(inactiveEvent, out script) && (!(this.LevelManager.Name == "WATERFALL") || inactiveEvent != 9 || this.GameState.SaveData.ThisLevel.InactiveVolumes.Contains(20)))
          script.Disabled = true;
      }
    }
    this.LastLevel = this.LevelManager.Name;
    foreach (Script levelScript in this.levelScripts)
      this.HookScriptTriggers(levelScript);
  }

  private void HookScriptTriggers(Script script)
  {
    foreach (ScriptTrigger trigger in script.Triggers)
    {
      ScriptTrigger triggerCopy = trigger;
      EntityTypeDescriptor type = EntityTypes.Types[trigger.Object.Type];
      EventDescriptor eventDescriptor = type.Events[trigger.Event];
      if (type.Static)
      {
        Action action = (Action) (() => this.ProcessTrigger(triggerCopy, script));
        object obj = eventDescriptor.AddHandler((object) this.services[trigger.Object.Type], (object) action);
      }
      else
      {
        Action<int> action = (Action<int>) (id => this.ProcessTrigger(triggerCopy, script, new int?(id)));
        object obj = eventDescriptor.AddHandler((object) this.services[trigger.Object.Type], (object) action);
      }
    }
  }

  private void ProcessTrigger(ScriptTrigger trigger, Script script)
  {
    this.ProcessTrigger(trigger, script, new int?());
  }

  private void ProcessTrigger(ScriptTrigger trigger, Script script, int? id)
  {
    if (this.GameState.Loading && trigger.Object.Type != "Level" && trigger.Event != "Start" || script.Disabled)
      return;
    int? nullable = id;
    int? identifier = trigger.Object.Identifier;
    if ((nullable.GetValueOrDefault() == identifier.GetValueOrDefault() ? (nullable.HasValue != identifier.HasValue ? 1 : 0) : 1) != 0 || script.Conditions != null && script.Conditions.Any<ScriptCondition>((Func<ScriptCondition, bool>) (c => !c.Check(this.services[c.Object.Type]))) || script.OneTime && this.activeScripts.Any<ActiveScript>((Func<ActiveScript, bool>) (x => x.Script == script)))
      return;
    ActiveScript activeScript = new ActiveScript(script, trigger);
    this.activeScripts.Add(activeScript);
    if (script.IsWinCondition && !this.GameState.SaveData.ThisLevel.FilledConditions.ScriptIds.Contains(script.Id))
    {
      this.GameState.SaveData.ThisLevel.FilledConditions.ScriptIds.Add(script.Id);
      this.GameState.SaveData.ThisLevel.FilledConditions.ScriptIds.Sort();
    }
    foreach (ScriptAction action in script.Actions)
    {
      RunnableAction runnableAction = new RunnableAction()
      {
        Action = action
      };
      runnableAction.Invocation = (Func<object>) (() => runnableAction.Action.Invoke((object) this.services[runnableAction.Action.Object.Type], runnableAction.Action.ProcessedArguments));
      activeScript.EnqueueAction(runnableAction);
    }
    if (!script.IgnoreEndTriggers)
    {
      foreach (ScriptTrigger trigger1 in script.Triggers)
      {
        EntityTypeDescriptor type = EntityTypes.Types[trigger1.Object.Type];
        DynamicMethodDelegate endTriggerHandler = type.Events[trigger1.Event].AddEndTriggerHandler;
        if (endTriggerHandler != null)
        {
          if (type.Static)
          {
            Action action = (Action) (() => ScriptingHost.ProcessEndTrigger(trigger, activeScript));
            object obj = endTriggerHandler((object) this.services[trigger.Object.Type], (object) action);
          }
          else
          {
            Action<int> action = (Action<int>) (i => ScriptingHost.ProcessEndTrigger(trigger, activeScript, new int?(i)));
            object obj = endTriggerHandler((object) this.services[trigger.Object.Type], (object) action);
          }
        }
      }
    }
    activeScript.Disposed += (Action) (() => this.ScriptService.OnComplete(activeScript.Script.Id));
    ScriptingHost.ScriptExecuted = true;
  }

  private static void ProcessEndTrigger(ScriptTrigger trigger, ActiveScript script)
  {
    ScriptingHost.ProcessEndTrigger(trigger, script, new int?());
  }

  private static void ProcessEndTrigger(ScriptTrigger trigger, ActiveScript script, int? id)
  {
    int? nullable = id;
    int? identifier = trigger.Object.Identifier;
    if ((nullable.GetValueOrDefault() == identifier.GetValueOrDefault() ? (nullable.HasValue == identifier.HasValue ? 1 : 0) : 0) == 0)
      return;
    script.Dispose();
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.Loading || this.GameState.InCutscene || this.GameState.InFpsMode)
      return;
    this.ForceUpdate(gameTime);
  }

  public void ForceUpdate(GameTime gameTime)
  {
    foreach (Script levelScript in this.levelScripts)
    {
      if (levelScript.ScheduleEvalulation)
      {
        this.ProcessTrigger((ScriptTrigger) ScriptingHost.NullTrigger.Instance, levelScript);
        levelScript.ScheduleEvalulation = false;
      }
    }
    for (int index = this.activeScripts.Count - 1; index != -1; --index)
    {
      ActiveScript activeScript = this.activeScripts[index];
      this.EvaluatedScript = activeScript;
      activeScript.Update(gameTime.ElapsedGameTime);
      if (activeScript.IsDisposed && this.activeScripts.Count > 0 && this.activeScripts[index] == activeScript)
      {
        this.activeScripts.RemoveAt(index);
        if (activeScript.Script.OneTime)
        {
          activeScript.Script.Disabled = true;
          if (!activeScript.Script.LevelWideOneTime)
            this.GameState.SaveData.ThisLevel.InactiveEvents.Add(activeScript.Script.Id);
          this.GameState.Save();
        }
      }
    }
    this.EvaluatedScript = (ActiveScript) null;
  }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IScriptService ScriptService { private get; set; }

  private class NullTrigger : ScriptTrigger
  {
    public static readonly ScriptingHost.NullTrigger Instance = new ScriptingHost.NullTrigger();
    private const string NullEvent = "Null Event";

    private NullTrigger()
    {
      this.Event = "Null Event";
      this.Object = new Entity()
      {
        Type = (string) null,
        Identifier = new int?()
      };
    }
  }
}
