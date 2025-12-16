// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.SamplerStates
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Tools;

public static class SamplerStates
{
  public static readonly SamplerState PointMipWrap = new SamplerState()
  {
    AddressU = TextureAddressMode.Wrap,
    AddressV = TextureAddressMode.Wrap,
    Filter = TextureFilter.MinLinearMagPointMipLinear
  };
  public static readonly SamplerState PointMipClamp = new SamplerState()
  {
    AddressU = TextureAddressMode.Clamp,
    AddressV = TextureAddressMode.Clamp,
    Filter = TextureFilter.MinLinearMagPointMipLinear
  };
  public static readonly SamplerState LinearUWrapVClamp = new SamplerState()
  {
    AddressU = TextureAddressMode.Wrap,
    AddressV = TextureAddressMode.Clamp,
    Filter = TextureFilter.Linear
  };
  public static readonly SamplerState PointUWrapVClamp = new SamplerState()
  {
    AddressU = TextureAddressMode.Wrap,
    AddressV = TextureAddressMode.Clamp,
    Filter = TextureFilter.Point
  };
}
