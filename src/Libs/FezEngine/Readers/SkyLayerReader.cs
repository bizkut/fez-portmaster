// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.SkyLayerReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class SkyLayerReader : ContentTypeReader<SkyLayer>
{
  protected override SkyLayer Read(ContentReader input, SkyLayer existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new SkyLayer();
    existingInstance.Name = input.ReadString();
    existingInstance.InFront = input.ReadBoolean();
    existingInstance.Opacity = input.ReadSingle();
    existingInstance.FogTint = input.ReadSingle();
    return existingInstance;
  }
}
