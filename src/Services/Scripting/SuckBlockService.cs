// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.SuckBlockService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Services.Scripting;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Services.Scripting;

internal class SuckBlockService : ISuckBlockService, IScriptingBase
{
  private readonly Dictionary<int, bool> SuckState = new Dictionary<int, bool>();

  public void ResetEvents()
  {
    this.Sucked = new Action<int>(Util.NullAction<int>);
    this.SuckState.Clear();
  }

  public event Action<int> Sucked;

  public void OnSuck(int id)
  {
    this.SuckState[id] = true;
    this.Sucked(id);
  }

  public bool get_IsSucked(int id)
  {
    if (this.SuckState.Count == 0)
      return true;
    bool flag;
    return this.SuckState.TryGetValue(id, out flag) && flag;
  }
}
