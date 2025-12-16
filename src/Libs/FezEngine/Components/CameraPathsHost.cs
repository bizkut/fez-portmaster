// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.CameraPathsHost
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Components;

public class CameraPathsHost : GameComponent
{
  private readonly List<CameraPathsHost.CameraPathState> trackedPaths = new List<CameraPathsHost.CameraPathState>();

  public CameraPathsHost(Game game)
    : base(game)
  {
    this.UpdateOrder = -2;
  }

  private void TrackNewPaths()
  {
    this.trackedPaths.Clear();
    foreach (MovementPath path in (IEnumerable<MovementPath>) this.LevelManager.Paths.Values)
      this.trackedPaths.Add(new CameraPathsHost.CameraPathState(path));
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TrackNewPaths);
  }

  public override void Update(GameTime gameTime)
  {
    if (this.EngineState.Loading || this.EngineState.TimePaused)
      return;
    if (this.trackedPaths.Count != this.LevelManager.Paths.Count)
      this.TrackNewPaths();
    if (this.trackedPaths.Count == 0)
      return;
    foreach (CameraPathsHost.CameraPathState trackedPath in this.trackedPaths)
      trackedPath.Update(gameTime.ElapsedGameTime);
  }

  [ServiceDependency]
  public ILevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IEngineStateManager EngineState { private get; set; }

  private class CameraPathState
  {
    private readonly MovementPath Path;
    private readonly List<PathSegment> Nodes;
    private TimeSpan sinceSegmentStarted;
    private int nodeIndex;
    private Viewpoint originalViewpoint;
    private Viewpoint firstNodeViewpoint;
    private float originalPixelsPerTrixel;
    private Vector3 originalCenter;
    private Vector3 originalDirection;
    private float originalRadius;
    private bool justStarted;

    private bool Enabled { get; set; }

    public CameraPathState(MovementPath path)
    {
      ServiceHelper.InjectServices((object) this);
      this.Path = path;
      this.Nodes = path.Segments;
      this.Enabled = true;
      foreach (PathSegment node in this.Nodes)
      {
        CameraNodeData customData = node.CustomData as CameraNodeData;
        if (customData.SoundName != null)
          node.Sound = this.CMProvider.CurrentLevel.Load<SoundEffect>("Sounds/" + customData.SoundName);
      }
      this.Reset();
      this.StartNewSegment();
    }

    private PathSegment CurrentNode => this.Nodes[this.nodeIndex];

    private void Reset()
    {
      this.nodeIndex = 1;
      this.sinceSegmentStarted = TimeSpan.Zero;
      this.justStarted = true;
    }

    public void Update(TimeSpan elapsed)
    {
      if (!this.Enabled || this.Path.NeedsTrigger)
        return;
      if (this.justStarted)
      {
        this.originalViewpoint = this.CameraManager.Viewpoint;
        this.originalCenter = this.CameraManager.Center;
        this.originalDirection = this.CameraManager.Direction;
        this.originalPixelsPerTrixel = this.CameraManager.PixelsPerTrixel;
        this.originalRadius = this.CameraManager.Radius;
        bool perspective = (this.Nodes[0].CustomData as CameraNodeData).Perspective;
        if (this.Path.InTransition)
        {
          this.nodeIndex = 1;
          this.Nodes.Insert(0, new PathSegment()
          {
            Destination = this.originalCenter,
            Orientation = Quaternion.Inverse(this.CameraManager.Rotation),
            CustomData = (ICloneable) new CameraNodeData()
            {
              PixelsPerTrixel = (int) this.originalPixelsPerTrixel,
              Perspective = perspective
            }
          });
        }
        if (this.Path.OutTransition)
          this.Nodes.Add(new PathSegment()
          {
            Destination = this.originalCenter,
            Orientation = Quaternion.Inverse(this.CameraManager.Rotation),
            CustomData = (ICloneable) new CameraNodeData()
            {
              PixelsPerTrixel = (int) this.originalPixelsPerTrixel,
              Perspective = perspective
            }
          });
        if (this.Nodes.Count < 2)
        {
          this.EndPath();
          return;
        }
        CameraNodeData customData = this.Nodes[0].CustomData as CameraNodeData;
        this.firstNodeViewpoint = FezMath.OrientationFromDirection(Vector3.Transform(Vector3.Forward, this.Nodes[0].Orientation).MaxClampXZ()).AsViewpoint();
        this.CameraManager.ChangeViewpoint(customData.Perspective ? Viewpoint.Perspective : this.firstNodeViewpoint);
        if (customData.Perspective)
          this.CameraManager.Radius = 1f / 1000f;
        if (customData.PixelsPerTrixel != 0 && (double) this.CameraManager.PixelsPerTrixel != (double) customData.PixelsPerTrixel)
          this.CameraManager.PixelsPerTrixel = (float) customData.PixelsPerTrixel;
        this.StartNewSegment();
        this.justStarted = false;
      }
      if (this.CameraManager.ActionRunning)
        this.sinceSegmentStarted += elapsed;
      if (this.sinceSegmentStarted >= this.CurrentNode.Duration + this.CurrentNode.WaitTimeOnFinish)
        this.ChangeSegment();
      if (!this.Enabled || this.Path.NeedsTrigger)
        return;
      float linearStep = (float) FezMath.Saturate(this.sinceSegmentStarted.TotalSeconds / this.CurrentNode.Duration.TotalSeconds);
      float amount = (double) this.CurrentNode.Deceleration != 0.0 || (double) this.CurrentNode.Acceleration != 0.0 ? ((double) this.CurrentNode.Acceleration != 0.0 ? ((double) this.CurrentNode.Deceleration != 0.0 ? Easing.EaseInOut((double) linearStep, EasingType.Sine, this.CurrentNode.Acceleration, EasingType.Sine, this.CurrentNode.Deceleration) : Easing.Ease((double) linearStep, this.CurrentNode.Acceleration, EasingType.Quadratic)) : Easing.Ease((double) linearStep, -this.CurrentNode.Deceleration, EasingType.Quadratic)) : linearStep;
      PathSegment node1 = this.Nodes[Math.Max(this.nodeIndex - 1, 0)];
      PathSegment currentNode = this.CurrentNode;
      Vector3 vector3_1;
      Quaternion rotation;
      if (this.Path.IsSpline)
      {
        PathSegment node2 = this.Nodes[Math.Max(this.nodeIndex - 2, 0)];
        PathSegment node3 = this.Nodes[Math.Min(this.nodeIndex + 1, this.Nodes.Count - 1)];
        vector3_1 = Vector3.CatmullRom(node2.Destination, node1.Destination, currentNode.Destination, node3.Destination, amount);
        rotation = Quaternion.Slerp(node1.Orientation, currentNode.Orientation, amount);
      }
      else
      {
        vector3_1 = Vector3.Lerp(node1.Destination, currentNode.Destination, amount);
        rotation = Quaternion.Slerp(node1.Orientation, currentNode.Orientation, amount);
      }
      float distance = MathHelper.Lerp(node1.JitterFactor, currentNode.JitterFactor, amount);
      if ((double) distance > 0.0)
        vector3_1 += new Vector3(RandomHelper.Centered((double) distance) * 0.5f, RandomHelper.Centered((double) distance) * 0.5f, RandomHelper.Centered((double) distance) * 0.5f);
      Vector3 vector3_2 = Vector3.Transform(Vector3.Forward, rotation);
      CameraNodeData customData1 = node1.CustomData as CameraNodeData;
      CameraNodeData customData2 = currentNode.CustomData as CameraNodeData;
      if (!customData2.Perspective)
        this.CameraManager.PixelsPerTrixel = MathHelper.Lerp(customData1.PixelsPerTrixel == 0 ? this.originalPixelsPerTrixel : (float) customData1.PixelsPerTrixel, customData2.PixelsPerTrixel == 0 ? this.originalPixelsPerTrixel : (float) customData2.PixelsPerTrixel, amount);
      Viewpoint view = customData2.Perspective ? Viewpoint.Perspective : this.firstNodeViewpoint;
      if (view != this.CameraManager.Viewpoint)
      {
        if (view == Viewpoint.Perspective)
          this.CameraManager.Radius = 1f / 1000f;
        this.CameraManager.ChangeViewpoint(view);
      }
      this.CameraManager.Center = vector3_1;
      this.CameraManager.Direction = vector3_2;
      if (!customData2.Perspective)
        return;
      if (this.nodeIndex == 1)
        this.CameraManager.Radius = MathHelper.Lerp(this.originalRadius, 1f / 1000f, amount);
      else if (this.nodeIndex == this.Nodes.Count - 1)
        this.CameraManager.Radius = MathHelper.Lerp(1f / 1000f, this.originalRadius, amount);
      else
        this.CameraManager.Radius = 1f / 1000f;
    }

    private void ChangeSegment()
    {
      this.sinceSegmentStarted -= this.CurrentNode.Duration + this.CurrentNode.WaitTimeOnFinish;
      if (this.CurrentNode.Sound != null)
        this.CurrentNode.Sound.EmitAt(this.CameraManager.Center, false, RandomHelper.Centered(0.075000002980232239), false);
      ++this.nodeIndex;
      if (this.nodeIndex == this.Nodes.Count || this.nodeIndex == -1)
        this.EndPath();
      if (this.Enabled && !this.Path.NeedsTrigger)
        this.StartNewSegment();
      if (!this.Path.RunSingleSegment)
        return;
      this.Path.NeedsTrigger = true;
      this.Path.RunSingleSegment = false;
    }

    private void StartNewSegment() => this.sinceSegmentStarted -= this.CurrentNode.WaitTimeOnStart;

    private void EndPath()
    {
      if (this.Path.InTransition)
        this.Nodes.RemoveAt(0);
      if (this.Path.OutTransition)
        this.Nodes.RemoveAt(this.Nodes.Count - 1);
      this.Path.NeedsTrigger = true;
      this.Path.RunOnce = false;
      this.CameraManager.ChangeViewpoint(this.originalViewpoint);
      this.Reset();
    }

    [ServiceDependency]
    public IDefaultCameraManager CameraManager { private get; set; }

    [ServiceDependency]
    public IDebuggingBag DebuggingBag { private get; set; }

    [ServiceDependency]
    public IContentManagerProvider CMProvider { private get; set; }
  }
}
