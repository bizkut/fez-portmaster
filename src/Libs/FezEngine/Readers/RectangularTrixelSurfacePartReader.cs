// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.RectangularTrixelSurfacePartReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class RectangularTrixelSurfacePartReader : ContentTypeReader<RectangularTrixelSurfacePart>
{
  protected override RectangularTrixelSurfacePart Read(
    ContentReader input,
    RectangularTrixelSurfacePart existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new RectangularTrixelSurfacePart();
    existingInstance.Start = input.ReadTrixelIdentifier();
    existingInstance.Orientation = input.ReadObject<FaceOrientation>();
    existingInstance.TangentSize = input.ReadInt32();
    existingInstance.BitangentSize = input.ReadInt32();
    return existingInstance;
  }
}
