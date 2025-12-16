// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.BoxCollisionResultExtensions
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;

#nullable disable
namespace FezGame.Structure;

public static class BoxCollisionResultExtensions
{
  public static CollisionResult First(this MultipleHits<CollisionResult> result)
  {
    if (result.NearLow.Collided)
      return result.NearLow;
    return !result.FarHigh.Collided ? new CollisionResult() : result.FarHigh;
  }

  public static bool AnyCollided(this MultipleHits<CollisionResult> result)
  {
    return result.NearLow.Collided || result.FarHigh.Collided;
  }

  public static bool AnyHit(this MultipleHits<CollisionResult> result)
  {
    return result.NearLow.Destination != null || result.FarHigh.Destination != null;
  }
}
