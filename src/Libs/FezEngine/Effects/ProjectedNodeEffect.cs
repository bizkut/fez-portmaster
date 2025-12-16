// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.ProjectedNodeEffect
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

public class ProjectedNodeEffect : BaseEffect
{
  private readonly SemanticMappedTexture texture;
  private readonly SemanticMappedVector2 textureSize;
  private readonly SemanticMappedVector2 viewportSize;
  private readonly SemanticMappedVector3 cubeOffset;
  private readonly SemanticMappedSingle pixPerTrix;
  private readonly SemanticMappedBoolean noTexture;
  private readonly SemanticMappedBoolean complete;
  private Vector3 lastDiffuse;
  private bool lastWasComplete;

  public ProjectedNodeEffect()
    : base(nameof (ProjectedNodeEffect))
  {
    this.texture = new SemanticMappedTexture(this.effect.Parameters, "BaseTexture");
    this.textureSize = new SemanticMappedVector2(this.effect.Parameters, "TextureSize");
    this.viewportSize = new SemanticMappedVector2(this.effect.Parameters, "ViewportSize");
    this.cubeOffset = new SemanticMappedVector3(this.effect.Parameters, "CubeOffset");
    this.pixPerTrix = new SemanticMappedSingle(this.effect.Parameters, "PixelsPerTrixel");
    this.noTexture = new SemanticMappedBoolean(this.effect.Parameters, "NoTexture");
    this.complete = new SemanticMappedBoolean(this.effect.Parameters, "Complete");
    this.Pass = LightingEffectPass.Main;
  }

  public override BaseEffect Clone() => (BaseEffect) new ProjectedNodeEffect();

  public override void Prepare(Mesh mesh)
  {
    base.Prepare(mesh);
    float viewScale = this.GraphicsDeviceService.GraphicsDevice.GetViewScale();
    float num1 = (float) this.GraphicsDeviceService.GraphicsDevice.Viewport.Width / (1280f * viewScale);
    float num2 = (float) this.GraphicsDeviceService.GraphicsDevice.Viewport.Height / (720f * viewScale);
    Viewport viewport = this.GraphicsDeviceService.GraphicsDevice.Viewport;
    this.matrices.ViewProjection = this.viewProjection;
    this.pixPerTrix.Set((float) ((double) this.CameraProvider.Radius / (double) num1 / 45.0 * 18.0) / viewScale);
    this.viewportSize.Set(new Vector2(1280f * num1, 720f * num2));
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    this.cubeOffset.Set(Vector3.Transform(Vector3.Zero, (Matrix) group.WorldMatrix));
    if (this.IgnoreCache || !group.EffectOwner || group.InverseTransposeWorldMatrix.Dirty)
    {
      this.matrices.WorldInverseTranspose = (Matrix) group.InverseTransposeWorldMatrix;
      group.InverseTransposeWorldMatrix.Clean();
    }
    if (group.Material != null)
    {
      if (this.lastDiffuse != group.Material.Diffuse)
      {
        this.material.Diffuse = group.Material.Diffuse;
        this.lastDiffuse = group.Material.Diffuse;
      }
    }
    else
      this.material.Diffuse = group.Mesh.Material.Diffuse;
    if (group.Material != null)
      this.material.Opacity = group.Mesh.Material.Opacity * group.Material.Opacity;
    this.noTexture.Set(group.TexturingType != TexturingType.Texture2D);
    bool complete = (group.CustomData as NodeGroupData).Complete;
    this.complete.Set(complete);
    if (complete)
    {
      this.GraphicsDeviceService.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Trails));
      this.lastWasComplete = true;
    }
    else if (this.lastWasComplete)
      this.GraphicsDeviceService.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
    if (group.TexturingType != TexturingType.Texture2D)
      return;
    this.texture.Set(group.Texture);
    this.textureSize.Set(new Vector2((float) group.TextureMap.Width, (float) group.TextureMap.Height));
  }

  public LightingEffectPass Pass
  {
    set => this.currentPass = this.currentTechnique.Passes[value == LightingEffectPass.Pre ? 1 : 0];
  }
}
