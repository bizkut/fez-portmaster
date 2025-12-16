// Decompiled with JetBrains decompiler
// Type: FezGame.Components.HeavyGroupState
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using System;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class HeavyGroupState
{
  private readonly TrileGroup group;
  private readonly TrileInstance[] bottomTriles;
  private bool moving;
  private bool velocityNeedsReset;

  public HeavyGroupState(TrileGroup group)
  {
    ServiceHelper.InjectServices((object) this);
    this.group = group;
    int minY = group.Triles.Min<TrileInstance>((Func<TrileInstance, int>) (x => x.Emplacement.Y));
    group.Triles.Sort((Comparison<TrileInstance>) ((a, b) => a.Emplacement.Y - b.Emplacement.Y));
    this.bottomTriles = group.Triles.Where<TrileInstance>((Func<TrileInstance, bool>) (x => x.Emplacement.Y == minY)).ToArray<TrileInstance>();
    foreach (TrileInstance trile in group.Triles)
      trile.PhysicsState = new InstancePhysicsState(trile);
    this.MarkGrounds();
  }

  private void MarkGrounds()
  {
    foreach (TrileInstance trile in this.group.Triles)
    {
      TrileInstance trileInstance = this.LevelManager.ActualInstanceAt(trile.Center - trile.Trile.Size.Y * Vector3.UnitY);
      trile.PhysicsState.Ground = new MultipleHits<TrileInstance>()
      {
        NearLow = trileInstance
      };
      trile.IsMovingGroup = false;
    }
  }

  public void Update(TimeSpan elapsed)
  {
    if (!this.moving)
    {
      if (this.velocityNeedsReset)
      {
        foreach (TrileInstance trile in this.group.Triles)
          trile.PhysicsState.Velocity = Vector3.Zero;
        this.velocityNeedsReset = false;
      }
      bool flag = false;
      foreach (TrileInstance bottomTrile in this.bottomTriles)
      {
        TrileInstance first = bottomTrile.PhysicsState.Ground.First;
        flag = ((flag ? 1 : 0) | (first == null || !first.Enabled ? 0 : (first.PhysicsState == null ? 1 : (first.PhysicsState.Grounded ? 1 : 0)))) != 0;
      }
      if (!flag)
      {
        this.moving = true;
        foreach (TrileInstance trile in this.group.Triles)
        {
          trile.PhysicsState.Ground = new MultipleHits<TrileInstance>();
          trile.IsMovingGroup = true;
        }
      }
    }
    if (!this.moving)
      return;
    Vector3 vector3_1 = 0.472500026f * (float) elapsed.TotalSeconds * -Vector3.UnitY;
    foreach (TrileInstance trile in this.group.Triles)
      trile.PhysicsState.UpdatingPhysics = true;
    bool flag1 = false;
    Vector3 vector3_2 = Vector3.Zero;
    foreach (TrileInstance bottomTrile in this.bottomTriles)
    {
      MultipleHits<CollisionResult> multipleHits = this.CollisionManager.CollideEdge(bottomTrile.Center, bottomTrile.PhysicsState.Velocity + vector3_1, bottomTrile.TransformedSize / 2f, Direction2D.Vertical);
      if (multipleHits.First.Collided)
      {
        flag1 = true;
        vector3_2 = Vector3.Max(vector3_2, multipleHits.First.Response);
      }
    }
    Vector3 vector3_3 = vector3_1 + vector3_2;
    foreach (TrileInstance trile in this.group.Triles)
    {
      trile.Position += (trile.PhysicsState.Velocity += vector3_3);
      this.LevelManager.UpdateInstance(trile);
      trile.PhysicsState.UpdatingPhysics = false;
    }
    if (!flag1)
      return;
    this.MarkGrounds();
    this.moving = false;
    this.velocityNeedsReset = true;
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public ICollisionManager CollisionManager { private get; set; }
}
