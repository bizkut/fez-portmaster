// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.InstancePhysicsState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

#nullable disable
namespace FezEngine.Structure;

public class InstancePhysicsState : ISimplePhysicsEntity, IPhysicsEntity
{
  private readonly TrileInstance instance;

  public bool Vanished { get; set; }

  public bool ShouldRespawn { get; set; }

  public bool Respawned { get; set; }

  public bool UpdatingPhysics { get; set; }

  public bool Sticky { get; set; }

  public bool Puppet { get; set; }

  public bool Paused { get; set; }

  public bool PushedUp { get; set; }

  public TrileInstance PushedDownBy { get; set; }

  public bool IgnoreCollision { get; set; }

  public bool ForceNonStatic { get; set; }

  public bool Background { get; set; }

  public MultipleHits<CollisionResult> WallCollision { get; set; }

  public MultipleHits<TrileInstance> Ground { get; set; }

  public Vector3 Velocity { get; set; }

  public Vector3 GroundMovement { get; set; }

  public bool NoVelocityClamping { get; set; }

  public bool IgnoreClampToWater { get; set; }

  public InstancePhysicsState(TrileInstance instance)
  {
    this.instance = instance;
    this.Center = instance.Center;
    this.CornerCollision = new PointCollision[4];
    Trile trile = instance.Trile;
    this.Elasticity = trile.ActorSettings.Type == ActorType.Vase || trile.Faces.Values.Any<CollisionType>((Func<CollisionType, bool>) (x => x == CollisionType.AllSides)) ? 0.0f : 0.15f;
  }

  public PointCollision[] CornerCollision { get; private set; }

  public bool Grounded => this.Ground.First != null;

  public Vector3 Center { get; set; }

  public Vector3 Size => this.instance.TransformedSize;

  public bool Static
  {
    get
    {
      return this.StaticGrounds && this.Grounded && FezMath.AlmostEqual(this.Velocity, Vector3.Zero) && !this.ForceNonStatic;
    }
  }

  public bool StaticGrounds
  {
    get
    {
      return InstancePhysicsState.IsGroundStatic(this.Ground.NearLow) && InstancePhysicsState.IsGroundStatic(this.Ground.FarHigh);
    }
  }

  private static bool IsGroundStatic(TrileInstance ground)
  {
    if (ground == null || ground.PhysicsState == null)
      return true;
    return ground.PhysicsState.Velocity == Vector3.Zero && ground.PhysicsState.GroundMovement == Vector3.Zero;
  }

  public bool Sliding => !FezMath.AlmostEqual(this.Velocity.XZ(), Vector2.Zero);

  public float Elasticity { get; private set; }

  public bool Floating { get; set; }

  public void UpdateInstance()
  {
    this.instance.Position = this.Center - (this.instance.Center - this.instance.Position);
  }
}
