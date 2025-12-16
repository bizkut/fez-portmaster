// Decompiled with JetBrains decompiler
// Type: FezEngine.Effects.LightingPostEffect
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Common;
using FezEngine.Effects.Structures;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Effects;

public class LightingPostEffect : BaseEffect
{
  private readonly SemanticMappedSingle dawnContribution;
  private readonly SemanticMappedSingle duskContribution;
  private readonly SemanticMappedSingle nightContribution;
  private readonly Dictionary<LightingPostEffect.Passes, EffectPass> passes;

  public LightingPostEffect()
    : base(nameof (LightingPostEffect))
  {
    this.dawnContribution = new SemanticMappedSingle(this.effect.Parameters, nameof (DawnContribution));
    this.duskContribution = new SemanticMappedSingle(this.effect.Parameters, nameof (DuskContribution));
    this.nightContribution = new SemanticMappedSingle(this.effect.Parameters, nameof (NightContribution));
    this.passes = new Dictionary<LightingPostEffect.Passes, EffectPass>((IEqualityComparer<LightingPostEffect.Passes>) LightingPostEffect.PassesComparer.Default);
    foreach (LightingPostEffect.Passes key in Util.GetValues<LightingPostEffect.Passes>())
      this.passes.Add(key, this.currentTechnique.Passes[key.ToString()]);
  }

  public float DawnContribution
  {
    set => this.dawnContribution.Set(value);
    get => this.dawnContribution.Get();
  }

  public float DuskContribution
  {
    set => this.duskContribution.Set(value);
    get => this.duskContribution.Get();
  }

  public float NightContribution
  {
    set => this.nightContribution.Set(value);
    get => this.nightContribution.Get();
  }

  public LightingPostEffect.Passes Pass
  {
    set => this.currentPass = this.passes[value];
  }

  public enum Passes
  {
    Dawn,
    Dusk_Multiply,
    Dusk_Screen,
    Night,
  }

  private class PassesComparer : IEqualityComparer<LightingPostEffect.Passes>
  {
    public static readonly LightingPostEffect.PassesComparer Default = new LightingPostEffect.PassesComparer();

    public bool Equals(LightingPostEffect.Passes x, LightingPostEffect.Passes y) => x == y;

    public int GetHashCode(LightingPostEffect.Passes obj) => (int) obj;
  }
}
