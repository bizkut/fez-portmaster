// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.BaseEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects.Structures;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;

#nullable disable
namespace FezEngine.Effects;

public abstract class BaseEffect : IDisposable
{
  internal FogEffectStructure fog;
  protected SemanticMappedSingle aspectRatio;
  protected SemanticMappedSingle time;
  protected SemanticMappedVector3 eye;
  protected SemanticMappedVector3 baseAmbient;
  protected SemanticMappedVector2 texelOffset;
  protected SemanticMappedVector3 diffuseLight;
  protected SemanticMappedVector3 eyeSign;
  protected SemanticMappedVector3 levelCenter;
  protected Matrix viewProjection;
  private Stopwatch stopWatch;
  internal readonly MatricesEffectStructure matrices;
  internal readonly MaterialEffectStructure material;
  protected Effect effect;
  protected EffectPass currentPass;
  protected EffectTechnique currentTechnique;
  protected bool SimpleGroupPrepare;
  protected bool SimpleMeshPrepare;
  public bool IsDisposed;
  public static readonly object DeviceLock = new object();
  private static Vector3 sharedEyeSign;
  private static Vector3 sharedLevelCenter;
  private static bool useHardwareInstancing = false;
  protected bool textureMatrixDirty;
  public bool IgnoreCache;

  public static event Action InstancingModeChanged;

  public static bool UseHardwareInstancing
  {
    get => BaseEffect.useHardwareInstancing;
    set
    {
      int num = BaseEffect.useHardwareInstancing == value ? 0 : (BaseEffect.InstancingModeChanged != null ? 1 : 0);
      BaseEffect.useHardwareInstancing = value;
      if (num == 0)
        return;
      Logger.Log("Instancing", LogSeverity.Information, "Hardware instancing is now " + (BaseEffect.useHardwareInstancing ? "enabled" : "disabled"));
      BaseEffect.InstancingModeChanged();
    }
  }

  protected BaseEffect(string effectName)
    : this(effectName, false)
  {
  }

  protected BaseEffect(string effectName, bool skipClone)
  {
    ServiceHelper.InjectServices((object) this);
    this.effect = this.CMProvider.Global.Load<Effect>("Effects/" + effectName);
    if (!skipClone)
    {
      this.TryCloneEffect(this.effect);
      while (this.currentPass == null)
      {
        Logger.Log("Effect", LogSeverity.Warning, "Could not validate effect " + effectName);
        this.TryCloneEffect(this.effect);
      }
    }
    else
    {
      this.currentTechnique = this.effect.Techniques[0];
      this.currentPass = this.currentTechnique.Passes[0];
    }
    this.matrices = new MatricesEffectStructure(this.effect.Parameters);
    this.material = new MaterialEffectStructure(this.effect.Parameters);
    this.Initialize();
  }

  private void TryCloneEffect(Effect sharedEffect)
  {
    this.effect = sharedEffect.Clone();
    using (List<EffectTechnique>.Enumerator enumerator = this.effect.Techniques.GetEnumerator())
    {
      if (!enumerator.MoveNext())
        return;
      EffectTechnique current = enumerator.Current;
      this.currentTechnique = current;
      this.currentPass = current.Passes[0];
    }
  }

  public virtual BaseEffect Clone() => throw new NotImplementedException();

  public void Dispose()
  {
    if (this.IsDisposed)
      return;
    this.effect = (Effect) null;
    this.currentPass = (EffectPass) null;
    this.currentTechnique = (EffectTechnique) null;
    this.IsDisposed = true;
    this.EngineState.PauseStateChanged -= new Action(this.CheckPause);
    this.CameraProvider.ViewChanged -= new Action(this.RefreshViewProjection);
    this.CameraProvider.ProjectionChanged -= new Action(this.RefreshViewProjection);
    this.CameraProvider.ViewChanged -= new Action(this.RefreshCenterPosition);
    this.CameraProvider.ProjectionChanged -= new Action(this.RefreshAspectRatio);
    this.FogProvider.FogSettingsChanged -= new Action(this.RefreshFog);
    this.LevelManager.LightingChanged -= new Action(this.RefreshLighting);
    this.GraphicsDeviceService.DeviceReset -= new EventHandler<EventArgs>(this.RefreshTexelSize);
    if (this.stopWatch != null)
      this.stopWatch.Stop();
    this.stopWatch = (Stopwatch) null;
  }

