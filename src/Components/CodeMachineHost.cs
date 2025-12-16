// Decompiled with JetBrains decompiler
// Type: FezGame.Components.CodeMachineHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Services;
using FezEngine.Services.Scripting;
using FezEngine.Structure;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using FezGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezGame.Components;

internal class CodeMachineHost(Game game) : GameComponent(game)
{
  private static readonly TimeSpan FadeOutDuration = TimeSpan.FromSeconds(0.10000000149011612);
  private static readonly TimeSpan FadeInDuration = TimeSpan.FromSeconds(0.20000000298023224);
  private static readonly TimeSpan Delay = TimeSpan.FromSeconds(0.033333335071802139);
  private static readonly TimeSpan TimeOut = TimeSpan.FromSeconds(2.0);
  private static readonly Dictionary<CodeInput, int[]> BitPatterns = new Dictionary<CodeInput, int[]>((IEqualityComparer<CodeInput>) CodeInputComparer.Default)
  {
    {
      CodeInput.Down,
      new int[36]
      {
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0
      }
    },
    {
      CodeInput.Up,
      new int[36]
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1
      }
    },
    {
      CodeInput.Left,
      new int[36]
      {
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1
      }
    },
    {
      CodeInput.Right,
      new int[36]
      {
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        0,
        0,
        0,
        0
      }
    },
    {
      CodeInput.SpinRight,
      new int[36]
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0
      }
    },
    {
      CodeInput.SpinLeft,
      new int[36]
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1
      }
    },
    {
      CodeInput.Jump,
      new int[36]
      {
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1,
        0,
        0,
        1,
        1,
        1,
        1
      }
    }
  };
  private ArtObjectInstance CodeMachineAO;
  private BackgroundPlane[] BitPlanes;
  private CodeMachineHost.BitState[] BitStates;
  private readonly List<CodeInput> Input = new List<CodeInput>();
  private TimeSpan SinceInput;
  private SoundEffect inputSound;
  private SoundEmitter inputEmitter;
  private bool needsInitialize;

  public override void Initialize()
  {
    base.Initialize();
    this.inputSound = this.CMProvider.Global.Load<SoundEffect>("Sounds/Zu/CodeMachineInput");
    this.LevelManager.LevelChanged += new Action(this.TryInitialize);
    this.TryInitialize();
  }

  private void TryInitialize()
  {
    this.CodeMachineAO = (ArtObjectInstance) null;
    this.BitPlanes = (BackgroundPlane[]) null;
    this.BitStates = (CodeMachineHost.BitState[]) null;
    this.Enabled = false;
    this.Input.Clear();
    this.Enabled = (this.CodeMachineAO = this.LevelManager.ArtObjects.Values.FirstOrDefault<ArtObjectInstance>((Func<ArtObjectInstance, bool>) (x => x.ArtObject.ActorType == ActorType.CodeMachine))) != null;
    if (!this.Enabled)
      return;
    this.BitPlanes = new BackgroundPlane[144 /*0x90*/];
    this.BitStates = new CodeMachineHost.BitState[144 /*0x90*/];
    this.needsInitialize = true;
  }

  public override void Update(GameTime gameTime)
  {
    if (this.GameState.Loading || this.GameState.Paused || this.GameState.InMenuCube || this.GameState.InMap)
      return;
    if (this.needsInitialize)
    {
      Texture2D texture2D = this.CMProvider.CurrentLevel.Load<Texture2D>("Other Textures/glow/code_machine_glowbit");
      for (int index1 = 0; index1 < 36; ++index1)
      {
        BackgroundPlane backgroundPlane1 = new BackgroundPlane(this.LevelMaterializer.StaticPlanesMesh, (Texture) texture2D)
        {
          Fullbright = true,
          Opacity = 0.0f,
          Rotation = Quaternion.Identity
        };
        this.BitPlanes[index1 * 4] = backgroundPlane1;
        BackgroundPlane backgroundPlane2 = backgroundPlane1.Clone();
        backgroundPlane2.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 1.57079637f);
        this.BitPlanes[index1 * 4 + 1] = backgroundPlane2;
        BackgroundPlane backgroundPlane3 = backgroundPlane1.Clone();
        backgroundPlane3.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 3.14159274f);
        this.BitPlanes[index1 * 4 + 2] = backgroundPlane3;
        BackgroundPlane backgroundPlane4 = backgroundPlane1.Clone();
        backgroundPlane4.Rotation = Quaternion.CreateFromAxisAngle(Vector3.Up, 4.712389f);
        this.BitPlanes[index1 * 4 + 3] = backgroundPlane4;
        int num1 = index1 % 6;
        int num2 = index1 / 6;
        for (int index2 = 0; index2 < 4; ++index2)
        {
          BackgroundPlane bitPlane = this.BitPlanes[index1 * 4 + index2];
          this.BitStates[index1 * 4 + index2] = new CodeMachineHost.BitState();
          Vector3 vector3_1 = Vector3.Transform(Vector3.UnitZ, bitPlane.Rotation);
          Vector3 vector3_2 = Vector3.Transform(Vector3.Right, bitPlane.Rotation);
          bitPlane.Position = this.CodeMachineAO.Position + vector3_1 * 1.5f + vector3_2 * (float) (num1 * 8 - 20) / 16f + Vector3.Up * (float) (35 - num2 * 8) / 16f;
          this.LevelManager.AddPlane(bitPlane);
        }
      }
      this.needsInitialize = false;
    }
    if (this.CameraManager.ViewTransitionReached)
      this.CheckInput();
    this.UpdateBits(gameTime.ElapsedGameTime);
    this.SinceInput += gameTime.ElapsedGameTime;
    if (!(this.SinceInput > CodeMachineHost.TimeOut))
      return;
    this.Input.Clear();
  }

  private void UpdateBits(TimeSpan elapsed)
  {
    for (int index1 = 0; index1 < 36; ++index1)
    {
      int num1 = index1 % 6;
      int num2 = index1 / 6;
      for (int index2 = 0; index2 < 4; ++index2)
      {
        BackgroundPlane bitPlane = this.BitPlanes[index1 * 4 + index2];
        CodeMachineHost.BitState bitState1 = this.BitStates[index1 * 4 + index2];
        TimeSpan timeSpan1 = CodeMachineHost.Delay;
        TimeSpan timeSpan2 = TimeSpan.FromTicks(timeSpan1.Ticks * (long) (num1 + num2));
        if (bitState1.On)
        {
          if (bitState1.SinceOn < CodeMachineHost.FadeInDuration)
          {
            bitState1.SinceOn += elapsed;
            BackgroundPlane backgroundPlane = bitPlane;
            double ticks1 = (double) bitState1.SinceOn.Ticks;
            timeSpan1 = CodeMachineHost.FadeInDuration;
            double ticks2 = (double) timeSpan1.Ticks;
            double num3 = (double) FezMath.Saturate((float) (ticks1 / ticks2));
            backgroundPlane.Opacity = (float) num3;
            CodeMachineHost.BitState bitState2 = bitState1;
            double num4 = 1.0 - (double) bitPlane.Opacity;
            timeSpan1 = CodeMachineHost.FadeOutDuration;
            double totalSeconds = timeSpan1.TotalSeconds;
            TimeSpan timeSpan3 = TimeSpan.FromSeconds(num4 * totalSeconds) - timeSpan2;
            bitState2.SinceOff = timeSpan3;
          }
          else if (bitState1.SinceIdle < CodeMachineHost.TimeOut)
            bitState1.SinceIdle += elapsed;
          else
            bitState1.On = false;
        }
        else if (bitState1.SinceOff < CodeMachineHost.FadeOutDuration)
        {
          bitState1.SinceOff += elapsed;
          BackgroundPlane backgroundPlane = bitPlane;
          double ticks3 = (double) bitState1.SinceOff.Ticks;
          timeSpan1 = CodeMachineHost.FadeOutDuration;
          double ticks4 = (double) timeSpan1.Ticks;
          double num5 = (double) FezMath.Saturate((float) (1.0 - ticks3 / ticks4));
          backgroundPlane.Opacity = (float) num5;
          CodeMachineHost.BitState bitState3 = bitState1;
          double opacity = (double) bitPlane.Opacity;
          timeSpan1 = CodeMachineHost.FadeInDuration;
          double totalSeconds = timeSpan1.TotalSeconds;
          TimeSpan timeSpan4 = TimeSpan.FromSeconds(opacity * totalSeconds) - timeSpan2;
          bitState3.SinceOn = timeSpan4;
        }
      }
    }
  }

  private void CheckInput()
  {
    Vector3 vector3_1 = this.CameraManager.Viewpoint.ScreenSpaceMask();
    Vector3 vector3_2 = this.CameraManager.Viewpoint.DepthMask();
    Vector3 vector3_3 = this.CodeMachineAO.ArtObject.Size * vector3_1;
    Vector3 vector3_4 = this.CodeMachineAO.Position * vector3_1;
    if (new BoundingBox(vector3_4 - vector3_3 - Vector3.UnitY * 2f, vector3_4 + vector3_3 + vector3_2).Contains(this.PlayerManager.Position * vector3_1 + vector3_2 / 2f) == ContainmentType.Disjoint)
      return;
    if (this.InputManager.Jump == FezButtonState.Pressed)
      this.OnInput(CodeInput.Jump);
    else if (this.InputManager.RotateRight == FezButtonState.Pressed)
      this.OnInput(CodeInput.SpinRight);
    else if (this.InputManager.RotateLeft == FezButtonState.Pressed)
      this.OnInput(CodeInput.SpinLeft);
    else if (this.InputManager.Left == FezButtonState.Pressed)
      this.OnInput(CodeInput.Left);
    else if (this.InputManager.Right == FezButtonState.Pressed)
      this.OnInput(CodeInput.Right);
    else if (this.InputManager.Up == FezButtonState.Pressed)
    {
      this.OnInput(CodeInput.Up);
    }
    else
    {
      if (this.InputManager.Down != FezButtonState.Pressed)
        return;
      this.OnInput(CodeInput.Down);
    }
  }

  private void OnInput(CodeInput newInput)
  {
    int[] bitPattern = CodeMachineHost.BitPatterns[newInput];
    if (this.inputEmitter != null && !this.inputEmitter.Dead)
      this.inputEmitter.Cue.Stop();
    this.inputEmitter = this.inputSound.EmitAt(this.CodeMachineAO.Position, RandomHelper.Between(-0.05, 0.05));
    for (int index1 = 0; index1 < 36; ++index1)
    {
      for (int index2 = 0; index2 < 4; ++index2)
      {
        CodeMachineHost.BitState bitState = this.BitStates[index1 * 4 + index2];
        bitState.On = bitPattern[index1] == 1;
        if (bitState.On)
          bitState.SinceIdle = TimeSpan.Zero;
      }
    }
    this.SinceInput = TimeSpan.Zero;
  }

  [ServiceDependency]
  public IContentManagerProvider CMProvider { private get; set; }

  [ServiceDependency]
  public ILevelMaterializer LevelMaterializer { private get; set; }

  [ServiceDependency]
  public IInputManager InputManager { private get; set; }

  [ServiceDependency]
  public IGameLevelManager LevelManager { private get; set; }

  [ServiceDependency]
  public IGameStateManager GameState { private get; set; }

  [ServiceDependency]
  public IPlayerManager PlayerManager { private get; set; }

  [ServiceDependency]
  public IGameCameraManager CameraManager { private get; set; }

  [ServiceDependency]
  public ISoundManager SoundManager { private get; set; }

  [ServiceDependency]
  public ICodePatternService CPService { private get; set; }

  private class BitState
  {
    public TimeSpan SinceOn;
    public TimeSpan SinceOff;
    public TimeSpan SinceIdle;
    public bool On;
  }
}
