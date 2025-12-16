// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.RenderTargetHandle
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Services;

public class RenderTargetHandle
{
  public RenderTarget2D Target { get; set; }

  public bool Locked { get; set; }
}
