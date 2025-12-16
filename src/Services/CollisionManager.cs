// Decompiled with JetBrains decompiler
// Type: FezGame.Services.CollisionManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Common;
using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezGame.Services;

public class CollisionManager : ICollisionManager
{
  private float gravityFactor;

  public event Action GravityChanged = new Action(Util.NullAction);

  public CollisionManager() => this.gravityFactor = 1f;

  public float DistanceEpsilon => 1f / 1000f;

  public float GravityFactor
  {
    get => this.gravityFactor;
    set
    {
      if ((double) value == (double) this.gravityFactor)
        return;
      this.gravityFactor = value;
      this.GravityChanged();
    }
  }

  public void CollideRectangle(
    Vector3 position,
    Vector3 impulse,
    Vector3 size,
    out MultipleHits<CollisionResult> horizontalResults,
    out MultipleHits<CollisionResult> verticalResults)
  {
    this.CollideRectangle(position, impulse, size, QueryOptions.None, out horizontalResults, out verticalResults);
  }

  public void CollideRectangle(
    Vector3 position,
    Vector3 impulse,
    Vector3 size,
    QueryOptions options,
    out MultipleHits<CollisionResult> horizontalResults,
    out MultipleHits<CollisionResult> verticalResults)
  {
    this.CollideRectangle(position, impulse, size, options, 0.0f, out horizontalResults, out verticalResults);
  }

  public void CollideRectangle(
    Vector3 position,
    Vector3 impulse,
    Vector3 size,
    QueryOptions options,
    float elasticity,
    out MultipleHits<CollisionResult> horizontalResults,
    out MultipleHits<CollisionResult> verticalResults)
  {
    this.CollideRectangle(position, impulse, size, options, elasticity, this.CameraManager.Viewpoint, out horizontalResults, out verticalResults);
  }

  public void CollideRectangle(
    Vector3 position,
    Vector3 impulse,
    Vector3 size,
    QueryOptions options,
    float elasticity,
    Viewpoint viewpoint,
    out MultipleHits<CollisionResult> horizontalResults,
    out MultipleHits<CollisionResult> verticalResults)
  {
    Vector3 impulse1 = (viewpoint == Viewpoint.Front ? 1 : (viewpoint == Viewpoint.Back ? 1 : 0)) != 0 ? new Vector3(impulse.X, 0.0f, 0.0f) : new Vector3(0.0f, 0.0f, impulse.Z);
    Vector3 impulse2 = new Vector3(0.0f, impulse.Y, 0.0f);
    Vector3 halfSize = size / 2f;
    horizontalResults = this.CollideEdge(position, impulse1, halfSize, Direction2D.Horizontal, options, elasticity, viewpoint);
    verticalResults = this.CollideEdge(position, impulse2, halfSize, Direction2D.Vertical, options, elasticity, viewpoint);
    if ((options & QueryOptions.Simple) == QueryOptions.Simple || horizontalResults.AnyCollided() || verticalResults.AnyCollided())
      return;
    horizontalResults = this.CollideEdge(position + impulse2, impulse1, halfSize, Direction2D.Horizontal, options, elasticity, viewpoint);
    verticalResults = this.CollideEdge(position + impulse1, impulse2, halfSize, Direction2D.Vertical, options, elasticity, viewpoint);
  }

  public MultipleHits<CollisionResult> CollideEdge(
    Vector3 position,
    Vector3 impulse,
    Vector3 halfSize,
    Direction2D direction)
  {
    return this.CollideEdge(position, impulse, halfSize, direction, QueryOptions.None);
  }

  public MultipleHits<CollisionResult> CollideEdge(
    Vector3 position,
    Vector3 impulse,
    Vector3 halfSize,
    Direction2D direction,
    QueryOptions options)
  {
    return this.CollideEdge(position, impulse, halfSize, direction, options, 0.0f);
  }

