// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.SettingsManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using ContentSerialization;
using FezEngine.Effects;
using FezEngine.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SDL2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

#nullable disable
namespace FezEngine.Tools;

public static class SettingsManager
{
  public const float SixteenByNine = 1.77777779f;
  private const string SettingsFilename = "Settings";
  public static Settings Settings;
  public static GraphicsDeviceManager DeviceManager;
  public static bool FirstOpen;
  public static List<DisplayMode> Resolutions;
  public static DisplayMode NativeResolution;
  private static float viewScale;
  private static int letterboxW;
  private static int letterboxH;

  public static bool SupportsHardwareInstancing { private set; get; }

  public static int MaxMultiSampleCount { private set; get; }

  public static void InitializeSettings()
  {
    string str = Path.Combine(Util.LocalConfigFolder, "Settings");
    SettingsManager.FirstOpen = !File.Exists(str);
    if (SettingsManager.FirstOpen)
    {
      SettingsManager.Settings = new Settings();
    }
    else
    {
      try
      {
        SettingsManager.Settings = SdlSerializer.Deserialize<Settings>(str);
      }
      catch (Exception ex)
      {
        SettingsManager.Settings = new Settings();
      }
    }
    Culture.Language = SettingsManager.Settings.Language;
    SettingsManager.Save();
  }

  public static void InitializeResolutions()
  {
    SettingsManager.Resolutions = GraphicsAdapter.DefaultAdapter.SupportedDisplayModes.Distinct<DisplayMode>((IEqualityComparer<DisplayMode>) DisplayModeEqualityComparer.Default).Where<DisplayMode>((Func<DisplayMode, bool>) (x => x.Width >= 1280 /*0x0500*/ || x.Height >= 720)).OrderBy<DisplayMode, int>((Func<DisplayMode, int>) (x => x.Width)).ThenBy<DisplayMode, int>((Func<DisplayMode, int>) (x => x.Height)).ToList<DisplayMode>();
    SettingsManager.NativeResolution = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
    if (!SettingsManager.Resolutions.Any<DisplayMode>((Func<DisplayMode, bool>) (x => x.Width >= 1280 /*0x0500*/ && x.Height >= 720)))
      return;
    SettingsManager.Resolutions.RemoveAll((Predicate<DisplayMode>) (x =>
    {
      if (x.Width < 1280 /*0x0500*/)
        return true;
      return x.Height < 720 && x != SettingsManager.NativeResolution;
    }));
  }

  public static void InitializeCapabilities()
  {
    object obj = typeof (GraphicsDevice).GetField("GLDevice", BindingFlags.Instance | BindingFlags.NonPublic).GetValue((object) SettingsManager.DeviceManager.GraphicsDevice);
    Type type = typeof (GraphicsDevice).Assembly.GetType("Microsoft.Xna.Framework.Graphics.IGLDevice");
    SettingsManager.SupportsHardwareInstancing = (bool) type.GetProperty("SupportsHardwareInstancing").GetValue(obj, (object[]) null);
    if (SettingsManager.Settings.HardwareInstancing && !SettingsManager.SupportsHardwareInstancing)
      SettingsManager.Settings.HardwareInstancing = false;
    Logger.Log("Instancing", LogSeverity.Information, "Hardware instancing is " + (SettingsManager.Settings.HardwareInstancing ? "enabled" : "disabled"));
    SettingsManager.MaxMultiSampleCount = (int) type.GetProperty("MaxMultiSampleCount").GetValue(obj, (object[]) null);
    if (SettingsManager.Settings.MultiSampleCount > SettingsManager.MaxMultiSampleCount)
      SettingsManager.Settings.MultiSampleCount = SettingsManager.MaxMultiSampleCount;
    SettingsManager.Settings.HighDPI = Environment.GetEnvironmentVariable("FNA_GRAPHICS_ENABLE_HIGHDPI") == "1";
    SettingsManager.DeviceManager.DeviceReset += (EventHandler<EventArgs>) ((o, e) => SettingsManager.DeviceManager.GraphicsDevice.SetupViewport());
  }

  public static void Save()
  {
    SdlSerializer.Serialize<Settings>(Path.Combine(Util.LocalConfigFolder, "Settings"), SettingsManager.Settings);
  }

