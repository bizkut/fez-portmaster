// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.AnimatedTextureReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Content;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Readers;

public class AnimatedTextureReader : ContentTypeReader<AnimatedTexture>
{
  protected override AnimatedTexture Read(ContentReader input, AnimatedTexture existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new AnimatedTexture();
    GraphicsDevice graphicsDevice = ((IGraphicsDeviceService) input.ContentManager.ServiceProvider.GetService(typeof (IGraphicsDeviceService))).GraphicsDevice;
    int width = input.ReadInt32();
    int height = input.ReadInt32();
    existingInstance.FrameWidth = input.ReadInt32();
    existingInstance.FrameHeight = input.ReadInt32();
    byte[] packedImageBytes = input.ReadBytes(input.ReadInt32());
    List<FrameContent> source = input.ReadObject<List<FrameContent>>();
    DrawActionScheduler.Schedule((Action) (() =>
    {
      existingInstance.Texture = new Texture2D(graphicsDevice, width, height, false, SurfaceFormat.Color);
      existingInstance.Texture.SetData<byte>(packedImageBytes);
    }));
    existingInstance.Offsets = source.Select<FrameContent, Rectangle>((Func<FrameContent, Rectangle>) (x => x.Rectangle)).ToArray<Rectangle>();
    existingInstance.Timing = new AnimationTiming(0, source.Count - 1, source.Select<FrameContent, float>((Func<FrameContent, float>) (x => (float) x.Duration.TotalSeconds)).ToArray<float>());
    existingInstance.PotOffset = new Vector2((float) (FezMath.NextPowerOfTwo((double) existingInstance.FrameWidth) - existingInstance.FrameWidth), (float) (FezMath.NextPowerOfTwo((double) existingInstance.FrameHeight) - existingInstance.FrameHeight));
    return existingInstance;
  }
}
