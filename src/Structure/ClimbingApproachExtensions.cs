// Decompiled with JetBrains decompiler
// Type: FezGame.Structure.ClimbingApproachExtensions
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine;

#nullable disable
namespace FezGame.Structure;

public static class ClimbingApproachExtensions
{
  public static HorizontalDirection AsDirection(this ClimbingApproach approach)
  {
    if (approach == ClimbingApproach.Left)
      return HorizontalDirection.Left;
    return HorizontalDirection.Right;
  }
}
