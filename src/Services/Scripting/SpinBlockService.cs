// Decompiled with JetBrains decompiler
// Type: FezGame.Services.Scripting.SpinBlockService
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Tools;

#nullable disable
namespace FezGame.Services.Scripting;

public class SpinBlockService : ISpinBlockService, IScriptingBase
{
  public void ResetEvents()
  {
  }

  public void SetEnabled(int id, bool enabled)
  {
    this.LevelManager.ArtObjects[id].ActorSettings.Inactive = !enabled;
  }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }
}
