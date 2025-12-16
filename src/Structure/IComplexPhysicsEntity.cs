// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.IComplexPhysicsEntity
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Structure;

public interface IComplexPhysicsEntity : IPhysicsEntity
{
  bool MustBeClampedToGround { get; set; }

  Vector3? GroundedVelocity { get; set; }

  HorizontalDirection MovingDirection { get; set; }

  bool Climbing { get; }

  bool Swimming { get; }

  Dictionary<VerticalDirection, NearestTriles> AxisCollision { get; }

  MultipleHits<CollisionResult> Ceiling { get; set; }

  bool HandlesZClamping { get; }
}
