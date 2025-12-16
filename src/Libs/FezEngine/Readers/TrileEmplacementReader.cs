// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.TrileEmplacementReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class TrileEmplacementReader : ContentTypeReader<TrileEmplacement>
{
  protected override TrileEmplacement Read(ContentReader input, TrileEmplacement existingInstance)
  {
    return new TrileEmplacement()
    {
      X = input.ReadInt32(),
      Y = input.ReadInt32(),
      Z = input.ReadInt32()
    };
  }
}
