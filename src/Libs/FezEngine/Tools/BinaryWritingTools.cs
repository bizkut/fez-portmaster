// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.BinaryWritingTools
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using FezEngine.Structure;
using Microsoft.Xna.Framework;
using System;

#nullable disable
namespace FezEngine.Tools;

public static class BinaryWritingTools
{
  public static void WriteObject(this CrcWriter writer, string s)
  {
    writer.Write(s != null);
    if (s == null)
      return;
    writer.Write(s);
  }

  public static string ReadNullableString(this CrcReader reader)
  {
    return reader.ReadBoolean() ? reader.ReadString() : (string) null;
  }

  public static void WriteObject(this CrcWriter writer, float? s)
  {
    writer.Write(s.HasValue);
    if (!s.HasValue)
      return;
    writer.Write(s.Value);
  }

  public static float? ReadNullableSingle(this CrcReader reader)
  {
    return reader.ReadBoolean() ? new float?(reader.ReadSingle()) : new float?();
  }

  public static void Write(this CrcWriter writer, Vector3 s)
  {
    writer.Write(s.X);
    writer.Write(s.Y);
    writer.Write(s.Z);
  }

  public static Vector3 ReadVector3(this CrcReader reader)
  {
    return new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
  }

  public static void Write(this CrcWriter writer, TimeSpan s) => writer.Write(s.Ticks);

  public static TimeSpan ReadTimeSpan(this CrcReader reader) => new TimeSpan(reader.ReadInt64());

  public static void Write(this CrcWriter writer, TrileEmplacement s)
  {
    writer.Write(s.X);
    writer.Write(s.Y);
    writer.Write(s.Z);
  }

  public static TrileEmplacement ReadTrileEmplacement(this CrcReader reader)
  {
    return new TrileEmplacement(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
  }
}
