// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.TrileCustomData
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

#nullable disable
namespace FezEngine.Structure;

public class TrileCustomData
{
  public bool Unstable;
  public bool Shiny;
  public bool TiltTwoAxis;
  public bool IsCustom;

  public void DetermineCustom() => this.IsCustom = this.Unstable || this.Shiny || this.TiltTwoAxis;
}
