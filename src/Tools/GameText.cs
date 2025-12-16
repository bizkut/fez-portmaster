// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.GameText
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Tools;

public static class GameText
{
  private static readonly Dictionary<string, string> Fallback;
  private static readonly Dictionary<string, Dictionary<string, string>> AllResources = ServiceHelper.Get<IContentManagerProvider>().Global.Load<Dictionary<string, Dictionary<string, string>>>("Resources/GameText");

  static GameText() => GameText.Fallback = GameText.AllResources[string.Empty];

  public static string GetString(string tag)
  {
    string letterIsoLanguageName = Culture.TwoLetterISOLanguageName;
    Dictionary<string, string> fallback;
    if (!GameText.AllResources.TryGetValue(letterIsoLanguageName, out fallback))
      fallback = GameText.Fallback;
    string str;
    return (tag == null || !fallback.TryGetValue(tag, out str)) && (tag == null || !GameText.Fallback.TryGetValue(tag, out str)) ? "[MISSING TEXT]" : str;
  }

  public static string GetStringRaw(string tag)
  {
    string str;
    return tag == null || !GameText.Fallback.TryGetValue(tag, out str) ? "[MISSING TEXT]" : str;
  }
}
