// Decompiled with JetBrains decompiler
// Type: FezGame.Components.IPlaneParticleSystems
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Components;
using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezGame.Components;

public interface IPlaneParticleSystems
{
  PlaneParticleSystem RainSplash(Vector3 center);

  void Splash(IPhysicsEntity entity, bool outwards);

  void Splash(IPhysicsEntity entity, bool outwards, float velocityBonus);

  void Add(PlaneParticleSystem system);

  void Remove(PlaneParticleSystem system, bool returnToPool);

  void ForceDraw();
}
