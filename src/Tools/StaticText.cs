// Decompiled with JetBrains decompiler
// Type: FezGame.Tools.StaticText
// Assembly: FEZ, Version=1.12.0.0, Culture=neutral, PublicKeyToken=null
// MVID: C7C60EB9-854C-4056-8C0D-A9A87FFECC57
// Assembly location: E:\GOG Games\Fez\FEZ.exe

using FezEngine.Services;
using FezEngine.Tools;
using System.Collections.Generic;

#nullable disable
namespace FezGame.Tools;

public static class StaticText
{
  private static readonly Dictionary<string, string> Fallback;
  private static readonly Dictionary<string, Dictionary<string, string>> AllResources = ServiceHelper.Get<IContentManagerProvider>().Global.Load<Dictionary<string, Dictionary<string, string>>>("Resources/StaticText");

  static StaticText() => StaticText.Fallback = StaticText.AllResources[string.Empty];

  public static bool TryGetString(string tag, out string text)
  {
    string letterIsoLanguageName = Culture.TwoLetterISOLanguageName;
    Dictionary<string, string> fallback;
    if (!StaticText.AllResources.TryGetValue(letterIsoLanguageName, out fallback))
      fallback = StaticText.Fallback;
    if (tag != null && fallback.TryGetValue(tag, out text) || tag != null && StaticText.Fallback.TryGetValue(tag, out text))
      return true;
    text = "[MISSING TEXT]";
    return false;
  }

  public static string GetString(string tag)
  {
    string text;
    return StaticText.TryGetString(tag, out text) ? text : "[MISSING TEXT]";
  }
}
