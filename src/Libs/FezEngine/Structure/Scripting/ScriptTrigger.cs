// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.ScriptTrigger
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Structure.Scripting;

public class ScriptTrigger : ScriptPart
{
  public string Event { get; set; }

  public override string ToString()
  {
    return $"{(this.Object == null ? "(none)" : this.Object.ToString())}.{this.Event ?? "(none)"}";
  }

  public ScriptTrigger Clone()
  {
    ScriptTrigger scriptTrigger = new ScriptTrigger();
    scriptTrigger.Event = this.Event;
    scriptTrigger.Object = this.Object.Clone();
    return scriptTrigger;
  }
}
