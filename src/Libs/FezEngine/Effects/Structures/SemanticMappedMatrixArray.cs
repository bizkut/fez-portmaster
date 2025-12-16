// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.SemanticMappedMatrixArray
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Reflection;

#nullable disable
namespace FezEngine.Effects.Structures;

public class SemanticMappedMatrixArray : 
  SemanticMappedParameter<Matrix[]>,
  SemanticMappedArrayParameter<Matrix[]>
{
  private IntPtr raw;

  public SemanticMappedMatrixArray(EffectParameterCollection parent, string semanticName)
    : base(parent, semanticName)
  {
    this.raw = (IntPtr) ReflectionHelper.GetValue(this.parameter.GetType().GetMember("values", BindingFlags.Instance | BindingFlags.NonPublic)[0], (object) this.parameter);
  }

  protected override void DoSet(Matrix[] value) => this.Set(value, 0, value.Length);

  protected override void DoSet(Matrix[] value, int length) => this.Set(value, 0, length);

  public unsafe void Set(Matrix[] value, int start, int length)
  {
    int columnCount = this.parameter.ColumnCount;
    int rowCount = this.parameter.RowCount;
    float* raw = (float*) (void*) this.raw;
    if (columnCount == 4 && rowCount == 4)
    {
      int index = start;
      while (index < start + length)
      {
        *raw = value[index].M11;
        raw[1] = value[index].M21;
        raw[2] = value[index].M31;
        raw[3] = value[index].M41;
        raw[4] = value[index].M12;
        raw[5] = value[index].M22;
        raw[6] = value[index].M32;
        raw[7] = value[index].M42;
        raw[8] = value[index].M13;
        raw[9] = value[index].M23;
        raw[10] = value[index].M33;
        raw[11] = value[index].M43;
        raw[12] = value[index].M14;
        raw[13] = value[index].M24;
        raw[14] = value[index].M34;
        raw[15] = value[index].M44;
        ++index;
        raw += 16 /*0x10*/;
      }
    }
    else if (columnCount == 3 && rowCount == 3)
    {
      int index = start;
      while (index < start + length)
      {
        *raw = value[index].M11;
        raw[1] = value[index].M21;
        raw[2] = value[index].M31;
        raw[4] = value[index].M12;
        raw[5] = value[index].M22;
        raw[6] = value[index].M32;
        raw[8] = value[index].M13;
        raw[9] = value[index].M23;
        raw[10] = value[index].M33;
        ++index;
        raw += 12;
      }
    }
    else if (columnCount == 4 && rowCount == 3)
    {
      int index = start;
      while (index < start + length)
      {
        *raw = value[index].M11;
        raw[1] = value[index].M21;
        raw[2] = value[index].M31;
        raw[3] = value[index].M41;
        raw[4] = value[index].M12;
        raw[5] = value[index].M22;
        raw[6] = value[index].M32;
        raw[7] = value[index].M42;
        raw[8] = value[index].M13;
        raw[9] = value[index].M23;
        raw[10] = value[index].M33;
        raw[11] = value[index].M43;
        ++index;
        raw += 12;
      }
    }
    else if (columnCount == 3 && rowCount == 4)
    {
      int index = start;
      while (index < start + length)
      {
        *raw = value[index].M11;
        raw[1] = value[index].M21;
        raw[2] = value[index].M31;
        raw[4] = value[index].M12;
        raw[5] = value[index].M22;
        raw[6] = value[index].M32;
        raw[8] = value[index].M13;
        raw[9] = value[index].M23;
        raw[10] = value[index].M33;
        raw[12] = value[index].M14;
        raw[13] = value[index].M24;
        raw[14] = value[index].M34;
        ++index;
        raw += 16 /*0x10*/;
      }
    }
    else
    {
      if (columnCount != 2 || rowCount != 2)
        throw new NotImplementedException($"Matrix Size: {rowCount.ToString()} {columnCount.ToString()}");
      int index = start;
      while (index < start + length)
      {
        *raw = value[index].M11;
        raw[1] = value[index].M21;
        raw[4] = value[index].M12;
        raw[5] = value[index].M22;
        ++index;
        raw += 8;
      }
    }
  }
}
