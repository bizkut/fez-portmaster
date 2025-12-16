// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.CodeInputComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System.Collections.Generic;

#nullable disable
namespace FezEngine.Structure.Input;

public class CodeInputComparer : IEqualityComparer<CodeInput>
{
  public static readonly CodeInputComparer Default = new CodeInputComparer();

  public bool Equals(CodeInput x, CodeInput y) => x == y;

  public int GetHashCode(CodeInput obj) => (int) obj;
}
