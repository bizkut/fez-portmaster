// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.GamepadState
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

#nullable disable
namespace FezEngine.Structure.Input;

public class GamepadState
{
  public static EventHandler OnLayoutChanged;
  private static GamepadState.GamepadLayout? INTERNAL_forcedLayout = new GamepadState.GamepadLayout?();
  private static GamepadState.GamepadLayout INTERNAL_layout = GamepadState.GamepadLayout.Xbox360;
  private static readonly TimeSpan ConnectedCheckFrequency = TimeSpan.FromSeconds(1.0);
  private GamepadState.VibrationMotorState leftMotor;
  private GamepadState.VibrationMotorState rightMotor;
  private TimeSpan sinceCheckedConnected = GamepadState.ConnectedCheckFrequency;
  public readonly PlayerIndex PlayerIndex;

  public static GamepadState.GamepadLayout? ForcedLayout
  {
    get => GamepadState.INTERNAL_forcedLayout;
    set
    {
      GamepadState.INTERNAL_forcedLayout = value;
      GamepadState.UpdateLayout();
    }
  }

  public static GamepadState.GamepadLayout Layout
  {
    get => GamepadState.INTERNAL_layout;
    private set
    {
      if (value == GamepadState.INTERNAL_layout)
        return;
      GamepadState.INTERNAL_layout = value;
      if (GamepadState.OnLayoutChanged == null)
        return;
      GamepadState.OnLayoutChanged((object) null, (EventArgs) null);
    }
  }

  private static void UpdateLayout()
  {
    if (GamepadState.ForcedLayout.HasValue)
    {
      GamepadState.Layout = GamepadState.ForcedLayout.Value;
    }
    else
    {
      string guidext = GamePad.GetGUIDEXT(PlayerIndex.One);
      switch (guidext)
      {
        case "4c05c405":
          GamepadState.Layout = GamepadState.GamepadLayout.PlayStation4;
          break;
        case "4c056802":
        case "88880803":
        case "25090500":
          GamepadState.Layout = GamepadState.GamepadLayout.PlayStation3;
          break;
        default:
          if (string.IsNullOrEmpty(guidext))
            break;
          GamepadState.Layout = GamepadState.GamepadLayout.Xbox360;
          break;
      }
    }
  }

  public GamepadState(PlayerIndex playerIndex) => this.PlayerIndex = playerIndex;

  public DirectionalState DPad { get; private set; }

  public ThumbstickState LeftStick { get; private set; }

  public ThumbstickState RightStick { get; private set; }

  public FezButtonState ExactUp { get; private set; }

  public TimedButtonState A { get; private set; }

  public TimedButtonState B { get; private set; }

  public TimedButtonState X { get; private set; }

  public TimedButtonState Y { get; private set; }

  public TimedButtonState RightShoulder { get; private set; }

  public TimedButtonState LeftShoulder { get; private set; }

  public TimedAnalogButtonState RightTrigger { get; private set; }

  public TimedAnalogButtonState LeftTrigger { get; private set; }

  public FezButtonState Start { get; private set; }

  public FezButtonState Back { get; private set; }

  public static bool AnyConnected { get; set; }

  public bool Connected { get; set; }

  public bool NewlyDisconnected { get; set; }

  public void Update(TimeSpan elapsed)
  {
    this.sinceCheckedConnected += elapsed;
    if (this.sinceCheckedConnected >= GamepadState.ConnectedCheckFrequency)
    {
      bool connected = this.Connected;
      this.Connected = GamePad.GetState(this.PlayerIndex).IsConnected;
      if (connected && !this.Connected)
        this.NewlyDisconnected = true;
      else if (this.Connected && !connected)
      {
        GamepadState.AnyConnected = true;
        GamepadState.UpdateLayout();
      }
    }
    if (!this.Connected)
      return;
    GamepadState.AnyConnected = true;
    GamePadState state;
    try
    {
      state = GamePad.GetState(this.PlayerIndex, GamePadDeadZone.IndependentAxes);
    }
    catch
    {
      return;
    }
    this.Connected = state.IsConnected;
    if (!this.Connected)
      return;
    if (SettingsManager.Settings.Vibration)
    {
      if (this.leftMotor.Active)
        this.leftMotor = GamepadState.UpdateMotor(this.leftMotor, elapsed);
      if (this.rightMotor.Active)
        this.rightMotor = GamepadState.UpdateMotor(this.rightMotor, elapsed);
      if ((double) this.leftMotor.LastAmount != (double) this.leftMotor.CurrentAmount || (double) this.rightMotor.LastAmount != (double) this.rightMotor.CurrentAmount)
        GamePad.SetVibration(this.PlayerIndex, this.leftMotor.CurrentAmount, this.rightMotor.CurrentAmount);
    }
    this.UpdateFromState(state, elapsed);
    if (!(this.sinceCheckedConnected >= GamepadState.ConnectedCheckFrequency))
      return;
    this.sinceCheckedConnected = TimeSpan.Zero;
  }

