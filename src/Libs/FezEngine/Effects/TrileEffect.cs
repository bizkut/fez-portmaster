// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.TrileEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class TrileEffect : BaseEffect, IShaderInstantiatableEffect<Vector4>
{
  private readonly SemanticMappedTexture textureAtlas;
  private readonly SemanticMappedBoolean blink;
  private readonly SemanticMappedBoolean unstable;
  private readonly SemanticMappedBoolean tiltTwoAxis;
  private readonly SemanticMappedBoolean shiny;
  private readonly SemanticMappedVectorArray instanceData;
  private readonly bool InEditor;
  private static readonly TrileCustomData DefaultCustom = new TrileCustomData();
  private bool lastWasCustom;

  public TrileEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwTrileEffect" : nameof (TrileEffect))
  {
    this.textureAtlas = new SemanticMappedTexture(this.effect.Parameters, "AtlasTexture");
    this.blink = new SemanticMappedBoolean(this.effect.Parameters, nameof (Blink));
    this.unstable = new SemanticMappedBoolean(this.effect.Parameters, "Unstable");
    this.tiltTwoAxis = new SemanticMappedBoolean(this.effect.Parameters, "TiltTwoAxis");
    this.shiny = new SemanticMappedBoolean(this.effect.Parameters, "Shiny");
    if (!BaseEffect.UseHardwareInstancing)
      this.instanceData = new SemanticMappedVectorArray(this.effect.Parameters, "InstanceData");
    this.InEditor = this.EngineState.InEditor;
    this.Pass = LightingEffectPass.Main;
    this.SimpleGroupPrepare = true;
    this.material.Opacity = 1f;
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.textureAtlas.Set((Texture) mesh.Texture);
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    if (this.InEditor)
      this.textureAtlas.Set(group.Texture);
    if (!(group.CustomData is TrileCustomData trileCustomData1))
      trileCustomData1 = TrileEffect.DefaultCustom;
    TrileCustomData trileCustomData2 = trileCustomData1;
    bool isCustom = trileCustomData2.IsCustom;
    if (!(this.lastWasCustom | isCustom))
      return;
    this.unstable.Set(trileCustomData2.Unstable);
    this.shiny.Set(trileCustomData2.Shiny);
    this.tiltTwoAxis.Set(trileCustomData2.TiltTwoAxis);
    this.lastWasCustom = isCustom;
  }

  public void SetInstanceData(Vector4[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 0 : 1];
  }

  public bool Blink
  {
    set => this.blink.Set(value);
  }
}
