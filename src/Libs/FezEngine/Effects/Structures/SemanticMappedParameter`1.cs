// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.Structures.SemanticMappedParameter`1
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Graphics;

#nullable disable
namespace FezEngine.Effects.Structures;

public abstract class SemanticMappedParameter<T>
{
  protected readonly EffectParameter parameter;
  private readonly bool missingParameter;
  protected T currentValue;
  protected bool firstSet = true;

  protected SemanticMappedParameter(EffectParameterCollection parent, string semanticName)
  {
    this.parameter = parent[semanticName] ?? parent[semanticName.Replace("Sampler", "Texture")];
    this.missingParameter = this.parameter == null;
  }

  public void Set(T value)
  {
    if (this.missingParameter)
      return;
    this.DoSet(value);
  }

  public void Set(T value, int length)
  {
    if (this.missingParameter)
      return;
    this.DoSet(value, length);
  }

  protected abstract void DoSet(T value);

  protected virtual void DoSet(T value, int length)
  {
  }

  public T Get() => this.currentValue;
}
