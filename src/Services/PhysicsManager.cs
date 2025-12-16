// Decompiled with JetBrains decompiler
// Type: FezGame.Services.PhysicsManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Services;

public class PhysicsManager : IPhysicsManager
{
  public const float TrileSize = 0.15f;
  private const float GroundFriction = 0.85f;
  private const float WaterFriction = 0.925f;
  private const float SlidingFriction = 0.8f;
  private const float AirFriction = 0.9975f;
  private const float FallingSpeedLimit = 0.4f;
  private const float HuggingDistance = 0.002f;
  private static readonly Vector3 GroundFrictionV = new Vector3(0.85f, 1f, 0.85f);
  private static readonly Vector3 AirFrictionV = new Vector3(0.9975f, 1f, 0.9975f);
  private static readonly Vector3 WaterFrictionV = new Vector3(0.925f, 1f, 0.925f);
  private static readonly Vector3 SlidingFrictionV = new Vector3(0.8f, 1f, 0.8f);

  public void DetermineOverlaps(IComplexPhysicsEntity entity)
  {
    this.DetermineOverlapsInternal((IPhysicsEntity) entity, this.CameraManager.Viewpoint);
    Vector3 center = entity.Center;
    Vector3 vector3 = entity.Size / 2f - new Vector3(1f / 1000f);
    if ((double) this.CollisionManager.GravityFactor < 0.0)
      vector3 *= new Vector3(1f, -1f, 1f);
    QueryOptions options = entity.Background ? QueryOptions.Background : QueryOptions.None;
    entity.AxisCollision[VerticalDirection.Up] = this.LevelManager.NearestTrile(center + vector3 * Vector3.Up, options);
    entity.AxisCollision[VerticalDirection.Down] = this.LevelManager.NearestTrile(center + vector3 * Vector3.Down, options);
  }

  public void DetermineOverlaps(ISimplePhysicsEntity entity)
  {
    this.DetermineOverlapsInternal((IPhysicsEntity) entity, this.CameraManager.Viewpoint);
  }

  private void DetermineOverlapsInternal(IPhysicsEntity entity, Viewpoint viewpoint)
  {
    QueryOptions options = QueryOptions.None;
    if (entity.Background)
      options |= QueryOptions.Background;
    Vector3 center = entity.Center;
    if (entity.CornerCollision.Length == 1)
    {
      entity.CornerCollision[0] = new PointCollision(center, this.LevelManager.NearestTrile(center, options, new Viewpoint?(viewpoint)));
    }
    else
    {
      Vector3 vector3_1 = viewpoint.RightVector();
      Vector3 vector3_2 = entity.Size / 2f - new Vector3(1f / 1000f);
      if ((double) this.CollisionManager.GravityFactor < 0.0)
        vector3_2 *= new Vector3(1f, -1f, 1f);
      Vector3 vector3_3 = center + (vector3_1 + Vector3.Up) * vector3_2;
      entity.CornerCollision[0] = new PointCollision(vector3_3, this.LevelManager.NearestTrile(vector3_3, options, new Viewpoint?(viewpoint)));
      if (entity.CornerCollision[0].Instances.Deep == null && this.PlayerManager.CarriedInstance != null)
      {
        this.PlayerManager.CarriedInstance.PhysicsState.UpdatingPhysics = true;
        entity.CornerCollision[0] = new PointCollision(center, this.LevelManager.NearestTrile(center, options, new Viewpoint?(viewpoint)));
        this.PlayerManager.CarriedInstance.PhysicsState.UpdatingPhysics = false;
      }
      Vector3 vector3_4 = center + (vector3_1 + Vector3.Down) * vector3_2;
      entity.CornerCollision[1] = new PointCollision(vector3_4, this.LevelManager.NearestTrile(vector3_4, options, new Viewpoint?(viewpoint)));
      Vector3 vector3_5 = center + (-vector3_1 + Vector3.Up) * vector3_2;
      entity.CornerCollision[2] = new PointCollision(vector3_5, this.LevelManager.NearestTrile(vector3_5, options, new Viewpoint?(viewpoint)));
      Vector3 vector3_6 = center + (-vector3_1 + Vector3.Down) * vector3_2;
      entity.CornerCollision[3] = new PointCollision(vector3_6, this.LevelManager.NearestTrile(vector3_6, options, new Viewpoint?(viewpoint)));
    }
  }

