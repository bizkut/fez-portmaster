// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.LiquidTypeExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Structure;

public static class LiquidTypeExtensions
{
  public static bool IsWater(this LiquidType waterType)
  {
    return waterType == LiquidType.Blood || waterType == LiquidType.Water || waterType == LiquidType.Purple || waterType == LiquidType.Green;
  }
}
