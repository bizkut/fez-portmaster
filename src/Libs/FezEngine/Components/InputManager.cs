// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.InputManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Services;
using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Components;

public class InputManager : GameComponent, IInputManager
{
  private readonly bool mouse;
  private readonly bool keyboard;
  private readonly bool gamepad;
  private Vector2 lastMouseCenter;
  private readonly Stack<InputManager.State> savedStates = new Stack<InputManager.State>();
  private GamepadState MockGamepad = new GamepadState(PlayerIndex.One);

  public event Action<PlayerIndex> ActiveControllerDisconnected;

  public ControllerIndex ActiveControllers { get; private set; }

  public FezButtonState GrabThrow { get; private set; }

  public Vector2 Movement { get; private set; }

  public Vector2 FreeLook { get; private set; }

  public FezButtonState Jump { get; private set; }

  public FezButtonState Back { get; private set; }

  public FezButtonState OpenInventory { get; private set; }

  public FezButtonState Start { get; private set; }

  public FezButtonState RotateLeft { get; private set; }

  public FezButtonState RotateRight { get; private set; }

  public FezButtonState CancelTalk { get; private set; }

  public FezButtonState Up { get; private set; }

  public FezButtonState Down { get; private set; }

  public FezButtonState Left { get; private set; }

  public FezButtonState Right { get; private set; }

  public FezButtonState ClampLook { get; private set; }

  public FezButtonState FpsToggle { get; private set; }

  public FezButtonState ExactUp { get; private set; }

  public FezButtonState MapZoomIn { get; private set; }

  public FezButtonState MapZoomOut { get; private set; }

  public bool StrictRotation { get; set; }

  public InputManager(Game game, bool mouse, bool keyboard, bool gamepad)
    : base(game)
  {
    this.ActiveControllers = ControllerIndex.Any;
    this.mouse = mouse;
    this.keyboard = keyboard;
    this.gamepad = gamepad;
  }

  private FezButtonState GetStateForButton(Buttons button, GamepadState gps)
  {
    switch (button)
    {
      case Buttons.DPadUp:
        return gps.DPad.Up.State;
      case Buttons.DPadDown:
        return gps.DPad.Down.State;
      case Buttons.DPadLeft:
        return gps.DPad.Left.State;
      case Buttons.DPadRight:
        return gps.DPad.Right.State;
      case Buttons.Start:
        return gps.Start;
      case Buttons.Back:
        return gps.Back;
      case Buttons.LeftStick:
        return gps.LeftStick.Clicked.State;
      case Buttons.RightStick:
        return gps.RightStick.Clicked.State;
      case Buttons.LeftShoulder:
        return gps.LeftShoulder.State;
      case Buttons.RightShoulder:
        return gps.RightShoulder.State;
      case Buttons.BigButton:
        throw new NotSupportedException("Guide button should not be bindable!");
      case Buttons.A:
        return gps.A.State;
      case Buttons.B:
        return gps.B.State;
      case Buttons.X:
        return gps.X.State;
      case Buttons.Y:
        return gps.Y.State;
      case Buttons.LeftThumbstickLeft:
        return gps.LeftStick.Left.State;
      case Buttons.RightTrigger:
        return gps.RightTrigger.State;
      case Buttons.LeftTrigger:
        return gps.LeftTrigger.State;
      case Buttons.RightThumbstickUp:
        return gps.RightStick.Up.State;
      case Buttons.RightThumbstickDown:
        return gps.RightStick.Down.State;
      case Buttons.RightThumbstickRight:
        return gps.RightStick.Right.State;
      case Buttons.RightThumbstickLeft:
        return gps.RightStick.Left.State;
      case Buttons.LeftThumbstickUp:
        return gps.LeftStick.Up.State;
      case Buttons.LeftThumbstickDown:
        return gps.LeftStick.Down.State;
      case Buttons.LeftThumbstickRight:
        return gps.LeftStick.Right.State;
      default:
        throw new InvalidOperationException("How did you get here...?");
    }
  }

