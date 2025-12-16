// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.BlendCombiner
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class BlendCombiner
{
  private readonly Dictionary<int, BlendState> stateObjectCache = new Dictionary<int, BlendState>();
  private BlendFunction colorBlendFunction;
  private Blend colorSourceBlend;
  private Blend colorDestinationBlend;
  private BlendingMode blendingMode = BlendingMode.Alphablending;

  public BlendFunction AlphaBlendFunction { get; set; }

  public Blend AlphaSourceBlend { get; set; }

  public Blend AlphaDestinationBlend { get; set; }

  public ColorWriteChannels ColorWriteChannels { get; set; }

  public BlendingMode BlendingMode
  {
    set
    {
      this.blendingMode = value;
      switch (value)
      {
        case BlendingMode.Additive:
          this.colorSourceBlend = BlendState.Additive.ColorSourceBlend;
          this.colorDestinationBlend = BlendState.Additive.ColorDestinationBlend;
          this.colorBlendFunction = BlendState.Additive.ColorBlendFunction;
          break;
        case BlendingMode.Screen:
          this.colorBlendFunction = BlendFunction.Add;
          this.colorSourceBlend = Blend.InverseDestinationColor;
          this.colorDestinationBlend = Blend.One;
          break;
        case BlendingMode.Multiply:
          this.colorBlendFunction = BlendFunction.Add;
          this.colorSourceBlend = Blend.DestinationColor;
          this.colorDestinationBlend = Blend.Zero;
          break;
        case BlendingMode.Alphablending:
          this.colorSourceBlend = BlendState.NonPremultiplied.ColorSourceBlend;
          this.colorDestinationBlend = BlendState.NonPremultiplied.ColorDestinationBlend;
          this.colorBlendFunction = BlendState.NonPremultiplied.ColorBlendFunction;
          break;
        case BlendingMode.Multiply2X:
          this.colorBlendFunction = BlendFunction.Add;
          this.colorSourceBlend = Blend.DestinationColor;
          this.colorDestinationBlend = Blend.SourceColor;
          break;
        case BlendingMode.Maximum:
          this.colorBlendFunction = BlendFunction.Max;
          this.colorSourceBlend = Blend.One;
          this.colorDestinationBlend = Blend.One;
          break;
        case BlendingMode.Minimum:
          this.colorBlendFunction = BlendFunction.Min;
          this.colorSourceBlend = Blend.One;
          this.colorDestinationBlend = Blend.One;
          break;
        case BlendingMode.Subtract:
          this.colorBlendFunction = BlendFunction.ReverseSubtract;
          this.colorSourceBlend = Blend.One;
          this.colorDestinationBlend = Blend.One;
          break;
        case BlendingMode.StarsOverClouds:
          this.colorBlendFunction = BlendFunction.Add;
          this.colorSourceBlend = Blend.One;
          this.colorDestinationBlend = Blend.InverseSourceColor;
          break;
        case BlendingMode.Opaque:
          this.colorSourceBlend = BlendState.Opaque.ColorSourceBlend;
          this.colorDestinationBlend = BlendState.Opaque.ColorDestinationBlend;
          this.colorBlendFunction = BlendState.Opaque.ColorBlendFunction;
          break;
        case BlendingMode.Lightmap:
          this.colorSourceBlend = BlendState.Opaque.ColorSourceBlend;
          this.colorDestinationBlend = BlendState.Opaque.ColorDestinationBlend;
          this.colorBlendFunction = BlendState.Opaque.ColorBlendFunction;
          break;
      }
    }
    get => this.blendingMode;
  }

  public BlendState Current => this.FindOrCreateStateObject(this.CalculateNewHash());

  internal void Apply(GraphicsDevice device)
  {
    int newHash = this.CalculateNewHash();
    device.BlendState = this.FindOrCreateStateObject(newHash);
  }

  private BlendState FindOrCreateStateObject(int hash)
  {
    BlendState createStateObject;
    if (!this.stateObjectCache.TryGetValue(hash, out createStateObject))
    {
      createStateObject = new BlendState()
      {
        ColorBlendFunction = this.colorBlendFunction,
        ColorSourceBlend = this.colorSourceBlend,
        ColorDestinationBlend = this.colorDestinationBlend,
        ColorWriteChannels = this.ColorWriteChannels,
        AlphaBlendFunction = this.AlphaBlendFunction,
        AlphaSourceBlend = this.AlphaSourceBlend,
        AlphaDestinationBlend = this.AlphaDestinationBlend
      };
      this.stateObjectCache.Add(hash, createStateObject);
    }
    return createStateObject;
  }

  private int CalculateNewHash()
  {
    return (int) (byte) this.colorBlendFunction | (int) (byte) this.colorSourceBlend << 3 | (int) (byte) this.colorDestinationBlend << 7 | (int) (byte) this.AlphaBlendFunction << 11 | (int) (byte) this.AlphaSourceBlend << 14 | (int) (byte) this.AlphaDestinationBlend << 18 | (int) (byte) this.ColorWriteChannels << 22;
  }
}
