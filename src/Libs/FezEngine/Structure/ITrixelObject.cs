// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.ITrixelObject
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public interface ITrixelObject
{
  bool TrixelExists(TrixelEmplacement trixelIdentifier);

  bool CanContain(TrixelEmplacement trixel);

  bool IsBorderTrixelFace(TrixelEmplacement id, FaceOrientation face);

  bool IsBorderTrixelFace(TrixelEmplacement traversed);

  Vector3 Size { get; }

  TrixelCluster MissingTrixels { get; }
}
