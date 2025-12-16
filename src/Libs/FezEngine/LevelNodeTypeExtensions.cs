// Decompiled with JetBrains decompiler
// Type: FezEngine.LevelNodeTypeExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine;

public static class LevelNodeTypeExtensions
{
  public static float GetSizeFactor(this LevelNodeType nodeType)
  {
    switch (nodeType)
    {
      case LevelNodeType.Node:
        return 1f;
      case LevelNodeType.Hub:
        return 2f;
      case LevelNodeType.Lesser:
        return 0.5f;
      default:
        throw new InvalidOperationException();
    }
  }
}
