// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Input.CodeInput
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using System;

#nullable disable
namespace FezEngine.Structure.Input;

[Flags]
public enum CodeInput
{
  None = 0,
  Up = 1,
  Down = 2,
  Left = 4,
  Right = 8,
  SpinLeft = 16, // 0x00000010
  SpinRight = 32, // 0x00000020
  Jump = 64, // 0x00000040
}
