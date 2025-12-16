// Decompiled with JetBrains decompiler
// Type: FezGame.Services.IPlayerManager
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Structure;
using FezGame.Components;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Services;

public interface IPlayerManager : IComplexPhysicsEntity, IPhysicsEntity
{
  void Reset();

  List<Volume> CurrentVolumes { get; }

  bool CanRotate { get; set; }

  ActionType Action { get; set; }

  ActionType LastAction { get; set; }

  ActionType NextAction { get; set; }

  bool CanDoubleJump { get; set; }

  void SyncCollisionSize();

  TrileInstance HeldInstance { get; set; }

  TrileInstance CarriedInstance { get; set; }

  TrileInstance PushedInstance { get; set; }

  Vector3 Position { get; set; }

  Vector3 RespawnPosition { get; }

  Vector3 LeaveGroundPosition { get; set; }

  float OffsetAtLeaveGround { get; }

  TrileInstance CheckpointGround { get; set; }

  HorizontalDirection LookingDirection { get; set; }

  void RecordRespawnInformation();

  void RecordRespawnInformation(bool markCheckpoint);

  void Respawn();

  void RespawnAtCheckpoint();

  void ForceOverlapsDetermination();

  GomezHost MeshHost { get; set; }

  bool CanControl { get; set; }

  bool DoorEndsTrial { get; set; }

  TimeSpan AirTime { get; set; }

  string NextLevel { get; set; }

  int? DoorVolume { get; set; }

  int? PipeVolume { get; set; }

  int? TunnelVolume { get; set; }

  AnimatedTexture Animation { get; set; }

  bool IgnoreFreefall { get; set; }

  bool SpinThroughDoor { get; set; }

  bool Hidden { get; set; }

  bool IsOnRotato { get; set; }

  TrileInstance ForcedTreasure { get; set; }

  bool HideFez { get; set; }

  bool InDoorTransition { get; set; }

  WarpPanel WarpPanel { get; set; }

  Viewpoint OriginWarpViewpoint { get; set; }

  AnimatedTexture GetAnimation(ActionType type);

  void FillAnimations();

  float GomezOpacity { get; set; }

  bool FullBright { get; set; }

  float BlinkSpeed { get; set; }

  Vector3 SplitUpCubeCollectorOffset { get; set; }

  void CopyTo(IPlayerManager other);

  bool FreshlyRespawned { get; set; }
}
