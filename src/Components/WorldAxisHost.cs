// Decompiled with JetBrains decompiler
// Type: FezGame.Components.WorldAxisHost
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;
using FezEngine.Effects;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

internal class WorldAxisHost(Game game) : DrawableGameComponent(game)
{
  private Mesh axisMesh;

  public override void Initialize()
  {
    this.axisMesh = new Mesh() { AlwaysOnTop = true };
    this.axisMesh.AddWireframeArrow(1f, 0.1f, Vector3.Zero, FaceOrientation.Right, Color.Red);
    this.axisMesh.AddWireframeArrow(1f, 0.1f, Vector3.Zero, FaceOrientation.Top, Color.Green);
    this.axisMesh.AddWireframeArrow(1f, 0.1f, Vector3.Zero, FaceOrientation.Front, Color.Blue);
    this.DrawOrder = 1000;
    base.Initialize();
  }

  protected override void LoadContent()
  {
    this.axisMesh.Effect = (BaseEffect) new DefaultEffect.VertexColored();
  }

  public override void Draw(GameTime gameTime) => this.axisMesh.Draw();
}
