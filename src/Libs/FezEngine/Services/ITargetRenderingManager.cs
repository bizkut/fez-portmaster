// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.ITargetRenderingManager
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

#nullable disable
namespace FezEngine.Services;

public interface ITargetRenderingManager
{
  event Action<GameTime> PreDraw;

  void OnPreDraw(GameTime gameTime);

  bool HasRtInQueue { get; }

  void OnRtPrepare();

  void ScheduleHook(int drawOrder, RenderTarget2D rt);

  void UnscheduleHook(RenderTarget2D rt);

  void Resolve(RenderTarget2D rt, bool reschedule);

  bool IsHooked(RenderTarget2D rt);

  RenderTargetHandle TakeTarget();

  void ReturnTarget(RenderTargetHandle handle);

  void DrawFullscreen(Color color);

  void DrawFullscreen(Texture texture);

  void DrawFullscreen(Texture texture, Color color);

  void DrawFullscreen(Texture texture, Matrix textureMatrix);

  void DrawFullscreen(Texture texture, Matrix textureMatrix, Color color);

  void DrawFullscreen(BaseEffect effect);

  void DrawFullscreen(BaseEffect effect, Color color);

  void DrawFullscreen(BaseEffect effect, Texture texture);

  void DrawFullscreen(BaseEffect effect, Texture texture, Matrix? textureMatrix);

  void DrawFullscreen(BaseEffect effect, Texture texture, Matrix? textureMatrix, Color color);
}
