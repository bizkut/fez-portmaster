// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.SpeechLine
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;

#nullable disable
namespace FezEngine.Structure;

public class SpeechLine
{
  public string Text { get; set; }

  [Serialization(Optional = true)]
  public NpcActionContent OverrideContent { get; set; }

  public SpeechLine Clone()
  {
    return new SpeechLine()
    {
      Text = this.Text,
      OverrideContent = this.OverrideContent == null ? (NpcActionContent) null : this.OverrideContent.Clone()
    };
  }
}
