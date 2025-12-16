// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.CloudLayerExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Components;

internal static class CloudLayerExtensions
{
  public static float SpeedFactor(this Layer layer)
  {
    if (layer == Layer.Middle)
      return 0.6f;
    return layer == Layer.Near ? 1f : 0.2f;
  }

  public static float DistanceFactor(this Layer layer)
  {
    if (layer == Layer.Middle)
      return 0.5f;
    return layer == Layer.Near ? 0.0f : 1f;
  }

  public static float ParallaxFactor(this Layer layer)
  {
    if (layer == Layer.Middle)
      return 0.4f;
    return layer == Layer.Near ? 0.2f : 0.6f;
  }

  public static float Opacity(this Layer layer)
  {
    if (layer == Layer.Middle)
      return 0.6f;
    return layer == Layer.Near ? 1f : 0.3f;
  }
}
