// Decompiled with JetBrains decompiler
// Type: FezEngine.Readers.BackgroundPlaneReader
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework.Content;

#nullable disable
namespace FezEngine.Readers;

public class BackgroundPlaneReader : ContentTypeReader<BackgroundPlane>
{
  protected override BackgroundPlane Read(ContentReader input, BackgroundPlane existingInstance)
  {
    if (existingInstance == null)
      existingInstance = new BackgroundPlane();
    existingInstance.Position = input.ReadVector3();
    existingInstance.Rotation = input.ReadQuaternion();
    existingInstance.Scale = input.ReadVector3();
    existingInstance.Size = input.ReadVector3();
    existingInstance.TextureName = input.ReadString();
    existingInstance.LightMap = input.ReadBoolean();
    existingInstance.AllowOverbrightness = input.ReadBoolean();
    existingInstance.Filter = input.ReadColor();
    existingInstance.Animated = input.ReadBoolean();
    existingInstance.Doublesided = input.ReadBoolean();
    existingInstance.Opacity = input.ReadSingle();
    existingInstance.AttachedGroup = input.ReadObject<int?>();
    existingInstance.Billboard = input.ReadBoolean();
    existingInstance.SyncWithSamples = input.ReadBoolean();
    existingInstance.Crosshatch = input.ReadBoolean();
    input.ReadBoolean();
    existingInstance.AlwaysOnTop = input.ReadBoolean();
    existingInstance.Fullbright = input.ReadBoolean();
    existingInstance.PixelatedLightmap = input.ReadBoolean();
    existingInstance.XTextureRepeat = input.ReadBoolean();
    existingInstance.YTextureRepeat = input.ReadBoolean();
    existingInstance.ClampTexture = input.ReadBoolean();
    existingInstance.ActorType = input.ReadObject<ActorType>();
    existingInstance.AttachedPlane = input.ReadObject<int?>();
    existingInstance.ParallaxFactor = input.ReadSingle();
    return existingInstance;
  }
}
