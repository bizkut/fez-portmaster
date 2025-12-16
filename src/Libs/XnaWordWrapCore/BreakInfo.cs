// Decompiled with JetBrains decompiler
// Type: Microsoft.Xna.Framework.Graphics.Localization.BreakInfo
// Assembly: XnaWordWrapCore, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 6B8A288C-2178-4AAA-BC9E-71D510EB6454
// Assembly location: E:\GOG Games\Fez\XnaWordWrapCore.dll

using System;

#nullable disable
namespace Microsoft.Xna.Framework.Graphics.Localization;

public struct BreakInfo(uint character, bool isNonBeginningCharacter, bool isNonEndingCharacter)
{
  private uint m_Character = character <= 1114111U ? character : throw new ArgumentException("Invalid code point.");
  private bool m_IsNonBeginningCharacter = isNonBeginningCharacter;
  private bool m_IsNonEndingCharacter = isNonEndingCharacter;

  public uint Character => this.m_Character;

  public bool IsNonBeginningCharacter => this.m_IsNonBeginningCharacter;

  public bool IsNonEndingCharacter => this.m_IsNonEndingCharacter;
}
