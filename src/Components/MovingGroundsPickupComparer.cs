// Decompiled with JetBrains decompiler
// Type: FezGame.Components.MovingGroundsPickupComparer
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Components;

internal class MovingGroundsPickupComparer : Comparer<PickupState>
{
  public static readonly MovingGroundsPickupComparer Default = new MovingGroundsPickupComparer();

  public override int Compare(PickupState x, PickupState y)
  {
    int num1 = 0;
    int num2 = 0;
    TrileInstance trileInstance1 = (TrileInstance) null;
    TrileInstance trileInstance2 = (TrileInstance) null;
    MultipleHits<TrileInstance> ground;
    for (InstancePhysicsState physicsState = x.Instance.PhysicsState; physicsState.Grounded; physicsState = trileInstance1.PhysicsState)
    {
      ground = physicsState.Ground;
      if (ground.First.PhysicsState != null)
      {
        ground = physicsState.Ground;
        if (ground.First != x.Instance)
        {
          ground = physicsState.Ground;
          if (ground.First != trileInstance2)
          {
            trileInstance2 = trileInstance1;
            ++num1;
            ground = physicsState.Ground;
            trileInstance1 = ground.First;
          }
          else
            break;
        }
        else
          break;
      }
      else
        break;
    }
    InstancePhysicsState physicsState1 = y.Instance.PhysicsState;
    TrileInstance trileInstance3;
    TrileInstance trileInstance4 = trileInstance3 = (TrileInstance) null;
    for (; physicsState1.Grounded; physicsState1 = trileInstance4.PhysicsState)
    {
      ground = physicsState1.Ground;
      if (ground.First.PhysicsState != null)
      {
        ground = physicsState1.Ground;
        if (ground.First != y.Instance)
        {
          ground = physicsState1.Ground;
          if (ground.First != trileInstance3)
          {
            trileInstance3 = trileInstance4;
            ++num2;
            ground = physicsState1.Ground;
            trileInstance4 = ground.First;
          }
          else
            break;
        }
        else
          break;
      }
      else
        break;
    }
    if (num1 - num2 != 0)
      return num1 - num2;
    Vector3 b = x.Instance.PhysicsState.Velocity.Sign() * FezMath.XZMask;
    return b == y.Instance.PhysicsState.Velocity.Sign() * FezMath.XZMask ? Math.Sign((x.Instance.Position - y.Instance.Position).Dot(b)) : Math.Sign(x.Instance.Position.Y - y.Instance.Position.Y);
  }
}
