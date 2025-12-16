// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.TrileSetReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework.Content;
using System;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Readers;

public class TrileSetReader : ContentTypeReader<TrileSet>
{
  protected override TrileSet Read(ContentReader input, TrileSet existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new TrileSet();
    existingInstance.Name = input.ReadString();
    existingInstance.Triles = input.ReadObject<Dictionary<int, Trile>>(existingInstance.Triles);
    FutureTexture2D futureAtlas = input.ReadObject<FutureTexture2D>((ContentTypeReader) FutureTexture2DReader.Instance);
    DrawActionScheduler.Schedule((Action) (() => existingInstance.TextureAtlas = futureAtlas.Create()));
    existingInstance.OnDeserialization();
    return existingInstance;
  }
}
