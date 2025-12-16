// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.Scripting.Entity
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;

#nullable disable
namespace FezEngine.Structure.Scripting;

public class Entity
{
  public string Type { get; set; }

  [Serialization(Optional = true)]
  public int? Identifier { get; set; }

  public override string ToString()
  {
    if (!this.Identifier.HasValue)
      return this.Type;
    return $"{this.Type}[{(object) this.Identifier}]";
  }

  public Entity Clone()
  {
    return new Entity()
    {
      Type = this.Type,
      Identifier = this.Identifier
    };
  }
}
