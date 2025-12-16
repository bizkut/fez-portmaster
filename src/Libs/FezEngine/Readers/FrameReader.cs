// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.FrameReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using System;

#nullable disable
namespace FezEngine.Readers;

public class FrameReader : ContentTypeReader<FrameContent>
{
  protected override FrameContent Read(ContentReader input, FrameContent existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new FrameContent();
    existingInstance.Duration = input.ReadObject<TimeSpan>();
    existingInstance.Rectangle = input.ReadObject<Rectangle>();
    return existingInstance;
  }
}
