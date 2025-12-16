// Decompiled with JetBrains decompiler
// Type: FezGame.Components.TelescopeHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using FezGame.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class TelescopeHost : DrawableGameComponent
{
  private const float FadeSeconds = 0.75f;
  private const string Code = "RTLTLTRTRTLT";
  private readonly Dictionary<char, int[]> LetterValues = new Dictionary<char, int[]>(3)
  {
    {
      'R',
      new int[8]{ 0, 1, 0, 1, 0, 0, 1, 0 }
    },
    {
      'T',
      new int[8]{ 0, 1, 0, 1, 0, 1, 0, 0 }
    },
    {
      'L',
      new int[8]{ 0, 1, 0, 0, 1, 1, 0, 0 }
    }
  };
  private readonly Dictionary<Viewpoint, Texture2D> Textures = new Dictionary<Viewpoint, Texture2D>(3, (IEqualityComparer<Viewpoint>) ViewpointComparer.Default);
  private Texture2D Mask;
  private Texture2D Vignette;
  private ArtObjectInstance TelescopeAo;
  private Viewpoint NowViewing;
  private readonly Texture2D[] LeftTextures = new Texture2D[3];
  private readonly int[] Message;
  private float BitTimer;
  private int MessageIndex;
  private TimeSpan Fader;
  private TelescopeHost.StateType State;

  public TelescopeHost(Game game)
    : base(game)
  {
    this.DrawOrder = 100;
    this.Message = "RTLTLTRTRTLT".SelectMany<char, int>((Func<char, IEnumerable<int>>) (x => (IEnumerable<int>) this.LetterValues[x])).ToArray<int>();
  }

  public override void Initialize()
  {
    base.Initialize();
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.Textures.Clear();
    this.LeftTextures[0] = this.LeftTextures[1] = this.LeftTextures[2] = (Texture2D) null;
    this.Enabled = (this.TelescopeAo = this.LevelManager.ArtObjects.Values.FirstOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.Telescope))) != null;
    if (!this.Enabled)
      this.Visible = false;
    else
      DrawActionScheduler.Schedule((Action) (() =>
      {
        this.Textures.Add(Viewpoint.Front, this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_STARS_A"));
        this.Textures.Add(Viewpoint.Right, this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_STARS_B"));
        this.Textures.Add(Viewpoint.Back, this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_STARS_C"));
        this.LeftTextures[0] = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_STARS_D_0");
        this.LeftTextures[1] = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_STARS_D_1");
        this.LeftTextures[2] = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_STARS_D_PAUSE");
        this.Mask = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_MASK");
        this.Vignette = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/telescope/TELESCOPE_VIGNETTE");
      }));
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMap || this.GameState.InMenuCube || this.GameState.InFpsMode)
      return;
    SkyHost.Instance.RotateLayer(0, this.TelescopeAo.Rotation);
    SkyHost.Instance.RotateLayer(1, this.TelescopeAo.Rotation);
    SkyHost.Instance.RotateLayer(2, this.TelescopeAo.Rotation);
    SkyHost.Instance.RotateLayer(3, this.TelescopeAo.Rotation);
    if (this.State == TelescopeHost.StateType.Idle)
      this.CheckForUp();
    else if (this.State == TelescopeHost.StateType.Out)
    {
      this.Fader -= gameTime.ElapsedGameTime;
      this.Fader -= gameTime.ElapsedGameTime;
    }
    else
    {
      this.Fader += gameTime.ElapsedGameTime;
      this.CheckForExit();
    }
    if (this.Fader.TotalSeconds <= 0.75 && this.Fader.TotalSeconds >= 0.0)
      return;
    switch (this.State)
    {
      case TelescopeHost.StateType.In:
        this.State = TelescopeHost.StateType.VisibleWait;
        break;
      case TelescopeHost.StateType.Out:
        this.State = TelescopeHost.StateType.Idle;
        this.Visible = false;
        this.PlayerManager.CanControl = true;
        break;
    }
  }

  private void CheckForUp()
  {
    if (this.GameState.Loading || this.GameState.InMap || this.GameState.Paused || !this.CameraManager.ActionRunning || !this.PlayerManager.Grounded || this.InputManager.GrabThrow != FezButtonState.Pressed || this.CameraManager.Viewpoint != FezMath.OrientationFromDirection(FezMath.AlmostClamp(Vector3.Transform(Vector3.Left, this.TelescopeAo.Rotation))).AsViewpoint())
      return;
    Vector3 vector3 = Vector3.Transform(this.PlayerManager.Position - this.TelescopeAo.Position, this.TelescopeAo.Rotation);
    if (((double) Math.Abs(vector3.Z) >= 0.5 ? 0 : ((double) Math.Abs(vector3.Y) < 0.5 ? 1 : 0)) == 0)
      return;
    this.Visible = true;
    this.State = TelescopeHost.StateType.In;
    this.BitTimer = 0.0f;
    this.MessageIndex = -3;
    this.Fader = TimeSpan.Zero;
    this.NowViewing = this.CameraManager.Viewpoint;
    this.PlayerManager.CanControl = false;
    this.PlayerManager.Action = ActionType.ReadTurnAround;
    this.DotManager.Hidden = true;
    this.DotManager.PreventPoI = true;
  }

  private void CheckForExit()
  {
    if (this.InputManager.Back != FezButtonState.Pressed && this.InputManager.CancelTalk != FezButtonState.Pressed && this.InputManager.GrabThrow != FezButtonState.Pressed)
      return;
    this.State = TelescopeHost.StateType.Out;
    this.Fader = TimeSpan.FromSeconds(0.75);
    this.DotManager.Hidden = false;
    this.DotManager.PreventPoI = false;
  }

  public override void Draw(GameTime gameTime)
  {
    if (this.GameState.Paused || this.GameState.InFpsMode || this.GameState.Loading || this.GameState.InMap)
      return;
    float alpha = Easing.EaseOut(FezMath.Saturate(this.Fader.TotalSeconds / 0.75), EasingType.Sine);
    Texture2D texture2D;
    if (this.NowViewing == Viewpoint.Left)
    {
      this.BitTimer -= (float) gameTime.ElapsedGameTime.TotalSeconds;
      if ((double) this.BitTimer <= 0.0)
      {
        ++this.MessageIndex;
        if (this.MessageIndex > this.Message.Length + 2)
          this.MessageIndex = -1;
        this.BitTimer = this.MessageIndex < 0 || this.MessageIndex >= this.Message.Length || this.Message[this.MessageIndex] == 1 ? 0.75f : 0.5f;
      }
      texture2D = this.MessageIndex < 0 || this.MessageIndex >= this.Message.Length || (double) this.BitTimer < 0.25 ? this.LeftTextures[2] : this.LeftTextures[this.Message[this.MessageIndex]];
    }
    else
      texture2D = this.Textures[this.NowViewing];
    float width1 = (float) this.GraphicsDevice.Viewport.Width;
    float height1 = (float) this.GraphicsDevice.Viewport.Height;
    float width2 = (float) texture2D.Width;
    float height2 = (float) texture2D.Height;
    this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    float viewScale = this.GraphicsDevice.GetViewScale();
    Matrix textureMatrix = new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, -0.5f, -0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f) * new Matrix((float) ((double) width1 / (double) width2 / 2.0) / viewScale, 0.0f, 0.0f, 0.0f, 0.0f, (float) ((double) height1 / (double) height2 / 2.0) / viewScale, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f) * new Matrix(1f, 0.0f, 0.0f, 0.0f, 0.0f, 1f, 0.0f, 0.0f, 0.5f, 0.5f, 1f, 0.0f, 0.0f, 0.0f, 0.0f, 0.0f);
    GraphicsDevice graphicsDevice = this.GraphicsDevice;
    graphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.CutsceneWipe));
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.None);
    this.TargetRenderer.DrawFullscreen((Texture) this.Mask, textureMatrix, new Color(1f, 1f, 1f, alpha));
    graphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.CutsceneWipe);
    graphicsDevice.SetColorWriteChannels(ColorWriteChannels.All);
    this.TargetRenderer.DrawFullscreen(new Color(0.0f, 0.0f, 0.0f, alpha));
    graphicsDevice.GetDssCombiner().StencilFunction = CompareFunction.NotEqual;
    float skyOpacity = this.GameState.SkyOpacity;
    this.GameState.SkyOpacity = alpha;
    graphicsDevice.SetBlendingMode(BlendingMode.Additive);
    SkyHost.Instance.DrawBackground();
    this.GameState.SkyOpacity = skyOpacity;
    this.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
    graphicsDevice.SetBlendingMode(BlendingMode.Alphablending);
    graphicsDevice.GetDssCombiner().StencilFunction = CompareFunction.Always;
    this.TargetRenderer.DrawFullscreen((Texture) this.Vignette, textureMatrix, new Color(1f, 1f, 1f, alpha * (1f - this.TimeManager.NightContribution)));
    graphicsDevice.GetDssCombiner().StencilFunction = CompareFunction.NotEqual;
    this.TargetRenderer.DrawFullscreen((Texture) texture2D, textureMatrix, new Color(1f, 1f, 1f, alpha * this.TimeManager.NightContribution));
  }

  [ServiceDependency]
  public ITimeManager TimeManager { get; set; }

  [ServiceDependency]
  public IDotManager DotManager { get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { get; set; }

  [ServiceDependency]
  public IInputManager InputManager { get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { get; set; }

  [ServiceDependency]
  public ILevelManager LevelManager { get; set; }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { get; set; }

  [ServiceDependency]
  public ITargetRenderingManager TargetRenderer { get; set; }

  private enum StateType
  {
    Idle,
    In,
    VisibleWait,
    Out,
  }
}
