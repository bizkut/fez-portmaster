// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.PointCollision
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Services;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Structure;

public struct PointCollision(Vector3 point, NearestTriles instances)
{
  public readonly Vector3 Point = point;
  public readonly NearestTriles Instances = instances;
}
