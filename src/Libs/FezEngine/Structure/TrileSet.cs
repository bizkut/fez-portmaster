// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrileSet
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization;
using ContentSerialization.Attributes;
using FezEngine.Effects.Structures;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure;

public class TrileSet : IDeserializationCallback, IDisposable
{
  public TrileSet() => this.Triles = new Dictionary<int, Trile>();

  public string Name { get; set; }

  public Dictionary<int, Trile> Triles { get; set; }

  [Serialization(Ignore = true)]
  public Texture2D TextureAtlas { get; set; }

  public Trile this[int id]
  {
    get => this.Triles[id];
    set => this.Triles[id] = value;
  }

  public void OnDeserialization()
  {
    foreach (int key in this.Triles.Keys)
    {
      this.Triles[key].TrileSet = this;
      this.Triles[key].Id = key;
    }
  }

  public void Dispose()
  {
    if (this.TextureAtlas != null)
    {
      this.TextureAtlas.Unhook();
      this.TextureAtlas.Dispose();
    }
    foreach (Trile trile in this.Triles.Values)
      trile.Dispose();
  }
}
