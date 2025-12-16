// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.VirtualTrile
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public struct VirtualTrile
{
  public int VerticalOffset;

  public TrileInstance Instance { get; set; }

  public Vector3 Position
  {
    get => this.Instance.Position + new Vector3(0.0f, (float) this.VerticalOffset, 0.0f);
  }
}
