// Decompiled with JetBrains decompiler
// Type: FezGame.Services.IGameCameraManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Services;

public interface IGameCameraManager : IDefaultCameraManager, ICameraProvider
{
  Viewpoint RequestedViewpoint { get; set; }

  void RecordNewCarriedInstancePhi();

  void CancelViewTransition();

  Vector3 OriginalDirection { get; set; }
}