  private void Initialize()
  {
    this.fog = new FogEffectStructure(this.effect.Parameters);
    this.aspectRatio = new SemanticMappedSingle(this.effect.Parameters, "AspectRatio");
    this.texelOffset = new SemanticMappedVector2(this.effect.Parameters, "TexelOffset");
    this.time = new SemanticMappedSingle(this.effect.Parameters, "Time");
    this.baseAmbient = new SemanticMappedVector3(this.effect.Parameters, "BaseAmbient");
    this.eye = new SemanticMappedVector3(this.effect.Parameters, "Eye");
    this.diffuseLight = new SemanticMappedVector3(this.effect.Parameters, "DiffuseLight");
    this.eyeSign = new SemanticMappedVector3(this.effect.Parameters, "EyeSign");
    this.levelCenter = new SemanticMappedVector3(this.effect.Parameters, "LevelCenter");
    this.stopWatch = Stopwatch.StartNew();
    this.EngineState.PauseStateChanged += new Action(this.CheckPause);
    this.CameraProvider.ViewChanged += new Action(this.RefreshViewProjection);
    this.CameraProvider.ProjectionChanged += new Action(this.RefreshViewProjection);
    this.RefreshViewProjection();
    this.CameraProvider.ViewChanged += new Action(this.RefreshCenterPosition);
    this.RefreshCenterPosition();
    this.CameraProvider.ProjectionChanged += new Action(this.RefreshAspectRatio);
    this.RefreshAspectRatio();
    this.FogProvider.FogSettingsChanged += new Action(this.RefreshFog);
    this.RefreshFog();
    this.LevelManager.LightingChanged += new Action(this.RefreshLighting);
    this.RefreshLighting();
    this.GraphicsDeviceService.DeviceReset += new EventHandler<EventArgs>(this.RefreshTexelSize);
    this.RefreshTexelSize();
    this.eyeSign.Set(BaseEffect.sharedEyeSign);
    this.levelCenter.Set(BaseEffect.sharedLevelCenter);
  }

  private void RefreshTexelSize(object sender, EventArgs ea) => this.RefreshTexelSize();

  private void RefreshTexelSize()
  {
    this.texelOffset.Set(new Vector2(-0.5f / (float) this.GraphicsDeviceService.GraphicsDevice.Viewport.Width, 0.5f / (float) this.GraphicsDeviceService.GraphicsDevice.Viewport.Height));
  }

  private void RefreshLighting()
  {
    this.baseAmbient.Set(this.LevelManager.ActualAmbient.ToVector3());
    this.diffuseLight.Set(this.LevelManager.ActualDiffuse.ToVector3());
  }

  private void CheckPause()
  {
    if (this.stopWatch.IsRunning && this.EngineState.Paused)
    {
      this.stopWatch.Stop();
    }
    else
    {
      if (this.stopWatch.IsRunning || this.EngineState.Paused)
        return;
      this.stopWatch.Start();
    }
  }

  public static Vector3 EyeSign
  {
    set => BaseEffect.sharedEyeSign = value;
  }

  public static Vector3 LevelCenter
  {
    set => BaseEffect.sharedLevelCenter = value;
  }

  private void RefreshFog()
  {
    this.fog.FogType = this.FogProvider.Type;
    this.fog.FogColor = this.FogProvider.Color;
    if (this.EngineState.InEditor)
      this.fog.FogDensity = this.FogProvider.Density;
    else
      this.fog.FogDensity = this.FogProvider.Density * 1.25f;
  }

  private void RefreshViewProjection()
  {
    this.viewProjection = this.CameraProvider.View * this.CameraProvider.Projection;
    this.matrices.ViewProjection = this.viewProjection;
  }

  private void RefreshCenterPosition() => this.eye.Set(this.CameraProvider.InverseView.Forward);

  private void RefreshAspectRatio() => this.aspectRatio.Set(this.CameraProvider.AspectRatio);

