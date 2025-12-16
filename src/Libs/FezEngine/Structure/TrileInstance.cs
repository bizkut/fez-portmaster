// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrileInstance
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Services;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class TrileInstance
{
  private static readonly ILevelManager LevelManager;
  private Vector3 cachedPosition;
  private TrileEmplacement cachedEmplacement;
  private Vector3 lastUpdatePosition = -Vector3.One;
  private TrileInstanceData data;
  private Quaternion phiQuat;
  private FaceOrientation phiOri;
  private bool hasPhi;
  private static readonly Quaternion[] QuatLookup = new Quaternion[4]
  {
    FezMath.QuaternionFromPhi(-3.14159274f),
    FezMath.QuaternionFromPhi(-1.57079637f),
    FezMath.QuaternionFromPhi(0.0f),
    FezMath.QuaternionFromPhi(1.57079637f)
  };
  private static readonly FaceOrientation[] OrientationLookup = new FaceOrientation[4]
  {
    FaceOrientation.Back,
    FaceOrientation.Left,
    FaceOrientation.Front,
    FaceOrientation.Right
  };
  [Serialization(Ignore = true)]
  public Trile Trile;
  [Serialization(Ignore = true)]
  public Trile VisualTrile;

  static TrileInstance()
  {
    if (!ServiceHelper.IsFull)
      return;
    TrileInstance.LevelManager = ServiceHelper.Get<ILevelManager>();
  }

  public TrileInstance()
  {
    this.ActorSettings = new InstanceActorSettings();
    this.Enabled = true;
    this.InstanceId = -1;
  }

  public TrileInstance(TrileEmplacement emplacement, int trileId)
    : this(emplacement.AsVector, trileId)
  {
  }

  public TrileInstance(Vector3 position, int trileId)
    : this()
  {
    this.data.PositionPhi = new Vector4(position, 0.0f);
    this.cachedPosition = position;
    this.cachedEmplacement = new TrileEmplacement(position);
    this.Update();
    this.TrileId = trileId;
    this.RefreshTrile();
  }

  [Serialization(Name = "Id", UseAttribute = true)]
  public int TrileId { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public InstanceActorSettings ActorSettings { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public float Phi
  {
    get => this.data.PositionPhi.W;
    set
    {
      this.data.PositionPhi.W = value;
      this.phiQuat = FezMath.QuaternionFromPhi(value);
      this.phiOri = FezMath.OrientationFromPhi(value);
      this.hasPhi = (double) value != 0.0;
    }
  }

  public void SetPhiLight(float phi) => this.data.PositionPhi.W = phi;

  public void SetPhiLight(byte orientation)
  {
    this.data.PositionPhi.W = (float) ((int) orientation - 2) * 1.57079637f;
    this.phiQuat = TrileInstance.QuatLookup[(int) orientation];
    this.phiOri = TrileInstance.OrientationLookup[(int) orientation];
    this.hasPhi = orientation != (byte) 2;
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public List<TrileInstance> OverlappedTriles { get; set; }

  [Serialization(Optional = true)]
  public Vector3 Position
  {
    get => this.cachedPosition;
    set
    {
      this.data.PositionPhi.X = value.X;
      this.data.PositionPhi.Y = value.Y;
      this.data.PositionPhi.Z = value.Z;
      this.cachedPosition = value;
      if (this.PhysicsState != null)
        this.PhysicsState.Center = this.Center;
      this.cachedEmplacement = new TrileEmplacement(this.cachedPosition);
    }
  }

  [Serialization(Ignore = true)]
  public int? VisualTrileId { get; set; }

  [Serialization(Ignore = true)]
  public int InstanceId { get; set; }

  [Serialization(Ignore = true)]
  public bool Enabled { get; set; }

  [Serialization(Ignore = true)]
  public bool Removed { get; set; }

  [Serialization(Ignore = true)]
  public bool Collected { get; set; }

  [Serialization(Ignore = true)]
  public bool Hidden { get; set; }

  [Serialization(Ignore = true)]
  public bool Foreign { get; set; }

  [Serialization(Ignore = true)]
  public bool GlobalSpawn { get; set; }

  [Serialization(Ignore = true)]
  public bool ForceSeeThrough { get; set; }

  [Serialization(Ignore = true)]
  public bool ForceTopMaybe { get; set; }

  [Serialization(Ignore = true)]
  public bool ForceClampToGround { get; set; }

  [Serialization(Ignore = true)]
  public bool SkipCulling { get; set; }

  [Serialization(Ignore = true)]
  public bool NeedsRandomCleanup { get; set; }

  [Serialization(Ignore = true)]
  public bool RandomTracked { get; set; }

  [Serialization(Ignore = true)]
  public float LastTreasureSin { get; set; }

  [Serialization(Ignore = true)]
  internal Vector3 LastUpdatePosition => this.lastUpdatePosition;

  [Serialization(Ignore = true)]
  public InstancePhysicsState PhysicsState { get; set; }

  [Serialization(Ignore = true)]
  public TrileEmplacement Emplacement
  {
    get => this.cachedEmplacement;
    set
    {
      this.data.PositionPhi.X = (float) value.X;
      this.data.PositionPhi.Y = (float) value.Y;
      this.data.PositionPhi.Z = (float) value.Z;
      this.cachedPosition = new Vector3((float) value.X, (float) value.Y, (float) value.Z);
      if (this.PhysicsState != null)
        this.PhysicsState.Center = this.Center;
      this.cachedEmplacement = value;
    }
  }

  public TrileInstanceData Data => this.data;

  [Serialization(Ignore = true)]
  public bool IsMovingGroup { get; set; }

  [Serialization(Ignore = true)]
  public TrileEmplacement OriginalEmplacement { get; set; }

  [Serialization(Ignore = true)]
  public bool Unsafe { get; set; }

  [Serialization(Ignore = true)]
  public Point? OldSsEmplacement { get; set; }

  public void Update() => this.lastUpdatePosition = this.Position;

  public bool Overlaps => this.OverlappedTriles != null && this.OverlappedTriles.Count > 0;

  public TrileInstance PopOverlap()
  {
    if (this.OverlappedTriles == null || this.OverlappedTriles.Count == 0)
      throw new InvalidOperationException();
    int index1 = this.OverlappedTriles.Count - 1;
    TrileInstance overlappedTrile = this.OverlappedTriles[index1];
    this.OverlappedTriles.RemoveAt(index1);
    for (int index2 = index1 - 1; index2 >= 0 && this.OverlappedTriles.Count > index2; --index2)
      overlappedTrile.PushOverlap(this.OverlappedTriles[index2]);
    this.OverlappedTriles.Clear();
    return overlappedTrile;
  }

  public void PushOverlap(TrileInstance instance)
  {
    if (this.OverlappedTriles == null)
      this.OverlappedTriles = new List<TrileInstance>();
    this.OverlappedTriles.Add(instance);
    if (!instance.Overlaps)
      return;
    this.OverlappedTriles.AddRange((IEnumerable<TrileInstance>) instance.OverlappedTriles);
    instance.OverlappedTriles.Clear();
  }

  public void ResetTrile()
  {
    this.VisualTrile = this.Trile = (Trile) null;
    if (TrileInstance.LevelManager.TrileSet != null && !TrileInstance.LevelManager.TrileSet.Triles.ContainsKey(this.TrileId))
      this.TrileId = -1;
    this.RefreshTrile();
  }

  public void RefreshTrile()
  {
    this.Trile = TrileInstance.LevelManager.SafeGetTrile(this.TrileId);
    if (this.VisualTrileId.HasValue)
      this.VisualTrile = TrileInstance.LevelManager.SafeGetTrile(this.VisualTrileId.Value);
    else
      this.VisualTrile = this.Trile;
  }

  public Vector3 TransformedSize
  {
    get
    {
      return this.phiOri == FaceOrientation.Left || this.phiOri == FaceOrientation.Right ? this.Trile.Size.ZYX() : this.Trile.Size;
    }
  }

  public Vector3 Center
  {
    get
    {
      Vector3 vector3 = this.hasPhi ? Vector3.Transform(this.Trile.Offset, this.phiQuat) : this.Trile.Offset;
      return new Vector3((float) ((double) vector3.X * 0.5 + 0.5) + this.cachedPosition.X, (float) ((double) vector3.Y * 0.5 + 0.5) + this.cachedPosition.Y, (float) ((double) vector3.Z * 0.5 + 0.5) + this.cachedPosition.Z);
    }
  }

  public CollisionType GetRotatedFace(FaceOrientation face)
  {
    FaceOrientation key = FezMath.OrientationFromPhi(face.ToPhi() - this.Phi);
    CollisionType rotatedFace = this.Trile.Faces[key];
    if (rotatedFace == CollisionType.TopOnly)
    {
      TrileEmplacement emplacement = this.Emplacement;
      ++emplacement.Y;
      Vector3 mask = face.AsAxis().GetMask();
      TrileInstance trileInstance;
      if (TrileInstance.LevelManager.Triles.TryGetValue(emplacement, out trileInstance) && trileInstance.Enabled && !trileInstance.IsMovingGroup && (trileInstance.Trile.Geometry == null || !trileInstance.Trile.Geometry.Empty || trileInstance.Trile.Faces[key] != CollisionType.None) && !trileInstance.Trile.Immaterial && trileInstance.Trile.Faces[key] != CollisionType.Immaterial && !trileInstance.Trile.Thin && !trileInstance.Trile.ActorSettings.Type.IsPickable() && ((double) trileInstance.Trile.Size.Y == 1.0 || trileInstance.ForceTopMaybe) && FezMath.AlmostEqual(trileInstance.Center.Dot(mask), this.Center.Dot(mask)))
        rotatedFace = CollisionType.None;
    }
    return rotatedFace;
  }

  public TrileInstance Clone()
  {
    TrileInstance trileInstance = new TrileInstance(this.Position, this.TrileId)
    {
      ActorSettings = new InstanceActorSettings(this.ActorSettings),
      TrileId = this.TrileId,
      Phi = this.Phi
    };
    if (this.Overlaps)
    {
      trileInstance.OverlappedTriles = new List<TrileInstance>();
      foreach (TrileInstance overlappedTrile in this.OverlappedTriles)
        trileInstance.OverlappedTriles.Add(overlappedTrile.Clone());
    }
    return trileInstance;
  }
}