  private void UpdateFromState(GamePadState gamepadState, TimeSpan elapsed)
  {
    this.LeftShoulder = this.LeftShoulder.NextState(gamepadState.Buttons.LeftShoulder == ButtonState.Pressed, elapsed);
    this.RightShoulder = this.RightShoulder.NextState(gamepadState.Buttons.RightShoulder == ButtonState.Pressed, elapsed);
    this.LeftTrigger = this.LeftTrigger.NextState(gamepadState.Triggers.Left, elapsed);
    this.RightTrigger = this.RightTrigger.NextState(gamepadState.Triggers.Right, elapsed);
    this.Start = this.Start.NextState(gamepadState.Buttons.Start == ButtonState.Pressed);
    this.Back = this.Back.NextState(gamepadState.Buttons.Back == ButtonState.Pressed);
    this.A = this.A.NextState(gamepadState.Buttons.A == ButtonState.Pressed, elapsed);
    this.B = this.B.NextState(gamepadState.Buttons.B == ButtonState.Pressed, elapsed);
    this.X = this.X.NextState(gamepadState.Buttons.X == ButtonState.Pressed, elapsed);
    this.Y = this.Y.NextState(gamepadState.Buttons.Y == ButtonState.Pressed, elapsed);
    DirectionalState dpad1 = this.DPad;
    ref DirectionalState local = ref dpad1;
    GamePadDPad dpad2 = gamepadState.DPad;
    int num1 = dpad2.Up == ButtonState.Pressed ? 1 : 0;
    dpad2 = gamepadState.DPad;
    int num2 = dpad2.Down == ButtonState.Pressed ? 1 : 0;
    dpad2 = gamepadState.DPad;
    int num3 = dpad2.Left == ButtonState.Pressed ? 1 : 0;
    dpad2 = gamepadState.DPad;
    int num4 = dpad2.Right == ButtonState.Pressed ? 1 : 0;
    TimeSpan elapsed1 = elapsed;
    this.DPad = local.NextState(num1 != 0, num2 != 0, num3 != 0, num4 != 0, elapsed1);
    this.LeftStick = this.LeftStick.NextState(gamepadState.ThumbSticks.Left, gamepadState.Buttons.LeftStick == ButtonState.Pressed, elapsed);
    this.RightStick = this.RightStick.NextState(gamepadState.ThumbSticks.Right, gamepadState.Buttons.RightStick == ButtonState.Pressed, elapsed);
    this.ExactUp = this.ExactUp.NextState((double) this.LeftStick.Position.Y > 0.89999997615814209 || this.DPad.Up.State.IsDown() && this.DPad.Left.State == FezButtonState.Up && this.DPad.Right.State == FezButtonState.Up);
  }

  private static GamepadState.VibrationMotorState UpdateMotor(
    GamepadState.VibrationMotorState motorState,
    TimeSpan elapsedTime)
  {
    if (motorState.ElapsedTime <= motorState.Duration)
    {
      float num = Easing.EaseIn(1.0 - motorState.ElapsedTime.TotalSeconds / motorState.Duration.TotalSeconds, motorState.EasingType);
      motorState.CurrentAmount = num * motorState.MaximumAmount;
    }
    else
    {
      motorState.CurrentAmount = 0.0f;
      motorState.Active = false;
    }
    motorState.ElapsedTime += elapsedTime;
    return motorState;
  }

  public void Vibrate(VibrationMotor motor, double amount, TimeSpan duration)
  {
    this.Vibrate(motor, amount, duration, EasingType.Linear);
  }

  public void Vibrate(
    VibrationMotor motor,
    double amount,
    TimeSpan duration,
    EasingType easingType)
  {
    GamepadState.VibrationMotorState vibrationMotorState = new GamepadState.VibrationMotorState(amount, duration, easingType);
    if (motor != VibrationMotor.LeftLow)
    {
      if (motor != VibrationMotor.RightHigh)
        return;
      this.rightMotor = vibrationMotorState;
    }
    else
      this.leftMotor = vibrationMotorState;
  }

  public enum GamepadLayout
  {
    Xbox360,
    PlayStation3,
    PlayStation4,
  }

  private struct VibrationMotorState
  {
    public readonly float MaximumAmount;
    public readonly TimeSpan Duration;
    public readonly EasingType EasingType;
    public bool Active;
    public TimeSpan ElapsedTime;
    private float currentAmount;

    public float LastAmount { get; private set; }

    public float CurrentAmount
    {
      get => this.currentAmount;
      set
      {
        this.LastAmount = this.currentAmount;
        this.currentAmount = value;
      }
    }

    public VibrationMotorState(double maximumAmount, TimeSpan duration, EasingType easingType)
      : this()
    {
      this.Active = true;
      this.LastAmount = this.CurrentAmount = 0.0f;
      this.ElapsedTime = TimeSpan.Zero;
      this.MaximumAmount = (float) FezMath.Saturate(maximumAmount);
      this.Duration = duration;
      this.EasingType = easingType;
    }
  }
}