  public MultipleHits<CollisionResult> CollideEdge(
    Vector3 position,
    Vector3 impulse,
    Vector3 halfSize,
    Direction2D direction,
    QueryOptions options,
    float elasticity)
  {
    return this.CollideEdge(position, impulse, halfSize, direction, options, elasticity, this.CameraManager.Viewpoint);
  }

  public MultipleHits<CollisionResult> CollideEdge(
    Vector3 position,
    Vector3 impulse,
    Vector3 halfSize,
    Direction2D direction,
    QueryOptions options,
    float elasticity,
    Viewpoint viewpoint)
  {
    MultipleHits<CollisionResult> multipleHits = new MultipleHits<CollisionResult>();
    if (impulse == Vector3.Zero)
      return multipleHits;
    bool flag = (options & QueryOptions.Simple) == QueryOptions.Simple;
    Vector3 vector3_1 = new Vector3((float) Math.Sign(impulse.X), (float) Math.Sign(impulse.Y), (float) Math.Sign(impulse.Z));
    if (!flag)
    {
      Vector3 position1 = position;
      Vector3 position2 = position;
      switch (direction)
      {
        case Direction2D.Horizontal:
          position1 += (vector3_1 + Vector3.Down) * halfSize;
          position2 += (vector3_1 + Vector3.Up) * halfSize;
          break;
        case Direction2D.Vertical:
          Vector3 vector3_2 = viewpoint.RightVector() * (float) this.PlayerManager.LookingDirection.Sign();
          position1 += (vector3_1 - vector3_2) * halfSize;
          position2 += (vector3_1 + vector3_2) * halfSize;
          break;
      }
      multipleHits.NearLow = this.CollidePoint(position1, impulse, options, elasticity, viewpoint);
      multipleHits.FarHigh = this.CollidePoint(position2, impulse, options, elasticity, viewpoint);
    }
    if (flag || !multipleHits.NearLow.Collided)
    {
      Vector3 position3 = position + vector3_1 * halfSize;
      multipleHits.NearLow = this.CollidePoint(position3, impulse, options, elasticity, viewpoint);
    }
    return multipleHits;
  }

  public CollisionResult CollidePoint(Vector3 position, Vector3 impulse)
  {
    return this.CollidePoint(position, impulse, QueryOptions.None);
  }

  public CollisionResult CollidePoint(Vector3 position, Vector3 impulse, QueryOptions options)
  {
    return this.CollidePoint(position, impulse, options, 0.0f);
  }

  public CollisionResult CollidePoint(
    Vector3 position,
    Vector3 impulse,
    QueryOptions options,
    float elasticity)
  {
    return this.CollidePoint(position, impulse, options, elasticity, this.CameraManager.Viewpoint);
  }

  public CollisionResult CollidePoint(
    Vector3 position,
    Vector3 impulse,
    QueryOptions options,
    float elasticity,
    Viewpoint viewpoint)
  {
    CollisionResult collisionResult = new CollisionResult();
    Vector3 vector3 = position + impulse;
    TrileInstance instance1 = (TrileInstance) null;
    if ((options & QueryOptions.Background) != QueryOptions.None)
      instance1 = this.LevelManager.ActualInstanceAt(vector3);
    if (instance1 == null)
    {
      NearestTriles nearestTriles = this.LevelManager.NearestTrile(vector3, options, new Viewpoint?(viewpoint));
      instance1 = nearestTriles.Deep ?? nearestTriles.Surface;
    }
    bool invertedGravity = (double) this.GravityFactor < 0.0;
    if (instance1 != null)
      collisionResult = CollisionManager.CollideWithInstance(position, vector3, impulse, instance1, options, elasticity, viewpoint, invertedGravity);
    if ((options & QueryOptions.Background) != QueryOptions.None && !collisionResult.Collided && (double) impulse.Y < 0.0)
    {
      NearestTriles nearestTriles = this.LevelManager.NearestTrile(vector3, options, new Viewpoint?(viewpoint));
      TrileInstance instance2 = nearestTriles.Deep ?? nearestTriles.Surface;
      if (instance2 != null)
        collisionResult = CollisionManager.CollideWithInstance(position, vector3, impulse, instance2, options, elasticity, viewpoint, invertedGravity);
    }
    if (collisionResult.Collided && (invertedGravity ? ((double) impulse.Y > 0.0 ? 1 : 0) : ((double) impulse.Y < 0.0 ? 1 : 0)) != 0)
    {
      if ((double) vector3.X % 0.25 == 0.0)
        vector3.X += 1f / 1000f;
      if ((double) vector3.Z % 0.25 == 0.0)
        vector3.Z += 1f / 1000f;
      TrileInstance trileInstance = this.LevelManager.ActualInstanceAt(vector3);
      CollisionType rotatedFace;
      collisionResult.ShouldBeClamped = trileInstance == null || !trileInstance.Enabled || (rotatedFace = trileInstance.GetRotatedFace(this.CameraManager.VisibleOrientation)) == CollisionType.None || rotatedFace == CollisionType.Immaterial;
    }
    return collisionResult;
  }