  public bool DetermineInBackground(
    IPhysicsEntity entity,
    bool allowEnterInBackground,
    bool postRotation,
    bool keepInFront)
  {
    bool inBackground = false;
    if (allowEnterInBackground)
    {
      Vector3? distance = new Vector3?();
      Vector3 center = entity.Center;
      if (entity is IComplexPhysicsEntity)
      {
        IComplexPhysicsEntity complexPhysicsEntity = entity as IComplexPhysicsEntity;
        Vector3 impulse = 1f / 32f * Vector3.Down;
        QueryOptions options = QueryOptions.None;
        if (complexPhysicsEntity.Background)
          options |= QueryOptions.Background;
        bool flag = this.CollisionManager.CollideEdge(entity.Center, impulse, entity.Size / 2f, Direction2D.Vertical, options).AnyHit();
        if (!flag)
          flag |= this.CollisionManager.CollideEdge(entity.Center, impulse, entity.Size / 2f, Direction2D.Vertical, options, 0.0f, this.CameraManager.Viewpoint.GetOpposite()).AnyHit();
        if (complexPhysicsEntity.Grounded && !flag)
        {
          this.DebuggingBag.Add("zz. had to re-clamp to ground", (object) "POSITIF");
          MultipleHits<CollisionResult> result = this.CollisionManager.CollideEdge(entity.Center, impulse, entity.Size / 2f, Direction2D.Vertical, options, 0.0f, this.CameraManager.LastViewpoint);
          if (result.AnyCollided())
            distance = new Vector3?(result.First().NearestDistance);
        }
      }
      entity.Background = false;
      PhysicsManager.WallHuggingResult wallHuggingResult1;
      do
      {
        this.DetermineOverlapsInternal(entity, this.CameraManager.Viewpoint);
        wallHuggingResult1 = this.HugWalls(entity, true, postRotation, keepInFront);
      }
      while (wallHuggingResult1.Hugged);
      entity.Background = wallHuggingResult1.Behind;
      if (!entity.Background && distance.HasValue)
      {
        inBackground = true;
        entity.Center = center;
        this.ClampToGround(entity, distance, this.CameraManager.LastViewpoint);
        this.ClampToGround(entity, distance, this.CameraManager.Viewpoint);
        entity.Velocity *= Vector3.UnitY;
        entity.Background = false;
        PhysicsManager.WallHuggingResult wallHuggingResult2;
        do
        {
          this.DetermineOverlapsInternal(entity, this.CameraManager.Viewpoint);
          wallHuggingResult2 = this.HugWalls(entity, true, postRotation, keepInFront);
        }
        while (wallHuggingResult2.Hugged);
        entity.Background = wallHuggingResult2.Behind;
      }
      this.DetermineOverlapsInternal(entity, this.CameraManager.Viewpoint);
      this.HugWalls(entity, false, false, keepInFront);
    }
    else if (entity.Background)
    {
      bool flag = true;
      foreach (PointCollision pointCollision in entity.CornerCollision)
        flag &= !this.IsHuggable(pointCollision.Instances.Deep, entity);
      if (flag)
        entity.Background = false;
    }
    return inBackground;
  }

