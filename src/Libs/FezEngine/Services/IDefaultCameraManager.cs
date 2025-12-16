// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IDefaultCameraManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Services;

public interface IDefaultCameraManager : ICameraProvider
{
  event Action ViewpointChanged;

  event Action PreViewpointChanged;

  float InterpolationSpeed { get; set; }

  Vector3 Center { get; set; }

  Vector3 InterpolatedCenter { get; set; }

  Vector3 Direction { get; set; }

  float FieldOfView { get; }

  bool ActionRunning { get; }

  bool ViewTransitionReached { get; }

  bool ProjectionTransitionNewlyReached { get; }

  float ViewTransitionStep { get; }

  bool InterpolationReached { get; }

  Viewpoint Viewpoint { get; }

  Viewpoint LastViewpoint { get; }

  FaceOrientation VisibleOrientation { get; }

  Vector3 Position { get; }

  float AspectRatio { get; }

  void ResetViewpoints();

  float DefaultViewableWidth { get; set; }

  float Radius { get; set; }

  bool ProjectionTransition { get; }

  float PixelsPerTrixel { get; set; }

  void RebuildView();

  bool ChangeViewpoint(Viewpoint view);

  bool ChangeViewpoint(Viewpoint view, float speedFactor);

  void AlterTransition(Viewpoint newTo);

  void AlterTransition(Vector3 newDestinationDirection);

  void SnapInterpolation();

  BoundingFrustum Frustum { get; }

  Quaternion Rotation { get; }

  Matrix InverseView { get; }

  float NearPlane { get; }

  float FarPlane { get; }

  Vector3 ViewOffset { get; set; }

  bool StickyCam { get; set; }

  bool DollyZoomOut { set; }

  bool Constrained { get; set; }

  bool ForceTransition { get; set; }

  bool ViewTransitionCancelled { get; }

  Vector2? PanningConstraints { get; set; }

  Vector3 ConstrainedCenter { get; set; }

  bool ForceInterpolation { get; set; }

  void InterpolationCallback(GameTime gameTime);
}
