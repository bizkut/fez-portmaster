// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.SemanticMappedInt32
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects.Structures;

public class SemanticMappedInt32(EffectParameterCollection parent, string semanticName) : 
  SemanticMappedParameter<int>(parent, semanticName)
{
  protected override void DoSet(int value)
  {
    this.parameter.SetValue((float) value);
    this.currentValue = value;
  }
}
