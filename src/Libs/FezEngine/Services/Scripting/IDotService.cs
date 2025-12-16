// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.Scripting.IDotService
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Components.Scripting;
using FezEngine.Structure.Scripting;

#nullable disable
namespace FezEngine.Services.Scripting;

[Entity(Static = true)]
public interface IDotService : IScriptingBase
{
  [Description("Makes Dot say a custom text line")]
  LongRunningAction Say(string line, bool nearGomez, bool hideAfter);

  [Description("Hides Dot in Gomez's hat")]
  LongRunningAction ComeBackAndHide(bool withCamera);

  [Description("Spiral around the level, yo")]
  LongRunningAction SpiralAround(bool withCamera, bool hideDot);
}