  public static void Apply()
  {
    Game game = ServiceHelper.Game;
    int num1 = SettingsManager.Settings.Width;
    int num2 = SettingsManager.Settings.Height;
    if (SettingsManager.Settings.UseCurrentMode)
    {
      DisplayMode currentDisplayMode = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode;
      SettingsManager.Settings.Height = num2 = currentDisplayMode.Height;
      SettingsManager.Settings.Width = num1 = (int) Math.Round((double) num2 * (double) currentDisplayMode.AspectRatio);
    }
    SettingsManager.DeviceManager.IsFullScreen = SettingsManager.Settings.ScreenMode == ScreenMode.Fullscreen;
    if (SettingsManager.DeviceManager.IsFullScreen)
    {
      float num3 = (float) num1 / (float) num2 / SettingsManager.DeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode.AspectRatio;
      if ((double) num3 > 1.0)
      {
        int num4 = (int) Math.Round((double) num2 * (double) num3);
        SettingsManager.letterboxH = num4 - num2;
        num2 = num4;
        SettingsManager.letterboxW = 0;
      }
      else
      {
        int num5 = (int) Math.Round((double) num1 / (double) num3);
        SettingsManager.letterboxW = num5 - num1;
        num1 = num5;
        SettingsManager.letterboxH = 0;
      }
    }
    else
    {
      SettingsManager.letterboxW = 0;
      SettingsManager.letterboxH = 0;
    }
    SettingsManager.DeviceManager.PreferredBackBufferWidth = num1;
    SettingsManager.DeviceManager.PreferredBackBufferHeight = num2;
    SettingsManager.DeviceManager.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
    game.IsMouseVisible = !SettingsManager.DeviceManager.IsFullScreen;
    SettingsManager.DeviceManager.SynchronizeWithVerticalRetrace = SettingsManager.Settings.VSync;
    BaseEffect.UseHardwareInstancing = SettingsManager.Settings.HardwareInstancing;
    SettingsManager.DeviceManager.PreferMultiSampling = SettingsManager.Settings.MultiSampleCount != 0;
    if (SettingsManager.DeviceManager.PreferMultiSampling)
      SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.MultiSampleCount = SettingsManager.Settings.MultiSampleCount;
    SettingsManager.DeviceManager.ApplyChanges();
    game.Window.IsBorderlessEXT = SettingsManager.Settings.ScreenMode == ScreenMode.BorderlessWindowed;
    if (game.Window.IsBorderlessEXT && !SettingsManager.DeviceManager.IsFullScreen && SettingsManager.DeviceManager.GraphicsDevice.DisplayMode == SettingsManager.DeviceManager.GraphicsDevice.Adapter.CurrentDisplayMode)
    {
      int num6 = SDL.SDL_WINDOWPOS_CENTERED_DISPLAY(SDL.SDL_GetWindowDisplayIndex(game.Window.Handle));
      SDL.SDL_SetWindowPosition(game.Window.Handle, num6, num6);
    }
    if (SettingsManager.Settings.ScaleMode == ScaleMode.Supersampled)
    {
      float num7 = (float) num1 / (float) num2;
      int num8;
      int num9;
      if ((double) Math.Abs(num7 - 1.77777779f) > 0.10000000149011612 && (double) num7 > 1.7777777910232544)
      {
        int num10 = 720;
        int num11 = (int) Math.Ceiling((double) num2 / (double) num10);
        num8 = num10 * num11;
        num9 = (int) Math.Ceiling((double) num10 * (double) num7 * (double) num11);
      }
      else
      {
        int num12 = 1280 /*0x0500*/;
        int num13 = (int) Math.Ceiling((double) num1 / (double) num12);
        num9 = num12 * num13;
        num8 = (int) Math.Ceiling((double) num12 / (double) num7 * (double) num13);
      }
      if (SettingsManager.DeviceManager.IsFullScreen)
      {
        SettingsManager.letterboxW = (int) Math.Round((double) SettingsManager.letterboxW * ((double) num9 / (double) num1));
        SettingsManager.letterboxH = (int) Math.Round((double) SettingsManager.letterboxH * ((double) num8 / (double) num2));
      }
      int num14 = num9;
      int num15 = num8;
      if (SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth != num14 || SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight != num15)
      {
        SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth = num14;
        SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight = num15;
        SettingsManager.DeviceManager.GraphicsDevice.Reset();
      }
    }
    Logger.Log(nameof (SettingsManager), "Screen set to " + (object) GraphicsAdapter.DefaultAdapter.CurrentDisplayMode);
    Logger.Log(nameof (SettingsManager), "Screen mode is : " + (SettingsManager.Settings.ScreenMode == ScreenMode.Fullscreen ? "Fullscreen" : (SettingsManager.Settings.ScreenMode == ScreenMode.BorderlessWindowed ? "Borderless Window" : "Windowed")));
    if (SettingsManager.Settings.HighDPI)
      Logger.Log(nameof (SettingsManager), "Hi-DPI mode is enabled.");
    Logger.Log(nameof (SettingsManager), $"Backbuffer is {SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.BackBufferWidth}x{SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.BackBufferHeight}");
    Logger.Log(nameof (SettingsManager), "VSync is " + (SettingsManager.DeviceManager.SynchronizeWithVerticalRetrace ? "on" : "off"));
    Logger.Log(nameof (SettingsManager), "Multisample count is " + SettingsManager.DeviceManager.GraphicsDevice.PresentationParameters.MultiSampleCount.ToString());
    game.IsMouseVisible = false;
  }