  public PhysicsManager.WallHuggingResult HugWalls(
    IPhysicsEntity entity,
    bool determineBackground,
    bool postRotation,
    bool keepInFront)
  {
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ForwardVector();
    if (!entity.Background)
      vector3_1 = -vector3_1;
    float num1 = 1f / 500f;
    if (entity is ISimplePhysicsEntity)
      num1 = 1f / 16f;
    PhysicsManager.WallHuggingResult wallHuggingResult = new PhysicsManager.WallHuggingResult();
    Vector3 vector3_2 = new Vector3();
    if (entity.Background && entity.Grounded)
      return wallHuggingResult;
    foreach (PointCollision pointCollision in entity.CornerCollision)
    {
      TrileInstance trileInstance1 = (TrileInstance) null;
      if (this.IsHuggable(pointCollision.Instances.Surface, entity))
      {
        FaceOrientation face = FaceOrientation.Down;
        TrileEmplacement traversal = pointCollision.Instances.Surface.Emplacement.GetTraversal(ref face);
        TrileInstance trileInstance2 = this.LevelManager.TrileInstanceAt(ref traversal);
        if (trileInstance2 != null && trileInstance2.Enabled && trileInstance2.GetRotatedFace(this.CameraManager.VisibleOrientation) != CollisionType.None)
          trileInstance1 = pointCollision.Instances.Surface;
      }
      if (trileInstance1 == null && this.IsHuggable(pointCollision.Instances.Deep, entity))
        trileInstance1 = pointCollision.Instances.Deep;
      if (trileInstance1 != null && (!(entity is ISimplePhysicsEntity) || trileInstance1.PhysicsState == null || !trileInstance1.PhysicsState.Puppet) && trileInstance1.PhysicsState != entity)
      {
        Vector3 vector3_3 = trileInstance1.Center + vector3_1 * trileInstance1.TransformedSize / 2f;
        Vector3 vector1 = entity.Center - vector3_1 * entity.Size / 2f - vector3_3 + num1 * -vector3_1;
        float x = Vector3.Dot(vector1, vector3_1);
        if ((double) FezMath.AlmostClamp(x) < 0.0)
        {
          if (determineBackground && (!trileInstance1.Trile.Thin || trileInstance1.Trile.ForceHugging))
          {
            float num2 = Math.Abs((trileInstance1.TransformedSize / 2f + entity.Size / 2f).Dot(vector3_1));
            wallHuggingResult.Behind |= (double) Math.Abs(x) > (double) num2;
          }
          else if (keepInFront)
          {
            Vector3 vector3_4 = vector1 * vector3_1.Abs();
            vector3_2 -= vector3_4;
            entity.Center -= vector3_4;
            wallHuggingResult.Hugged = true;
          }
        }
      }
      if (postRotation)
      {
        Vector3 vector3_5 = this.CameraManager.LastViewpoint.VisibleOrientation().AsVector();
        TrileInstance instance = this.LevelManager.ActualInstanceAt(pointCollision.Point + vector3_2);
        if (this.IsHuggable(instance, entity))
        {
          Vector3 vector3_6 = instance.Center + vector3_5 * instance.TransformedSize.ZYX() / 2f;
          Vector3 vector1 = entity.Center - vector3_5 * entity.Size / 2f - vector3_6 + 1f / 500f * vector3_5;
          float x = Vector3.Dot(vector1, vector3_5);
          if ((((double) FezMath.AlmostClamp(x) >= 0.0 ? 0 : ((double) x > -1.0 ? 1 : 0)) & (keepInFront ? 1 : 0)) != 0)
          {
            Vector3 vector3_7 = vector1 * vector3_5.Abs();
            vector3_2 -= vector3_7;
            entity.Center -= vector3_7;
            wallHuggingResult.Hugged = true;
          }
        }
      }
    }
    return wallHuggingResult;
  }

  private static bool IsInstanceStateful(TrileInstance instance)
  {
    return instance != null && instance.PhysicsState != null;
  }

  private bool IsHuggable(TrileInstance instance, IPhysicsEntity entity)
  {
    return instance != null && instance.Enabled && !instance.Trile.Immaterial && (!instance.Trile.Thin || instance.Trile.ForceHugging) && instance != this.PlayerManager.CarriedInstance && instance != this.PlayerManager.PushedInstance && (!instance.Trile.ActorSettings.Type.IsBomb() || entity.Background) && (instance.PhysicsState == null || instance.PhysicsState != entity) && !FezMath.In<CollisionType>(instance.GetRotatedFace(entity.Background ? this.CameraManager.VisibleOrientation.GetOpposite() : this.CameraManager.VisibleOrientation), CollisionType.Immaterial, CollisionType.TopNoStraightLedge, CollisionType.AllSides, (IEqualityComparer<CollisionType>) CollisionTypeComparer.Default);
  }

