// Decompiled with JetBrains decompiler
// Type: FezEngine.Structure.NpcInstance
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using ContentSerialization.Attributes;
using FezEngine.Components;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace FezEngine.Structure;

public class NpcInstance
{
  [Serialization(Ignore = true)]
  public readonly NpcMetadata Metadata = new NpcMetadata();

  [Serialization(Ignore = true)]
  public int Id { get; set; }

  [Serialization(Ignore = true)]
  public bool Talking { get; set; }

  [Serialization(Ignore = true)]
  public bool Enabled { get; set; }

  [Serialization(Ignore = true)]
  public bool Visible { get; set; }

  public string Name { get; set; }

  public Vector3 Position { get; set; }

  public Vector3 DestinationOffset { get; set; }

  [Serialization(Optional = true)]
  public bool RandomizeSpeech { get; set; }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool SayFirstSpeechLineOnce { get; set; }

  public List<SpeechLine> Speech { get; set; }

  [Serialization(Ignore = true)]
  public SpeechLine CustomSpeechLine { get; set; }

  [Serialization(Ignore = true)]
  public Group Group { get; set; }

  [Serialization(Ignore = true)]
  public NpcState State { get; set; }

  public Dictionary<NpcAction, NpcActionContent> Actions { get; set; }

  [Serialization(Optional = true)]
  public float WalkSpeed
  {
    get => this.Metadata.WalkSpeed;
    set => this.Metadata.WalkSpeed = value;
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public bool AvoidsGomez
  {
    get => this.Metadata.AvoidsGomez;
    set => this.Metadata.AvoidsGomez = value;
  }

  [Serialization(Optional = true, DefaultValueOptional = true)]
  public ActorType ActorType
  {
    get => this.Metadata.ActorType;
    set => this.Metadata.ActorType = value;
  }

  public NpcInstance()
  {
    this.Speech = new List<SpeechLine>();
    this.Actions = new Dictionary<NpcAction, NpcActionContent>((IEqualityComparer<NpcAction>) NpcActionComparer.Default);
    this.Enabled = true;
    this.Visible = true;
  }

  public NpcInstance Clone()
  {
    List<SpeechLine> list = this.Speech.Select<SpeechLine, SpeechLine>((Func<SpeechLine, SpeechLine>) (line => line.Clone())).ToList<SpeechLine>();
    Dictionary<NpcAction, NpcActionContent> dictionary = this.Actions.Keys.ToDictionary<NpcAction, NpcAction, NpcActionContent>((Func<NpcAction, NpcAction>) (action => action), (Func<NpcAction, NpcActionContent>) (action => this.Actions[action].Clone()));
    return new NpcInstance()
    {
      Name = this.Name,
      Position = this.Position,
      DestinationOffset = this.DestinationOffset,
      WalkSpeed = this.WalkSpeed,
      RandomizeSpeech = this.RandomizeSpeech,
      SayFirstSpeechLineOnce = this.SayFirstSpeechLineOnce,
      Speech = list,
      Actions = dictionary,
      AvoidsGomez = this.AvoidsGomez,
      ActorType = this.ActorType
    };
  }

  public void CopyFrom(NpcInstance instance)
  {
    this.Name = instance.Name;
    this.Position = instance.Position;
    this.DestinationOffset = instance.DestinationOffset;
    this.WalkSpeed = instance.WalkSpeed;
    this.RandomizeSpeech = instance.RandomizeSpeech;
    this.SayFirstSpeechLineOnce = instance.SayFirstSpeechLineOnce;
    this.Speech = instance.Speech;
    this.Actions = instance.Actions;
    this.AvoidsGomez = instance.AvoidsGomez;
    this.ActorType = instance.ActorType;
  }

  public void FillMetadata(NpcMetadata md)
  {
    this.Metadata.AvoidsGomez = md.AvoidsGomez;
    this.Metadata.WalkSpeed = md.WalkSpeed;
    this.Metadata.SoundPath = md.SoundPath;
    this.Metadata.SoundActions = md.SoundActions;
  }
}
