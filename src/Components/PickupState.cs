// Decompiled with JetBrains decompiler
// Type: FezGame.Components.PickupState
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

internal class PickupState
{
  public readonly TrileInstance Instance;
  public readonly Vector3 OriginalCenter;
  public TrileGroup Group;
  public Vector3 LastGroundedCenter;
  public Vector3 LastVelocity;
  public float FlightApex;
  public bool TouchesWater;
  public float FloatSeed;
  public float FloatMalus;
  public Vector3 LastMovement;
  public PickupState VisibleOverlapper;
  public ArtObjectInstance[] AttachedAOs;

  public PickupState(TrileInstance ti, TrileGroup group)
  {
    this.Instance = ti;
    this.OriginalCenter = ti.Center;
    this.Group = group;
  }
}
