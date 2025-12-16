// Decompiled with JetBrains decompiler
// Type: Common.Crc32
// Assembly: Common, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BC7A950C-D861-40F4-B8D6-28776BD88C9A
// Assembly location: E:\GOG Games\Fez\Common.dll

#nullable disable
namespace Common;

public class Crc32
{
  private static readonly uint[] table = new uint[256 /*0x0100*/];

  static Crc32()
  {
    for (uint index1 = 0; (long) index1 < (long) Crc32.table.Length; ++index1)
    {
      uint num = index1;
      for (int index2 = 8; index2 > 0; --index2)
      {
        if (((int) num & 1) == 1)
          num = num >> 1 ^ 3988292384U;
        else
          num >>= 1;
      }
      Crc32.table[(int) index1] = num;
    }
  }

  public static uint ComputeChecksum(byte[] bytes)
  {
    uint num = uint.MaxValue;
    for (int index1 = 0; index1 < bytes.Length; ++index1)
    {
      byte index2 = (byte) (num & (uint) byte.MaxValue ^ (uint) bytes[index1]);
      num = num >> 8 ^ Crc32.table[(int) index2];
    }
    return ~num;
  }
}
