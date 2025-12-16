// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.BackgroundPlane
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Services;
using FezEngine.Structure.Geometry;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure;

public class BackgroundPlane
{
  private IContentManagerProvider CMProvider;
  private ILevelManager LevelManager;
  private ILevelMaterializer LevelMaterializer;
  private Color filter = Color.White;
  private Vector3 position;
  private Vector3 scale = new Vector3(1f);
  private int actualWidth;
  private int actualHeight;
  private Quaternion rotation = Quaternion.Identity;
  private float opacity = 1f;
  private bool lightMap;
  private bool allowOverbrightness;
  private bool fullbright;
  private bool pixelatedLightmap;
  private bool doublesided;
  private bool crosshatch;
  private bool billboard;
  private bool alwaysOnTop;
  private bool xTextureRepeat;
  private bool yTextureRepeat;
  private bool clampTexture;
  private bool drawDirty = true;
  private bool boundsDirty = true;
  private AnimatedTexture animation;
  [Serialization(Ignore = true)]
  public BoundingBox Bounds;
  private bool visible;

  [Serialization(Ignore = true)]
  public int Id { get; set; }

  public Group Group { get; private set; }

  [Serialization(Ignore = true)]
  public ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix> Geometry { get; private set; }

  public Mesh HostMesh { private get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public ActorType ActorType { get; set; }

  [Serialization(Ignore = true)]
  public int InstanceIndex { get; private set; }

  public BackgroundPlane()
  {
    this.Loop = true;
    this.Visible = true;
    this.Orientation = FaceOrientation.Front;
    this.OriginalRotation = Quaternion.Identity;
  }

  public BackgroundPlane(Mesh hostMesh, AnimatedTexture animation)
    : this()
  {
    BackgroundPlane backgroundPlane = this;
    this.HostMesh = hostMesh;
    this.Timing = animation.Timing.Clone();
    if (animation.Texture == null)
      DrawActionScheduler.Schedule((Action) (() => backgroundPlane.Texture = (Texture) animation.Texture));
    else
      this.Texture = (Texture) animation.Texture;
    this.Animated = true;
    this.animation = animation;
    this.actualWidth = animation.FrameWidth;
    this.actualHeight = animation.FrameHeight;
    this.Initialize();
  }

  public BackgroundPlane(Mesh hostMesh, Texture texture)
    : this()
  {
    this.HostMesh = hostMesh;
    this.Texture = texture;
    this.Animated = false;
    this.Initialize();
  }

  public BackgroundPlane(Mesh hostMesh, string textureName, bool animated)
    : this()
  {
    this.HostMesh = hostMesh;
    this.TextureName = textureName;
    this.Animated = animated;
    this.Initialize();
  }

  public void Initialize()
  {
    if (ServiceHelper.IsFull)
    {
      this.CMProvider = ServiceHelper.Get<IContentManagerProvider>();
      this.LevelManager = ServiceHelper.Get<ILevelManager>();
      this.LevelMaterializer = ServiceHelper.Get<ILevelMaterializer>();
    }
    if (this.Animated)
    {
      if (this.animation == null)
      {
        AnimatedTexture newAnim = this.CMProvider.CurrentLevel.Load<AnimatedTexture>("Background Planes/" + this.TextureName);
        this.animation = newAnim;
        this.Timing = this.animation.Timing.Clone();
        if (newAnim.Texture == null)
          DrawActionScheduler.Schedule((Action) (() => this.Texture = (Texture) newAnim.Texture));
        else
          this.Texture = (Texture) newAnim.Texture;
        this.actualWidth = this.animation.FrameWidth;
        this.actualHeight = this.animation.FrameHeight;
      }
      this.Timing.Loop = true;
      this.Timing.RandomizeStep();
      this.Size = new Vector3((float) this.actualWidth / 16f, (float) this.actualHeight / 16f, 0.125f);
      this.InitializeGroup();
    }
    else if (this.Texture == null)
    {
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.Texture = (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>("Background Planes/" + this.TextureName);
        if (this.TextureName.StartsWith("ZU_HOUSE_QR_A"))
        {
          this.XboxTexture = this.Texture;
          this.SonyTexture = (Texture) this.CMProvider.CurrentLevel.Load<Texture2D>($"Background Planes/{this.TextureName}_SONY");
          GamepadState.OnLayoutChanged += new EventHandler(this.UpdateControllerTexture);
          if (GamepadState.Layout != GamepadState.GamepadLayout.Xbox360)
            this.Texture = this.SonyTexture;
        }
        this.Size = new Vector3((float) (this.Texture as Texture2D).Width / 16f, (float) (this.Texture as Texture2D).Height / 16f, 0.125f);
        this.InitializeGroup();
      }));
    }
    else
    {
      this.Size = new Vector3((float) (this.Texture as Texture2D).Width / 16f, (float) (this.Texture as Texture2D).Height / 16f, 0.125f);
      this.InitializeGroup();
    }
  }

  private void InitializeGroup()
  {
    if (this.Group != null)
      this.DestroyInstancedGroup();
    BackgroundPlane backgroundPlane = (BackgroundPlane) null;
    lock (this.LevelManager.BackgroundPlanes)
      backgroundPlane = this.LevelManager.BackgroundPlanes.Values.FirstOrDefault<BackgroundPlane>((Func<BackgroundPlane, bool>) (x => x != this && x.Animated == this.Animated && x.doublesided == this.doublesided && x.crosshatch == this.crosshatch && (this.Texture != null && x.Texture == this.Texture || this.TextureName != null && x.TextureName == this.TextureName) && x.Group != null && x.clampTexture == this.clampTexture && x.lightMap == this.lightMap && x.allowOverbrightness == this.allowOverbrightness && x.pixelatedLightmap == this.pixelatedLightmap));
    if (backgroundPlane == null)
    {
      Group groupCopy = this.Group = this.HostMesh.AddFace(this.Size, Vector3.Zero, FaceOrientation.Front, true, this.doublesided, this.crosshatch);
      this.Geometry = new ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix>(PrimitiveType.TriangleList, 58);
      this.Geometry.Vertices = ((IEnumerable<FezVertexPositionNormalTexture>) (this.Group.Geometry as IndexedUserPrimitives<FezVertexPositionNormalTexture>).Vertices).Select<FezVertexPositionNormalTexture, VertexPositionNormalTextureInstance>((Func<FezVertexPositionNormalTexture, VertexPositionNormalTextureInstance>) (x => new VertexPositionNormalTextureInstance()
      {
        Position = x.Position,
        Normal = x.Normal,
        TextureCoordinate = x.TextureCoordinate
      })).ToArray<VertexPositionNormalTextureInstance>();
      this.Geometry.Indices = ((IEnumerable<int>) (this.Group.Geometry as IndexedUserPrimitives<FezVertexPositionNormalTexture>).Indices).ToArray<int>();
      this.Geometry.PredictiveBatchSize = 1;
      this.Group.Geometry = (IIndexedPrimitiveCollection) this.Geometry;
      this.Geometry.Instances = new Matrix[4];
      if (this.Texture == null)
        DrawActionScheduler.Schedule((Action) (() => groupCopy.Texture = this.Texture));
      else
        this.Group.Texture = this.Texture;
      if (this.Animated)
        DrawActionScheduler.Schedule((Action) (() => groupCopy.CustomData = (object) new Vector2((float) this.animation.Offsets[0].Width / (float) this.animation.Texture.Width, (float) this.animation.Offsets[0].Height / (float) this.animation.Texture.Height)));
    }
    else
    {
      this.Group = backgroundPlane.Group;
      this.Geometry = backgroundPlane.Geometry;
    }
    this.InstanceIndex = this.Geometry.InstanceCount++;
    this.UpdateGroupSetings();
  }

  private void DestroyInstancedGroup()
  {
    int num = 0;
    foreach (BackgroundPlane backgroundPlane in (IEnumerable<BackgroundPlane>) this.LevelManager.BackgroundPlanes.Values)
    {
      if (backgroundPlane != this && backgroundPlane.Group == this.Group)
      {
        backgroundPlane.InstanceIndex = num++;
        backgroundPlane.drawDirty = true;
        backgroundPlane.Update();
      }
    }
    if (this.Geometry == null)
      return;
    this.Geometry.InstanceCount = num;
    if (num == 0 && this.Group != null)
    {
      this.Geometry = (ShaderInstancedIndexedPrimitives<VertexPositionNormalTextureInstance, Matrix>) null;
      if (this.Animated)
        this.LevelMaterializer.AnimatedPlanesMesh.RemoveGroup(this.Group);
      else
        this.LevelMaterializer.StaticPlanesMesh.RemoveGroup(this.Group);
    }
    this.Group = (Group) null;
  }

  public void Update()
  {
    if (!this.drawDirty || this.Geometry == null)
      return;
    if (this.Geometry.Instances.Length < this.Geometry.InstanceCount)
      Array.Resize<Matrix>(ref this.Geometry.Instances, this.Geometry.InstanceCount + 4);
    this.Geometry.UpdateBuffers();
    Vector3 vector3_1 = this.Visible ? this.Position : new Vector3(float.MinValue);
    Vector3 vector3_2 = this.Filter.ToVector3();
    int m34 = (this.fullbright ? 1 : 0) | (this.clampTexture ? 2 : 0) | (this.xTextureRepeat ? 4 : 0) | (this.yTextureRepeat ? 8 : 0);
    Vector2 zero = Vector2.Zero;
    if (this.Animated)
    {
      if (this.animation.Texture == null)
        return;
      int frame = this.Timing.Frame;
      zero.X = (float) this.animation.Offsets[frame].X / (float) this.animation.Texture.Width;
      zero.Y = (float) this.animation.Offsets[frame].Y / (float) this.animation.Texture.Height;
    }
    this.Group.NoAlphaWrite = (double) this.opacity == 0.0 || (double) this.opacity == 1.0 ? new bool?() : new bool?(true);
    this.Geometry.Instances[this.InstanceIndex] = new Matrix(vector3_1.X, vector3_1.Y, vector3_1.Z, zero.X, this.Rotation.X, this.Rotation.Y, this.Rotation.Z, this.Rotation.W, this.Scale.X, this.Scale.Y, zero.Y, (float) m34, vector3_2.X, vector3_2.Y, vector3_2.Z, this.Opacity);
    this.Geometry.InstancesDirty = true;
    this.drawDirty = false;
  }

  [Serialization(Ignore = true)]
  public bool Visible
  {
    get => this.visible;
    set
    {
      this.drawDirty |= this.visible != value;
      this.visible = value;
    }
  }

  [Serialization(Ignore = true)]
  public bool Hidden { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Animated { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Billboard
  {
    get => this.billboard;
    set
    {
      if (this.billboard && !value)
        this.Rotation = Quaternion.Identity;
      this.billboard = value;
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool SyncWithSamples { get; set; }

  [Serialization(Ignore = true)]
  public AnimationTiming Timing { get; set; }

  [Serialization(Ignore = true)]
  public bool Loop { get; set; }

  [Serialization(Ignore = true)]
  public Vector3 Forward { get; private set; }

  [Serialization(Ignore = true)]
  public Vector3? OriginalPosition { get; set; }

  [Serialization(Ignore = true)]
  public Quaternion OriginalRotation { get; set; }

  public Vector3 Position
  {
    get => this.position;
    set
    {
      this.position = value;
      this.drawDirty = this.boundsDirty = true;
    }
  }

  [Serialization(Ignore = true)]
  public FaceOrientation Orientation { get; private set; }

  [Serialization(Optional = true)]
  public Quaternion Rotation
  {
    get => this.rotation;
    set
    {
      this.rotation = value;
      this.drawDirty = this.boundsDirty = true;
      this.Orientation = FezMath.OrientationFromDirection(FezMath.AlmostClamp(Vector3.Transform(Vector3.UnitZ, this.rotation)));
      this.Forward = Vector3.Transform(Vector3.Forward, this.rotation).Round();
    }
  }

  [Serialization(Optional = true)]
  public Vector3 Scale
  {
    get => this.scale;
    set
    {
      this.scale = value;
      this.drawDirty = this.boundsDirty = true;
    }
  }

  public Vector3 Size { get; set; }

  public string TextureName { get; set; }

  [Serialization(Ignore = true)]
  public Texture Texture { get; set; }

  [Serialization(Ignore = true)]
  private Texture XboxTexture { get; set; }

  [Serialization(Ignore = true)]
  private Texture SonyTexture { get; set; }

  private void UpdateControllerTexture(object sender, EventArgs e)
  {
    if (GamepadState.Layout == GamepadState.GamepadLayout.Xbox360)
      this.Group.Texture = this.XboxTexture;
    else
      this.Group.Texture = this.SonyTexture;
  }

  [Serialization(Optional = true)]
  public float Opacity
  {
    get => this.opacity;
    set
    {
      this.opacity = value;
      this.drawDirty = true;
    }
  }

  [Serialization(Optional = true)]
  public float ParallaxFactor { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool LightMap
  {
    get => this.lightMap;
    set
    {
      this.lightMap = value;
      if (this.Group == null)
        return;
      this.InitializeGroup();
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool AllowOverbrightness
  {
    get => this.allowOverbrightness;
    set
    {
      this.allowOverbrightness = value;
      if (this.Group == null)
        return;
      this.InitializeGroup();
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Doublesided
  {
    get => this.doublesided;
    set
    {
      this.doublesided = value;
      if (this.Group == null)
        return;
      this.InitializeGroup();
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Crosshatch
  {
    get => this.crosshatch;
    set
    {
      this.crosshatch = value;
      if (this.Group == null)
        return;
      this.InitializeGroup();
    }
  }

  [Serialization(Optional = true)]
  public int? AttachedGroup { get; set; }

  [Serialization(Optional = true)]
  public int? AttachedPlane { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public Color Filter
  {
    get => this.filter;
    set
    {
      this.filter = value;
      this.drawDirty = true;
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool AlwaysOnTop
  {
    get => this.alwaysOnTop;
    set
    {
      this.alwaysOnTop = value;
      if (this.Group == null)
        return;
      this.InitializeGroup();
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool Fullbright
  {
    get => this.fullbright;
    set
    {
      this.fullbright = value;
      this.drawDirty = true;
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool PixelatedLightmap
  {
    get => this.pixelatedLightmap;
    set
    {
      this.pixelatedLightmap = value;
      if (this.Group == null)
        return;
      this.InitializeGroup();
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool ClampTexture
  {
    get => this.clampTexture;
    set
    {
      this.clampTexture = value;
      if (this.Group == null)
        return;
      this.InitializeGroup();
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool XTextureRepeat
  {
    get => this.xTextureRepeat;
    set
    {
      this.xTextureRepeat = value;
      this.drawDirty = true;
    }
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool YTextureRepeat
  {
    get => this.yTextureRepeat;
    set
    {
      this.yTextureRepeat = value;
      this.drawDirty = true;
    }
  }

  public void UpdateBounds()
  {
    if (!this.boundsDirty)
      return;
    Vector3 vector3 = Vector3.Transform(this.Size / 2f * this.scale, this.rotation).Abs();
    this.Bounds = FezMath.Enclose(this.position - vector3, this.position + vector3);
    this.boundsDirty = false;
  }

  public void MarkDirty() => this.drawDirty = true;

  public void UpdateGroupSetings()
  {
    this.Group.Blending = !this.lightMap ? new BlendingMode?(BlendingMode.Alphablending) : new BlendingMode?(this.allowOverbrightness ? BlendingMode.Additive : BlendingMode.Maximum);
    this.Group.SamplerState = !this.lightMap || this.pixelatedLightmap ? (this.clampTexture || !this.xTextureRepeat && !this.yTextureRepeat ? SamplerState.PointClamp : SamplerState.PointWrap) : (this.clampTexture || !this.xTextureRepeat && !this.yTextureRepeat ? SamplerState.LinearClamp : SamplerState.LinearWrap);
    this.drawDirty = true;
  }

  [Serialization(Ignore = true)]
  public bool Disposed { get; set; }

  public void Dispose()
  {
    this.DestroyInstancedGroup();
    this.Disposed = true;
  }

  public BackgroundPlane Clone()
  {
    BackgroundPlane backgroundPlane = new BackgroundPlane();
    backgroundPlane.HostMesh = this.HostMesh;
    backgroundPlane.Animated = this.Animated;
    backgroundPlane.TextureName = this.TextureName;
    backgroundPlane.Texture = this.Texture;
    backgroundPlane.Timing = this.Timing == null ? (AnimationTiming) null : this.Timing.Clone();
    backgroundPlane.Position = this.position;
    backgroundPlane.Rotation = this.rotation;
    backgroundPlane.Scale = this.scale;
    backgroundPlane.LightMap = this.lightMap;
    backgroundPlane.AllowOverbrightness = this.allowOverbrightness;
    backgroundPlane.Filter = this.filter;
    backgroundPlane.Doublesided = this.doublesided;
    backgroundPlane.Opacity = this.opacity;
    backgroundPlane.Crosshatch = this.crosshatch;
    backgroundPlane.SyncWithSamples = this.SyncWithSamples;
    backgroundPlane.AlwaysOnTop = this.alwaysOnTop;
    backgroundPlane.Fullbright = this.fullbright;
    backgroundPlane.Loop = this.Loop;
    backgroundPlane.Billboard = this.billboard;
    backgroundPlane.AttachedGroup = this.AttachedGroup;
    backgroundPlane.YTextureRepeat = this.YTextureRepeat;
    backgroundPlane.XTextureRepeat = this.XTextureRepeat;
    backgroundPlane.ClampTexture = this.ClampTexture;
    backgroundPlane.AttachedPlane = this.AttachedPlane;
    backgroundPlane.ActorType = this.ActorType;
    backgroundPlane.ParallaxFactor = this.ParallaxFactor;
    backgroundPlane.Initialize();
    return backgroundPlane;
  }
}
