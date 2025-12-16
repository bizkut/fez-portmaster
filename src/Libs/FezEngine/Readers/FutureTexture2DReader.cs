// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.FutureTexture2DReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

#nullable disable
namespace FezEngine.Readers;

public class FutureTexture2DReader : ContentTypeReader<FutureTexture2D>
{
  public static readonly FutureTexture2DReader Instance = new FutureTexture2DReader();

  protected override FutureTexture2D Read(ContentReader input, FutureTexture2D existingInstance)
  {
    do
      ;
    while ((int) input.ReadByte() >> 7 == 1);
    FutureTexture2D futureTexture2D = new FutureTexture2D()
    {
      BackingStream = (input.BaseStream as MemoryStream).GetBuffer(),
      Format = (SurfaceFormat) input.ReadInt32(),
      Width = input.ReadInt32(),
      Height = input.ReadInt32(),
      MipLevels = new FutureTexture2D.MipLevel[input.ReadInt32()]
    };
    for (int index = 0; index < futureTexture2D.MipLevels.Length; ++index)
    {
      futureTexture2D.MipLevels[index].SizeInBytes = input.ReadInt32();
      futureTexture2D.MipLevels[index].StreamOffset = input.BaseStream.Position;
      input.BaseStream.Seek((long) futureTexture2D.MipLevels[index].SizeInBytes, SeekOrigin.Current);
    }
    return futureTexture2D;
  }
}
