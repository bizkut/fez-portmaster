// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.TimeswitchService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine.Services.Scripting;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

public class TimeswitchService : ITimeswitchService, IScriptingBase
{
  public event Action<int> ScrewedOut = new Action<int>(Util.NullAction<int>);

  public event Action<int> HitBase = new Action<int>(Util.NullAction<int>);

  public void ResetEvents()
  {
    this.ScrewedOut = new Action<int>(Util.NullAction<int>);
    this.HitBase = new Action<int>(Util.NullAction<int>);
  }

  public void OnScrewedOut(int id) => this.ScrewedOut(id);

  public void OnHitBase(int id) => this.HitBase(id);
}