  public bool Update(IComplexPhysicsEntity entity)
  {
    QueryOptions queryOptions = QueryOptions.None;
    if (entity.Background)
      queryOptions |= QueryOptions.Background;
    this.MoveAlongWithGround((IPhysicsEntity) entity, queryOptions);
    MultipleHits<CollisionResult> horizontalResults;
    MultipleHits<CollisionResult> verticalResults;
    this.CollisionManager.CollideRectangle(entity.Center, entity.Velocity, entity.Size, queryOptions, entity.Elasticity, out horizontalResults, out verticalResults);
    bool grounded = entity.Grounded;
    MultipleHits<TrileInstance> ground1 = entity.Ground;
    Vector3? clampToGroundDistance = new Vector3?();
    FaceOrientation visibleOrientation = this.CameraManager.VisibleOrientation;
    bool flag1 = (double) this.CollisionManager.GravityFactor < 0.0;
    if (verticalResults.AnyCollided() && (flag1 ? ((double) entity.Velocity.Y > 0.0 ? 1 : 0) : ((double) entity.Velocity.Y < 0.0 ? 1 : 0)) != 0)
    {
      MultipleHits<TrileInstance> ground2 = entity.Ground;
      CollisionResult nearLow = verticalResults.NearLow;
      CollisionResult farHigh = verticalResults.FarHigh;
      if (farHigh.Destination != null && farHigh.Destination.GetRotatedFace(visibleOrientation) != CollisionType.None)
      {
        ground2.FarHigh = farHigh.Destination;
        if (farHigh.Collided && (farHigh.ShouldBeClamped || entity.MustBeClampedToGround))
          clampToGroundDistance = new Vector3?(farHigh.NearestDistance);
      }
      else
        ground2.FarHigh = (TrileInstance) null;
      if (nearLow.Destination != null && nearLow.Destination.GetRotatedFace(visibleOrientation) != CollisionType.None)
      {
        ground2.NearLow = nearLow.Destination;
        if (nearLow.Collided && (nearLow.ShouldBeClamped || entity.MustBeClampedToGround))
          clampToGroundDistance = new Vector3?(nearLow.NearestDistance);
      }
      else
        ground2.NearLow = (TrileInstance) null;
      entity.Ground = ground2;
    }
    else
      entity.Ground = new MultipleHits<TrileInstance>();
    entity.Ceiling = (double) entity.Velocity.Y <= 0.0 || !verticalResults.AnyCollided() ? new MultipleHits<CollisionResult>() : verticalResults;
    bool flag2 = (this.PlayerManager.Action == ActionType.Grabbing || this.PlayerManager.Action == ActionType.Pushing || this.PlayerManager.Action == ActionType.GrabCornerLedge || this.PlayerManager.Action == ActionType.LowerToCornerLedge || this.PlayerManager.Action == ActionType.SuckedIn || this.PlayerManager.Action == ActionType.Landing) | entity.MustBeClampedToGround;
    entity.MustBeClampedToGround = false;
    bool velocityIrrelevant = ((flag2 ? 1 : 0) | (!entity.Grounded ? 0 : (entity.Ground.First.ForceClampToGround ? 1 : 0))) != 0;
    if (grounded && !entity.Grounded)
      entity.GroundedVelocity = new Vector3?(entity.Velocity);
    else if (!grounded && entity.Grounded)
      entity.GroundedVelocity = new Vector3?();
    Vector3 vector2 = this.CameraManager.Viewpoint.RightVector();
    entity.MovingDirection = FezMath.DirectionFromMovement(Vector3.Dot(entity.Velocity, vector2));
    bool flag3 = this.PlayerManager.Action == ActionType.FrontClimbingLadder || this.PlayerManager.Action == ActionType.FrontClimbingVine;
    if (entity.GroundMovement != Vector3.Zero | flag3)
      this.DetermineInBackground((IPhysicsEntity) entity, true, false, !this.PlayerManager.Climbing);
    return this.UpdateInternal((IPhysicsEntity) entity, horizontalResults, verticalResults, clampToGroundDistance, grounded, !entity.HandlesZClamping, velocityIrrelevant, false);
  }

  public bool Update(ISimplePhysicsEntity entity) => this.Update(entity, false, true);

