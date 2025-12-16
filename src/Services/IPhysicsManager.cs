// Decompiled with JetBrains decompiler
// Type: FezGame.Services.IPhysicsManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezGame.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Services;

public interface IPhysicsManager
{
  void DetermineOverlaps(IComplexPhysicsEntity entity);

  void DetermineOverlaps(ISimplePhysicsEntity entity);

  bool DetermineInBackground(
    IPhysicsEntity entity,
    bool allowEnterInBackground,
    bool viewpointChanged,
    bool keepInFront);

  bool Update(ISimplePhysicsEntity entity);

  bool Update(ISimplePhysicsEntity entity, bool simple, bool keepInFront);

  bool Update(IComplexPhysicsEntity entity);

  void ClampToGround(IPhysicsEntity entity, Vector3? distance, Viewpoint viewpoint);

  PhysicsManager.WallHuggingResult HugWalls(
    IPhysicsEntity entity,
    bool determineBackground,
    bool postRotation,
    bool keepInFront);
}
