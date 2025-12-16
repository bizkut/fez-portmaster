// Decompiled with JetBrains decompiler
// Type: FezGame.Components.BridgeState
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

internal class BridgeState
{
  public readonly TrileInstance Instance;
  public Vector3 OriginalPosition;
  public float Downforce;
  public bool Dirty;

  public BridgeState(TrileInstance instance)
  {
    this.Instance = instance;
    this.OriginalPosition = instance.Position;
    if (instance.PhysicsState != null)
      return;
    instance.PhysicsState = new InstancePhysicsState(instance)
    {
      Sticky = true
    };
  }
}