  public static void SetupViewport(this GraphicsDevice device)
  {
    RenderTargetBinding[] renderTargets = device.GetRenderTargets();
    if (renderTargets.Length != 0 && renderTargets[0].RenderTarget is Texture2D)
      return;
    int backBufferWidth = device.PresentationParameters.BackBufferWidth;
    int backBufferHeight = device.PresentationParameters.BackBufferHeight;
    GraphicsDevice graphicsDevice = device;
    Viewport viewport1 = new Viewport();
    viewport1.X = SettingsManager.letterboxW / 2;
    viewport1.Y = SettingsManager.letterboxH / 2;
    viewport1.Width = backBufferWidth - SettingsManager.letterboxW;
    viewport1.Height = backBufferHeight - SettingsManager.letterboxH;
    viewport1.MinDepth = 0.0f;
    viewport1.MaxDepth = 1f;
    Viewport viewport2 = viewport1;
    graphicsDevice.Viewport = viewport2;
    device.ScissorRectangle = new Rectangle(SettingsManager.letterboxW / 2, SettingsManager.letterboxH / 2, backBufferWidth - SettingsManager.letterboxW, backBufferHeight - SettingsManager.letterboxH);
    viewport1 = device.Viewport;
    int width = viewport1.Width;
    viewport1 = device.Viewport;
    int height = viewport1.Height;
    float num1 = (float) width / (float) height;
    switch (SettingsManager.Settings.ScaleMode)
    {
      case ScaleMode.FullAspect:
        float num2 = (float) width / 1.77777779f;
        float num3 = (float) height * 1.77777779f;
        SettingsManager.viewScale = (double) width < (double) num3 ? num2 / 720f : num3 / 1280f;
        if ((double) SettingsManager.viewScale >= 1.0)
          break;
        SettingsManager.viewScale = 1f;
        break;
      case ScaleMode.PixelPerfect:
        if ((double) num1 > 1.7777777910232544)
        {
          SettingsManager.viewScale = Math.Max((float) (height / 720), 1f);
          break;
        }
        SettingsManager.viewScale = Math.Max((float) (width / 1280 /*0x0500*/), 1f);
        break;
      case ScaleMode.Supersampled:
        SettingsManager.viewScale = Math.Min((float) height / 720f, (float) width / 1280f);
        break;
    }
  }

  public static void UnsetupViewport(this GraphicsDevice device)
  {
    int backBufferWidth = device.PresentationParameters.BackBufferWidth;
    int backBufferHeight = device.PresentationParameters.BackBufferHeight;
    device.Viewport = new Viewport()
    {
      X = 0,
      Y = 0,
      Width = backBufferWidth,
      Height = backBufferHeight,
      MinDepth = 0.0f,
      MaxDepth = 1f
    };
    SettingsManager.viewScale = (float) device.Viewport.Width / 1280f;
  }

  public static Point PositionInViewport(this IMouseStateManager mouse)
  {
    Viewport viewport = ServiceHelper.Game.GraphicsDevice.Viewport;
    if (viewport.Width != ServiceHelper.Game.GraphicsDevice.PresentationParameters.BackBufferWidth && viewport.X == 0)
    {
      viewport.X = (ServiceHelper.Game.GraphicsDevice.PresentationParameters.BackBufferWidth - viewport.Width) / 2;
      viewport.Y = (ServiceHelper.Game.GraphicsDevice.PresentationParameters.BackBufferHeight - viewport.Height) / 2;
    }
    Point position = mouse.Position;
    return new Point((int) MathHelper.Clamp((float) (position.X - viewport.X), 0.0f, (float) viewport.Width), (int) MathHelper.Clamp((float) (position.Y - viewport.Y), 0.0f, (float) viewport.Height));
  }

  public static float GetViewScale(this GraphicsDevice _) => SettingsManager.viewScale;
}
