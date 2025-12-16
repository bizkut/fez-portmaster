// Decompiled with JetBrains decompiler
// Type: FezGame.Components.ITrixelParticleSystems
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

public interface ITrixelParticleSystems
{
  void Add(TrixelParticleSystem system);

  void PropagateEnergy(Vector3 energySource, float energy);

  void UnGroundAll();

  int Count { get; }

  void ForceDraw();
}
