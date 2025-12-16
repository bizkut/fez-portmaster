// Decompiled with JetBrains decompiler
// Type: FezEngine.Content.AnimatedTextureContent
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Content;

public class AnimatedTextureContent
{
  public readonly List<FrameContent> Frames = new List<FrameContent>();
  public int FrameWidth;
  public int FrameHeight;
  public int Width;
  public int Height;
  public byte[] PackedImage;
}
