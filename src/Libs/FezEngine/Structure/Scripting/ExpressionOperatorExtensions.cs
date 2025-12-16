// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.ExpressionOperatorExtensions
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Structure.Scripting;

public static class ExpressionOperatorExtensions
{
  public static string ToSymbol(this ComparisonOperator op)
  {
    switch (op)
    {
      case ComparisonOperator.None:
        return "";
      case ComparisonOperator.Equal:
        return "=";
      case ComparisonOperator.Greater:
        return ">";
      case ComparisonOperator.GreaterEqual:
        return ">=";
      case ComparisonOperator.Less:
        return "<";
      case ComparisonOperator.LessEqual:
        return "<=";
      case ComparisonOperator.NotEqual:
        return "!=";
      default:
        throw new InvalidOperationException();
    }
  }
}
