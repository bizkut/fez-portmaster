// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.RasterizerCombiner
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class RasterizerCombiner
{
  private readonly Dictionary<long, RasterizerState> stateObjectCache = new Dictionary<long, RasterizerState>();

  public CullMode CullMode { get; set; }

  public FillMode FillMode { get; set; }

  public float DepthBias { get; set; }

  public float SlopeScaleDepthBias { get; set; }

  public RasterizerState Current => this.FindOrCreateStateObject(this.CalculateNewHash());

  internal void Apply(GraphicsDevice device)
  {
    long newHash = this.CalculateNewHash();
    device.RasterizerState = this.FindOrCreateStateObject(newHash);
  }

  private RasterizerState FindOrCreateStateObject(long hash)
  {
    RasterizerState createStateObject;
    if (!this.stateObjectCache.TryGetValue(hash, out createStateObject))
    {
      createStateObject = new RasterizerState()
      {
        CullMode = this.CullMode,
        FillMode = this.FillMode,
        DepthBias = this.DepthBias,
        SlopeScaleDepthBias = this.SlopeScaleDepthBias
      };
      this.stateObjectCache.Add(hash, createStateObject);
    }
    return createStateObject;
  }

  private long CalculateNewHash()
  {
    return (long) ((int) BitConverter.DoubleToInt64Bits((double) this.DepthBias) >> 4 | (int) BitConverter.DoubleToInt64Bits((double) this.SlopeScaleDepthBias) >> 4 << 30 | (int) (byte) this.CullMode << 28 | (int) (byte) this.FillMode << 30);
  }
}
