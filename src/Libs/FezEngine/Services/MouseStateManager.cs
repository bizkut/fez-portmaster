// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.MouseStateManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure.Input;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

#nullable disable
namespace FezEngine.Services;

public class MouseStateManager : IMouseStateManager
{
  private const int DraggingThreshold = 3;
  private IEngineStateManager EngineState;
  private MouseState lastState;
  private Point dragOffset;
  private MouseButtonState leftButton;
  private MouseButtonState middleButton;
  private MouseButtonState rightButton;
  private FezButtonState wheelTurnedUp;
  private FezButtonState wheelTurnedDown;
  private int wheelTurns;
  private Point position;
  private Point movement;
  private IntPtr renderPanelHandle;
  private IntPtr parentFormHandle;

  public MouseButtonState LeftButton => this.leftButton;

  public MouseButtonState MiddleButton => this.middleButton;

  public MouseButtonState RightButton => this.rightButton;

  public int WheelTurns => this.wheelTurns;

  public Point Position => this.position;

  public Point Movement => this.movement;

  public IntPtr RenderPanelHandle
  {
    set => this.renderPanelHandle = value;
  }

  public IntPtr ParentFormHandle
  {
    set => this.parentFormHandle = value;
  }

  public FezButtonState WheelTurnedUp => this.wheelTurnedUp;

  public FezButtonState WheelTurnedDown => this.wheelTurnedDown;

  public void Update(GameTime time)
  {
    MouseState state1 = Mouse.GetState();
    this.wheelTurns = state1.ScrollWheelValue - this.lastState.ScrollWheelValue;
    this.wheelTurnedUp = this.wheelTurnedUp.NextState(this.wheelTurns > 0);
    this.wheelTurnedDown = this.wheelTurnedDown.NextState(this.wheelTurns < 0);
    if (this.renderPanelHandle != this.parentFormHandle)
      state1 = Mouse.GetState();
    this.movement = new Point(state1.X - this.position.X - this.dragOffset.X, state1.Y - this.position.Y - this.dragOffset.Y);
    this.position = new Point(state1.X + this.dragOffset.X, state1.Y + this.dragOffset.Y);
    if (ServiceHelper.Game.IsActive && state1.LeftButton == ButtonState.Pressed && (this.EngineState ?? (this.EngineState = ServiceHelper.Get<IEngineStateManager>())).InFpsMode)
    {
      Rectangle rectangle = ServiceHelper.Game.GraphicsDevice.PresentationParameters.IsFullScreen ? ServiceHelper.Game.GraphicsDevice.Viewport.Bounds : ServiceHelper.Game.Window.ClientBounds;
      Point point1 = new Point();
      if (state1.X <= 0)
        point1.X += rectangle.Width - 2;
      if (state1.X >= rectangle.Width - 1)
        point1.X -= rectangle.Width - 2;
      if (state1.Y <= 0)
        point1.Y += rectangle.Height - 2;
      if (state1.Y >= rectangle.Height - 1)
        point1.Y -= rectangle.Height - 2;
      if (point1.X != 0 || point1.Y != 0)
      {
        MouseState state2 = Mouse.GetState();
        Point point2 = new Point(state2.X - state1.X, state2.Y - state1.Y);
        Mouse.SetPosition(point2.X + state1.X + point1.X, point2.Y + state1.Y + point1.Y);
        this.dragOffset.X -= point1.X;
        this.dragOffset.Y -= point1.Y;
      }
    }
    else
      this.dragOffset = Point.Zero;
    if (state1 != this.lastState)
    {
      bool hasMoved = this.movement.X != 0 || this.movement.Y != 0;
      this.leftButton = this.DeduceMouseButtonState(this.leftButton, this.lastState.LeftButton, state1.LeftButton, hasMoved);
      this.middleButton = this.DeduceMouseButtonState(this.middleButton, this.lastState.MiddleButton, state1.MiddleButton, hasMoved);
      this.rightButton = this.DeduceMouseButtonState(this.rightButton, this.lastState.RightButton, state1.RightButton, hasMoved);
      this.lastState = state1;
    }
    else
    {
      this.ResetButton(ref this.leftButton);
      this.ResetButton(ref this.middleButton);
      this.ResetButton(ref this.rightButton);
    }
  }

  private MouseButtonState DeduceMouseButtonState(
    MouseButtonState lastMouseButtonState,
    ButtonState lastButtonState,
    ButtonState buttonState,
    bool hasMoved)
  {
    if (lastButtonState == ButtonState.Released && buttonState == ButtonState.Released)
      return new MouseButtonState(MouseButtonStates.Idle);
    if (lastButtonState == ButtonState.Released && buttonState == ButtonState.Pressed)
      return new MouseButtonState(MouseButtonStates.Pressed, new MouseDragState(this.position, this.position));
    if (lastButtonState == ButtonState.Pressed && buttonState == ButtonState.Pressed)
    {
      if (!hasMoved)
        return lastMouseButtonState;
      MouseDragState dragState = lastMouseButtonState.DragState;
      if (dragState.PreDrag)
      {
        int x1 = this.position.X;
        dragState = lastMouseButtonState.DragState;
        int x2 = dragState.Start.X;
        if (Math.Abs(x1 - x2) <= 3)
        {
          int y1 = this.position.Y;
          dragState = lastMouseButtonState.DragState;
          int y2 = dragState.Start.Y;
          if (Math.Abs(y1 - y2) <= 3)
            goto label_12;
        }
        if (lastMouseButtonState.State == MouseButtonStates.DragStarted || lastMouseButtonState.State == MouseButtonStates.Dragging)
        {
          dragState = lastMouseButtonState.DragState;
          return new MouseButtonState(MouseButtonStates.Dragging, new MouseDragState(dragState.Start, this.position, true));
        }
        dragState = lastMouseButtonState.DragState;
        return new MouseButtonState(MouseButtonStates.DragStarted, new MouseDragState(dragState.Start, this.position, true));
      }
label_12:
      dragState = lastMouseButtonState.DragState;
      return new MouseButtonState(MouseButtonStates.Down, new MouseDragState(dragState.Start, this.position, true));
    }
    if (lastButtonState != ButtonState.Pressed || buttonState != ButtonState.Released)
      throw new InvalidOperationException();
    return (lastMouseButtonState.State == MouseButtonStates.Pressed || lastMouseButtonState.State == MouseButtonStates.Down) && !hasMoved ? new MouseButtonState(MouseButtonStates.Clicked) : new MouseButtonState(MouseButtonStates.DragEnded);
  }

  private void ResetButton(ref MouseButtonState button)
  {
    if (button.State == MouseButtonStates.Pressed)
      button = new MouseButtonState(MouseButtonStates.Down, button.DragState);
    if (button.State == MouseButtonStates.Clicked || button.State == MouseButtonStates.DragEnded)
      button = new MouseButtonState(MouseButtonStates.Idle);
    if (button.State != MouseButtonStates.DragStarted)
      return;
    button = new MouseButtonState(MouseButtonStates.Dragging, button.DragState);
  }
}
