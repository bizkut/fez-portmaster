// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.SemanticMappedVector3
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects.Structures;

public class SemanticMappedVector3(EffectParameterCollection parent, string semanticName) : 
  SemanticMappedParameter<Vector3>(parent, semanticName)
{
  protected override void DoSet(Vector3 value)
  {
    this.parameter.SetValue(value);
    this.firstSet = false;
  }
}
