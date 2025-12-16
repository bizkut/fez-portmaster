// Decompiled with JetBrains decompiler
// Type: FezGame.SpeedRun
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Diagnostics;
using System.IO;

#nullable disable
namespace FezGame;

public static class SpeedRun
{
  private static int CubeCount;
  private static string RunData;
  private static Stopwatch Timer;
  private static SpriteBatch Batch;
  private static Texture2D Font;
  private static bool Began;
  private static bool TimeCalled;
  private static readonly char[] chars = new char[12]
  {
    '0',
    '1',
    '2',
    '3',
    '4',
    '5',
    '6',
    '7',
    '8',
    '9',
    'C',
    'A'
  };
  private static Rectangle charSrc = new Rectangle(0, 0, 5, 5);
  private static readonly Rectangle colonSrc = new Rectangle(60, 0, 3, 5);

  public static void Begin(Texture2D font)
  {
    SpeedRun.CubeCount = 0;
    SpeedRun.RunData = string.Empty;
    SpeedRun.Timer = Stopwatch.StartNew();
    SpeedRun.Font = font;
    SpeedRun.Batch = new SpriteBatch(SpeedRun.Font.GraphicsDevice);
    SpeedRun.TimeCalled = false;
    SpeedRun.Began = true;
  }

  public static void Dispose()
  {
    SpeedRun.Began = false;
    if (SpeedRun.Batch == null)
      return;
    SpeedRun.Batch.Dispose();
    SpeedRun.Batch = (SpriteBatch) null;
  }

  public static void AddCube(bool anti)
  {
    if (!SpeedRun.Began)
      return;
    TimeSpan elapsed = SpeedRun.Timer.Elapsed;
    SpeedRun.RunData += string.Format("\n{5}{0:D2}: {1}:{2:D2}:{3:D2}:{4:D3}", (object) ++SpeedRun.CubeCount, (object) elapsed.Hours, (object) elapsed.Minutes, (object) elapsed.Seconds, (object) elapsed.Milliseconds, (object) (char) (anti ? 65 : 67));
  }

  public static void CallTime(string saveFolder)
  {
    if (!SpeedRun.Began)
      return;
    SpeedRun.TimeCalled = true;
    SpeedRun.Timer.Stop();
    TimeSpan elapsed = SpeedRun.Timer.Elapsed;
    DateTime now = DateTime.Now;
    File.WriteAllText(Path.Combine(saveFolder, $"RUN_{now.Day:D2}{now.Month:D2}{now.Year:D4}_{now.Hour:D2}{now.Minute:D2}{now.Second:D2}.txt"), string.Format("{0}:{1:D2}:{2:D2}:{3:D3}:{5:D4}{4}", (object) elapsed.Hours, (object) elapsed.Minutes, (object) elapsed.Seconds, (object) elapsed.Milliseconds, (object) SpeedRun.RunData.Replace("\n", Environment.NewLine), (object) (elapsed.Ticks % 10000L)));
  }

  public static void PauseForLoading()
  {
    if (!SpeedRun.Began)
      return;
    SpeedRun.Timer.Stop();
  }

  public static void ResumeAfterLoading()
  {
    if (!SpeedRun.Began || SpeedRun.TimeCalled)
      return;
    SpeedRun.Timer.Start();
  }

  public static void Draw(float scale)
  {
    if (!SpeedRun.Began)
      return;
    TimeSpan elapsed = SpeedRun.Timer.Elapsed;
    Viewport viewport = SpeedRun.Batch.GraphicsDevice.Viewport;
    Vector2 pos = new Vector2((float) (viewport.Width - (int) (250.0 * ((double) viewport.Width / 1280.0))), (float) (100 * (int) ((double) viewport.Height / 720.0)));
    string text = string.Format(SpeedRun.Timer.IsRunning ? "{0}:{1:D2}:{2:D2}:{3:D3}{4}" : "{0}:{1:D2}:{2:D2}:{3:D3}:{5:D4}{4}", (object) elapsed.Hours, (object) elapsed.Minutes, (object) elapsed.Seconds, (object) elapsed.Milliseconds, SpeedRun.CubeCount > 32 /*0x20*/ ? (object) SpeedRun.RunData.Substring(17 * (SpeedRun.CubeCount - 32 /*0x20*/)) : (object) SpeedRun.RunData, (object) (elapsed.Ticks % 10000L));
    SpeedRun.Batch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
    SpeedRun.DrawText(text, Color.Black, pos + new Vector2(scale, scale), scale);
    SpeedRun.DrawText(text, Color.White, pos, scale);
    SpeedRun.Batch.End();
  }

  private static void DrawText(string text, Color color, Vector2 pos, float scale)
  {
    float x = pos.X;
    foreach (char ch in text)
    {
      Rectangle? sourceRectangle;
      int num;
      switch (ch)
      {
        case '\n':
          pos.X = x;
          pos.Y += 15f * scale;
          continue;
        case ' ':
          pos.X += 9f * scale;
          continue;
        case ':':
          sourceRectangle = new Rectangle?(SpeedRun.colonSrc);
          pos.X -= 3f * scale;
          num = 8;
          break;
        default:
          SpeedRun.charSrc.X = 5 * Array.IndexOf<char>(SpeedRun.chars, ch);
          sourceRectangle = new Rectangle?(SpeedRun.charSrc);
          num = 15;
          break;
      }
      SpeedRun.Batch.Draw(SpeedRun.Font, pos, sourceRectangle, color, 0.0f, Vector2.Zero, 2f * scale, SpriteEffects.None, 0.0f);
      pos.X += (float) num * scale;
    }
  }
}
