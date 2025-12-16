// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.SemanticMappedVectorArray
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

public class SemanticMappedVectorArray : 
  SemanticMappedParameter<Vector4[]>,
  SemanticMappedArrayParameter<Vector4[]>
{
  private IntPtr raw;

  public SemanticMappedVectorArray(EffectParameterCollection parent, string semanticName)
    : base(parent, semanticName)
  {
    this.raw = (IntPtr) ReflectionHelper.GetValue(this.parameter.GetType().GetMember("values", BindingFlags.Instance | BindingFlags.NonPublic)[0], (object) this.parameter);
  }

  protected override void DoSet(Vector4[] value) => this.Set(value, 0, value.Length);

  protected override void DoSet(Vector4[] value, int length) => this.Set(value, 0, length);

  public unsafe void Set(Vector4[] value, int start, int length)
  {
    float* raw = (float*) (void*) this.raw;
    int index = start;
    while (index < start + length)
    {
      *raw = value[index].X;
      raw[1] = value[index].Y;
      raw[2] = value[index].Z;
      raw[3] = value[index].W;
      ++index;
      raw += 4;
    }
  }
}
