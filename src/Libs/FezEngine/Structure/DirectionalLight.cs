// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.DirectionalLight
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public struct DirectionalLight
{
  public void Initialize() => this.Diffuse = this.Specular = Vector3.One;

  public Vector3 Direction { get; set; }

  public Vector3 Diffuse { get; set; }

  public Vector3 Specular { get; set; }
}
