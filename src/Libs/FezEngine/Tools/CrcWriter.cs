// Decompiled with JetBrains decompiler
// Type: FezEngine.Tools.CrcWriter
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

#nullable disable
namespace FezEngine.Tools;

public class CrcWriter
{
  private readonly BinaryWriter writer;
  private readonly List<byte> writtenBytes = new List<byte>();

  public CrcWriter(BinaryWriter writer) => this.writer = writer;

  public void Write(bool value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(byte value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes((short) value));
    this.writer.Write(value);
  }

  public void Write(byte[] buffer)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) buffer);
    this.writer.Write(buffer);
  }

  public void Write(double value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(float value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(int value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(long value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(sbyte value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes((short) value));
    this.writer.Write(value);
  }

  public void Write(short value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(string value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) Encoding.Unicode.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(uint value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(ulong value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void Write(ushort value)
  {
    this.writtenBytes.AddRange((IEnumerable<byte>) BitConverter.GetBytes(value));
    this.writer.Write(value);
  }

  public void WriteHash() => this.Write(Crc32.ComputeChecksum(this.writtenBytes.ToArray()));
}