  public override void Update(GameTime gameTime)
  {
    if (!ServiceHelper.Game.IsActive)
      return;
    this.Reset();
    if (this.keyboard)
    {
      this.KeyboardState.Update(Keyboard.GetState(), gameTime);
      this.Movement = new Vector2(this.KeyboardState.Right.IsDown() ? 1f : (this.KeyboardState.Left.IsDown() ? -1f : 0.0f), this.KeyboardState.Up.IsDown() ? 1f : (this.KeyboardState.Down.IsDown() ? -1f : 0.0f));
      this.FreeLook = new Vector2(this.KeyboardState.LookRight.IsDown() ? 1f : (this.KeyboardState.LookLeft.IsDown() ? -1f : 0.0f), this.KeyboardState.LookUp.IsDown() ? 1f : (this.KeyboardState.LookDown.IsDown() ? -1f : 0.0f));
      this.Back = this.KeyboardState.OpenMap;
      this.Start = this.KeyboardState.Pause;
      this.Jump = this.KeyboardState.Jump;
      this.GrabThrow = this.KeyboardState.GrabThrow;
      this.CancelTalk = this.KeyboardState.CancelTalk;
      this.Down = this.KeyboardState.Down;
      this.ExactUp = this.Up = this.KeyboardState.Up;
      this.Left = this.KeyboardState.Left;
      this.Right = this.KeyboardState.Right;
      this.OpenInventory = this.KeyboardState.OpenInventory;
      this.RotateLeft = this.KeyboardState.RotateLeft;
      this.RotateRight = this.KeyboardState.RotateRight;
      this.MapZoomIn = this.KeyboardState.MapZoomIn;
      this.MapZoomOut = this.KeyboardState.MapZoomOut;
      this.FpsToggle = this.KeyboardState.FpViewToggle;
      this.ClampLook = this.KeyboardState.ClampLook;
    }
    if (this.gamepad)
    {
      Dictionary<MappedAction, Buttons> controllerMapping = SettingsManager.Settings.ControllerMapping;
      PlayerIndex[] players = ControllerIndex.Any.GetPlayers();
      for (int index = 0; index < players.Length; ++index)
      {
        GamepadState gps = this.GamepadsManager[players[index]];
        if (!gps.Connected)
        {
          if (gps.NewlyDisconnected && this.ActiveControllerDisconnected != null)
            this.ActiveControllerDisconnected(players[index]);
        }
        else
        {
          this.ClampLook = FezMath.Coalesce<FezButtonState>(this.ClampLook, this.GetStateForButton(controllerMapping[MappedAction.ClampLook], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.FpsToggle = FezMath.Coalesce<FezButtonState>(this.FpsToggle, this.GetStateForButton(controllerMapping[MappedAction.FpViewToggle], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          Vector2 second1 = Vector2.Clamp(ThumbstickState.CircleToSquare(gps.LeftStick.Position), -Vector2.One, Vector2.One);
          Vector2 second2 = Vector2.Clamp(ThumbstickState.CircleToSquare(gps.RightStick.Position), -Vector2.One, Vector2.One);
          this.Movement = FezMath.Coalesce<Vector2>(this.Movement, second1, gps.DPad.Direction);
          this.FreeLook = FezMath.Coalesce<Vector2>(this.FreeLook, second2);
          this.Back = FezMath.Coalesce<FezButtonState>(this.Back, this.GetStateForButton(controllerMapping[MappedAction.OpenMap], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.Start = FezMath.Coalesce<FezButtonState>(this.Start, this.GetStateForButton(controllerMapping[MappedAction.Pause], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.Jump = FezMath.Coalesce<FezButtonState>(this.Jump, this.GetStateForButton(controllerMapping[MappedAction.Jump], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.GrabThrow = FezMath.Coalesce<FezButtonState>(this.GrabThrow, this.GetStateForButton(controllerMapping[MappedAction.GrabThrow], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.CancelTalk = FezMath.Coalesce<FezButtonState>(this.CancelTalk, this.GetStateForButton(controllerMapping[MappedAction.CancelTalk], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.OpenInventory = FezMath.Coalesce<FezButtonState>(this.OpenInventory, this.GetStateForButton(controllerMapping[MappedAction.OpenInventory], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.Up = FezMath.Coalesce<FezButtonState>(this.Up, gps.DPad.Up.State, gps.LeftStick.Up.State, (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.Down = FezMath.Coalesce<FezButtonState>(this.Down, gps.DPad.Down.State, gps.LeftStick.Down.State, (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.Left = FezMath.Coalesce<FezButtonState>(this.Left, gps.DPad.Left.State, gps.LeftStick.Left.State, (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.Right = FezMath.Coalesce<FezButtonState>(this.Right, gps.DPad.Right.State, gps.LeftStick.Right.State, (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.ExactUp = FezMath.Coalesce<FezButtonState>(this.ExactUp, gps.ExactUp, (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.MapZoomIn = FezMath.Coalesce<FezButtonState>(this.MapZoomIn, this.GetStateForButton(controllerMapping[MappedAction.MapZoomIn], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          this.MapZoomOut = FezMath.Coalesce<FezButtonState>(this.MapZoomOut, this.GetStateForButton(controllerMapping[MappedAction.MapZoomOut], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          if (this.StrictRotation)
          {
            this.RotateLeft = FezMath.Coalesce<FezButtonState>(this.RotateLeft, this.GetStateForButton(controllerMapping[MappedAction.RotateLeft], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
            this.RotateRight = FezMath.Coalesce<FezButtonState>(this.RotateRight, this.GetStateForButton(controllerMapping[MappedAction.RotateRight], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          }
          else
          {
            this.RotateLeft = FezMath.Coalesce<FezButtonState>(this.RotateLeft, this.GetStateForButton(controllerMapping[MappedAction.MapZoomOut], gps), this.GetStateForButton(controllerMapping[MappedAction.RotateLeft], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
            this.RotateRight = FezMath.Coalesce<FezButtonState>(this.RotateRight, this.GetStateForButton(controllerMapping[MappedAction.MapZoomIn], gps), this.GetStateForButton(controllerMapping[MappedAction.RotateRight], gps), (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
          }
          if (SettingsManager.Settings.DeadZone != 0)
          {
            Vector2 vector2 = this.Movement;
            float num1 = vector2.Length();
            if ((double) num1 > 0.0 && (double) num1 < (double) SettingsManager.Settings.DeadZone / 100.0)
              this.Movement = Vector2.Zero;
            vector2 = this.FreeLook;
            float num2 = vector2.Length();
            if ((double) num2 > 0.0 && (double) num2 < (double) SettingsManager.Settings.DeadZone / 100.0)
              this.FreeLook = Vector2.Zero;
          }
        }
      }
    }
    if (!this.mouse)
      return;
    this.MouseState.Update(gameTime);
    Vector2 second = Vector2.Zero;
    switch (this.MouseState.LeftButton.State)
    {
      case MouseButtonStates.DragStarted:
        MouseButtonState leftButton1 = this.MouseState.LeftButton;
        MouseDragState dragState1 = leftButton1.DragState;
        double x1 = (double) dragState1.Movement.X;
        leftButton1 = this.MouseState.LeftButton;
        dragState1 = leftButton1.DragState;
        double y1 = (double) -dragState1.Movement.Y;
        this.lastMouseCenter = new Vector2((float) x1, (float) y1);
        break;
      case MouseButtonStates.Dragging:
        Vector2 vector2_1;
        ref Vector2 local = ref vector2_1;
        MouseButtonState leftButton2 = this.MouseState.LeftButton;
        MouseDragState dragState2 = leftButton2.DragState;
        double x2 = (double) dragState2.Movement.X;
        leftButton2 = this.MouseState.LeftButton;
        dragState2 = leftButton2.DragState;
        double y2 = (double) -dragState2.Movement.Y;
        local = new Vector2((float) x2, (float) y2);
        second = (this.lastMouseCenter - vector2_1) / 32f;
        this.lastMouseCenter = vector2_1;
        break;
    }
    this.FreeLook = FezMath.Coalesce<Vector2>(this.FreeLook, second);
    this.MapZoomIn = FezMath.Coalesce<FezButtonState>(this.MapZoomIn, this.MouseState.WheelTurnedUp, (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
    this.MapZoomOut = FezMath.Coalesce<FezButtonState>(this.MapZoomOut, this.MouseState.WheelTurnedDown, (IEqualityComparer<FezButtonState>) FezButtonStateComparer.Default);
  }

  public void SaveState()
  {
    this.savedStates.Push(new InputManager.State()
    {
      Up = this.Up,
      Down = this.Down,
      Left = this.Left,
      Right = this.Right,
      ExactUp = this.ExactUp,
      Cancel = this.CancelTalk,
      GrabThrow = this.GrabThrow,
      Jump = this.Jump,
      RotateLeft = this.RotateLeft,
      RotateRight = this.RotateRight,
      Start = this.Start,
      Back = this.Back,
      FreeLook = this.FreeLook,
      Movement = this.Movement,
      OpenInventory = this.OpenInventory,
      ClampLook = this.ClampLook,
      FpsToggle = this.FpsToggle,
      MapZoomIn = this.MapZoomIn,
      MapZoomOut = this.MapZoomOut
    });
  }

  public void RecoverState()
  {
    if (this.savedStates.Count == 0)
      return;
    InputManager.State state = this.savedStates.Pop();
    this.Up = state.Up;
    this.Down = state.Down;
    this.Left = state.Left;
    this.Right = state.Right;
    this.ExactUp = state.ExactUp;
    this.CancelTalk = state.Cancel;
    this.GrabThrow = state.GrabThrow;
    this.Jump = state.Jump;
    this.RotateLeft = state.RotateLeft;
    this.RotateRight = state.RotateRight;
    this.Start = state.Start;
    this.Back = state.Back;
    this.FreeLook = state.FreeLook;
    this.Movement = state.Movement;
    this.OpenInventory = state.OpenInventory;
    this.ClampLook = state.ClampLook;
    this.FpsToggle = state.FpsToggle;
    this.MapZoomIn = state.MapZoomIn;
    this.MapZoomOut = state.MapZoomOut;
  }

  public void Reset()
  {
    this.Up = this.Down = this.Left = this.Right = this.ExactUp = FezButtonState.Up;
    Vector2 vector2 = new Vector2();
    this.FreeLook = vector2;
    this.Movement = vector2;
    this.CancelTalk = this.GrabThrow = this.Jump = FezButtonState.Up;
    this.Start = this.Back = FezButtonState.Up;
    this.RotateLeft = this.RotateRight = FezButtonState.Up;
    this.OpenInventory = FezButtonState.Up;
    this.ClampLook = this.FpsToggle = FezButtonState.Up;
    this.MapZoomIn = this.MapZoomOut = FezButtonState.Up;
  }

  public void PressedToDown()
  {
    if (this.ExactUp == FezButtonState.Pressed)
      this.ExactUp = FezButtonState.Down;
    if (this.Up == FezButtonState.Pressed)
      this.Up = FezButtonState.Down;
    if (this.Down == FezButtonState.Pressed)
      this.Down = FezButtonState.Down;
    if (this.Left == FezButtonState.Pressed)
      this.Left = FezButtonState.Down;
    if (this.Right == FezButtonState.Pressed)
      this.Right = FezButtonState.Down;
    if (this.CancelTalk == FezButtonState.Pressed)
      this.CancelTalk = FezButtonState.Down;
    if (this.GrabThrow == FezButtonState.Pressed)
      this.GrabThrow = FezButtonState.Down;
    if (this.Jump == FezButtonState.Pressed)
      this.Jump = FezButtonState.Down;
    if (this.Start == FezButtonState.Pressed)
      this.Start = FezButtonState.Down;
    if (this.Back == FezButtonState.Pressed)
      this.Back = FezButtonState.Down;
    if (this.RotateLeft == FezButtonState.Pressed)
      this.RotateLeft = FezButtonState.Down;
    if (this.OpenInventory == FezButtonState.Pressed)
      this.OpenInventory = FezButtonState.Down;
    if (this.RotateRight == FezButtonState.Pressed)
      this.RotateRight = FezButtonState.Down;
    if (this.ClampLook == FezButtonState.Pressed)
      this.ClampLook = FezButtonState.Down;
    if (this.FpsToggle == FezButtonState.Pressed)
      this.FpsToggle = FezButtonState.Down;
    if (this.MapZoomIn == FezButtonState.Pressed)
      this.MapZoomIn = FezButtonState.Down;
    if (this.MapZoomOut != FezButtonState.Pressed)
      return;
    this.MapZoomOut = FezButtonState.Down;
  }

  public void ForceActiveController(ControllerIndex ci) => this.ActiveControllers = ci;

  public void DetermineActiveController()
  {
    if (!this.gamepad)
      return;
    foreach (PlayerIndex player in this.ActiveControllers.GetPlayers())
    {
      GamepadState gamepadState = this.GamepadsManager[player];
      if (gamepadState.Start.IsDown() || gamepadState.A.State.IsDown() || gamepadState.Back.IsDown() || gamepadState.B.State.IsDown())
      {
        this.ActiveControllers = player.ToControllerIndex();
        return;
      }
    }
    this.ActiveControllers = ControllerIndex.None;
  }

  public void ClearActiveController() => this.ActiveControllers = ControllerIndex.Any;

  public GamepadState ActiveGamepad
  {
    get
    {
      if (!this.gamepad)
        return this.MockGamepad;
      return this.ActiveControllers == ControllerIndex.None ? this.GamepadsManager[PlayerIndex.One] : this.GamepadsManager[this.ActiveControllers.GetPlayer()];
    }
  }

  protected override void Dispose(bool disposing)
  {
    base.Dispose(disposing);
    foreach (PlayerIndex playerIndex in Util.GetValues<PlayerIndex>())
      GamePad.SetVibration(playerIndex, 0.0f, 0.0f);
  }

  public bool AnyButtonPressed()
  {
    return this.GrabThrow == FezButtonState.Pressed || this.Jump == FezButtonState.Pressed || this.OpenInventory == FezButtonState.Pressed || this.Start == FezButtonState.Pressed || this.CancelTalk == FezButtonState.Pressed;
  }

  [ServiceDependency(Optional = true)]
  public IMouseStateManager MouseState { private get; set; }

  [ServiceDependency(Optional = true)]
  public IKeyboardStateManager KeyboardState { private get; set; }

  [ServiceDependency(Optional = true)]
  public IGamepadsManager GamepadsManager { private get; set; }

  private struct State
  {
    public FezButtonState Up;
    public FezButtonState Down;
    public FezButtonState Left;
    public FezButtonState Right;
    public FezButtonState Cancel;
    public FezButtonState GrabThrow;
    public FezButtonState RotateLeft;
    public FezButtonState RotateRight;
    public FezButtonState Start;
    public FezButtonState Back;
    public FezButtonState Jump;
    public FezButtonState OpenInventory;
    public FezButtonState ExactUp;
    public FezButtonState ClampLook;
    public FezButtonState FpsToggle;
    public FezButtonState MapZoomIn;
    public FezButtonState MapZoomOut;
    public Vector2 Movement;
    public Vector2 FreeLook;
  }
}