  public virtual void Prepare(Mesh mesh)
  {
    this.eyeSign.Set(BaseEffect.sharedEyeSign);
    this.levelCenter.Set(BaseEffect.sharedLevelCenter);
    this.time.Set((float) this.stopWatch.Elapsed.TotalSeconds);
    if (mesh.TextureMatrix.Dirty || this.IgnoreCache)
    {
      this.matrices.TextureMatrix = (Matrix) mesh.TextureMatrix;
      mesh.TextureMatrix.Clean();
    }
    if (this.SimpleMeshPrepare)
      this.matrices.WorldViewProjection = this.viewProjection;
    else if (this.SimpleGroupPrepare)
    {
      Matrix matrix1 = this.viewProjection;
      Matrix? nullable;
      if (this.ForcedViewMatrix.HasValue)
      {
        nullable = this.ForcedProjectionMatrix;
        if (!nullable.HasValue)
        {
          nullable = this.ForcedViewMatrix;
          matrix1 = nullable.Value * this.CameraProvider.Projection;
          goto label_14;
        }
      }
      nullable = this.ForcedViewMatrix;
      if (!nullable.HasValue)
      {
        nullable = this.ForcedProjectionMatrix;
        if (nullable.HasValue)
        {
          Matrix view = this.CameraProvider.View;
          nullable = this.ForcedProjectionMatrix;
          Matrix matrix2 = nullable.Value;
          matrix1 = view * matrix2;
          goto label_14;
        }
      }
      nullable = this.ForcedViewMatrix;
      if (nullable.HasValue)
      {
        nullable = this.ForcedProjectionMatrix;
        if (nullable.HasValue)
        {
          nullable = this.ForcedViewMatrix;
          Matrix matrix3 = nullable.Value;
          nullable = this.ForcedProjectionMatrix;
          Matrix matrix4 = nullable.Value;
          matrix1 = matrix3 * matrix4;
        }
      }
label_14:
      this.matrices.WorldViewProjection = mesh.WorldMatrix * matrix1;
    }
    else
      this.material.Opacity = mesh.Material.Opacity;
  }

  public virtual void Prepare(Group group)
  {
    if (this.SimpleGroupPrepare)
      return;
    Matrix matrix1 = this.viewProjection;
    Matrix? nullable = this.ForcedViewMatrix;
    if (nullable.HasValue)
    {
      nullable = this.ForcedProjectionMatrix;
      if (!nullable.HasValue)
      {
        nullable = this.ForcedViewMatrix;
        matrix1 = nullable.Value * this.CameraProvider.Projection;
        goto label_11;
      }
    }
    nullable = this.ForcedViewMatrix;
    if (!nullable.HasValue)
    {
      nullable = this.ForcedProjectionMatrix;
      if (nullable.HasValue)
      {
        Matrix view = this.CameraProvider.View;
        nullable = this.ForcedProjectionMatrix;
        Matrix matrix2 = nullable.Value;
        matrix1 = view * matrix2;
        goto label_11;
      }
    }
    nullable = this.ForcedViewMatrix;
    if (nullable.HasValue)
    {
      nullable = this.ForcedProjectionMatrix;
      if (nullable.HasValue)
      {
        nullable = this.ForcedViewMatrix;
        Matrix matrix3 = nullable.Value;
        nullable = this.ForcedProjectionMatrix;
        Matrix matrix4 = nullable.Value;
        matrix1 = matrix3 * matrix4;
      }
    }
label_11:
    this.matrices.WorldViewProjection = group.WorldMatrix.Value * matrix1;
    this.material.Opacity = group.Material != null ? group.Material.Opacity : group.Mesh.Material.Opacity;
    if (group.TextureMatrix.Value.HasValue)
    {
      this.matrices.TextureMatrix = group.TextureMatrix.Value.Value;
      this.textureMatrixDirty = true;
    }
    else
    {
      if (!this.textureMatrixDirty)
        return;
      this.matrices.TextureMatrix = Matrix.Identity;
      this.textureMatrixDirty = false;
    }
  }

  public EffectPass CurrentPass => this.currentPass;

  public Matrix? ForcedViewMatrix { get; set; }

  public Matrix? ForcedProjectionMatrix { get; set; }

  public void Apply() => this.currentPass.Apply();

  [ServiceDependency]
  public IEngineStateManager EngineState { protected get; set; }

  [ServiceDependency]
  public IGraphicsDeviceService GraphicsDeviceService { protected get; set; }

  [ServiceDependency]
  public IDefaultCameraManager CameraProvider { protected get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { protected get; set; }

  [ServiceDependency]
  public IFogManager FogProvider { protected get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { protected get; set; }
}
