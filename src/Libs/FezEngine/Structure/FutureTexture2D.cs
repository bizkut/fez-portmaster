// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.FutureTexture2D
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Structure;

public class FutureTexture2D
{
  public SurfaceFormat Format;
  public int Width;
  public int Height;
  public byte[] BackingStream;
  public FutureTexture2D.MipLevel[] MipLevels;

  public Texture2D Create()
  {
    Texture2D texture2D = new Texture2D(ServiceHelper.Game.GraphicsDevice, this.Width, this.Height, this.MipLevels.Length > 1, this.Format);
    for (int level = 0; level < this.MipLevels.Length; ++level)
      texture2D.SetData<byte>(level, new Rectangle?(), this.BackingStream, (int) this.MipLevels[level].StreamOffset, this.MipLevels[level].SizeInBytes);
    return texture2D;
  }

  public struct MipLevel
  {
    public long StreamOffset;
    public int SizeInBytes;
  }
}
