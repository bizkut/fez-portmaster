// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.TrileFaceReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class TrileFaceReader : ContentTypeReader<TrileFace>
{
  protected override TrileFace Read(ContentReader input, TrileFace existingInstance)
  {
    if (existingInstance == (TrileFace) null)
      existingInstance = new TrileFace();
    existingInstance.Id = input.ReadObject<TrileEmplacement>();
    existingInstance.Face = input.ReadObject<FaceOrientation>();
    return existingInstance;
  }
}
