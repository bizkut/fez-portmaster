// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.TombstoneService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services.Scripting;
using System;

#nullable disable
namespace FezGame.Services.Scripting;

public class TombstoneService : ITombstoneService, IScriptingBase
{
  private int alignCount;

  public void ResetEvents() => this.MoreThanOneAligned = (Action) null;

  public event Action MoreThanOneAligned;

  public void OnMoreThanOneAligned()
  {
    if (this.MoreThanOneAligned == null)
      return;
    this.MoreThanOneAligned();
  }

  public int get_AlignedCount() => this.alignCount;

  public void UpdateAlignCount(int count) => this.alignCount = count;
}
