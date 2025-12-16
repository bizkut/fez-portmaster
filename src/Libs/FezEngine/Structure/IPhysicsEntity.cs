// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.IPhysicsEntity
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public interface IPhysicsEntity
{
  MultipleHits<TrileInstance> Ground { get; set; }

  bool Grounded { get; }

  bool Sliding { get; }

  Vector3 Velocity { get; set; }

  Vector3 GroundMovement { get; set; }

  Vector3 Center { get; set; }

  Vector3 Size { get; }

  PointCollision[] CornerCollision { get; }

  MultipleHits<CollisionResult> WallCollision { get; set; }

  bool Background { get; set; }

  float Elasticity { get; }

  bool NoVelocityClamping { get; }
}
