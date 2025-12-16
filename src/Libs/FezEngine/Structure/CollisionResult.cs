// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.CollisionResult
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public struct CollisionResult
{
  public bool Collided;
  public bool ShouldBeClamped;
  public Vector3 Response;
  public Vector3 NearestDistance;
  public TrileInstance Destination;

  public override string ToString()
  {
    return $"{{{this.Collided} @ {(this.Destination == null ? (object) "none" : (object) this.Destination.ToString())}}}";
  }
}
