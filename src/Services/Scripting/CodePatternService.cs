// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.CodePatternService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Services.Scripting;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

internal class CodePatternService : ICodePatternService, IScriptingBase
{
  public void ResetEvents() => this.Activated = new Action<int>(Util.NullAction<int>);

  public event Action<int> Activated;

  public void OnActivate(int id) => this.Activated(id);
}
