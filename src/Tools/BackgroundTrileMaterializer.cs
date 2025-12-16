// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.BackgroundTrileMaterializer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;

#nullable disable
namespace FezGame.Tools;

internal class BackgroundTrileMaterializer(Trile trile, Mesh levelMesh) : TrileMaterializer(trile, levelMesh)
{
  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }
}
