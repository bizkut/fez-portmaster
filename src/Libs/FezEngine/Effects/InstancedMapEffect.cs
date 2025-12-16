// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.InstancedMapEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Effects.Structures;
using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class InstancedMapEffect : BaseEffect, IShaderInstantiatableEffect<Matrix>
{
  private bool lastWasComplete;
  private readonly SemanticMappedBoolean billboard;
  private readonly SemanticMappedMatrix cameraRotation;
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedMatrixArray instanceData;

  public InstancedMapEffect()
    : base(BaseEffect.UseHardwareInstancing ? "HwInstancedMapEffect" : nameof (InstancedMapEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.billboard = new SemanticMappedBoolean(this.effect.Parameters, nameof (Billboard));
    this.cameraRotation = new SemanticMappedMatrix(this.effect.Parameters, "CameraRotation");
    if (BaseEffect.UseHardwareInstancing)
      return;
    this.instanceData = new SemanticMappedMatrixArray(this.effect.Parameters, "InstanceData");
  }

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    this.texture.Set((Texture) mesh.Texture);
    this.cameraRotation.Set(Matrix.CreateFromQuaternion(this.CameraProvider.Rotation));
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    if (group.CustomData != null && (bool) group.CustomData)
    {
      this.GraphicsDeviceService.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Trails));
      this.lastWasComplete = true;
    }
    else
    {
      if (!this.lastWasComplete)
        return;
      this.GraphicsDeviceService.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    }
  }

  public bool Billboard
  {
    set => this.billboard.Set(value);
  }

  public void SetInstanceData(Matrix[] instances, int start, int batchInstanceCount)
  {
    this.instanceData.Set(instances, start, batchInstanceCount);
  }
}
