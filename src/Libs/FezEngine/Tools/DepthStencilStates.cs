// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.DepthStencilStates
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Tools;

public static class DepthStencilStates
{
  public static readonly DepthStencilState DefaultWithStencil = new DepthStencilState()
  {
    StencilEnable = true
  };
}
