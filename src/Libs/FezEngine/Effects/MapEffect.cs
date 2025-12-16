// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.MapEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using FezEngine.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects;

public class MapEffect : BaseEffect
{
  private bool lastWasComplete;
  private Vector3 lastDiffuse;

  public MapEffect()
    : base(nameof (MapEffect))
  {
  }

  public override void Prepare(Group group)
  {
    base.Prepare(group);
    if (group.CustomData != null && (bool) group.CustomData)
    {
      this.GraphicsDeviceService.GraphicsDevice.PrepareStencilWrite(new FezEngine.Structure.StencilMask?(FezEngine.Structure.StencilMask.Trails));
      this.lastWasComplete = true;
    }
    else if (this.lastWasComplete)
      this.GraphicsDeviceService.GraphicsDevice.PrepareStencilRead(CompareFunction.Always, FezEngine.Structure.StencilMask.None);
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
    if (group.Material == null)
      return;
    this.material.Opacity = group.Mesh.Material.Opacity * group.Material.Opacity;
  }
}
