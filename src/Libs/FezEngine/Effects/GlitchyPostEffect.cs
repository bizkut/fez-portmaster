// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.GlitchyPostEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class GlitchyPostEffect : BaseEffect, IShaderInstantiatableEffect<Matrix>
{
  private readonly SemanticMappedTexture glitchTexture;
  private readonly SemanticMappedMatrixArray instanceData;

  public GlitchyPostEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwGlitchyPostEffect" : nameof (GlitchyPostEffect))
  {
    this.glitchTexture = new SemanticMappedTexture(this.effect.Parameters, "GlitchTexture");
    if (!BaseEffect.UseHardwareInstancing)
      this.instanceData = new SemanticMappedMatrixArray(this.effect.Parameters, "InstanceData");
    this.SimpleGroupPrepare = true;
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.glitchTexture.Set((Texture) mesh.Texture);
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    this.Apply();
  }

  public void SetInstanceData(Matrix[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }
}