  public bool Update(ISimplePhysicsEntity entity, bool simple, bool keepInFront)
  {
    QueryOptions queryOptions = QueryOptions.None;
    if (entity.Background)
      queryOptions |= QueryOptions.Background;
    if (simple)
      queryOptions |= QueryOptions.Simple;
    if (entity is InstancePhysicsState)
      (entity as InstancePhysicsState).UpdatingPhysics = true;
    if (!simple)
      this.MoveAlongWithGround((IPhysicsEntity) entity, queryOptions);
    Vector3? clampToGroundDistance = new Vector3?();
    bool grounded = entity.Grounded;
    MultipleHits<CollisionResult> horizontalResults;
    MultipleHits<CollisionResult> verticalResults;
    if (!entity.IgnoreCollision)
    {
      this.CollisionManager.CollideRectangle(entity.Center, entity.Velocity, entity.Size, queryOptions, entity.Elasticity, out horizontalResults, out verticalResults);
      int num = (double) this.CollisionManager.GravityFactor < 0.0 ? 1 : 0;
      FaceOrientation faceOrientation = this.CameraManager.VisibleOrientation;
      if (entity.Background)
        faceOrientation = faceOrientation.GetOpposite();
      if ((num != 0 ? ((double) entity.Velocity.Y > 0.0 ? 1 : 0) : ((double) entity.Velocity.Y < 0.0 ? 1 : 0)) != 0 && verticalResults.AnyCollided())
      {
        MultipleHits<TrileInstance> ground = entity.Ground;
        CollisionResult nearLow = verticalResults.NearLow;
        CollisionResult farHigh = verticalResults.FarHigh;
        if (farHigh.Destination != null && farHigh.Destination.GetRotatedFace(faceOrientation) != CollisionType.None)
        {
          ground.FarHigh = farHigh.Destination;
          if (farHigh.Collided && farHigh.ShouldBeClamped)
            clampToGroundDistance = new Vector3?(farHigh.NearestDistance);
        }
        else
          ground.FarHigh = (TrileInstance) null;
        if (nearLow.Destination != null && nearLow.Destination.GetRotatedFace(faceOrientation) != CollisionType.None)
        {
          ground.NearLow = nearLow.Destination;
          if (nearLow.Collided && nearLow.ShouldBeClamped)
            clampToGroundDistance = new Vector3?(nearLow.NearestDistance);
        }
        else
          ground.NearLow = (TrileInstance) null;
        entity.Ground = ground;
      }
      else
        entity.Ground = new MultipleHits<TrileInstance>();
    }
    else
    {
      horizontalResults = new MultipleHits<CollisionResult>();
      verticalResults = new MultipleHits<CollisionResult>();
    }
    int num1 = this.UpdateInternal((IPhysicsEntity) entity, horizontalResults, verticalResults, clampToGroundDistance, grounded, keepInFront, false, simple) ? 1 : 0;
    if (!(entity is InstancePhysicsState))
      return num1 != 0;
    (entity as InstancePhysicsState).UpdatingPhysics = false;
    return num1 != 0;
  }

  private bool UpdateInternal(
    IPhysicsEntity entity,
    MultipleHits<CollisionResult> horizontalResults,
    MultipleHits<CollisionResult> verticalResults,
    Vector3? clampToGroundDistance,
    bool wasGrounded,
    bool hugWalls,
    bool velocityIrrelevant,
    bool simple)
  {
    Vector3 velocity = entity.Velocity;
    if (!simple)
    {
      MultipleHits<CollisionResult> multipleHits = new MultipleHits<CollisionResult>();
      if (horizontalResults.AnyCollided())
      {
        if (horizontalResults.NearLow.Collided)
          multipleHits.NearLow = horizontalResults.NearLow;
        if (horizontalResults.FarHigh.Collided)
          multipleHits.FarHigh = horizontalResults.FarHigh;
      }
      entity.WallCollision = multipleHits;
    }
    if (horizontalResults.NearLow.Collided)
      velocity += horizontalResults.NearLow.Response;
    else if (horizontalResults.FarHigh.Collided)
      velocity += horizontalResults.FarHigh.Response;
    if (verticalResults.NearLow.Collided)
      velocity += verticalResults.NearLow.Response;
    else if (verticalResults.FarHigh.Collided)
      velocity += verticalResults.FarHigh.Response;
    Vector3 vector3_1 = (!(entity is IComplexPhysicsEntity) ? 0 : ((entity as IComplexPhysicsEntity).Swimming ? 1 : 0)) == 0 ? (!(entity.Grounded | wasGrounded) ? PhysicsManager.AirFrictionV : (entity.Sliding ? PhysicsManager.SlidingFrictionV : PhysicsManager.GroundFrictionV)) : PhysicsManager.WaterFrictionV;
    float amount = (float) ((1.2000000476837158 + (double) Math.Abs(this.CollisionManager.GravityFactor) * 0.800000011920929) / 2.0);
    Vector3 vector3_2 = FezMath.AlmostClamp(velocity * Vector3.Lerp(Vector3.One, vector3_1, amount), 1E-06f);
    if (!entity.NoVelocityClamping)
    {
      float max = entity is IComplexPhysicsEntity ? 0.4f : 0.38f;
      vector3_2.Y = MathHelper.Clamp(vector3_2.Y, -max, max);
    }
    Vector3 center = entity.Center;
    Vector3 a = center + vector3_2;
    bool flag = !FezMath.AlmostEqual(a, center);
    entity.Velocity = vector3_2;
    if (flag)
      entity.Center = a;
    if (velocityIrrelevant | flag)
    {
      this.DetermineInBackground(entity, false, false, hugWalls);
      this.ClampToGround(entity, clampToGroundDistance, this.CameraManager.Viewpoint);
      if (hugWalls)
        this.HugWalls(entity, false, false, true);
    }
    if (!simple && (!(entity is ISimplePhysicsEntity) || !(entity as ISimplePhysicsEntity).IgnoreCollision))
    {
      if (this.LevelManager.IsInvalidatingScreen)
        this.ScheduleRedefineCorners(entity);
      else
        this.RedefineCorners(entity);
    }
    return flag;
  }

