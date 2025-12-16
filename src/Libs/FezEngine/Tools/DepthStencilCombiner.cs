// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.DepthStencilCombiner
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Tools;

public class DepthStencilCombiner
{
  private readonly Dictionary<int, DepthStencilState> stateObjectCache = new Dictionary<int, DepthStencilState>();

  public bool DepthBufferEnable { get; set; }

  public CompareFunction DepthBufferFunction { get; set; }

  public bool DepthBufferWriteEnable { get; set; }

  public bool StencilEnable { get; set; }

  public StencilOperation StencilPass { get; set; }

  public CompareFunction StencilFunction { get; set; }

  public int ReferenceStencil { get; set; }

  public DepthStencilState Current => this.FindOrCreateStateObject(this.CalculateNewHash());

  internal void Apply(GraphicsDevice device)
  {
    int newHash = this.CalculateNewHash();
    device.DepthStencilState = this.FindOrCreateStateObject(newHash);
  }

  private DepthStencilState FindOrCreateStateObject(int hash)
  {
    DepthStencilState createStateObject;
    if (!this.stateObjectCache.TryGetValue(hash, out createStateObject))
    {
      createStateObject = new DepthStencilState()
      {
        DepthBufferEnable = this.DepthBufferEnable,
        DepthBufferWriteEnable = this.DepthBufferWriteEnable,
        DepthBufferFunction = this.DepthBufferFunction,
        StencilEnable = this.StencilEnable,
        StencilPass = this.StencilPass,
        StencilFunction = this.StencilFunction,
        ReferenceStencil = this.ReferenceStencil
      };
      this.stateObjectCache.Add(hash, createStateObject);
    }
    return createStateObject;
  }

  private int CalculateNewHash()
  {
    return (this.DepthBufferEnable ? 1 : 0) | (int) (byte) this.DepthBufferFunction << 1 | (this.DepthBufferWriteEnable ? 1 : 0) << 5 | (this.StencilEnable ? 1 : 0) << 6 | (int) (byte) this.StencilPass << 7 | (int) (byte) this.StencilFunction << 11 | (int) (byte) this.ReferenceStencil << 15;
  }
}
