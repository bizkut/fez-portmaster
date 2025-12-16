// Decompiled with JetBrains decompiler
// Type: FezEngine.Services.IContentManagerProvider
// Assembly: FezEngine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: BF0F94A8-07FE-4074-8978-FD18CC5EF662
// Assembly location: E:\GOG Games\Fez\FezEngine.dll

using Microsoft.Xna.Framework.Content;
using System.Collections.Generic;

#nullable disable
namespace FezEngine.Services;

public interface IContentManagerProvider
{
  ContentManager Global { get; }

  ContentManager GetForLevel(string levelName);

  ContentManager CurrentLevel { get; }

  ContentManager Get(CM name);

  IEnumerable<string> GetAllIn(string directory);

  void Dispose(CM name);
}