  private static CollisionResult CollideWithInstance(
    Vector3 origin,
    Vector3 destination,
    Vector3 impulse,
    TrileInstance instance,
    QueryOptions options,
    float elasticity,
    Viewpoint viewpoint,
    bool invertedGravity)
  {
    CollisionResult collisionResult = new CollisionResult();
    Vector3 normal = -impulse.Sign();
    FaceOrientation faceOrientation = viewpoint.VisibleOrientation();
    if ((options & QueryOptions.Background) == QueryOptions.Background)
      faceOrientation = faceOrientation.GetOpposite();
    CollisionType rotatedFace = instance.GetRotatedFace(faceOrientation);
    if (rotatedFace != CollisionType.None)
    {
      collisionResult.Destination = instance;
      collisionResult.NearestDistance = instance.Center;
      collisionResult.Response = CollisionManager.SolidCollision(normal, instance, origin, destination, impulse, elasticity);
      if (collisionResult.Response != Vector3.Zero)
      {
        ref CollisionResult local = ref collisionResult;
        int num;
        switch (rotatedFace)
        {
          case CollisionType.AllSides:
            num = 1;
            break;
          case CollisionType.TopOnly:
          case CollisionType.TopNoStraightLedge:
            num = invertedGravity ? ((double) normal.Y < 0.0 ? 1 : 0) : ((double) normal.Y > 0.0 ? 1 : 0);
            break;
          default:
            num = 0;
            break;
        }
        local.Collided = num != 0;
      }
    }
    return collisionResult;
  }

  private static Vector3 SolidCollision(
    Vector3 normal,
    TrileInstance instance,
    Vector3 origin,
    Vector3 destination,
    Vector3 impulse,
    float elasticity)
  {
    Vector3 vector3_1 = instance.TransformedSize / 2f;
    Vector3 vector3_2 = instance.Center + vector3_1 * normal;
    Vector3 vector3_3 = Vector3.Zero;
    Vector3 vector3_4;
    if (instance.PhysicsState != null)
    {
      Vector3 vector3_5 = instance.PhysicsState.Sticky ? FezMath.XZMask : Vector3.One;
      vector3_4 = instance.Center - instance.PhysicsState.Velocity * vector3_5 + vector3_1 * normal;
      vector3_3 = vector3_2 - vector3_4;
    }
    else
      vector3_4 = vector3_2;
    Vector3 a1 = origin - vector3_4;
    Vector3 a2 = destination - vector3_2;
    Vector3 b = normal;
    if ((double) FezMath.AlmostClamp(a1.Dot(b)) < 0.0 || (double) FezMath.AlmostClamp(a2.Dot(normal)) > 0.0)
      return Vector3.Zero;
    Vector3 vector3_6 = normal.Abs();
    return (double) elasticity <= 0.0 ? (vector3_2 - destination) * vector3_6 : (vector3_3 - impulse) * vector3_6 * (1f + elasticity);
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }
}
