// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.KeysEqualityComparer
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public class KeysEqualityComparer : IEqualityComparer<Keys>
{
  public static readonly KeysEqualityComparer Default = new KeysEqualityComparer();

  public bool Equals(Keys x, Keys y) => x == y;

  public int GetHashCode(Keys obj) => (int) obj;
}