  private void ScheduleRedefineCorners(IPhysicsEntity entity)
  {
    this.LevelManager.ScreenInvalidated += (Action) (() => this.RedefineCorners(entity));
  }

  private void RedefineCorners(IPhysicsEntity entity)
  {
    if (entity is IComplexPhysicsEntity)
      this.DetermineOverlaps(entity as IComplexPhysicsEntity);
    else
      this.DetermineOverlaps(entity as ISimplePhysicsEntity);
  }

  private void MoveAlongWithGround(IPhysicsEntity entity, QueryOptions queryOptions)
  {
    TrileInstance trileInstance = (TrileInstance) null;
    bool flag = false;
    if (PhysicsManager.IsInstanceStateful(entity.Ground.NearLow))
      trileInstance = entity.Ground.NearLow;
    else if (PhysicsManager.IsInstanceStateful(entity.Ground.FarHigh))
      trileInstance = entity.Ground.FarHigh;
    Vector3 vector3 = entity.GroundMovement;
    if (trileInstance != null)
    {
      entity.GroundMovement = FezMath.AlmostClamp(trileInstance.PhysicsState.Velocity + trileInstance.PhysicsState.GroundMovement);
      if (trileInstance.PhysicsState.Sticky)
        flag = true;
    }
    else
    {
      vector3 = Vector3.Clamp(vector3, -FezMath.XZMask, Vector3.One);
      entity.GroundMovement = Vector3.Zero;
    }
    if (entity.GroundMovement != Vector3.Zero)
    {
      MultipleHits<CollisionResult> horizontalResults;
      MultipleHits<CollisionResult> verticalResults;
      this.CollisionManager.CollideRectangle(entity.Center, entity.GroundMovement, entity.Size, queryOptions, entity.Elasticity, out horizontalResults, out verticalResults);
      entity.GroundMovement += (horizontalResults.AnyCollided() ? horizontalResults.First().Response : Vector3.Zero) + (verticalResults.AnyCollided() ? verticalResults.First().Response : Vector3.Zero);
      Vector3 min = flag ? -Vector3.One : -FezMath.XZMask;
      entity.Center += Vector3.Clamp(entity.GroundMovement, min, Vector3.One);
      if (!(vector3 == Vector3.Zero) || (double) entity.Velocity.Y <= 0.0)
        return;
      entity.Velocity -= entity.GroundMovement * Vector3.UnitY;
    }
    else
    {
      if (flag || !(vector3 != Vector3.Zero))
        return;
      entity.Velocity += vector3 * 0.85f;
    }
  }

  public void ClampToGround(IPhysicsEntity entity, Vector3? distance, Viewpoint viewpoint)
  {
    if (!distance.HasValue)
      return;
    Vector3 mask = viewpoint.VisibleAxis().GetMask();
    entity.Center = distance.Value * mask + (Vector3.One - mask) * entity.Center;
  }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }

  [ServiceDependency]
  public IDebuggingBag DebuggingBag { private get; set; }

  public struct WallHuggingResult
  {
    public bool Hugged;
    public bool Behind;
  }
}
