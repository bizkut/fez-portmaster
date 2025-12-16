// Decompiled with JetBrains decompiler
// Type: FezEngine.Components.FarawayPlaceData
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework;

#nullable disable
namespace FezEngine.Components;

internal struct FarawayPlaceData
{
  public Mesh WaterBodyMesh;
  public Vector3 OriginalCenter;
  public Viewpoint Viewpoint;
  public Volume Volume;
  public Vector3 DestinationOffset;
  public float? WaterLevelOffset;
  public string DestinationLevelName;
  public float DestinationWaterLevel;
  public float DestinationLevelSize;
}
